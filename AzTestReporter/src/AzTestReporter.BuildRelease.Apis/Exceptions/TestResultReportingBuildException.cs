namespace AzTestReporter.BuildRelease.Apis.Exceptions
{
    using System;

    public class TestResultReportingBuildException : TestResultReportingException
    {
        public TestResultReportingBuildException(string message) 
            : base(message)
        {
        }
    }
}
