namespace AzTestReporter.BuildRelease.Builder.Test.Unit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using HtmlAgilityPack;
    using Newtonsoft.Json;
    using NSubstitute;
    using AzTestReporter.BuildRelease.Apis;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using Xunit;

    [ExcludeFromCodeCoverage]
    
    public class BuildPath_ReportBuilderTests
    {
        [Fact]
        public void ReportBuilder_Initialization_throws()
        {
            // Arrange & Act
            Action act = () => _ = new ReportBuilder(null);

            // Verify
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetBuildData_with_null_buildparams_pipelineargs_throws()
        {
            // Arrange
            var reportbuilder = new ReportBuilder(Substitute.For<IBuildandReleaseReader>());
            var reportBuilderParams = new ReportBuilderParameters();

            // Act
            Action act = () => reportbuilder.GetReleasesRunsandResults(ref reportBuilderParams);

            // Verify
            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*PipelineEnvironmentOptions*");
        }

        [Fact]
        public void GetBuildData_with_null_builddefid_throws()
        {
            // Arrange
            var reportbuilder = new ReportBuilder(Substitute.For<IBuildandReleaseReader>());
            var reportBuilderParams = new ReportBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions(),
            };

            // Act
            Action act = () => reportbuilder.GetBuildData(reportBuilderParams);

            // Verify
            act.Should().ThrowExactly<ArgumentNullException>().WithMessage("*BuildDefinitionID*");
        }

            [Fact]
        public void Can_successfully_create_a_daily_report_for_build_based_execution_path()
        {
            // Arrange
            // This is a really long set of arranges as there are that many calls
            // internally to query for various things before
            // a report can be generated.
            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters()
            {
                ResultSourceIsBuild = true,
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    BuildDefinitionName = "projectbuilddefinition",
                    BuildRepositoryName = "repository",
                    BuildDefinitionID = "1",
                    SystemTeamProject = "fake",
                },
            };

            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Task<AzureSuccessReponse> buildsasr =
                Task.Run(() =>
                {
                    string response = File.ReadAllText(@"TestData\\builds.json");
                    return AzureSuccessReponse.ConverttoAzureSuccessResponse(response);
                });

            azureReader.GetBuildsbyDefinitionIdAsync("1").ReturnsForAnyArgs(buildsasr);

            Task<AzureSuccessReponse> coverageasr = Task.Run(() => AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\codecoveragedataref.json")));
            azureReader.GetTestBuildCoverageDataAsync("1", string.Empty).ReturnsForAnyArgs(coverageasr);

            string response = File.ReadAllText(@"TestData\\testrun.json");
            var testrunasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(response);

            // Need to set the Build ID explicitly to match the latest build in the builds.json.
            var testruns = new TestRunsCollection(testrunasr);
            testruns[0].BuildConfiguration.id = 38;

            testrunasr = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(JsonConvert.SerializeObject(testruns));

            DateTime maxDate = DateTime.Now.Date.AddDays(2);
            string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "1", true).ReturnsForAnyArgs(testrunasr);

            Task<AzureSuccessReponse> testresultasr = Task.Run(() => AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\TestResult.json")));
            azureReader.GetTestResultListAsync(106).ReturnsForAnyArgs(testresultasr);

            Task<TestResultData> testResultData = Task.Run(() => JsonConvert.DeserializeObject<TestResultData>(File.ReadAllText(@"TestData\\TestResultWithBugLink.json")));
            azureReader.GetTestResultWithLinksAsync(1, "2").ReturnsForAnyArgs(testResultData);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            dailyreport.Should().NotBeNull();
            dailyreport.Title.Should().NotBeNullOrEmpty();
            dailyreport.Title.Should().StartWith("repository").And.Contain("Unit");

            dailyreport.dailyResultSummaryDataModel.ResultSummary.Should().NotBeNull();
            dailyreport.dailyResultSummaryDataModel.TestClassResultsSummary.Should().NotBeNull();
            dailyreport.dailyResultSummaryDataModel.FailuresbyTestClass.Should().BeNull();
            dailyreport.dailyResultSummaryDataModel.CodeCoverageAggregates.Should().NotBeEmpty();
            var html = dailyreport.ToHTML();
            html.Should().NotBeNullOrEmpty();

            reportBuilderParameters.IsPrivateRelease.Should().BeFalse();
        }

        
        [Fact]
        public void Can_successfully_create_a_daily_report_for_build_based_execution_path_privatebuilds()
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
                    BuildDefinitionID = "1",
                    SystemTeamProject = "fake",
                },

                ResultSourceIsBuild = true,
            };

            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            AzureSuccessReponse buildsasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\builds.json"));

            // Make the source branch private.
            var builds = new BuildsCollection(buildsasr);
            builds[0].SourceBranch = "personal/loganwol/change1";

            buildsasr = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(JsonConvert.SerializeObject(builds));

            azureReader.GetBuildsbyDefinitionIdAsync("1").ReturnsForAnyArgs(buildsasr);

            Task<AzureSuccessReponse> coverageasr = Task.Run(() => AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\codecoveragedataref.json")));
            azureReader.GetTestBuildCoverageDataAsync("1", string.Empty).ReturnsForAnyArgs(coverageasr);

            string response = File.ReadAllText(@"TestData\\testrun.json");
            var testrunasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(response);

            // Need to set the Build ID explicitly to match the latest build in the builds.json.
            var testruns = new TestRunsCollection(testrunasr);
            testruns[0].BuildConfiguration.id = 38;

            testrunasr = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(JsonConvert.SerializeObject(testruns));

            DateTime maxDate = DateTime.Now.Date.AddDays(2);
            string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "1", true).ReturnsForAnyArgs(testrunasr);

            Task<AzureSuccessReponse> testresultasr = Task.Run(() => AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\TestResult.json")));
            azureReader.GetTestResultListAsync(106).ReturnsForAnyArgs(testresultasr);

            Task<TestResultData> testResultData = Task.Run(() => JsonConvert.DeserializeObject<TestResultData>(File.ReadAllText(@"TestData\\TestResultWithBugLink.json")));
            azureReader.GetTestResultWithLinksAsync(1, "2").ReturnsForAnyArgs(testResultData);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            dailyreport.Title.Should().StartWith("(Private)").And.Contain("repository").And.Contain("Unit");

            dailyreport.dailyResultSummaryDataModel.ResultSummary.Should().NotBeNull();
            dailyreport.dailyResultSummaryDataModel.TestClassResultsSummary.Should().NotBeNull();
            dailyreport.dailyResultSummaryDataModel.FailuresbyTestClass.Should().BeNull();
            dailyreport.dailyResultSummaryDataModel.CodeCoverageAggregates.Should().NotBeEmpty();
            var html = dailyreport.ToHTML();
            html.Should().NotBeNullOrEmpty();

            reportBuilderParameters.IsPrivateRelease.Should().BeTrue();
        }

        
        [Fact]
        public void Can_pick_up_a_specific_build_number()
        {
            // Arrange
            // This is a really long set of arranges as there are that many calls
            // internally to query for various things before
            // a report can be generated.
            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters()
            {
                ResultSourceIsBuild = true,
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    BuildDefinitionName = "projectbuilddefinition",
                    BuildRepositoryName = "repository",
                    BuildDefinitionID = "1",
                    SystemTeamProject = "fake",
                    BuildNumber = "20210115.5",
                },
            };

            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Task<AzureSuccessReponse> buildsasr =
                Task.Run(() =>
                {
                    string response = File.ReadAllText(@"TestData\\builds.json");
                    return AzureSuccessReponse.ConverttoAzureSuccessResponse(response);
                });

            azureReader.GetBuildsbyDefinitionIdAsync("1").ReturnsForAnyArgs(buildsasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var buildData = new ReportBuilder(azureReader).GetBuildData(reportBuilderParameters);

            // Verify
            buildData.Should().NotBeNull();
            buildData.BuildNumber.Should().Be(reportBuilderParameters.PipelineEnvironmentOptions.BuildNumber);
            buildData.FinishTime.Should().NotBeNull();
            buildData.ExecutionDateTime.Year.ToString(CultureInfo.InvariantCulture).Should().Be("2021");
            buildData.ExecutionDateTime.Day.ToString(CultureInfo.InvariantCulture).Should().Be("15");
        }

        [Fact]
        public void When_Build_number_is_not_found_getbuilddata_throws()
        {
            // Arrange
            // This is a really long set of arranges as there are that many calls
            // internally to query for various things before
            // a report can be generated.
            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters()
            {
                ResultSourceIsBuild = true,
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    BuildDefinitionName = "projectbuilddefinition",
                    BuildRepositoryName = "repository",
                    BuildDefinitionID = "1",
                    SystemTeamProject = "fake",
                    BuildNumber = "1.1.5",
                },
            };

            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Task<AzureSuccessReponse> buildsasr =
                Task.Run(() =>
                {
                    string response = File.ReadAllText(@"TestData\\builds.json");
                    return AzureSuccessReponse.ConverttoAzureSuccessResponse(response);
                });

            azureReader.GetBuildsbyDefinitionIdAsync("1").ReturnsForAnyArgs(buildsasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // ACT
            Action act = () => new ReportBuilder(azureReader).GetBuildData(reportBuilderParameters);

            // Verify
            act.Should().Throw<TestResultReportingException>();
        }

        [Fact]
        public void When_requestedby_is_empty_throw_an_exception_for_private_builds()
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
                    BuildDefinitionID = "1",
                    SystemTeamProject = "fake",
                },

                ResultSourceIsBuild = true,
            };

            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            AzureSuccessReponse buildsasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\builds.json"));

            // Make the source branch private.
            var builds = new BuildsCollection(buildsasr);
            builds[0].SourceBranch = "personal/loganwol/change1";
            builds[0].RequestedFor = null;

            buildsasr = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(JsonConvert.SerializeObject(builds));

            azureReader.GetBuildsbyDefinitionIdAsync("1").ReturnsForAnyArgs(buildsasr);

            Task<AzureSuccessReponse> coverageasr = Task.Run(() => AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\codecoveragedataref.json")));
            azureReader.GetTestBuildCoverageDataAsync("1", string.Empty).ReturnsForAnyArgs(coverageasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            Action act = () => _ = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            act.Should().ThrowExactly<TestResultReportingSendToNullException>();
        }
    }
}
