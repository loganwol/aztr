namespace AzTestReporter.BuildRelease.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using AutoMapper;
    using Newtonsoft.Json;
    using NLog;
    using AzTestReporter.BuildRelease.Apis;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using Validation;
    using static AzTestReporter.BuildRelease.Apis.Exceptions.TestResultReportingReleaseNotFoundException;
    using System.Text;

    /// <summary>
    /// Report builder class.
    /// </summary>
    public class ReportBuilder
    {
        internal static Logger Log;
        private IBuildandReleaseReader buildandReleaseReader;

        public ReportBuilder(IBuildandReleaseReader buildandReleaseReader)
        {
            Requires.NotNull(buildandReleaseReader, nameof(buildandReleaseReader));

            this.buildandReleaseReader = buildandReleaseReader;
        }

        public DailyHTMLReportBuilder GetReleasesRunsandResults(ref ReportBuilderParameters builderParameters)
        {
            Requires.NotNull(builderParameters.PipelineEnvironmentOptions, nameof(builderParameters.PipelineEnvironmentOptions));

            Log = LogManager.GetCurrentClassLogger();

            CodeCoverageModuleDataCollection coverageAggregateColl = null;

            string buildoreleaseid = string.Empty;
            string testrundashboardlink = string.Empty;
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string buildVersion = string.Empty;
            string reportBody = string.Empty;
            string branchName = string.Empty;
            List<TestResultData> testResultData = new List<TestResultData>();
            List<string> logs = new List<string>();
            List<Run> supportedRunsList = null;
            List<string> testrunnameslist = null;
            int executionStageId = -1;
            bool isPipelineFailed = false;
            bool tasksFailed = false;
            bool testRunContainsFailures = false;
            DateTime executiontime = DateTime.Now;
            string failedTaskName = string.Empty;
            DateTime releasetesttaskexecutiontime = DateTime.MaxValue;
            List<AzureBugData> bugs = new List<AzureBugData>();
            Release releaseDetails = null;
            Dictionary<string, string> pipelineVariables = null;
            string requestedBy = string.Empty;
            string releasename = string.Empty;

            Log?.Trace("Setting up HTTPclient to call REST apis");

            // use REST API and HTTP Client to access ADO
            using (HttpClient client = new HttpClient())
            {
                buildandReleaseReader.SetupandAuthenticate(client, builderParameters.PipelineEnvironmentOptions.SystemAccessToken);

                Log?.Trace("Authentication completed.");

                if (builderParameters.ResultSourceIsBuild)
                {
                    Log?.Info("Getting data for Unit test results");

                    BuildData builddata = buildandReleaseReader.GetBuildData(builderParameters.PipelineEnvironmentOptions.BuildID).GetAwaiter().GetResult();
                    if (builddata == null)
                    {
                        throw new TestResultReportingException($"Build data for build id: {builderParameters.PipelineEnvironmentOptions.BuildID} was not found.");
                    }

                    pipelineVariables = builddata.BuildVariables;
                    executiontime = string.IsNullOrEmpty(builddata.FinishTime) ? builddata.BuildStartTime : builddata.ExecutionDateTime;

                    testrunnameslist = new List<string>() { builddata.BuildId };
                    branchName = builddata.BranchName;
                    builderParameters.IsPrivateRelease = builddata.IsPrivateBuild;
                    buildoreleaseid = builddata.BuildId;
                    testrundashboardlink = builddata.GetBuildTestsUrl(
                        builderParameters.PipelineEnvironmentOptions.SystemTeamFoundationCollectionURI,
                        builderParameters.PipelineEnvironmentOptions.SystemTeamProject);

                    coverageAggregateColl = new CodeCoverageModuleDataCollection(this.buildandReleaseReader, builddata.BuildId);

                    requestedBy = builddata.BuildRequestedBy;

                    if (string.IsNullOrEmpty(builderParameters.PipelineEnvironmentOptions.BuildNumber) ||
                        builddata.BuildNumber != builderParameters.PipelineEnvironmentOptions.BuildNumber)
                    {
                        Log?.Info($"Build number was not found, setting to {builddata.BuildNumber}");
                        buildVersion = builddata.BuildNumber;
                    }
                    else
                    {
                        buildVersion = builderParameters.PipelineEnvironmentOptions.BuildNumber;
                    }

                    Log?.Info($"The build status is {builddata.Status} when generating the report.");

                    if (builddata.Status == BuildData.BuildStatus.completed &&
                        string.IsNullOrEmpty(builddata.Result) == false &&
                        builddata.Result.ToUpperInvariant() == "failed".ToUpperInvariant())
                    {
                        Log?.Info("Pipeline contains failed jobs or tasks");
                        isPipelineFailed = true;
                    }

                    if (string.IsNullOrEmpty(builderParameters.PipelineEnvironmentOptions.BuildRepositoryName))
                    {
                        Log?.Info($"Using build data repository as pipeline did not contain the data.");
                        builderParameters.PipelineEnvironmentOptions.BuildRepositoryName = builddata.RepositoryName;
                    }
                }
                else
                {
                    Log?.Info("Getting data for Integration test results");

                    releaseDetails = buildandReleaseReader
                        .GetReleaseResultAsync(builderParameters.PipelineEnvironmentOptions.ReleaseID)
                        .GetAwaiter().GetResult();
                    if (releaseDetails == null)
                    {
                        throw new TestResultReportingReleaseNotFoundException(ReleaseDataType.ReleaseDefinition, builderParameters.PipelineEnvironmentOptions.ReleaseDefinitionName);
                    }

                    pipelineVariables = releaseDetails.ReleaseVariables;

                    if (releaseDetails.Id != builderParameters.PipelineEnvironmentOptions.ReleaseID)
                    {
                        throw new TestResultReportingException("Mismatched Release ID found.");
                    }

                    buildoreleaseid = releaseDetails.Id;
                    releasename = releaseDetails.Name;
                    executionStageId = builderParameters.PipelineEnvironmentOptions.ReleaseStageID;
                    releaseDetails.CurrentAttempt = builderParameters.PipelineEnvironmentOptions.ReleaseAttempt;
                    testrundashboardlink = releaseDetails.GetTestRunLink(
                        builderParameters.PipelineEnvironmentOptions.SystemTeamFoundationCollectionURI,
                        builderParameters.PipelineEnvironmentOptions.SystemTeamProject);

                    testrunnameslist = releaseDetails.TestRunNames;
                    if (!testrunnameslist.Any())
                    {
                        throw new TestResultReportingException("No test runs found.");
                    }

                    releasetesttaskexecutiontime = releaseDetails.FirstJobExecutionDateTime;

                    Log?.Info($"Current Release execution stage id for which the report is being generated is : {executionStageId}");
                    Log?.Info($"Found the following test runs to generate report: {string.Join(",", testrunnameslist?.ToArray())}");

                    failedTaskName = releaseDetails.FailedTaskName;
                    if (!string.IsNullOrEmpty(failedTaskName))
                    {
                        Log?.Warn("This stage has failures before the test run task!");
                        tasksFailed = true;
                        testrundashboardlink = releaseDetails.GetTestRunLogLink(
                            builderParameters.PipelineEnvironmentOptions.SystemTeamFoundationCollectionURI,
                            builderParameters.PipelineEnvironmentOptions.SystemTeamProject);
                    }

                    builderParameters.IsPrivateRelease = releaseDetails.ReleaseType != JobType.Master;
                    if (string.IsNullOrEmpty(builderParameters.PipelineEnvironmentOptions.BuildNumber))
                    {
                        builderParameters.PipelineEnvironmentOptions.BuildNumber = releaseDetails.AssociatedBuildNumber;
                    }

                    if (string.IsNullOrEmpty(builderParameters.PipelineEnvironmentOptions.BuildRepositoryName))
                    {
                        builderParameters.PipelineEnvironmentOptions.BuildRepositoryName = releaseDetails.RepoName;
                    }

                    branchName = releaseDetails.BranchName;
                    executiontime = releaseDetails.CreatedOn;
                    requestedBy = releaseDetails.CreatedBy.EmailAddress;
                }

                if (builderParameters.IsPrivateRelease)
                {
                    if (string.IsNullOrEmpty(requestedBy))
                    {
                        throw new TestResultReportingException("Build Requested by is blank.");
                    }

                    builderParameters.SendTo = requestedBy;
                    Log?.Info($"This is a private release {requestedBy}");
                }

                if (!tasksFailed)
                { 
                    TestRunsCollection testruns = new TestRunsCollection(
                        buildandReleaseReader,
                        executiontime,
                        buildoreleaseid,
                        builderParameters.ResultSourceIsBuild);

                    if (!isPipelineFailed && testruns.Count == 0 && !tasksFailed)
                    {
                        throw new TestResultReportingNoResultsFoundException($"No runs were found for the current Release or Build Id: {buildoreleaseid} \r\n Link: {testrundashboardlink}");
                    }
                    else if (testruns.Count == 0 && tasksFailed)
                    {
                        isPipelineFailed = true;
                    }

                    if (builderParameters.ResultSourceIsBuild)
                    {
                        supportedRunsList = testruns.MatchedRunsByBuildIds(testrunnameslist);
                    }
                    else
                    {
                        supportedRunsList = testruns.MatchedRunsbyStageandExecution(executionStageId, releasetesttaskexecutiontime);
                    }

                    Log?.Info($"Found {supportedRunsList.Count} runs in the current stage.");
                    Log?.Info($"Found the following runs with ID to generate report: {string.Join(",", supportedRunsList?.Select(r => r.Id).Distinct().ToArray())}");

                    foreach (Run run in supportedRunsList)
                    {
                        var testRunResult = buildandReleaseReader.GetTestResultListAsync(run.Id).GetAwaiter().GetResult();

                        var testDataCollection = new TestResultDataCollection(testRunResult);

                        var datadriventests = testDataCollection.FindAll(r => r.ResultGroupType == ResultGroupTypeEnum.dataDriven);
                        if (datadriventests.Any())
                        {
                            var originalrunstatistic = run.RunStatistics[0];
                            datadriventests.ForEach(test =>
                            {
                                var testresultswithsubtestdata = buildandReleaseReader.GetTestResultWithLinksAsync(run.Id, test.Id).GetAwaiter().GetResult();

                                if (testresultswithsubtestdata.TestSubResults.Any())
                                {
                                    originalrunstatistic.count--;

                                    var subtestresultstatistic = new RunStatistic();
                                    subtestresultstatistic.count = testresultswithsubtestdata.TestSubResults.ToList().Where(r => r.Outcome == Apis.Common.OutcomeEnum.Passed).Count();
                                    subtestresultstatistic.outcome = Apis.Common.OutcomeEnum.Passed;

                                    run.RunStatistics.Add(subtestresultstatistic);

                                    subtestresultstatistic = new RunStatistic();
                                    subtestresultstatistic.count = testresultswithsubtestdata.TestSubResults.ToList().Where(r => r.Outcome == Apis.Common.OutcomeEnum.Failed).Count();
                                    subtestresultstatistic.outcome = Apis.Common.OutcomeEnum.Failed;

                                    run.RunStatistics.Add(subtestresultstatistic);
                                }
                            });
                        }
                        
                        List<TestResultData> failuresWithLinks = new List<TestResultData>();
                        List<TestResultData> testcaseResultsInterim = testDataCollection;

                        List<TestResultData> testRunCasesFailures = testcaseResultsInterim.FindAll(tcf => tcf.Outcome == Apis.Common.OutcomeEnum.Failed);
                        if (testRunCasesFailures.Any())
                        {
                            testRunContainsFailures = true;
                            foreach (TestResultData data in testRunCasesFailures)
                            {
                                failuresWithLinks.Add(buildandReleaseReader.GetTestResultWithLinksAsync(run.Id, data.Id).GetAwaiter().GetResult());
                            }
                        }

                        foreach (TestResultData data in failuresWithLinks)
                        {
                            if (data.AssociatedBugs != null)
                            {
                                foreach (AzureBugLinkData bugLink in data.AssociatedBugs)
                                {
                                    bugs.Add(buildandReleaseReader.GetBugDataAsync(bugLink).GetAwaiter().GetResult());
                                }
                            }
                        }

                        List<TestResultData> testRunCasesNonFailures = testcaseResultsInterim.FindAll(tcf => tcf.Outcome != Apis.Common.OutcomeEnum.Failed);

                        testResultData.AddRange(testRunCasesNonFailures);
                        testResultData.AddRange(failuresWithLinks);
                    }
                }
            }

            Log?.Info("Copying datamodel result parameters from startcollection.");
            MapperConfiguration mapperconfig = new MapperConfiguration(cnf =>
            {
                cnf.CreateMap<ReportBuilderParameters, DailyTestResultBuilderParameters>()
                    .ForMember(dest => dest.IsUnitTest, act => act.MapFrom(src => src.ResultSourceIsBuild));
            });

            var configMapper = new Mapper(mapperconfig);
            DailyTestResultBuilderParameters testResultBuilderParameters = configMapper.Map<DailyTestResultBuilderParameters>(builderParameters);

            if (pipelineVariables == null)
            {
                pipelineVariables = new Dictionary<string, string>();
            }

            pipelineVariables.Add("Requested By", requestedBy);
			pipelineVariables.Add("Build ID", builderParameters.PipelineEnvironmentOptions.BuildID);

            testResultBuilderParameters.ToolVersion = version;
            testResultBuilderParameters.TestResultsData = testResultData.OrderBy(r => r.TestClassName).ToList();
            testResultBuilderParameters.TestRunsList = supportedRunsList;
            testResultBuilderParameters.AzureReportLink = testrundashboardlink;            
            testResultBuilderParameters.IsPipelineFail = isPipelineFailed;
            testResultBuilderParameters.FailedTaskName = failedTaskName;
            testResultBuilderParameters.PipelineEnvironmentOptions = builderParameters.PipelineEnvironmentOptions;
            testResultBuilderParameters.ContainsFailures = testRunContainsFailures;
            testResultBuilderParameters.Bugs = bugs;
            testResultBuilderParameters.PipelineVariables = pipelineVariables;
            testResultBuilderParameters.ExecutionTime = executiontime;
            testResultBuilderParameters.ReleaseName = releasename;

            if (coverageAggregateColl != null && coverageAggregateColl.All.Count > 0)
            {
                Log?.Info("Adding code coverage aggregate data");
                testResultBuilderParameters.CodeCoverageData = coverageAggregateColl?.All.OrderBy(r => r.Name).ToList();
                testResultBuilderParameters.CodeCoverageFileURL = coverageAggregateColl.CodeCoverageURL;
            }

            Log?.Info("Creating HTML Datamodel");
            return new DailyHTMLReportBuilder(testResultBuilderParameters);
        }

        // Local function.
        internal BuildData GetBuildData(ReportBuilderParameters builderParameters)
        {
            Requires.NotNull(builderParameters, nameof(builderParameters));
            Requires.NotNullOrEmpty(builderParameters.PipelineEnvironmentOptions.BuildDefinitionID, nameof(builderParameters.PipelineEnvironmentOptions.BuildDefinitionID));

            DateTime maxDate = DateTime.Now.Date.AddDays(2);
            string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            BuildsCollection buildCollection = new BuildsCollection(
                this.buildandReleaseReader,
                builderParameters.PipelineEnvironmentOptions.BuildDefinitionID,
                builderParameters.PipelineEnvironmentOptions.ReleaseSourceBranchName,
                minDateString,
                maxDateString,
                includebuildswithunittestfailures: true,
                retry: true);
            BuildData buildData = buildCollection.GetBuildDataforBuild(builderParameters.PipelineEnvironmentOptions.BuildDefinitionID, builderParameters.PipelineEnvironmentOptions.BuildNumber);
            if (buildData == null)
            {
                throw new TestResultReportingException($"Build data for build id: {builderParameters.PipelineEnvironmentOptions.BuildNumber} was not found.");
            }

            return buildData;
        }
    }
}
