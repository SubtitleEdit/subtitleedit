using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Media;

public class ShotChangesHelper
{
    private static string GetShotChangesFileName(string videoFileName, int audioTrackNumber)
    {
        var dir = Se.ShotChangesFolder;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var videoFileNameWithoutExtension = Path.GetFileNameWithoutExtension(videoFileName)
            .Replace(".", string.Empty)
            .Replace("_", string.Empty);
        if (videoFileNameWithoutExtension.Length > 25)
        {
            videoFileNameWithoutExtension = videoFileNameWithoutExtension.Substring(0, 25);
        }
        
        var trackSuffix = audioTrackNumber >= 0 ? $"_{audioTrackNumber}" : string.Empty;

        var newFileName = $"{MovieHasher.GenerateHash(videoFileName)}{trackSuffix}_{videoFileNameWithoutExtension}.shotchanges";
        newFileName = Path.Combine(dir, newFileName);
        return newFileName;
    }

    /// <summary>
    /// Find shot changes file name
    /// </summary>
    /// <param name="videoFileName">Video file name</param>
    /// <returns>Return file name of existing shot changes, or null</returns>
    private static string FindShotChangesFileName(string videoFileName)
    {
        var dir = Se.ShotChangesFolder;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var videoFileNameWithoutExtension = Path.GetFileNameWithoutExtension(videoFileName)
            .Replace(".", string.Empty)
            .Replace("_", string.Empty);
        if (videoFileNameWithoutExtension.Length > 25)
        {
            videoFileNameWithoutExtension = videoFileNameWithoutExtension.Substring(0, 25);
        }

        var hash = MovieHasher.GenerateHash(videoFileName);

        var newFileName = Path.Combine(dir, $"{hash}_{videoFileNameWithoutExtension}.shotchanges");
        if (File.Exists(newFileName))
        {
            return newFileName;
        }

        var searchFileName = $"{hash}*.shotchanges";
        var files = Directory.GetFiles(dir, searchFileName);
        if (files.Length > 0)
        {
            return files[0];
        }

        return string.Empty;
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

        var shotChangesFileName = FindShotChangesFileName(videoFileName);
        if (string.IsNullOrEmpty(shotChangesFileName))
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
    /// <param name="audioTrackNumber">Audio track number, -1 if no track number</param>
    public static void SaveShotChanges(string videoFileName, List<double> list, int audioTrackNumber)
    {
        var sb = new StringBuilder();
        foreach (var d in list)
        {
            sb.AppendLine(d.ToString(CultureInfo.InvariantCulture));
        }
        
        File.WriteAllText(GetShotChangesFileName(videoFileName, audioTrackNumber), sb.ToString().Trim());
    }

    /// <summary>
    /// Delete shot changes file associated with video file
    /// </summary>
    /// <param name="videoFileName">Video file name</param>
    public static void DeleteShotChanges(string videoFileName, int audioTrackNumber)
    {
        var shotChangesFileName = GetShotChangesFileName(videoFileName, audioTrackNumber);
        if (File.Exists(shotChangesFileName))
        {
            File.Delete(shotChangesFileName);
        }
    }


    // Util functions

    public static double? GetPreviousShotChange(List<double> shotChanges, TimeCode currentTime)
    {
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return null;
        }

        var maxDifference = (TimeCodesBeautifierUtils.GetFrameDurationMs() - 1) / 1000;
        var previousShotChange = shotChanges.FirstOnOrBefore(currentTime.TotalSeconds, maxDifference, -1);
        if (previousShotChange >= 0)
        {
            return previousShotChange;
        }

        return null;
    }

    public static double? GetPreviousShotChangeInMs(List<double> shotChanges, TimeCode currentTime)
    {
        var previousShotChange = GetPreviousShotChange(shotChanges, currentTime);
        if (previousShotChange != null)
        {
            return previousShotChange * 1000;
        }

        return null;
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

    public static double? GetNextShotChange(List<double> shotChanges, TimeCode currentTime)
    {
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return null;
        }

        var maxDifference = (TimeCodesBeautifierUtils.GetFrameDurationMs() - 1) / 1000;
        var nextShotChange = shotChanges.FirstOnOrAfter(currentTime.TotalSeconds, maxDifference, -1);
        if (nextShotChange >= 0)
        {
            return nextShotChange;
        }

        return null;
    }

    public static double? GetNextShotChangeInMs(List<double> shotChanges, TimeCode currentTime)
    {
        var nextShotChange = GetNextShotChange(shotChanges, currentTime);
        if (nextShotChange != null)
        {
            return nextShotChange * 1000;
        }

        return null;
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
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return null;
        }

        return shotChanges.ClosestTo(currentTime.TotalSeconds);
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
