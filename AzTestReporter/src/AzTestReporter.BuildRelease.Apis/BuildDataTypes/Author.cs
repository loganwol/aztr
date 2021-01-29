namespace AzTestReporter.BuildRelease.Apis
{
    using Newtonsoft.Json;

    public class Author
    {
        [JsonProperty(PropertyName = "displayName", Required = Required.Always)]
        public string DisplayName { get; set; }

        public Link Links { get; set; }

        public object Id { get; set; }

        public string UniqueName { get; set; }
    }
}
