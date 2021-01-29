namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class ReleaseCreatedBy
    {
        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "uniqueName")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "isContainer")]
        public bool IsContainer { get; set; }
    }
}
