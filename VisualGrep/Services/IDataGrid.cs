using VisualGrep.Models;

namespace VisualGrep.Services
{
    public interface IDataGrid
    {
        void AddColumns(string[] names);
        void Clear();

        void AddEntries(LogRecord[] entries);
        int AddEntry(LogRecord entries);
        void ModifyEntry(int pos, LogRecord entries);
    }
}
