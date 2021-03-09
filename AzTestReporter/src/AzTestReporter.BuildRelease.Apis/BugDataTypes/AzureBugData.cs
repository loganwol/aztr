namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the JSON data structure for BugData
    /// </summary>
    public class AzureBugData
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "fields", Required = Required.Always)]
        public AzureWorkitemField Field { get; set; }

        /// <summary>
        /// Gets whom the bug is assigned to.
        /// </summary>
        public string AssignedTo
        {
            get
            {
                return this.Field?.AssignedTo?.DisplayName;
            }
        }

        /// <summary>
        /// Gets the priority of the bug.
        /// </summary>
        public int Priority
        {
            get
            {
                return !string.IsNullOrEmpty(this.Field?.Priority) ? Convert.ToInt32(this.Field?.Priority, CultureInfo.InvariantCulture) : -1;
            }
        }

        /// <summary>
        /// Gets the title of the bug.
        /// </summary>
        public string Title
        {
            get
            {
                return this.Field?.Title;
            }
        }

        /// <summary>
        /// Gets the date the bug was opened or created.
        /// </summary>
        public string CreatedDate
        {
            get
            {
                return this.Field == null ? string.Empty : this.Field.CreatedDate.ToShortDateString();
            }
        }

        /// <summary>
        /// Gets the date the bug was last changed or edited.
        /// </summary>
        public string ChangedDate
        {
            get
            {
                return this.Field == null? string.Empty : this.Field.ChangedDate.ToShortDateString();
            }
        }

        /// <summary>
        /// Gets the state of the bug.
        /// </summary>
        public string State
        {
            get
            {
                return this.Field?.State;
            }
        }

        /// <summary>
        /// Gets the Triage status of the bug.
        /// </summary>
        public string Triage
        {
            get
            {
                return this.Field?.Triage;
            }
        }

        /// <summary>
        /// Gets a friendly URL to the bug as presented in the Azure UI.
        /// </summary>
        public string Link { get; set; }
    }
}
