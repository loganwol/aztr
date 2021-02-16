namespace AzTestReporter.BuildRelease.Builder.Test.Unit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using FluentAssertions;
    using AzTestReporter.BuildRelease.Builder;
    using Xunit;
    using Xunit.Abstractions;

    [ExcludeFromCodeCoverage]
    public class AzurePipelineEnviornmentOptionsTests
    {
        private static bool IsRunningAzurePipeline = !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("SYSTEM_HOSTTYPE"));
        private ITestOutputHelper outputHelper;

        public AzurePipelineEnviornmentOptionsTests(ITestOutputHelper output)
        {
            this.outputHelper = output;
        }

        [Theory]
        [InlineData("azresultsummary", "refs/heads/main", true)]
        [InlineData("azresultsummary", "refs/heads/feature", true)]
        [InlineData("azresultsummary", "refs/heads/release", true)]
        [InlineData("azresultsummary", "refs/heads/product", true)]
        [InlineData("_azresultsummary", "refs/heads/main", false)]
        [InlineData("next", "refs/heads/flkg", false)]
        [InlineData("PR_azresultsummary", "refs/pull/123232", false)]
        [InlineData("", "refs/heads/main", false)]
        public void Check_IsOfficalBranch_value(string builddef, string buildbranch, bool expectedresult)
        {
            var p = new AzurePipelineEnvironmentOptions();
            p.BuildDefinitionName = builddef;
            p.ReleaseSourceBranchName = buildbranch;

            p.IsOfficialBranch.Should().Equals(expectedresult);
        }

        [SkippableFact]
        public void Can_successfully_read_pipeline_build_environment_variables_when_running_in_pipelines()
        {
            Skip.If(!IsRunningAzurePipeline, "This test is meant only to run in azure pipelines");

            var pipeline = new AzurePipelineEnvironmentOptions();
            pipeline.Read();

            pipeline.IsReleasePipeline.Should().BeFalse();
            pipeline.SystemTeamFoundationCollectionURI.Should().NotBeNullOrEmpty().And.StartWith("https://dev").Equals(true);
            pipeline.SystemTeamProject.Should().NotBeNullOrEmpty();
            pipeline.SystemAccessToken.Should().BeNullOrEmpty();
            pipeline.ReleaseSourceBranchName.Should().NotBeNullOrEmpty();
            pipeline.BuildRepositoryName.Should().NotBeNullOrEmpty();
            pipeline.BuildDefinitionName.Should().NotBeNullOrEmpty();
            pipeline.BuildNumber.Should().NotBeNullOrEmpty();
            pipeline.BuildDefinitionID.Should().NotBeNullOrEmpty();
        }

        [SkippableFact]
        public void Can_successfully_get_pipelinevars_tostring()
        {
            Skip.If(!IsRunningAzurePipeline, "This test is meant only to run in azure pipelines");

            var pipeline = new AzurePipelineEnvironmentOptions();
            pipeline.Read();

            pipeline.ToString().Should().NotBeNullOrEmpty();
        }

        [SkippableFact]
        public void Can_successfully_get_pipelinevars_todictionary()
        {
            Skip.If(!IsRunningAzurePipeline, "This test is meant only to run in azure pipelines");

            var pipeline = new AzurePipelineEnvironmentOptions();
            pipeline.Read();

            pipeline.ToDictionary().Should().NotBeNull().And.HaveCount(12);
        }

        [SkippableFact]
        public void Can_successfully_determine_when_we_are_running_in_buildpipeline()
        {
            Skip.If(!IsRunningAzurePipeline, "This test is meant only to run in azure pipelines");

            var pipeline = new AzurePipelineEnvironmentOptions();
            pipeline.environmentvars = new Dictionary<string, string>()
            {
                { "SYSTEM_HOSTTYPE", "build" },
            };

            Action act = () => pipeline.Read();

            act.Should().Throw<KeyNotFoundException>();
            pipeline.IsReleasePipeline.Should().BeFalse();
        }

        [Fact]
        public void Ensure_release_pipeline_variables_are_read_only_when_intended()
        {
            var pipeline = new AzurePipelineEnvironmentOptions();
            pipeline.environmentvars = new Dictionary<string, string>()
            {   
                { "SYSTEM_HOSTTYPE", "build" },
            };

            Action act = () => pipeline.Read(false);

            act.Should().Throw<KeyNotFoundException>();
            pipeline.IsReleasePipeline.Should().BeFalse();
        }

        [Fact]
        public void Can_successfully_determine_when_we_are_running_in_releasepipeline()
        {
            var pipeline = new AzurePipelineEnvironmentOptions();
            pipeline.environmentvars = new Dictionary<string, string>()
            {
                { "SYSTEM_ENABLEACCESSTOKEN", "true" },
                { "SYSTEM_HOSTTYPE", "release" },
            };

            Action act = () => pipeline.Read(true);

            act.Should().Throw<KeyNotFoundException>().WithMessage("The given key was not present in the dictionary.");
            pipeline.IsReleasePipeline.Should().BeTrue();
        }

        [Fact]
        public void Can_successfully_read_systemaccesstoken_from_azurepipeline()
        {
            var pipeline = new AzurePipelineEnvironmentOptions();
            pipeline.environmentvars = new Dictionary<string, string>()
            {
                { "SYSTEM_HOSTTYPE", "build" },
                { "SYSTEM_ENABLEACCESSTOKEN", string.Empty },
                { "SYSTEM_ACCESSTOKEN", "thisisasecret" }
            };

            Action act = () => pipeline.Read(true);

            act.Should().Throw<KeyNotFoundException>();
            pipeline.SystemAccessToken.Should().Be("thisisasecret");
        }

        [SkippableFact]
        public void Can_read_required_azure_pipeline_build_envrionmentvariables()
        {
            var systemhost = Environment.GetEnvironmentVariable("SYSTEM_HOSTTYPE");

            Skip.IfNot(systemhost != null && systemhost.Equals("release", StringComparison.InvariantCultureIgnoreCase),
                "This test is intended to run only in release pipelines as an integration test.");

            var pipeline = new AzurePipelineEnvironmentOptions();

            // Act
            pipeline.Read();

            // Verify
            pipeline.ReleaseDefinitionName.Should().NotBeNullOrEmpty();
            pipeline.ReleaseName.Should().NotBeNullOrEmpty();
            pipeline.ReleaseExecutionStage.Should().NotBeNullOrEmpty();
            pipeline.ReleaseSourceBranchName.Should().NotBeNullOrEmpty();
            pipeline.ReleasePipelineDefaultWorkingDirectory.Should().NotBeNullOrEmpty();
            pipeline.ReleaseAttempt.Should().BeGreaterOrEqualTo(1);
            pipeline.AgentID.Should().NotBeNullOrEmpty();
            pipeline.BuildDefinitionName.Should().NotBeNullOrEmpty();
            pipeline.BuildNumber.Should().NotBeNullOrEmpty();
            pipeline.BuildRepositoryName.Should().NotBeNullOrEmpty();
            pipeline.SystemAccessToken.Should().NotBeNullOrEmpty();
        }

        [SkippableFact]
        public void Output_all_env_variables()
        {
            Skip.IfNot(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")), "This test is intended to run only in azure pipelines as an integration test.");

            var systemhost = Environment.GetEnvironmentVariable("SYSTEM_HOSTTYPE");

            Skip.IfNot(systemhost != null && systemhost.Equals("release", StringComparison.InvariantCultureIgnoreCase),
                "This test is intended to run only in release pipelines as an integration test.");

            var environmentvars = Environment.GetEnvironmentVariables()
                        .Cast<DictionaryEntry>()
                        .ToDictionary(r => r.Key.ToString(), r => r.Value.ToString());

            foreach(var envvar in environmentvars)
            {
                this.outputHelper.WriteLine($"{envvar.Key}\t={envvar.Value}");
            }
        }
    }
}
