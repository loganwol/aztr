namespace AzTestReporter.App
{
    using System.Collections.Generic;

    public class ReporterConfig
    {
        public string AzureOrganizationCollection { get; set; }

        public string MailerAccount { get; set; }

        public List<string> FailuresSendToList { get; set; }
    }
}
