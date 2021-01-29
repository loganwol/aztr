namespace AzTestReporter.BuildRelease.Apis.Test.Unit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NSubstitute;
    using AzTestReporter.BuildRelease.Apis;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class BuildsCollectionsTests
    {
        [Fact]
        public void Initializing_BuildsCollection_with_null_reader_throws()
        {
            // Act
            Action act = () => new BuildsCollection(null, null);

            // Verify
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Initializing_BuildsCollection_with_null_definitionid_throws()
        {
            // Arrange
            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();

            // Act
            Action act = () => new BuildsCollection(reader, null);

            // Verify
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_success_response_does_not_have_any_values_throw_reporting_exception()
        {
            string builddefinitionsresponse = "{ 'count': 0, 'value' : [] }";
            AzureSuccessReponse successResponse = AzureSuccessReponse.ConverttoAzureSuccessResponse(builddefinitionsresponse);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync(string.Empty).ReturnsForAnyArgs(successResponse);

            // Act
            Action act = () => new BuildsCollection(reader, "10301");

            // Verify
            act.Should().Throw<TestResultReportingException>($"Details for 10301 could not be found.");
        }

        [Fact]
        public void When_success_response_does_not_have_any_values_retry_has_some_intialize_buildcollections_Successfully()
        {
            string blankbuilddefinitionsresponse = "{ 'count': 0, 'value' : [] }";
            AzureSuccessReponse blanksuccessResponse = AzureSuccessReponse.ConverttoAzureSuccessResponse(blankbuilddefinitionsresponse);
            string retrybuilddefinitionsresponse = File.ReadAllText(@"TestData\builds.json");
            var retrysuccessResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(retrybuilddefinitionsresponse);

            DateTime maxDate = DateTime.Now.Date.AddDays(2);
            string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync("11825", "ref/heads/master", minDateString, maxDateString, 20, true).Returns(blanksuccessResponse);
            reader.GetBuildsbyDefinitionIdAsync("11825", "ref/heads/master", null, null, 20, true).Returns(retrysuccessResponse);

            // Act
            var builds = new BuildsCollection(reader, "11825", "ref/heads/master", minDateString, maxDateString, 20, true, true);

            // Verify
            builds.Should().NotBeNull();
        }

        [Fact]
        public void When_success_response_does_not_have_any_values_retry_also_does_not_expect_a_failure()
        {
            string blankbuilddefinitionsresponse = "{ 'count': 0, 'value' : [] }";
            AzureSuccessReponse blanksuccessResponse = AzureSuccessReponse.ConverttoAzureSuccessResponse(blankbuilddefinitionsresponse);

            DateTime maxDate = DateTime.Now.Date.AddDays(2);
            string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync("11825", "ref/heads/master", minDateString, maxDateString, 20, true).Returns(blanksuccessResponse);
            reader.GetBuildsbyDefinitionIdAsync("11825", "ref/heads/master", null, null, 20, true).Returns(blanksuccessResponse);

            // Act
            Action act = () => _ = new BuildsCollection(reader, "11825", "ref/heads/master", minDateString, maxDateString, 20, true, true);

            // Verify
            act.Should().Throw<TestResultReportingException>().WithMessage("Details for 11825 could not be found.");
        }

        [Fact]
        public void GetBuilddataForBuild_throws_for_null_buildnumber()
        {
            string builddefinitionsresponse = File.ReadAllText(@"TestData\builds.json");
            var successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(builddefinitionsresponse);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync(string.Empty).ReturnsForAnyArgs(successResponse);

            var buildscollection = new BuildsCollection(reader, "10301");

            // Act
            Action act = () => buildscollection.GetBuildDataforBuild(string.Empty);

            // Verify
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Can_get_the_desired_build_data_basedon_buildnumber()
        {
            string builddefinitionsresponse = File.ReadAllText(@"TestData\builds.json");
            var successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(builddefinitionsresponse);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync(string.Empty).ReturnsForAnyArgs(successResponse);

            var buildscollection = new BuildsCollection(reader, "1");

            // Act
            var builddata = buildscollection.GetBuildDataforBuild("1", "20210115.5");

            // Verify
            builddata.Should().NotBeNull();
            builddata.BuildId.Should().Be("27");
        }

        [Fact]
        public void When_no_buildnumber_is_input_to_GetBuildDataforBuild_the_latest_buildis_returned()
        {
            string builddefinitionsresponse = File.ReadAllText(@"TestData\builds.json");
            var successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(builddefinitionsresponse);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync(string.Empty).ReturnsForAnyArgs(successResponse);

            var buildscollection = new BuildsCollection(reader, "1");

            // Act
            var builddata = buildscollection.GetBuildDataforBuild("1");

            builddata.Should().NotBeNull();
            builddata.BuildNumber.Should().Be("20210116.9");
        }

        [Fact]
        public void Can_get_a_build_by_build_number_that_was_successfully_built()
        {
            string builddefinitionsresponse = File.ReadAllText(@"TestData\builds.json");
            var successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(builddefinitionsresponse);

            var builds = AzureSuccessReponse.ConvertTo<BuildData>(successResponse).ToList();
            var sortedbuilds = builds.Where(r => r.SourceBranch == "refs/heads/main");
            var newbuilddefresponse = JsonConvert.SerializeObject(sortedbuilds);
            successResponse = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(newbuilddefresponse);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync("38").Returns(successResponse);

            var buildscollection = new BuildsCollection(reader, "38");

            // Act
            var build = buildscollection.GetBuildbyBuildNumber("20210116.9");

            // Verify
            build.Should().NotBeNull();
            build.BuildNumber.Should().Be("20210116.9");
            build.Status = BuildData.BuildStatus.completed;
            build.BuiltSuccessfully.Should().BeTrue();
        }

        [Fact]
        public void GetSuccessfulBuildbyBuildNumber_throws_if_empty_build_number_is_input()
        {
            string builddefinitionsresponse = File.ReadAllText(@"TestData\builds.json");
            var successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(builddefinitionsresponse);

            var builds = AzureSuccessReponse.ConvertTo<BuildData>(successResponse).ToList();
            var sortedbuilds = builds.Where(r => r.SourceBranch == "refs/heads/main");
            var newbuilddefresponse = JsonConvert.SerializeObject(sortedbuilds);
            successResponse = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(newbuilddefresponse);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync("1").Returns(successResponse);

            var buildscollection = new BuildsCollection(reader, "1");

            // Act
            Action act = () => buildscollection.GetBuildbyBuildNumber(string.Empty);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void When_a_build_does_not_exist_GetSuccessfulBuildbyBuildNumber_returns_null()
        {
            string builddefinitionsresponse = File.ReadAllText(@"TestData\builds.json");
            var successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(builddefinitionsresponse);

            var builds = AzureSuccessReponse.ConvertTo<BuildData>(successResponse).ToList();
            var sortedbuilds = builds.Where(r => r.SourceBranch == "refs/heads/main");
            var newbuilddefresponse = JsonConvert.SerializeObject(sortedbuilds);
            successResponse = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(newbuilddefresponse);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync("1").Returns(successResponse);

            var buildscollection = new BuildsCollection(reader, "1");

            // Act
            // this build was cancelled, so it wouldn't have built successfully.
            var build = buildscollection.GetBuildbyBuildNumber("2.2.2+1");

            // Verify
            build.Should().BeNull();
        }

        [Fact]
        public void When_querying_for_a_particular_branch_and_build_number_does_exist()
        {
            string builddefinitionsresponse = File.ReadAllText(@"TestData\builds.json");
            var successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(builddefinitionsresponse);

            var builds = AzureSuccessReponse.ConvertTo<BuildData>(successResponse).ToList();
            var sortedbuilds = builds.Where(r => r.SourceBranch == "refs/heads/release");
            var newbuilddefresponse = JsonConvert.SerializeObject(sortedbuilds);
            successResponse = AzureSuccessReponse.BuildAzureSuccessResponseFromValueArray(newbuilddefresponse);

            var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
            reader.GetBuildsbyDefinitionIdAsync("1").Returns(successResponse);

            // Act
            Action act = () => _ = new BuildsCollection(reader, "1");

            // Verify
            act.Should().ThrowExactly<TestResultReportingException>();
        }
    }
}
