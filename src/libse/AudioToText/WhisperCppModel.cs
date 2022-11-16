using System;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class WhisperModel : IWhisperModel
    {
        public string UrlPrimary { get; set; }
        public string UrlSecondary { get; set; }
        public string Size { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Size})";
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
        public  WhisperModel[] Models => new[]
        {
            new WhisperModel
            {
                Name = "tiny.en",
                Size = "74 MB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/d3dd57d32accea0b295c96e26691aa14d8822fac7d9d27d5dc00b4ca2826dd03/tiny.en.pt",
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/65147644a518d12f04e32d6f3b26facc3f8dd46e5390956a9424a650c0ce22b9/tiny.pt",
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "142 MB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/25a8566e1d0c1e2231d1c762132cd20e0f96a85d16145c3a00adf5d1ac670ead/base.en.pt",
            },
            new WhisperModel
            {
                Name = "base",
                Size = "142 MB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/ed3a0b6b1c0edf879ad9b11b1af5a0e6ab5db9205f891f668f8b0e6c6326e34e/base.pt",
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "472 MB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/d7440d1dc186f76616474e0ff0b3b6b879abc9d1a4926b7adfa41db2d497ab4f/medium.en.pt",
            },
            new WhisperModel
            {
                Name = "small",
                Size = "472 MB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/9ecf779972d90ba49c06d968637d720dd632c55bbf19d441fb42bf17a411e794/small.pt",
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.5 GB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/345ae4da62f9b3d59415adc60127b97c714f32e89e936602e85993674d08dcb1/medium.pt",
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/d7440d1dc186f76616474e0ff0b3b6b879abc9d1a4926b7adfa41db2d497ab4f/medium.en.pt",
            },
            new WhisperModel
            {
                Name = "large",
                Size = "2.1 GB",
                UrlSecondary = "https://openaipublic.azureedge.net/main/whisper/models/e4b87e7e0bf463eb8e6956e646f1e277e901512310def2c24bf0e11bd3c28e9a/large.pt",
            },
        }.OrderBy(p=>p.Name).ToArray();
    }
}
