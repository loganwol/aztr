namespace AzTestReporter.BuildRelease.Apis
{
    public partial class Release
    {
        public enum ReleaseReason
        {
            ContinuousIntegration,
            Manual,
            None,
            PullRequest,
            Schedule
        }
    }
}
