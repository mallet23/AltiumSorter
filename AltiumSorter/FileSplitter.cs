using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AltiumSorter.Entities;

namespace AltiumSorter
{
    public class FileSplitter
    {
        private readonly string _fileName;
        private readonly ChannelWriter<ListPart> _fileWriter;

        public FileSplitter(string fileName, ChannelWriter<ListPart> fileWriter)
        {
            _fileName = fileName;
            _fileWriter = fileWriter;
        }

        public async Task SplitAsync(long maxSize, CancellationToken token)
        {
            var partSize = 0L;

            var lines = new List<string>();
            string line;

            using var reader = new StreamReader(_fileName);
            while ((line = await reader.ReadLineAsync()) != null)
            {
                partSize += line.Length * sizeof(char);

                if (partSize >= maxSize)
                {
                    await _fileWriter.WriteAsync(new ListPart(lines.ToArray()), token);
                    
                    partSize = 0;
                    lines.Clear();
                }
                
                lines.Add(line);
            }
            
            if (partSize < maxSize)
            {
                await _fileWriter.WriteAsync(new ListPart(lines.ToArray()), token);
            }
            
            _fileWriter.Complete();
        }
    }
}