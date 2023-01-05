using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public static class WhisperTimingFixer
    {
        private static int SecondsToSampleIndex(double seconds, int sampleRate)
        {
            return (int)Math.Round(seconds * sampleRate, MidpointRounding.AwayFromZero);
        }

        private static double FindPercentage(double startSeconds, double endSeconds, WavePeakData wavePeaks)
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

        public static Subtitle ShortenViaWavePeaks(Subtitle subtitle, WavePeakData wavePeaks)
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
                    prevEndSecs = s.Paragraphs[index].EndTime.TotalSeconds;
                }

                // Find nearest silence
                var startPos = p.StartTime.TotalSeconds;
                var pctHere = FindPercentage(startPos - 0.05, startPos + 0.05, wavePeaks);
                if (Math.Abs(pctHere - (-1)) < 0.01)
                {
                    if (p.Duration.TotalMilliseconds < minDurationMs)
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
                            if (p.Duration.TotalMilliseconds < minDurationMs)
                            {
                                s.Paragraphs[index] = oldP;
                            }

                            return s;
                        }

                        if (pct < percentageMax + 1 && p.Duration.TotalSeconds < 5)
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
                                    p.StartTime.TotalMilliseconds = x;
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
                                    p.StartTime.TotalMilliseconds = x;
                                }
                            }

                            break;
                        }

                        startPosBack -= 0.05;



                        var pctF = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                        if (Math.Abs(pctF - (-1)) < 0.01)
                        {
                            if (p.Duration.TotalMilliseconds < minDurationMs)
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
                    if (p.Duration.TotalMilliseconds < minDurationMs)
                    {
                        s.Paragraphs[index] = oldP;
                    }

                    return s;
                }

                if (pctHere < percentageMax)
                {
                    var startPosForward = p.StartTime.TotalSeconds;
                    while (pctHere < percentageMax && startPos < p.EndTime.TotalSeconds - 1)
                    {
                        pctHere = FindPercentage(startPosForward - 0.05, startPosForward + 0.05, wavePeaks);
                        if (Math.Abs(pctHere - (- 1)) < 0.01)
                        {
                            if (p.Duration.TotalMilliseconds < 1000)
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

                if (p.Duration.TotalMilliseconds < minDurationMs)
                {
                    s.Paragraphs[index] = oldP;
                }
            }

            return s;
        }

        public static Subtitle ShortenLongTexts(Subtitle subtitle)
        {
            var s = new Subtitle(subtitle);

            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (p.Duration.TotalMilliseconds > 8000)
                {
                    p.StartTime.TotalMilliseconds = p.EndTime.TotalMilliseconds - 5000;
                }
            }

            return s;
        }
    }
}
