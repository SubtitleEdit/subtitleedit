using Nikse.SubtitleEdit.Core.Common;
using System.IO;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class WhisperCppModel : IWhisperModel
    {
        public string ModelFolder => Path.Combine(Configuration.DataDirectory, "Whisper", "Models");

        public void CreateModelFolder()
        {
            var whisperFolder = Path.Combine(Configuration.DataDirectory, "Whisper");
            if (!Directory.Exists(whisperFolder))
            {
                Directory.CreateDirectory(whisperFolder);
            }

            if (!Directory.Exists(ModelFolder))
            {
                Directory.CreateDirectory(ModelFolder);
            }
        }

        public WhisperModel[] Models  => new[] 
        {
            new WhisperModel
            {
                Name = "tiny.en",
                Size = "78 MB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-tiny.en.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-tiny.en.bin",
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "78 MB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-tiny.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin",
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "148 MB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-base.en.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-base.en.bin",
            },
            new WhisperModel
            {
                Name = "base",
                Size = "148 MB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-base.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-base.bin",
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "488 MB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-small.en.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-small.en.bin",
            },
            new WhisperModel
            {
                Name = "small",
                Size = "488 MB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-small.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-small.bin",
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.5 GB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-medium.en.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-medium.en.bin",
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-medium.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin",
            },
            new WhisperModel
            {
                Name = "large",
                Size = "3.1 GB",
                UrlSecondary = "https://ggml.ggerganov.com/ggml-model-whisper-large.bin",
                UrlPrimary = "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-large.bin",
            },
        };
    }
}
