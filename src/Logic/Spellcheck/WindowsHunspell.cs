using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
    public class WindowsHunspell: Hunspell
    {
        private NHunspell.Hunspell _hunspell;

        public WindowsHunspell (string affDictionary, string dicDictionary)
        {
            _hunspell = new NHunspell.Hunspell(affDictionary,dicDictionary);
        }

        public override bool Spell(string word)
        {
            return _hunspell.Spell(word);
        }

        public override List<string> Suggest(string word)
        {
            return _hunspell.Suggest(word);
        }

        ~WindowsHunspell()
        {
            if (_hunspell != null && !_hunspell.IsDisposed)
                _hunspell.Dispose();
        }

    }
}
