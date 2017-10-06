using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Casing
{
    public class NameCaseConverter : CaseConverter
    {
        // TODO: Maybe move this to SubtitleEditRegex.cs
        public static readonly Regex RegexNonWordChar = new Regex("\\W", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        protected readonly IList<string> _nameList;
        private ICollection<string> _namesInSubtitle;
        private CasingContext _context;

        public NameCaseConverter(IList<string> nameList)
        {
            _nameList = nameList;
            _namesInSubtitle = new HashSet<string>();
        }

        public override void Convert(Subtitle subtitle, CasingContext context)
        {
            _context = context;

            foreach (var p in subtitle.Paragraphs)
            {
                string text = DoNameCasing(p.Text);
                if (text.Equals(p.Text, StringComparison.Ordinal))
                {
                    continue;
                }
                // if no listener is bound, update paragraph directly (e.g: in cmd mode)
                if (!OnTextChanged(p, new CasingResult() { Before = p.Text, After = text }))
                {
                    p.Text = text;
                }
                Count++;
            }
        }

        public string DoNameCasing(string text)
        {
            for (int i = _nameList.Count - 1; i >= 0; i--)
            {
                string name = _nameList[i];
                // Duh ^_^!
                if (name.Length > text.Length)
                {
                    continue;
                }
                int idx = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                while (idx >= 0)
                {
                    if (ShouldFixName(text, name, idx))
                    {
                        text = text.Remove(idx, name.Length);
                        text = text.Insert(idx, name);
                        _namesInSubtitle.Add(name);
                    }
                    idx = text.IndexOf(name, idx + name.Length, StringComparison.OrdinalIgnoreCase);
                }
            }
            return text;
        }

        private bool ShouldFixName(string text, string name, int nameIdx)
        {
            string nameFromText = text.Substring(nameIdx, name.Length);

            // check if name is a word and not inside a word
            if (nameIdx > 0)
            {
                // check pre char
                string preChar = text[nameIdx - 1].ToString();
                if (!RegexNonWordChar.IsMatch(preChar))
                {
                    return false;
                }
            }

            // check post charmn
            if (nameIdx + name.Length < text.Length)
            {
                string postChar = text[nameIdx + name.Length].ToString();
                if (!RegexNonWordChar.IsMatch(postChar))
                {
                    return false;
                }

                // do not fix word like: don't
                if (_context.IsEnglish && nameFromText.Equals("don", StringComparison.OrdinalIgnoreCase) &&
                    (text[name.Length] == '`' || text[name.Length] == '\''))
                {
                    return false;
                }
            }

            // name already with correct casing
            if (nameFromText.Equals(name, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        protected virtual bool OnTextChanged(Paragraph p, CasingResult casingResult)
        {
            if (TextChanged == null)
            {
                return false;
            }
            TextChanged?.Invoke(this, new TextChangedEventArgs(p, casingResult));
            return true;
        }

        /// <summary>
        /// Lis of all names found in Subtitle.
        /// </summary>
        public ICollection<string> NamesInSubtitle => _namesInSubtitle;

        public event EventHandler<TextChangedEventArgs> TextChanged;

    }
}
