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
    /// Find shot changes file name. The file written for <paramref name="audioTrackNumber"/> wins
    /// (that is the name <see cref="SaveShotChanges"/> uses), then the track-less name, then any
    /// file for the video.
    /// </summary>
    /// <param name="videoFileName">Video file name</param>
    /// <param name="audioTrackNumber">Audio track number, -1 if no track number</param>
    /// <returns>Return file name of existing shot changes, or empty string</returns>
    private static string FindShotChangesFileName(string videoFileName, int audioTrackNumber)
    {
        if (audioTrackNumber >= 0)
        {
            var trackFileName = GetShotChangesFileName(videoFileName, audioTrackNumber);
            if (File.Exists(trackFileName))
            {
                return trackFileName;
            }
        }

        var newFileName = GetShotChangesFileName(videoFileName, -1);
        if (File.Exists(newFileName))
        {
            return newFileName;
        }

        var files = GetAllShotChangesFileNames(videoFileName);
        if (files.Length > 0)
        {
            return files[0];
        }

        return string.Empty;
    }

    // Every shot changes file for the video, whatever audio track it was written for - the set
    // FindShotChangesFileName picks from.
    private static string[] GetAllShotChangesFileNames(string videoFileName)
    {
        var dir = Se.ShotChangesFolder;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        return Directory.GetFiles(dir, $"{MovieHasher.GenerateHash(videoFileName)}*.shotchanges");
    }

    /// <summary>
    /// Load shot changes from file
    /// </summary>
    /// <param name="videoFileName">Video file name</param>
    /// <param name="audioTrackNumber">Audio track number, -1 if no track number</param>
    /// <returns>List of shot changes in seconds</returns>
    public static List<double> FromDisk(string videoFileName, int audioTrackNumber = -1)
    {
        var list = new List<double>();

        if (string.IsNullOrEmpty(videoFileName))
        {
            return list;
        }

        var shotChangesFileName = FindShotChangesFileName(videoFileName, audioTrackNumber);
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

        return EnsureSorted(list);
    }

    /// <summary>
    /// Sorts shot changes (in place) when they are out of order. Consumers binary-search the list,
    /// but an imported or hand-edited file keeps whatever order it was written in.
    /// </summary>
    public static List<double> EnsureSorted(List<double> shotChanges)
    {
        for (var i = 1; i < shotChanges.Count; i++)
        {
            if (shotChanges[i] < shotChanges[i - 1])
            {
                shotChanges.Sort();
                break;
            }
        }

        return shotChanges;
    }

    /// <summary>
    /// Saves shot changes. Other shot changes files for the same video (another audio track, or
    /// the track-less name) are removed, so every reader - whatever track it asks for - gets
    /// this list rather than a stale one.
    /// </summary>
    /// <param name="videoFileName">Video file name</param>
    /// <param name="list">List of shot changes in seconds</param>
    /// <param name="audioTrackNumber">Audio track number, -1 if no track number</param>
    public static void SaveShotChanges(string videoFileName, List<double> list, int audioTrackNumber)
    {
        var fileName = GetShotChangesFileName(videoFileName, audioTrackNumber);
        foreach (var other in GetAllShotChangesFileNames(videoFileName))
        {
            if (!string.Equals(other, fileName, StringComparison.OrdinalIgnoreCase))
            {
                TryDelete(other);
            }
        }

        File.WriteAllText(fileName, ToText(list));
    }

    /// <summary>
    /// Shot changes as text, the same as the .shotchanges files: one time in seconds per line,
    /// invariant culture.
    /// </summary>
    public static string ToText(IEnumerable<double> list)
    {
        var sb = new StringBuilder();
        foreach (var d in list)
        {
            sb.AppendLine(d.ToString(CultureInfo.InvariantCulture));
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Delete the shot changes associated with video file - every file <see cref="FromDisk"/>
    /// could read for it, not just the one for <paramref name="audioTrackNumber"/>, or an older
    /// file would bring the deleted shot changes back on reopen.
    /// </summary>
    /// <param name="videoFileName">Video file name</param>
    /// <param name="audioTrackNumber">Audio track number, -1 if no track number</param>
    public static void DeleteShotChanges(string videoFileName, int audioTrackNumber)
    {
        foreach (var fileName in GetAllShotChangesFileNames(videoFileName))
        {
            TryDelete(fileName);
        }
    }

    private static void TryDelete(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // ignore - a locked/read-only cache file is not worth failing the edit over
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
    /// <item>a shot change never shortens the line - a cut at or before the current end means
    /// "already where it should be", so nothing moves.</item>
    /// </list>
    /// <para>
    /// The next subtitle's start minus <paramref name="minGapMs"/> caps the result (and is the only
    /// bound when no cut lies ahead, or no shot changes are loaded - the "or next subtitle" half of
    /// the command), and a result longer than <paramref name="maxDurationMs"/> is dropped rather than
    /// clamped: a clamped end would sit in the middle of a shot, which is the opposite of the point.
    /// </para>
    /// <para>
    /// The minimum gap is the one bound that may pull the end backwards: a line that overlaps the
    /// next subtitle, or ends inside the minimum gap, is trimmed to the next start minus the gap
    /// like SE4 does (issue #15719) - as long as a positive duration is left.
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
        if (nextStartMs.HasValue)
        {
            var limitMs = nextStartMs.Value - minGapMs;
            if (limitMs < endMs)
            {
                return limitMs > startMs ? limitMs : null;
            }
        }

        // The first shot change at or after the end. The list is sorted, so binary search -
        // the command runs once per selected line, and a scan from the start made a select-all
        // cost lines x shot changes.
        double? newEndMs = null;
        var low = 0;
        var high = shotChanges.Count;
        while (low < high)
        {
            var middle = (low + high) >>> 1;
            if (shotChanges[middle] * 1000.0 >= endMs)
            {
                high = middle;
            }
            else
            {
                low = middle + 1;
            }
        }

        if (low < shotChanges.Count)
        {
            newEndMs = shotChanges[low] * 1000.0 - outCuesGapMs;
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
    /// <paramref name="minGapMs"/> is the floor - and the one bound that may move the start later,
    /// when the line overlaps the previous subtitle or starts inside the minimum gap (issue #15719).
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
        if (previousEndMs.HasValue)
        {
            var limitMs = previousEndMs.Value + minGapMs;
            if (limitMs > startMs)
            {
                return limitMs < endMs ? limitMs : null;
            }
        }

        // The last shot change at or before the start, by binary search (see GetExtendedEndMs).
        double? newStartMs = null;
        var low = 0;
        var high = shotChanges.Count;
        while (low < high)
        {
            var middle = (low + high) >>> 1;
            if (shotChanges[middle] * 1000.0 <= startMs)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        if (low > 0)
        {
            newStartMs = shotChanges[low - 1] * 1000.0 + inCuesGapMs;
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

        // Shot changes come from video frame positions, so cut ± gap is a fractional
        // millisecond. Round to the whole millisecond the subtitle will actually store; the
        // no-op guard below then compares like with like, so re-running the command on an
        // already-snapped line stays a no-op instead of reporting a change (#14056).
        var newStartMs = nearestStart != null ? Math.Round(nearestStart.Value + inCuesGapMs, MidpointRounding.AwayFromZero) : startMs;
        var newEndMs = nearestEnd != null ? Math.Round(nearestEnd.Value - outCuesGapMs, MidpointRounding.AwayFromZero) : endMs;

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

    /// <summary>
    /// Where a cue sits relative to its nearest shot change, for the grid's "Shot in"/"Shot out"
    /// columns: the signed distance cue minus cut (negative = the cue is before the cut), in
    /// milliseconds and in frames. False when there are no shot changes or the nearest one is more
    /// than <paramref name="maxDistanceFrames"/> away - a cue far from any cut has nothing to show.
    /// <para>
    /// Frames are counted the way the beautifier counts them (cue and cut each rounded to a frame,
    /// then subtracted), so the column and <see cref="IsCueInShotChangeZone"/> agree with what
    /// Beautify time codes would do to the cue.
    /// </para>
    /// </summary>
    public static bool TryGetShotChangeOffset(
        List<double> shotChanges,
        double cueMs,
        double frameRate,
        int maxDistanceFrames,
        out double offsetMs,
        out int offsetFrames)
    {
        offsetMs = 0;
        offsetFrames = 0;
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return false;
        }

        var shotChangeMs = shotChanges.ClosestTo(cueMs / 1000.0) * 1000.0;
        var frames = SubtitleFormat.MillisecondsToFrames(cueMs, frameRate) -
                     SubtitleFormat.MillisecondsToFrames(shotChangeMs, frameRate);
        if (Math.Abs(frames) > maxDistanceFrames)
        {
            return false;
        }

        offsetMs = cueMs - shotChangeMs;
        offsetFrames = frames;
        return true;
    }

    /// <summary>
    /// True when Beautify time codes would move a cue that is <paramref name="offsetFrames"/> from
    /// its nearest shot change: it sits in a red zone (which snaps to the cut plus the gap) or in a
    /// green zone (which pushes it out to the zone's edge), and is not already on the target
    /// <paramref name="gapFrames"/> - the in cues gap after the cut, or minus the out cues gap
    /// before it. Zone bounds match <c>TimeCodesBeautifier.FindBestCueFrame</c>: red zones are
    /// inclusive, green zones exclusive.
    /// </summary>
    public static bool IsCueInShotChangeZone(
        int offsetFrames,
        int gapFrames,
        int leftGreenZone,
        int leftRedZone,
        int rightRedZone,
        int rightGreenZone)
    {
        if (offsetFrames == gapFrames)
        {
            return false;
        }

        var inRedZone = offsetFrames >= -leftRedZone && offsetFrames <= rightRedZone;
        var inGreenZone = offsetFrames > -leftGreenZone && offsetFrames < rightGreenZone;
        return inRedZone || inGreenZone;
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
