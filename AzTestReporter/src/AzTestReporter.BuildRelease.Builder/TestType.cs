namespace AzTestReporter.BuildRelease.Builder
{
    public partial class ReportBuilderParameters
    {
        /// <summary>
        /// The Test type to use when generatinig the report.
        /// </summary>
        public enum TestType
        {
            /// <summary>
            /// Unit test
            /// </summary>
            Unit,

            /// <summary>
            /// Integration test.
            /// </summary>
            Integration
        }
    }
}
