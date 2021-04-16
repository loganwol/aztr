using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AzTestReporter.App.Test.Integration
{
    public class AzTestReporterExecutableIntegrationTests
    {
        [Fact]
        public void Can_successfully_run_reporter_for_integration_test_in_build_generating_only_html_output()
        {
            string[] reportfiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "TestExecutionReport-*-ExecutionID*.html");
            foreach(var reportfile in reportfiles)
            {
                File.Delete(reportfile);
            }

            string[] args = new string[4]
            {
                "--trt",
                "Integration",
                "--sendmail",
                "false"
            };

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            Task.Run(() =>
            {
                Program.Main(args);
                manualResetEvent.Set();
            });

            manualResetEvent.WaitOne((int)TimeSpan.FromMinutes(2).TotalMilliseconds, false);

            Thread.Sleep(TimeSpan.FromMilliseconds(5000));

            Environment.ExitCode.Should().Be(0);
            StringBuilder outputfile = new StringBuilder("TestExecutionReport-");
            outputfile.Append(Environment.GetEnvironmentVariable("RELEASE_ENVIRONMENTNAME"));
            outputfile.Append("-Attempt1-");
            outputfile.Append($"{Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER")}");
            outputfile.Append("-ExecutionID");
            outputfile.Append($"{Environment.GetEnvironmentVariable("RELEASE_RELEASEID")}.html");

            File.Exists(outputfile.ToString()).Should().BeTrue();
            File.Delete(outputfile.ToString());
        }

        [Fact]
        public void Can_successfully_run_reporter_for_integration_test_in_build_generating_only_json_output()
        {
            string[] reportfiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*-TestResults.json");
            foreach (var reportfile in reportfiles)
            {
                File.Delete(reportfile);
            }

            string[] args = new string[6]
            {
                "--trt",
                "Integration",
                "--sendmail",
                "false",
                "-f",
                "JSON"
            };

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            Task.Run(() =>
            {
                Program.Main(args);
                manualResetEvent.Set();
            });

            manualResetEvent.WaitOne((int)TimeSpan.FromMinutes(2).TotalMilliseconds, false);

            Thread.Sleep(TimeSpan.FromMilliseconds(5000));

            Environment.ExitCode.Should().Be(0);
            
            string outputfile = $"{Environment.GetEnvironmentVariable("RELEASE_RELEASEID")}-TestResults.json";

            File.Exists(outputfile).Should().BeTrue();
            File.Delete(outputfile.ToString());
        }
    }
}
