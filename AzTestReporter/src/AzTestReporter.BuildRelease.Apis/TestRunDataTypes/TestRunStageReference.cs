namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class TestRunStageReference
    {
        [JsonProperty(PropertyName = "stageName")]
        public string StageName { get; set; }
    }
}