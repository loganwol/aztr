using AzTestReporter.BuildRelease.Apis.Common;
using Newtonsoft.Json;

namespace AzTestReporter.BuildRelease.Apis
{
    public class RunStatistic
    {
        public string state { get; set; }

        [JsonProperty(PropertyName = "outcome")]
        public OutcomeEnum outcome { get; set; }

        public int count { get; set; }
    }
}