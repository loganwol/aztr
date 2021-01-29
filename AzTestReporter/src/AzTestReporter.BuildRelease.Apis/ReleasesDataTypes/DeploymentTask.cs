namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class DeploymentTask
    {
        [JsonProperty(PropertyName = "logUrl")]
        public string LogUrl { get; set; }

        [JsonProperty(PropertyName = "agentName")]
        public string AgentName { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "task")]
        public DeploymentTaskSubTask SubTask { get; set; }
    }
}
