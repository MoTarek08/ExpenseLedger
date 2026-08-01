using Application.ErrorNamespace;
using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.SharedServices;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.ScheduledExpenseNamespace;
using FakeItEasy;
using Infrastructure.Scheduling.BackgroundJobs;

namespace UnitTests.Infrastructure.UnitTests.BackgroundJobs
{
    public class GenerateExpenseFromScheduledExpenseTests
    {
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly IExpensesRepository _expensesRepository;
        private readonly IScheduledExpensesRepository _scheduledExpensesRepository;
        private readonly IBuildExpenseService _buildExpenseService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly GenerateExpenseFromScheduledExpense _sut;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();
        private readonly Guid _scheduledExpenseId = Guid.NewGuid();
        private readonly DateOnly _expectedDueDate = new DateOnly(2026, 8, 1);

        public GenerateExpenseFromScheduledExpenseTests()
        {
            _backgroundJobsService = A.Fake<IBackgroundJobsService>();
            _expensesRepository = A.Fake<IExpensesRepository>();
            _scheduledExpensesRepository = A.Fake<IScheduledExpensesRepository>();
            _buildExpenseService = A.Fake<IBuildExpenseService>();
            _unitOfWork = A.Fake<IUnitOfWork>();

            _sut = new GenerateExpenseFromScheduledExpense(
                _backgroundJobsService,
                _expensesRepository,
                _scheduledExpensesRepository,
                _buildExpenseService,
                _unitOfWork);
        }

        private ScheduledExpense CreateActiveScheduledExpense(CadenceInterval cadence = CadenceInterval.Monthly)
        {
            return ScheduledExpense.Create(
                _userId,
                "Test expense",
                500m,
                _categoryId,
                null,
                cadence,
                new DateOnly(2026, 8, 1),
                new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero));
        }

        [Fact]
        public async Task Execute_ScheduledExpenseIsNull_ReturnsEarly()
        {
            A.CallTo(() => _scheduledExpensesRepository.FindAsync(_scheduledExpenseId, A<CancellationToken>._))
                .Returns((ScheduledExpense?)null);

            await _sut.Execute(_scheduledExpenseId, _expectedDueDate);

            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_ScheduledExpenseIsNotActive_ReturnsEarly()
        {
            var scheduledExpense = CreateActiveScheduledExpense();
            scheduledExpense.Cancel();

            A.CallTo(() => _scheduledExpensesRepository.FindAsync(_scheduledExpenseId, A<CancellationToken>._))
                .Returns(scheduledExpense);

            await _sut.Execute(_scheduledExpenseId, _expectedDueDate);

            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_ExpectedDueDateDoesNotMatchNextDueOn_ReturnsEarly()
        {
            var scheduledExpense = CreateActiveScheduledExpense();
            var differentDueDate = new DateOnly(2025, 1, 1);

            A.CallTo(() => _scheduledExpensesRepository.FindAsync(_scheduledExpenseId, A<CancellationToken>._))
                .Returns(scheduledExpense);

            await _sut.Execute(_scheduledExpenseId, differentDueDate);

            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_ExpenseBuildFailed_ReturnsEarly()
        {
            var scheduledExpense = CreateActiveScheduledExpense();

            A.CallTo(() => _scheduledExpensesRepository.FindAsync(_scheduledExpenseId, A<CancellationToken>._))
                .Returns(scheduledExpense);
            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .Returns(Result<Expense>.Failure(new Error("BUILD_FAILED")));

            await _sut.Execute(_scheduledExpenseId, _expectedDueDate);

            A.CallTo(() => _expensesRepository.Add(A<Expense>._))
                .MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_ValidWithMonthlyCadence_MarksAsProcessedAndTriggersAfterJobs()
        {
            var scheduledExpense = CreateActiveScheduledExpense(CadenceInterval.Monthly);
            var builtExpense = Expense.CreateManualExpense(
                _userId, _categoryId, "Test expense", 500m,
                _expectedDueDate, new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _scheduledExpensesRepository.FindAsync(_scheduledExpenseId, A<CancellationToken>._))
                .Returns(scheduledExpense);
            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .Returns(Result<Expense>.Success(builtExpense));
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _sut.Execute(_scheduledExpenseId, _expectedDueDate);

            A.CallTo(() => _expensesRepository.Add(builtExpense))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .MustHaveHappenedOnceExactly();
            Assert.Equal(_expectedDueDate, scheduledExpense.LastProcessedAt);
            Assert.True(scheduledExpense.IsActive);

            A.CallTo(() => _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                    _scheduledExpenseId, A<DateOnly>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _backgroundJobsService.TriggerAfterBackgroundExpenseCreationJobs(builtExpense.Id))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_ValidWithOnceCadence_DoesNotScheduleNextJob()
        {
            var scheduledExpense = CreateActiveScheduledExpense(CadenceInterval.Once);
            var builtExpense = Expense.CreateManualExpense(
                _userId, _categoryId, "Test expense", 500m,
                _expectedDueDate, new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _scheduledExpensesRepository.FindAsync(_scheduledExpenseId, A<CancellationToken>._))
                .Returns(scheduledExpense);
            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .Returns(Result<Expense>.Success(builtExpense));
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            await _sut.Execute(_scheduledExpenseId, _expectedDueDate);

            Assert.Null(scheduledExpense.NextDueOn);
            Assert.False(scheduledExpense.IsActive);

            A.CallTo(() => _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                    A<Guid>._, A<DateOnly>._))
                .MustNotHaveHappened();
            A.CallTo(() => _backgroundJobsService.TriggerAfterBackgroundExpenseCreationJobs(builtExpense.Id))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_UniqueViolationOnSave_ReturnsSilently()
        {
            var scheduledExpense = CreateActiveScheduledExpense();
            var builtExpense = Expense.CreateManualExpense(
                _userId, _categoryId, "Test expense", 500m,
                _expectedDueDate, new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _scheduledExpensesRepository.FindAsync(_scheduledExpenseId, A<CancellationToken>._))
                .Returns(scheduledExpense);
            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .Returns(Result<Expense>.Success(builtExpense));
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .Throws(new GeneratedExpenseForThatDayAlreadyExists());

            await _sut.Execute(_scheduledExpenseId, _expectedDueDate);

            A.CallTo(() => _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                    A<Guid>._, A<DateOnly>._))
                .MustNotHaveHappened();
            A.CallTo(() => _backgroundJobsService.TriggerAfterBackgroundExpenseCreationJobs(A<Guid>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_NonUniqueViolationExceptionOnSave_Propagates()
        {
            var scheduledExpense = CreateActiveScheduledExpense();
            var builtExpense = Expense.CreateManualExpense(
                _userId, _categoryId, "Test expense", 500m,
                _expectedDueDate, new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero));

            A.CallTo(() => _scheduledExpensesRepository.FindAsync(_scheduledExpenseId, A<CancellationToken>._))
                .Returns(scheduledExpense);
            A.CallTo(() => _buildExpenseService.BuildExpense(A<Guid>._, A<CreateExpenseRequestModel>._, A<CancellationToken>._))
                .Returns(Result<Expense>.Success(builtExpense));
            A.CallTo(() => _unitOfWork.SaveChangesAsync())
                .Throws(new InvalidOperationException("DB failure"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.Execute(_scheduledExpenseId, _expectedDueDate));
        }
    }
}
