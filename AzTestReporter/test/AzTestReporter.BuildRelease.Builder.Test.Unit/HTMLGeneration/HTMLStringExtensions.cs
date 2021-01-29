namespace AzTestReporter.BuildRelease.Builder.HTMLGeneration.Test.Unit
{
    public static class HTMLStringExtensions
    {
        public static string RemoveHTMLExtras(this string htmltext)
        {
            return htmltext?
                .Replace("&nbsp;", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty).Trim();
        }
    }
}
