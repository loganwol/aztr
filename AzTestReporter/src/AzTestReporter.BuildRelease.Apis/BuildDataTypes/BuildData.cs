namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Globalization;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the data structure of Azure Build Data.
    /// Documentation can be found here - https://docs.microsoft.com/en-us/rest/api/azure/devops/build/builds/get?view=azure-devops-rest-5.1
    /// </summary>
    public partial class BuildData
    {
        /// <summary>
        /// Gets or sets the Build ID.
        /// </summary>
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string BuildId { get; set; }

        /// <summary>
        /// Gets or sets the Build number.
        /// </summary>
        [JsonProperty(PropertyName = "buildNumber", Required = Required.Always)]
        public string BuildNumber { get; set; }

        /// <summary>
        /// Gets or sets the Build result.
        /// </summary>
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public BuildStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the Build completion time.
        /// </summary>
        [JsonProperty(PropertyName = "finishTime")]
        public string FinishTime { get; set; }

        /// <summary>
        /// Gets or sets the Build completion time.
        /// </summary>
        [JsonProperty(PropertyName = "queueTime", Required = Required.Always)]
        public string QueueTime { get; set; }

        /// <summary>
        /// Gets or sets the Build links.
        /// </summary>
        [JsonProperty(PropertyName = "_links", Required = Required.Always)]
        public Link Links { get; set; }

        /// <summary>
        /// Gets or sets the Build definition the build is related to.
        /// </summary>
        [JsonProperty(PropertyName = "definition", Required = Required.Always)]
        public BuildDefinition Definition { get; set; }

        /// <summary>
        /// Gets or sets which repository the build belongs to.
        /// </summary>
        [JsonProperty(PropertyName = "repository", Required = Required.Always)]
        public BuildRepository Repository { get; set; }

        /// <summary>
        /// Gets or sets why the build was started.
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the branch from which the build was generated.
        /// </summary>
        [JsonProperty(PropertyName = "sourceBranch")]
        public string SourceBranch { get; set; }

        /// <summary>
        /// Gets or sets whom the build was generated for.
        /// </summary>
        [JsonProperty(PropertyName = "requestedFor")]
        public RequestedFor RequestedFor { get; set; }

        /// <summary>
        /// Gets the Build completion time.
        /// </summary>
        public DateTime ExecutionDateTime => Convert.ToDateTime(this.FinishTime, CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets when the build was queued to start.
        /// </summary>
        public DateTime BuildStartTime => Convert.ToDateTime(this.QueueTime, CultureInfo.InvariantCulture);

        public string GetBuildTestsUrl(string projectcollectionuri, string teamproject)
        {
            var buildtesturl = new StringBuilder();
            buildtesturl.Append(projectcollectionuri);
            buildtesturl.Append($"/{teamproject}");
            buildtesturl.Append($"/_build/results?buildId={this.BuildId}&view=ms.vss-test-web.build-test-results-tab");

            return buildtesturl.ToString();
        }

        /// <summary>
        /// Gets the Build definition ID.
        /// </summary>
        public string DefinitionId => this.Definition?.DefinitionId;

        /// <summary>
        /// Gets the Build name.
        /// </summary>
        public string BuildName => this.Definition?.DefinitionName;

        /// <summary>
        /// Gets the Build repository name.
        /// </summary>
        public string RepositoryName => this.Repository?.RepositoryName;

        /// <summary>
        /// Gets the Build branch name.
        /// </summary>
        public string BranchName
        {
            get
            {
                return this.SourceBranch?.Replace("refs/heads/", string.Empty);
            }
        }

        /// <summary>
        /// Gets the current Build URL.
        /// </summary>
        public string BuildUrl => this.Links.Web.Url;

        /// <summary>
        /// Gets the Build type.
        /// </summary>
        public JobType BuildType
        {
            get
            {
                JobType releaseType = JobType.Other;
                BuildReason buildReason = (BuildReason)Enum.Parse(typeof(BuildReason), this.Reason, true);
                if (buildReason == BuildReason.batchedCI || buildReason == BuildReason.schedule || buildReason == BuildReason.individualCI)
                {
                    releaseType = JobType.Master;
                }
                else if (this.Reason == "manual")
                {
                    releaseType = JobType.Private;
                }

                return releaseType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Build is a private build.
        /// </summary>
        public bool IsPrivateBuild => this.BranchName.StartsWith("personal", StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets a value of who the build was generated for.
        /// </summary>
        public string BuildRequestedBy
        {
            get
            {
                return this.RequestedFor?.UniqueName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Build was completed successfully.
        /// </summary>
        public bool BuiltSuccessfully => this.Result == "succeeded" && this.Status == BuildStatus.completed;
    }
}
