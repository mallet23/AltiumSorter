using System;
using System.IO;

namespace AltiumFileGenerator
{
    public class FileGenerator
    {
        private const string Phrase = "The quick brown fox jumps over the lazy dog";
        private readonly string _fileName;
        private readonly Random _random;

        public FileGenerator(string fileName)
        {
            _fileName = fileName;
            _random = new Random();
        }

        public void Generate(long size)
        {
            long currentSize = 0;
            using var fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);

            // at least one number + 2 separators + at least one character
            const int lastLineMinSize = 1 + 2 + 1;
            while (true)
            {
                var line = GenerateLine();

                if (currentSize + line.Length > size - lastLineMinSize)
                {
                    var lastLineStringSize = size - currentSize - lastLineMinSize + 1;
                    var lastLine = $"{GenerateNumber(1)}. {GenerateString((int)lastLineStringSize)}";
                    streamWriter.Write(lastLine);
                    break;
                }

                streamWriter.Write(line);
                currentSize += line.Length;
            }
        }

        private string GenerateLine() =>
            $"{GenerateNumber()}. {GenerateString(_random.Next(1, Phrase.Length))}{Environment.NewLine}";
        
        private string GenerateString(int length)
        {
            var index = _random.Next(0, Phrase.Length - length);
            return Phrase.Substring(index, length);
        }

        private int GenerateNumber(int maxValue = int.MaxValue) => _random.Next(maxValue);

    }
}