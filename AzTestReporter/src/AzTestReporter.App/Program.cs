namespace AzTestReporter.App
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using CommandLine;
    using Microsoft.Extensions.Configuration;
    using NLog;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using AzTestReporter.BuildRelease.Builder;
    using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

    public class Program
    {
        private static Logger log;

        public static int Main(string[] args)
        {
            log = LogManager.GetCurrentClassLogger();
            log.Info("Starting TRR...");
            log.Info($"Executing version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}.");

            var parserresult = Parser.Default.ParseArguments<ReporterCommandlineOptions>(args);
            int returncode = 0;
            parserresult.WithNotParsed<ReporterCommandlineOptions>(errors =>
            {
                returncode = HandleParseErrors(errors);
            });

            parserresult.WithParsed<ReporterCommandlineOptions>(opts =>
            {
                returncode = RunOptionsandReturnExitCode(opts);
            });
                
            return returncode;
        }

        public static int RunOptionsandReturnExitCode(ReporterCommandlineOptions clOptions)
        {
            string appSettingsFile = string.Empty;

            // When running in release pipeline this will get all the environment variables.
            clOptions.ProcessPipelineEnvVars();

            ReporterConfig trrConfig = new ReporterConfig();
            appSettingsFile = "appsettings.json";

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile(appSettingsFile, optional: false)
                .Build();
            configuration.Bind(trrConfig);

            log.Trace("Transforming Commandline input and config input.");

            log.Trace("Test Run Result reporter started.");
            log.Trace($"TRR Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");

            ReportBuilderParameters startCollectionInfo = default(ReportBuilderParameters);

            try
            {
                log.Trace("Commandline options specified.", clOptions.ToDictionary());

                var processor = new InputProcessor();
                Tuple<ReportBuilderParameters, MailerParameters> tuple = processor.TransformCommandlineInput(clOptions, trrConfig);

                startCollectionInfo = tuple.Item1;
                MailerParameters mailerParameters = tuple.Item2;

                log.Trace("Merged Start Options.", startCollectionInfo.ToDictionary());
                log.Trace("Mail parameters.", mailerParameters.ToDictionary());

                StringBuilder errorMessageBuilder = new StringBuilder();

                processor.GenerateReportMail(startCollectionInfo, clOptions, ref mailerParameters);

                if ((bool)clOptions.SendMail && clOptions.OutputFormat != ReportBuilderParameters.OutputFormat.JSON)
                {
                    log.Trace("Mail subject", new Dictionary<string, string>()
                            {
                                { "MailSubject", mailerParameters.MailSubject },
                            });
                    new TestResultMailer(mailerParameters).SendTestResultEmail();
                }

                log.Info("Successfully generated report.");
            }
            catch (TestResultReportingException trrex)
            {
                log.Error($"There was a known failure when generating the reports \"{trrex.Message}\".");
                log.Trace($"Application threw an exception that is one of the recognized points of failure.");

                var tempdictionary = startCollectionInfo.ToDictionary();
                log.Trace("Application encountered a known point of failure.", new Dictionary<string, string>()
                        {
                            { "Expected failure", trrex.GetType().ToString() },
                            { "Pipeline details of the run", startCollectionInfo?.ToString() },
                        });

                log.Error(trrex);
            }
            catch (Exception ex)
            {
                log.Trace("Application threw an unexpected exception.", new Dictionary<string, string>()
                        {
                            { "Unexpected failure", ex.GetType().ToString() },
                            { "Pipeline details of the run", startCollectionInfo?.ToString() },
                        });

                log.Error(ex);
                return -1;
            }

            return 0;
        }

        private static int HandleParseErrors(IEnumerable<Error> errors)
        {
            log.Error("Invalid arugments specified");

            Debug.Assert(false, "Errors have been reported when parsing the command line arguments.");
            return -1;
        }
    }
}
