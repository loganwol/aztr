namespace AzTestReporter.BuildRelease.Apis.Exceptions
{
    public class TestResultReportingNoResultsFoundException : TestResultReportingException
    {
        public TestResultReportingNoResultsFoundException(string message) 
            : base(message) 
        {
        }

        public TestResultReportingNoResultsFoundException(string linkname, string linkurl)
            : base($"No runs were found for the current Release or Build Id: {linkname} \r\n Link: {linkurl}")
        {
        }
    }
}
