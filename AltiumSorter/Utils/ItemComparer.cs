using System;
using System.Collections.Generic;

namespace AltiumSorter.Utils
{
    public class ItemComparer : IComparer<string>
    {
        internal static readonly ItemComparer Instance = new ItemComparer();
        private const string PartsSeparator = ". ";

        private ItemComparer() { }

        public int Compare(string item1, string item2)
        {
            if (item1 == item2) return 0;
            if (item1 == null) return -1;
            if (item2 == null) return 1;

            var item1Span = item1.AsSpan();
            var item2Span = item2.AsSpan();
			
            var item1SeparatorIdx = item1Span.IndexOf(PartsSeparator);
            var item2SeparatorIdx = item2Span.IndexOf(PartsSeparator);
            
            // skip number + 2 separator chars
            var item1StringPart = item1Span.Skip(item1SeparatorIdx + 2); 
            var item2StringPart = item2Span.Skip(item2SeparatorIdx + 2);

            var stringComparisionResult = item1StringPart.SequenceCompareTo(item2StringPart);

            if (stringComparisionResult != 0)
            {
                return stringComparisionResult;
            }

            var item1NumberPart = item1Span.Take(item1SeparatorIdx);
            var item2NumberPart = item2Span.Take(item2SeparatorIdx);
			
            if (item1NumberPart.Length != item2NumberPart.Length)
            {
                return item1NumberPart.Length - item2NumberPart.Length;
            }
			
            stringComparisionResult = item1NumberPart.SequenceCompareTo(item2NumberPart);

            // Returning 1 on 0 result required by Array.Sort distributing algorithm
            return stringComparisionResult == 0 ? 1 : stringComparisionResult;
        }
        
    }
}