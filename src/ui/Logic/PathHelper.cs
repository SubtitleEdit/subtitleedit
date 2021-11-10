using System.Drawing;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic
{
    public static class PathHelper
    {
        public static string ShortenPath(Graphics g, Font font, string path, int pixelMaxLength)
        {
            var s = path;
            while (g.MeasureString(s, font).Width > pixelMaxLength)
            {
                var arr = s.Split(Path.DirectorySeparatorChar).ToList();
                if (arr.Count < 3)
                {
                    return Path.GetFileName(path);
                }

                var middle = arr.Count / 2 - 1;
                if (middle == 0)
                {
                    middle++;
                }

                while (arr[middle] == "..." && middle < arr.Count - 1)
                {
                    middle++;
                }

                if (arr[middle] == "..." || middle >= arr.Count - 1)
                {
                    return Path.GetFileName(path);
                }

                arr[middle] = "...";
                s = string.Join(Path.DirectorySeparatorChar.ToString(), arr);
                s = s.Replace("\\...\\...", "\\...");
            }

            return s;
        }
    }
}
