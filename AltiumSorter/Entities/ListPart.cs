using System;
using AltiumSorter.Utils;

namespace AltiumSorter.Entities
{
    public class ListPart
    {
        private readonly string[] _lines;

        public ListPart(string[] lines)
        {
            _lines = lines;
        }

        public string[] SortedList()
        {
            Array.Sort(_lines, ItemComparer.Instance);

            return _lines;
        }
    }
}