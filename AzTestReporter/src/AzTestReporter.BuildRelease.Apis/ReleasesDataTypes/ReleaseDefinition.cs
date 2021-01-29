namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class ReleaseDefinition
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "branches")]
        public BasicName Branches { get; set; }

        [JsonProperty(PropertyName = "repository")]
        public BasicName Repository { get; set; }

        [JsonProperty(PropertyName = "version")]
        public BasicName Version { get; set; }
    }
}
