using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Media;
using System;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public static class SpeechToTextTimingFixer
{
    private static int SecondsToSampleIndex(double seconds, int sampleRate)
    {
        return (int)Math.Round(seconds * sampleRate, MidpointRounding.AwayFromZero);
    }

    private static double FindPercentage(double startSeconds, double endSeconds, WavePeakData2 wavePeaks)
    {
        var min = Math.Max(0, SecondsToSampleIndex(startSeconds, wavePeaks.SampleRate));
        var max = Math.Min(wavePeaks.Peaks.Count, SecondsToSampleIndex(endSeconds, wavePeaks.SampleRate));

        var maxPeak = int.MinValue;
        var count = 0;
        var total = 0;
        for (var i = min; i < max; i++)
        {
            var v = wavePeaks.Peaks[i].Abs;
            count++;
            total += v;
            if (v > maxPeak)
            {
                maxPeak = v;
            }
        }

        if (count == 0)
        {
            return -1;
        }

        var pctAvg = (total / (double)count) * 100.0 / wavePeaks.HighestPeak;
        var pctMax = maxPeak * 100.0 / wavePeaks.HighestPeak;
        return (pctAvg + pctMax + pctMax) / 3.0;
    }

    public static Subtitle ShortenViaWavePeaks(Subtitle subtitle, WavePeakData2 wavePeaks)
    {
        var minDurationMs = 600;

        var s = new Subtitle(subtitle);
        const double percentageMax = 7.0;

        for (var index = 0; index < s.Paragraphs.Count; index++)
        {
            var p = s.Paragraphs[index];
            var oldP = new Paragraph(p);
            var prevEndSecs = -1.0;
            if (index > 0)
            {
                prevEndSecs = s.Paragraphs[index - 1].EndTime.TotalSeconds;
            }

            // Find nearest silence
            var startPos = p.StartTime.TotalSeconds;
            var pctHere = FindPercentage(startPos - 0.05, startPos + 0.05, wavePeaks);
            if (Math.Abs(pctHere - (-1)) < 0.01)
            {
                if (p.DurationTotalMilliseconds < minDurationMs)
                {
                    s.Paragraphs[index] = oldP;
                }

                return s;
            }

            if (pctHere > percentageMax)
            {
                var startPosBack = startPos;
                var startPosForward = startPos;
                for (var ms = 50; ms < 255; ms += 50)
                {
                    var pct = FindPercentage(startPosBack - 0.05, startPosBack + 0.05, wavePeaks);
                    if (Math.Abs(pct - (-1)) < 0.01)
                    {
                        if (p.DurationTotalMilliseconds < minDurationMs)
                        {
                            s.Paragraphs[index] = oldP;
                        }

                        return s;
                    }

                    if (pct < percentageMax + 1 && p.DurationTotalSeconds < 5)
                    {
                        startPosBack -= 0.025;
                        var pct2 = FindPercentage(startPosBack - 0.05, startPosBack + 0.05, wavePeaks);
                        if (pct2 < pct && pct2 >= 0)
                        {
                            var x = startPosBack;
                            if (x < 0)
                            {
                                x = 0;
                            }

                            if (x > prevEndSecs)
                            {
                                p.StartTime.TotalSeconds = x;
                            }
                        }
                        else
                        {
                            var x = startPosBack + 0.025;
                            if (x < 0)
                            {
                                x = 0;
                            }

                            if (x > prevEndSecs)
                            {
                                p.StartTime.TotalSeconds = x;
                            }
                        }

                        break;
                    }

                    startPosBack -= 0.05;



                    var pctF = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                    if (Math.Abs(pctF - (-1)) < 0.01)
                    {
                        if (p.DurationTotalMilliseconds < minDurationMs)
                        {
                            s.Paragraphs[index] = oldP;
                        }

                        return s;
                    }

                    if (pctF < percentageMax)
                    {
                        startPosForward -= 0.025;
                        var pct2 = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                        if (pct2 < pctF && pct2 >= 0)
                        {
                            p.StartTime.TotalSeconds = startPosForward;
                        }
                        else
                        {
                            p.StartTime.TotalSeconds = startPosForward + 0.025;
                        }

                        break;
                    }

                    startPosForward += 0.05;
                }
            }

            // find next non-silence
            startPos = p.StartTime.TotalSeconds;
            pctHere = FindPercentage(startPos - 0.05, startPos + 0.05, wavePeaks);
            if (Math.Abs(pctHere - (-1)) < 0.01)
            {
                if (p.DurationTotalMilliseconds < minDurationMs)
                {
                    s.Paragraphs[index] = oldP;
                }

                return s;
            }

            if (pctHere < percentageMax)
            {
                var startPosForward = p.StartTime.TotalSeconds;
                // Bound the scan by the variable that actually advances. "startPos" is assigned
                // once above and never changes inside the loop, so this guard was a constant: the
                // scan ran past the cue's own end until FindPercentage gave up, and the -1 exit
                // then abandoned the whole method, leaving every later line unadjusted.
                while (pctHere < percentageMax && startPosForward < p.EndTime.TotalSeconds - 1)
                {
                    pctHere = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                    if (Math.Abs(pctHere - (-1)) < 0.01)
                    {
                        if (p.DurationTotalMilliseconds < 1000)
                        {
                            s.Paragraphs[index] = oldP;
                        }

                        return s;
                    }

                    p.StartTime.TotalSeconds = startPosForward;
                    if (pctHere >= percentageMax)
                    {
                        startPosForward -= 0.025;
                        var pct2 = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                        if (pct2 < pctHere && pct2 >= 0)
                        {
                            p.StartTime.TotalSeconds -= 0.025;

                            pctHere = pct2;
                            startPosForward -= 0.025;
                            pct2 = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                            if (pct2 < pctHere && pct2 >= 0)
                            {
                                p.StartTime.TotalSeconds -= 0.025;
                            }
                        }

                        break;
                    }

                    startPosForward += 0.05;
                }
            }

            if (p.DurationTotalMilliseconds < minDurationMs)
            {
                s.Paragraphs[index] = oldP;
            }
        }

        return s;
    }

    /// <summary>
    /// Puts transcription results in time order and removes any overlap between
    /// neighbors.
    ///
    /// Engines are supposed to emit segments in order, but a broken slice/chunk
    /// mapping can send one backwards in time - Crisp ASR + Parakeet did this 18
    /// times in a 10 minute file (issue #13548), some of them by 10 seconds. That
    /// particular engine bug was fixed upstream in CrispASR v0.8.29
    /// (CrispStrobe/CrispASR#356/#357), but this guard stays: it is engine-agnostic,
    /// and users keep running older installs of every engine here. The
    /// merge step in <see cref="SpeechToTextPostProcessor"/> assumes sorted,
    /// non-overlapping input and fuses gap-free runs, which turns a small
    /// out-of-order emission into a huge overlapping cue, so this has to run
    /// before post-processing.
    ///
    /// An overlap is resolved by truncating the earlier paragraph at the start of
    /// the later one; that keeps every line at the time it was actually spoken
    /// instead of pushing text minutes away from its audio. When two paragraphs
    /// share a start time the later one is moved instead, since truncating would
    /// leave nothing of the first.
    /// </summary>
    public static Subtitle SortAndRemoveOverlaps(Subtitle subtitle)
    {
        var s = new Subtitle(subtitle);
        if (s.Paragraphs.Count < 2)
        {
            return s;
        }

        s.Paragraphs.Sort((a, b) =>
        {
            var byStart = a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds);
            return byStart != 0 ? byStart : a.EndTime.TotalMilliseconds.CompareTo(b.EndTime.TotalMilliseconds);
        });

        for (var i = 1; i < s.Paragraphs.Count; i++)
        {
            var prev = s.Paragraphs[i - 1];
            var p = s.Paragraphs[i];
            if (p.StartTime.TotalMilliseconds >= prev.EndTime.TotalMilliseconds)
            {
                continue;
            }

            if (p.StartTime.TotalMilliseconds > prev.StartTime.TotalMilliseconds)
            {
                prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
            }
            else
            {
                var durationMs = p.DurationTotalMilliseconds;
                p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Math.Max(0, durationMs);
            }
        }

        s.Renumber();

        return s;
    }

    public static Subtitle ShortenLongDuration(Subtitle subtitle)
    {
        var s = new Subtitle(subtitle);

        foreach (var p in s.Paragraphs)
        {
            if (p.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
            {
                p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
            }
        }

        return s;
    }
}

