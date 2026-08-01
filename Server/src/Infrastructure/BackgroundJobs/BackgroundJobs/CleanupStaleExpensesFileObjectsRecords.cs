using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs.BackgroundJobs
{
    public class CleanupStaleExpensesFileObjectsRecords
    {
        private readonly IExpensesFileObjectsRepository _expensesFileObjectsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<CleanupStaleExpensesFileObjectsRecords> _logger;

        public CleanupStaleExpensesFileObjectsRecords(
            IExpensesFileObjectsRepository expensesFileObjectsRepository,
            IDateProvider dateTimeProvider,
            ILogger<CleanupStaleExpensesFileObjectsRecords> logger)
        {
            _expensesFileObjectsRepository = expensesFileObjectsRepository;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task Execute()
        {
            var lastSeenId = Guid.Empty;
            int totalDeletedRows = 0;
            while (true)
            {
                var batch = await _expensesFileObjectsRepository.FindsStaleUploadsAsync(lastSeenId,_dateTimeProvider.Now);

                if (batch.Count == 0)
                    break;

                lastSeenId = batch.Last().Id;

                var deletedRows = await _expensesFileObjectsRepository.BulkDeleteAsync(batch.Select(f => f.Id).ToList());
                totalDeletedRows += deletedRows;
            }
            if (totalDeletedRows > 0)
                _logger.LogInformation("{CleanedUpStaleExpensesFileObjects} stale expenses file objects has been removed", totalDeletedRows);
        }
    }
}
