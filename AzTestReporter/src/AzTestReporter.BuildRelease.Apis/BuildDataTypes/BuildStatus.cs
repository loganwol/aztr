namespace AzTestReporter.BuildRelease.Apis
{
    public partial class BuildData
    {
        public enum BuildStatus
        {
            all,
            cancelling,
            completed,
            inProgress,
            none,
            notStarted,
            postponed,
        }
    }
}
