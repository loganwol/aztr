using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    public class RunResultSummaryDataModel
    {

        /// <summary>
        /// Gets or sets the Total number of tests executed.
        /// </summary>
        public int Total => this.Passed + this.Failed + this.NotExecuted;

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
