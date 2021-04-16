namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System.Collections.Generic;

    public class FailuresbyTestAreaDataModel
    {
        public string TestClassName { get; set; }

        public string TestNamespace { get; internal set; }

        public string FailingSince { get; set; }

        public string LinktoRunWeb { get; set; }

        public List<FailuresinTestAreaDataModel> FailuresinTestArea;
    }
}
