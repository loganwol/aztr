namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using AzTestReporter.BuildRelease.Apis;
    using System.Collections.Generic;
    using System.Linq;
    using Validation;

    public class TestAreaResultsSummaryDataModel : ResultSummaryDataModel
    {
        public string TestClassName { get; internal set; }

        public string TestNamespace { get; internal set; }

        public TestAreaResultsSummaryDataModel(string testclassname, List<TestResultData> testResults)
        {
            Requires.NotNull(testResults, nameof(testResults));

            this.TestNamespace = testResults[0].TestNamespace;

            this.TestClassName = testclassname;

            foreach (var testresult in testResults)
            {
                if (testresult.TestSubResults != null && testresult.TestSubResults.Any())
                {
                    this.Total += testresult.TestSubResults.Count();
                    this.Passed += testresult.TestSubResults
                        .Where(r => r.Outcome == Apis.Common.OutcomeEnum.Passed).Count();
                    this.Failed += testresult.TestSubResults
                        .Where(r => r.Outcome == Apis.Common.OutcomeEnum.Failed).Count();
                }
                else
                {
                    this.Total++;
                    this.Passed += testresult.Outcome == Apis.Common.OutcomeEnum.Passed ? 1 : 0;
                    this.Failed += testresult.Outcome == Apis.Common.OutcomeEnum.Failed ? 1 : 0;
                }
            }
        }
    }
}
