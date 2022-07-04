using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class GermanNouns
    {
        private readonly List<string> _germanNouns;

        public GermanNouns()
        {
            _germanNouns = new List<string>();
            var inputFile = Path.Combine(Configuration.DictionariesDirectory, "deu_Nouns.txt");
            if (File.Exists(inputFile))
            {
                _germanNouns = FileUtil.ReadAllLinesShared(inputFile, Encoding.UTF8);
            }
        }

        public string UppercaseNouns(string text)
        {
            var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
            if (textNoTags != textNoTags.ToUpperInvariant() && !string.IsNullOrEmpty(text))
            {
                var st = new StrippableText(text);
                st.FixCasing(_germanNouns, true, false, false, string.Empty);
                return st.MergedString;
            }

            return text;
        }
    }
}
