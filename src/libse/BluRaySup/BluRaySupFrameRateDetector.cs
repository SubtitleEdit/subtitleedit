using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    /// <summary>
    /// Finds the frame rate a Blu-ray sup was timed at from its PCS presentation times. The PCS
    /// frame_rate byte is often wrong: files written with a default of 25 fps declare 0x30 over
    /// 23.976 timing, and others declare 0x10 over 25 fps timing. Players like ffmpeg ignore the
    /// byte, so the times are the truth.
    /// </summary>
    public static class BluRaySupFrameRateDetector
    {
        /// <summary>
        /// A PTS within this many 90 kHz ticks of a frame counts as on it - a time stored in whole
        /// milliseconds is up to half a millisecond (45 ticks) off its frame.
        /// </summary>
        public const double ToleranceTicks = 46;

        /// <summary>
        /// The share of the times that must be on a frame rate's grid for it to be trusted.
        /// 29.97 fits about a third of 23.976 times, and 50 is a superset of 25, so this is high.
        /// </summary>
        public const double MinimumFit = 0.9;

        /// <summary>
        /// With fewer start times than this any grid fits by chance, so the declared rate is kept.
        /// </summary>
        public const int MinimumTimes = 10;

        private const double NearTie = 0.02;
        private const int MaxOrigins = 64;

        private struct StandardFrameRate
        {
            public double Fps;

            /// <summary>
            /// Rates of one family share (nearly) one grid - 25 is every other 50 frame, and 24 drifts
            /// off 23.976 only slowly - so a tie between them is resolved to the lower rate.
            /// </summary>
            public int Family;
        }

        // Ascending, so the first near-best rate is the lowest one.
        private static readonly StandardFrameRate[] StandardFrameRates =
        {
            new StandardFrameRate { Fps = Core.Fps24P, Family = 24 }, // 23.976
            new StandardFrameRate { Fps = Core.Fps24Hz, Family = 24 },
            new StandardFrameRate { Fps = Core.FpsPal, Family = 25 },
            new StandardFrameRate { Fps = Core.FpsNtsc, Family = 30 }, // 29.97
            new StandardFrameRate { Fps = Core.FpsPalI, Family = 25 },
            new StandardFrameRate { Fps = Core.FpsNtscI, Family = 30 }, // 59.94
        };

        /// <summary>
        /// The frame rate <paramref name="startTimes"/> (90 kHz PTS) are on. The declared rate
        /// (from the PCS frame_rate code <paramref name="framesPerSecondType"/>) is kept when the
        /// times fit it or are too few to tell, otherwise the best fitting standard rate is used -
        /// the lower one on a near tie within a family (25/50, 29.97/59.94, 23.976/24). 0 when no
        /// rate fits, or rates of different families fit equally well (e.g. whole-second times).
        /// </summary>
        public static double Detect(IList<long> startTimes, int framesPerSecondType)
        {
            var declared = BluRaySupPicture.GetFrameRate(framesPerSecondType);
            if (startTimes == null || startTimes.Count < MinimumTimes)
            {
                return declared;
            }

            if (declared > 0 && GetFit(startTimes, declared) >= MinimumFit)
            {
                return declared;
            }

            var fits = new double[StandardFrameRates.Length];
            var bestFit = 0.0;
            for (var i = 0; i < StandardFrameRates.Length; i++)
            {
                fits[i] = GetFit(startTimes, StandardFrameRates[i].Fps);
                bestFit = Math.Max(bestFit, fits[i]);
            }

            if (bestFit < MinimumFit)
            {
                return 0;
            }

            var result = 0.0;
            var family = 0;
            for (var i = 0; i < StandardFrameRates.Length; i++)
            {
                if (fits[i] < bestFit - NearTie)
                {
                    continue;
                }

                if (family == 0)
                {
                    result = StandardFrameRates[i].Fps;
                    family = StandardFrameRates[i].Family;
                }
                else if (family != StandardFrameRates[i].Family)
                {
                    return 0;
                }
            }

            return result;
        }

        /// <summary>
        /// The share (0-1) of <paramref name="pts"/> on one frame grid of <paramref name="fps"/>.
        /// The grid's origin is not known - a stream need not start on a frame at PTS 0 - so the
        /// best of origin 0 and the phases of a sample of the times themselves is used.
        /// </summary>
        public static double GetFit(IList<long> pts, double fps)
        {
            if (pts == null || pts.Count == 0 || fps <= 0)
            {
                return 0;
            }

            var frameTicks = 90000.0 / fps;
            var phases = new double[pts.Count];
            for (var i = 0; i < pts.Count; i++)
            {
                phases[i] = Mod(pts[i], frameTicks);
            }

            var best = CountOnGrid(phases, 0, frameTicks);
            var step = Math.Max(1, phases.Length / MaxOrigins);
            for (var i = 0; i < phases.Length; i += step)
            {
                best = Math.Max(best, CountOnGrid(phases, phases[i], frameTicks));
            }

            return (double)best / pts.Count;
        }

        private static int CountOnGrid(double[] phases, double origin, double frameTicks)
        {
            var count = 0;
            foreach (var phase in phases)
            {
                var distance = Math.Abs(phase - origin);
                if (Math.Min(distance, frameTicks - distance) <= ToleranceTicks)
                {
                    count++;
                }
            }

            return count;
        }

        private static double Mod(long value, double modulus)
        {
            var result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }
}
