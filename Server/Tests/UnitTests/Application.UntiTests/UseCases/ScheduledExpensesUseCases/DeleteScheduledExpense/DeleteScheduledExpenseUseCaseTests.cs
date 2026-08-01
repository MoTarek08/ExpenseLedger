using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ScheduledExpensesUseCases.DeleteScheduledExpense;
using Domain.Entities.DomainEnums;
using Domain.Entities.ScheduledExpenseNamespace;
using FakeItEasy;

namespace UnitTests.Application.UntiTests.UseCases.ScheduledExpensesUseCases.DeleteScheduledExpense
{
    public class DeleteScheduledExpenseUseCaseTests
    {
        private readonly IScheduledExpensesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DeleteScheduledExpenseUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid ScheduledExpenseId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");
        private static readonly Guid CategoryId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");

        public DeleteScheduledExpenseUseCaseTests()
        {
            _repository = A.Fake<IScheduledExpensesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _sut = new DeleteScheduledExpenseUseCase(_repository, _unitOfWork, A.Fake<Microsoft.Extensions.Logging.ILogger<DeleteScheduledExpenseUseCase>>());
        }

        [Fact]
        public async Task Execute_WhenScheduledExpenseNotFound_ShouldReturnFailure()
        {
            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns((ScheduledExpense?)null);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, default);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenNotOwnedByUser_ShouldReturnFailure()
        {
            var expense = ScheduledExpense.Create(OtherUserId, "Test", 100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, default);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.SCHEDULED_EXPENSE_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenOwnedByUser_ShouldRemoveAndSave()
        {
            var expense = ScheduledExpense.Create(UserId, "Test", 100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, default);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Remove(expense)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenAlreadyCancelled_ShouldStillRemove()
        {
            var expense = ScheduledExpense.Create(UserId, "Test", 100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            expense.Cancel();

            A.CallTo(() => _repository.FindAsync(ScheduledExpenseId, A<CancellationToken>._))
                .Returns(expense);

            var result = await _sut.Execute(ScheduledExpenseId, UserId, default);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _repository.Remove(expense)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }
    }
}
