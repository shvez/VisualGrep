namespace VisualGrep.Models
{
    public class LogRecord
    {
        public string FileName { get; set; } = string.Empty;
        public string LineNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Словарь: ключ = имя столбца, значение = данные
        public Dictionary<string, string> AdditionalInfo { get; set; } = [];
    }
}