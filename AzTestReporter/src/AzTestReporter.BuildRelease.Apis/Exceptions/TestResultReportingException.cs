namespace AzTestReporter.BuildRelease.Apis.Exceptions
{
    using System;

    public class TestResultReportingException : Exception
    {
        public TestResultReportingException(string message) 
            : base(message)
        {
        }
    }
}
