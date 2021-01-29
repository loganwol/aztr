namespace AzTestReporter.BuildRelease.Apis.Test.Unit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using FluentAssertions;
    using Newtonsoft.Json;
    using AzTestReporter.BuildRelease.Apis;
    using Xunit;

    [ExcludeFromCodeCoverage]
    
    public class TestResultDataCollectionTests
    {
        [Fact]
        public void Get_the_testclassname_from_testresult_data_for_dotnettest()
        {
            // Arrange
            var responsebody = @"{
                'count': 1,
                'value': [{
                  'id': 100000,
                  'startedDate': '2020-01-15T07:03:42.683Z',
                  'completedDate': '2020-01-15T07:03:42.687Z',
                  'outcome': 'Failed',
                  'testRun': {
                            'id': '1378464',
                    'name': 'adadaBVT',
                    'url': 'https://dev.azure.com/ORGANIZATIONNAME/PROJECTNAME/_apis/test/Runs/1378464'
                  },
                  'build': {
                            'id': '2121568',
                    'name': '1.3.191210-1944',
                    'url': 'https://dev.azure.com/ORGANIZATIONNAME/_apis/build/Builds/2121568'
                  },
                  'errorMessage': '',
                  'automatedTestStorage': 'adada.integrationtests.dll',
                  'automatedTestType': 'UnitTest',
                  'testCaseTitle': 'adadas',
                  'stackTrace': '',
                  'automatedTestName': 'adada.asdfadfasd',
            }]}";

            AzureSuccessReponse asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(responsebody);

            // Act
            var agg = new TestResultDataCollection(asr);

            // verify
            agg.Should().HaveCount(1);
            agg[0].TestClassName.Should().Be("adada");
        }

        [Fact]
        public void Successfully_get_the_featurearea_ignoring_the_testcasename()
        {
            // Arrange
            var responsebody = File.ReadAllText(@"TestData\\testresult.json");

            AzureSuccessReponse asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(responsebody);

            // Act
            var agg = new TestResultDataCollection(asr);

            // verify
            agg[0].TestClassName.Should().Be("MSTestRepeatTestMethodAttributeIntegrationTest");
        }

        [Fact]
        public void Get_the_testclassname_from_testresult_data_for_pytest()
        {
            // Arrange
            var responsebody = @"{
                'count': 1,
                'value': [{
                  'id': 100000,
                  'startedDate': '2020-01-15T07:03:42.683Z',
                  'completedDate': '2020-01-15T07:03:42.687Z',
                  'outcome': 'Failed',
                  'testCase': {
                     'name': 'asdad[adasdasd]'
                   },
                  'testRun': {
                            'id': '1378464',
                    'name': 'adad',
                    'url': 'https://dev.azure.com/ORGANIZATIONNAME/PROJECTNAME/_apis/test/Runs/1378464'
                  },
                  'build': {
                            'id': '2121568',
                    'name': '1.3.191210-1944',
                    'url': 'https://dev.azure.com/ORGANIZATIONNAME/_apis/build/Builds/2121568'
                  },
                  'errorMessage': '',
                  'automatedTestStorage': 'test_storage',
                  'automatedTestType': 'JUnit',
                  'testCaseTitle': 'adada',
                  'stackTrace': '',
                  'automatedTestName': 'adada',
            }]}";

            AzureSuccessReponse asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(responsebody);

            // Act
            var agg = new TestResultDataCollection(asr);

            // verify
            agg.Should().HaveCount(1);
            agg[0].TestClassName.Should().Be("adada");
        }

        [Fact]
        public void Get_the_testclassname_from_testresult_data_for_rubytest()
        {
            // Arrange
            var responsebody = @"{
                'count': 1,
                'value': [{
                  'id': 100000,
                  'startedDate': '2020-01-15T07:03:42.683Z',
                  'completedDate': '2020-01-15T07:03:42.687Z',
                  'outcome': 'Failed',
                  'testCase': {
                     'name': 'adada'
                   },
                  'testRun': {
                            'id': '1378464',
                    'name': 'adas',
                    'url': 'https://dev.azure.com/ORGANIZATIONNAME/PROJECTNAME/_apis/test/Runs/1378464'
                  },
                  'build': {
                            'id': '2121568',
                    'name': '1.3.191210-1944',
                    'url': 'https://dev.azure.com/ORGANIZATIONNAME/_apis/build/Builds/2121568'
                  },
                  'errorMessage': '',
                  'automatedTestStorage': 'test_storage',
                  'automatedTestType': 'RUnit',
                  'testCaseTitle': 'adada',
                  'stackTrace': '',
                  'automatedTestName': 'adada',
            }]}";

            AzureSuccessReponse asr = AzureSuccessReponse.ConverttoAzureSuccessResponse(responsebody);

            // Act
            Action act = () => _ = new TestResultDataCollection(asr);

            // verify
            act.Should().Throw<JsonSerializationException>().WithMessage("*Error converting value*");
        }
    }
}
