namespace Nikse.SubtitleEdit.Core.Common.TimeFormatters
{
    /// <summary>
    /// Formats as "HH:MM:SS:FF" with leading zero groups ("00:") trimmed.
    /// </summary>
    public class ShortHhMmSsFfTimeFormatter : ITimeFormatter
    {
        private static readonly HhMmSsFfTimeFormatter LongFormatter = new HhMmSsFfTimeFormatter();

        public string Format(TimeCode timeCode)
        {
            var s = LongFormatter.Format(timeCode);
            var pre = string.Empty;
            if (s.StartsWith('-'))
            {
                pre = "-";
                s = s.TrimStart('-');
            }

            var j = 0;
            var len = s.Length;
            while (j + 6 < len && s[j] == '0' && s[j + 1] == '0' && s[j + 2] == ':')
            {
                j += 3;
            }
            s = j > 0 ? s.Substring(j) : s;
            return pre + s;
        }
    }
}
