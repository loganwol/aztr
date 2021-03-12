namespace AzTestReporter.BuildRelease.Builder.DataModels.Test.Unit
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using AzTestReporter.BuildRelease.Apis;
    using AzTestReporter.BuildRelease.Builder.DataModels;
    using Xunit;

    [ExcludeFromCodeCoverage]
    
    public class DailyResultSummaryDataModelTests
    {
        [Fact]
        public void Initialize_DailyResultSummaryDataModel_with_basic_buildparams()
        {
            // Arrange
            var dailyTestResultBuilderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "apn",
                }
            };

            // Act
            DailyResultSummaryDataModel resultSummaryDataModel = new DailyResultSummaryDataModel(dailyTestResultBuilderParameters);

            // Verify
            resultSummaryDataModel.Should().NotBeNull();
            resultSummaryDataModel.ReleaseType.Should().Be("Release");
            resultSummaryDataModel.BranchName.Should().Be(dailyTestResultBuilderParameters.PipelineEnvironmentOptions.ReleaseSourceBranchName);
            resultSummaryDataModel.ToolVersion.Should().Be(dailyTestResultBuilderParameters.ToolVersion);
            resultSummaryDataModel.HeaderTitle.Should().Be(" - ");
            resultSummaryDataModel.RepoName.Should().BeNullOrEmpty();
            resultSummaryDataModel.ReleaseLink.Should().BeNull();
            resultSummaryDataModel.LinktoDashboard.Should().BeNull();
            resultSummaryDataModel.ResultSummary.Should().BeNull();
            resultSummaryDataModel.TestRunLinks.Should().BeNull();
            resultSummaryDataModel.FailuresbyTestClass.Should().BeNull();
            resultSummaryDataModel.BuildVersion.Should().BeNull();
            resultSummaryDataModel.TestClassResultsSummary.Should().BeNull();
            resultSummaryDataModel.CodeCoverageAggregates.Should().BeNull();
        }

        [Fact]
        public void Verifying_the_testareasummary_list_is_alphabetically_ordered()
        {
            // Arrange
            var dailyTestResultBuilderParameters = new DailyTestResultBuilderParameters()
            {
                PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions()
                {
                    SystemTeamProject = "apn",
                }
            };

            var testresultdata = new List<TestResultData>();
            testresultdata.Add(new TestResultData()
            {
                AutomatedTestName = "Company.Feature2.subfeature.testclassname1.test1&23(23.@#*)",
                TestCaseName = "test1&23(23.@#*)",
                Build = new Build() { Name = "1.2.3.4" },
                Outcome = "ignore",
            });
            testresultdata.Add(new TestResultData()
            {
                AutomatedTestName = "Company.Feature1.subfeature.testclassname2.test2",
                TestCaseName = "test2",
                Build = new Build() { Name = "3.2.3.4" },
                Outcome = "ignore",
            });

            dailyTestResultBuilderParameters.TestResultsData = testresultdata;

            // Act
            DailyResultSummaryDataModel resultSummaryDataModel = new DailyResultSummaryDataModel(dailyTestResultBuilderParameters);

            resultSummaryDataModel.TestClassResultsSummary.Should().NotBeNull();
            resultSummaryDataModel.TestClassResultsSummary[0].TestClassName.Should().Be("testclassname1");
            resultSummaryDataModel.TestClassResultsSummary[1].TestClassName.Should().Be("testclassname2");


            resultSummaryDataModel.TestClassResultsSummary[0].TestNamespace.Should().Be("Company.Feature2.subfeature");
            resultSummaryDataModel.TestClassResultsSummary[1].TestNamespace.Should().Be("Company.Feature1.subfeature");
        }
    }
}
