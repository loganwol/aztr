// <copyright file="AzureReleaseCollectionTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace AzTestReporter.BuildRelease.Apis.Test.Unit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using FluentAssertions;
    using NSubstitute;
    using AzTestReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class ReleaseCollectionTests
    {
        private ReleasesCollection releaseAggregator;
        private IBuildandReleaseReader devopsReader;
        private Release releaseData;

        public ReleaseCollectionTests()
        {
            string releasesresponse = File.ReadAllText(@"TestData\releases.json");
            var asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(releasesresponse);

            this.devopsReader = Substitute.For<IBuildandReleaseReader>();
            this.devopsReader.GetReleasesbyDateRangeAsync(" ", " ").ReturnsForAnyArgs(asr);
        }

        [Fact]
        public void Can_create_a_buildDefinitionsAggregator_object_successfully()
        {
            this.InitializeAgg();
            this.releaseAggregator.Should().NotBeNull();
        }

        [Fact]
        public void Can_find_tools_tests_in_buildDefinitionsAggregator()
        {
            this.releaseAggregator = new ReleasesCollection(this.devopsReader, "Azure Test Reporter Release");
            this.releaseAggregator.GetReleaseDatabyName("ATR Release-46").Should().NotBeNull();
        }

        [Fact]
        public void Can_buildDefinitionsAggregator_property_createdon_is_matched()
        {
            this.InitializeBuildData();
            this.releaseData.CreatedOn.Should().NotBeAfter(DateTime.UtcNow);
        }

        [Fact]
        public void Can_buildDefinitionsAggregator_property_CreatedBy_DisplayName_is_notnullorempty()
        {
            this.InitializeBuildData();
            this.releaseData.CreatedBy.DisplayName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Can_buildDefinitionsAggregator_property_Id_is_notnullorempty()
        {
            this.InitializeBuildData();
            this.releaseData.Id.Should().Be("76");
        }

        [Fact]
        public void Can_buildDefinitionsAggregator_property_Name_is_notnullorempty()
        {
            this.InitializeBuildData();
            this.releaseData.Name.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Can_buildDefinitionsAggregator_property_Reason_is_notnullorempty()
        {
            this.InitializeBuildData();
            this.releaseData.Reason.Should().NotBeNullOrEmpty();
        }

        private void InitializeAgg()
        {
            if (this.releaseAggregator == null)
            {
                this.releaseAggregator = new ReleasesCollection(this.devopsReader, "Azure Test Reporter Release");
            }
        }

        private void InitializeBuildData()
        {
            if (this.releaseData == null)
            {
                this.InitializeAgg();
                this.releaseData = this.releaseAggregator.GetReleaseDatabyName("ATR Release-46");
            }
        }
    }
}