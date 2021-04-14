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
    public class HTMLGeneration_TitleTests
    {
        DailyTestResultBuilderParameters builderParameters;

        public HTMLGeneration_TitleTests()
        {
            string responseBody = File.ReadAllText(@"TestData\\TestRun.json");
            AzureSuccessReponse runsResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            TestRunsCollection runs = new TestRunsCollection(runsResponse);

            this.builderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "Project",
                    //ReleaseName = "My release",
                }
            };            
        }

        [Fact]
        public void Verify_releasedetail_is_added_togenerated_html_when_generating_html_for_build_results_with_buildversion_added_to_releasedetails()
        {
            // Arrange
            this.builderParameters.IsUnitTest = true;
            this.builderParameters.ToolVersion = "1.0.3.4";

            var testdata = new List<TestResultData>();
            testdata.Add(new TestResultData()
            {
                AutomatedTestName = "Company.Feature2.subfeature.foo.test1",
                TestCaseName = "foo",
                Build = new Build() { Name = "1.2.3.4" },
                Outcome = Apis.Common.OutcomeEnum.Ignore,
            });

            this.builderParameters.TestResultsData = testdata;

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            htmlDocument.GetElementbyId("releasedetail").Should().NotBeNull();
            htmlDocument.GetElementbyId("releasedetail")?.InnerText.RemoveHTMLExtras().Should().Be("1.2.3.4");
        }

        [Fact]
        public void Verify_releasedetail_is_bvt_when_resultsourceisbuild_is_set_to_false()
        {
            // Arrange
            this.builderParameters.IsUnitTest = false;
            this.builderParameters.ToolVersion = "1.2.3.4";
            this.builderParameters.ReleaseName = "Myrelease";

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            htmlDocument.GetElementbyId("releasedetail").Should().NotBeNull();
            htmlDocument.GetElementbyId("releasedetail")?.InnerText.RemoveHTMLExtras().Should().Be("Myrelease");
        }

        [Fact]
        public void Verify_dashboardlink_is_blank_in_generated_html()
        {
            // Arrange
            this.builderParameters.IsUnitTest = true;
            this.builderParameters.ToolVersion = "1.2.3.4";

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            var element = htmlDocument.GetElementbyId("dashboardlink");
            element.Should().NotBeNull();
            element.Name.Should().Be("a");
            element.InnerText.RemoveHTMLExtras().Should().Be("TestResultsLinkinAzureDevOps-Build:");
            element.Attributes["href"].Should().BeNull();
        }

        [Fact]
        public void Verify_dashboardlink_href_attribute_is_not_found_in_generated_html()
        {
            // Arrange
            this.builderParameters.IsUnitTest = true;
            this.builderParameters.ToolVersion = "1.2.3.4";

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
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
            this.builderParameters.IsUnitTest = true;
            this.builderParameters.ToolVersion = "1.2.3.4";

            // Act
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            // Verify
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            htmlDocument.GetElementbyId("projectname").Should().NotBeNull();
            htmlDocument.GetElementbyId("projectname")?.InnerText.RemoveHTMLExtras().Should().Be("ProjectAutomatedtestexecutionreport");
        }
    }
}
