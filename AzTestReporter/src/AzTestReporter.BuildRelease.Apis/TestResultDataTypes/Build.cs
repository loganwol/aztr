namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class Build
    {
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url", Required = Required.Always)]
        public string Url { get; set; }
    }
}
