namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System.Collections.Generic;
    using System.Linq;
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

            this.AzureProjectName = testResultBuilderParameters.PipelineEnvironmentOptions.SystemTeamProject;
            this.RepoName = testResultBuilderParameters.PipelineEnvironmentOptions.BuildRepositoryName;
            this.ReleaseName = testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseName;
            this.BranchName = testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseSourceBranchName;
            this.ReleaseType = testResultBuilderParameters.IsUnitTest ? "Build" : "Release";
            this.ToolVersion = testResultBuilderParameters.ToolVersion;
            this.IsPrivateRun = testResultBuilderParameters.IsPrivateRelease;
            this.FailedTaskName = testResultBuilderParameters.FailedTaskName;
            this.CodeCoverageFileURL = testResultBuilderParameters.CodeCoverageFileURL;

            this.HeaderTitle = this.RepoName?.Trim();

            if (!testResultBuilderParameters.IsUnitTest)
            {
                this.HeaderTitle += $" - {testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseExecutionStage?.Trim()}";

                if (testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseAttempt > 1)
                {
                    this.HeaderTitle += $" [Attempt - {testResultBuilderParameters.PipelineEnvironmentOptions.ReleaseAttempt}] ";
                }
            }

            this.LinktoDashboard = testResultBuilderParameters.AzureReportLink;

            if (testResultBuilderParameters.TestRunsList != null)
            {
                this.ResultSummary = new ResultSummaryDataModel(testResultBuilderParameters.TestRunsList);
                if (testResultBuilderParameters.CodeCoverageData == null)
                {
                    this.ResultSummary.CodeCoverage = -1;
                }

                this.TestRunLinks = new TestRunNameLinksCollectionDataModel(testResultBuilderParameters.TestRunsList);
            }

            if (testResultBuilderParameters.TestResultsData != null)
            {
                if (testResultBuilderParameters.ContainsFailures)
                {
                    this.FailuresbyTestClass = new FailuresbyTestClassCollectionDataModel(
                            testResultBuilderParameters.PipelineEnvironmentOptions.SystemTeamProject,
                            testResultBuilderParameters.TestResultsData);
                }

                this.BuildVersion = testResultBuilderParameters.TestResultsData?.Select(x => x.Build.Name).FirstOrDefault();

                // Use the same object in the HTML to generate the summary table.
                this.TestClassResultsSummary = new TestAreaResultsSummaryCollectionDataModel(
                    testResultBuilderParameters.TestResultsData);
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

                    this.ResultSummary.CodeCoverage = (int)((coveredblocks / totalblocks) * 100);
                }
            }
        }

        /// <summary>
        /// gets or sets the Title of the header for the result report.
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Gets the Name of the target release.
        /// </summary>
        public object ReleaseName { get; }

        /// <summary>
        /// gets the release type (private or master).
        /// </summary>
        public string ReleaseType { get; }

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
        public string AzureProjectName { get; }

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

        /// <summary>
        /// Gets or sets a value indicating whether if the current results span multiple repos.
        /// </summary>
        public bool MultipleRepoResults { get; set; }
        
        public int TotalCodeCoverageBlocks { get; }
        
        public int TotalCoveredCodeCoverageBlocks { get; }
    }
}
