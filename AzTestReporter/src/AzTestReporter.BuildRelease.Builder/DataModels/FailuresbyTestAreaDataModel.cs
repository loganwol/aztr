namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System.Collections.Generic;

    public class FailuresbyTestAreaDataModel
    {
        public string TestClassName { get; set; }

        public string TestName { get; set; }

        public string LinktoRunWeb { get; set; }

        public string Duration { get; set; }

        public string ErrorMessage { get; set; }

        public string FailingSince { get; set; }

        public Dictionary<string, string> BugandLink { get; set; }
        
        public int RowCount { get; internal set; }
    }
}
