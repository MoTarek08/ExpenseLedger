using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.RepositoriesNamespace;
using Application.UseCases.ExpensesUseCases.GetExpenseById;
using Application.UseCases.ExpensesUseCases.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.ExpensesUseCases.GetExpenseById
{
    public class GetExpenseByIdUseCaseTests
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly ILogger<GetExpenseByIdUseCase> _logger;
        private readonly GetExpenseByIdUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid ExpenseId = Guid.Parse("851813ff-22a0-49f8-b13b-9e1b8879da9a");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");

        public GetExpenseByIdUseCaseTests()
        {
            _expensesRepository = A.Fake<IExpensesRepository>();
            _logger = A.Fake<ILogger<GetExpenseByIdUseCase>>();
            _sut = new GetExpenseByIdUseCase(_expensesRepository, _logger);
        }

        [Fact]
        public async Task Execute_WhenExpenseFoundAndOwned_ShouldReturnDto()
        {
            var dto = new ExpenseDto(ExpenseId, UserId, new DateOnly(2026, 7, 1), "Test", 100, "FOOD", null, null, 0);

            A.CallTo(() => _expensesRepository.FindExpenseDtoByIdAsync(ExpenseId, UserId, A<CancellationToken>._))
                .Returns(dto);

            var result = await _sut.Execute(UserId, ExpenseId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(ExpenseId, result.Data!.Id);
        }

        [Fact]
        public async Task Execute_WhenExpenseNotFound_ShouldReturnNotFound()
        {
            A.CallTo(() => _expensesRepository.FindExpenseDtoByIdAsync(ExpenseId, UserId, A<CancellationToken>._))
                .Returns((ExpenseDto?)null);

            var result = await _sut.Execute(UserId, ExpenseId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(ExpensesErrorCodes.EXPENSE_NOT_FOUND, result.Error!.Code);
        }
    }
}
