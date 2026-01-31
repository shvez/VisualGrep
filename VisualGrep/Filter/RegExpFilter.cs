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
                // Skip group 0 (full match) and add only named/captured groups
                for (int i = 1; i < groups.Count; i++)
                {
                    var group = groups[i];
                    lr.AdditionalInfo.Add(group.Name, group.Value);
                }
                return lr;
            }
            return null;
        }
    }
}
