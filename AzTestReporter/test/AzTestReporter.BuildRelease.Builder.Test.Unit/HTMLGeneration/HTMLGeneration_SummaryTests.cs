namespace AzTestReporter.BuildRelease.Builder.HTMLGeneration.Test.Unit
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using FluentAssertions;
    using HtmlAgilityPack;
    using Newtonsoft.Json;
    using AzTestReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]
    
    public class HTMLGeneration_SummaryTests
    {
        private HtmlDocument htmlDocument;
        private DailyHTMLReportBuilder dailyHTMLReportBuilder;
        private DailyTestResultBuilderParameters builderParameters;

        public HTMLGeneration_SummaryTests()
        {
            string responseBody = File.ReadAllText(@"TestData\\TestRun.json");
            AzureSuccessReponse runsResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);

            TestRunsCollection runs = new TestRunsCollection(runsResponse);

            runs.RemoveRange(0, 6);
            runs[0].RunStatistics[0].count = 12;

            runs[0].RunStatistics.Add(new RunStatistic()
            {
                count = 1,
                outcome = "notexecuted",
                state = "unexpected",
            });

            runs[0].RunStatistics.Add(new RunStatistic()
            {
                count = 3,
                outcome = "failed",
                state = "unexpected",
            });

            this.builderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "Project",
                },

                ContainsFailures = true,
                TestRunsList = runs,
            };

            this.dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = this.dailyHTMLReportBuilder.ToHTML();

            this.htmlDocument = new HtmlDocument();
            this.htmlDocument.LoadHtml(emailhtml);
        }

        [Fact]
        public void TestsSummary_header_should_startwith_testarea_when_reporting_by_subsystem_is_set_to_True()
        {
            var summarytableheader = this.htmlDocument.GetElementbyId("testsummarytableheader");
            summarytableheader.Should().NotBeNull();
            summarytableheader.InnerText.ToLowerInvariant().RemoveHTMLExtras().Should().StartWith("testclasssummary");
        }

        [Fact]
        public void TestsSummary_table_header_should_startwith_testareas_when_reporting_by_subsystem_is_set_to_True()
        {
            var summarytableheader = this.htmlDocument.GetElementbyId("testsummarytablecolheader");
            summarytableheader.Should().NotBeNull();
            summarytableheader.InnerText.ToLowerInvariant().RemoveHTMLExtras().Should().StartWith("testclass");
        }

        [Fact]
        public void Verify_passrate_value_is_expected_value_in_generated_html()
        {
            var element = this.htmlDocument.GetElementbyId("passrate");
            element.Should().NotBeNull();

            element.InnerText.RemoveHTMLExtras().Should().Be("75");
            this.dailyHTMLReportBuilder.PassRate.Should().Be(75);
        }

        [Fact]
        public void Verify_totaltests_value_is_expected_value_in_generated_html()
        {
            var element = this.htmlDocument.GetElementbyId("totaltests");
            element.Should().NotBeNull();

            element.InnerText.RemoveHTMLExtras().Should().Be("16");
        }

        [Fact]
        public void Verify_passedtests_value_is_expected_value_in_generated_html()
        {
            var element = this.htmlDocument.GetElementbyId("passedtests");
            element.Should().NotBeNull();

            element.InnerText.RemoveHTMLExtras().Should().Be("12");
        }

        [Fact]
        public void Verify_notexecutedtests_value_is_expected_value_in_generated_html()
        {
            var element = this.htmlDocument.GetElementbyId("notexecutedtests");
            element.Should().NotBeNull();
            element.InnerText.RemoveHTMLExtras().Should().Be("1");

            element = this.htmlDocument.GetElementbyId("notexecutedtestslink");
            element.Should().BeNull();
        }

        [Fact]
        public void Verify_failedtests_value_is_expected_value_in_generated_html()
        {
            var element = this.htmlDocument.GetElementbyId("failedtests");
            element.Should().NotBeNull();

            element = this.htmlDocument.GetElementbyId("failedtestslink");
            element.Should().NotBeNull();
            element.InnerText.RemoveHTMLExtras().Should().Be("3");
        }
    }
}
