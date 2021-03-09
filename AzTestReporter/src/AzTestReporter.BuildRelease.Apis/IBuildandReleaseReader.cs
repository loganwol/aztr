namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IBuildandReleaseReader
    {
        /// <summary>
        /// Setup the authentication for the client including what headers 
        /// need to be regiestered using the Access Token.
        /// </summary>
        /// <param name="client">HTTP client.</param>
        /// <param name="pat">Personal Access Token.</param>
        void SetupandAuthenticate(HttpClient client, string pat);

        /// <summary>
        /// Gets all the releases in the specified Date range.
        /// </summary>
        /// <param name="minDateString">The start date.</param>
        /// <param name="maxDateString">The end date.</param>
        /// <returns>Returns the Response return from ADO.</returns>
        Task<AzureSuccessReponse> GetReleasesbyDateRangeAsync(string minDateString, string maxDateString);

        /// <summary>
        /// Gets the Release result for the specific Release ID.
        /// </summary>
        /// <param name="releaseId">The Release ID.</param>
        /// <returns>Returns the Release information.</returns>
        Task<Release> GetReleaseResultAsync(string releaseId);

        /// <summary>
        /// Gets the Tests run within a specified date range for a specific Release or build by it's ID.
        /// </summary>
        /// <param name="minDateString">The start date.</param>
        /// <param name="maxDateString">The end date.</param>
        /// <param name="targetReleaseId">The release or build id to query against.</param>
        /// <param name="fromBuild">Set to true if the ID is a build ID.</param>
        /// <returns>The response that ADO returns.</returns>
        Task<AzureSuccessReponse> GetTestRunListByDateRangeAsync(
            string minDateString,
            string maxDateString,
            string targetReleaseId,
            bool fromBuild);

        /// <summary>
        /// Gets all the builds for a specific definition within the date range
        /// and when specified a specific branch and if build result needs to be
        /// considered.
        /// </summary>
        /// <param name="definitionId">The definition ID to query for all builds.</param>
        /// <param name="branchName">The name of the branch.</param>
        /// <param name="minDateString">The start date.</param>
        /// <param name="maxDateString">The end date.</param>
        /// <param name="topCount">The number of build results to return.</param>
        /// <param name="includebuildswithunittestfailures">Set to true if results returns should include builds that succeeded but had unit test failures.</param>
        /// <returns>The response that ADO returns.</returns>
        Task<AzureSuccessReponse> GetBuildsbyDefinitionIdAsync(
            string definitionId,
            string branchName = null,
            string minDateString = null,
            string maxDateString = null,
            int topCount = 0,
            bool includebuildswithunittestfailures = false);


        /// <summary>
        /// Gets the Build data specific to the build Id.
        /// </summary>
        /// <param name="buildId">The build ID to get the data for.</param>
        /// <returns>The Build data object representing the data for the build.</returns>
        Task<BuildData> GetBuildData(string buildId);

        /// <summary>
        /// Gets the code coverage for a specific build by it's build id
        /// and if specified the repo name.
        /// </summary>
        /// <param name="buildId">The build ID to use to query ADO.</param>
        /// <param name="reponame">The repo name to use optionally to query ADO.</param>
        /// <returns>The response that ADO returns.</returns>
        Task<AzureSuccessReponse> GetTestBuildCoverageDataAsync(string buildId, string reponame = "");

        /// <summary>
        /// Get the test run results for a specific run id.
        /// </summary>
        /// <param name="runId">The test run id to use to query for test results in ADO.</param>
        /// <returns>The response that ADO returns.</returns>
        Task<AzureSuccessReponse> GetTestResultListAsync(int runId);

        /// <summary>
        /// Get the test results with links by test run id and specific result id.
        /// </summary>
        /// <param name="runId">The test run id to use to query for test results in ADO.</param>
        /// <param name="resultId">The test result id to use to query for test results in ADO.</param>
        /// <returns>The response that ADO returns.</returns>
        Task<TestResultData> GetTestResultWithLinksAsync(int runId, string resultId);

        /// <summary>
        /// Get the build data for a specific bug object.
        /// </summary>
        /// <param name="bug">The bug details.</param>
        /// <returns>The response that ADO returns.</returns>
        Task<AzureBugData> GetBugDataAsync(AzureBugLinkData bug);
    }
}
