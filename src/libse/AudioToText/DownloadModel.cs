namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class DownloadModel
    {
        public string Url { get; set; }
        public string TwoLetterLanguageCode { get; set; }
        public string LanguageName { get; set; }

        public override string ToString()
        {
            return LanguageName;
        }

        public static readonly DownloadModel[] VoskModels = new[]
        {
            new DownloadModel
            {
                TwoLetterLanguageCode = "en",
                LanguageName = "English (medium size, 128 MB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-en-us-0.22-lgraph.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "en",
                LanguageName = "English (very large, 1.8 GB)",
                Url = "https://alphacephei.com/vosk/models/vosk-model-en-us-0.22.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "cn",
                LanguageName = "Chinese",
                Url = "https://alphacephei.com/vosk/models/vosk-model-cn-kaldi-multicn-2-lgraph.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "fr",
                LanguageName = "French",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-fr-pguyot-0.3.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "es",
                LanguageName = "Spanish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-es-0.3.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "de",
                LanguageName = "German",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-de-0.15.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "pt",
                LanguageName = "Portuguese",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-pt-0.3.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "it",
                LanguageName = "Italian",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-it-0.4.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "nl",
                LanguageName = "Dutch",
                Url = "https://alphacephei.com/vosk/models/vosk-model-nl-spraakherkenning-0.6-lgraph.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "sv",
                LanguageName = "Swedish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-sv-rhasspy-0.15.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "ru",
                LanguageName = "Russian",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-ru-0.22.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "fa",
                LanguageName = "Farsi",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-fa-0.5.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "tr",
                LanguageName = "Turkish",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-tr-0.3.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "el",
                LanguageName = "Greek",
                Url = "https://alphacephei.com/vosk/models/vosk-model-el-gr-0.7.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "ar",
                LanguageName = "Arabic",
                Url = "https://alphacephei.com/vosk/models/vosk-model-ar-mgb2-0.4.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "uk",
                LanguageName = "Ukrainian",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-uk-v3-small.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "ph",
                LanguageName = "Filipino",
                Url = "https://alphacephei.com/vosk/models/vosk-model-tl-ph-generic-0.6.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "kz",
                LanguageName = "Kazakh",
                Url = "https://alphacephei.com/vosk/models/vosk-model-kz-0.15.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "jp",
                LanguageName = "Japanese",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-ja-0.22.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "ca",
                LanguageName = "Catalan",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-ca-0.4.zip",
            },
            new DownloadModel
            {
                TwoLetterLanguageCode = "hi",
                LanguageName = "Hindi",
                Url = "https://alphacephei.com/vosk/models/vosk-model-small-hi-0.22.zip",
            },
        };
    }
}