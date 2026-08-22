using System;
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

    /// <summary>
    /// The end an "extend to next shot change (or next subtitle)" should produce, or null when the
    /// line must be left alone.
    /// <para>
    /// The command exists to give a line as much reading time as possible without letting it cross a
    /// cut, which fixes the rules (issue #13811):
    /// </para>
    /// <list type="number">
    /// <item>the target is the <b>first</b> shot change at or after the current end - never a later
    /// one, or the line would span the cut it was supposed to stop at;</item>
    /// <item>it lands <paramref name="outCuesGapMs"/> before that cut (the beautify profile's out
    /// cues gap, so this command, the beautifier and the snap commands share one rule);</item>
    /// <item>it only ever extends - a target at or before the current end means "already where it
    /// should be", so nothing moves. Shortening a line is not what the user asked for.</item>
    /// </list>
    /// <para>
    /// The next subtitle's start minus <paramref name="minGapMs"/> caps the result (and is the only
    /// bound when no cut lies ahead - the "or next subtitle" half of the command), and a result
    /// longer than <paramref name="maxDurationMs"/> is dropped rather than clamped: a clamped end
    /// would sit in the middle of a shot, which is the opposite of the point.
    /// </para>
    /// </summary>
    public static double? GetExtendedEndMs(
        IReadOnlyList<double> shotChanges,
        double startMs,
        double endMs,
        double? nextStartMs,
        double outCuesGapMs,
        double minGapMs,
        double maxDurationMs)
    {
        double? newEndMs = null;
        foreach (var shotChange in shotChanges)
        {
            var shotChangeMs = shotChange * 1000.0;
            if (shotChangeMs >= endMs)
            {
                newEndMs = shotChangeMs - outCuesGapMs;
                break;
            }
        }

        if (nextStartMs.HasValue)
        {
            var nextStartMinusGapMs = nextStartMs.Value - minGapMs;
            newEndMs = newEndMs.HasValue ? Math.Min(newEndMs.Value, nextStartMinusGapMs) : nextStartMinusGapMs;
        }

        if (newEndMs == null || newEndMs.Value <= endMs)
        {
            return null;
        }

        var durationMs = newEndMs.Value - startMs;
        if (durationMs <= 0 || durationMs > maxDurationMs)
        {
            return null;
        }

        return newEndMs;
    }

    /// <summary>
    /// The start an "extend to previous shot change" should produce, or null when the line must be
    /// left alone - <see cref="GetExtendedEndMs"/> mirrored: the <b>last</b> shot change at or before
    /// the current start, plus the in cues gap so the line starts after the cut rather than on it,
    /// and only when that moves the start earlier. The previous subtitle's end plus
    /// <paramref name="minGapMs"/> is the floor.
    /// </summary>
    public static double? GetExtendedStartMs(
        IReadOnlyList<double> shotChanges,
        double startMs,
        double endMs,
        double? previousEndMs,
        double inCuesGapMs,
        double minGapMs,
        double maxDurationMs)
    {
        double? newStartMs = null;
        for (var i = shotChanges.Count - 1; i >= 0; i--)
        {
            var shotChangeMs = shotChanges[i] * 1000.0;
            if (shotChangeMs <= startMs)
            {
                newStartMs = shotChangeMs + inCuesGapMs;
                break;
            }
        }

        if (previousEndMs.HasValue)
        {
            var previousEndPlusGapMs = previousEndMs.Value + minGapMs;
            newStartMs = newStartMs.HasValue ? Math.Max(newStartMs.Value, previousEndPlusGapMs) : previousEndPlusGapMs;
        }

        if (newStartMs == null || newStartMs.Value >= startMs)
        {
            return null;
        }

        var durationMs = endMs - newStartMs.Value;
        if (durationMs <= 0 || durationMs > maxDurationMs)
        {
            return null;
        }

        return newStartMs;
    }

    /// <summary>
    /// The end a "snap selected lines' end to previous shot change" should produce, or null when the
    /// line must be left alone (issue #13948).
    /// <para>
    /// Snapping parks the out cue on the cut the line is currently running past, which means:
    /// </para>
    /// <list type="number">
    /// <item>the target is the shot change <b>on or before</b> the end. "On" is generous by just
    /// under a frame (<paramref name="frameDurationMs"/>), so an end already sitting on a cut snaps
    /// to that cut instead of skipping a whole shot backwards;</item>
    /// <item>it lands <paramref name="outCuesGapMs"/> <b>before</b> that cut - the beautify profile's
    /// out cues gap, the same rule the beautifier and the extend commands use. An out cue exactly on
    /// the cut is the thing the gap exists to prevent;</item>
    /// <item>the only veto is a result that would not leave a positive duration. Minimum/maximum
    /// display duration deliberately do not veto: the user asked for this cue to move, and silently
    /// doing nothing reads as a dead shortcut.</item>
    /// </list>
    /// </summary>
    public static double? GetSnappedEndMs(
        List<double> shotChanges,
        double startMs,
        double endMs,
        double outCuesGapMs,
        double frameDurationMs)
    {
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return null;
        }

        var maxDifference = (frameDurationMs - 1) / 1000;
        var shotChangeSeconds = shotChanges.FirstOnOrBefore(endMs / 1000.0, maxDifference, -1);
        if (shotChangeSeconds < 0)
        {
            return null;
        }

        var newEndMs = shotChangeSeconds * 1000.0 - outCuesGapMs;
        if (newEndMs <= startMs)
        {
            return null;
        }

        return newEndMs;
    }

    /// <summary>
    /// The start a "snap selected lines' start to next shot change" should produce, or null when the
    /// line must be left alone - <see cref="GetSnappedEndMs"/> mirrored: the shot change on or after
    /// the start, plus <paramref name="inCuesGapMs"/> so the in cue lands after the cut rather than
    /// on it, vetoed only when it would not leave a positive duration.
    /// </summary>
    public static double? GetSnappedStartMs(
        List<double> shotChanges,
        double startMs,
        double endMs,
        double inCuesGapMs,
        double frameDurationMs)
    {
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return null;
        }

        var maxDifference = (frameDurationMs - 1) / 1000;
        var shotChangeSeconds = shotChanges.FirstOnOrAfter(startMs / 1000.0, maxDifference, -1);
        if (shotChangeSeconds < 0)
        {
            return null;
        }

        var newStartMs = shotChangeSeconds * 1000.0 + inCuesGapMs;
        if (newStartMs >= endMs)
        {
            return null;
        }

        return newStartMs;
    }

    /// <summary>
    /// The new start and end a "snap selected lines to nearest shot change" should produce, or null
    /// when the line must be left alone.
    /// <para>
    /// Each cue is snapped independently to its nearest shot change within its own capture distance
    /// (<paramref name="maxStartDistanceMs"/> / <paramref name="maxEndDistanceMs"/>), landing the
    /// profile's in/out cues gap either side of the cut - the same landing rule as the waveform drag
    /// and the start/end snap shortcuts, so every way of snapping a cue to a cut puts it in the same
    /// place (issues #13948, #13984).
    /// </para>
    /// <para>
    /// When both cues find the <b>same</b> cut the line straddles it, and snapping both would
    /// collapse it onto the cut. The start keeps that cut (the nearest one by construction) and the
    /// end retries within the tighter <paramref name="maxSameShotEndDistanceMs"/> for a cut further
    /// on; if there is none the end stays put.
    /// </para>
    /// <para>
    /// Like the start/end snap shortcuts, the only veto is a result that would not leave a positive
    /// duration. Minimum/maximum display duration deliberately do not veto: the user asked for this
    /// line to move, and a silently ignored shortcut reads as a dead one.
    /// </para>
    /// </summary>
    public static (double StartMs, double EndMs)? GetSnappedToNearestMs(
        List<double> shotChanges,
        double startMs,
        double endMs,
        double inCuesGapMs,
        double outCuesGapMs,
        double maxStartDistanceMs,
        double maxEndDistanceMs,
        double maxSameShotEndDistanceMs)
    {
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return null;
        }

        var nearestStart = ClosestWithin(shotChanges, startMs, maxStartDistanceMs);
        var nearestEnd = ClosestWithin(shotChanges, endMs, maxEndDistanceMs);

        if (nearestStart == null && nearestEnd == null)
        {
            return null;
        }

        if (nearestStart != null && nearestEnd != null && nearestStart.Value == nearestEnd.Value)
        {
            // Straddling one cut: the start takes it, and the end only moves if the next cut
            // *after* that one sits within the same-shot distance - otherwise it stays where it
            // is. A nearest-overall retry would just find the straddled cut again.
            nearestEnd = FirstAfterWithin(shotChanges, nearestStart.Value, endMs, maxSameShotEndDistanceMs);
        }

        var newStartMs = nearestStart != null ? nearestStart.Value + inCuesGapMs : startMs;
        var newEndMs = nearestEnd != null ? nearestEnd.Value - outCuesGapMs : endMs;

        if (newEndMs <= newStartMs)
        {
            return null;
        }

        if (newStartMs == startMs && newEndMs == endMs)
        {
            return null;
        }

        return (newStartMs, newEndMs);
    }

    // The shot change (in ms) nearest to targetMs, or null when none lies strictly within
    // maxDistanceMs. Shot changes are seconds on disk; the comparison is done in ms.
    private static double? ClosestWithin(List<double> shotChanges, double targetMs, double maxDistanceMs)
    {
        var closestSeconds = shotChanges.ClosestTo(targetMs / 1000.0);
        var closestMs = closestSeconds * 1000.0;
        return Math.Abs(closestMs - targetMs) < maxDistanceMs ? closestMs : null;
    }

    // The first shot change (in ms) strictly after afterMs, or null when there is none or it lies
    // outside maxDistanceMs of targetMs. The list is sorted, so a binary search finds the spot.
    private static double? FirstAfterWithin(List<double> shotChanges, double afterMs, double targetMs, double maxDistanceMs)
    {
        var index = shotChanges.BinarySearch(afterMs / 1000.0);
        index = index < 0 ? ~index : index + 1;
        if (index >= shotChanges.Count)
        {
            return null;
        }

        var candidateMs = shotChanges[index] * 1000.0;
        return Math.Abs(candidateMs - targetMs) < maxDistanceMs ? candidateMs : null;
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
