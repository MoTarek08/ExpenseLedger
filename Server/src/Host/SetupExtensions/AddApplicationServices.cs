using Application.UseCases.AuthUseCases.Login;
using Application.UseCases.AuthUseCases.Logout;
using Application.UseCases.AuthUseCases.RefreshTokensNamespace;
using Application.UseCases.AuthUseCases.Register;
using Application.UseCases.BudgetUseCases.GetRemainingBudget;
using Application.UseCases.CategoriesUseCases.GetAllCategories;
using Application.UseCases.CategoriesUseCases.GetCategoryByCode;
using Application.UseCases.ExpensesUseCases.ConfirmImageUpload;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace;
using Application.UseCases.ExpensesUseCases.DeleteExpense;
using Application.UseCases.ExpensesUseCases.GetExpenseById;
using Application.UseCases.ExpensesUseCases.GetExpensesByDay;
using Application.UseCases.ExpensesUseCases.SearchExpenses;
using Application.UseCases.ExpensesUseCases.UpdateExpense;
using Application.UseCases.ExpensesUseCases.UploadExpenseFile;
using Application.UseCases.NotesUseCases.CreateNote;
using Application.UseCases.NotesUseCases.DeleteNote;
using Application.UseCases.NotesUseCases.GetNoteById;
using Application.UseCases.NotesUseCases.UpdateNote;
using Application.UseCases.NotificationsUseCases.DeleteNotification;
using Application.UseCases.NotificationsUseCases.GetCurrentPeriodNotifications;
using Application.UseCases.NotificationsUseCases.GetNotificationById;
using Application.UseCases.NotificationsUseCases.MarkNotificationAsRead;
using Application.UseCases.NotificationsUseCases.RestoreNotification;
using Application.UseCases.NotificationsUseCases.SearchNotifications;
using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense;
using Application.UseCases.ScheduledExpensesUseCases.DeleteScheduledExpense;
using Application.UseCases.ScheduledExpensesUseCases.GetScheduledExpenseById;
using Application.UseCases.ScheduledExpensesUseCases.SearchScheduledExpenses;
using Application.UseCases.ScheduledExpensesUseCases.UpdateScheduledExpense;
using Application.UseCases.SpendingGoalsUseCases.CreateSpendingGoal;
using Application.UseCases.SpendingGoalsUseCases.DeleteSpendingGoal;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById;
using Application.UseCases.SpendingGoalsUseCases.UpdateSpendingGoal;
using Application.UseCases.UserCategoryPreferencesUseCases.CreateUserCategoryPreference;
using Application.UseCases.UserCategoryPreferencesUseCases.DeleteUserCategoryPreference;
using Application.UseCases.UserCategoryPreferencesUseCases.GetUserCategoryPreferenceById;
using Application.UseCases.UserCategoryPreferencesUseCases.SearchUserCategoryPreferences;
using Application.UseCases.UserCategoryPreferencesUseCases.UpdateUserCategoryPreference;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.CreateUserFinancialProfileNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.GetFinancialProfileNamespace;
using Application.UseCases.UsersFinancialProfilesUseCasesNamespace.UpdateFinancialProfileNamespace;
using Application.UseCases.UsersUseCases.GetUserProfileNamespace;
using Application.UseCases.UsersUseCases.UpdateUserNamespace;

namespace Host.SetupExtensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<RegisterUseCase>();
            services.AddScoped<LoginUserUseCase>();
            services.AddScoped<RefreshTokensUseCase>();
            services.AddScoped<LogoutUseCase>();

            services.AddScoped<CreateUserCategoryPreferenceUseCase>();
            services.AddScoped<UpdateUserCategoryPrefereneUseCase>();
            services.AddScoped<GetUserCategoryPreferenceByIdUseCase>();
            services.AddScoped<SearchUserCategoryPreferencesUseCase>();
            services.AddScoped<DeleteUserCategoryPreferenceUseCase>();

            services.AddScoped<GetUserProfileUseCase>();
            services.AddScoped<UpdateUserUseCase>();

            services.AddScoped<GetFinancialProfileUseCase>();
            services.AddScoped<CreateUserFinancialProfileUseCase>();
            services.AddScoped<UpdateFinancialProfileUseCase>();

            services.AddScoped<GetNotificationByIdUseCase>();
            services.AddScoped<MarkNotificationAsReadUseCase>();
            services.AddScoped<DeleteNotificationUseCase>();

            services.AddScoped<CreateExpenseUseCase>();
            services.AddScoped<UpdateExpenseUseCase>();
            services.AddScoped<GetExpensesByDayUseCase>();
            services.AddScoped<SearchExpensesUseCase>();
            services.AddScoped<UploadExpenseFileUseCase>();
            services.AddScoped<ConfirmExpenseFileUploadUseCase>();
            services.AddScoped<DeleteExpenseUseCase>();
            services.AddScoped<GetExpenseByIdUseCase>();

            services.AddScoped<CreateNoteUseCase>();
            services.AddScoped<UpdateNoteUseCase>();
            services.AddScoped<DeleteNoteUseCase>();
            services.AddScoped<GetNoteByIdUseCase>();

            services.AddScoped<CreateSpendingGoalUseCase>();
            services.AddScoped<UpdateSpendingGoalUseCase>();
            services.AddScoped<DeleteSpendingGoalUseCase>();
            services.AddScoped<GetSpendingGoalByIdUseCase>();
            services.AddScoped<GetSpendingGoalsByStatusUseCase>();

            services.AddScoped<CreateScheduledExpenseUseCase>();
            services.AddScoped<UpdateScheduledExpenseUseCase>();
            services.AddScoped<DeleteScheduledExpenseUseCase>();
            services.AddScoped<SearchScheduledExpensesUseCase>();
            services.AddScoped<GetScheduledExpenseByIdUseCase>();

            services.AddScoped<GetRemainingBudgetUseCase>();

            services.AddScoped<GetAllCategoriesUseCase>();
            services.AddScoped<GetCategoryByCodeUseCase>();

            services.AddScoped<GetCurrentPeriodNotificationsUseCase>();
            services.AddScoped<GetNotificationByIdUseCase>();
            services.AddScoped<MarkNotificationAsReadUseCase>();
            services.AddScoped<RestoreNotificationUseCase>();
            services.AddScoped<DeleteNotificationUseCase>();
            services.AddScoped<SearchNotificationsUseCase>();

            return services;
        }
    }
}
