namespace AzTestReporter.BuildRelease.Apis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json;
    using AzTestReporter.BuildRelease.Apis.Exceptions;
    using Validation;

    public class ReleasesCollection : List<Release>
    {
        public ReleasesCollection(IBuildandReleaseReader dataReader, string releasedefinitionname)
        {
            Requires.NotNull(dataReader, nameof(dataReader));
            Requires.NotNull(releasedefinitionname, nameof(releasedefinitionname));

            DateTime maxDate = DateTime.Now.Date.AddDays(2);
            string maxDateString = maxDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            string minDateString = maxDate.AddDays(-7).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
            AzureSuccessReponse releases = dataReader.GetReleasesbyDateRangeAsync(minDateString, maxDateString).GetAwaiter().GetResult();
            if (releases == null)
            {
                throw new TestResultReportingReleaseNotFoundException("No releases were found.");
            }

            for (int i = 0; i < releases.Count; i++)
            {
                var release = JsonConvert.DeserializeObject<Release>(releases.Value[i].ToString());
                if (release.ReleaseDefinitionName.ToUpperInvariant() == releasedefinitionname.ToUpperInvariant())
                {
                    this.Add(release);
                }
            }
        }

        public Release GetReleaseDatabyName(string releaseName)
        {
            Requires.NotNullOrEmpty(releaseName, nameof(releaseName));

            var matchedreleaseslist = this.Where(r =>
                r.Name.ToUpperInvariant() == releaseName.ToUpperInvariant());
            if (!matchedreleaseslist.Any())
            {
                return null;
            }

            if (matchedreleaseslist.Count() > 1)
            {
                throw new TestResultReportingException($"More than one release with the same name {releaseName} was found.");
            }

            return matchedreleaseslist.First();
        }
    }
}
