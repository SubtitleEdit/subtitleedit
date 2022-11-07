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
                Size = "74 MB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-tiny.en.bin",
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-tiny.bin",
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "142 MB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-base.en.bin",
            },
            new WhisperModel
            {
                Name = "base",
                Size = "142 MB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-base.bin",
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "472 MB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-small.en.bin",
            },
            new WhisperModel
            {
                Name = "small",
                Size = "472 MB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-small.bin",
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.4 GB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-medium.en.bin",
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-medium.bin",
            },
            new WhisperModel
            {
                Name = "large",
                Size = "3.1 GB",
                Url = "https://ggml.ggerganov.com/ggml-model-whisper-large.bin",
            },
        };
    }
}
