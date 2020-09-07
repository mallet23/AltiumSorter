using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AltiumSorter.Entities;
using AltiumSorter.Utils;
using FileInfo = AltiumSorter.Entities.FileInfo;

namespace AltiumSorter
{
    public class FilePartSorter
    {
        private readonly ChannelReader<ListPart> _filePartsReader;
        private readonly ChannelWriter<FileInfo> _sortedFilesWriter;
        private readonly string _tempFolder;

        public FilePartSorter(ChannelReader<ListPart> filePartsReader, ChannelWriter<FileInfo> sortedFilesWriter, string tempFolder)
        {
            _sortedFilesWriter = sortedFilesWriter;
            _tempFolder = tempFolder;
            _filePartsReader = filePartsReader;
        }
        
        public async Task SortAsync(CancellationToken token)
        {
            var tasks = new List<Task>();
            await foreach (var part in _filePartsReader.ReadAllAsync(token))
            {
                var sortedLines = part.SortedList();
                
                var fileName = GenerateFileName();
                tasks.Add(File.WriteAllLinesAsync(fileName, sortedLines, token));

                var file = new FileInfo { FileName = fileName };
                await _sortedFilesWriter.WriteAsync(file, token);
            }

            await Task.WhenAll(tasks);
            _sortedFilesWriter.Complete();
        }
        
        private string GenerateFileName () => $"{_tempFolder}/{Guid.NewGuid():N}";
    }
}