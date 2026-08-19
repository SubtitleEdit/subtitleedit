#if NETSTANDARD2_1
using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// The ReadOnlyMemory&lt;char&gt;-receiver Trim overloads exist on .NET Core 3.0+
    /// but are missing from the .NET Standard 2.1 reference assemblies - polyfill them
    /// on top of the span-receiver overloads, which are available.
    /// </summary>
    internal static class MemoryCharExtensions
    {
        public static ReadOnlyMemory<char> Trim(this ReadOnlyMemory<char> memory)
        {
            var span = memory.Span;
            var start = span.Length - span.TrimStart().Length;
            return memory.Slice(start, span.Trim().Length);
        }

        public static ReadOnlyMemory<char> TrimStart(this ReadOnlyMemory<char> memory, ReadOnlySpan<char> trimChars)
            => memory.Slice(memory.Length - memory.Span.TrimStart(trimChars).Length);

        public static ReadOnlyMemory<char> TrimEnd(this ReadOnlyMemory<char> memory, ReadOnlySpan<char> trimChars)
            => memory.Slice(0, memory.Span.TrimEnd(trimChars).Length);
    }
}
#endif
