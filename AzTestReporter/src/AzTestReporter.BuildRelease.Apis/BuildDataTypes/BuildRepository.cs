namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class BuildRepository
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string RepositoryName { get; set; }
    }
}
