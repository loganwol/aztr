namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class Environment
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "deploySteps")]
        public IReadOnlyList<DeployStep> DeploySteps { get; set; }

        [JsonProperty(PropertyName = "deployPhasesSnapshot")]
        public IReadOnlyList<ReleaseDeployPhasesSnapshot> DeployPhasesSnapshots { get; set; }

        /// <summary>
        /// Gets a list of agents that have the given task name.
        /// </summary>
        /// <param name="taskName">Name of the task to find.</param>
        /// <returns>A list of tasks.</returns>
        public IReadOnlyList<string> GetAgentNameFromTaskName(string taskName)
        {
            List<DeploymentTask> deploymentTaskLists = this.GetDeploymentTaskListContainingTask(taskName);
            return deploymentTaskLists?.Select(r => r.AgentName).Distinct().ToList();
        }

        /// <summary>
        /// Gets the logurl of a given task.
        /// </summary>
        /// <param name="taskName">The name of the task.</param>
        /// <returns>Return the Log url if the task is found.</returns>
        public string GetLogUrl(string taskName)
        {
            List<DeploymentTask> deploymentTaskLists = this.GetDeploymentTaskListContainingTask("Powershell");

            return deploymentTaskLists.Where(r => 
                    !string.IsNullOrEmpty(r.Name) && 
                    r.Name.ToUpperInvariant().Contains(taskName.ToUpperInvariant()) &&
                    !string.IsNullOrEmpty(r.LogUrl))
                .Select(r => r.LogUrl).FirstOrDefault();
        }

        private List<DeploymentTask> GetDeploymentTaskListContainingTask(string taskName)
        {
            List<DeploymentTask> deploymentTaskLists = new List<DeploymentTask>();
            foreach (var deployStep in DeploySteps)
            {
                var deploymentTaskList = deployStep.ReleaseDeployPhases.SelectMany(r => r.DeploymentJobs)
                    .SelectMany(r => r.Tasks)
                    .Where(r => 
                        r.SubTask != null && 
                        !string.IsNullOrEmpty(r.SubTask.Name) 
                        && r.SubTask.Name.Equals(taskName, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                if (deploymentTaskList != null)
                {
                    deploymentTaskLists.AddRange(deploymentTaskList);
                }
            }

            return deploymentTaskLists;
        }
    }
}