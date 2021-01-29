namespace AzTestReporter.BuildRelease.Builder
{
    using System.Collections.Generic;
    using System.Text;

    public class ReportBuilderParameters
    {
        private Dictionary<string, string> currentValues;

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

        /// <summary>
        /// Gets or sets the Organization name to use to construct the Azure REST API URI.
        /// </summary>
        public string AzureOrganizationCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if the current Result source 
        /// is from Build pipeline or release pipeline.
        /// </summary>
        public bool ResultSourceIsBuild { get; set; }

        /// <summary>
        /// Gets or sets the list of users the report needs to be sent to.
        /// </summary>
        public string SendTo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if this is a private release.
        /// </summary>
        public bool IsPrivateRelease { get; set; }

        /// <summary>
        /// Gets or sets the Azure Pipeline variables object.
        /// </summary>
        public AzurePipelineEnvironmentOptions PipelineEnvironmentOptions { get; set; }
        
        /// <summary>
        /// Gets or sets the Pool of agents to look at information for.
        /// </summary>
        public List<string> AgentPools { get; set; }

        public override string ToString()
        {
            this.currentValues = new Dictionary<string, string>() 
            {
                { "Result source is a build   ", this.ResultSourceIsBuild.ToString() },
                { "Agent Pools                ", this.AgentPools == null ? string.Empty : string.Join(", ", this.AgentPools) },
            };

            if (this.PipelineEnvironmentOptions != null)
            {
                var pipelinedict = this.PipelineEnvironmentOptions.ToDictionary();
                foreach (var pipelinearg in pipelinedict.Keys)
                {
                    this.currentValues.Add(pipelinearg, pipelinedict[pipelinearg]);
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\t\t********* INPUT PARAMETERS FOR TRR *********");
            foreach (var key in currentValues.Keys)
            {
                stringBuilder.AppendLine($"\t{key}:{currentValues[key]}");
            }

            return stringBuilder.ToString();
        }

        public Dictionary<string, string> ToDictionary()
        {
            _ = this.ToString();
            return this.currentValues;
        }
    }
}
