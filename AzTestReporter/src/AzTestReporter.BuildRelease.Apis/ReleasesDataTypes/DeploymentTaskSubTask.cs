namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class DeploymentTaskSubTask
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        internal bool IsTestTask => this.Name.Equals("VSTest", System.StringComparison.InvariantCultureIgnoreCase);
    }
}
