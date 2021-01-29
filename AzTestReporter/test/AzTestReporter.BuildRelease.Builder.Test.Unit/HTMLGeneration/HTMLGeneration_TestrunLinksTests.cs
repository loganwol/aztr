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
    
    public class HTMLGeneration_TestrunLinksTests
    {
        private List<TestResultData> testResultData = new List<TestResultData>();
        private DailyTestResultBuilderParameters builderParameters;

        public HTMLGeneration_TestrunLinksTests()
        {
            this.builderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "Project",
                },
            };

            string responseBody = File.ReadAllText(@"TestData\\TestResult.json");
            AzureSuccessReponse runResultSuccessReponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            var testDataCollection = new TestResultDataCollection(runResultSuccessReponse);

            this.testResultData.AddRange(testDataCollection);
        }

        [Fact]
        public void Can_successfully_generate_a_htmlreport_with_three_resultlinks_with_a_comma_added_in_between()
        {
            List<Run> runsList = new List<Run>();
            for (int i = 0; i < 3; i++)
            {
                runsList.Add(new Run(null)
                {
                    Name = $"Link {i}",
                    webAccessUrl = $"http://bing.com/link{i}",
                });
            }

            this.builderParameters.TestRunsList = runsList;

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);
            htmlDocument.Should().NotBeNull();

            for (int i = 0; i < 3; i++)
            {
                var element = htmlDocument.GetElementbyId($"testruntitlecell{i}");
                element.Should().NotBeNull();
                element.InnerText.RemoveHTMLExtras().Should().Be($"Link{i}");

                element = htmlDocument.GetElementbyId($"testrunlinkscell{i}");
                element.Attributes["href"].Should().NotBeNull();
                element.Attributes["href"].Value.Should().Be($"http://bing.com/link{i}");
            }
        }

        [Fact]
        public void Can_successfully_generate_a_htmlreport_with_twenty_resultlinks_with_a_comma_added_in_between()
        {
            List<Run> runsList = new List<Run>();
            for (int i = 0; i < 20; i++)
            {
                runsList.Add(new Run(null)
                {
                    Name = $"Link {i}",
                    webAccessUrl = $"http://bing.com/link{i}",
                });
            }

            this.builderParameters.TestRunsList = runsList;

            DailyHTMLReportBuilder dailyHTMLReportBuilder = new DailyHTMLReportBuilder(this.builderParameters);
            string emailhtml = dailyHTMLReportBuilder.ToHTML();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(emailhtml);
            htmlDocument.Should().NotBeNull();

            for (int i = 0; i < 20; i++)
            {

                var element = htmlDocument.GetElementbyId($"testruntitlecell{i}");
                element.Should().NotBeNull();
                element.InnerText.RemoveHTMLExtras().Should().Be($"Link{i}");

                element = htmlDocument.GetElementbyId($"testrunlinkscell{i}");
                element.Attributes["href"].Should().NotBeNull();
                element.Attributes["href"].Value.Should().Be($"http://bing.com/link{i}");
            }
        }
    }
}
