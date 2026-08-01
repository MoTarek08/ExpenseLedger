using Hangfire;

namespace Host.SetupExtensions
{
    public static class UseDashboardsExtensions
    {
        public static IApplicationBuilder UseDashboards(this IApplicationBuilder app)
        {
            app.UseHangfireDashboard();
            return app;
        }
    }
}
