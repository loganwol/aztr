namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NLog;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using Validation;

    public class BuildandReleaseReader : IBuildandReleaseReader
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string VSRMDEVAZUREURL = "https://vsrm.dev.azure.com/";
        private const string DEVAZUREURL = "https://dev.azure.com/";
        private string organizationname = string.Empty;
        private string projectname = string.Empty;
        private string systemteamfoundationserverurivsrm;
        private string systemteamfoundationcollectionuridev;
        private HttpClient httpClient;
        private bool enableoutputlogging = false;
        private string debugoutputfilename = string.Empty;

        public BuildandReleaseReader(string organization, string projectname)
        {
            this.organizationname = organization;
            this.projectname = projectname;
            this.VSRMDevOpsServerProjectURI = $"{VSRMDEVAZUREURL}/{this.organizationname}/{this.projectname}";
            this.DevOpsServerProjectURI = $"{DEVAZUREURL}/{this.organizationname}/{this.projectname}";
        }

        public BuildandReleaseReader(
            string systemteamfoundationserveruri,
            string systemteamfoundationcollectionuri,
            string systemteamproject,
            bool enabledebugoutput = false)
        {
            Requires.NotNull(systemteamfoundationserveruri, nameof(systemteamfoundationserveruri));
            Requires.NotNull(systemteamfoundationcollectionuri, nameof(systemteamfoundationcollectionuri));
            Requires.NotNull(systemteamproject, nameof(systemteamproject));

            this.projectname = systemteamproject;
            this.systemteamfoundationserverurivsrm = systemteamfoundationserveruri;
            this.systemteamfoundationcollectionuridev = systemteamfoundationcollectionuri;
            this.VSRMDevOpsServerProjectURI = $"{systemteamfoundationserveruri}/{this.projectname}";
            this.DevOpsServerProjectURI = $"{systemteamfoundationcollectionuri}/{this.projectname}";
            this.enableoutputlogging = enabledebugoutput;
        }

        public string VSRMDevOpsServerProjectURI { get; private set; }

        public string DevOpsServerProjectURI { get; private set; }

        // Input:HttpClient, build definition id
        // Output: list of LKG build ids
        public async Task<AzureSuccessReponse> GetReleasesbyDateRangeAsync(string minDateString, string maxDateString)
        {
            // builds list with definition id
            string queryurl = $"{VSRMDevOpsServerProjectURI}/_apis/release/releases?minLastUpdatedDate={minDateString}&maxLastUpdatedDate={maxDateString}&$top=100&api-version=5.0";
            debugoutputfilename = "aztr-ReleasesbyDateRange";
            return await this.QueryAzureDevOpsAsyncandDeserializetoASR(queryurl);
        }

        public async Task<Release> GetReleaseResultAsync(string releaseId)
        {
            string queryurl = $"{VSRMDevOpsServerProjectURI}/_apis/release/releases/{releaseId}?api-version=5.1";
            debugoutputfilename = "aztr-ReleaseResult";
            string responseBody = await QueryAzureDevOpsAsyncGetResponseBody(queryurl);

            return JsonConvert.DeserializeObject<Release>(responseBody);
        }

        // Input: HttpClient
        // Output: TestRunsList object
        public async Task<AzureSuccessReponse> GetTestRunListByDateRangeAsync(string minDateString, string maxDateString, string targetReleaseId, bool fromBuild)
        {
            string queryUrl;

            // runs list with details
            if (fromBuild)
            {
                queryUrl = $"{this.DevOpsServerProjectURI}/_apis/test/runs?minLastUpdatedDate={minDateString}&maxLastUpdatedDate={maxDateString}&buildIds={targetReleaseId}&includeRunDetails=true&api-version=5.1";
            }
            else
            {
                queryUrl = $"{this.DevOpsServerProjectURI}/_apis/test/runs?minLastUpdatedDate={minDateString}&maxLastUpdatedDate={maxDateString}&releaseIds={targetReleaseId}&includeRunDetails=true&api-version=5.1";
            }

            debugoutputfilename = "aztr-TestRunListbyDateRange";
            string responseBody = await this.QueryAzureDevOpsAsyncGetResponseBody(queryUrl);

            // deserialize runs list
            return JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
        }

        public async Task<AzureSuccessReponse> GetBuildsbyDefinitionIdAsync(
            string definitionId,
            string branchName = null,
            string minDateString = null,
            string maxDateString = null,
            int topCount = 0,
            bool includebuildswithunittestfailures = false)
        {
            Requires.NotNullOrEmpty(definitionId, nameof(definitionId));
            StringBuilder urlParameters = new StringBuilder();
            urlParameters.Append($"definitions={definitionId}");
            if (!includebuildswithunittestfailures)
            {
                urlParameters.Append($"&statusFilter=completed&resultFilter=succeeded");
            }

            if (topCount > 0)
            {
                urlParameters.Append($"&$top={topCount}");
            }

            if (!string.IsNullOrEmpty(branchName))
            {
                urlParameters.Append($"&branchName={branchName}");
            }

            if (!string.IsNullOrEmpty(minDateString))
            {
                urlParameters.Append($"&minTime={minDateString}");
            }

            if (!string.IsNullOrEmpty(maxDateString))
            {
                urlParameters.Append($"&maxTime={maxDateString}");
            }

            string query = $"{this.DevOpsServerProjectURI}/_apis/build/builds?{urlParameters}&api-version=5.1";

            Log?.Trace("Getting Builds by definition");
            debugoutputfilename = "aztr-BuildsbyDefinitionID";
            return await QueryAzureDevOpsAsyncandDeserializetoASR(query);
        }

        public async Task<BuildData> GetBuildData(string buildId)
        {
            var query = $"{this.DevOpsServerProjectURI}/_apis/build/builds/{buildId}?api-version=5.1";

            Log?.Trace("Getting build data");
            debugoutputfilename = "aztr-BuildData";
            
            string responseBody = await this.QueryAzureDevOpsAsyncGetResponseBody(query);
            
            return JsonConvert.DeserializeObject<BuildData>(responseBody);
        }

        public async Task<AzureSuccessReponse> GetTestBuildCoverageDataAsync(string buildId, string reponame = "")
        {
            string query = $"{this.DevOpsServerProjectURI}/_apis/test/codecoverage?buildId={buildId}&flags=7&api-version=5.1-preview.1";
            if (reponame != string.Empty)
            {
                query = $"{DEVAZUREURL}/{this.organizationname}/{reponame}/_apis/test/codecoverage?buildId={buildId}&flags=7&api-version=5.1-preview.1";
            }

            Log?.Trace("Getting unit test coverage.");
            debugoutputfilename = "aztr-BuildCodeCoverage";
            return await QueryAzureDevOpsAsyncandDeserializetoASR(query);
        }

        public async Task<AzureSuccessReponse> GetTestResultListAsync(int runId)
        {
            string query = $"{this.DevOpsServerProjectURI}/_apis/test/Runs/{runId}/results?api-version=5.1";
            Log?.Trace("Getting Test results by run id.");
            debugoutputfilename = $"aztr-TestResults-{runId}";
            return await QueryAzureDevOpsAsyncandDeserializetoASR(query);
        }

        public async Task<TestResultData> GetTestResultWithLinksAsync(int runId, string resultId)
        {
            Requires.NotNullOrEmpty(resultId, nameof(resultId));

            string query = $"{this.DevOpsServerProjectURI}/_apis/test/Runs/{runId}/results/{resultId}?detailsToInclude=workitems&api-version=5.1";

            string responseBody = await this.QueryAzureDevOpsAsyncGetResponseBody(query);
            Log?.Trace("Getting Test result with links.");
            debugoutputfilename = "aztr-TestResultLinks";
            return JsonConvert.DeserializeObject<TestResultData>(responseBody);
        }

        /// <inheritdoc/>
        public async Task<AzureBugData> GetBugDataAsync(AzureBugLinkData bug)
        {
            string queryString = $"{this.DevOpsServerProjectURI}/_apis/wit/workitems/{bug.Id}?api-version=5.1";

            string responseBody = await this.QueryAzureDevOpsAsyncGetResponseBody(queryString);
            AzureBugData tempBug = JsonConvert.DeserializeObject<AzureBugData>(responseBody);
            tempBug.Link = $"{this.DevOpsServerProjectURI}/{this.projectname}/_workitems/edit/{bug.Id}";
            return tempBug;
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        // Helper Methods
        // Input: HttpClient, PersonalAccessToken
        // Output: set up and authenticate before accessing ADO
        public void SetupandAuthenticate(HttpClient client, string pat)
        {
            this.httpClient = client;

            // set up and connect to ADO
            this.httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", string.Empty, pat))));
        }

        protected async Task<string> QueryAzureDevOpsAsyncGetResponseBody(string queryurl)
        {
            string responseBody = string.Empty;

            Log?.Trace($"Query Url {queryurl}");
            using (HttpResponseMessage response = await this.httpClient.GetAsync(queryurl))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new TestResultAzureQueryException("Unauthorized access", queryurl, responseBody);
                }

                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();

                Log?.Info("Logging debug information for ADO queries and responses.");
                if (!string.IsNullOrEmpty(debugoutputfilename))
                {
                    File.WriteAllText($"{debugoutputfilename}.json", responseBody);
                    Log?.Trace($"Logged to {debugoutputfilename}.json");
                }

                if (this.enableoutputlogging)
                {
                    Log?.Trace("------------- Begin Response body ----------");
                    Log?.Trace(responseBody);
                    Log?.Trace("------------- End Response body ------------");
                }
            }

            return responseBody;
        }

        private async Task<AzureSuccessReponse> QueryAzureDevOpsAsyncandDeserializetoASR(string queryString)
        {
            Requires.NotNullOrEmpty(queryString, nameof(queryString));

            AzureSuccessReponse azureSuccessReponse;
            string responseBody = string.Empty;

            try
            {
                responseBody = await this.QueryAzureDevOpsAsyncGetResponseBody(queryString);
                azureSuccessReponse = AzureSuccessReponse.ConverttoAzureSuccessResponse(responseBody);
            }
            catch (JsonException jex)
            {
                throw new TestResultAzureQueryException(jex.Message, queryString, responseBody);
            }

            return azureSuccessReponse;
        }
    }
}
