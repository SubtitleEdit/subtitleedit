using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private readonly string[] _fileNames = { "model.bin", "config.json", "vocabulary.txt", "vocabulary.json", "tokenizer.json", "preprocessor_config.json" };


        public string ModelFolder => Path.Combine(Configuration.DataDirectory, "Whisper", "Purfview-Faster-Whisper-XXL", "_models");

        public void CreateModelFolder()
        {
            var dir = Path.Combine(Configuration.DataDirectory, "Whisper", "Purfview-Faster-Whisper-XXL");
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
        public WhisperModel[] Models
        {
            get
            {
                var predefined = GetPredefinedModels();
                var custom = GetCustomModels(predefined);
                var all = new List<WhisperModel>(predefined);
                all.AddRange(custom);
                return all.ToArray();
            }
        }

        private List<WhisperModel> GetCustomModels(WhisperModel[] predefined)
        {
            var custom = new List<WhisperModel>();
            if (!Directory.Exists(ModelFolder))
            {
                return custom;
            }

            var predefinedFolders = new HashSet<string>(
                predefined.Select(m => m.Folder ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);

            foreach (var dir in Directory.GetDirectories(ModelFolder))
            {
                var folderName = Path.GetFileName(dir);
                if (predefinedFolders.Contains(folderName))
                {
                    continue;
                }

                // Must contain model.bin to be considered a valid model
                if (!File.Exists(Path.Combine(dir, "model.bin")))
                {
                    continue;
                }

                // Derive a display name: strip leading "faster-whisper-" prefix if present
                var displayName = folderName;
                if (displayName.StartsWith("faster-whisper-", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = displayName.Substring("faster-whisper-".Length);
                }

                custom.Add(new WhisperModel
                {
                    Name = displayName,
                    Size = "custom",
                    Urls = System.Array.Empty<string>(),
                    Folder = folderName,
                });
            }

            return custom;
        }

        private WhisperModel[] GetPredefinedModels() => new[]
        {
            new WhisperModel
            {
                Name = "tiny.en",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-tiny.en/resolve/main"),
                Folder = "faster-whisper-tiny.en",
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-tiny/resolve/main"),
                Folder = "faster-whisper-tiny",
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-base.en/resolve/main"),
                Folder = "faster-whisper-base.en",
            },
            new WhisperModel
            {
                Name = "base",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-base/resolve/main"),
                Folder = "faster-whisper-base",
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-small.en/resolve/main"),
                Folder = "faster-whisper-small.en",
            },
            new WhisperModel
            {
                Name = "small",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-small/resolve/main"),
                Folder = "faster-whisper-small",
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-medium.en/resolve/main"),
                Folder = "faster-whisper-medium.en",
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-medium/resolve/main"),
                Folder = "faster-whisper-medium",
            },
            new WhisperModel
            {
                Name = "large-v1",
                Size = "2.9 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v1/resolve/main"),
                Folder = "faster-whisper-large-v1",
            },
            new WhisperModel
            {
                Name = "large-v2",
                Size = "2.9 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v2/resolve/main"),
                Folder = "faster-whisper-large-v2",
            },
            new WhisperModel
            {
                Name = "large-v3-turbo",
                Size = "1.6 GB",
                Urls = MakeUrls("https://huggingface.co/mobiuslabsgmbh/faster-whisper-large-v3-turbo/resolve/main"),
                Folder = "faster-whisper-large-v3-turbo",
            },
            new WhisperModel
            {
                Name = "large-v3",
                Size = "3.1 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-whisper-large-v3/resolve/main"),
                Folder = "faster-whisper-large-v3",
            },

            new WhisperModel
            {
                Name = "distil-small.en",
                Size = "334 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-distil-whisper-small.en/resolve/main"),
                Folder = "faster-distil-whisper-small.en",
            },
            new WhisperModel
            {
                Name = "distil-medium.en",
                Size = "755 MB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-distil-whisper-medium.en/resolve/main"),
                Folder = "faster-distil-whisper-medium.en",
            },
            new WhisperModel
            {
                Name = "distil-large-v2",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-distil-whisper-large-v2/resolve/main"),
                Folder = "faster-distil-whisper-large-v2",
            },
            new WhisperModel
            {
                Name = "distil-large-v3",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/Systran/faster-distil-whisper-large-v3/resolve/main"),
                Folder = "faster-distil-whisper-large-v3",
            },

            new WhisperModel
            {
                Name = "distil-large-v3.5",
                Size = "1.5 GB English",
                Urls = MakeUrls("https://huggingface.co/Purfview/faster-distil-whisper-large-v3.5/resolve/main"),
                Folder = "faster-distil-whisper-large-v3.5",
            },

            new WhisperModel
            {
                Name = "tiny.nb",
                Size = "151 MB Norwegian",
                Urls = MakeUrls("https://huggingface.co/NbAiLab/nb-whisper-tiny/resolve/main"),
                Folder = "faster-whisper-tiny.nb",
            },
            new WhisperModel
            {
                Name = "base.nb",
                Size = "290 MB Norwegian",
                Urls = MakeUrls("https://huggingface.co/NbAiLab/nb-whisper-base/resolve/main"),
                Folder = "faster-whisper-base.nb",
            },
            new WhisperModel
            {
                Name = "small.nb",
                Size = "151 MB Norwegian",
                Urls = MakeUrls("https://huggingface.co/NbAiLab/nb-whisper-small/resolve/main"),
                Folder = "faster-whisper-small.nb",
            },
            new WhisperModel
            {
                Name = "medium.nb",
                Size = "3.1 GB Norwegian",
                Urls = MakeUrls("https://huggingface.co/NbAiLab/nb-whisper-medium/resolve/main"),
                Folder = "faster-whisper-medium.nb",
            },
            new WhisperModel
            {
                Name = "large.nb",
                Size = "6.2 GB Norwegian",
                Urls = MakeUrls("https://huggingface.co/NbAiLab/nb-whisper-large/resolve/main"),
                Folder = "faster-whisper-large.nb",
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
