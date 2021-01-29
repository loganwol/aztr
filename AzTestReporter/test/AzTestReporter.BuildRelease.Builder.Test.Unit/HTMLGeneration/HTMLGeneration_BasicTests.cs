namespace AzTestReporter.BuildRelease.Builder.HTMLGeneration.Test.Unit
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using FluentAssertions;
    using HtmlAgilityPack;
    using Newtonsoft.Json;
    using AzTestReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class HTMLGeneration_BasicTests
    {
        private TestRunsCollection runsCollection = null;
        private List<TestResultData> testResultData = new List<TestResultData>();
        private CodeCoverageModuleDataCollection codeCoverageAggregator;
        private DailyTestResultBuilderParameters builderParameters = null;

        public HTMLGeneration_BasicTests()
        {
            string responseBody = File.ReadAllText(@"TestData\TestRun.json");
            AzureSuccessReponse runs = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            this.runsCollection = new TestRunsCollection(runs);

            responseBody = File.ReadAllText(@"TestData\TestResult.json");
            AzureSuccessReponse runResultSuccessReponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            var testDataCollection = new TestResultDataCollection(runResultSuccessReponse);

            this.testResultData = testDataCollection;

            responseBody = File.ReadAllText(@"TestData\TestResultWithBugLink.json");
            TestResultData resultWithBug = JsonConvert.DeserializeObject<TestResultData>(responseBody);

            this.testResultData.Add(resultWithBug);

            responseBody = File.ReadAllText(@"TestData\codecoveragedataref.json");
            runResultSuccessReponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            this.codeCoverageAggregator = new CodeCoverageModuleDataCollection(AzureSuccessReponse.ConvertTo<CodeCoverageData>(runResultSuccessReponse));

            this.builderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "Project",
                },
            };
        }

        [Fact]
        public void Can_successfully_generate_a_basic_htmlreport_with_only_the_headers_for_builds()
        {
            this.builderParameters.IsUnitTest = true;

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);
            htmlDocument.Should().NotBeNull();
        }

        [Fact]
        public void Can_successfully_generate_a_basic_htmlreport_with_only_the_headers_for_releasess()
        {
            // Arrange
            this.builderParameters.IsUnitTest = false;
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);

            // Act
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);
            htmlDocument.Should().NotBeNull();
        }

        [Fact]
        public void Can_successfully_generate_a_full_htmlreport_summary_and_failures()
        {
            this.builderParameters.TestResultsData = this.testResultData;
            this.builderParameters.TestRunsList = this.runsCollection;
            this.builderParameters.CodeCoverageData = this.codeCoverageAggregator.All;
            this.builderParameters.CodeCoverageFileURL = string.Empty;
            this.builderParameters.FailedTaskName = "fa";

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);
            htmlDocument.Should().NotBeNull();
        }

        [Fact]
        public void Can_successfully_generate_a_htmlreport_with_tools_version()
        {
            builderParameters.ToolVersion = "1.2.3.4";

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);
            htmlDocument.Should().NotBeNull();

            var element = htmlDocument.GetElementbyId("toolsversion");
            element.Should().NotBeNull();

            element.InnerText.RemoveHTMLExtras().Should().Be("CreatedwithAzureTestReporterVersion1.2.3.4");
        }

        [Fact]
        public void Can_successfully_generate_a_htmlreport_with_codecoveragedata()
        {
            this.builderParameters.CodeCoverageData = this.codeCoverageAggregator.All;
            this.builderParameters.TestRunsList = this.runsCollection;

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);
            htmlDocument.Should().NotBeNull();

            var element = htmlDocument.GetElementbyId("missingcodecoveragesummaryheader");
            element.Should().BeNull();
        }

        [Fact]
        public void Can_successfully_generate_a_htmlreport_with_testrunlinks()
        {
            List<Run> runsList = new List<Run>();
            runsList.Add(new Run(null)
            {
                Name = "Foo",
                webAccessUrl = "http://bing.com",
            });

            this.builderParameters.TestRunsList = runsList;

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);
            htmlDocument.Should().NotBeNull();

            var element = htmlDocument.GetElementbyId("testruntitlecell0");
            element.InnerText.RemoveHTMLExtras().Should().Be("Foo");
            element = htmlDocument.GetElementbyId("testrunlinkscell0");
            element.Attributes["href"].Should().NotBeNull();
            element.Attributes["href"].Value.Should().Be("http://bing.com");
        }
    }
}
