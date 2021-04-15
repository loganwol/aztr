namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AzTestReporter.BuildRelease.Apis;
    using Validation;

    /// <summary>
    /// Represents the summary table in the generated HTML.
    /// </summary>
    public class ResultSummaryDataModel
    {
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
        public ResultSummaryDataModel(IReadOnlyList<Run> runsList)
        {
            Requires.NotNull(runsList, nameof(runsList));

            int totalNum = 0;

            foreach (var testRunResult in runsList)
            {
                if (testRunResult.RunStatistics == null)
                {
                    continue;
                }

                this.Passed += testRunResult.RunStatistics
                    .Where(r => r.outcome == Apis.Common.OutcomeEnum.Passed).Sum(r => r.count);
                this.Failed += testRunResult.RunStatistics
                    .Where(r => r.outcome == Apis.Common.OutcomeEnum.Failed).Sum(r => r.count);
                this.NotExecuted += testRunResult.RunStatistics
                    .Where(r => r.outcome != Apis.Common.OutcomeEnum.Passed && r.outcome != Apis.Common.OutcomeEnum.Failed)
                    .Sum(r => r.count);
            }

            this.Total = this.Passed + this.Failed + this.NotExecuted;
        }

        /// <summary>
        /// Gets or sets the Total number of tests executed.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the total number of tests that have passed.
        /// </summary>
        public int Passed { get; set; }

        /// <summary>
        /// Gets or sets the total number of tests that have failed.
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// Gets the total number of tests that have not have been executed.
        /// </summary>
        public int NotExecuted { get; set; }

        /// <summary>
        /// Gets the percentage of tests passing.
        /// </summary>
        public int PassRate
        {
            get
            {
                if (this.Passed + this.Failed == 0)
                {
                    return 0;
                }

                return (int)((this.Passed / (double)this.Total) * 100);
            }
        }

        /// <summary>
        /// Gets or sets the current code coverage precentage for the tests.
        /// </summary>
        public int CodeCoverage { get; set; }
    }
}
