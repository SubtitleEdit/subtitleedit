using System.Collections.Generic;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core
{
    public class BookmarkPersistence
    {
        private readonly Subtitle _subtitle;
        private readonly string _fileName;

        public BookmarkPersistence(Subtitle subtitle, string fileName)
        {
            _subtitle = subtitle;
            _fileName = fileName;
        }

        private string GetBookmarksFileName()
        {
            return _fileName + ".SE.bookmarks";
        }

        public bool Save()
        {
            if (_fileName == null)
            {
                return false;
            }

            var fileName = GetBookmarksFileName();
            var b = SerializeBookmarks();
            if (b != null)
            {
                try
                {
                    File.WriteAllText(fileName, b, Encoding.UTF8);
                }
                catch
                {
                    return false;
                }
            }
            else if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    return false;
                }
            }
            return true;
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
            if (_fileName == null)
            {
                return false;
            }

            var fileName = GetBookmarksFileName();
            if (File.Exists(fileName))
            {
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

                }
                catch
                {
                    return false;
                }
            }

            return true;
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
