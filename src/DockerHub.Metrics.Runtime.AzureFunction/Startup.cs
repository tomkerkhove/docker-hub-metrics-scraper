using DockerHub.Metrics.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

[assembly: FunctionsStartup(typeof(DockerHub.Metrics.Runtime.AzureFunction.Startup))]
namespace DockerHub.Metrics.Runtime.AzureFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<DockerHubClient>();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var instrumentationKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithComponentName("Docker Hub Metrics Scraper")
                .Enrich.WithVersion()
                .WriteTo.Console()
                .WriteTo.AzureApplicationInsights(instrumentationKey)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
        }
    }
}
