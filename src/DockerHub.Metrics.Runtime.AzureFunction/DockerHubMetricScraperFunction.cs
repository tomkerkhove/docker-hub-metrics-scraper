using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DockerHub.Metrics.Integration;
using DockerHub.Metrics.Runtime.AzureFunction.Contracts;
using GuardNet;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DockerHub.Metrics.Runtime.AzureFunction
{
    public class DockerHubMetricScraperFunction
    {
        private readonly DockerHubClient _dockerHubClient;
        private readonly ILogger<DockerHubMetricScraperFunction> _logger;

        public DockerHubMetricScraperFunction(DockerHubClient dockerHubClient, ILogger<DockerHubMetricScraperFunction> logger)
        {
            Guard.NotNull(dockerHubClient, nameof(dockerHubClient));
            Guard.NotNull(logger, nameof(logger));

            _dockerHubClient = dockerHubClient;
            _logger = logger;
        }

        [FunctionName("docker-hub-metric-scraper")]
        public async Task Run([ServiceBusTrigger("scrape-requests", Connection = "ServiceBusConnectionString")] ScrapeRequestMessage scrapeRequest)
        {
            if (string.IsNullOrWhiteSpace(scrapeRequest.RepoName))
            {
                throw new ArgumentException("No repo name was provided");
            }
            if (string.IsNullOrWhiteSpace(scrapeRequest.ImageName))
            {
                throw new ArgumentException("No image name was provided");
            }

            var imageId = $"{scrapeRequest.RepoName}/{scrapeRequest.ImageName}";
            _logger.LogInformation($"Starting to scrape Docker Hub metrics at {DateTime.UtcNow} for {imageId}");
            
            var imageMetrics = await _dockerHubClient.GetImageMetricsAsync(scrapeRequest.RepoName, scrapeRequest.ImageName);

            var contextualInformation = new Dictionary<string, object>
            {
                {"Repo Name", scrapeRequest.RepoName},
                {"Image Name", scrapeRequest.ImageName},
                {"Image ID", imageId}
            };

            _logger.LogMetric("Image Pulls", imageMetrics.Pulls, contextualInformation);
            _logger.LogMetric("Image Stars", imageMetrics.Stars, contextualInformation);

            _logger.LogInformation($"Docker Hub metrics are reported around {DateTime.UtcNow}");
        }
    }
}