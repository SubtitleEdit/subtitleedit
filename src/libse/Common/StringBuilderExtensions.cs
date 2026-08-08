using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Trims leading/trailing whitespace inside the builder - the "sb.ToString().Trim()"
        /// idiom allocates the whole output an extra time.
        /// </summary>
        public static void Trim(this StringBuilder sb)
        {
            while (sb.Length > 0 && char.IsWhiteSpace(sb[sb.Length - 1]))
            {
                sb.Length--;
            }

            var start = 0;
            while (start < sb.Length && char.IsWhiteSpace(sb[start]))
            {
                start++;
            }

            if (start > 0)
            {
                sb.Remove(0, start);
            }
        }

        public static bool StartsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[0] == c;
        }

        public static bool EndsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[sb.Length - 1] == c;
        }

        // Same count as scanning sb.ToString() without materializing the whole
        // accumulated text into a fresh string.
        public static int CountChar(this StringBuilder sb, char c)
        {
            var count = 0;
#if NET8_0_OR_GREATER
            foreach (var chunk in sb.GetChunks())
            {
                foreach (var ch in chunk.Span)
                {
                    if (ch == c)
                    {
                        count++;
                    }
                }
            }
#else
            // GetChunks is missing from the netstandard2.1 reference assemblies.
            for (var i = 0; i < sb.Length; i++)
            {
                if (sb[i] == c)
                {
                    count++;
                }
            }
#endif
            return count;
        }

        // Matches "{0:00}"-style formatting: sign first, then the absolute value padded with
        // leading zeros to minDigits.
        public static void AppendNumber(this StringBuilder sb, int value, int minDigits)
        {
            if (value < 0)
            {
                sb.Append('-');
                value = -value;
            }

            if (minDigits >= 2 && value < 10)
            {
                sb.Append('0');
            }

            sb.Append(value);
        }
    }
}
