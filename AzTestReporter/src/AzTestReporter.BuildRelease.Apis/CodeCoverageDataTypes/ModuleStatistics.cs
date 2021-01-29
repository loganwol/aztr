namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents one instance of Azure Module Statistics data.
    /// </summary>
    public class ModuleStatistics
    {
        /// <summary>
        /// Gets or sets the number of blocks not covered.
        /// </summary>
        [JsonProperty(PropertyName = "blocksNotCovered")]
        public int BlocksNotCovered { get; set; }

        /// <summary>
        /// Gets or sets the number of lines covered.
        /// </summary>
        [JsonProperty(PropertyName = "linesCovered")]
        public int LinesCovered { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks covered.
        /// </summary>
        [JsonProperty(PropertyName = "blocksCovered")]
        public int BlocksCovered { get; set; }

        /// <summary>
        /// Gets or sets the number of lines partially covered.
        /// </summary>
        [JsonProperty(PropertyName = "linesPartiallyCovered")]
        public int LinesPartiallyCovered { get; set; }

        /// <summary>
        /// Gets or sets the number of lines not covered.
        /// </summary>
        [JsonProperty(PropertyName = "linesNotCovered")]
        public int LinesNotCovered { get; set; }
    }
}
