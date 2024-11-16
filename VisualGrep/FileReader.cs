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

                string? s;
                List<LogRecord> records = new List<LogRecord>();
                var firstRecord = new LogRecord()
                {
                    FileName = shortFileName,
                    LineNumber = "-",
                    Message = $"found {matchesCount} matches in {lineNumber} lines"
                };
                records.Add(firstRecord);
                while ((s = await sr.ReadLineAsync(cancellationToken)) != null)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        firstRecord.Message = $"found {matchesCount} matches in {lineNumber} lines";
                        yield break;
                    }

                    if (lineNumber % 500 == 0)
                    {
                        if (records.Count > 0)
                        {
                            yield return records;

                            records = [];
                        }
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
