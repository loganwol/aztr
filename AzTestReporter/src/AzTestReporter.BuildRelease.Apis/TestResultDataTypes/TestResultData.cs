namespace AzTestReporter.BuildRelease.Apis
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    public class TestResultData
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "outcome", Required = Required.Always)]
        public string Outcome { get; set; }

        [JsonProperty(PropertyName = "testCaseTitle", Required = Required.Always)]
        public string TestCaseName { get; set; }

        [JsonProperty(PropertyName = "startedDate", Required = Required.Always)]
        public string StartedDate { get; set; }

        [JsonProperty(PropertyName = "completedDate", Required = Required.Always)]
        public string CompletedDate { get; set; }

        [JsonProperty(PropertyName = "testRun", Required = Required.Always)]
        public Run TestRun { get; set; }

        [JsonProperty(PropertyName = "build", Required = Required.Always)]
        public Build Build { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "stackTrace")]
        public string StackTrace { get; set; }

        [JsonProperty(PropertyName = "failingSince")]
        public FailingSince FailingSince { get; set; }

        [JsonProperty(PropertyName = "automatedTestName")]
        public string AutomatedTestName { get; set; }

        [JsonProperty(PropertyName = "automatedTestType")]
        public AutomatedTestTypeEnum AutomatedTestType { get; set; }

        [JsonProperty(PropertyName = "automatedTestStorage")]
        public string AutomatedTestStorage { get; set; }

        [JsonProperty(PropertyName = "associatedBugs")]
        public List<AzureBugLinkData> AssociatedBugs { get; set; }

        /// <summary>
        /// Gets the Test Class Name discovered from the Automated test name.
        /// </summary>
        public string TestClassName => this.GetTestTitle(false);

        private string GetTestTitle(bool issubsystem, string reponame = "")
        {
            if (this.AutomatedTestType == AutomatedTestTypeEnum.UnitTest)
            {
                string[] fqnStrings = this.AutomatedTestName?.Split('.');
                if (fqnStrings.Length > 0)
                {
                    string name = string.Empty;
                    if (fqnStrings.Length == 1 || this.AutomatedTestName.Contains("["))
                    {
                        name = fqnStrings[0];
                    }
                    else if (fqnStrings.Length > 1)
                    {
                        if (issubsystem)
                        {
                            if (!string.IsNullOrEmpty(reponame))
                            {
                                name = $"{reponame} - ";
                            }

                            name += fqnStrings[2];
                        }
                        else
                        {
                            const string matchpattern = @"\W";
                            name = fqnStrings[fqnStrings.Length - 2];
                            if (Regex.Match(name, matchpattern).Success)
                            {
                                Stack<string> namespacewalker = new Stack<string>();
                                string testname = this.AutomatedTestName;

                                while (Regex.Match(testname, matchpattern).Success)
                                {
                                    int index = testname.IndexOf(".", System.StringComparison.InvariantCulture);
                                    if (index >= 0)
                                    {
                                        var wordinnamespace = testname.Substring(0, index);
                                        var remainingnamespace = testname.Substring(index);

                                        if (Regex.Match(remainingnamespace, matchpattern).Success && Regex.Match(wordinnamespace, matchpattern).Success == false)
                                        {
                                            if (remainingnamespace.StartsWith(".", System.StringComparison.InvariantCulture))
                                            {
                                                remainingnamespace = remainingnamespace.Substring(1);
                                            }

                                            testname = remainingnamespace;
                                        }
                                        else
                                        {
                                            break;
                                        }

                                        namespacewalker.Push(wordinnamespace);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                name = namespacewalker.Pop();
                            }
                        }
                    }

                    return name;
                }
            }
            else if (this.AutomatedTestType == AutomatedTestTypeEnum.JUnit)
            {
                string[] temp = Regex.Split(this.AutomatedTestName, @"(\[|\])+");
                if (temp.Length > 1)
                {
                    return temp[0];
                }
                else
                {
                    return this.AutomatedTestName;
                }   
            }

            return string.Empty;
        }
    }
}
