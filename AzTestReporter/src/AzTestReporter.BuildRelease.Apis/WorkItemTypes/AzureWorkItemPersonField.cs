namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Implements the JSON Data structure for a work item person field object in Azure.
    /// </summary>
    public partial class AzureWorkItemPersonField
    {
        /// <summary>
        /// Gets or sets the work item display.
        /// </summary>
        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "uniqueName")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "isContainer")]
        public bool IsContainer { get; set; }
    }
}
