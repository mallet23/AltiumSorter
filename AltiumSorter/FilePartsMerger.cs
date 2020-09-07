using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AltiumSorter.Utils;
using FileInfo = AltiumSorter.Entities.FileInfo;

namespace AltiumSorter
{
    public class FilePartsMerger
    {
        private readonly string _outputFile;
        private readonly ChannelReader<FileInfo> _channelReader;

        public FilePartsMerger(string outputFile, ChannelReader<FileInfo> channelReader)
        {
            _outputFile = outputFile;
            _channelReader = channelReader;
        }

        public async Task MergeAsync(CancellationToken token)
        {
            var sortedFiles = new List<FileInfo>();
            await foreach (var item in _channelReader.ReadAllAsync(token))
            {
                sortedFiles.Add(item);
            }
            
            var sortedList = new SortedList<string, (int count, int partIndex)>(ItemComparer.Instance);
            var partsReaders = sortedFiles.Select(file => new StreamReader(file.FileName)).ToList();

            await using var writer = new StreamWriter(_outputFile);
            try
            {
                // Initial values
                for (var i = 0; i < partsReaders.Count; i++)
                {
                    var line = await partsReaders[i].ReadLineAsync();
                    await TryAdd(sortedList, line, i, partsReaders[i]);
                }

                // Take min value and set new one
                do
                {
                    var (count, partIndex) = sortedList.Values[0];
                    
                    for (var i = 0; i < count; i++)
                    {
                        await writer.WriteLineAsync(sortedList.Keys[0]);
                    }
                    
                    sortedList.RemoveAt(0);

                    var line = await partsReaders[partIndex].ReadLineAsync();
                    
                    // if min value then do not set into sorted list and write into file
                    while (line != null && sortedList.Count > 0 && ItemComparer.Instance.Compare(line, sortedList.Keys[0]) < 0)
                    {
                        await writer.WriteLineAsync(line);
                        line = await partsReaders[partIndex].ReadLineAsync();
                    }
                    
                    await TryAdd(sortedList, line, partIndex, partsReaders[partIndex]);
                } while (sortedList.Count > 0);
            }
            finally
            {
                foreach (var streamReader in partsReaders)
                {
                    streamReader.Dispose();
                }
            }
        }

        private static async Task TryAdd(SortedList<string, (int count, int partIndex)> sortedList, string line, int partIndex, TextReader reader)
        {
            while (true)
            {
                if (line == null)
                {
                    return;
                }

                if (sortedList.TryAdd(line, (1, partIndex)))
                {
                    return;
                }

                // count the same values and find different element
                do
                {
                    var (count, index) = sortedList[line];
                    sortedList[line] = (++count, index);
                    line = await reader.ReadLineAsync();
                } while (line != null && sortedList.ContainsKey(line));
            }
        }
    }
}