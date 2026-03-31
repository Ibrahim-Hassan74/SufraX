using EStoreX.API.Filters;
using EStoreX.API.Middleware;
using EStoreX.API.StartupExtensions;
using EStoreX.Core;
using EStoreX.Core.BackgroundJobs.Schedulers;
using EStoreX.Infrastructure;
using Hangfire;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

[assembly: Microsoft.Extensions.Localization.RootNamespace("EStoreX.API")]

var builder = WebApplication.CreateBuilder(args);

var configPath = Path.Combine(builder.Environment.ContentRootPath, "Configurations");
builder.Configuration
    .AddJsonFile(Path.Combine(configPath, "appsettings.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine(configPath, $"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true);

// Add services to the container.

builder.Services.ConfigureServices(builder.Configuration);

builder.Services.ConfigureInfrastructure(builder.Configuration);

builder.Services.ConfigureCore(builder.Configuration);

var licenseConfig = builder.Configuration["EPPlus:ExcelPackage:License"];

ExcelPackage.License.SetNonCommercialPersonal(licenseConfig);

var app = builder.Build();

var localizationOptions = app.Services
    .GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;

app.UseRequestLocalization(localizationOptions);

//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "2.0");
    });
}

app.UseApiKeyMiddleware();

app.UseCors("AllowAllOrigins");

app.UseRateLimiter();

app.UseExceptionHandlingMiddleware();

app.UseHtmlRewriteMiddleware();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();


app.UseHangfireDashboard("/dashboard", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter(app.Services) },
    AppPath = null
});

JobScheduler.ScheduleJobs();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
