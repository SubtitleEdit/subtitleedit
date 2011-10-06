using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.SpellCheck
{
    public abstract class Hunspell
    {
        public static Hunspell GetHunspell(string dictionary)
        {
             if (Utilities.IsRunningOnLinux())
                    return new LinuxHunspell(dictionary + ".aff", dictionary + ".dic");
             else if (Utilities.IsRunningOnMac())
                 return new MacHunspell(dictionary + ".aff", dictionary + ".dic");

             return new WindowsHunspell(dictionary + ".aff", dictionary + ".dic");
        }

        public abstract bool Spell(string word);
        public abstract List<string> Suggest(string word);
    }
}

