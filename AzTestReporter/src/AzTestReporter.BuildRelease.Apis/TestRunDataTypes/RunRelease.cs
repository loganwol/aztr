namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class RunRelease
    {
        [JsonProperty(PropertyName = "environmentId")]
        public int EnvironmentId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }
}