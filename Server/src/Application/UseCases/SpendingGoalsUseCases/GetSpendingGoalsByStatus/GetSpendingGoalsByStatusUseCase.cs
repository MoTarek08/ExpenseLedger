using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models.Result;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalWithStatus.Models;

public class GetSpendingGoalsByStatusUseCase
{
    private readonly IUsersRepository _usersRepository;
    private readonly ISpendingGoalsRepository _spendingGoalsRepository;
    private readonly IDateProvider _dateProvider;

    public GetSpendingGoalsByStatusUseCase(
        IUsersRepository usersRepository,
        ISpendingGoalsRepository spendingGoalsRepository,
        IDateProvider dateProvider)
    {
        _usersRepository = usersRepository;
        _spendingGoalsRepository = spendingGoalsRepository;
        _dateProvider = dateProvider;
    }

    public async Task<Result<List<GetSpendingGoalsByStatusDto>>> Execute(
        Guid userId,
        SpendingGoalStatus status,
        GetSpendingGoalsByStatusQueryParameters queryParameters,
        CancellationToken cancelletionToken)
    {
        var today = _dateProvider.Today;

        var query = _spendingGoalsRepository.GetAllForUserQuery(userId);

        if (queryParameters.CategoryId.HasValue)
            query = query.Where(g => g.CategoryId == queryParameters.CategoryId);

        if (queryParameters.EndingDateFrom.HasValue)
            query = query.Where(g => g.EndsAt >= queryParameters.EndingDateFrom.Value);

        if (queryParameters.EndingDateTo.HasValue)
            query = query.Where(g => g.EndsAt <= queryParameters.EndingDateTo.Value);

        if (status == SpendingGoalStatus.Succeeded)
        {
            return Result<List<GetSpendingGoalsByStatusDto>>.Success(
                await _spendingGoalsRepository.GetSucceededGoalsAsync(query
                ,_dateProvider.Today
                ,queryParameters.PageNumber,
                queryParameters.PageSize,
                cancelletionToken));
        }

        else if (status == SpendingGoalStatus.Failed)
        {
            return Result<List<GetSpendingGoalsByStatusDto>>.Success(
                await _spendingGoalsRepository.GetFailedGoalsAsync(query
                , _dateProvider.Today
                , queryParameters.PageNumber,
                queryParameters.PageSize,
                cancelletionToken));
        }

        else if (status == SpendingGoalStatus.InProgress)
        {
            return Result<List<GetSpendingGoalsByStatusDto>>.Success(
                await _spendingGoalsRepository.GetInProgressGoalsAsync(query
                , today
                , queryParameters.PageNumber,
                queryParameters.PageSize,
                cancelletionToken));
        }

        return Result<List<GetSpendingGoalsByStatusDto>>.Success(
            await _spendingGoalsRepository.GetPendingGoalsAsync(query
            , today
            , queryParameters.PageNumber,
            queryParameters.PageSize,
            cancelletionToken));
    }
}