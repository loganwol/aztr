namespace AzTestReporter.App
{
    using System.Collections.Generic;
    using System.Text;
    using CommandLine;
    using AzTestReporter.BuildRelease.Builder;
    using static AzTestReporter.BuildRelease.Builder.ReportBuilderParameters;

    public class ReporterCommandlineOptions
    {
        private Dictionary<string, string> currentvalues;

        [Option("trt", HelpText = "Specify the test run type, supported types are - (Unit|Integration)")]
        public TestType TestRunType { get; set; }

        [Option("sendmail", HelpText = "Set this to false when debugging or when trying out the tool and not sending mail inadvertently.", Default = true)]
        public bool? SendMail { get; set; }

        [Option("mailserver", HelpText = "The mail server to use when sending mail containing the test results.")]
        public string SmtpServer { get; set; }

        [Option("mailaccount", HelpText = "The mail account to send mail.")]
        public string MailAccount { get; set; }

        [Option("mailpwd", HelpText = "The mail account password to use when sending mail.")]
        public string MailPassword { get; set; }

        [Option("sendto", HelpText = "List of people or groups mail needs to be sent to. This is a comma delimited list")]
        public string SendTo { get; set; }

        [Option("cc", HelpText = "List of people or groups to cc in mail sent. This is a comma delimited list")]
        public string CC { get; set; }

        [Option('v', HelpText = "Enable verbose debug output.", Hidden = true, Default = false)]
        public bool Verbose { get; set; }

        [Option('o', HelpText = "Output directory path.")]
        public string OutputDirectory { get; set; }
		
        [Option('f', HelpText = "Specify the output format of the report (options are HTML|JSON|All)", Default = OutputFormat.HTML)]
        public OutputFormat OutputFormat { get; set; }

        [Option('s', HelpText = "Show summarized sub results")]
        public bool ShowSummarizedSubResults { get; set; }

        public AzurePipelineEnvironmentOptions PipelineEnvironmentOptions { get; set; }

        public void ProcessPipelineEnvVars()
        {
            this.PipelineEnvironmentOptions = new AzurePipelineEnvironmentOptions();
            this.PipelineEnvironmentOptions.Read(this.TestRunType == TestType.Integration);
        }

        public override string ToString()
        {
            this.currentvalues = new Dictionary<string, string>()
            {
                { "SendMail                   ", this.SendMail?.ToString() },
                { "Mail password specified    ", (!string.IsNullOrEmpty(this.MailPassword)).ToString() },
                { "CC                         ", this.CC },
                { "Verbose logging enabled    ", this.Verbose.ToString() },
            };

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\t\tCommandline Arguments ----------------- ");
            foreach (var key in currentvalues.Keys)
            {
                stringBuilder.AppendLine($"\t{key}: {currentvalues[key]}");
            }

            return stringBuilder.ToString();
        }

        public Dictionary<string, string> ToDictionary()
        {
            _ = this.ToString();
            return this.currentvalues;
        }
    }
}
