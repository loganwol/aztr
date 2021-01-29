namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    /// <summary>
    /// Implements the JSON data structure for a web url json section.
    /// </summary>
    public class Web
    {
        /// <summary>
        /// The URL link for a web object in ADO.
        /// </summary>
        [JsonProperty(PropertyName = "href", Required = Required.Always)]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }
    }
}
