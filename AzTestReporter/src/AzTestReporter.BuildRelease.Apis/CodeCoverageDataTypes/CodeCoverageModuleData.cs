namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the class for the data structure for code coverage as defined by Azure.
    /// </summary>
    public class CodeCoverageModuleData
    {
        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of blocks for the current module.
        /// </summary>
        [JsonProperty(PropertyName = "blockCount")]
        public int BlockCount { get; set; }

        /// <summary>
        /// Gets or sets the statistic code coverage information for the module.
        /// </summary>
        [JsonProperty(PropertyName = "statistics", Required = Required.Always)]
        public ModuleStatistics ModuleStatistics { get; set; }
    }
}
