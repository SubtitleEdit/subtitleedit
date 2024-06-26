using System.Linq;

namespace Nikse.SubtitleEdit.Forms.Translate.InputLanguage
{
    public class WindowInputLanguage : IInputLanguage
    {
        public string[] GetLanguages()
        {
            // English (United States) - US
            // French (France) - French
            // German (Germany) - German
            // Spanish (Spain) - Spanish
            return System.Windows.Forms.InputLanguage.InstalledInputLanguages
                .OfType<System.Windows.Forms.InputLanguage>()
                .Where(language => !string.IsNullOrEmpty(language.LayoutName))
                .Select(language => language.LayoutName)
                .ToArray();
        }
    }
}