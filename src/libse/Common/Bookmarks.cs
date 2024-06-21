using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class Bookmarks
    {
        private readonly Subtitle _subtitle;

        public Bookmarks(Subtitle subtitle)
        {
            _subtitle = subtitle;
        }

        private string GetBookmarksFileName()
        {
            return _subtitle.FileName + ".SE.bookmarks";
        }

        public bool Save()
        {
            if (_subtitle.FileName == null)
            {
                return false;
            }

            var fileName = GetBookmarksFileName();
            var serializedBookmarks = SerializeBookmarks();

            try
            {
                if (serializedBookmarks != null)
                {
                    File.WriteAllText(fileName, serializedBookmarks, Encoding.UTF8);
                }
                else if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string SerializeBookmarks()
        {
            int count = 0;
            var sb = new StringBuilder();
            sb.AppendLine("{\"bookmarks\":[");
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];
                if (p.Bookmark != null)
                {
                    count++;
                    if (count > 1)
                    {
                        sb.Append(",");
                    }
                    sb.Append("{\"idx\":" + i + ",\"txt\":\"" + Json.EncodeJsonText(p.Bookmark) + "\"}");
                }
            }
            sb.AppendLine("]}");
            if (count > 0)
            {
                return sb.ToString();
            }

            return null;
        }

        public bool Load()
        {
            var fileName = GetBookmarksFileName();
            if (!File.Exists(fileName))
            {
                return false;
            }

            try
            {
                var dic = DeserializeBookmarks(File.ReadAllText(fileName, Encoding.UTF8));
                foreach (var kvp in dic)
                {
                    var p = _subtitle.GetParagraphOrDefault(kvp.Key);
                    if (p != null)
                    {
                        p.Bookmark = kvp.Value;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Dictionary<int, string> DeserializeBookmarks(string input)
        {
            var dic = new Dictionary<int, string>();
            var s = input.Trim();
            var bookmarks = Json.ReadObjectArray(s.Substring(s.IndexOf('[')).TrimEnd('}'));
            if (bookmarks == null || bookmarks.Count == 0)
            {
                return dic;
            }

            foreach (var bm in bookmarks)
            {
                var idx = Json.ReadTag(bm, "idx");
                var txt = Json.DecodeJsonText(Json.ReadTag(bm, "txt"));
                if (int.TryParse(idx, out var number))
                {
                    dic.Add(number, txt);
                }
            }

            return dic;
        }

    }
}
