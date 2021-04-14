namespace AzTestReporter.BuildRelease.Builder
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using AzTestReporter.BuildRelease.Builder.DataModels;
    using Newtonsoft.Json;
    using RazorEngine;
    using RazorEngine.Configuration;
    using RazorEngine.Templating;

    /// <summary>
    /// Implements summarizing the Test Result data into a HTML report.
    /// </summary>
    public class DailyHTMLReportBuilder
    {
        internal DailyTestResultBuilderParameters testResultBuilderParameters;
        internal DailyResultSummaryDataModel dailyResultSummaryDataModel;

        private string toolversion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyHTMLReportBuilder"/> class.
        /// Initializes the summary data model that does all the work to generate
        /// data that will be referenced by the HTML templates.
        /// </summary>
        /// <param name="testResultBuilderParameters">All the parameters needed to create the HTML.</param>
        public DailyHTMLReportBuilder(DailyTestResultBuilderParameters testResultBuilderParameters)
        {
            this.testResultBuilderParameters = testResultBuilderParameters;
            this.dailyResultSummaryDataModel = new DailyResultSummaryDataModel(testResultBuilderParameters);
        }

        /// <summary>
        /// Generates the Subject title of the HTML email that is sent out.
        /// </summary>
        public string Title
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (this.testResultBuilderParameters.IsUnitTest)
                {
                    stringBuilder.Append($"{this.dailyResultSummaryDataModel.RepoName} - Unit Test Results for {this.dailyResultSummaryDataModel.ReleaseType}");
                    stringBuilder.Append($": {this.dailyResultSummaryDataModel.BuildVersion}.");
                }
                else
                {
                    stringBuilder.Append($"{this.dailyResultSummaryDataModel.HeaderTitle} Test Results for ");
                    stringBuilder.Append($"({this.dailyResultSummaryDataModel.ReleaseName}).");
                }

                if (this.dailyResultSummaryDataModel.IsPrivateRun)
                {
                    stringBuilder.Insert(0, "(Private)");
                }

                return stringBuilder.ToString();
            }
        }

        public int PassRate
        {
            get
            {
                if (this.dailyResultSummaryDataModel.ResultSummary != null)
                {
                    return this.dailyResultSummaryDataModel.ResultSummary.PassRate;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the summary results detected the build the
        /// report was targeted to, to be generated, has failed.
        /// </summary>
        public bool IsPipelineFailed => this.testResultBuilderParameters != null && !string.IsNullOrEmpty(this.FailedTaskName) ? this.testResultBuilderParameters.IsPipelineFail: false;

        /// <summary>
        /// Gets a string containing the name of the task in the release that has failed.
        /// </summary>
        public string FailedTaskName
        {
            get
            {
                return this.testResultBuilderParameters.FailedTaskName;
            }
        }

        /// <summary>
        /// Defines specific checks to perform if the summary was generated
        /// successfully to generate the HTML.
        /// </summary>
        public void CheckResults()
        {
            if (this.dailyResultSummaryDataModel.ResultSummary == null)
            {
                throw new TestResultReportingException("Result Summary is null.");
            }

            if (this.dailyResultSummaryDataModel.TestClassResultsSummary.Count < 1)
            {
                throw new TestResultReportingException("There are no summary results.");
            }

            if (!(this.dailyResultSummaryDataModel.ResultSummary.PassRate >= 0))
            {
                throw new TestResultReportingException("Result Summary has a negative pass rate.");
            }
        }

        /// <summary>
        /// Generates the HTML summary through RazorEngine.
        /// </summary>
        /// <returns>The html representation of the summary or the failure details.</returns>
        public string ToHTML()
        {
            string templateKey = "templateEmailKey";
            string templatefilepath;
            if (!string.IsNullOrEmpty(this.testResultBuilderParameters.FailedTaskName))
            {
                templatefilepath = this.testResultBuilderParameters.FailedBuildTemplate;
                templateKey = "templateFailedkey";
            }
            else
            {
                templatefilepath = this.testResultBuilderParameters.MailTemplate;
            }

            if (!File.Exists(templatefilepath))
            {
                throw new FileNotFoundException($"Template file {this.testResultBuilderParameters.MailTemplate} was not found in directory MailTemplates.");
            }

            var config = new TemplateServiceConfiguration
            {
                TemplateManager = new ResolvePathTemplateManager(new[] { templateKey }),
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }),
            };

            Engine.Razor = RazorEngineService.Create(config);

            string htmlcontents = Engine.Razor.RunCompile(
                templatefilepath,
                typeof(DailyResultSummaryDataModel),
                this.dailyResultSummaryDataModel);

            return htmlcontents;
        }

        public string ToJson()
        {
            if (!string.IsNullOrEmpty(this.testResultBuilderParameters.FailedTaskName))
            {
                return string.Empty;
            }

            var json = JsonConvert.SerializeObject(this.dailyResultSummaryDataModel, Formatting.Indented);          
            return json;
        }
    }
}
