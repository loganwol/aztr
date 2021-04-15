namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AutoMapper;
    using AzTestReporter.BuildRelease.Apis;
    using Validation;

    /// <summary>
    /// Class for the Daily results data model.
    /// </summary>
    public class DailyResultSummaryDataModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DailyResultSummaryDataModel"/> class.
        /// </summary>
        /// <param name="testResultBuilderParameters"><see cref="DailyTestResultBuilderParameters"/>.</param>
        public DailyResultSummaryDataModel(DailyTestResultBuilderParameters testResultBuilderParameters)
        {
            Requires.NotNull(testResultBuilderParameters, nameof(testResultBuilderParameters));
            Requires.NotNull(testResultBuilderParameters.PipelineEnvironmentOptions, nameof(testResultBuilderParameters.PipelineEnvironmentOptions));

            MapperConfiguration mapperconfig = new MapperConfiguration(cnf =>
                cnf.CreateMap<DailyTestResultBuilderParameters, DailyResultSummaryDataModel>()
                            .ForMember(dest => dest.AzureProjectName, act => act.MapFrom(src => src.PipelineEnvironmentOptions.SystemTeamProject))
                            .ForMember(dest => dest.RepoName, act => act.MapFrom(src => src.PipelineEnvironmentOptions.BuildRepositoryName))
                            .ForMember(dest => dest.BranchName, act => act.MapFrom(src => src.PipelineEnvironmentOptions.ReleaseSourceBranchName))
                            .ForMember(dest => dest.ReleaseName, act => act.MapFrom(src => src.ReleaseName))
                            .ForMember(dest => dest.ToolVersion, act => act.MapFrom(src => src.ToolVersion))
                            .ForMember(dest => dest.IsPrivateRun, act => act.MapFrom(src => src.IsPrivateRelease))
                            .ForMember(dest => dest.FailedTaskName, act => act.MapFrom(src => src.FailedTaskName))
                            .ForMember(dest => dest.CodeCoverageFileURL, act => act.MapFrom(src => src.CodeCoverageFileURL))
                            .ForMember(dest => dest.LinktoDashboard, act => act.MapFrom(src => src.AzureReportLink))
                            .ForMember(dest => dest.PipelineVariables, act => act.MapFrom(src => src.PipelineVariables)));
            var configMapper = new Mapper(mapperconfig);

            configMapper.Map(testResultBuilderParameters, this);

            this.ReleaseType = testResultBuilderParameters.IsUnitTest ? "Build" : "Release";
            this.ExecutionDate = testResultBuilderParameters.ExecutionTime.ToString("G");

            this.HeaderTitle = this.RepoName?.Trim();

            if (!testResultBuilderParameters.IsUnitTest)
            {
                this.HeaderTitle += $" - {testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseExecutionStage?.Trim()}";

                if (testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseAttempt > 1)
                {
                    this.HeaderTitle += $" [Attempt - {testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseAttempt}] ";
                }
            }

            if (testResultBuilderParameters.TestRunsList != null)
            {
                this.ResultSummary = new ResultSummaryDataModel(testResultBuilderParameters.TestRunsList, this.ShowSummarizedSubResults);
                if (testResultBuilderParameters.CodeCoverageData == null)
                {
                    if (this.ShowSummarizedSubResults)
                    {
                        this.ResultSummary.SubResultsSummaryDataModel.CodeCoverage = -1;
                    }
                    else
                    {
                        this.ResultSummary.OverallResultSummaryDataModel.CodeCoverage = -1;
                    }
                }

                this.TestRunLinks = new TestRunNameLinksCollectionDataModel(testResultBuilderParameters.TestRunsList);
            }

            if (testResultBuilderParameters.TestResultsData != null)
            {
                if (testResultBuilderParameters.ContainsFailures)
                {
                    StringBuilder resultsrooturl = new StringBuilder(testResultBuilderParameters.PipelineEnvironmentOptions.SystemTeamFoundationCollectionURI);
                    resultsrooturl.Append($"{testResultBuilderParameters.PipelineEnvironmentOptions.SystemTeamProject}/");
                    if (testResultBuilderParameters.IsUnitTest)
                    {
                        resultsrooturl.Append($"_build/results?buildId={testResultBuilderParameters.PipelineEnvironmentOptions.BuildID}&view=ms.vss-test-web.build-test-results-tab");
                    }
                    else
                    {
                        resultsrooturl.Append($"_releaseProgress?_a=release-environment-extension");
                        resultsrooturl.Append("&releaseId=");
                        resultsrooturl.Append(testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseID);
                        resultsrooturl.Append("&environmentId=");
                        resultsrooturl.Append(testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseStageID);
                        resultsrooturl.Append($"&extensionId=ms.vss-test-web.test-result-in-release-environment-editor-tab");
                    }

                    this.FailuresbyTestClass = new FailuresbyTestClassCollectionDataModel(
                            resultsrooturl.ToString(),
                            testResultBuilderParameters.TestResultsData,
                            testResultBuilderParameters.ShowSummarizedSubResults);
                }

                this.BuildVersion = testResultBuilderParameters.TestResultsData?.Select(x => x.Build.Name).FirstOrDefault();

                // Use the same object in the HTML to generate the summary table.
                this.TestClassResultsSummary = new TestAreaResultsSummaryCollectionDataModel(
                    testResultBuilderParameters.TestResultsData, testResultBuilderParameters.ShowSummarizedSubResults);
            }

            if (testResultBuilderParameters.CodeCoverageData != null)
            {
                this.CodeCoverageAggregates = testResultBuilderParameters.CodeCoverageData.OrderBy(r => r.Name).ToList();
                if (this.ResultSummary != null)
                {
                    double totalblocks = testResultBuilderParameters.CodeCoverageData.Sum(r => r.TotalBlocks);
                    double coveredblocks = testResultBuilderParameters.CodeCoverageData.Sum(r => r.NumberofCoveredBlocks);
                    double notcoveredblocks = testResultBuilderParameters.CodeCoverageData.Sum(r => r.NumberofNotCoveredBlocks);

                    this.TotalCodeCoverageBlocks = (int)totalblocks;
                    this.TotalCoveredCodeCoverageBlocks = (int)coveredblocks;

                    if (testResultBuilderParameters.ShowSummarizedSubResults)
                    {
                        this.ResultSummary.SubResultsSummaryDataModel.CodeCoverage = (int)((coveredblocks / totalblocks) * 100);
                    }
                    else
                    {
                        this.ResultSummary.OverallResultSummaryDataModel.CodeCoverage = (int)((coveredblocks / totalblocks) * 100);
                    }
                }
            }

            if (testResultBuilderParameters.Bugs != null)
            {
                this.Bugs = testResultBuilderParameters.Bugs.ToList();
            }
        }

        /// <summary>
        /// gets or sets the Title of the header for the result report.
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Gets the Name of the target release.
        /// </summary>
        public object ReleaseName { get; internal set; }

        /// <summary>
        /// gets the release type (private or master).
        /// </summary>
        public string ReleaseType { get; internal set; }

        /// <summary>
        /// gets, private sets the version of the tool used to create the report.
        /// </summary>
        public string ToolVersion { get; private set; }

        /// <summary>
        /// gets or sets the name of the branch used for the target release.
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// gets or sets the name of the source repo used for the target release.
        /// </summary>
        public string RepoName { get; set; }

        /// <summary>
        /// gets or sets the link to the dashboard to be added to the report.
        /// </summary>
        public string LinktoDashboard { get; set; }

        /// <summary>
        /// gets or sets the build version of the build used to create the target release.
        /// </summary>
        public string BuildVersion { get; set; }

        /// <summary>
        /// gets or sets the link to the release used for the report.
        /// </summary>
        public string ReleaseLink { get; set; }

        /// <summary>
        /// gets the name of the project containing the target release pipeline.
        /// </summary>
        public string AzureProjectName { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the tool is running against a private run.
        /// </summary>
        public bool IsPrivateRun { get; internal set; }

        /// <summary>
        /// gets or sets an instance of <see cref="ResultSummaryDataModel"/>.
        /// </summary>
        public ResultSummaryDataModel ResultSummary { get; set; }

        /// <summary>
        /// Gets instance of <see cref="FailuresbyTestClassCollectionDataModel"/>.
        /// </summary>
        public FailuresbyTestClassCollectionDataModel FailuresbyTestClass { get; internal set; }

        /// <summary>
        /// gets or sets an instance of <see cref="TestRunNameLinksCollectionDataModel"/>.
        /// </summary>
        public TestRunNameLinksCollectionDataModel TestRunLinks { get; set; }

        /// <summary>
        /// gets or sets an instance of <see cref="TestAreaResultsSummaryCollectionDataModel"/>.
        /// </summary>
        public TestAreaResultsSummaryCollectionDataModel TestClassResultsSummary { get; set; }

        /// <summary>
        /// gets or sets an instance List of Type <see cref="CodeCoverageAggregate"/>.
        /// </summary>
        public List<CodeCoverageAggregateCollection> CodeCoverageAggregates { get; set; }

        /// <summary>
        /// Gets the .coverage file url for the build.
        /// </summary>
        public string CodeCoverageFileURL { get; internal set; }

        /// <summary>
        /// Gets or Sets a value indicating the name of a failed task.
        /// </summary>
        public string FailedTaskName { get; set; }
        
        public int TotalCodeCoverageBlocks { get; }
        
        public int TotalCoveredCodeCoverageBlocks { get; }

        /// <summary>
        /// gets or sets an instance List of Type <see cref="AzureBugData"/>.
        /// </summary>
        public List<AzureBugData> Bugs { get; set; }

        public Dictionary<string, string> PipelineVariables { get; set; }

        /// <summary>
        /// Gets or sets the date the build or release was executed.
        /// </summary>
        public string ExecutionDate { get; set; }

        public bool ShowSummarizedSubResults { get; set; }
    }
}
