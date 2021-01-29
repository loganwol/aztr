namespace AzTestReporter.BuildRelease.Apis
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Validation;

    /// <summary>
    /// Defines collection of code coverage module data returned from DevOps API.
    /// </summary>
    public class CodeCoverageModuleDataCollection
    {
        private List<CodeCoverageData> coverageDataList;

        public CodeCoverageModuleDataCollection(
            IBuildandReleaseReader buildandReleaseReader, 
            string buildid)
        {
            Requires.NotNull(buildandReleaseReader, nameof(buildandReleaseReader));
            Requires.NotNullOrEmpty(buildid, nameof(buildid));

            AzureSuccessReponse codecoverageData = buildandReleaseReader.GetTestBuildCoverageDataAsync(buildid).GetAwaiter().GetResult();
            
            var values = codecoverageData.Value;
            this.coverageDataList = new List<CodeCoverageData>();
            for (int i = 0; i < codecoverageData.Count; i++)
            {
                this.coverageDataList.Add(JsonConvert.DeserializeObject<CodeCoverageData>(values[i].ToString()));
            }
        }

        public CodeCoverageModuleDataCollection(List<CodeCoverageData> azureCodeCoverageData)
        {
            this.coverageDataList = azureCodeCoverageData;
        }

        public List<CodeCoverageAggregateCollection> All
        {
            get
            {
                List<CodeCoverageModuleData> modulesdatalist = new List<CodeCoverageModuleData>();
                foreach (CodeCoverageData azureCodeCoverageData in this.coverageDataList)
                {
                    modulesdatalist.AddRange(azureCodeCoverageData.Modules.ToList());
                }

                return modulesdatalist.GroupBy(r => r.Name).Select(r => new CodeCoverageAggregateCollection(r.ToList())).ToList();
            }
        }

        public string CodeCoverageURL => this.coverageDataList.Select(r => r.CodeCoverageFileUrl).FirstOrDefault();
    }
}
