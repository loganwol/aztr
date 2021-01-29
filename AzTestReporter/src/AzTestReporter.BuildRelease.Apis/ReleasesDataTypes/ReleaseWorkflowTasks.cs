namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class ReleaseWorkflowTasks
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "taskId")]
        public string TaskID { get; set; }

        [JsonProperty(PropertyName = "inputs")]
        public ReleaseWorkflowTasksInputs Inputs { get; set; }
    }
}