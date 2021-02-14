// <copyright file="ReleaseTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace AzTestReporter.BuildRelease.Apis.Test.Unit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NSubstitute;
    using AzTestReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]    
    public class ReleaseTest
    {
        [Fact]
        public void Verify_releasetype_is_private_for_manual_releases()
        {
            // Act
            Release release = new Release() { Reason = "manual" };

            // Verify
            release.ReleaseType.Should().Be(JobType.Private);
        }

        [Fact]
        public void Verify_releasetype_is_master_for_ci_releases()
        {
            // Act
            Release release = new Release() { Reason = "continuousIntegration" };

            // Verify
            release.ReleaseType.Should().Be(JobType.Master);
        }

        [Fact]
        public void Verify_releasetype_is_master_for_scheduled_releases()
        {
            // Act
            Release release = new Release() { Reason = "Schedule" };

            // Verify
            release.ReleaseType.Should().Be(JobType.Master);
        }

        [Fact]
        public void Verify_invalid_releasetype_throws()
        {
            // Arrange
            Release release = new Release() { Reason = "scheduled" };

            // Act
            Action action = () => { var rt = release.ReleaseType; };

            // Verify
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Return_a_valid_envrionment_id_for_a_stage_in_release()
        {
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));
            azureReader.GetReleaseResultAsync("1085").ReturnsForAnyArgs(release);

            var environmentid = release.GetStageId("Integration test Execution");

            environmentid.Should().NotBe(-1);
            environmentid.Should().Be(59);
        }

        [Fact]
        public void Return_a_valid_envrionment_id_for_a_stage_name_does_not_startwith_in_release()
        {
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));
            azureReader.GetReleaseResultAsync("1085").ReturnsForAnyArgs(release);

            var environmentid = release.GetStageId("badtext");

            environmentid.Should().Be(-1);
        }

        [Fact]
        public void If_input_argument_of_stagename_is_null_GetStageId_throws()
        {
            Release release = new Release();

            Action act = () =>
            {
                var environmentid = release.GetStageId(string.Empty);
            };

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_return_null_if_environment_was_not_found_in_release_details()
        {
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));
            release.Environments = null;
            azureReader.GetReleaseResultAsync("1085").ReturnsForAnyArgs(release);

            var environmentid = release.GetStageId("test");

            environmentid.Should().Be(-1);
        }

        [Fact]
        public void Should_return_true_if_release_contains_a_specific_named_stage()
        {
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));
            azureReader.GetReleaseResultAsync("1085").ReturnsForAnyArgs(release);

            var stageresult = release.ContainsStage("Integration test Execution");

            stageresult.Should().BeTrue();
        }

        [Fact]
        public void Should_not_find_a_failed_task_as_stage_does_not_contain_testruns()
        {
            // Arrange
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\ReleaseWithFailingTask.json"));
            release.CurrentAttempt = 1;
            azureReader.GetReleaseResultAsync("76").ReturnsForAnyArgs(release);

            // Act
            var failingTaskName = release.FailedTaskName;

            // Verify
            failingTaskName.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Should_return_false_if_release_contains_a_specific_named_stage()
        {
            IBuildandReleaseReader azureReader = Substitute.For<IBuildandReleaseReader>();

            Release release = JsonConvert.DeserializeObject<Release>(File.ReadAllText(@"TestData\\release.json"));
            azureReader.GetReleaseResultAsync("1085").ReturnsForAnyArgs(release);

            var stageresult = release.ContainsStage("baddata");

            stageresult.Should().BeFalse();
        }
    }
}
