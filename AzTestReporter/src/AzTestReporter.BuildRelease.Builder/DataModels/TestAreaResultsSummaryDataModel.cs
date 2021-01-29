namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using AzTestReporter.BuildRelease.Apis;
    using System.Collections.Generic;
    using System.Linq;
    using Validation;

    public class TestAreaResultsSummaryDataModel : ResultSummaryDataModel
    {
        public string TestClassName { get; internal set; }

        public TestAreaResultsSummaryDataModel(string testclassname, List<TestResultData> testResults)
        {
            Requires.NotNull(testResults, nameof(testResults));

            this.TestClassName = testclassname;
            this.Total = testResults.Count;
            this.Passed = testResults.Where(r => r.Outcome == "Passed").Count();
            this.Failed = testResults.Where(r => r.Outcome == "Failed").Count();
        }
    }
}
