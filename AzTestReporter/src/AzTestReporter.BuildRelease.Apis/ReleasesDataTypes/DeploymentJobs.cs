namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;

    public class DeploymentJobs
    {
        [JsonProperty(PropertyName = "job")]
        public Job Job { get; set; }

        [JsonProperty(PropertyName = "tasks")]
        public List<DeploymentTask> Tasks { get; set; }

        internal bool ContainsTestTask => this.Tasks.Any(r => r.SubTask?.IsTestTask == true);
    }
}
