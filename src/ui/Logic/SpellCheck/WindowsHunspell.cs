using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
    public class WindowsHunspell : Hunspell
    {
        private NHunspell.Hunspell _hunspell;

        public WindowsHunspell(string affDictionary, string dicDictionary)
        {
            _hunspell = new NHunspell.Hunspell(affDictionary, dicDictionary);
        }

        public override bool Spell(string word)
        {
            return _hunspell.Spell(word);
        }

        public override List<string> Suggest(string word)
        {
            string filtered = Regex.Replace(word, @"\p{Cs}", "");
            var list = _hunspell.Suggest(filtered);
            AddIShouldBeLowercaseLSuggestion(list, filtered);
            return list;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_hunspell != null && !_hunspell.IsDisposed)
                {
                    _hunspell.Dispose();
                }
                _hunspell = null;
            }
        }

    }
}
