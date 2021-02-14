namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class ReleaseDeployPhases
    {
        [JsonProperty(PropertyName = "deploymentJobs")]
        public List<DeploymentJobs> DeploymentJobs { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        internal DateTime? JobExecution 
        {
            get
            {
                if (this.GetTestTaskNames()?.Any() == true)
                {
                    var jobs = this.DeploymentJobs
                        .Where(r => r.ContainsTestTask)
                        .ToList();

                    return jobs
                            .Where(r => r.ContainsTestTask)
                            .Min(r => r.Job.DateStarted);
                }

                return null;
            }
        }

        internal List<string> TestrunNames
        {
            get
            {
                var jobs = this.DeploymentJobs
                    .Where(r => r.ContainsTestTask)
                    .ToList();

                if (jobs != null)
                {
                    var tasks = jobs
                        .Where(r => r.Tasks != null)
                        .SelectMany(r => r.Tasks).ToList();

                    if (tasks?.Any(r => r.SubTask?.IsTestTask == true) == true)
                    {
                        var testtasks = tasks
                            .Where(r => r.SubTask?.IsTestTask == true)
                            .Select(r => r.Name)
                            .ToList();

                        return testtasks;
                    }
                }

                return null;
            }
        }

        internal string FailedTask
        {
            get
            {
                if (this.Status.Equals("succeeded", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    return string.Empty;
                }

                var tasks = this.DeploymentJobs
                    .Where(r => r.Tasks != null)
                    .SelectMany(r => r.Tasks).ToList();

                var taskscontaintests = tasks?.Any(r => r.SubTask?.IsTestTask == true);
                if (taskscontaintests == true)
                {
                    var testtasks = tasks.Where(r => r.SubTask?.IsTestTask == true).ToList();
                    foreach(var testtask in testtasks)
                    {
                        int index = tasks.IndexOf(testtask);
                        int failureindex = tasks.FindIndex(r => r.Status.Equals("failed", System.StringComparison.InvariantCultureIgnoreCase));
                        if (failureindex > 0 && failureindex < index)
                        {
                            return tasks[failureindex].Name;
                        }
                    }
                }

                return string.Empty;
            }
        }

        private List<string> GetTestTaskNames()
        {
            var jobs = this.DeploymentJobs
                    .Where(r => r.ContainsTestTask)
                    .ToList();

            if (jobs != null)
            {
                var tasks = jobs
                    .Where(r => r.Tasks != null)
                    .SelectMany(r => r.Tasks).ToList();

                if (tasks?.Any(r => r.SubTask?.IsTestTask == true) == true)
                {
                    var testtasks = tasks
                        .Where(r => r.SubTask?.IsTestTask == true)
                        .Select(r => r.Name)
                        .ToList();

                    return testtasks;
                }
            }

            return null;
        }
    }
}
