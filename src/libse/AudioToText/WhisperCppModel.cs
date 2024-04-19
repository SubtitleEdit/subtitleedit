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

        private const string DownloadUrlPrefix = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/";

        public WhisperModel[] Models => new[]
        {
            new WhisperModel
            {
                Name = "tiny.en",
                Size = "74 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-tiny.en.bin" },
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-tiny.bin" },
            },
            new WhisperModel
            {
                Name = "tiny.en-q5_1",
                Size = "32 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-tiny.en-q5_1.bin" },
            },
            new WhisperModel
            {
                Name = "tiny-q5_1",
                Size = "32 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-tiny-q5_1.bin" },
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "141 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-base.en.bin" },
            },
            new WhisperModel
            {
                Name = "base",
                Size = "141 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-base.bin" },
            },
            new WhisperModel
            {
                Name = "base.en-q5_1",
                Size = "60 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-base.en-q5_1.bin" },
            },
            new WhisperModel
            {
                Name = "base-q5_1",
                Size = "60 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-base-q5_1.bin" },
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "465 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-small.en.bin" },
            },
            new WhisperModel
            {
                Name = "small",
                Size = "465 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-small.bin" },
            },
            new WhisperModel
            {
                Name = "small.en-q5_1.bin",
                Size = "190 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-small.en-q5_1.bin" },
            },
            new WhisperModel
            {
                Name = "small-q5_1",
                Size = "190 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-small-q5_1.bin" },
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.42 GB",
                Urls = new []{ DownloadUrlPrefix + "ggml-medium.en.bin" },
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.42 GB",
                Urls = new []{ DownloadUrlPrefix + "ggml-medium.bin" },
            },
            new WhisperModel
            {
                Name = "medium.en-q5_0",
                Size = "539 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-medium.en-q5_0.bin" },
            },
            new WhisperModel
            {
                Name = "medium-q5_0",
                Size = "539 MB",
                Urls = new []{ DownloadUrlPrefix + "ggml-medium-q5_0.bin" },
            },
            new WhisperModel
            {
                Name = "large-v1",
                Size = "2.88 GB",
                Urls = new []{ DownloadUrlPrefix + "ggml-large-v1.bin" },
            },
            new WhisperModel
            {
                Name = "large-v2",
                Size = "2.88 GB",
                Urls = new []{ DownloadUrlPrefix + "ggml-large-v2.bin" },
            },
            new WhisperModel
            {
                Name = "large",
                Size = "2.88 GB",
                Urls = new []{ DownloadUrlPrefix + "ggml-large-v3.bin" },
            },
            new WhisperModel
            {
                Name = "large-q5_0",
                Size = "2.88 GB",
                Urls = new []{ DownloadUrlPrefix + "ggml-large-v3-q5_0.bin" },
            },

            new WhisperModel
            {
                Name = "tiny.nb",
                Rename = true,
                Size = "78 MB Norwegian",
                Urls = new []{ "https://huggingface.co/NbAiLab/nb-whisper-tiny/resolve/main/ggml-model.bin" },
            },
            new WhisperModel
            {
                Name = "base.nb",
                Rename = true,
                Size = "148 MB Norwegian",
                Urls = new []{ "https://huggingface.co/NbAiLab/nb-whisper-base/resolve/main/ggml-model.bin" },
            },
            new WhisperModel
            {
                Name = "small.nb",
                Rename = true,
                Size = "488 MB Norwegian",
                Urls = new []{ "https://huggingface.co/NbAiLab/nb-whisper-small/resolve/main/ggml-model.bin" },
            },
            new WhisperModel
            {
                Name = "medium.nb",
                Rename = true,
                Size = "1.5 GB Norwegian",
                Urls = new []{ "https://huggingface.co/NbAiLab/nb-whisper-medium/resolve/main/ggml-model.bin" },
            },
            new WhisperModel
            {
                Name = "large.nb",
                Rename = true,
                Size = "3.1 GB Norwegian",
                Urls = new []{ "https://huggingface.co/NbAiLab/nb-whisper-large/resolve/main/ggml-model.bin" },
            },
        };
    }
}
