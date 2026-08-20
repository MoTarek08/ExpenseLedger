using Application.Interfaces.ObjectStorage;
using FluentValidation;
using Host.Middlewares;
using Host.MiddlewaresNamespace;
using Host.RateLimiters;
using Host.SetupExtensions;
using Host.Validation.ValidatorsNamespace;
using Infrastructure.DependencyInjection.Authentication;
using Infrastructure.DependencyInjection.Authorization;
using Infrastructure.DependencyInjection.BackgroundJobsClientConfiguration;
using Infrastructure.DependencyInjection.Database;
using Infrastructure.DependencyInjection.DatabaseRelatedImplementations;
using Infrastructure.DependencyInjection.HealthChecks;
using Infrastructure.DependencyInjection.Logging;
using Infrastructure.DependencyInjection.ObjectStorageClientConfiguration;
using Infrastructure.DependencyInjection.Services;
using Infrastructure.Logging.SerilogNamespace;
using Infrastructure.ObjectStorage;

var builder = WebApplication.CreateBuilder(args);

builder.AddHttpPipelineConfiguration();


builder.Services.AddSwaggerGenConfiguration();

builder.Services.AddControllers();
builder.Services.AddSecurityPolicies(builder.Configuration);

builder.Services.AddVersioning();

builder.Services.AddValidatorsFromAssembly(typeof(RegisterRequestModelValidator).Assembly);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Host.AddSerilogConfiguration(builder.Configuration);

builder.Services.AddDatabaseConfiguration(builder.Configuration);

builder.Services.AddAuthenticationConfiguration(builder.Configuration);

builder.Services.AddAuthorizationConfiguration();
builder.Services.AddBackgroundJobsClientConfiguration(builder.Configuration);
builder.Services.AddBackgroundJobsConfiguration();
builder.Services.AddObjectStorageClient(builder.Configuration);
builder.Services.AddScoped<IObjectStorageService, ObjectStorageService>();
builder.Services.AddDatabaseRelatedImplementations();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddRateLimitersExtenstion(builder.Configuration);

builder.Services.CustomAddHealthChecks(builder.Configuration);


var app = builder.Build();


app.CustomMapHealthChecks();

app.ConfigureRequestLogging();

await ObjectStorageClientLifecycleConfiguration.AddLifecycleConfiguration(app.Services);

app.TriggerStartupBackgroundJobs();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDashboards();
    await app.ApplyMigrations();
}

else
{
    app.UseHsts();
}

app.UseContentTypeChecker();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.RegisterSecurityPoliciesMiddelwares();

app.MapControllers();

app.Run();

public partial class Program;
