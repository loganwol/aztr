namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using Newtonsoft.Json;

    public class FailingSince
    {
        [JsonProperty(PropertyName = "date", Required = Required.Always)]
        public DateTime Date { get; set; }
    }
}
