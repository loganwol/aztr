namespace AzTestReporter.BuildRelease.Apis.Exceptions
{
    using Validation;

    public class TestResultReportingSendToNullException : TestResultReportingException
    {
        public TestResultReportingSendToNullException(string message, string detailsjson) 
            : base(message)
        {
            Requires.NotNullOrEmpty(message, nameof(message));
            Requires.NotNullOrEmpty(detailsjson, nameof(detailsjson));
            this.DetailsJson = detailsjson;
        }

        /// <summary>
        /// Gets the Json data respective to the Build or Release it's reteriving.
        /// </summary>
        public string DetailsJson { get; }
    }
}
