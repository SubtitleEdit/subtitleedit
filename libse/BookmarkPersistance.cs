using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core
{
    public class BookmarkPersistance
    {
        private readonly Subtitle _subtitle;

        public BookmarkPersistance(Subtitle subtitle)
        {
            _subtitle = subtitle;
        }

        public void SaveToFirstLine()
        {
            var b = SerializeBookmarks();
            if (b != null)
            {
                var first = _subtitle.Paragraphs.FirstOrDefault(p => p.StartTime.TotalMilliseconds == 0 && p.EndTime.TotalMilliseconds == 0 && p.Text.Contains("\"bookmarks\""));
                if (first != null)
                    _subtitle.Paragraphs.Remove(first);
                _subtitle.Paragraphs.Insert(0, new Paragraph(b, 0, 0));
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

        public void LoadFromFirstLine()
        {
            var first = _subtitle.Paragraphs.FirstOrDefault(p => p.StartTime.TotalMilliseconds == 0 && p.EndTime.TotalMilliseconds == 0 && p.Text.Contains("\"bookmarks\""));
            if (first == null)
                return;

            var dic = DeserializeBookmarks(first.Text);
            _subtitle.Paragraphs.Remove(first);
            foreach (var kvp in dic)
            {
                var p = _subtitle.GetParagraphOrDefault(kvp.Key);
                if (p != null)
                {
                    p.Bookmark = kvp.Value;
                }
            }
        }

        private Dictionary<int, string> DeserializeBookmarks(string s)
        {
            var dic = new Dictionary<int, string>();
            var bookmarks = Json.ReadObjectArray(s.Substring(s.IndexOf('[')).TrimEnd('}'));
            if (bookmarks == null || bookmarks.Count == 0)
            {
                return dic;
            }

            foreach (var bm in bookmarks)
            {
                var idx = Json.ReadTag(bm, "idx");
                var txt = Json.ReadTag(bm, "txt");
                int number;
                if (int.TryParse(idx, out number))
                {
                    dic.Add(number, txt);
                }
            }

            return dic;
        }

    }
}
