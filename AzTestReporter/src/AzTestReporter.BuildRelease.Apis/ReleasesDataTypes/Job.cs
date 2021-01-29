namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;
    using System;
    using System.Globalization;

    public class Job
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "agentName")]
        public string AgentName { get; set; }

        [JsonProperty(PropertyName = "logUrl")]
        public string LogUrl { get; set; }

        [JsonProperty(PropertyName = "dateStarted")]
        public DateTime DateStarted { get; set; }
    }
}
