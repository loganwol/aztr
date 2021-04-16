using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    public class FailuresinTestAreaDataModel
    {
        public string TestName { get; set; }

        public string Duration { get; set; }

        public string ErrorMessage { get; set; }

        public Dictionary<string, string> BugandLink { get; set; }
    }
}
