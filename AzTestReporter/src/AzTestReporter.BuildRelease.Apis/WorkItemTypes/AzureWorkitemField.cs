namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the JSON data structure of a field object
    /// in places where it's used, used in work item and bud fields.
    /// </summary>
    public class AzureWorkitemField
    {
        /// <summary>
        /// Gets or sets the Work item assigned to property.
        /// </summary>
        [JsonProperty(PropertyName = "System.AssignedTo")]
        public AzureWorkItemPersonField AssignedTo { get; set; }

        /// <summary>
        /// Gets or sets the Work item priority.
        /// </summary>
        [JsonProperty(PropertyName = "Microsoft.VSTS.Common.Priority")]
        public string Priority { get; set; }

        /// <summary>
        /// Gets or sets the Work item title.
        /// </summary>
        [JsonProperty(PropertyName = "System.Title", Required = Required.Always)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Work item date created.
        /// </summary>
        [JsonProperty(PropertyName = "System.CreatedDate", Required = Required.Always)]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the Work item changed date.
        /// </summary>
        [JsonProperty(PropertyName = "System.ChangedDate", Required = Required.Always)]
        public DateTime ChangedDate { get; set; }

        /// <summary>
        /// Gets or sets the Work item State.
        /// </summary>
        [JsonProperty(PropertyName = "System.State", Required = Required.Always)]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the Work item iteration path.
        /// </summary>
        [JsonProperty(PropertyName = "System.IterationPath")]
        public string SystemIterationPath { get; set; }

        /// <summary>
        /// Gets or sets the Work item resolution reason.
        /// </summary>
        [JsonProperty(PropertyName = "Microsoft.VSTS.Common.ResolvedReason")]
        public string ResolvedReason { get; set; }

        /// <summary>
        /// Gets or sets the Work item finish date.
        /// </summary>
        [JsonProperty(PropertyName = "Microsoft.VSTS.Scheduling.FinishDate")]
        public string MicrosoftVSTSSchedulingFinishDate { get; set; }

        /// <summary>
        /// Gets or sets the Work item triage field.
        /// </summary>
        [JsonProperty(PropertyName = "Microsoft.VSTS.Common.Triage")]
        public string Triage { get; set; }

        /// <summary>
        /// Gets or sets the Work item type.
        /// </summary>
        [JsonProperty(PropertyName = "System.WorkItemType")]
        public string WorkItemType { get; set; }

        /// <summary>
        /// Gets or sets the Area path of the Work item.
        /// </summary>
        [JsonProperty(PropertyName = "System.AreaPath")]
        public string AreaPath { get; set; }

        /// <summary>
        /// Gets the resolved by fields in azure. As sometimes
        /// the field can be directly moved to closed, we try to
        /// get either, with closed being first priority.
        /// </summary>
        public string ResolvedBy
        {
            get
            {
                if (this.MicrosoftVSTSCommonResolvedBy == null && this.MicrosoftVSTSCommonClosedBy != null ||
                    this.MicrosoftVSTSCommonResolvedBy != null && this.MicrosoftVSTSCommonClosedBy != null)
                {
                    return this.MicrosoftVSTSCommonClosedBy.DisplayName;
                }
                else if (this.MicrosoftVSTSCommonResolvedBy != null && this.MicrosoftVSTSCommonClosedBy == null)
                {
                    return this.MicrosoftVSTSCommonResolvedBy.DisplayName;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the resolved date fields in azure. As sometimes
        /// the field can be directly moved to closed, we try to
        /// get either, with closed being first priority.
        /// </summary>
        public string ResolvedDate
        {
            get
            {
                if (string.IsNullOrEmpty(this.MicrosoftVSTSCommonResolvedDate) && !string.IsNullOrEmpty(this.MicrosoftVSTSCommonClosedDate) ||
                    !string.IsNullOrEmpty(this.MicrosoftVSTSCommonResolvedDate) && !string.IsNullOrEmpty(this.MicrosoftVSTSCommonClosedDate))
                {
                    return Convert.ToDateTime(this.MicrosoftVSTSCommonClosedDate, CultureInfo.InvariantCulture).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                }
                else if (!string.IsNullOrEmpty(this.MicrosoftVSTSCommonResolvedDate) && string.IsNullOrEmpty(this.MicrosoftVSTSCommonClosedDate))
                {
                    return Convert.ToDateTime(this.MicrosoftVSTSCommonResolvedDate, CultureInfo.InvariantCulture).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the Work item resolved by.
        /// </summary>
        [JsonProperty(PropertyName = "Microsoft.VSTS.Common.ResolvedBy")]
        internal AzureWorkItemPersonField MicrosoftVSTSCommonResolvedBy { get; set; }

        /// <summary>
        /// Gets or sets the Work item resolved date.
        /// </summary>
        [JsonProperty(PropertyName = "Microsoft.VSTS.Common.ResolvedDate")]
        internal string MicrosoftVSTSCommonResolvedDate { get; set; }

        /// <summary>
        /// Gets or sets the Work item closed by.
        /// </summary>
        [JsonProperty(PropertyName = "Microsoft.VSTS.Common.ClosedBy")]
        internal AzureWorkItemPersonField MicrosoftVSTSCommonClosedBy { get; set; }

        /// <summary>
        /// Gets or sets the Work item closed date.
        /// </summary>
        [JsonProperty(PropertyName = "Microsoft.VSTS.Common.ClosedDate")]
        internal string MicrosoftVSTSCommonClosedDate { get; set; }
    }
}
