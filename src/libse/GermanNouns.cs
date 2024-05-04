using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core
{
    public class GermanNouns
    {
        private readonly List<string> _germanNouns;
        private readonly Dictionary<Regex, string> _regularExpressionList;

        public GermanNouns()
        {
            var inputFile = Path.Combine(Configuration.DictionariesDirectory, "deu_Nouns.txt");
            if (File.Exists(inputFile))
            {
                _germanNouns = FileUtil.ReadAllLinesShared(inputFile, Encoding.UTF8).Select(p => p.Trim()).Where(p => p.Length > 1).ToList();
            }

            _germanNouns = _germanNouns ?? new List<string>();
            
            _regularExpressionList = new Dictionary<Regex, string>
            {
                { new Regex(@"\bDas essen\b", RegexOptions.Compiled), "Das Essen" },
                { new Regex(@"\bdas essen\b", RegexOptions.Compiled), "das Essen" }
            };
        }

        public string UppercaseNouns(string text)
        {
            var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
            if (textNoTags.ContainsLetter(UnicodeCategory.LowercaseLetter))
            {
                var st = new StrippableText(text);
                if (IsNounsLoaded())
                {
                    st.FixCasing(_germanNouns, true, false, false, string.Empty);
                }

                foreach (var regex in _regularExpressionList.Keys)
                {
                    st.StrippedText = regex.Replace(st.StrippedText, _regularExpressionList[regex]);
                }

                return st.MergedString;
            }

            return text;
        }

        private bool IsNounsLoaded() => _germanNouns.Count > 0;
    }
}
