namespace DockerHub.Metrics.Runtime.AzureFunction.Contracts
{
    public class ScrapeRequestMessage
    {
        public string RepoName { get; set; }
        public string ImageName { get; set; }
    }
}
