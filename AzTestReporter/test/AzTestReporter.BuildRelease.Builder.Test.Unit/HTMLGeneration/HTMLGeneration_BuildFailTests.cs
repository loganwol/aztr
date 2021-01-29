namespace AzTestReporter.BuildRelease.Builder.HTMLGeneration.Test.Unit
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using HtmlAgilityPack;
    using Newtonsoft.Json;
    using AzTestReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]
    
    public class HTMLGeneration_BuildFailTests
    {
        private DailyTestResultBuilderParameters resultBuilderParameters;

        public HTMLGeneration_BuildFailTests()
        {
            this.resultBuilderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "Project",
                },
            };
        }

        [Fact]
        public void Verify_report_selects_correct_template()
        {
            // Arrange
            this.resultBuilderParameters.IsUnitTest = true;
            this.resultBuilderParameters.ToolVersion = "1.0.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;

            // Act
            DailyHTMLReportBuilder failHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = failHTMLReportBuilder.ToHTML();

            // Verify
            var htmlFailDocument = new HtmlDocument();
            htmlFailDocument.LoadHtml(emailhtml);

            var targetFailAttribute = htmlFailDocument.GetElementbyId("failheadertable").Attributes[3];
            targetFailAttribute.Value.Should().Contain("#D7191C");
        }

        [Fact]
        public void Verify_releasedetail_is_added_togenerated_html_when_generating_html_for_build_results_with_buildversion_added_to_releasedetails()
        {
            // Arrange
            this.resultBuilderParameters.IsUnitTest = true;

            this.resultBuilderParameters.ToolVersion = "1.0.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;

            var testresultdata = new System.Collections.Generic.List<TestResultData>();
            testresultdata.Add(new TestResultData()
            {
                AutomatedTestName = "foo",
                Build = new Build() { Name = "1.2.3.4" },
                Outcome = "ignore",
            });

            this.resultBuilderParameters.TestResultsData = testresultdata;

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            htmlDocument.GetElementbyId("releasedetail").Should().NotBeNull();
            htmlDocument.GetElementbyId("releasedetail")?.InnerText.RemoveHTMLExtras().Should().Be("1.2.3.4");
        }

        [Fact]
        public void Verify_releasedetail_is_bvt_when_IsUnitTest_is_set_to_false()
        {
            // Arrange
            this.resultBuilderParameters.IsUnitTest = false;
            this.resultBuilderParameters.PipelineEnvironmentOptions.ReleaseName = "My release";
            this.resultBuilderParameters.ToolVersion = "1.2.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            htmlDocument.GetElementbyId("releasedetail").Should().NotBeNull();
            htmlDocument.GetElementbyId("releasedetail")?.InnerText.RemoveHTMLExtras().Should().Be("Myrelease");
        }

        [Fact]
        public void Verify_failed_task_string_is_populated()
        {
            // Arrange
            this.resultBuilderParameters.IsUnitTest = false;
            this.resultBuilderParameters.PipelineEnvironmentOptions.ReleaseName = "My release";
            this.resultBuilderParameters.ToolVersion = "1.2.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;
            this.resultBuilderParameters.FailedTaskName = "myFailedTask";

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            htmlDocument.GetElementbyId("failedtaskstring").Should().NotBeNull();
            htmlDocument.GetElementbyId("failedtaskstring")?.InnerText.RemoveHTMLExtras().Should().Be("myFailedTask");
        }

        [Fact]
        public void Verify_dashboardlink_is_blank_in_generated_html()
        {
            // Arrange
            this.resultBuilderParameters.IsUnitTest = true;
            this.resultBuilderParameters.ToolVersion = "1.2.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            var element = htmlDocument.GetElementbyId("dashboardlink");
            element.Should().NotBeNull();
            element.Name.Should().Be("a");
            element.InnerText.RemoveHTMLExtras().Should().Be("LogsLinkinAzureDevOps-Build:");
            element.Attributes["href"].Should().BeNull();
        }

        [Fact]
        public void Verify_dashboardlink_href_attribute_is_not_found_in_generated_html()
        {
            // Arrange
            this.resultBuilderParameters.IsUnitTest = true;
            this.resultBuilderParameters.ToolVersion = "1.2.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            var element = htmlDocument.GetElementbyId("dashboardlink");
            element.Attributes["href"].Should().BeNull();
        }

        [Fact]
        public void Verify_projectname_is_added_togenerated_html()
        {
            // Arrange
            this.resultBuilderParameters.IsUnitTest = true;
            this.resultBuilderParameters.ToolVersion = "1.2.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            htmlDocument.GetElementbyId("projectname").Should().NotBeNull();
            htmlDocument.GetElementbyId("projectname")?.InnerText.RemoveHTMLExtras().Should().Be("ProjectAutomatedtestexecutionreport");
        }

        [Fact]
        public void Verify_test_failure_table_is_generated_and_failures_are_grouped_as_desired_exists()
        {
            // Arrange
            this.resultBuilderParameters.ToolVersion = "1.0.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;

            string responseBody = File.ReadAllText(@"TestData\\TestRun.json");
            AzureSuccessReponse runsResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            TestRunsCollection runs = new TestRunsCollection(runsResponse);

            responseBody = File.ReadAllText(@"TestData\\TestResult.json");
            AzureSuccessReponse testRunResultSuccessReponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);

            var testresults = AzureSuccessReponse.ConvertTo<TestResultData>(testRunResultSuccessReponse);
            testresults[0].Outcome = "Failed";
            testresults[5].Outcome = "Failed";

            responseBody = JsonConvert.SerializeObject(testresults);
            testRunResultSuccessReponse = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(responseBody);

            var testDataCollection = new TestResultDataCollection(testRunResultSuccessReponse);

            List<TestResultData> testResultData = testDataCollection;

            this.resultBuilderParameters.ContainsFailures = true;
            this.resultBuilderParameters.TestRunsList = runs;
            this.resultBuilderParameters.TestResultsData = testResultData;

            // Act
            DailyHTMLReportBuilder failHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = failHTMLReportBuilder.ToHTML();

            // Verify
            var htmlFailDocument = new HtmlDocument();
            htmlFailDocument.LoadHtml(emailhtml);

            var element = htmlFailDocument.GetElementbyId("failuresbytestclasstable");
            element.Should().NotBeNull();

            element = htmlFailDocument.GetElementbyId("failuresbytestclassrow");
            element.Should().NotBeNull();

            element = htmlFailDocument.GetElementbyId("failuresrow1");
            element.Should().NotBeNull();

            // Get the first cell. This is the important cell that sets the rowspan
            // grouped by the Rowcount property. 
            htmlFailDocument.GetElementbyId("failuresrow1").ChildNodes.Count.Should().BeGreaterOrEqualTo(11);

            element = htmlFailDocument.GetElementbyId("failuresrow1").ChildNodes[1];
            element.Name.Should().Be("td");
            element.Attributes["rowspan"].Value.Should().Be("2");

            // For all subsequent rows for a test area collection, the first cell
            // is removed. Hence rows 2 & 8 should not have the rowspan set on them.
            element = htmlFailDocument.GetElementbyId("failuresrow2").ChildNodes[1];
            element.Name.Should().Be("td");
            element.Attributes["rowspan"].Should().BeNull();

            element = htmlFailDocument.GetElementbyId("passrate");
            element.Should().BeNull();
        }

        [Fact]
        public void Verify_test_failure_table_does_not_exist()
        {
            // Arrange
            this.resultBuilderParameters.ToolVersion = "1.0.3.4";
            this.resultBuilderParameters.IsPipelineFail = true;

            // Act
            DailyHTMLReportBuilder failHTMLReportBuilder = new DailyHTMLReportBuilder(this.resultBuilderParameters);
            string emailhtml = failHTMLReportBuilder.ToHTML();

            // Verify
            var htmlFailDocument = new HtmlDocument();
            htmlFailDocument.LoadHtml(emailhtml);

            var element = htmlFailDocument.GetElementbyId("failuresbytestclasstable");
            element.Should().BeNull();
        }
    }
}
