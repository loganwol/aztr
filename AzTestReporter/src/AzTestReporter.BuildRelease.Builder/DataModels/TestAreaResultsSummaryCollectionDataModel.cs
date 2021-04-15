namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System.Collections.Generic;
    using System.Linq;
    using AzTestReporter.BuildRelease.Apis;

    public class TestAreaResultsSummaryCollectionDataModel : List<TestAreaResultsSummaryDataModel>
    {
        public TestAreaResultsSummaryCollectionDataModel(IReadOnlyList<TestResultData> testResultData, bool summarizewithsubresults = false)
        {
            List<TestAreaResultsSummaryDataModel> orderedtestresultdata = new List<TestAreaResultsSummaryDataModel>();
            if (testResultData != null)
            {
                orderedtestresultdata.AddRange(testResultData.GroupBy(r => r.TestClassName)
                    .OrderBy(r => r.Key)
                    .Select(r => new TestAreaResultsSummaryDataModel(r.Key, r.ToList(), summarizewithsubresults)).ToList());

                orderedtestresultdata = orderedtestresultdata.OrderBy(r => r.TestClassName).ToList();

                this.AddRange(orderedtestresultdata);
            }
        }
    }
}
