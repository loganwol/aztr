namespace AzTestReporter.BuildRelease.Builder.Test.Unit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzTestReporter.BuildRelease.Apis;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using AzTestReporter.BuildRelease.Builder.HTMLGeneration.Test.Unit;
    using FluentAssertions;
    using HtmlAgilityPack;
    using Newtonsoft.Json;
    using NSubstitute;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class ReleasePath_ReportBuilderTests
    {
        private static DateTime maxDate = DateTime.Now.Date.AddDays(2);
        private static string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
        private static string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);

        [Fact]
        public void Can_successfully_create_a_daily_report_for_releasedefinition_based_execution_path()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    ReleaseDefinitionName = "MSTest.Repeat",
                    //ReleaseName = "MSTest.Repeat 1.1.27- Release 6",
                    ReleaseExecutionStage = "Integration test Execution",
                    BuildRepositoryName = "loganwol/mstestrepeat",
                    ReleaseAttempt = 1,
                    ReleaseID = "59",
                    ReleaseStageID = 59,
                    SystemHostType = "release",
                },
                ResultSourceIsBuild = false,
            };

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));

            // Set to automated release execution.
            release.Reason = "Schedule";
            var alltasks = release.Environments[0].DeploySteps[0].ReleaseDeployPhases[0].DeploymentJobs[0].Tasks;
            alltasks.ForEach(r =>
            {
                r.Status = "succeeded";
            });

            azureReader.GetReleaseResultAsync("59").ReturnsForAnyArgs(release);

            // Remove all other instances
            AzureSuccessReponse testrunasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\testrun.json"));
            var testruns = AzureSuccessReponse.ConvertTo<Run>(testrunasr);
            testruns.RemoveRange(0, 6);

            testrunasr = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(JsonConvert.SerializeObject(testruns));

            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "59", true).ReturnsForAnyArgs(testrunasr);

            Task<AzureSuccessReponse> testresultasr = Task.Run(() =>
                AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\TestResult.json")));
            azureReader.GetTestResultListAsync(246).ReturnsForAnyArgs(testresultasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            reportBuilderParameters.IsPrivateRelease.Should().BeFalse();

            dailyreport.Should().NotBeNull();
            dailyreport.Title.Should().NotBeNullOrEmpty();
            dailyreport.Title.Should().StartWith(reportBuilderParameters.PipelineEnvironmentOptions.BuildRepositoryName)
                .And.Contain(reportBuilderParameters.PipelineEnvironmentOptions.ReleaseExecutionStage)
                .And.NotContain("(Private)");

            dailyreport.dailyResultSummaryDataModel.ResultSummary.Should().NotBeNull();
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.Total.Should().Be(16);
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.Failed.Should().Be(0);
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.NotExecuted.Should().Be(0);
            dailyreport.dailyResultSummaryDataModel.TestClassResultsSummary.Should().NotBeNull()
                .And.HaveCount(1);

            dailyreport.ToHTML().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Can_successfully_create_a_daily_report_for_releasedefinition_based_execution_path_with_multiple_attempts()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            var pipelinevars = new AzurePipelineEnvironmentOptions();

            pipelinevars.ReleaseDefinitionName = "MSTest.Repeat";
            //pipelinevars.ReleaseName = "MSTest.Repeat 1.1.27- Release 6";
            pipelinevars.ReleaseExecutionStage = "Integration test Execution";
            pipelinevars.BuildRepositoryName = "loganwol/mstestrepeat";
            pipelinevars.ReleaseAttempt = 7;
            pipelinevars.ReleaseID = "59";
            pipelinevars.ReleaseStageID = 59;
            pipelinevars.SystemHostType = "release";

            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters();
            reportBuilderParameters.PipelineEnvironmentOptions = pipelinevars;
            reportBuilderParameters.ResultSourceIsBuild = false;

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));

            // Set to automated release execution.
            release.Reason = "Schedule";

            azureReader.GetReleaseResultAsync("59").ReturnsForAnyArgs(release);


            Task<AzureSuccessReponse> testrunasr = Task.Run(() =>
                AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\testrun.json")));
            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "59", true).ReturnsForAnyArgs(testrunasr);

            Task<AzureSuccessReponse> testresultasr = Task.Run(() =>
                AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\TestResult.json")));
            azureReader.GetTestResultListAsync(246).ReturnsForAnyArgs(testresultasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            reportBuilderParameters.IsPrivateRelease.Should().BeFalse();

            dailyreport.Should().NotBeNull();
            dailyreport.Title.Should().NotBeNullOrEmpty();
            dailyreport.Title.Should().StartWith(pipelinevars.BuildRepositoryName)
                .And.Contain(pipelinevars.ReleaseExecutionStage)
                .And.Contain($"[Attempt - {pipelinevars.ReleaseAttempt}]")
                //.And.Contain($"({pipelinevars.ReleaseName})")
                .And.NotContain("(Private)");

            dailyreport.ToHTML().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Can_successfully_create_a_daily_report_for_releasedefinition_based_execution_path_for_privateruns()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            var pipelinevars = new AzurePipelineEnvironmentOptions();

            pipelinevars.ReleaseDefinitionName = "MSTest.Repeat";
            //pipelinevars.ReleaseName = "MSTest.Repeat 1.1.27- Release 6";
            pipelinevars.ReleaseExecutionStage = "Integration test Execution";
            pipelinevars.BuildRepositoryName = "loganwol/mstestrepeat";
            pipelinevars.ReleaseAttempt = 7;
            pipelinevars.ReleaseID = "59";
            pipelinevars.ReleaseStageID = 59;
            pipelinevars.SystemHostType = "release";

            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters();
            reportBuilderParameters.PipelineEnvironmentOptions = pipelinevars;
            reportBuilderParameters.ResultSourceIsBuild = false;

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));
            azureReader.GetReleaseResultAsync("59").ReturnsForAnyArgs(release);

            Task<AzureSuccessReponse> testrunasr = Task.Run(() =>
                AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\testrun.json")));
            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "59", true).ReturnsForAnyArgs(testrunasr);

            Task<AzureSuccessReponse> testresultasr = Task.Run(() =>
                AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\TestResult.json")));
            azureReader.GetTestResultListAsync(246).ReturnsForAnyArgs(testresultasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            reportBuilderParameters.IsPrivateRelease.Should().BeTrue();

            dailyreport.Should().NotBeNull();
            dailyreport.Title.Should().NotBeNullOrEmpty();
            dailyreport.Title.Should().StartWith("(Private)")
                .And.Contain(pipelinevars.BuildRepositoryName)
                .And.Contain(pipelinevars.ReleaseExecutionStage)
                .And.Contain($"[Attempt - {pipelinevars.ReleaseAttempt}]");
            //.And.Contain($"({pipelinevars.ReleaseName})");

            dailyreport.ToHTML().Should().NotBeNullOrEmpty();

            dailyreport.dailyResultSummaryDataModel.ResultSummary.Should().NotBeNull();
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.Total.Should().Be(16);
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.Failed.Should().Be(0);
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.NotExecuted.Should().Be(0);
            dailyreport.dailyResultSummaryDataModel.TestClassResultsSummary.Should().NotBeNull()
                .And.HaveCount(1);

            dailyreport.dailyResultSummaryDataModel.TestClassResultsSummary[0].TestClassName.Should().Be("MSTestRepeatTestMethodAttributeIntegrationTest");
        }

        [Fact]
        public void Can_correctly_not_find_any_testruns_with_attempts_is_mismatched()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            var pipelinevars = new AzurePipelineEnvironmentOptions();

            pipelinevars.ReleaseDefinitionName = "MSTest.Repeat";
            //pipelinevars.ReleaseName = "MSTest.Repeat 1.1.27- Release 6";
            pipelinevars.ReleaseExecutionStage = "Integration test Execution";
            pipelinevars.BuildRepositoryName = "loganwol/mstestrepeat";
            pipelinevars.ReleaseAttempt = 1;
            pipelinevars.ReleaseID = "59";
            pipelinevars.ReleaseStageID = 59;
            pipelinevars.SystemHostType = "release";

            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters();
            reportBuilderParameters.PipelineEnvironmentOptions = pipelinevars;
            reportBuilderParameters.ResultSourceIsBuild = false;

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));

            // Set to automated release execution.
            release.Reason = "Schedule";
            release.Environments[0].DeploySteps[0].Attempt = 2;
            var alltasks = release.Environments[0].DeploySteps[0].ReleaseDeployPhases[0].DeploymentJobs[0].Tasks;
            alltasks.ForEach(r =>
            {
                r.Status = "succeeded";
            });

            azureReader.GetReleaseResultAsync("59").ReturnsForAnyArgs(release);

            // Remove all other instances
            AzureSuccessReponse testrunasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\testrun.json"));
            var testruns = AzureSuccessReponse.ConvertTo<Run>(testrunasr);
            testruns.RemoveRange(0, 6);

            testrunasr = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(JsonConvert.SerializeObject(testruns));

            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "59", true).ReturnsForAnyArgs(testrunasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            Action act = () => _ = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            act.Should().ThrowExactly<TestResultReportingException>();
        }

        [Fact]
        public void When_no_tests_runs_are_found_throw_exception()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            var pipelinevars = new AzurePipelineEnvironmentOptions();

            pipelinevars.ReleaseDefinitionName = "MSTest.Repeat";
            //pipelinevars.ReleaseName = "MSTest.Repeat 1.1.27- Release 6";
            pipelinevars.ReleaseExecutionStage = "Integration test Execution";
            pipelinevars.BuildRepositoryName = "loganwol/mstestrepeat";
            pipelinevars.ReleaseAttempt = 1;
            pipelinevars.ReleaseID = "59";
            pipelinevars.ReleaseStageID = 59;
            pipelinevars.SystemHostType = "release";

            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters();
            reportBuilderParameters.PipelineEnvironmentOptions = pipelinevars;
            reportBuilderParameters.ResultSourceIsBuild = false;

            string releasejson = File.ReadAllText(@"TestData\\release.json");
            releasejson = releasejson.Replace("VSTest", "Test");

            Release release = JsonConvert.DeserializeObject<Release>(releasejson);

            // Set to automated release execution.
            release.Reason = "Schedule";

            azureReader.GetReleaseResultAsync("59").ReturnsForAnyArgs(release);


            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            Action act = () => _ = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            act.Should().ThrowExactly<TestResultReportingException>().WithMessage("No test runs found.");
        }

        [Fact]
        public void Can_successfully_find_a_failedtask_before_test_task_and_report_a_failed()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

			var releasename = "MSTest.Repeat 1.1.27- Release 6";
            var pipelinevars = new AzurePipelineEnvironmentOptions();

            pipelinevars.ReleaseDefinitionName = "MSTest.Repeat";            
            pipelinevars.ReleaseExecutionStage = "Integration test Execution";
            pipelinevars.BuildRepositoryName = "loganwol/mstestrepeat";
            pipelinevars.ReleaseAttempt = 1;
            pipelinevars.ReleaseID = "59";
            pipelinevars.ReleaseStageID = 59;
            pipelinevars.SystemTeamFoundationCollectionURI = "https://dev.azure.com/ProjectCollection";
            pipelinevars.SystemTeamProject = "Project";
            pipelinevars.SystemHostType = "release";

            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters();
            reportBuilderParameters.PipelineEnvironmentOptions = pipelinevars;
            reportBuilderParameters.ResultSourceIsBuild = false;

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));

            // Set to automated release execution.
            release.Reason = "Schedule";
            release.Environments[0].DeploySteps[0].ReleaseDeployPhases[0].DeploymentJobs[0].Tasks[2].Status = "failed";

            azureReader.GetReleaseResultAsync("59").ReturnsForAnyArgs(release);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify

            dailyreport.Should().NotBeNull();
            dailyreport.Title.Should().NotBeNullOrEmpty();
            dailyreport.Title.Should().StartWith(pipelinevars.BuildRepositoryName)
                .And.Contain(pipelinevars.ReleaseExecutionStage)
                .And.Contain($"({releasename})")
                .And.NotContain("(Private)");

            dailyreport.dailyResultSummaryDataModel.ResultSummary.Should().BeNull();

            var html = dailyreport.ToHTML();
            html.Should().NotBeNullOrEmpty();

            var htmldoc = new HtmlAgilityPack.HtmlDocument();
            htmldoc.LoadHtml(html);

            var expectedresultslink = string.Concat(
                    pipelinevars.SystemTeamFoundationCollectionURI,
                    "/",
                    pipelinevars.SystemTeamProject,
                    "/",
                    "_releaseProgress?releaseId=",
                    pipelinevars.ReleaseID,
                    "&amp;environmentId=",
                    pipelinevars.ReleaseStageID,
                    "&amp;_a=release-environment-logs");

            var resultslink = htmldoc.GetElementbyId("dashboardlink");
            resultslink.Attributes[1].Value.Should().Be(expectedresultslink);

            var failedtaskname = htmldoc.GetElementbyId("failedtaskstring");
            failedtaskname.InnerText.RemoveHTMLExtras().Should().Be("PowerShellScript");
        }


        [Fact]
        public void Can_successfully_ignore_a_task_that_failed_after_the_testtask()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

			var pipelinevars = new AzurePipelineEnvironmentOptions();
            pipelinevars.ReleaseDefinitionName = "MSTest.Repeat";
            pipelinevars.ReleaseExecutionStage = "Integration test Execution";
            pipelinevars.BuildRepositoryName = "loganwol/mstestrepeat";
            pipelinevars.ReleaseAttempt = 1;
            pipelinevars.ReleaseID = "59";
            pipelinevars.ReleaseStageID = 59;
            pipelinevars.SystemTeamFoundationCollectionURI = "https://dev.azure.com/ProjectCollection";
            pipelinevars.SystemTeamProject = "Project";
            pipelinevars.SystemHostType = "release";

            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters();
            reportBuilderParameters.PipelineEnvironmentOptions = pipelinevars;
            reportBuilderParameters.ResultSourceIsBuild = false;

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));

            // Set to automated release execution.
            release.Reason = "Schedule";

            azureReader.GetReleaseResultAsync("59").ReturnsForAnyArgs(release);

            // Remove all other instances
            AzureSuccessReponse testrunasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\testrun.json"));
            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "59", true).ReturnsForAnyArgs(testrunasr);

            Task<AzureSuccessReponse> testresultasr = Task.Run(() =>
                AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\TestResult.json")));
            azureReader.GetTestResultListAsync(246).ReturnsForAnyArgs(testresultasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify

            dailyreport.Should().NotBeNull();
            dailyreport.FailedTaskName.Should().BeNullOrEmpty();
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Should().NotBeNull();

            var html = dailyreport.ToHTML();
            html.Should().NotBeNullOrEmpty();

            var htmldoc = new HtmlAgilityPack.HtmlDocument();
            htmldoc.LoadHtml(html);

            var expectedresultslink = string.Concat(
                    pipelinevars.SystemTeamFoundationCollectionURI,
                    "/",
                    pipelinevars.SystemTeamProject,
                    "/",
                    "_releaseProgress?releaseId=",
                    pipelinevars.ReleaseID,
                    "&amp;environmentId=",
                    pipelinevars.ReleaseStageID,
                    "&amp;_a=release-environment-logs");

            var resultslink = htmldoc.GetElementbyId("dashboardlink");
            resultslink.InnerText.RemoveHTMLExtras().Should().Be("TestResultsLinkinAzureDevOps-Build:1.1.27");

            htmldoc.GetElementbyId("failedtaskstring").Should().BeNull();
        }

        [Fact]
        public void Can_successfully_create_a_daily_report_for_releasedefinition_with_multiple_deploysteps()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            var pipelinevars = new AzurePipelineEnvironmentOptions();

            pipelinevars.ReleaseDefinitionName = "MSTest.Repeat";
            //pipelinevars.ReleaseName = "MSTest.Repeat 1.1.27- Release 11";
            pipelinevars.ReleaseExecutionStage = "Integration test Execution";
            pipelinevars.BuildRepositoryName = "loganwol/mstestrepeat";
            pipelinevars.ReleaseAttempt = 3;
            pipelinevars.ReleaseID = "72";
            pipelinevars.ReleaseStageID = 72;
            pipelinevars.SystemHostType = "release";

            ReportBuilderParameters reportBuilderParameters = new ReportBuilderParameters();
            reportBuilderParameters.PipelineEnvironmentOptions = pipelinevars;
            reportBuilderParameters.ResultSourceIsBuild = false;

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release-multideploysteps.json"));

            // Set to automated release execution.
            release.Reason = "Schedule";

            azureReader.GetReleaseResultAsync("72").ReturnsForAnyArgs(release);

            // Remove all other instances
            AzureSuccessReponse testrunasr = AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\testrun-multideploy.json"));
            azureReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, "59", true).ReturnsForAnyArgs(testrunasr);

            Task<AzureSuccessReponse> testresultasr = Task.Run(() =>
                AzureSuccessReponse.ConverttoAzureSuccessResponse(File.ReadAllText(@"TestData\\TestResult.json")));
            azureReader.GetTestResultListAsync(294).ReturnsForAnyArgs(testresultasr);
            azureReader.GetTestResultListAsync(296).ReturnsForAnyArgs(testresultasr);
            azureReader.GetTestResultListAsync(298).ReturnsForAnyArgs(testresultasr);

            int temp = 0;
            var client = Substitute.For<HttpClient>();
            client.When(r => r.DefaultRequestHeaders.Add("foo", "bar")).Do(r => temp++);

            azureReader.When(r => r.SetupandAuthenticate(client, string.Empty)).Do(r => temp++);

            // Act
            var dailyreport = new ReportBuilder(azureReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            // Verify
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Should().NotBeNull();
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.Total.Should().Be(20);
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.Failed.Should().Be(0);
            dailyreport.dailyResultSummaryDataModel.ResultSummary.Summary.NotExecuted.Should().Be(0);
            dailyreport.dailyResultSummaryDataModel.TestClassResultsSummary.Should().NotBeNull()
                .And.HaveCount(1);

            dailyreport.dailyResultSummaryDataModel.TestRunLinks.Should().HaveCount(1);
            dailyreport.ToHTML().Should().NotBeNullOrEmpty();
        }

    }
}
