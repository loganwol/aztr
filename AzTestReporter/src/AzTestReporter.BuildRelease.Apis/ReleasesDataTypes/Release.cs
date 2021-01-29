namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using Validation;
    using System.Text;

    public partial class Release
    {
        private List<DeployStep> currentdeploymentsteps;

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "releaseDefinition", Required = Required.Always)]
        public ReleaseDefinition ReleaseDefinition { get; set; }

        [JsonProperty(PropertyName = "_links", Required = Required.Always)]
        public Link Links { get; set; }

        [JsonProperty(PropertyName = "environments")]
        public IReadOnlyList<Environment> Environments { get; set; }

        [JsonProperty(PropertyName = "createdOn", Required = Required.Always)]
        public DateTime CreatedOn { get; set; }

        [JsonProperty(PropertyName = "reason", Required = Required.Always)]
        public string Reason { get; set; }

        [JsonProperty(PropertyName = "createdBy", Required = Required.Always)]
        public ReleaseCreatedBy CreatedBy { get; set; }

        [JsonProperty(PropertyName = "artifacts")]
        public ReleaseArtifact[] Artifacts { get; set; }

        public int CurrentAttempt { private get;  set; }

        private IReadOnlyCollection<DeployStep> CurrentDeployStep
        {
            get
            {
                if (this.currentdeploymentsteps == null)
                {
                    currentdeploymentsteps = this.Environments
                        .SelectMany(r => r.DeploySteps)
                        .Where(r => r.Attempt == this.CurrentAttempt)
                        .Select(r => r)
                        .ToList();
                }

                return this.currentdeploymentsteps;
            }
        }

        /// <summary>
        /// Gets the type of Release.
        /// </summary>
        public JobType ReleaseType
        {
            get
            {
                JobType releaseType = JobType.Master;
                ReleaseReason releaseReason = (ReleaseReason)Enum.Parse(typeof(ReleaseReason), this.Reason, true);
                if (releaseReason == ReleaseReason.Manual)
                {
                    releaseType = JobType.Private;
                }

                return releaseType;
            }
        }

        /// <summary>
        /// Gets the Release reason.
        /// </summary>
        public ReleaseReason ReasonofRelease => (ReleaseReason)Enum.Parse(typeof(ReleaseReason), this.Reason, true);

        /// <summary>
        /// Gets the Branch name.
        /// </summary>
        public string BranchName
        {
            get
            {
                return this.Artifacts?.Select(r =>
                    r.DefinitionReference.Branches.Name).Distinct().FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the envrionemnt reponame.
        /// </summary>
        public string RepoName
        {
            get
            {
                return this.Artifacts?
                    .Where(r => r.IsPrimary)
                    .Select(r => r.DefinitionReference.Repository.Name)
                    .Distinct().FirstOrDefault();
            }
        }


        /// <summary>
        /// Gets list of test runs.
        /// </summary>
        public List<string> TestRunNames
        {
            get
            {
                return this.CurrentDeployStep.SelectMany(r => r.TestrunNames).ToList();
            }
        }

        public DateTime FirstJobExecutionDateTime
        {
            get
            {
                return this.CurrentDeployStep.Min(r => r.FirstJobExecutionDateTime);
            }
        }

        /// <summary>
        /// Gets the ID of a stage based on it's name.
        /// </summary>
        /// <param name="stageName">The name of the stage to find in the enviornment.</param>
        /// <returns>The ID of the stage if found.</returns>
        public int GetStageId(string stageName)
        {
            Requires.NotNullOrEmpty(stageName, nameof(stageName));

            if (this.Environments == null)
            {
                return -1;
            }

            var envrionmentids = this.Environments?
                .Where(r => r.Name.StartsWith(stageName, StringComparison.InvariantCultureIgnoreCase))
                .Select(r => r.Id)
                .Distinct().ToList();

            if (envrionmentids.Count > 1)
            {
                throw new TestResultReportingReleaseNotFoundException("A stage was reported multiple times as an enviornment in the Releases JSON.");
            }

            if (envrionmentids.FirstOrDefault() == 0)
            {
                return -1;
            }

            return envrionmentids.First();
        }

        /// <summary>
        /// Check if a stage exists in the environment.
        /// </summary>
        /// <param name="desiredStage">The name of the stage to find.</param>
        /// <returns>True/False if stage was found.</returns>
        public bool ContainsStage(string desiredStage)
        {
            var stageslist = this.Environments?.Select(r => r.Name).Distinct().ToList();
            if (stageslist != null)
            {
                return stageslist.Any(r => r.ToUpperInvariant().Contains(desiredStage.ToUpperInvariant()));
            }

            return false;
        }

        /// <summary>
        /// Gets the failed task name if one is found in a stage.
        /// </summary>
        public string FailedTaskName
        {
            get
            {
                return this.CurrentDeployStep
                    .Where(r => !string.IsNullOrEmpty(r.FailedTask))
                    .Select(r => r.FailedTask)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the build number associated with the Release.
        /// </summary>
        public string AssociatedBuildNumber
        {
            get
            {
                return this.Artifacts?.Select(r =>
                    r.DefinitionReference.Version.Name).Distinct().FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the Release definition associated with the current Release.
        /// </summary>
        public string ReleaseDefinitionName
        {
            get
            {
                return this.ReleaseDefinition.Name;
            }
        }

        public string GetTestRunLink(string projectcollection, string projectname)
        {
            var testrunlink = new StringBuilder();
            testrunlink.Append(projectcollection);
            testrunlink.Append($"/{projectname}");
            testrunlink.Append($"/_releaseProgress?releaseId={this.Id}&environmentId={this.Environments[0].Id}&_a=release-environment-extension&extensionId=ms.vss-test-web.test-result-in-release-environment-editor-tab");
            return testrunlink.ToString();
        }

        public string GetTestRunLogLink(string projectcollection, string projectname)
        {
            var testrunlink = new StringBuilder();
            testrunlink.Append(projectcollection);
            testrunlink.Append($"/{projectname}");
            testrunlink.Append($"/_releaseProgress?releaseId={this.Id}&environmentId={this.Environments[0].Id}&_a=release-environment-logs");
            return testrunlink.ToString();
        }

    }
}
