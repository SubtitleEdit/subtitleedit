using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class WhisperModel : IWhisperModel
    {
        public string[] Urls { get; set; }
        public string Size { get; set; }
        public string Name { get; set; }
        public bool Rename { get; set; }
        public string Folder { get; set; }
        public bool AlreadyDownloaded { get; set; }
        public long Bytes { get; set; }
        public bool Dynamic { get; set; }

        public override string ToString()
        {
            return $"{(AlreadyDownloaded ? "* " : string.Empty)}{Name} ({Size})";
        }

        public string ModelFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "whisper");

        public void CreateModelFolder()
        {
            var cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
            if (!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            if (!Directory.Exists(ModelFolder))
            {
                Directory.CreateDirectory(ModelFolder);
            }
        }

        // See https://github.com/openai/whisper/blob/main/whisper/__init__.py
        public WhisperModel[] Models => new[]
        {
            new WhisperModel
            {
                Name = "tiny.en",
                Size = "74 MB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/d3dd57d32accea0b295c96e26691aa14d8822fac7d9d27d5dc00b4ca2826dd03/tiny.en.pt" },
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/65147644a518d12f04e32d6f3b26facc3f8dd46e5390956a9424a650c0ce22b9/tiny.pt" },
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "142 MB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/25a8566e1d0c1e2231d1c762132cd20e0f96a85d16145c3a00adf5d1ac670ead/base.en.pt" },
            },
            new WhisperModel
            {
                Name = "base",
                Size = "142 MB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/ed3a0b6b1c0edf879ad9b11b1af5a0e6ab5db9205f891f668f8b0e6c6326e34e/base.pt" },
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "472 MB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/f953ad0fd29cacd07d5a9eda5624af0f6bcf2258be67c92b79389873d91e0872/small.en.pt" },
            },
            new WhisperModel
            {
                Name = "small",
                Size = "472 MB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/9ecf779972d90ba49c06d968637d720dd632c55bbf19d441fb42bf17a411e794/small.pt" },
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/345ae4da62f9b3d59415adc60127b97c714f32e89e936602e85993674d08dcb1/medium.pt" },
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.5 GB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/d7440d1dc186f76616474e0ff0b3b6b879abc9d1a4926b7adfa41db2d497ab4f/medium.en.pt" },
            },
            new WhisperModel
            {
                Name = "large-v2", // large-v2
                Size = "2.9 GB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/81f7c96c852ee8fc832187b0132e569d6c3065a3252ed18e56effd0b6a73e524/large-v2.pt" },
            },
            new WhisperModel
            {
                Name = "large-v3", // large-v3
                Size = "2.9 GB",
                Urls = new []{ "https://openaipublic.azureedge.net/main/whisper/models/e5b1a55b89c1367dacf97e3e19bfd829a01529dbfdeefa8caeb59b3f1b81dadb/large-v3.pt" },
            },
        };
    }
}
