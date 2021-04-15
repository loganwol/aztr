namespace AzTestReporter.BuildRelease.Builder
{
    using NLog;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class AzurePipelineEnvironmentOptions
    {
        internal Dictionary<string, string> environmentvars;
        internal static Logger Log;
        private Dictionary<string, string> displayvalues;

        public AzurePipelineEnvironmentOptions()
        {
            Log = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the value indicating that TRR is executing within a pipeline or not.
        /// </summary>
        public bool IsExecutinginReleasePipeline { get; set; }

        /// <summary>
        /// Gets the team foundation collection uri. Typically starts with
        /// https://dev.azure.com/{organizationname}.
        /// </summary>
        public string SystemTeamFoundationCollectionURI { get; internal set; }

        /// <summary>
        /// Gets the team foundation server uri. Typically starts with
        /// https://vsrm.dev.azure.com/{organizationname}.
        /// </summary>
        public string SystemTeamFoundationServerURI { get; internal set; }

        /// <summary>
        /// Gets or sets the name of the project as registered in Azure DEVOPS.
        /// </summary>
        public string SystemTeamProject { get; set; }

        /// <summary>
        /// Gets or sets the Branch from which the code being reported on was built.
        /// </summary>
        public string ReleaseSourceBranchName { get; set; } = "refs/heads/master";

        /// <summary>
        /// Gets or sets the Build repo name.
        /// </summary>
        public string BuildRepositoryName { get; set; }

        /// <summary>
        /// Gets or sets the Build definition name.
        /// </summary>
        public string BuildDefinitionName { get; set; }

        /// <summary>
        /// Gets or sets the current build number set.
        /// </summary>
        public string BuildNumber { get; set; }

        /// <summary>
        /// Gets the current build definition ID.
        /// </summary>
        public string BuildDefinitionID { get; internal set; }

        /// <summary>
        /// Gets the current build ID.
        /// </summary>
        public string BuildID { get; internal set; }

        /// <summary>
        /// Gets the current ID of the agent being used.
        /// </summary>
        public string AgentID { get; internal set; }

        /// <summary>
        /// Gets the current run attempt for the stage.
        /// </summary>
        public int ReleaseAttempt { get; internal set; }

        /// <summary>
        /// Gets the key that's used to communicate with Azure.
        /// </summary>
        public string SystemAccessToken { get; internal set; }

        /// <summary>
        /// Gets the pipeline context the code is running in.
        /// </summary>
        public string SystemHostType { get; internal set; }

        /// <summary>
        /// Gets or sets the Release definition name for which you would like to generate a report.
        /// </summary>
        public string ReleaseDefinitionName { get; set; }

        /// <summary>
        /// Gets or sets the Execution Stage in the Release for which you would like to generate a report.
        /// </summary>
        public string ReleaseExecutionStage { get; set; }

        /// <summary>
        /// Gets the Release stage ID from the pipeline.
        /// </summary>
        public int ReleaseStageID { get; set; }

        /// <summary>
        /// Gets the release pipelines default working directory.
        /// </summary>
        public string ReleasePipelineDefaultWorkingDirectory { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether gets if the current execution context is Release Pipeline or BuildPipeline.
        /// </summary>
        public bool IsReleasePipeline { get; internal set; } = false;

        /// <summary>
        /// Gets the release ID from the release environment RELEASE_RELEASEID variable.
        /// </summary>
        public string ReleaseID { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the current build is an official build.
        /// </summary>
        public bool IsOfficialBranch
        {
            get
            {
                if (this.environmentvars == null ||
                    (!string.IsNullOrEmpty(this.BuildDefinitionName) &&
                    this.BuildDefinitionName.StartsWith("PR", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(this.BranchName))
                {
                    return this.BranchName.StartsWith("release", StringComparison.InvariantCultureIgnoreCase) ||
                        this.BranchName.StartsWith("feature", StringComparison.InvariantCultureIgnoreCase) ||
                        this.BranchName.StartsWith("master", StringComparison.InvariantCultureIgnoreCase) ||
                        this.BranchName.StartsWith("main", StringComparison.InvariantCultureIgnoreCase) ||
                        this.BranchName.StartsWith("product", StringComparison.InvariantCultureIgnoreCase);
                }

                return false;
            }
        }

        private string BranchName => this.ReleaseSourceBranchName?.Replace("refs/heads/", string.Empty);

        /// <summary>
        /// Read the pipeline variables.
        /// </summary>
        /// <param name="readreleasepipelineenvvars">Set to true if you want to read the Release pipeline related environment variables.</param>
        public void Read(bool readreleasepipelineenvvars = true)
        {
            if (this.environmentvars == null)
            {
                this.environmentvars = Environment.GetEnvironmentVariables()
                        .Cast<DictionaryEntry>()
                        .ToDictionary(r => r.Key.ToString(), r => r.Value.ToString());
            }

            bool runninglocal = false;

            if (!this.environmentvars.ContainsKey("SYSTEM_HOSTTYPE"))
            {
                // warning that system is likely running local.
                runninglocal = true;
            }

            if (!runninglocal)
            {
                this.SystemHostType = this.environmentvars["SYSTEM_HOSTTYPE"];
            }

            if (this.environmentvars["SYSTEM_ENABLEACCESSTOKEN"].Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ApplicationException("Please set the enable access to token for this utility to work.");
            }

            this.IsExecutinginReleasePipeline = true;

            if (readreleasepipelineenvvars == true && !runninglocal && this.SystemHostType.Equals("release", StringComparison.InvariantCultureIgnoreCase))
            {
                this.IsReleasePipeline = true;
                Log?.Info("Reading release variables for Integration test run details.");

                this.ReleaseID = this.environmentvars["RELEASE_RELEASEID"];
                this.ReleaseDefinitionName = this.environmentvars["RELEASE_DEFINITIONNAME"];
                this.ReleaseExecutionStage = this.environmentvars["RELEASE_ENVIRONMENTNAME"];
                var id = this.environmentvars["RELEASE_ENVIRONMENTID"];
                if (!string.IsNullOrEmpty(id))
                {
                    this.ReleaseStageID = int.Parse(id);
                }

                this.AgentID = this.environmentvars["AGENT_ID"];
                this.ReleaseAttempt = int.Parse(this.environmentvars["RELEASE_ATTEMPTNUMBER"]);
            }

            if (this.environmentvars.ContainsKey("SYSTEM_DEFAULTWORKINGDIRECTORY"))
            {
                this.ReleasePipelineDefaultWorkingDirectory = this.environmentvars["SYSTEM_DEFAULTWORKINGDIRECTORY"];
            }

            if (this.environmentvars.ContainsKey("SYSTEM_ACCESSTOKEN"))
            {
                this.SystemAccessToken = this.environmentvars["SYSTEM_ACCESSTOKEN"];
            }

            this.SystemTeamFoundationCollectionURI = this.environmentvars["SYSTEM_TEAMFOUNDATIONCOLLECTIONURI"];
            this.SystemTeamFoundationServerURI = this.environmentvars["SYSTEM_TEAMFOUNDATIONSERVERURI"];
            this.SystemTeamProject = this.environmentvars["SYSTEM_TEAMPROJECT"];

            this.ReleaseSourceBranchName = this.environmentvars["BUILD_SOURCEBRANCH"];
            this.BuildRepositoryName = this.environmentvars["BUILD_REPOSITORY_NAME"];
            this.BuildDefinitionName = this.environmentvars["BUILD_DEFINITIONNAME"];
            this.BuildNumber = this.environmentvars["BUILD_BUILDNUMBER"];
			this.BuildID = this.environmentvars["BUILD_BUILDID"];

            if (this.SystemHostType.Equals("release", StringComparison.InvariantCultureIgnoreCase))
            {
                Log?.Info("Running in release pipeline.");
                this.BuildDefinitionID = this.environmentvars["BUILD_DEFINITIONID"];
                Log?.Debug($"Build definition ID via build definition ID = {this.environmentvars["BUILD_DEFINITIONID"]}");
            }
            else if (this.SystemHostType.Equals("build", StringComparison.InvariantCultureIgnoreCase))
            {
                Log?.Info("Running in build pipeline.");
                this.BuildDefinitionID = this.environmentvars["SYSTEM_DEFINITIONID"];
                Log?.Debug($"Build definition ID via system id = {this.environmentvars["SYSTEM_DEFINITIONID"]}");
            }
        }

        public override string ToString()
        {
            this.displayvalues = new Dictionary<string, string>()
            {
                { "\tCollection URI set to      ", this.SystemTeamFoundationCollectionURI },
                { "\tServer URI set to          ", this.SystemTeamFoundationServerURI },
                { "\tTeam project set to        ", this.SystemTeamProject },
                { "\tIs official branch         ", this.IsOfficialBranch.ToString() },
                { "\tBuild related variables ===", string.Empty },
                { "\tBuild source branch        ", this.ReleaseSourceBranchName },
                { "\tBuild Definition name      ", this.BuildDefinitionName },
                { "\tBuild Definition ID        ", this.BuildDefinitionID },
                { "\tBuild ID                   ", this.BuildID },
                { "\tBuild Repository name      ", this.BuildRepositoryName },
                { "\tBuild number               ", this.BuildNumber },
                { "\tWorking directory          ", this.ReleasePipelineDefaultWorkingDirectory },
                { "\tAccess token specified     ", (!string.IsNullOrEmpty(this.SystemAccessToken)).ToString() },
            };

            if (this.IsReleasePipeline)
            {
                this.displayvalues.Add("\tRelease related variables =====", string.Empty);
                this.displayvalues.Add("\tRelease Definition name        ", this.ReleaseDefinitionName);
                this.displayvalues.Add("\tRelease ID                     ", this.ReleaseID);
                this.displayvalues.Add("\tRelease Environment ID         ", this.ReleaseStageID.ToString());
                this.displayvalues.Add("\tRelease Stage name             ", this.ReleaseExecutionStage);
                this.displayvalues.Add("\tRelease Attempt Number         ", this.ReleaseAttempt.ToString());
                this.displayvalues.Add("\tAgent ID                       ", this.AgentID);
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("******************** Pipeline Args **********************");
            foreach (var option in this.displayvalues.Keys)
            {
                stringBuilder.AppendLine($"{option}: {this.displayvalues[option]}");
            }

            return stringBuilder.ToString();
        }

        public Dictionary<string, string> ToDictionary()
        {
            _ = this.ToString();
            return this.displayvalues;
        }
    }
}
