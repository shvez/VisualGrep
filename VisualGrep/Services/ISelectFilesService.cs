namespace VisualGrep.Services
{
    public interface ISelectFilesService
    {
        (string Folder, List<string> Files) GetFileList();
    }
}
