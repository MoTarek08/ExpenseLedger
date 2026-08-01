using Application.Interfaces.BackgroundJobs;
using Infrastructure.BackgroundJobs;
using Infrastructure.BackgroundJobs.BackgroundJobs;
using Infrastructure.Scheduling.BackgroundJobs;
using Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection.BackgroundJobsClientConfiguration
{
    public static class BackgroundJobsConfigurationExtensions
    {
        public static IServiceCollection AddBackgroundJobsConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IBackgroundJobsService, BackgroundJobsService>();
            services.AddScoped<GenerateExpenseFromScheduledExpense>();
            services.AddScoped<CleanupStaleExpensesFileObjectsRecords>();
            services.AddScoped<CheckGoalAchievement>();
            services.AddScoped<CheckBudgetAfterExpenseCreationJob>();
            services.AddScoped<CheckCategoryPreferenceViolation>();
            services.AddScoped<CreateScheduledExpenseGeneratedNotification>();
            services.AddScoped<ObjectStorageDeletionCleanupJob>();
            return services;
        }

        public static WebApplication TriggerStartupBackgroundJobs(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var backgroundJobsService = scope.ServiceProvider
                .GetRequiredService<IBackgroundJobsService>();
            backgroundJobsService.AddOrUpdateCleanUpStaleExpenseFileObjectsWorker();
            backgroundJobsService.AddOrUpdateObjectStorageDeletionCleanupWorker();
            return app;
        }
    }
}
