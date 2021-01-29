namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public partial class TestRunPipelineReference
    {
        [JsonProperty(PropertyName = "stageReference")]
        public TestRunStageReference StageReference { get; set; }
    }
}