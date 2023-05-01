using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class WhisperCTranslate2Model : IWhisperModel
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

        private readonly string[] _fileNames = { "model.bin", "config.json", "vocabulary.txt", "tokenizer.json" };

        public string ModelFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "huggingface", "hub");

        public void CreateModelFolder()
        {
            var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            cacheFolder = Path.Combine(cacheFolder, "hub");
            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
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
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-tiny.en/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-tiny.en/snapshots/7d45cf02c1ed72d240c0dbf99d544d19bef1b5a3",
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-tiny/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-tiny/snapshots/518d6e0b5a068b278f66842b17377f9523de5cd1",
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-base.en/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-base.en/snapshots/88b03866a4066bb4a97c12258abb82b1e9af0121",
            },
            new WhisperModel
            {
                Name = "base",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-base/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-base/snapshots/a80717a3a48b1b28aa687bca146cb7301feae1b1",
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-small.en/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-small.en/snapshots/e0e3c0a16c844a994ca4d6d1318ce35f68236052",
            },
            new WhisperModel
            {
                Name = "small",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-small/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-small/snapshots/2ec96c5472da50d38d40c0cfe0602af2e94b4c8a",
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-medium/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-medium/snapshots/7832330bcea9a8d5fd6d6637c49fe5d256e98277",
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-medium.en/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-medium.en/snapshots/f972a5fa3be9378617b8fedc0bb00facbdbb1bf9",
            },
            new WhisperModel
            {
                Name = "large-v1",
                Size = "2.9 GB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-large-v1/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-large-v1/snapshots/e46e40f0408853a954b7ded6683808b7ecd14390",
            },
            new WhisperModel
            {
                Name = "large-v2",
                Size = "2.9 GB",
                Urls = MakeUrls("https://huggingface.co/guillaumekln/faster-whisper-large-v2/resolve/main"),
                Folder = "models--guillaumekln--faster-whisper-large-v2/snapshots/fecb99cc227a240ccd295d99b6c9026e7a179508",
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
