using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace AltiumFileGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            var generatedFileName = configuration["GeneratedFilePath"];
            int.TryParse(configuration["FileSizeInGb"], out var fileSizeInGb);
       
            var fileSize = fileSizeInGb * 1024L * 1024L * 1024L;
            
            Console.WriteLine("Start file generation...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var fileGenerator =  new FileGenerator(generatedFileName);
            fileGenerator.Generate(fileSize);
            
            stopwatch.Stop();
            Console.WriteLine($"Generated: {stopwatch.Elapsed}");
        }
    }
}