using System.Text.RegularExpressions;
using VisualGrep.Models;

namespace VisualGrep.Filter
{
    internal class RegExpFilter : ISearchFilter
    {
        private readonly Regex regex;

        public RegExpFilter(string pattern, bool ignoreCase)
        {
            var options = RegexOptions.Compiled | RegexOptions.Singleline;
            if (ignoreCase)
            {
                options |= RegexOptions.IgnoreCase;
            }
            this.regex = new Regex(pattern, options);
        }

        public LogRecord? Match(string fileName, string str, string lineNumber)
        {
            var match = this.regex.Match(str);
            if (match.Success)
            {
                var lr = new LogRecord()
                {
                    FileName = fileName,
                    LineNumber = lineNumber,
                    Message = str,
                };

                var groups = match.Groups;
                foreach (Group group in groups)
                {
                }
                return lr;
            }
            return null;
        }
    }
}
