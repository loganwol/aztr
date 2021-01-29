namespace AzTestReporter.App
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Validation;

    public class MailerParameters
    {
        private Dictionary<string, string> currentvalues;

        /// <summary>
        /// Gets or sets the email account the mail is sent from.
        /// </summary>
        public string MailAccount { get; set; }

        /// <summary>
        /// Gets or sets the mail server the mail is sent from.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets the mail account password used to send mail.
        /// </summary>
        public string MailAccountPassword { get; set; }

        public string To { get; set; }

        public string CC { get; set; }

        internal List<string> SendToList { get; set; }

        internal List<string> CCList { get; set; }

        public List<string> FailureSendToList { get; set; }

        public List<string> ReleaseSendToList { get; set; }

        public string MailSubject { get; set; }

        public string MailBody { get; set; }

        public MessageType Type { get; set; }

        public ExecutionType ExecutionType { get; internal set; }

        public void ValidateAllParameters()
        {
            Requires.NotNullOrEmpty(this.SmtpServer, nameof(this.SmtpServer));
            Requires.NotNullOrEmpty(this.MailAccount, nameof(this.MailAccount));
            Requires.NotNullOrEmpty(this.MailAccountPassword, nameof(this.MailAccountPassword));
            Requires.NotNullOrEmpty(this.To, nameof(this.To));

            this.ValidateMailAddress(this.MailAccount);

            this.SendToList = this.To.Split(new char[] { ',' }).ToList().Select(r => r.Trim()).ToList();
            this.SendToList.ForEach(r => this.ValidateMailAddress(r));

            if (!string.IsNullOrEmpty(this.CC))
            {
                this.CCList = this.CC.Split(new char[] { ',' }).ToList().Select(r => r.Trim()).ToList();
                this.CCList.ForEach(r => this.ValidateMailAddress(r));
            }
        }

        public override string ToString()
        {
            currentvalues = new Dictionary<string, string>()
            {
                { "Smtp server            ", this.SmtpServer },
                { "Mail from address      ", this.MailAccount },
                { "Mail password is set   ", (!string.IsNullOrEmpty(this.MailAccountPassword)).ToString() },
                { "Send To List           ", this.To },
                { "CC List                ", this.CCList != null ? string.Join(";", this.CCList.ToArray()) : string.Empty },
                { "Failure Send to list   ", this.FailureSendToList != null ? string.Join(";", this.FailureSendToList.ToArray()) : string.Empty }
            };

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\t\t****** MAILER PARAMETERS****** ");

            foreach (var key in currentvalues.Keys)
            {
                stringBuilder.AppendLine($"\t{key}: {currentvalues[key]}");
            }

            stringBuilder.AppendLine("*******************************************");

            return stringBuilder.ToString();
        }

        public Dictionary<string, string> ToDictionary()
        {
            _ = this.ToString();
            return this.currentvalues;
        }

        private void ValidateMailAddress(string mailaddress)
        {
            Requires.NotNullOrEmpty(mailaddress, nameof(mailaddress));

            string mailpattern = @"^(|([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+([,.](([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$";
            Requires.ValidState(Regex.Match(mailaddress, mailpattern).Success, $"Invalid email address [{mailaddress}] specified.");
            Requires.ValidState(mailaddress.Where(r => r == '\"').Count() == 0, $"Email address contains quotes, which is an invalid email address.");
        }
    }
}
