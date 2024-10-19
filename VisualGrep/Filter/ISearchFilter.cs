using VisualGrep.Models;

namespace VisualGrep.Filter
{
    internal interface ISearchFilter
    {
        LogRecord? Match(string fileName, string str, string lineNumber);
    }
}
