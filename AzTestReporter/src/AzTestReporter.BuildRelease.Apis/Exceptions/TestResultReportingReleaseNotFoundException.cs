namespace AzTestReporter.BuildRelease.Apis.Exceptions
{
    public class TestResultReportingReleaseNotFoundException : TestResultReportingException
    {
        public enum ReleaseDataType
        {
            ReleaseDefinition,
            ReleaseName
        }

        public TestResultReportingReleaseNotFoundException(string message) 
            : base(message) 
        {}

        public TestResultReportingReleaseNotFoundException(ReleaseDataType releaseDataType, string name) : 
            base($"Could not find a {releaseDataType} with value: {name}.")
        {
        }

        public TestResultReportingReleaseNotFoundException(string releaseName, string stageName) :
            base($"Did not find a stage with name {stageName} in release {releaseName}.")
        {
        }

        public TestResultReportingReleaseNotFoundException(string releaseName, string stageName, string buildNumber) :
            base($"Did not find a stage with name {stageName} in release {releaseName} with build version {buildNumber}.")
        {
        }
    }
}
