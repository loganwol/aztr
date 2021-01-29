namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class ReleaseArtifact
    {
        [JsonProperty(PropertyName = "definitionReference", Required = Required.Always)]
        public ReleaseDefinition DefinitionReference { get; set; }

        [JsonProperty(PropertyName = "isPrimary")]
        public bool IsPrimary { get; set; }
    }
}
