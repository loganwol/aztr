namespace AzTestReporter.BuildRelease.Apis.Test.Unit
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NSubstitute;
    using AzTestReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]
    
    public class CodeCoverageAggregatorTests
    {
        private CodeCoverageModuleDataCollection codeCoverageAggregator;
        private AzureSuccessReponse successResponse;
        private CodeCoverageAggregate codeCoverageAggregate;

        public CodeCoverageAggregatorTests()
        {
            string codecoveragesresponse = File.ReadAllText(@"TestData\codecoveragedataref.json");
            this.successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(codecoveragesresponse);
        }

        [Fact]
        public void Can_initialize_a_coverage_Aggregator()
        {
            this.InitializeAggregator();
            this.codeCoverageAggregator.Should().NotBeNull();
        }

        [Fact]
        public void Can_return_a_list_of_aggregate_modules()
        {
            this.InitializeAggregator();
            this.codeCoverageAggregator.All.Should().NotBeNull();
            this.codeCoverageAggregator.All.Should().AllBeOfType(typeof(CodeCoverageAggregateCollection));
        }

        [Fact]
        public void Can_return_list_of_matched_aggregate_count()
        {
            this.InitializeAggregator();
            this.codeCoverageAggregator.All.Should().HaveCount(8);
        }

        [Fact]
        public void Aggregate_all_has_non_empty_or_null_names()
        {
            this.InitializeAggregator();
            this.codeCoverageAggregator.All.Where(r => string.IsNullOrEmpty(r.Name)).Should().HaveCount(0);
        }

        [Fact]
        public void Only_one_aggregate_for_each_module_should_be_created()
        {
            this.InitializeAggregator();
            this.codeCoverageAggregator.All.Select(r => r.Name).Distinct().Count()
                .Should().Be(this.codeCoverageAggregator.All.Count());
        }

        [Fact]
        public void Aggregate_CoveredBlocksPercentage_is_as_expected()
        {
            this.InitializeAggregate();
            this.codeCoverageAggregate.CoveredBlocksPercentage.Should().BeApproximately(49, 1);
        }

        [Fact]
        public void Aggregate_NotCoveredBlocksPercentage_is_as_expected()
        {
            this.InitializeAggregate();
            this.codeCoverageAggregate.NotCoveredBlocksPercentage.Should().BeApproximately(50, 1);
        }

        [Fact]
        public void Aggregate_TotalBlocksCovered_is_as_expected()
        {
            this.InitializeAggregate();
            this.codeCoverageAggregate.TotalBlocks.Should().BeApproximately(1005, 1);
        }

        [Fact]
        public void Aggregate_NumberofCoveredBlocks_is_as_expected()
        {
            this.InitializeAggregate();
            this.codeCoverageAggregate.NumberofCoveredBlocks.Should().BeApproximately(501, 1);
        }

        [Fact]
        public void Aggregate_NumberofNotCoveredBlocks_is_as_expected()
        {
            this.InitializeAggregate();
            this.codeCoverageAggregate.NumberofNotCoveredBlocks.Should().BeApproximately(504, 1);
        }

        private void InitializeAggregator()
        {
            if (this.codeCoverageAggregator == null)
            {
                var reader = NSubstitute.Substitute.For<IBuildandReleaseReader>();
                reader.GetTestBuildCoverageDataAsync("97").Returns(this.successResponse);
                this.codeCoverageAggregator = new CodeCoverageModuleDataCollection(reader, "97");
            }
        }

        private void InitializeAggregate()
        {
            this.InitializeAggregator();
            if (this.codeCoverageAggregate == null)
            {
                this.codeCoverageAggregate = this.codeCoverageAggregator.All.Where(r => r.Name == "AzTestReporter.buildrelease.builder.dll").FirstOrDefault();
            }
        }
    }
}
