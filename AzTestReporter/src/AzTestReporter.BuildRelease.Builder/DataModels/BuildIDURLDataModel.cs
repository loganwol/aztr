namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    public class BuildIDURLDataModel
    {
        public int ID { get; }

        public string Url { get; }

        public BuildIDURLDataModel(int releaseid, int environmentid, string url)
        {
            this.ID = releaseid;
        }
    }
}
