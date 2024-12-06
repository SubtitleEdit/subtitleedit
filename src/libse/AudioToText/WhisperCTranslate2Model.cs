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

        public WhisperModel[] Models => new[]
        {
            new WhisperModel
            {
                Name = "tiny.en",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-tiny.en/resolve/main"),
                Folder = "models--Systran--faster-whisper-tiny.en/snapshots/0d3d19a32d3338f10357c0889762bd8d64bbdeba",
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-tiny/resolve/main"),
                Folder = "models--Systran--faster-whisper-tiny/snapshots/d90ca5fe260221311c53c58e660288d3deb8d356",
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-base.en/resolve/main"),
                Folder = "models--Systran--faster-whisper-base.en/snapshots/3d3d5dee26484f91867d81cb899cfcf72b96be6c",
            },
            new WhisperModel
            {
                Name = "base",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-base/resolve/main"),
                Folder = "models--Systran--faster-whisper-base/snapshots/ebe41f70d5b6dfa9166e2c581c45c9c0cfc57b66",
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-small.en/resolve/main"),
                Folder = "models--Systran--faster-whisper-small.en/snapshots/d1d751a5f8271d482d14ca55d9e2deeebbae577f",
            },
            new WhisperModel
            {
                Name = "small",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-small/resolve/main"),
                Folder = "models--Systran--faster-whisper-small/snapshots/536b0662742c02347bc0e980a01041f333bce120",
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-medium/resolve/main"),
                Folder = "models--Systran--faster-whisper-medium/snapshots/08e178d48790749d25932bbc082711ddcfdfbc4f",
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-medium.en/resolve/main"),
                Folder = "models--Systran--faster-whisper-medium.en/snapshots/a29b04bd15381511a9af671baec01072039215e3",
            },
            new WhisperModel
            {
                Name = "large-v1",
                Size = "3.1 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v1/resolve/main"),
                Folder = "models--Systran--faster-whisper-large-v1/snapshots/b07c8d4be0be90092aa01a29c975077acb8d15c9",
            },
            new WhisperModel
            {
                Name = "large-v2",
                Size = "3.1 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v2/resolve/main"),
                Folder = "models--Systran--faster-whisper-large-v2/snapshots/f0fe81560cb8b68660e564f55dd99207059c092e",
            },
            new WhisperModel
            {
                Name = "large-v3",
                Size = "3.1 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "models--Systran--faster-whisper-large-v3/snapshots/edaa852ec7e145841d8ffdb056a99866b5f0a478",
            },
            new WhisperModel
            {
                Name = "large-v3-turbo",
                Size = "1.6 GB",
                Urls = MakeUrls("https://huggingface.co/mobiuslabsgmbh/faster-whisper-large-v3-turbo/resolve/main"),
                Folder = "models--mobiuslabsgmbh--faster-whisper-large-v3-turbo/snapshots/0c94664816ec82be77b20e824c8e8675995b0029",
            },
            new WhisperModel
            {
                Name = "distil-small.en",
                Size = "332 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-distil-whisper-small.en/resolve/main"),
                Folder = "models--Systran--faster-distil-whisper-small.en/snapshots/ef77d90526ccd62cde3808ee70626a01e5cf83e4",
            },
            new WhisperModel
            {
                Name = "distil-medium.en",
                Size = "789 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-distil-whisper-medium.en/resolve/main"),
                Folder = "models--Systran--faster-distil-whisper-medium.en/snapshots/80ddfce281f77766d8943d63109199fc8145dfa5",
            },
            new WhisperModel
            {
                Name = "distil-large-v2",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-distil-whisper-large-v2/resolve/main"),
                Folder = "models--Systran--faster-distil-whisper-large-v2/snapshots/fe9b404fc56de3f7c38606ef9ba6fd83526d05e4",
            },
            new WhisperModel
            {
                Name = "distil-large-v3",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-distil-whisper-large-v3/resolve/main"),
                Folder = "models--Systran--faster-distil-whisper-large-v3/snapshots/c3058b475261292e64a0412df1d2681c06260fab",
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
