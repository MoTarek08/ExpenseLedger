using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.ExpenseSubCategoryNamespace;
using Domain.Entities.FileObjectNamespace;
using Domain.Entities.NoteNamespace;
using Domain.Entities.Notification;
using Domain.Entities.ObjectStorageDeletionRequestNamespace;
using Domain.Entities.RefreshTokenNamespace;
using Domain.Entities.ScheduledExpenseNamespace;
using Domain.Entities.SpendingGoalNamespace;
using Domain.Entities.UserCategoryPreferenceNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using Domain.Entities.UserNamespace;
using Infrastructure.Database.DatabaseSettings;
using Infrastructure.Database.DomainEntitiesConfigurationNamespace;
using Infrastructure.Database.Seeding.CategorySeeding.CategorySeederNamespace;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Database.AppDbContextNamespace
{
    public class AppDbContext : DbContext
    {
        private readonly DbSettings _settings;

        public AppDbContext(DbContextOptions configOptions, DbSettings settings) : base(configOptions)
        {
            _settings = settings;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserFinancialProfile> UserFinancialProfiles => Set<UserFinancialProfile>();
        public DbSet<UserCategoryPreference> UserCategoryPreferences => Set<UserCategoryPreference>();


        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ScheduledExpense> ScheduledExpenses => Set<ScheduledExpense>();

        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<ExpenseSubCategory> ExpenseSubCategories => Set<ExpenseSubCategory>();

        public DbSet<ExpenseFileObject> ExpensesFileObjects => Set<ExpenseFileObject>();

        public DbSet<Note> Notes => Set<Note>();

        public DbSet<SpendingGoal> SpendingGoals => Set<SpendingGoal>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<Notification> Notifications => Set<Notification>();

        public DbSet<ObjectStorageDeletionRequest> ObjectStorageDeletionRequests => Set<ObjectStorageDeletionRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfig).Assembly);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.UseNpgsql(_settings.ConnectionString);

            optionsBuilder.UseAsyncSeeding(async (context,_,_) =>
            {
                if(!await context.Set<ExpenseCategory>().AnyAsync())
                    await CategorySeeder.SeedAsync(context);
            }
            );

            optionsBuilder.UseSeeding((context,_) =>
            {
                if (!context.Set<ExpenseCategory>().Any())
                    CategorySeeder.Seed(context);
            });


        }
    }
}
