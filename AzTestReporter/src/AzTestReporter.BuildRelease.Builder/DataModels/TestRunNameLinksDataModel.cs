namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    /// <summary>
    /// Class to hold the Test Run links data model.
    /// </summary>
    public class TestRunNameLinksDataModel
    {
        /// <summary>
        /// gets or sets the Test run name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// gets or sets the Url to the test run overview.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// gets or sets the Url to the tests from the test run.
        /// </summary>
        public string TestsUrl { get; set; }
    }
}
