namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class RequestedFor
    {
        [JsonProperty(PropertyName = "uniqueName")]
        public string UniqueName { get; set; }
    }
}
