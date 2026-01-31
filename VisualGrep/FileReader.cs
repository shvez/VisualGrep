using System.Runtime.CompilerServices;
using VisualGrep.Filter;
using VisualGrep.Models;

namespace VisualGrep
{
    internal class FileReader
    {
        private readonly string path;
        private readonly string fileMask;

        public FileReader(string path, string fileMask)
        {
            this.path = path;
            this.fileMask = fileMask;
        }


        public async IAsyncEnumerable<List<LogRecord>> GetLogRecords(ISearchFilter filter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var files = Directory.EnumerateFiles(this.path, this.fileMask);

            List<LogRecord> records = new List<LogRecord>(100_000);

            foreach (var fileName in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                var lineNumber = 0;
                var matchesCount = 0;
                var shortFileName = Path.GetFileName(fileName);
                using var sr = File.OpenText(fileName);

                records.Clear();
                var firstRecord = new LogRecord()
                {
                    FileName = shortFileName,
                    LineNumber = "-",
                    Message = $"found {matchesCount} matches in {lineNumber} lines"
                };
                records.Add(firstRecord);
                while (await sr.ReadLineAsync(cancellationToken) is { } s)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        firstRecord.Message = $"found {matchesCount} matches in {lineNumber} lines";
                        yield break;
                    }

                    var lr = filter.Match(shortFileName, s, Convert.ToString(lineNumber++));
                    if (lr != null)
                    {
                        ++matchesCount;
                        records.Add(lr);
                    }
                }

                firstRecord.Message = $"found {matchesCount} matches in {lineNumber} lines";

                if (records.Count > 0)
                {
                    yield return records;
                }
            }
        }
    }
}
