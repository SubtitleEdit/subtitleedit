using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ShotChangeHelper
    {

        private static string GetShotChangesFileName(string videoFileName)
        {
            var dir = Configuration.ShotChangesDirectory.TrimEnd(Path.DirectorySeparatorChar);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var newFileName = MovieHasher.GenerateHash(videoFileName) + ".shotchanges";
            newFileName = Path.Combine(dir, newFileName);
            return newFileName;
        }

        /// <summary>
        /// Load shot changes from file
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        /// <returns>List of shot changes in seconds</returns>
        public static List<double> FromDisk(string videoFileName)
        {
            var list = new List<double>();
            var shotChangesFileName = GetShotChangesFileName(videoFileName);
            if (!File.Exists(shotChangesFileName))
            {
                return list;
            }

            foreach (var line in File.ReadLines(shotChangesFileName))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    list.Add(double.Parse(line, CultureInfo.InvariantCulture));
                }
            }
            return list;
        }

        /// <summary>
        /// Saves shot changes
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        /// <param name="list">List of shot changes in seconds</param>
        public static void SaveShotChanges(string videoFileName, List<double> list)
        {
            var sb = new StringBuilder();
            foreach (var d in list)
            {
                sb.AppendLine(d.ToString(CultureInfo.InvariantCulture));
            }
            File.WriteAllText(GetShotChangesFileName(videoFileName), sb.ToString().Trim());
        }

        /// <summary>
        /// Delete shot changes file associated with video file
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        public static void DeleteShotChanges(string videoFileName)
        {
            var shotChangesFileName = GetShotChangesFileName(videoFileName);
            if (File.Exists(shotChangesFileName))
            {
                File.Delete(shotChangesFileName);
            }
        }
    }
}