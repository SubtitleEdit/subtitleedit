using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
    public abstract class Hunspell : IDisposable
    {
        public static Hunspell GetHunspell(string dictionary)
        {
            if (Configuration.IsRunningOnLinux())
                return new LinuxHunspell(dictionary + ".aff", dictionary + ".dic");
            if (Configuration.IsRunningOnMac())
                return new MacHunspell(dictionary + ".aff", dictionary + ".dic");

            // Finnish uses Voikko (not available via hunspell)
            if (dictionary.EndsWith("fi_fi", StringComparison.OrdinalIgnoreCase))
                return new VoikkoSpellCheck(Configuration.BaseDirectory, Configuration.DictionariesFolder);

            return new WindowsHunspell(dictionary + ".aff", dictionary + ".dic");
        }

        public abstract bool Spell(string word);
        public abstract List<string> Suggest(string word);

        public virtual void Dispose()
        {
        }

    }
}
