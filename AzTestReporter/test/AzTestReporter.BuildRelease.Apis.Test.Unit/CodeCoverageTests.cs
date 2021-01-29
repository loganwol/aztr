// <copyright file="AzureCodeCoverageTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace AzTestReporter.BuildRelease.Apis.Test.Unit
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using FluentAssertions;
    using Newtonsoft.Json;
    using AzTestReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class CodeCoverageTests
    {
        private string codecoveragesresponse;
        private AzureSuccessReponse successResponse;

        public CodeCoverageTests()
        {
            this.codecoveragesresponse = File.ReadAllText(@"TestData\codecoveragedataref.json");
        }

        [Fact]
        public void Can_serialize_codecoverage_to_response_object_with_no_exceptions()
        {
            this.DeserializeResponse();
            this.successResponse.Should().NotBeNull();
        }

        [Fact]
        public void Can_serialize_codecoverage_to_response_object_with_count_matched()
        {
            this.DeserializeResponse();
            this.successResponse.Count.Should().Be(1);
        }

        [Fact]
        public void Can_serialize_codecoverage_to_response_object_value_should_not_be_null()
        {
            this.DeserializeResponse();
            this.successResponse.Value.Should().NotBeNull();
        }

        private void DeserializeResponse()
        {
            if (this.successResponse == null)
            {
                this.successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(this.codecoveragesresponse);
            }
        }
    }
}
