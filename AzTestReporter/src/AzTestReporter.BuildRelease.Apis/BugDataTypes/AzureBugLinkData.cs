namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class AzureBugLinkData
    {

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
