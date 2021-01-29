namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class CodeCoverageConfiguration
    {
        [JsonProperty(PropertyName = "flavor")]
        public string Flavor { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
