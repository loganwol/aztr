namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    
    public class ReleaseDeployPhasesSnapshot
    {
        [JsonProperty(PropertyName = "workflowTasks")]
        public List<ReleaseWorkflowTasks> ReleaseWorkflowTasks { get; set; }
    }
}