namespace VisualGrep.Services
{
    public interface IDataGrid
    {
        void UpdateColumns(IEnumerable<string> additionalColumnNames);
    }
}
