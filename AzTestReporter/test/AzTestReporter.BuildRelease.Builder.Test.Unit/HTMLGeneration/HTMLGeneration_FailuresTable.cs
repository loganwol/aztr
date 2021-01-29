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
    
    public class HTMLGeneration_FailuresTable
    {
        private HtmlDocument htmlDocument = null;
        private DailyTestResultBuilderParameters builderParameters;

        public HTMLGeneration_FailuresTable()
        {
            this.builderParameters = new DailyTestResultBuilderParameters();
            this.builderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "Project",
                },
            };

            string responseBody = File.ReadAllText(@"TestData\\TestRun.json");
            AzureSuccessReponse runsResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            TestRunsCollection runs = new TestRunsCollection(runsResponse);

            responseBody = File.ReadAllText(@"TestData\\TestResult.json");
            AzureSuccessReponse testRunResultSuccessReponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            
            // Simulate failure.
            var testresults = AzureSuccessReponse.ConvertTo<TestResultData>(testRunResultSuccessReponse);
            testresults[1].Outcome = "Failed";

            responseBody = JsonConvert.SerializeObject(testresults);
            testRunResultSuccessReponse = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(responseBody);

            var testDataCollection = new TestResultDataCollection(testRunResultSuccessReponse);

            List<TestResultData> testResultData = testDataCollection;

            this.builderParameters.ContainsFailures = true;
            this.builderParameters.TestRunsList = runs;
            this.builderParameters.TestResultsData = testResultData;

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            this.htmlDocument = new HtmlDocument();
            this.htmlDocument.LoadHtml(emailhtml);
        }

        [Fact]
        public void Can_create_the_failuresbyfeaturearea_table_successfully()
        {
            var element = this.htmlDocument.GetElementbyId("failuresbytestclasstable");
            element.Should().NotBeNull();

            element = this.htmlDocument.GetElementbyId("failuresbytestclassrow");
            element.Should().NotBeNull();

            element = this.htmlDocument.GetElementbyId("failuresrow1");
            element.Should().NotBeNull();

            // Get the first cell. This is the important cell that sets the rowspan
            // grouped by the Rowcount property.
            this.htmlDocument.GetElementbyId("failuresrow1").ChildNodes.Count.Should().BeGreaterOrEqualTo(11);

            element = this.htmlDocument.GetElementbyId("failuresrow1").ChildNodes[1];
            element.Name.Should().Be("td");
            element.Attributes["rowspan"].Value.Should().Be("1");
        }

        [Fact]
        public void Generated_html_contains_duration_formatted_to_second_dot_milliseconds()
        {
            var element = this.htmlDocument.GetElementbyId("failuresrow1");

            element.ChildNodes[5].InnerText.RemoveHTMLExtras().Should().Be("0.0000");
        }

        [Fact]
        public void Generated_html_does_not_contain_the_failures_section_if_no_failures_are_found()
        {
            // Arrange
            this.builderParameters.TestResultsData = new List<TestResultData>();

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            
            // Act
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            emailhtml.Contains("failuresbytestclasstable").Should().BeFalse();
            emailhtml.Contains("failuresbytestclassrow").Should().BeFalse();
        }
    }
}
