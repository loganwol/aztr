namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using Validation;

    /// <summary>
    /// Implements getting a list of builds for a specific build definition. 
    /// This gives us more details of all the builds for that specific definition.
    /// </summary>
    public class BuildsCollection : List<BuildData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildsCollection"/> class.
        /// </summary>
        /// <param name="reader">Devops reader.</param>
        /// <param name="buildDefinitionID">Build definition ID.</param>
        /// <param name="branchName">The branch name.</param>
        /// <param name="minDateString">The min date string.</param>
        /// <param name="maxDateString">The max date string.</param>
        /// <param name="topBuildsCount">The count of top builds.</param>
        /// <param name="includebuildswithunittestfailures">Set to true if the response returned needs to include builds that completed successfully but had unit test failures.</param>
        /// <param name="retry">When a build is not found in the min/max date range, try expanding the search.</param>
        public BuildsCollection(
            IBuildandReleaseReader reader,
            string buildDefinitionID,
            string branchName = null,
            string minDateString = null,
            string maxDateString = null,
            int topBuildsCount = 0,
            bool includebuildswithunittestfailures = false, 
            bool retry = false)
        {
            Requires.NotNull(reader, nameof(reader));
            Requires.NotNull(buildDefinitionID, nameof(buildDefinitionID));

            var buildDataASR = reader.GetBuildsbyDefinitionIdAsync(buildDefinitionID, branchName, minDateString, maxDateString, topBuildsCount, includebuildswithunittestfailures).GetAwaiter().GetResult();
            if ((buildDataASR == null || buildDataASR.Count == 0) && retry)
            {
                buildDataASR = reader.GetBuildsbyDefinitionIdAsync(buildDefinitionID, branchName, topCount: topBuildsCount, includebuildswithunittestfailures: includebuildswithunittestfailures).GetAwaiter().GetResult();
            }

            if (buildDataASR == null || buildDataASR.Count == 0)
            {
                throw new TestResultReportingException($"Details for {buildDefinitionID} could not be found.");
            }

            this.AddRange(AzureSuccessReponse.ConvertTo<BuildData>(buildDataASR));
        }

        internal BuildsCollection(AzureSuccessReponse successReponse)
        {
            this.AddRange(AzureSuccessReponse.ConvertTo<BuildData>(successReponse));
        }

        /// <summary>
        /// Given a build number returns the build object if it's found and has successfully built.
        /// </summary>
        /// <param name="buildnumber">The build number to lookup.</param>
        /// <returns>A valid Build object or null.</returns>
        public BuildData GetBuildbyBuildNumber(string buildnumber)
        {
            Requires.NotNullOrEmpty(buildnumber, nameof(buildnumber));

            var sortedbuilds = this.OrderByDescending(r => r.BuildStartTime);

            var buildswithbuildnumber = this.OrderByDescending(r => r.BuildStartTime)
                .Where(r => r.BuildNumber.Equals(buildnumber.Trim(), StringComparison.InvariantCultureIgnoreCase));

            return buildswithbuildnumber
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the build data for the latest build from
        /// a specific repo from only the triggered jobs.
        /// </summary>
        /// <param name="definitionId">Definition ID.</param>
        /// <param name="buildnumber">The build number to filter against.</param>
        /// <returns>Return a build data object or null if it does not meet criteria.</returns>
        public BuildData GetBuildDataforBuild(string definitionId, string buildnumber = "")
        {
            Requires.NotNullOrEmpty(definitionId, nameof(definitionId));

            if (string.IsNullOrEmpty(buildnumber))
            {
                return this.Where(r => r.DefinitionId == definitionId)
                    .OrderByDescending(r => r.ExecutionDateTime)
                    .FirstOrDefault();
            }

            return this.Where(r => r.DefinitionId == definitionId && r.BuildNumber.ToUpperInvariant() == buildnumber.ToUpperInvariant()).FirstOrDefault();
        }
    }
}
