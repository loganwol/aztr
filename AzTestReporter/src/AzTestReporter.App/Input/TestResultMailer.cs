namespace AzTestReporter.App
{
    using System;
    using System.Diagnostics;
    using System.Net.Mail;
    using System.Reflection;
    using System.Text;
    using Validation;

    public class TestResultMailer
    {
        private MailerParameters mailerParameters;

        public TestResultMailer(MailerParameters mailerParameters)
        {
            Requires.NotNull(mailerParameters, nameof(mailerParameters));

            this.mailerParameters = mailerParameters;
            this.mailerParameters.ValidateAllParameters();
        }

        public void SendTestResultEmail()
        {
            if (Debugger.IsAttached)
            {
                Debug.Assert(false, "Do you really want to send email! If yes, hit continue in the Assert dialog.");
            }

            SmtpClient smtpClient;
            MailMessage mailMsg = this.BuildMail(out smtpClient);
            smtpClient.Send(mailMsg);
            smtpClient.Dispose();

            Console.WriteLine($"Sending test result email for '{mailMsg.Subject}' from '{mailMsg.From}'");
            Console.WriteLine($" Sending to '{string.Join(", ", mailMsg.To)}'");
            Console.WriteLine($" CC'ing to '{string.Join(", ", mailMsg.CC)}'");
            Console.WriteLine("Sent mail message.");
        }

        internal MailMessage BuildMail(out SmtpClient smtpClient)
        {
            if (mailerParameters.Type == MessageType.Success)
            {
                Requires.NotNull(mailerParameters.MailSubject, nameof(mailerParameters.MailSubject));
                Requires.NotNull(mailerParameters.MailBody, nameof(mailerParameters.MailBody));
            }

            smtpClient = new SmtpClient();
            smtpClient.Host = mailerParameters.SmtpServer;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential(
                mailerParameters.MailAccount, mailerParameters.MailAccountPassword);
            MailMessage mailMsg = new MailMessage();
            string mailsubject;

            mailMsg.From = new MailAddress(mailerParameters.MailAccount);
            foreach (string recipient in mailerParameters.SendToList)
            {
                mailMsg.To.Add(recipient.Trim());
            }

            if (mailerParameters.CCList != null)
            {
                foreach (string ccRecipient in mailerParameters.CCList)
                {
                    mailMsg.CC.Add(ccRecipient.Trim());
                }
            }

            if (mailerParameters.Type != MessageType.Success)
            {
                if (mailerParameters.Type == MessageType.UnexpectedFailure)
                {
                    mailsubject = mailerParameters.MailSubject;
                    if (string.IsNullOrEmpty(mailerParameters.MailSubject))
                    {
                        mailsubject = "Unexpected failure found when generating test results";
                    }

                    mailMsg.To.Clear();
                    foreach (string recipient in mailerParameters.FailureSendToList)
                    {
                        mailMsg.To.Add(recipient.Trim());
                    }

                    mailMsg.CC.Clear();
                }
                else
                {
                    mailsubject = "Expected error found when generating results mail.";
                }

                mailMsg.Body = $"{GetExecutionInformation()}\r\n{mailerParameters.MailBody}";
            }
            else
            {
                mailsubject = mailerParameters.MailSubject;
                mailMsg.IsBodyHtml = true;
                mailMsg.Body = mailerParameters.MailBody;

                if (mailerParameters.ExecutionType == ExecutionType.Private)
                {
                    mailsubject = $"(Private Release) - {mailsubject}";
                    mailMsg.CC.Clear();

                    foreach (string recipient in mailerParameters.ReleaseSendToList)
                    {
                        mailMsg.CC.Add(recipient.Trim());
                    }
                }
            }

            mailMsg.Subject = mailsubject;

            return mailMsg;
        }

        public static string GetExecutionInformation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"User name: {Environment.GetEnvironmentVariable("Username")}");
            stringBuilder.AppendLine($"Machine name : {Environment.GetEnvironmentVariable("computername")}");
            stringBuilder.AppendLine($"Tool version : {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            stringBuilder.AppendLine();

            return stringBuilder.ToString();
        }
    }
}
