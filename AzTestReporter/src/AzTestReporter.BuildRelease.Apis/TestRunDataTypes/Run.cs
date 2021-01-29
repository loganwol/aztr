namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Run
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "isAutomated")]
        public bool IsAutomated { get; set; }

        [JsonProperty(PropertyName = "startedDate")]
        public DateTime StartedDate { get; set; }

        public string state { get; set; }
        public int totalTests { get; set; }
        public int incompleteTests { get; set; }
        public int notApplicableTests { get; set; }
        public int passedTests { get; set; }
        public int unanalyzedTests { get; set; }
        public int revision { get; set; }
        public string webAccessUrl { get; set; }

        public List<RunStatistic> RunStatistics { get; set; }

        public Run(List<RunStatistic> runStatistics)
        {
            RunStatistics = runStatistics;
        }

        public BuildConfiguration BuildConfiguration { get; set; }

        [JsonProperty(PropertyName = "release")]
        public RunRelease Release { get; set; }

        [JsonProperty(PropertyName = "pipelineReference")]
        public TestRunPipelineReference PipelineReference { get; set; }

        public string SourceRepoName => this.PipelineReference?.StageReference?.StageName;
    }
}