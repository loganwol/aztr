namespace AzTestReporter.BuildRelease.Apis.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Xunit;

    [ExcludeFromCodeCoverage]
    
    public class TestRunDetectionTests
    {
        [Fact]
        public void Can_detect_a_testrun_from_releasedefinition_for_releasepath()
        {
            string response = File.ReadAllText(@"TestData\\release.json");
            var release = JsonConvert.DeserializeObject<Release>(response);

            // Act and Verify
            release.CurrentAttempt = 1;

            release.TestRunNames.Should().NotBeNull();
            release.TestRunNames[0].Should().Be("Run Integration Tests");
        }

        [Fact]
        public void Filterdown_to_testruns_that_are_unique_in_name_flavor_and_id_on_a_specific_environment()
        {
            string response = @"{
              'value': [
                {
                  'id': 1445820,
                  'name': 'TestRun_CI_Project_1.203.200319-2058',
                  'buildConfiguration': {
                    'id': 2576900,
                    'number': '1.203.200319-2058',
                    'flavor': 'Release',
                    'buildDefinitionId': 10923
                  },
                  'isAutomated': true,
                  'startedDate': '2020-03-19T21:03:15.513Z',
                  'completedDate': '2020-03-19T21:06:47.49Z',
                  'createdDate': '2020-03-19T21:06:47.91Z',
                  'revision': 0,
                  'release': {
                    'id': 0,
                    'name': null,
                    'environmentId': 1,
                    'environmentName': null,
                    'definitionId': 0,
                    'environmentDefinitionId': 0,
                    'environmentDefinitionName': null
                  }
                },
                {
                  'id': 1445822,
                  'name': 'TestRun_CI_Project_1.203.200319-2058',
                  'buildConfiguration': {
                    'id': 2576900,
                    'number': '1.203.200319-2058',
                    'flavor': 'Debug',
                    'buildDefinitionId': 10923
                  },
                  'isAutomated': true,
                  'startedDate': '2020-03-19T21:05:29.177Z',
                  'completedDate': '2020-03-19T21:08:09.487Z',
                  'state': 'Completed',
                  'totalTests': 624,
                  'incompleteTests': 0,
                  'notApplicableTests': 0,
                  'passedTests': 0,
                  'unanalyzedTests': 0,
                  'createdDate': '2020-03-19T21:08:09.91Z',
                  'lastUpdatedDate': '2020-03-19T21:08:11.173Z',
                  'revision': 0,
                  'release': {
                    'id': 0,
                    'name': null,
                    'definitionId': 0,
                  }
                },
                {
                  'id': 1445824,
                  'name': 'TestRun_CI_Project_1.203.200319-2058',
                  'buildConfiguration': {
                    'id': 2576900,
                    'number': '1.203.200319-2058',
                    'flavor': 'Release',
                    'buildDefinitionId': 10923
                  },
                  'isAutomated': true,
                  'startedDate': '2020-03-19T21:05:29.177Z',
                  'completedDate': '2020-03-19T21:08:09.487Z',
                  'state': 'Completed',
                  'totalTests': 624,
                  'incompleteTests': 0,
                  'notApplicableTests': 0,
                  'passedTests': 0,
                  'unanalyzedTests': 0,
                  'createdDate': '2020-03-19T21:08:09.91Z',
                  'lastUpdatedDate': '2020-03-19T21:08:11.173Z',
                  'revision': 0,
                  'release': {
                    'id': 0,
                    'name': null,
                    'definitionId': 0,
                  }
                },
                {
                  'id': 1445828,
                  'name': 'TestRun_CI_Project_1.203.200319-2058',
                  'buildConfiguration': {
                    'id': 2576900,
                    'number': '1.203.200319-2058',
                    'flavor': 'Debug',
                    'buildDefinitionId': 10923
                  },
                  'isAutomated': true,
                  'startedDate': '2020-03-19T21:05:29.177Z',
                  'completedDate': '2020-03-19T21:08:09.487Z',
                  'state': 'Completed',
                  'totalTests': 624,
                  'incompleteTests': 0,
                  'notApplicableTests': 0,
                  'passedTests': 0,
                  'unanalyzedTests': 0,
                  'createdDate': '2020-03-19T21:08:09.91Z',
                  'lastUpdatedDate': '2020-03-19T21:08:11.173Z',
                  'revision': 0,
                  'release': {
                    'id': 0,
                    'name': null,
                    'definitionId': 0,
                    'environmentId': 1,
                  }
                },
                {
                  'id': 1445830,
                  'name': 'TestRun_CI_Project',
                  'buildConfiguration': {
                    'id': 2576900,
                    'number': '1.203.200319-2058',
                    'flavor': '',
                    'buildDefinitionId': 10923
                  },
                  'isAutomated': true,
                  'startedDate': '2020-03-19T21:05:29.177Z',
                  'completedDate': '2020-03-19T21:08:09.487Z',
                  'state': 'Completed',
                  'totalTests': 624,
                  'incompleteTests': 0,
                  'notApplicableTests': 0,
                  'passedTests': 0,
                  'unanalyzedTests': 0,
                  'createdDate': '2020-03-19T21:08:09.91Z',
                  'lastUpdatedDate': '2020-03-19T21:08:11.173Z',
                  'revision': 0,
                  'release': {
                    'id': 0,
                    'name': null,
                    'definitionId': 0,
                    'environmentId': 2,
                  }
                },
                {
                  'id': 1445820,
                  'name': 'TestRun_CI_Project_1.203.200319-2058',
                  'buildConfiguration': {
                    'id': 2576900,
                    'number': '1.203.200319-2058',
                    'flavor': 'Release',
                    'buildDefinitionId': 10923
                  },
                  'isAutomated': true,
                  'startedDate': '2020-03-19T21:03:15.513Z',
                  'completedDate': '2020-03-19T21:06:47.49Z',
                  'createdDate': '2020-03-19T21:06:47.91Z',
                  'revision': 0,
                  'release': {
                    'id': 0,
                    'name': null,
                    'environmentId': 0,
                    'environmentName': null,
                    'definitionId': 0,
                    'environmentDefinitionId': 0,
                    'environmentDefinitionName': null
                  }
                }
              ],
              'count': 6
            }";

            AzureSuccessReponse asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(response);
            TestRunsCollection runs = new TestRunsCollection(asr);
            List<Run> testruns = runs.MatchedRunsbyStageandExecution(1, Convert.ToDateTime("2020-03-19T21:05:29"));

            testruns.Should().NotBeEmpty();
            testruns.Should().HaveCount(1);
            testruns.Where(r => r.Name.Equals("TestRun_CI_Project_1.203.200319-2058")).Should().HaveCount(1);

            testruns = runs.MatchedRunsbyStageandExecution(1, Convert.ToDateTime("2020-03-19T21:03:15"));
            testruns.Should().HaveCount(2);
        }

        [Fact]
        public void Only_automated_testruns_should_be_captured_to_generate_automated_testrun_result_Reports()
        {
            // Arrange
            string response = @"{
              'value': [
                {
                  'id': 1445820,
                  'name': 'TestRun_CI_Project_1.203.200319-2058',
                  'buildConfiguration': {
                    'id': 2576900,
                    'number': '1.203.200319-2058',
                    'flavor': 'Release',
                    'buildDefinitionId': 10923
                  },
                  'isAutomated': false,
                  'startedDate': '2020-03-19T21:03:15.513Z',
                  'completedDate': '2020-03-19T21:06:47.49Z',
                  'createdDate': '2020-03-19T21:06:47.91Z',
                  'revision': 0,
                  'release': {
                    'id': 0,
                    'name': null,
                    'environmentId': 0,
                    'environmentName': null,
                    'definitionId': 0,
                    'environmentDefinitionId': 0,
                    'environmentDefinitionName': null
                  }
                },
                {
                  'id': 1445822,
                  'name': 'TestRun_CI_Project_1.203.200319-2058',
                  'buildConfiguration': {
                    'id': 2576900,
                    'number': '1.203.200319-2058',
                    'flavor': 'Debug',
                    'buildDefinitionId': 10923
                  },
                  'isAutomated': true,
                  'startedDate': '2020-03-19T21:05:29.177Z',
                  'completedDate': '2020-03-19T21:08:09.487Z',
                  'state': 'Completed',
                  'totalTests': 624,
                  'incompleteTests': 0,
                  'notApplicableTests': 0,
                  'passedTests': 0,
                  'unanalyzedTests': 0,
                  'createdDate': '2020-03-19T21:08:09.91Z',
                  'lastUpdatedDate': '2020-03-19T21:08:11.173Z',
                  'revision': 0,
                  'release': {
                    'id': 0,
                    'name': null,
                    'definitionId': 0,
                  }
                },
              ],
              'count': 2
            }";

            AzureSuccessReponse asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(response);

            // Act
            TestRunsCollection runs = new TestRunsCollection(asr);

            // Verify
            runs.Count.Should().Be(1);
        }
    }
}
