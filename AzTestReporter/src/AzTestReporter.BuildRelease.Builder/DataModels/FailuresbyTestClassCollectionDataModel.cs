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
            IReadOnlyList<TestResultData> testResultDataList)
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
                    int rowcount = 0;
                    var testswithsubresults = testRunCasesFailures.Where(r => r.TestSubResults != null);
                    if (testswithsubresults.Any())
                    {
                        rowcount += testswithsubresults.SelectMany(r => r.TestSubResults).Where(r => r.Outcome == Apis.Common.OutcomeEnum.Failed).Count();
                    }

                    var testswithoutsubresults = testRunCasesFailures.Where(r => r.TestSubResults != null);
                    if (testswithoutsubresults.Any())
                    {
                        rowcount += testswithoutsubresults.Count();
                    }

                    //    .SelectMany(r => r.TestSubResults).ToList();

                    for (int i = 0; i < testRunCasesFailures.Count; i++)
                    {
                        var bugsandlinks = new Dictionary<string, string>();
                        if (testRunCasesFailures[i].AssociatedBugs?.Count > 0)
                        {
                            foreach (var bugandlink in testRunCasesFailures[i].AssociatedBugs)
                            {
                                if (!bugsandlinks.ContainsKey(bugandlink.Id))
                                {
                                    bugsandlinks.Add(bugandlink.Id, bugandlink.Url);
                                }
                            }
                        }

                        if (i != 0)
                        {
                            rowcount = -1;
                        }

                        if (!testRunCasesFailures[i].TestSubResults.Any())
                        {
                            var failuredm = new FailuresbyTestAreaDataModel()
                            {
                                RowCount = i == 0? rowcount: -1,
                                TestClassName = feature,
                                BugandLink = bugsandlinks,
                                TestNamespace = testRunCasesFailures[i].TestNamespace,
                                FailingSince = testRunCasesFailures[i].FailingSince != null ?
                                    this.ConvertToDays(testRunCasesFailures[i].FailingSince.Date) : string.Empty,
                                //LinktoRunWeb = $"{resultsrooturl}&runId={testRunCasesFailures[i].TestRun.Id}&resultId={testRunCasesFailures[i].Id}"
                            };

                            failuredm.FailuresinTestArea = new List<FailuresinTestAreaDataModel>();
                            var failure = new FailuresinTestAreaDataModel(){
                                Duration = this.ConvertToSecondsMilliseconds(
                                    Convert.ToDateTime(testRunCasesFailures[i].CompletedDate, CultureInfo.InvariantCulture) -
                                    Convert.ToDateTime(testRunCasesFailures[i].StartedDate, CultureInfo.InvariantCulture)),
                                TestName = this.ShortTestName(testRunCasesFailures[i].TestCaseName),
                                ErrorMessage = testRunCasesFailures[i].ErrorMessage?.ToString(),
                            };

                            failuredm.FailuresinTestArea.Add(failure);
                            this.Add(failuredm);
                        }
                        else
                        {
                            var failuredm = new FailuresbyTestAreaDataModel()
                            {
                                RowCount = i == 0 ? rowcount : -1,
                                TestClassName = feature,
                                BugandLink = bugsandlinks,
                                TestNamespace = testRunCasesFailures[i].TestNamespace,
                                //LinktoRunWeb = $"{resultsrooturl}&runId={testRunCasesFailures[i].TestRun.Id}&resultId={testRunCasesFailures[i].Id}"
                            };

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
                                    ErrorMessage = testsubresult.ErrorMessage?.ToString()
                                };

                                subfailures.Add(failure);
                            });

                            failuredm.FailuresinTestArea = subfailures;
                            this.Add(failuredm);
                        }
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
