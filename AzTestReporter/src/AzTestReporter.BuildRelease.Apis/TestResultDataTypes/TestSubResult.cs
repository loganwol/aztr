using AzTestReporter.BuildRelease.Apis.Common;
using Newtonsoft.Json;

namespace AzTestReporter.BuildRelease.Apis
{
    public class TestSubResult
    {
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "outcome")]
        public OutcomeEnum Outcome { get; set; }

        [JsonProperty(PropertyName = "durationInMs")]
        public int durationInMs { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "stackTrace")]
        public string StackTrace { get; set; }
    }
}