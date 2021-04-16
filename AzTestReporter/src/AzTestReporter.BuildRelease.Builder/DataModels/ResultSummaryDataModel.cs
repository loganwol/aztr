namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AzTestReporter.BuildRelease.Apis;
    using Newtonsoft.Json;
    using Validation;

    /// <summary>
    /// Represents the summary table in the generated HTML.
    /// </summary>
    public class ResultSummaryDataModel
    {
        private bool summarizewithsubresults;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultSummaryDataModel"/> class.
        /// </summary>
        public ResultSummaryDataModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultSummaryDataModel"/> class.
        /// </summary>
        /// <param name="runsList">The runs lists result information to summarize.</param>
        public ResultSummaryDataModel(IReadOnlyList<Run> runsList, bool summarizewithsubresults = false)
        {
            Requires.NotNull(runsList, nameof(runsList));

            this.summarizewithsubresults = summarizewithsubresults;

            this.OverallResultSummaryDataModel = this.GenerateSummary(runsList, false);
            this.SubResultsSummaryDataModel = this.GenerateSummary(runsList, true);
        }

        public RunResultSummaryDataModel OverallResultSummaryDataModel { get; set; }

        public RunResultSummaryDataModel SubResultsSummaryDataModel { get; set; }

        [JsonIgnore]
        public RunResultSummaryDataModel Summary => this.summarizewithsubresults ? this.SubResultsSummaryDataModel : this.OverallResultSummaryDataModel;

        private RunResultSummaryDataModel GenerateSummary(IReadOnlyList<Run> runsList, bool summarizewithsubresults)
        {
            var currentresultsummarydatamodel = new RunResultSummaryDataModel();
            foreach (var testRunResult in runsList)
            {
                if (testRunResult.RunStatistics == null)
                {
                    continue;
                }

                if (summarizewithsubresults && testRunResult.SubTestResultStatistics.Any())
                {
                    currentresultsummarydatamodel.Passed += testRunResult.SubTestResultStatistics
                        .Where(r => r.outcome == Apis.Common.OutcomeEnum.Passed).Sum(r => r.count);
                    currentresultsummarydatamodel.Failed += testRunResult.SubTestResultStatistics
                        .Where(r => r.outcome == Apis.Common.OutcomeEnum.Failed).Sum(r => r.count);
                    currentresultsummarydatamodel.NotExecuted += testRunResult.SubTestResultStatistics
                        .Where(r => r.outcome != Apis.Common.OutcomeEnum.Failed &&
                                    r.outcome != Apis.Common.OutcomeEnum.Passed).Sum(r => r.count);
                }
                else
                {
                    currentresultsummarydatamodel.Passed += testRunResult.RunStatistics
                        .Where(r => r.outcome == Apis.Common.OutcomeEnum.Passed).Sum(r => r.count);
                    currentresultsummarydatamodel.Failed += testRunResult.RunStatistics
                        .Where(r => r.outcome == Apis.Common.OutcomeEnum.Failed).Sum(r => r.count);
                    currentresultsummarydatamodel.NotExecuted += testRunResult.RunStatistics
                        .Where(r => r.outcome != Apis.Common.OutcomeEnum.Failed &&
                                    r.outcome != Apis.Common.OutcomeEnum.Passed).Sum(r => r.count);
                }
            }

            return currentresultsummarydatamodel;
        }
    }
}
