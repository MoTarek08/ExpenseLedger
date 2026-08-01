namespace Host.Middlewares
{
    public static class SecurityPoliciesMiddelwares
    {
        public static WebApplication RegisterSecurityPoliciesMiddelwares(this WebApplication app)
        {
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.XFrameOptions = "DENY";
                await next();
            });

            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Append(
                    "Referrer-Policy",
                    "strict-origin-when-cross-origin");

                await next();
            });

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                await next();
            });

            return app;
        }
    }
}
