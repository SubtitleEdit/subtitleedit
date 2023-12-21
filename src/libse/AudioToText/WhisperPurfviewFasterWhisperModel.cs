using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class WhisperPurfviewFasterWhisperModel : IWhisperModel
    {
        public string[] Urls { get; set; }
        public string Size { get; set; }
        public string Name { get; set; }
        public bool AlreadyDownloaded { get; set; }
        public long Bytes { get; set; }

        public override string ToString()
        {
            return $"{(AlreadyDownloaded ? "* " : string.Empty)}{Name} ({Size})";
        }

        private readonly string[] _fileNames = { "model.bin", "config.json", "vocabulary.txt", "vocabulary.json", "tokenizer.json" };


        public string ModelFolder => Path.Combine(Configuration.DataDirectory, "Whisper", "Purfview-Whisper-Faster", "_models");

        public void CreateModelFolder()
        {
            var dir = Path.Combine(Configuration.DataDirectory, "Whisper", "Purfview-Whisper-Faster");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!Directory.Exists(ModelFolder))
            {
                Directory.CreateDirectory(ModelFolder);
            }
        }

        // See https://github.com/jordimas/whisper-ctranslate2/blob/main/src/whisper_ctranslate2/models.py
        public WhisperModel[] Models => new[]
        {
            new WhisperModel
            {
                Name = "tiny.en",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-tiny.en",
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-tiny",
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-base.en",
            },
            new WhisperModel
            {
                Name = "base",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-base",
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-small.en",
            },
            new WhisperModel
            {
                Name = "small",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-small",
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-medium",
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-medium.en",
            },
            new WhisperModel
            {
                Name = "large-v1",
                Size = "2.9 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-large-v1",
            },
            new WhisperModel
            {
                Name = "large-v2",
                Size = "2.9 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-large-v2",
            },
            new WhisperModel
            {
                Name = "large-v3",
                Size = "3.1 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-large-v3",
            },
        };

        private string[] MakeUrls(string baseUrl)
        {
            var result = new List<string>();
            foreach (var fileName in _fileNames)
            {
                result.Add(baseUrl.TrimEnd('/') + "/" + fileName);
            }

            return result.ToArray();
        }
    }
}
