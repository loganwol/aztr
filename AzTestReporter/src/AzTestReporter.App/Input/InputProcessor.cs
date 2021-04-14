namespace AzTestReporter.App
{
    using System;
    using System.IO;
    using System.Text;
    using AutoMapper;
    using NLog;
    using AzTestReporter.BuildRelease.Apis;
    using AzTestReporter.BuildRelease.Builder;
    using Validation;
	using System.Linq;
    using AzTestReporter.BuildRelease.Builder.DataModels;
    using System.Reflection;

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

            var appfullpath = AppDomain.CurrentDomain.BaseDirectory;

            if (string.IsNullOrEmpty(appfullpath))
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null)
                {
                    assembly = Assembly.GetExecutingAssembly();
                }

                if (!assembly.Location.StartsWith(Path.GetTempPath()))
                {
                    appfullpath = Path.GetDirectoryName(Path.GetFullPath(assembly.Location));
                }
            }

            string outputdirectory = clOptions.OutputDirectory;
            if (string.IsNullOrEmpty(outputdirectory))
            {
                outputdirectory = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(outputdirectory))
            {
                Log?.Info("Creating output directory.");
                Directory.CreateDirectory(outputdirectory);
            }

            if (clOptions.OutputFormat != ReportBuilderParameters.OutputFormat.JSON)
            {
                string reportBody = reportBuilder.ToHTML();
                string emailsubject = reportBuilder.Title;

                StringBuilder outputfilename = new StringBuilder();

                if (reportBuilder.IsPipelineFailed)
                {
                    outputfilename.Append("ExecutionFailuresReport");
                }
                else
                {
                    outputfilename.Append("TestExecutionReport");
                }

                if (!reportBuilderParameters.ResultSourceIsBuild)
                {
                    outputfilename.Append($"-{reportBuilderParameters.PipelineEnvironmentOptions.ReleaseExecutionStage}");
                }

                if (reportBuilderParameters.PipelineEnvironmentOptions.ReleaseAttempt > 0)
                {
                    outputfilename.Append($"-Attempt{reportBuilderParameters.PipelineEnvironmentOptions.ReleaseAttempt}");
                }

                outputfilename.Append($"-{reportBuilderParameters.PipelineEnvironmentOptions.BuildNumber}");

                if (clOptions.TestRunType == ReportBuilderParameters.TestType.Unit)
                {
                    outputfilename.Append($"-ExecutionID{reportBuilderParameters.PipelineEnvironmentOptions.BuildID}");
                }
                else
                {
                    outputfilename.Append($"-ExecutionID{reportBuilderParameters.PipelineEnvironmentOptions.ReleaseID}");
                }

                outputfilename.Append(".html");

                string outputfilepath = Path.Combine(outputdirectory, outputfilename.ToString());

	            if (File.Exists(outputfilepath))
	            {
	                Log?.Info("Found previous report file, deleting the file.");
	                File.Delete(outputfilepath);
	            }
				
                File.WriteAllText(outputfilepath, reportBody);

                Log?.Info($"Successfully generated \"{outputfilepath}\".");
                Log?.Trace("Generated HTML successfully");

                if (!string.IsNullOrWhiteSpace(clOptions.OutputDirectory))
                {
                    var debugdirectory = Path.Combine(appfullpath, "Debug");
                    if (!Directory.Exists(debugdirectory))
                    {
                        Directory.CreateDirectory(debugdirectory);
                    }

                    var aztrfiles = Directory.GetFiles(appfullpath, "aztr-*.json");
                    Log?.Info($"Moving json files generated \"{aztrfiles.Length}\".");

                    foreach (var file in aztrfiles)
                    {
                        Log?.Trace($"Moving file {file} to destination.");
                        File.Move(file, Path.Combine(debugdirectory, Path.GetFileName(file)));
                    }
                }

                mailerParameters.MailSubject = emailsubject;
                mailerParameters.MailBody = reportBody;

                if (clOptions.SendMail != null && (bool)clOptions.SendMail)
                {
                    if (reportBuilder.IsPipelineFailed)
                    {
                        mailerParameters.To = string.Join(",", mailerParameters.FailureSendToList);
                    }
                }
            }
            
            if (clOptions.OutputFormat != ReportBuilderParameters.OutputFormat.HTML)
            {
                string json = reportBuilder.ToJson();
                if (!string.IsNullOrEmpty(json))
                {
                    StringBuilder outputfilename = new StringBuilder();

                    if (clOptions.TestRunType == ReportBuilderParameters.TestType.Unit)
                    {
                        outputfilename.Append($"{reportBuilderParameters.PipelineEnvironmentOptions.BuildID}");
                    }
                    else
                    {
                        outputfilename.Append($"{reportBuilderParameters.PipelineEnvironmentOptions.ReleaseID}");
                    }

                    outputfilename.Append($"-TestResults.json");
                    Log?.Info($"Outputting json file to {outputfilename.ToString()}.");
                    File.WriteAllText(Path.Combine(outputdirectory, outputfilename.ToString()), json);
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
