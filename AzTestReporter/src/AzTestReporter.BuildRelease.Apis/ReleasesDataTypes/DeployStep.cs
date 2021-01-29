namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class DeployStep
    {
        [JsonProperty(PropertyName = "attempt")]
        public int Attempt { get; set; } = 1;

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "releaseDeployPhases")]
        public List<ReleaseDeployPhases> ReleaseDeployPhases { get; set; }

        internal DateTime FirstJobExecutionDateTime
        {
            get
            {
                return this.ReleaseDeployPhases
                    .Where(r => r.JobExecution != null)
                    .Min(r => (DateTime)r.JobExecution);
            }
        }

        internal List<string> TestrunNames
        {
            get
            {
                return this.ReleaseDeployPhases
                    .Where(r => r.TestrunNames != null)
                    .SelectMany(r => r.TestrunNames)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets a failed task if it exists.
        /// </summary>
        internal string FailedTask
        {
            get
            {
                if (this.Status.Equals("succeeded", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    return string.Empty;
                }

                return this.ReleaseDeployPhases
                    .Where(r => !string.IsNullOrEmpty(r.FailedTask))
                    .Select(r => r.FailedTask)
                    .FirstOrDefault();
            }
        }
    }
}
