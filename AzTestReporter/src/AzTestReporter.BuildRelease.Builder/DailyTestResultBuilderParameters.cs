namespace AzTestReporter.BuildRelease.Builder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using AzTestReporter.BuildRelease.Apis;

    /// <summary>
    /// Class for the Daily report builder parameters.
    /// </summary>
    public class DailyTestResultBuilderParameters
    {
        private string fullpath;

        public AzurePipelineEnvironmentOptions PipelineEnvironmentOptions { get; set; }

        public DailyTestResultBuilderParameters()
        {
            this.fullpath = AppDomain.CurrentDomain.BaseDirectory;

            if (string.IsNullOrEmpty(this.fullpath))
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null)
                {
                    assembly = Assembly.GetExecutingAssembly();
                }


                if (!assembly.Location.StartsWith(Path.GetTempPath()))
                {
                    this.fullpath = Path.GetDirectoryName(Path.GetFullPath(assembly.Location));
                }
            }
        }

        /// <summary>
        /// Gets the Header title.
        /// </summary>
        public string HeaderTitle
        {
            get
            {
                return this.IsUnitTest ? $"{this.PipelineEnvironmentOptions.BuildRepositoryName} Unit" : $"{this.PipelineEnvironmentOptions.BuildRepositoryName} this.ExecutionStageName";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this report is for a build.
        /// </summary>
        public bool IsUnitTest { get; set; }

        /// <summary>
        /// Gets or sets the version of the report tool.
        /// </summary>
        public string ToolVersion { get; set; }

        /// <summary>
        /// Gets or sets value indicating of the particular context of run has failures.
        /// </summary>
        public bool ContainsFailures { get; set; }

        /// <summary>
        /// Gets or sets the list of Test result data.
        /// </summary>
        public IReadOnlyList<TestResultData> TestResultsData { get; set; }

        /// <summary>
        /// Gets or sets the list of test runs.
        /// </summary>
        public IReadOnlyList<Run> TestRunsList { get; set; }

        /// <summary>
        /// Gets or sets the links to target release.
        /// </summary>
        public string AzureReportLink { get; set; }

        /// <summary>
        /// Gets or sets the code coverage data for the report.
        /// </summary>
        public List<CodeCoverageAggregateCollection> CodeCoverageData { get; set; }

        /// <summary>
        /// Gets or sets the url to the coverage file for the build.
        /// </summary>
        public string CodeCoverageFileURL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the target release is private.
        /// </summary>
        public bool IsPrivateRelease { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the target pipeline failed.
        /// </summary>
        public bool IsPipelineFail { get; set; } = false;

        /// <summary>
        /// Gets the mail template string.
        /// </summary>
        public string MailTemplate => string.IsNullOrEmpty(this.fullpath) ? 
            @"MailTemplates\DailyTestResultReportTemplate.cshtml" : 
            Path.Combine(this.fullpath, @"MailTemplates\DailyTestResultReportTemplate.cshtml");

        /// <summary>
        /// Gets the failed build template string.
        /// </summary>
        public string FailedBuildTemplate => string.IsNullOrEmpty(this.fullpath) ? @"MailTemplates\BuildFailureDetectedTemplate.cshtml" : Path.Combine(this.fullpath, @"MailTemplates\BuildFailureDetectedTemplate.cshtml");
        
        /// <summary>
        /// Gets or sets the failed task name string.
        /// </summary>
        public string FailedTaskName { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating what time the build used to run tests was completed.
        /// </summary>
        public DateTime BuildTime { get; set; } = DateTime.MinValue;
    }
}
