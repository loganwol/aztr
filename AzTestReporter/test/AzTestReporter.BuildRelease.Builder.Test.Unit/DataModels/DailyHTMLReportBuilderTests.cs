namespace AzTestReporter.BuildRelease.Builder.DataModels.Test.Unit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using global::AzTestReporter.BuildRelease.Apis;
    using global::AzTestReporter.BuildRelease.Builder;
    using Newtonsoft.Json;
    using NSubstitute;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class DailyHTMLReportBuilderTests
    {
        private DailyTestResultBuilderParameters builderParameters;
        private TestRunsCollection runs;
        private TestResultDataCollection testDataCollection;

        public DailyHTMLReportBuilderTests()
        {
            string responseBody = File.ReadAllText(@"TestData\\TestRun.json");
            AzureSuccessReponse runsResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            this.runs = new TestRunsCollection(runsResponse);

            responseBody = File.ReadAllText(@"TestData\\TestResult.json");
            AzureSuccessReponse resultAsr = JsonConvert.DeserializeObject<AzureSuccessReponse>(responseBody);
            this.testDataCollection = new TestResultDataCollection(resultAsr);

            this.builderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "Project",
                }
            };
        }

        [Fact]
        public void Verify_dailyhtmlreportbuilder_initialization_fails_with_null_value()
        {
            // Act
            Action act = () => new DailyHTMLReportBuilder(null);

            // Verify
            act.Should().Throw<ArgumentException>();
        }
    }
}
