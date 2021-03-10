using Newtonsoft.Json;

namespace AzTestReporter.BuildRelease.Apis
{
    public class VariableValue
    {
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "allowOverride")]
        public bool Override { get; set; }
    }
}