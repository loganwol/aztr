namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using AzTestReporter.BuildRelease.Apis;
    using System.Collections.Generic;
    using System.Linq;
    using Validation;

    public class TestAreaResultsSummaryDataModel : RunResultSummaryDataModel
    {
        public string TestClassName { get; internal set; }

        public string TestNamespace { get; internal set; }

        public TestAreaResultsSummaryDataModel(string testclassname, List<TestResultData> testResults, bool summarizewithsubresults = false)
        {
            Requires.NotNull(testResults, nameof(testResults));

            this.TestNamespace = testResults[0].TestNamespace;

            this.TestClassName = testclassname;

            foreach (var testresult in testResults)
            {
                if (summarizewithsubresults && testresult.TestSubResults.Any())
                {
                    this.Passed += testresult.TestSubResults
                        .Where(r => r.Outcome == Apis.Common.OutcomeEnum.Passed).Count();
                    this.Failed += testresult.TestSubResults
                        .Where(r => r.Outcome == Apis.Common.OutcomeEnum.Failed).Count();
                }
                else
                {
                    this.Passed += testresult.Outcome == Apis.Common.OutcomeEnum.Passed ? 1 : 0;
                    this.Failed += testresult.Outcome == Apis.Common.OutcomeEnum.Failed ? 1 : 0;
                }
            }
        }
    }
}
