namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System;
    using System.Collections.Generic;
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

                int subFailed = 0;
                int subPassed = 0;

                foreach (RunStatistic stat in testRunResult.RunStatistics)
                {
                    if (stat.outcome.Equals("Passed", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = int.TryParse(stat.count.ToString(), out subPassed);
                        continue;
                    }

                    if (stat.outcome.Equals("Failed", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = int.TryParse(stat.count.ToString(), out subFailed);
                        continue;
                    }
                }

                totalNum += Convert.ToInt32(testRunResult.totalTests);
                this.Passed += subPassed;
                this.Failed += subFailed;
            }

            this.Total = totalNum;
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
        public int NotExecuted => this.Total - (this.Passed + this.Failed);

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
