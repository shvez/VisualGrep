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
                using StreamReader sr = File.OpenText(fileName);
                string? s;
                List<LogRecord>? records = null;
                while ((s = await sr.ReadLineAsync(cancellationToken)) != null)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        yield break;
                    }

                    if (lineNumber % 100 == 0)
                    {
                        if (records != null)
                        {
                            yield return records;
                        }

                        records = new List<LogRecord>();
                    }
                    lineNumber++;

                    var lr = filter.Match(fileName, s, Convert.ToString(lineNumber++));
                    if (lr != null)
                    {
                        records.Add(lr);
                    }
                }

                if (records != null)
                {
                    yield return records;
                }
            }
        }
    }
}
