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

        public string ModelFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "whisper-ctranslate2");

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

        // See https://github.com/jordimas/whisper-ctranslate2/blob/main/src/whisper_ctranslate2/models.py
        public WhisperModel[] Models => new[]
        {
            new WhisperModel
            {
                Name = "tiny.en",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/c4d1be0a2003ce00bb721abd23e7a34925a6f0c0d21d5c416f11c763ee7f7b15.tiny-en/"),
                Folder = "c4d1be0a2003ce00bb721abd23e7a34925a6f0c0d21d5c416f11c763ee7f7b15.tiny-en",
            },
            new WhisperModel
            {
                Name = "tiny",
                Size = "74 MB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/dc6a7765cdc8ae7822b1c3068a2f966eddc2549eda7e67406cae915a8c19430c.tiny/"),
                Folder = "dc6a7765cdc8ae7822b1c3068a2f966eddc2549eda7e67406cae915a8c19430c.tiny",
            },
            new WhisperModel
            {
                Name = "base.en",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/2ca96777261aeadd48e30bc5f26fd3ee462f4921dbc1f38dfd826a67c761c9b2.base-en/"),
                Folder = "2ca96777261aeadd48e30bc5f26fd3ee462f4921dbc1f38dfd826a67c761c9b2.base-en",
            },
            new WhisperModel
            {
                Name = "base",
                Size = "142 MB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/d7c4df31737340263ce37933bda1e77a38367dbb09cda7433ce1ee0c58ce1a60.base/"),
                Folder = "d7c4df31737340263ce37933bda1e77a38367dbb09cda7433ce1ee0c58ce1a60.base",
            },
            new WhisperModel
            {
                Name = "small.en",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/e2c14bb0f6a8a69afe12fbe1d82fa0c41494d4bde9615bdf399da5665f43cbc4.small-en/"),
                Folder = "e2c14bb0f6a8a69afe12fbe1d82fa0c41494d4bde9615bdf399da5665f43cbc4.small-en",
            },
            new WhisperModel
            {
                Name = "small",
                Size = "472 MB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/936cd99363be80fa388c0c5006e846d8cd42834c6cf8156a7c300723a3bf929f.small/"),
                Folder = "936cd99363be80fa388c0c5006e846d8cd42834c6cf8156a7c300723a3bf929f.small",
            },
            new WhisperModel
            {
                Name = "medium",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/d8b91e278db3041c3b41bf879716281edf5cfa7b0025823cc174b5429877d2bc.medium/"),
                Folder = "d8b91e278db3041c3b41bf879716281edf5cfa7b0025823cc174b5429877d2bc.medium",
            },
            new WhisperModel
            {
                Name = "medium.en",
                Size = "1.5 GB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/22d42d3e69ce9149bfd52c07d357e4cb72b992fb602805d6bb39f331400d6742.mediu-en/"),
                Folder = "22d42d3e69ce9149bfd52c07d357e4cb72b992fb602805d6bb39f331400d6742.mediu-en",
            },
            //new WhisperModel - large-v1
            //{
            //    Name = "large",
            //    Size = "2.1 GB",
            //    Urls =  "MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/ea44e7a9609bd21a604b28880b85fc7dc2c373876c627a7553ce5440a8c406c1.large/"),
            //},
            new WhisperModel
            {
                Name = "large", // large-v2
                Size = "2.9 GB",
                Urls = MakeUrls("https://huggingface.co/datasets/jordimas/whisper-ct2-v2/resolve/main/ff9f410b63b3d996274c895f6209e9b9ab01d497815b21c2d35ae336ab7d7f20.large-v2/"),
                Folder = "ff9f410b63b3d996274c895f6209e9b9ab01d497815b21c2d35ae336ab7d7f20.large-v2",
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
