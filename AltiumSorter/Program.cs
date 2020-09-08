using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AltiumSorter.Entities;
using Microsoft.Extensions.Configuration;

namespace AltiumSorter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            var generatedFileName = configuration["GeneratedFilePath"];
            var outputFileName = configuration["SortedFilePath"];
            var tempFolder = configuration["TempFolder"];
            int.TryParse(configuration["MaxMemorySizeInMb"], out var maxMemorySize);
       
            var maxSize = maxMemorySize * 1024 * 1024;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Start sorting...");

            using var source = new CancellationTokenSource();
            var notSortedFileParts = Channel.CreateUnbounded<ListPart>(new UnboundedChannelOptions
            {
                SingleWriter = true
            });
            var sortedFileParts = Channel.CreateUnbounded<FileInfo>(new UnboundedChannelOptions{SingleReader = true});
            
            var tasks = new List<Task>();
            
            var fileSplitter = new FileSplitter(generatedFileName, notSortedFileParts.Writer);
            tasks.Add(fileSplitter.SplitAsync(maxSize, source.Token));

            var filePartSorter = new FilePartSorter(notSortedFileParts.Reader, sortedFileParts.Writer, tempFolder);
            tasks.Add(filePartSorter.SortAsync(source.Token));
            
            var filePartsMerger = new FilePartsMerger(outputFileName, sortedFileParts.Reader);
            tasks.Add(filePartsMerger.MergeAsync(source.Token));

            await Task.WhenAll(tasks.ToArray());
            
            stopwatch.Stop();
            Console.WriteLine($"Sorted: {stopwatch.Elapsed}");
        }
    }
}