using Nikse.SubtitleEdit.Core.Common;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class VoskModel
    {
        public string Url { get; set; }
        public string TwoLetterLanguageCode { get; set; }
        public string LanguageName { get; set; }

        public override string ToString()
        {
            return LanguageName;
        }

        public static string ModelFolder => Path.Combine(Configuration.DataDirectory, "Vosk");

        public static readonly VoskModel[] Models = new[]
        {
            new VoskModel
            {
                TwoLetterLanguageCode = "en",
                LanguageName = "English (medium size, 128 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-en-us-0.22-lgraph.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "en",
                LanguageName = "English (very large, 1.8 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-en-us-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "cn",
                LanguageName = "Chinese (small, 42 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-cn-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "cn",
                LanguageName = "Chinese (very large, 1.3 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-cn-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "fr",
                LanguageName = "French (small, 39 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-fr-pguyot-0.3.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "fr",
                LanguageName = "French (large, 1.4 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-fr-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "es",
                LanguageName = "Spanish (small, 39 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-es-0.42.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "es",
                LanguageName = "Spanish (large, 1.4 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-es-0.42.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "ko",
                LanguageName = "Korean (small, 83 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-ko-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "de",
                LanguageName = "German (small, 45 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-de-0.15.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "de",
                LanguageName = "German (large, 1.9 GB)" ,
                Url = "https://alphacephei.com/vosk/models/vosk-model-de-0.21.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "pt",
                LanguageName = "Portuguese (small, 31 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-pt-0.3.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "pt",
                LanguageName = "Portuguese (large, 1.6 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-pt-fb-v0.1.1-20220516_2113.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "it",
                LanguageName = "Italian (small, 48 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-it-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "it",
                LanguageName = "Italian (large, 1.2 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-it-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "nl",
                LanguageName = "Dutch (large, 860 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-nl-spraakherkenning-0.6-lgraph.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "sv",
                LanguageName = "Swedish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-sv-rhasspy-0.15.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "ru",
                LanguageName = "Russian (small, 45 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-ru-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "ru",
                LanguageName = "Russian (large, 1.8 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-ru-0.42.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "fa",
                LanguageName = "Farsi",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-fa-0.5.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "tr",
                LanguageName = "Turkish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-tr-0.3.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "el",
                LanguageName = "Greek",
                Url = "https://alphacephei.com/vosk/models/vosk-model-el-gr-0.7.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "ar",
                LanguageName = "Arabic (small, 318 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-ar-mgb2-0.4.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "ar",
                LanguageName = "Arabic (large, 1.3 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-ar-0.22-linto-1.1.0.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "uk",
                LanguageName = "Ukrainian (small, 133 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-uk-v3-small.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "uk",
                LanguageName = "Ukrainian (medium, 325 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-uk-v3-lgraph.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "uz",
                LanguageName = "Uzbek (small, 49 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-uz-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "ph",
                LanguageName = "Filipino",
                Url = "https://alphacephei.com/vosk/models/vosk-model-tl-ph-generic-0.6.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "kz",
                LanguageName = "Kazakh",
                Url = "https://alphacephei.com/vosk/models/vosk-model-kz-0.15.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "jp",
                LanguageName = "Japanese (small, 48 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-ja-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "jp",
                LanguageName = "Japanese (large, 1 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-ja-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "ca",
                LanguageName = "Catalan",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-ca-0.4.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "hi",
                LanguageName = "Hindi (small, 42 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-hi-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "hi",
                LanguageName = "Hindi (large, 1.5 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-hi-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "cz",
                LanguageName = "Czech",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-cs-0.4-rhasspy.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "pl",
                LanguageName = "Polish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-pl-0.22.zip",
            },
            new VoskModel
            {
                TwoLetterLanguageCode = "br",
                LanguageName = "Breton (small, 70 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-br-0.8.zip",
            },
        };
    }
}