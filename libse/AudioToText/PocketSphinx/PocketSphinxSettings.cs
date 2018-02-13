using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.AudioToText.PocketSphinx
{
    public class PocketSphinxSettings
    {
        /// <summary>
        /// Language name + ISO 639-1 language code
        /// Languages available: https://sourceforge.net/projects/cmusphinx/files/Acoustic%20and%20Language%20Models/
        /// </summary>
        public Dictionary<string, string> Languages => new Dictionary<string, string>
        {
            { "Dutch", "nl" },
            { "English", "en" },
            { "French", "fr" },
            { "German", "de" },
            { "Greek", "el" },
            { "Hindi", "hi" },
            { "Indian", "id" },
            { "Italian", "it" },
            { "Kazakh", "kk" },
            { "Mandarin", "zh" },
            { "Spanish", "es" },
            { "Russian", "ru" },
        };

        public string FfmpegWaveExtractionParameters => "-i \"{0}\" -vn -ar 24000 -ac 2 -ab 128 -vol 448 -f wav {2} \"{1}\"";
        public string VlcWaveExtractionParameters => "";
        public string Name => "PocketSphinx";

    }
}
