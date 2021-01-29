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
    
    public class HTMLGeneration_CodeCoverageTests
    {
        private TestRunsCollection runsCollection = null;
        private CodeCoverageModuleDataCollection codeCoverageAggregator;
        private DailyTestResultBuilderParameters builderParameters;

        public HTMLGeneration_CodeCoverageTests()
        {
            this.builderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "Project",
                },
            };

            string responseBody = File.ReadAllText(@"TestData\\TestRun.json");
            AzureSuccessReponse runsReponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            this.runsCollection = new TestRunsCollection(runsReponse);

            responseBody = File.ReadAllText(@"TestData\\codecoveragedataref.json");
            AzureSuccessReponse runResultSuccessReponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            this.codeCoverageAggregator = new CodeCoverageModuleDataCollection(AzureSuccessReponse.ConvertTo<CodeCoverageData>(runResultSuccessReponse));
        }

        [Fact]
        public void Do_not_add_a_tile_for_code_coverage_if_nothing_exists()
        {
            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            htmlDocument.GetElementbyId("codecoverage").Should().BeNull();
            htmlDocument.GetElementbyId("codecoveragesummaryheader").Should().BeNull();
            htmlDocument.GetElementbyId("summarycodecoverageheader").Should().BeNull();
        }

        [Fact]
        public void Codecoverage_summarytable_is_visible()
        {
            this.builderParameters.CodeCoverageData = this.codeCoverageAggregator.All;
            this.builderParameters.TestRunsList = this.runsCollection;

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            var element = htmlDocument.GetElementbyId("codecoveragesummaryheader");
            element.Should().NotBeNull();

            element = htmlDocument.GetElementbyId("codecoveragesummarytable");
            element.Should().NotBeNull();
        }

        [Fact]
        public void A_tile_is_created_for_code_coverage()
        {
            this.builderParameters.CodeCoverageData = this.codeCoverageAggregator.All;
            this.builderParameters.TestRunsList = this.runsCollection;

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);

            var element = htmlDocument.GetElementbyId("codecoverage");
            element.Should().NotBeNull();
            string text = element.InnerText.RemoveHTMLExtras();
            text.Should().Be("23%");
        }
    }
}
