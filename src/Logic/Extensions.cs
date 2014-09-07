using System;

namespace Nikse.SubtitleEdit.Logic
{
    public static class Extensions
    {

        public static bool StartsWith(this String s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool EndsWith(this String s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
        }

    }
}
