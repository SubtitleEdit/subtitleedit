using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.Forms;

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


        // Util functions

        public static double GetPreviousShotChangeInMs(List<double> shotChanges, TimeCode currentTime)
        {
            var previousShotChangeInSeconds = new List<double> { double.MinValue }.Concat(shotChanges).Last(x => x <= currentTime.TotalSeconds); // will return minValue if none found
            return previousShotChangeInSeconds * 1000;
        }

        public static double GetPreviousShotChangePlusGapInMs(List<double> shotChanges, TimeCode currentTime)
        {
            return GetPreviousShotChangeInMs(shotChanges, currentTime) + TimeCodesBeautifierUtils.GetInCuesGapMs();
        }

        public static double GetNextShotChangeInMs(List<double> shotChanges, TimeCode currentTime)
        {
            var nextShotChangeInSeconds = shotChanges.Concat(new[] { double.MaxValue }).First(x => x >= currentTime.TotalSeconds); // will return maxValue if none found
            return nextShotChangeInSeconds * 1000;
        }

        public static double GetNextShotChangeMinusGapInMs(List<double> shotChanges, TimeCode currentTime)
        {
            return GetNextShotChangeInMs(shotChanges, currentTime) - TimeCodesBeautifierUtils.GetOutCuesGapMs();
        }
    }
}