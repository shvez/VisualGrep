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
                var shortFileName = Path.GetFileName(fileName);
                using var sr = File.OpenText(fileName);

                string? s;
                List<LogRecord> records = new List<LogRecord>();
                while ((s = await sr.ReadLineAsync(cancellationToken)) != null)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        yield break;
                    }

                    if (lineNumber % 100 == 0)
                    {
                        if (records.Count > 0)
                        {
                            yield return records;

                            records = [];
                        }
                    }
                    lineNumber++;

                    var lr = filter.Match(shortFileName, s, Convert.ToString(lineNumber++));
                    if (lr != null)
                    {
                        records.Add(lr);
                    }
                }

                if (records.Count > 0)
                {
                    yield return records;
                }
            }
        }
    }
}
