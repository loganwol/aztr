namespace AzTestReporter.BuildRelease.Apis.Exceptions
{
    public class TestResultAzureQueryException : TestResultReportingException
    {
        public TestResultAzureQueryException(string message, string queryUrl, string response) : base(message)
        {
            this.QueryUrl = queryUrl;
            this.Response = response;
        }

        public string QueryUrl { get; private set; }
        public string Response { get; private set; }
    }
}
