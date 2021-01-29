namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Validation;

    /// <summary>
    /// Aggregate class that summarizes the Module data.
    /// </summary>
    public class CodeCoverageAggregateCollection : CodeCoverageAggregate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeCoverageAggregateCollection"/> class.
        /// </summary>
        /// <param name="coverageModulesData">The list of Azure Code Coverage Module data to convert.</param>
        public CodeCoverageAggregateCollection(List<CodeCoverageModuleData> coverageModulesData)
        {
            Requires.NotNull(coverageModulesData, nameof(coverageModulesData));

            this.Name = coverageModulesData.Select(r => r.Name).FirstOrDefault();
            this.NumberofCoveredBlocks = coverageModulesData.Sum(r => r.ModuleStatistics.BlocksCovered);
            this.NumberofNotCoveredBlocks = coverageModulesData.Sum(r => r.ModuleStatistics.BlocksNotCovered);
            this.TotalBlocks = this.NumberofCoveredBlocks + this.NumberofNotCoveredBlocks;
            this.CoveredBlocksPercentage = Math.Round((this.NumberofCoveredBlocks / (double)this.TotalBlocks) * 100, 2);
            this.NotCoveredBlocksPercentage = Math.Round((NumberofNotCoveredBlocks / (double)this.TotalBlocks) * 100, 2);
        }
    }
}
