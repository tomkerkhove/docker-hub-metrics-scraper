using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Arcus.Observability.Telemetry.Core;
using DockerHub.Metrics.Integration.Models;
using Flurl;
using GuardNet;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DockerHub.Metrics.Integration
{
    public class DockerHubClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DockerHubClient> _logger;

        public DockerHubClient(HttpClient httpClient, ILogger<DockerHubClient> logger)
        {
            Guard.NotNull(httpClient, nameof(httpClient));
            Guard.NotNull(httpClient, nameof(httpClient));

            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ImageMetric> GetImageMetricsAsync(string repoName, string imageName)
        {
            var uri = "https://hub.docker.com/v2/repositories/"
                .AppendPathSegment(repoName)
                .AppendPathSegment(imageName);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var contextualInformation = new Dictionary<string, object>
            {
                {"Repo Name", repoName},
                {"Image Name", imageName}
            };

            var response = await SendRequestToApiAsync(request, contextualInformation);
            response.EnsureSuccessStatusCode();

            var rawResponse = await response.Content.ReadAsStringAsync();
            var imageMetric = ParseImageMetrics(rawResponse);

            return imageMetric;
        }

        private static ImageMetric ParseImageMetrics(string rawResponse)
        {
            var parsedResponse = JToken.Parse(rawResponse);

            var pulls = GetMetricOrDefault("pull_count", parsedResponse);
            var stars = GetMetricOrDefault("star_count", parsedResponse);

            var imageMetric = new ImageMetric
            {
                Pulls = pulls,
                Stars = stars,
            };

            return imageMetric;
        }

        private static double GetMetricOrDefault(string fieldName, JToken parsedResponse)
        {
            double metric = -1;
            if (double.TryParse(parsedResponse[fieldName].ToString(), out var parsedMetric))
            {
                metric = parsedMetric;
            }

            return metric;
        }

        private async Task<HttpResponseMessage> SendRequestToApiAsync(HttpRequestMessage request, Dictionary<string, object> contextualInformation)
        {
            using (var dependencyMeasurement = DependencyMeasurement.Start())
            {
                HttpResponseMessage response = null;
                try
                {
                    response = await _httpClient.SendAsync(request);
                    return response;
                }
                finally
                {
                    var statusCode = response?.StatusCode ?? HttpStatusCode.InternalServerError;
                    _logger.LogHttpDependency(request, statusCode, dependencyMeasurement, contextualInformation);
                }
            }
        }
    }
}
