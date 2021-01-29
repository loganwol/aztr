namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class ReleaseWorkflowTasksInputs
    {
        [JsonProperty(PropertyName = "testRunTitle")]
        public string TestRunTitle { get; set; }

        [JsonProperty(PropertyName = "testFiltercriteria")]
        public string TestFiltercriteria { get; set; }
    }
}