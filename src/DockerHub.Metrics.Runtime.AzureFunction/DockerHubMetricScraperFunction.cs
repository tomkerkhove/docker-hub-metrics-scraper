using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DockerHub.Metrics.Integration;
using GuardNet;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DockerHub.Metrics.Runtime.AzureFunction
{
    public class DockerHubMetricScraperFunction
    {
        private readonly DockerHubClient _dockerHubClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DockerHubMetricScraperFunction> _logger;

        public DockerHubMetricScraperFunction(DockerHubClient dockerHubClient, IConfiguration configuration, ILogger<DockerHubMetricScraperFunction> logger)
        {
            Guard.NotNull(dockerHubClient, nameof(dockerHubClient));
            Guard.NotNull(configuration, nameof(configuration));
            Guard.NotNull(logger, nameof(logger));

            _dockerHubClient = dockerHubClient;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("docker-hub-metric-scraper")]
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo timer)
        {
            _logger.LogInformation($"Starting to scrape Docker Hub metrics at {DateTime.UtcNow}");

            var repoName = _configuration["DOCKER_HUB_REPO_NAME"];
            var imageName = _configuration["DOCKER_HUB_IMAGE_NAME"];

            var imageMetrics = await _dockerHubClient.GetImageMetricsAsync(repoName, imageName);

            var contextualInformation = new Dictionary<string, object>
            {
                {"Repo Name", repoName},
                {"Image Name", imageName},
                {"Image ID", $"{repoName}/{imageName}"}
            };

            _logger.LogMetric("Image Pulls", imageMetrics.Pulls, contextualInformation);
            _logger.LogMetric("Image Stars", imageMetrics.Stars, contextualInformation);

            _logger.LogInformation($"Docker Hub metrics are reported around {DateTime.UtcNow}");
        }
    }
}