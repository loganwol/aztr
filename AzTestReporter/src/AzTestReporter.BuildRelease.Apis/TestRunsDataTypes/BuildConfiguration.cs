namespace AzTestReporter.BuildRelease.Apis
{
    public class BuildConfiguration
    {
        public int id { get; set; }
        public string number { get; set; }
        public string uri { get; set; }
        public string flavor { get; set; }
        public string platform { get; set; }
        public int buildDefinitionId { get; set; }
        public string branchName { get; set; }
        
        public string targetBranchName { get; set; }
    }
}