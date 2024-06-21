namespace Nikse.SubtitleEdit.Core.Settings
{
    public class WordListSettings
    {
        public string LastLanguage { get; set; }
        public string NamesUrl { get; set; }
        public bool UseOnlineNames { get; set; }

        public WordListSettings()
        {
            LastLanguage = "en-US";
            NamesUrl = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/main/Dictionaries/names.xml";
        }
    }
}