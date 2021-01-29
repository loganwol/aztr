namespace AzTestReporter.BuildRelease.Apis
{
    /// <summary>
    /// Represents aggregate code coverage data for one module.
    /// </summary>
    public class CodeCoverageAggregate
    {
        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of covered blocks.
        /// </summary>
        public double NumberofCoveredBlocks { get; set; }

        /// <summary>
        /// Gets or sets the number of not covered blocks.
        /// </summary>
        public double NumberofNotCoveredBlocks { get; set; }

        /// <summary>
        /// Gets or sets the total number of blocks.
        /// </summary>
        public double TotalBlocks { get; set; }

        /// <summary>
        /// Gets or sets the code coverage blocks percentage.
        /// </summary>
        public double CoveredBlocksPercentage { get; set; }

        /// <summary>
        /// Gets or sets the percentage of blocks not covered.
        /// </summary>
        public double NotCoveredBlocksPercentage { get; set; }
    }
}
