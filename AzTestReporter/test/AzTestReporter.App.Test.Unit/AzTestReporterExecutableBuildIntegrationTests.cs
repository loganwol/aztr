using FluentAssertions;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AzTestReporter.App.Test.Unit
{
    public class AzTestReporterExecutableBuildIntegrationTests
    {
        [Fact]
        public void Can_successfully_run_reporter_for_unit_test_in_build_generating_only_html_output()
        {
            string[] reportfiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "TestExecutionReport-*-ExecutionID*.html");
            foreach (var reportfile in reportfiles)
            {
                File.Delete(reportfile);
            }

            string[] args = new string[4]
            {
                "--trt",
                "Unit",
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
            outputfile.Append(Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER"));
            outputfile.Append("-ExecutionID");
            outputfile.Append($"{Environment.GetEnvironmentVariable("BUILD_BUILDID")}.html");

            File.Exists(outputfile.ToString()).Should().BeTrue();

            File.Delete(outputfile.ToString());
        }

        [Fact]
        public void Can_successfully_run_reporter_for_unit_test_in_build_generating_only_json_output()
        {
            string[] reportfiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*-TestResults.json");
            foreach (var reportfile in reportfiles)
            {
                File.Delete(reportfile);
            }

            string[] args = new string[6]
            {
                "--trt",
                "Unit",
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

            string outputfile = $"{Environment.GetEnvironmentVariable("BUILD_BUILDID")}-TestResults.json";

            File.Exists(outputfile).Should().BeTrue();

            File.Delete(outputfile.ToString());
        }
    }
}
