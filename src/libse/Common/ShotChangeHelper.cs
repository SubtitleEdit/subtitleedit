using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

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

            if (string.IsNullOrEmpty(videoFileName))
            {
                return list;
            }

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

        public static double? GetPreviousShotChangeInMs(List<double> shotChanges, TimeCode currentTime)
        {
            try
            {
                return shotChanges.Last(x => SubtitleFormat.MillisecondsToFrames(x * 1000) <= SubtitleFormat.MillisecondsToFrames(currentTime.TotalMilliseconds));
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static double? GetPreviousShotChangePlusGapInMs(List<double> shotChanges, TimeCode currentTime)
        {
            var previousShotChangeInMs = GetPreviousShotChangeInMs(shotChanges, currentTime);
            if (previousShotChangeInMs != null)
            {
                return previousShotChangeInMs + TimeCodesBeautifierUtils.GetInCuesGapMs();
            }

            return null;
        }

        public static double? GetNextShotChangeInMs(List<double> shotChanges, TimeCode currentTime)
        {
            try
            {
                return shotChanges.First(x => SubtitleFormat.MillisecondsToFrames(x * 1000) >= SubtitleFormat.MillisecondsToFrames(currentTime.TotalMilliseconds));
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static double? GetNextShotChangeMinusGapInMs(List<double> shotChanges, TimeCode currentTime)
        {
            var nextShotChangeInMs = GetNextShotChangeInMs(shotChanges, currentTime);
            if (nextShotChangeInMs != null)
            {
                return nextShotChangeInMs - TimeCodesBeautifierUtils.GetOutCuesGapMs();
            }

            return null;
        }

        public static double? GetClosestShotChange(List<double> shotChanges, TimeCode currentTime)
        {
            try
            {
                return shotChanges.Aggregate((x, y) => Math.Abs(x - currentTime.TotalSeconds) < Math.Abs(y - currentTime.TotalSeconds) ? x : y);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static bool IsCueOnShotChange(List<double> shotChanges, TimeCode currentTime, bool isInCue)
        {
            var closestShotChange = GetClosestShotChange(shotChanges, currentTime);
            if (closestShotChange != null)
            {
                var currentFrame = SubtitleFormat.MillisecondsToFrames(currentTime.TotalMilliseconds);
                var closestShotChangeFrame = SubtitleFormat.MillisecondsToFrames(closestShotChange.Value * 1000);

                if (isInCue)
                {
                    return currentFrame >= closestShotChangeFrame && currentFrame <= closestShotChangeFrame + Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap;
                }
                else
                {
                    return currentFrame <= closestShotChangeFrame && currentFrame >= closestShotChangeFrame - Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
                }
            }
            else
            {
                return false;
            }
        }
    }
}