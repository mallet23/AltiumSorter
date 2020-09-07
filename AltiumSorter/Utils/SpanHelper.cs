using System;

namespace AltiumSorter.Utils
{
    public static class SpanHelper
    {
        public static ReadOnlySpan<T> Skip<T>(this ReadOnlySpan<T> span, int skipCount)
        {
            return span.Slice(skipCount, span.Length - skipCount);
        }
		
        public static ReadOnlySpan<T> Take<T>(this ReadOnlySpan<T> span, int takeCount)
        {
            return span.Slice(0, takeCount);
        }
    }
}