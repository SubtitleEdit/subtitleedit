using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class TimeCodesFileHelper
    {
        private static string GetTimeCodesFileName(string videoFileName)
        {
            var dir = Configuration.TimeCodesDirectory.TrimEnd(Path.DirectorySeparatorChar);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var newFileName = MovieHasher.GenerateHash(videoFileName) + ".timecodes";
            newFileName = Path.Combine(dir, newFileName);
            return newFileName;
        }

        /// <summary>
        /// Load time codes from file
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        /// <returns>List of time codes in seconds</returns>
        public static List<double> FromDisk(string videoFileName)
        {
            var list = new List<double>();

            if (string.IsNullOrEmpty(videoFileName))
            {
                return list;
            }

            var timeCodesFileName = GetTimeCodesFileName(videoFileName);
            if (!File.Exists(timeCodesFileName))
            {
                return list;
            }

            foreach (var line in File.ReadLines(timeCodesFileName))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    list.Add(double.Parse(line, CultureInfo.InvariantCulture));
                }
            }

            return list;
        }

        /// <summary>
        /// Saves time codes
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        /// <param name="list">List of time codes in seconds</param>
        public static void SaveTimeCodes(string videoFileName, List<double> list)
        {
            var sb = new StringBuilder();
            foreach (var d in list)
            {
                sb.AppendLine(d.ToString(CultureInfo.InvariantCulture));
            }
            File.WriteAllText(GetTimeCodesFileName(videoFileName), sb.ToString().Trim());
        }

        /// <summary>
        /// Delete time codes file associated with video file
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        public static void DeleteTimeCodes(string videoFileName)
        {
            var timeCodesFileName = GetTimeCodesFileName(videoFileName);
            if (File.Exists(timeCodesFileName))
            {
                File.Delete(timeCodesFileName);
            }
        }
    }
}