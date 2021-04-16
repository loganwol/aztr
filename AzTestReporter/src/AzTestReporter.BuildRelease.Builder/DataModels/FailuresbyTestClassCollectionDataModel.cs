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
            string resultsrooturl,
            IReadOnlyList<TestResultData> testResultDataList,
            bool summarizewithsubresults = false)
        {
            //Requires.NotNullOrEmpty(azureprojectname, nameof(azureprojectname));
            Requires.NotNull(testResultDataList, nameof(testResultDataList));

            List<string> features = testResultDataList.Where(r => string.IsNullOrEmpty(r.TestClassName) == false)
                    .Select(r => r.TestClassName).Distinct().ToList();
            foreach (string feature in features)
            {
                List<TestResultData> testcaseResultsFromOneFeature = testResultDataList.ToList().FindAll(tc => tc.AutomatedTestName.Contains(feature));
                List<TestResultData> testRunCasesFailures = testcaseResultsFromOneFeature.FindAll(tcf => tcf.Outcome == Apis.Common.OutcomeEnum.Failed);

                if (testRunCasesFailures.Any())
                {
                    var failuredm = new FailuresbyTestAreaDataModel()
                    {
                        TestClassName = feature,
                        TestNamespace = testRunCasesFailures.First().TestNamespace,
                        FailingSince = testRunCasesFailures.First().FailingSince != null ?
                                    this.ConvertToDays(testRunCasesFailures.First().FailingSince.Date) : string.Empty,
                    };

                    failuredm.FailuresinTestArea = new List<FailuresinTestAreaDataModel>();

                    for (int i = 0; i < testRunCasesFailures.Count; i++)
                    {
                        var testbuglinks = new Dictionary<string, string>();
                        if (testRunCasesFailures[i].AssociatedBugs?.Count > 0)
                        {
                            foreach (var bugandlink in testRunCasesFailures[i].AssociatedBugs)
                            {
                                if (!testbuglinks.ContainsKey(bugandlink.Id))
                                {
                                    testbuglinks.Add(bugandlink.Id, bugandlink.Url);
                                }
                            }
                        }

                        if (!summarizewithsubresults)
                        {
                            var failure = new FailuresinTestAreaDataModel() {
                                Duration = this.ConvertToSecondsMilliseconds(
                                    Convert.ToDateTime(testRunCasesFailures[i].CompletedDate, CultureInfo.InvariantCulture) -
                                    Convert.ToDateTime(testRunCasesFailures[i].StartedDate, CultureInfo.InvariantCulture)),
                                TestName = this.ShortTestName(testRunCasesFailures[i].TestCaseName),
                                ErrorMessage = testRunCasesFailures[i].ErrorMessage?.ToString(),
                                BugandLink = testbuglinks,
                            };

                            failuredm.FailuresinTestArea.Add(failure);
                        }
                        else if(testRunCasesFailures[i].TestSubResults != null && testRunCasesFailures[i].TestSubResults.Any())
                        {
                            var subfailures = new List<FailuresinTestAreaDataModel>();
                            testRunCasesFailures[i].TestSubResults
                                .Where(r => r.Outcome == Apis.Common.OutcomeEnum.Failed)
                                .ToList()
                                .ForEach(testsubresult =>
                            {
                                var failure = new FailuresinTestAreaDataModel()
                                {
                                    Duration = TimeSpan.FromMilliseconds(testsubresult.durationInMs).TotalSeconds.ToString(),
                                    TestName = testsubresult.DisplayName,

                                    // TODO: failuredm.LinktoRunWeb = $"https://dev.azure.com/{azureorganizationame}/{azureprojectname}/_testManagement/runs?_a=resultSummary&runId={testRunCasesFailures[i].TestRun.Id}&resultId={testRunCasesFailures[i].Id}";
                                    ErrorMessage = testsubresult.ErrorMessage?.ToString(),
                                    BugandLink = testbuglinks,
                                };

                                subfailures.Add(failure);
                            });

                            failuredm.FailuresinTestArea = subfailures;
                        }
                    }

                    this.Add(failuredm);
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
