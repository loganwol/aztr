using AzTestReporter.BuildRelease.Apis;
using AzTestReporter.BuildRelease.Builder.DataModels;
using FluentAssertions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AzTestReporter.BuildRelease.Builder.Test.Unit.JSonGeneration
{
    [ExcludeFromCodeCoverage]
    public class BasicJsonGeneration
    {
        [Fact]
        public void Can_successfully_create_the_json_file_representing_the_summary_report()
        {
            // Arrange
            // This is a really long set of arranges as there are that many calls
            // internally to query for various things before
            // a report can be generated.
            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    BuildDefinitionName = "projectbuilddefinition",
                    BuildRepositoryName = "repository",
                    BuildID = "94",
                    SystemTeamProject = "fake",
                },

                ResultSourceIsBuild = true,
            };

            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Task<BuildData> buildData = Task.Run(() =>
            {
                var builddatajson = File.ReadAllText(@"TestData\\builddata.json");
                builddatajson = builddatajson.Replace(@": 62,", ": 94,");
                return JsonConvert.DeserializeObject<BuildData>(builddatajson);
            });

            azureReader.GetBuildData("94").Returns(buildData);

            Task<AzureSuccessReponse> coverageasr = Task.Run(() => AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\codecoveragedataref.json")));
            azureReader.GetTestBuildCoverageDataAsync("1", string.Empty).ReturnsForAnyArgs(coverageasr);

            string response = File.ReadAllText(@"TestData\\TestRun.json");
            response = response.Replace(": 59", ": 0");
            var testrunasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(response);

            var testruns = new TestRunsCollection(testrunasr);
            testruns.RemoveRange(1, testruns.Count() - 1);

            testrunasr = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(JsonConvert.SerializeObject(testruns));

            DateTime maxDate = DateTime.Now.Date.AddDays(2);
            string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "1", true).ReturnsForAnyArgs(testrunasr);

            Task<AzureSuccessReponse> testresultasr = Task.Run(() =>
            {
                var asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\TestResult.json"));
                var testresults = new TestResultDataCollection(asr);
                testresults[0].Outcome = "failed";

                asr = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(JsonConvert.SerializeObject(testresults));

                return asr;
            });

            azureReader.GetTestResultListAsync(246).ReturnsForAnyArgs(testresultasr);

            Task<TestResultData> testResultData = Task.Run(() => JsonConvert.DeserializeObject<TestResultData>(File.ReadAllText(@"TestData\\TestResultWithBugLink.json")));
            azureReader.GetTestResultWithLinksAsync(1, "2").ReturnsForAnyArgs(testResultData);

            var bugdata = JsonConvert.DeserializeObject<AzureBugData>(File.ReadAllText(@"TestData\bugdetails.json"));

            AzureBugLinkData azureBugLinkData = new AzureBugLinkData();
            azureBugLinkData.Id = "511492";
            azureReader.GetBugDataAsync(azureBugLinkData).ReturnsForAnyArgs(bugdata);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            dailyreport.ToJson().Should().BeTrue();

            File.Exists("94-TestResults.json").Should().BeTrue();

            var json = File.ReadAllText("94-TestResults.json");
            json.Should().NotBeNullOrEmpty();

            var jsonobject = JsonConvert.DeserializeObject(json);
            jsonobject.Should().NotBeNull();
        }
    }
}
