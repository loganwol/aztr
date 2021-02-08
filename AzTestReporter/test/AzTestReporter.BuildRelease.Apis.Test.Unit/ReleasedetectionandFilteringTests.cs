// <copyright file="ReleasedetectionandFilteringTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace TestRunResultReporter.BuildRelease.Apis.Test.Unit
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using FluentAssertions;
    using NSubstitute;
    using TestRunResultReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]
    
    public class ReleasedetectionandFilteringTests
    {
        private AzureSuccessReponse asr;
        private string responsebody;

        public ReleasedetectionandFilteringTests()
        {
            this.responsebody = File.ReadAllText(@"TestData\\releases.json");
        }

        [Fact]
        public void Can_get_a_valid_releaseobject_for_a_scheduled_run()
        {
            string releasejson = @"{
              'count': 1,
              'value': [
                {
                  'id': 1446,
                  'name': 'Release-133',
                  'status': 'active',
                  'createdOn': '2020 - 03 - 18T10: 00:00.58Z',
                  'createdBy': {
                    'displayName' : 'foo',
                    'uniqueName': 'vstfs:///Framework/IdentityDomain/5f7fbb9e-d1c9-41e5-bb95-992266749965\\Project Collection Service Accounts',
                  },
                  'releaseDefinition': {
                    'id': 19,
                    'name': 'Project',
                  },
                  'releaseDefinitionRevision': 83,
                  'description': 'Scheduled release for pipeline BVT.',
                  'reason': 'schedule',
                  '_links': {
                    'web': { 'href': 'https://dev.azure.com/ORGANIZATIONNAME/89ce17af-3021-4fd0-9586-12aff6749535/_release?releaseId=1446&_a=release-summary' }
                  },
                }
              ]
            }";

            var asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(releasejson);

            var devopsReader = Substitute.For<IBuildandReleaseReader>();
            devopsReader.GetReleasesbyDateRangeAsync(" ", " ").ReturnsForAnyArgs(asr);

            var releases = new ReleasesCollection(devopsReader, "Project");

            // Act
            Release release = releases.GetReleaseDatabyName("Release-133");

            // Verify
            release.Should().NotBeNull();
        }

        [Fact]
        public void Can_get_a_valid_release_object_for_a_manual_run()
        {
            string releasejson = @"{
              'count': 1,
              'value': [
                {
                  'id': 444,
                  'name': 'Release-1.644.200318-0018',
                  'status': 'active',
                  'createdOn': '2020-03-18T06:00:34.08Z',
                  'createdBy': {
                    'displayName' : 'foo',
                    'uniqueName': 'abc@microsoft.com'
                  },
                  'releaseDefinition': {
                    'id': 14,
                    'name': 'Manual run',
                    'projectReference': null
                  },
                  'reason': 'manual',
                  '_links': {
                    'web': { 'href': 'https://dev.azure.com/ORGANIZATIONNAME' }
                  }
                }
              ]
            }";

            var asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(releasejson);

            var devopsReader = Substitute.For<IBuildandReleaseReader>();
            devopsReader.GetReleasesbyDateRangeAsync(" ", " ").ReturnsForAnyArgs(asr);

            var releases = new ReleasesCollection(devopsReader, "Manual run");

            asr.Should().NotBeNull();

            Release release = releases.GetReleaseDatabyName("Release-1.644.200318-0018");
            release.Should().NotBeNull();
        }

        [Fact]
        public void Can_successfully_return_a_release_when_a_release_was_run_as_continousintegration()
        {
            string releasejson = @"{
              'count': 1,
              'value': [
                {
              'id': 443,
              'name': 'Release-1.644.200318-0018',
              'status': 'active',
              'createdOn': '2020-03-18T00:45:12.133Z',
              'createdBy': {
                'displayName' : 'foo',
                'uniqueName': 'abc@microsoft.com',
              },
              'releaseDefinition': {
                'id': 12,
                'name': 'CI Run',
              },
              'reason': 'continuousIntegration',
              '_links': {
                'web': { 'href': 'https://dev.azure.com/ORGANIZATIONNAME' }
              }
            }]}";

            var asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(releasejson);

            var devopsReader = Substitute.For<IBuildandReleaseReader>();
            devopsReader.GetReleasesbyDateRangeAsync(" ", " ").ReturnsForAnyArgs(asr);

            var releases = new ReleasesCollection(devopsReader, "CI Run");

            asr.Should().NotBeNull();

            // Act
            Release release = releases.GetReleaseDatabyName("Release-1.644.200318-0018");

            // Verify
            release.Should().NotBeNull();
        }
    }
}
