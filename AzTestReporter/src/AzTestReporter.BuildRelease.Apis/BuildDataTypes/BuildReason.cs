namespace AzTestReporter.BuildRelease.Apis
{
    public partial class BuildData
    {
        private enum BuildReason
        {
            all,
            batchedCI,
            buildCompletion,
            checkInShelveset,
            individualCI,
            manual,
            none,
            pullRequest,
            schedule,
            scheduleForced,
            triggered,
            userCreated,
            validateShelveset
        }
    }
}
