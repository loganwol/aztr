namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System.Collections.Generic;
    using System.Linq;
    using AzTestReporter.BuildRelease.Apis;    

    /// <summary>
    /// Class for Test Run names and links model.
    /// </summary>
    public class TestRunNameLinksCollectionDataModel : List<TestRunNameLinksDataModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunNameLinksCollectionDataModel"/> class.
        /// </summary>
        /// <param name="testRunsList">List of <see cref="Run">.</param>.
        public TestRunNameLinksCollectionDataModel(IReadOnlyList<Run> testRunsList)
        {
            if (testRunsList != null && testRunsList.Count > 0)
            {
                var namelinkstable = testRunsList.GroupBy(r => r.Name).ToDictionary(r => r.Key, r => r.First().webAccessUrl);

                this.AddRange(namelinkstable.Select(r => new TestRunNameLinksDataModel()
                {
                    Name = r.Key,
                    Url = r.Value,
                    TestsUrl = r.Value.Replace("runCharts", "resultQuery"),
                }).ToList());
            }
        }
    }
}
