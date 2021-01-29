namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class CodeCoverageData
    {
        [JsonProperty(PropertyName = "configuration")]
        public CodeCoverageConfiguration Configuration { get; set; }

        [JsonProperty(PropertyName = "modules")]
        public CodeCoverageModulesList Modules { get; set; }

        [JsonProperty(PropertyName = "codeCoverageFileUrl")]
        public string CodeCoverageFileUrl { get; set; }
    }
}
