using Application.Interfaces.Repositories;
using Application.UseCases.ScheduledExpensesUseCases.Models;
using Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses;
using Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.ScheduledExpenseNamespace;
using FakeItEasy;

namespace UnitTests.Application.UntiTests.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses
{
    public class SearchScheduledExpensesUseCaseTests
    {
        private readonly IScheduledExpensesRepository _repository;
        private readonly SearchScheduledExpensesUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid CategoryId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");

        public SearchScheduledExpensesUseCaseTests()
        {
            _repository = A.Fake<IScheduledExpensesRepository>();
            _sut = new SearchScheduledExpensesUseCase(_repository, A.Fake<Microsoft.Extensions.Logging.ILogger<SearchScheduledExpensesUseCase>>());
        }

        [Fact]
        public async Task Execute_WhenNoParameters_ShouldReturnPaginatedResult()
        {
            var queryParams = new SearchScheduledExpensesQueryParameters(null);
            var allExpenses = Enumerable.Range(0, queryParams.PageSize*2)
                .Select(i => ScheduledExpense.Create(UserId, $"Expense {i}", 100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow))
                .ToList();

            A.CallTo(() => _repository.GetAllForUserQuery(UserId))
                .Returns(allExpenses.AsQueryable());
            A.CallTo(() => _repository.GetScheduledExpenseDtoAsync(A<IQueryable<ScheduledExpense>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<ScheduledExpense> query, CancellationToken _) =>
                    query.Select(se => new ScheduledExpenseDto(
                        se.Id, se.IsActive, se.Title, se.Amount, se.Cadence,
                        "HOUSING", null, se.FirstDueOn, se.NextDueOn, se.LastProcessedAt, se.CreatedAt)).ToList());

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(queryParams.PageSize, result.Data!.Count);
        }

        [Fact]
        public async Task Execute_WhenActiveOnlyFilter_ShouldReturnOnlyActive()
        {
            var queryParams = new SearchScheduledExpensesQueryParameters(ActiveOnly: true);
            var allExpenses = new List<ScheduledExpense>
            {
                ScheduledExpense.Create(UserId, "Active", 100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow),
                ScheduledExpense.Create(UserId, "Inactive", 100m, CategoryId, null, CadenceInterval.Once, new DateOnly(2026, 6, 1), DateTimeOffset.UtcNow)
            };
            allExpenses[1].Cancel();

            A.CallTo(() => _repository.GetAllForUserQuery(UserId))
                .Returns(allExpenses.AsQueryable());
            A.CallTo(() => _repository.GetScheduledExpenseDtoAsync(A<IQueryable<ScheduledExpense>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<ScheduledExpense> query, CancellationToken _) =>
                    query.Select(se => new ScheduledExpenseDto(
                        se.Id, se.IsActive, se.Title, se.Amount, se.Cadence,
                        "HOUSING", null, se.FirstDueOn, se.NextDueOn, se.LastProcessedAt, se.CreatedAt)).ToList());

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.All(result.Data!, dto => Assert.True(dto.IsActive));
        }

        [Fact]
        public async Task Execute_SortByFirstDueOnAscending_ShouldReturnCorrectOrder()
        {
            var queryParams = new SearchScheduledExpensesQueryParameters(null, SortBy: "FirstDueOn", SortOrder: "Asc");
            var allExpenses = new List<ScheduledExpense>
            {
                ScheduledExpense.Create(UserId, "Second", 100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 8, 1), DateTimeOffset.UtcNow),
                ScheduledExpense.Create(UserId, "First", 100m, CategoryId, null, CadenceInterval.Monthly, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow)
            };

            A.CallTo(() => _repository.GetAllForUserQuery(UserId))
                .Returns(allExpenses.AsQueryable());
            A.CallTo(() => _repository.GetScheduledExpenseDtoAsync(A<IQueryable<ScheduledExpense>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<ScheduledExpense> query, CancellationToken _) =>
                    query.Select(se => new ScheduledExpenseDto(
                        se.Id, se.IsActive, se.Title, se.Amount, se.Cadence,
                        "HOUSING", null, se.FirstDueOn, se.NextDueOn, se.LastProcessedAt, se.CreatedAt)).ToList());

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.Count);
            Assert.Equal("First", result.Data[0].Title);
            Assert.Equal("Second", result.Data[1].Title);
        }

        [Fact]
        public async Task Execute_WhenNoResults_ShouldReturnEmptyList()
        {
            var queryParams = new SearchScheduledExpensesQueryParameters(null);

            A.CallTo(() => _repository.GetAllForUserQuery(UserId))
                .Returns(new List<ScheduledExpense>().AsQueryable());
            A.CallTo(() => _repository.GetScheduledExpenseDtoAsync(A<IQueryable<ScheduledExpense>>._, A<CancellationToken>._))
                .Returns(new List<ScheduledExpenseDto>());

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }
    }
}
