using Application.Interfaces.BusinessQueries;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Infrastructure.DatabaseRelatedImplementations.BusinessQueries;
using Infrastructure.DatabaseRelatedImplementations.Reposetories;
using Infrastructure.DatabaseRelatedImplementations.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection.DatabaseRelatedImplementations
{
    public static class DatabaseRelatedImplementationsExtensions
    {
        public static IServiceCollection AddDatabaseRelatedImplementations(this IServiceCollection services)
        {
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ICategoriesRepository, CategoriesRepository>();
            services.AddScoped<IExpensesRepository, ExpensesRepository>();
            services.AddScoped<IScheduledExpensesRepository, ScheduledExpensesRepository>();
            services.AddScoped<INotesRepository, NotesRepository>();
            services.AddScoped<IUserCategoryPreferencesRepository, UserCategoryPreferencesRepository>();
            services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
            services.AddScoped<ISpendingGoalsRepository, SpendingGoalsRepository>();
            services.AddScoped<INotificationsRepository, NotificationsRepository>();
            services.AddScoped<IExpensesFileObjectsRepository, ExpensesFileObjectsRepository>();
            // COMMENTED OUT: object storage deletion requests are no longer used
            //services.AddScoped<IObjectStorageDeletionRequestsRepository, ObjectStorageDeletionRequestsRepository>();
            services.AddScoped<IBudgetQueries, BudgetQueries>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
