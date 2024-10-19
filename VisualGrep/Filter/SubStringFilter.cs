using VisualGrep.Models;

namespace VisualGrep.Filter
{
    internal class SubStringFilter : ISearchFilter
    {
        private readonly string substring;
        private readonly StringComparison comparisonOptions;

        public SubStringFilter(string substring, bool ignoreCase)
        {
            this.substring = substring;
            this.comparisonOptions = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;
        }
        public LogRecord? Match(string fileName, string str, string lineNumber)
        {
            if (str.IndexOf(this.substring, this.comparisonOptions) != -1)
            {
                return new LogRecord()
                {
                    FileName = fileName,
                    LineNumber = lineNumber,
                    Message = str,
                };
            }
            return null;
        }
    }
}
