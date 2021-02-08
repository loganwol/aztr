namespace AzTestReporter.BuildRelease.Apis
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Validation;

    public class TestResultDataCollection : List<TestResultData>
    {
        public TestResultDataCollection(AzureSuccessReponse runData)
        {
            Requires.NotNull(runData, nameof(runData));

            var testresultdatalist = runData.Value
                .Select(r => JsonConvert.DeserializeObject<TestResultData>(r.ToString()))
                .ToList();

            this.AddRange(testresultdatalist);
        }
    }
}
