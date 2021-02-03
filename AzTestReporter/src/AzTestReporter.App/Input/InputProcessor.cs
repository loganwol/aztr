﻿namespace AzTestReporter.App
{
    using System;
    using System.IO;
    using System.Text;
    using AutoMapper;
    using NLog;
    using AzTestReporter.BuildRelease.Apis;
    using AzTestReporter.BuildRelease.Builder;
    using Validation;

    public class InputProcessor
    {
        internal static Logger Log;

        private static Mapper configMapper;

        internal static Mapper ConfigMapper
        {
            get
            {
                if (configMapper == null)
                {
                    MapperConfiguration mapperconfig = new MapperConfiguration(cnf =>
                    {
                        cnf.CreateMap<AzurePipelineEnvironmentOptions, ReporterCommandlineOptions>();

                        cnf.CreateMap<ReporterCommandlineOptions, ReportBuilderParameters>()
                            .ForMember(dest => dest.SendTo, src => src.Ignore());

                        cnf.CreateMap<ReporterConfig, MailerParameters>()
                            .ForMember(dest => dest.FailureSendToList, act => act.MapFrom(src => src.FailuresSendToList))
                            .ForMember(dest => dest.To, src => src.Ignore())
                            .ForMember(dest => dest.CC, src => src.Ignore())
                            .ForMember(dest => dest.MailAccountPassword, src => src.Ignore());
                    });

                    configMapper = new Mapper(mapperconfig);
                }

                return configMapper;
            }
        }

        public Tuple<ReportBuilderParameters, MailerParameters> TransformCommandlineInput(ReporterCommandlineOptions commandlineOptions, ReporterConfig config)
        {
            Requires.NotNull(commandlineOptions, nameof(commandlineOptions));
            Requires.NotNull(config, nameof(config));

            Log = LogManager.GetCurrentClassLogger();

            Log.Info("**************** Input Values *******************");
            Log.Info(commandlineOptions.ToString());
            Log.Info(string.Empty);
            Log.Info(config.ToString());
            Log.Info(string.Empty);
            Log.Info("*************************************************");

            ReportBuilderParameters builderParameters = ConfigMapper.Map<ReporterCommandlineOptions, ReportBuilderParameters>(commandlineOptions);
            builderParameters.ResultSourceIsBuild = commandlineOptions.TestRunType == ReportBuilderParameters.TestType.Unit;
            builderParameters.PipelineEnvironmentOptions = commandlineOptions.PipelineEnvironmentOptions;
            
            builderParameters.AzureOrganizationCollection = commandlineOptions.PipelineEnvironmentOptions.SystemTeamProject;

            MailerParameters mailerParameters = ConfigMapper.Map<ReporterConfig, MailerParameters>(config);

            if (commandlineOptions.SendMail == true)
            {
                mailerParameters.To = commandlineOptions.SendTo;
                mailerParameters.CC = commandlineOptions.CC;

                builderParameters.SendTo = mailerParameters.To;
                mailerParameters.MailAccountPassword = commandlineOptions.MailPassword;
            }

            Log.Info("*************************************************");
            Log.Info(builderParameters.ToString());
            Log.Info(string.Empty);
            Log.Info(mailerParameters.ToString());
            Log.Info(string.Empty);
            Log.Info(builderParameters.PipelineEnvironmentOptions.ToString());
            Log.Info("*************************************************");

            return new Tuple<ReportBuilderParameters, MailerParameters>(builderParameters, mailerParameters);
        }

        public void GenerateReportMail(
            ReportBuilderParameters reportBuilderParameters,
            ReporterCommandlineOptions clOptions,
            ref MailerParameters mailerParameters)
        {
            Requires.NotNull(reportBuilderParameters, nameof(reportBuilderParameters));
            Requires.NotNull(clOptions, nameof(clOptions));
            Requires.NotNull(mailerParameters, nameof(mailerParameters));
            Requires.NotNull(reportBuilderParameters.PipelineEnvironmentOptions, nameof(reportBuilderParameters.PipelineEnvironmentOptions));
            Requires.NotNull(reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamFoundationServerURI, nameof(reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamFoundationServerURI));
            Requires.NotNull(reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamFoundationCollectionURI, nameof(reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamFoundationCollectionURI));
            Requires.NotNull(reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamProject, nameof(reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamProject));

            IBuildandReleaseReader devWorkReader = new BuildandReleaseReader(
                reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamFoundationServerURI,
                reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamFoundationCollectionURI,
                reportBuilderParameters.PipelineEnvironmentOptions.SystemTeamProject,
                clOptions.Verbose);

            Log?.Info("Starting report builder");

            DailyHTMLReportBuilder reportBuilder = new ReportBuilder(devWorkReader).GetReleasesRunsandResults(ref reportBuilderParameters);

            Log?.Info("Report builder finished successfully.");

            if (reportBuilderParameters.IsPrivateRelease)
            {
                mailerParameters.To = reportBuilderParameters.SendTo;
                mailerParameters.ExecutionType = ExecutionType.Private;
            }

            if (!reportBuilder.IsPipelineFailed)
            {
                reportBuilder.CheckResults();
            }

            // As the SendTo on start collection hasn't been initialized, it's easier to check for 
            // null in the case of a private build and set it.
            if (reportBuilderParameters.IsPrivateRelease)
            {
                mailerParameters.To = reportBuilderParameters.SendTo;
            }

            string reportBody = reportBuilder.ToHTML();
            string emailsubject = reportBuilder.Title;

            StringBuilder outputfilename = new StringBuilder($"TestExecutionReport-");
            outputfilename.Append($"{reportBuilderParameters.PipelineEnvironmentOptions.ReleaseExecutionStage}");
            outputfilename.Append($"-Attempt{reportBuilderParameters.PipelineEnvironmentOptions.ReleaseAttempt}");
            outputfilename.Append($"-{reportBuilderParameters.PipelineEnvironmentOptions.BuildNumber}.html");
            string outputfilepath = Path.Combine(Directory.GetCurrentDirectory(), outputfilename.ToString());
            File.WriteAllText(outputfilepath, reportBody);

            Log?.Info($"Successfully generated {outputfilename}.");
            Log?.Trace("Generated HTML successfully");

            mailerParameters.MailSubject = emailsubject;
            mailerParameters.MailBody = reportBody;

            if (clOptions.SendMail != null && (bool)clOptions.SendMail)
            {
                if (reportBuilder.IsPipelineFailed)
                {
                    mailerParameters.To = string.Join(",", mailerParameters.FailureSendToList);
                }
            }

            mailerParameters.Type = MessageType.Success;
        }

        private string OutputCommandlineOptions(ReporterCommandlineOptions clOptions, string errorMessageTitle)
        {
            var errorMessageBuilder = new StringBuilder();
            errorMessageBuilder.AppendLine(errorMessageTitle);
            errorMessageBuilder.AppendLine();
            errorMessageBuilder.AppendLine("List of all the commandline options used in the execution of the tool.");
            errorMessageBuilder.AppendLine();
            errorMessageBuilder.AppendLine(clOptions.ToString());

            return errorMessageBuilder.ToString();
        }

        private string InputandExceptionInformation(ReporterCommandlineOptions clOptions, MailerParameters mailerParameters, Exception ex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (ex != null)
            {
                stringBuilder.AppendLine(ex.Message);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(ex.StackTrace);
                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine(clOptions.ToString());
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(mailerParameters.ToString());
            return stringBuilder.ToString();
        }

        private void WriteErrortoConsole(string message)
        {
            ConsoleColor previous = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log.Info(message);
            Console.ForegroundColor = previous;
        }
    }
}