namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using Newtonsoft.Json;

    public class Link
    {
        [JsonProperty(PropertyName = "web", Required = Required.Always)]
        public Web Web { get; set; }
    }
}
