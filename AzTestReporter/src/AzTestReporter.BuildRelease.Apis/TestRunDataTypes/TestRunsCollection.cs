namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;
    using Validation;

    /// <summary>
    /// Provides methods to access and aggregate runs lists.
    /// </summary>
    public class TestRunsCollection : List<Run>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunsCollection"/> class.
        /// Aggregates <see cref="AzureSuccessReponse"/> into list of <see cref="Run"/>.
        /// </summary>
        /// <param name="buildandReleaseReader"></param>
        /// <param name="buildid"></param>
        /// <param name="buildtime"></param>
        /// <param name="resultssource"></param>
        public TestRunsCollection(IBuildandReleaseReader buildandReleaseReader, DateTime buildtime, string buildid, bool resultssource)
        {
            DateTime maxDate = buildtime.AddDays(2);
            string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            AzureSuccessReponse testrunsasr = buildandReleaseReader.GetTestRunListByDateRangeAsync(minDateString, maxDateString, buildid, resultssource).GetAwaiter().GetResult();

            this.Initialize(testrunsasr);
        }

        internal TestRunsCollection(AzureSuccessReponse testrunsAsr)
        {
            this.Initialize(testrunsAsr);
        }

        private void Initialize(AzureSuccessReponse testrunsAsr)
        {
            var testruns = AzureSuccessReponse.ConvertTo<Run>(testrunsAsr);
            this.AddRange(testruns
                .Where(r => r.IsAutomated)
                .ToList());
        }

        /// <summary>
        /// Returns a List of <see cref="Run"/> matching the supplied build ids.
        /// </summary>
        /// <param name="buildIds">List of build ids.</param>
        /// <returns>List of <see cref="Run"/> matching the supplied build id.</returns>
        public List<Run> MatchedRunsByBuildIds(List<string> buildIds)
        {
            List<Run> runslist = (from returnedRuns in this
                                  from buildId in buildIds
                                  where returnedRuns.BuildConfiguration.id.ToString(CultureInfo.InvariantCulture).Equals(buildId)
                                  && returnedRuns.Release.Id.ToString(CultureInfo.InvariantCulture).Equals("0")
                                  select returnedRuns)
                        .ToList();

            // TODO: Need to return the run by attempt.
            return this.ResolveDuplicates(runslist);
        }

        /// <summary>
        /// Returns a List of <see cref="Run"/> matching the supplied Stage id.
        /// </summary>
        /// <param name="stageId">Stage Id.</param>
        /// <returns>List of <see cref="Run"/> matching the supplied Stage id.</returns>
        public List<Run> MatchedRunsbyStageandExecution(int? stageId, DateTime releasetesttaskexecutiontime)
        {
            List<Run> runslist = (from returnedRuns in this
                                  where returnedRuns.Release.EnvironmentId == stageId
                                    && DateTime.Compare(releasetesttaskexecutiontime, returnedRuns.StartedDate) < 0
                                  select returnedRuns)
                    .ToList();

            return this.ResolveDuplicates(runslist);
        }

        /// <summary>
        /// Removes duplicate test runs from a <see cref="Run"/> List.
        /// </summary>
        /// <param name="runslist">List of <see cref="Run"/>.</param>
        /// <returns>Deduplicated List of <see cref="Run"/>.</returns>
        private List<Run> ResolveDuplicates(List<Run> runslist)
        {
            List<Run> matchedRuns = new List<Run>();
            List<int> runids = new List<int>();
            foreach (var run in runslist)
            {
                if (runids.Contains(run.Id) == false)
                {
                    matchedRuns.Add(run);
                    runids.Add(run.Id);
                }
            }

            return matchedRuns;
        }
    }
}