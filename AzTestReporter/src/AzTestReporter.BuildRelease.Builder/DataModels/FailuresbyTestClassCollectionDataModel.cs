namespace AzTestReporter.BuildRelease.Builder.DataModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using AzTestReporter.BuildRelease.Apis;
    using Validation;

    public class FailuresbyTestClassCollectionDataModel : List<FailuresbyTestAreaDataModel>
    {
        public FailuresbyTestClassCollectionDataModel(
            string azureprojectname,
            IReadOnlyList<TestResultData> testResultDataList)
        {
            //Requires.NotNullOrEmpty(azureprojectname, nameof(azureprojectname));
            Requires.NotNull(testResultDataList, nameof(testResultDataList));

            List<string> features = testResultDataList.Where(r => string.IsNullOrEmpty(r.TestClassName) == false)
                    .Select(r => r.TestClassName).Distinct().ToList();
            foreach (string feature in features)
            {
                List<TestResultData> testcaseResultsFromOneFeature = testResultDataList.ToList().FindAll(tc => tc.AutomatedTestName.Contains(feature));
                List<TestResultData> testRunCasesFailures = testcaseResultsFromOneFeature.FindAll(tcf => tcf.Outcome.ToUpperInvariant() == "FAILED");

                if (testRunCasesFailures.Count > 0)
                {
                    int rowcount = 0;
                    for (int i = 0; i < testRunCasesFailures.Count; i++)
                    {
                        // TODO: This was removed but needs to be re-enabled.
                        /*var bugsandlinks = new Dictionary<string, string>();
                        if (testRunCasesFailures[i].AssociatedBugs?.Count > 0)
                        {
                            foreach (var bugandlink in testRunCasesFailures[i].AssociatedBugs)
                            {
                                if (!bugsandlinks.ContainsKey(bugandlink.Id))
                                {
                                    bugsandlinks.Add(bugandlink.Id, bugandlink.Url);
                                }
                            }
                        }*/

                        if (i == 0)
                        {
                            rowcount = testRunCasesFailures.Count;
                        }
                        else
                        {
                            rowcount = -1;
                        }

                        var failuredm = new FailuresbyTestAreaDataModel()
                        {
                            RowCount = rowcount,
                            TestClassName = feature,
                            //TODO: BugandLink = bugsandlinks,
                        };

                        failuredm.Duration = this.ConvertToSecondsMilliseconds(
                            Convert.ToDateTime(testRunCasesFailures[i].CompletedDate, CultureInfo.InvariantCulture) -
                            Convert.ToDateTime(testRunCasesFailures[i].StartedDate, CultureInfo.InvariantCulture));
                        failuredm.TestName = this.ShortTestName(testRunCasesFailures[i].TestCaseName);
                        // TODO: failuredm.LinktoRunWeb = $"https://dev.azure.com/{azureorganizationame}/{azureprojectname}/_testManagement/runs?_a=resultSummary&runId={testRunCasesFailures[i].TestRun.Id}&resultId={testRunCasesFailures[i].Id}";
                        failuredm.ErrorMessage = testRunCasesFailures[i].ErrorMessage?.ToString();
                        failuredm.FailingSince = testRunCasesFailures[i].FailingSince != null ?
                            this.ConvertToDays(testRunCasesFailures[i].FailingSince.Date) : string.Empty;

                        this.Add(failuredm);
                    }
                }
            }
        }

        private string ConvertToSecondsMilliseconds(TimeSpan duration)
        {
            return $"{(duration.Minutes * 60) + duration.Seconds}.{duration.Milliseconds:0000}";
        }

        private string ConvertToDays(DateTime dateTime)
        {
            return Math.Round((DateTime.Now - dateTime).TotalDays, 0).ToString();
        }

        private string ShortTestName(string testName)
        {
            if (!string.IsNullOrEmpty(testName))
            {
                return testName.Split('.')[testName.Split('.').Length - 1].Replace('_', ' ');
            }

            return string.Empty;
        }

        private string ShortErrorString(string errorMessage)
        {
            StringBuilder strBuilder = new StringBuilder();

            try
            {
                if (errorMessage.Length > 0)
                {
                    strBuilder.AppendLine(errorMessage.Substring(0, errorMessage.Length > 60 ? 60 : errorMessage.Length - 1));
                    if (errorMessage.Length > 60)
                    {
                        strBuilder.AppendLine(errorMessage.Substring(60, errorMessage.Length > 120 ? 60 : errorMessage.Length - 61));
                    }

                    if (errorMessage.Length > 120)
                    {
                        strBuilder.AppendLine(errorMessage.Substring(120, errorMessage.Length > 180 ? 60 : errorMessage.Length - 121));
                    }

                    return strBuilder.ToString();
                }
            }
            catch
            {
            }

            return errorMessage;
        }

    }
}
