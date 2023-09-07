using Nikse.SubtitleEdit.Core.Common;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class WhisperCppModel : IWhisperModel
    {
        public string ModelFolder
        {
            get
            {
                if (!string.IsNullOrEmpty(Configuration.Settings.Tools.WhisperCppModelLocation) &&
                    Directory.Exists(Configuration.Settings.Tools.WhisperCppModelLocation))
                {
                    return Configuration.Settings.Tools.WhisperCppModelLocation;
                }

                return Path.Combine(Configuration.DataDirectory, "Whisper", "Cpp", "Models");
            }
        }

        public void CreateModelFolder()
        {
            var whisperFolder = Path.Combine(Configuration.DataDirectory, "Whisper");
            if (!Directory.Exists(whisperFolder))
            {
                Directory.CreateDirectory(whisperFolder);
            }

            whisperFolder = Path.Combine(whisperFolder, "Cpp");
            if (!Directory.Exists(whisperFolder))
            {
                Directory.CreateDirectory(whisperFolder);
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
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-tiny.en.bin" },
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin" },
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "141 MB",
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en.bin" },
            },
            new WhisperModel
            {
                Name = "base",
                Size = "141 MB",
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin" },
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "465 MB",
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.en.bin" },
            },
            new WhisperModel
            {
                Name = "small",
                Size = "465 MB",
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin" },
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.42 GB",
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-medium.en.bin" },
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.42 GB",
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin" },
            },
            new WhisperModel
            {
                Name = "large",
                Size = "2.88 GB",
                Urls = new []{ "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-large.bin" },
            },
        };
    }
}
