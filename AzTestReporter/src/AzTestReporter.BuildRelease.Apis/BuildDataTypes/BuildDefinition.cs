namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    /// <summary>
    /// ADO BuildDefinition object.
    /// </summary>
    public class BuildDefinition
    {
        /// <summary>
        /// Gets or sets the Build Definition ID.
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string DefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the Build definiton name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string DefinitionName { get; set; }

        /// <summary>
        /// Gets or sets the Build Definition path.
        /// </summary>
        [JsonProperty(PropertyName = "path", Required = Required.Always)]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the Build Definition path.
        /// </summary>
        [JsonProperty(PropertyName = "queueStatus", Required = Required.Always)]
        public string QueueStatus { get; set; }

        /// <summary>
        /// Gets a value indicating whether check if a build definition is marked as Archived.
        /// </summary>
        public bool IsNotArchived => !this.Path.Equals("\\\\Archive", System.StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets the Build repo without the CI_.
        /// </summary>
        public string RepoName => this.DefinitionName.Replace("CI_", string.Empty);

        /// <summary>
        /// Gets a value indicating whether the build is a CI Build.
        /// </summary>
        public bool IsCIRepo => this.QueueStatus.Equals("enabled", System.StringComparison.InvariantCultureIgnoreCase);
    }
}
