using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
    public abstract class Hunspell : IDisposable
    {
        public static Hunspell GetHunspell(string dictionary)
        {
            // Finnish uses Voikko (not available via hunspell)
            if (dictionary.EndsWith("fi_fi", StringComparison.OrdinalIgnoreCase))
            {
                return new VoikkoSpellCheck(Configuration.BaseDirectory, Configuration.DictionariesDirectory);
            }

            if (Configuration.IsRunningOnLinux)
            {
                return new LinuxHunspell(dictionary + ".aff", dictionary + ".dic");
            }

            if (Configuration.IsRunningOnMac)
            {
                return new MacHunspell(dictionary + ".aff", dictionary + ".dic");
            }

            return new WindowsHunspell(dictionary + ".aff", dictionary + ".dic");
        }

        public abstract bool Spell(string word);
        public abstract List<string> Suggest(string word);

        public virtual void Dispose()
        {
        }

        protected void AddIShouldBeLowercaseLSuggestion(List<string> suggestions, string word)
        {
            if (suggestions == null)
            {
                return;
            }

            // "I" can often be an ocr bug - should really be "l"
            if (word.Length > 1 && word.StartsWith('I') && !suggestions.Contains("l" + word.Substring(1)) && Spell("l" + word.Substring(1)))
            {
                suggestions.Add("l" + word.Substring(1));
            }
        }

    }
}
