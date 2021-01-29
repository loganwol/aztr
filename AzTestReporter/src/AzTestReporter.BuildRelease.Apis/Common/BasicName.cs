namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class BasicName
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
