namespace AzTestReporter.BuildRelease.Apis.Test.Unit
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class BuildDefinitionsTests
    {
        private string builddefinitionsresponse;
        private AzureSuccessReponse successResponse;

        public BuildDefinitionsTests()
        {
            if (string.IsNullOrEmpty(builddefinitionsresponse))
            {
                this.builddefinitionsresponse = File.ReadAllText(@"TestData\builddefinitionresposeref.json");
            }
        }

        [Fact]
        public void Can_serialize_builddefinition_to_response_object_with_no_exceptions()
        {
            this.DeserializeResponse();
            this.successResponse.Should().NotBeNull();
        }

        [Fact]
        public void Can_serialize_builddefinition_to_response_object_with_count_matched()
        {
            this.DeserializeResponse();
            this.successResponse.Count.Should().Be(50);
        }

        [Fact]
        public void Can_serialize_builddefinition_to_response_object_value_should_not_be_null()
        {
            this.DeserializeResponse();
            this.successResponse.Value.Should().NotBeNull();
        }

        private void DeserializeResponse()
        {
            if (this.successResponse == null)
            {
                this.successResponse = JsonConvert.DeserializeObject<AzureSuccessReponse>(this.builddefinitionsresponse);
            }
        }
    }
}
