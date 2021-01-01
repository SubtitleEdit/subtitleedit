namespace Nikse.SubtitleEdit.Logic
{
    public static class LanguageSettings
    {
        private static Language _instance;

        public static Language Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Language();
                }

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
    }
}
