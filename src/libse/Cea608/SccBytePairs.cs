using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    /// <summary>
    /// The CEA-608 byte pairs of a Scenarist (.scc) file, each on its own caption slot: CEA-608
    /// carries one byte pair per field every 1/29.97 second, so slot n is at n * 1001/30 ms.
    /// </summary>
    public static class SccBytePairs
    {
        public const double SlotMilliseconds = 1001.0 / 30;

        private static readonly Regex TimeCodeLine = new Regex(@"^(\d+):(\d\d):(\d\d)[:;,.](\d\d)\s+(.*)$", RegexOptions.Compiled);

        public readonly struct TimedPair
        {
            public TimedPair(long slot, byte data1, byte data2)
            {
                Slot = slot;
                Data1 = data1;
                Data2 = data2;
            }

            public long Slot { get; }
            public byte Data1 { get; }
            public byte Data2 { get; }
        }

        /// <summary>
        /// The byte pairs of a subtitle file: a .scc file as it is, any other format converted
        /// to CEA-608 pop-on captions by the Scenarist writer.
        /// </summary>
        public static List<TimedPair> FromFile(string fileName)
        {
            var lines = new List<string>(File.ReadAllLines(fileName));
            if (lines.Count > 0 && lines[0].TrimStart('﻿').StartsWith("Scenarist_SCC", StringComparison.OrdinalIgnoreCase))
            {
                return FromSccLines(lines);
            }

            var subtitle = Subtitle.Parse(fileName);
            if (subtitle == null)
            {
                throw new InvalidDataException($"Unknown subtitle format: {fileName}");
            }

            return FromSubtitle(subtitle);
        }

        /// <summary>
        /// The Scenarist writer makes frame labels at the current frame rate - SCC is 29.97.
        /// </summary>
        public static List<TimedPair> FromSubtitle(Subtitle subtitle)
        {
            var savedFrameRate = Configuration.Settings.General.CurrentFrameRate;
            Configuration.Settings.General.CurrentFrameRate = 30000.0 / 1001;
            try
            {
                var text = new ScenaristClosedCaptions().ToText(subtitle, string.Empty);
                return FromSccLines(text.SplitToLines());
            }
            finally
            {
                Configuration.Settings.General.CurrentFrameRate = savedFrameRate;
            }
        }

        /// <summary>
        /// A line's timecode is the slot of its first byte pair, and the next pairs follow one
        /// slot each. Lines are sent in time order (the Scenarist writer puts a caption's load
        /// line after the previous caption's end), and a line that starts before the previous one
        /// is sent waits for it, like a caption encoder does. Timecodes are read like the Scenarist
        /// format reads them: the time of day plus frames at 29.97 - so the captions come where
        /// Subtitle Edit shows them.
        /// </summary>
        public static List<TimedPair> FromSccLines(IEnumerable<string> lines)
        {
            var timedLines = new List<(long Slot, string Words)>();
            foreach (var line in lines)
            {
                var match = TimeCodeLine.Match(line.Trim());
                if (!match.Success)
                {
                    continue;
                }

                var milliseconds = ToMilliseconds(match, SlotMilliseconds);
                timedLines.Add(((long)Math.Round(milliseconds / SlotMilliseconds), match.Groups[5].Value));
            }

            var ordered = timedLines.Select((line, index) => (line.Slot, line.Words, index)).OrderBy(line => line.Slot).ThenBy(line => line.index);
            var result = new List<TimedPair>();
            long nextFreeSlot = 0;
            foreach (var line in ordered)
            {
                var slot = Math.Max(nextFreeSlot, line.Slot);
                foreach (var word in line.Words.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (word.Length != 4 || !int.TryParse(word, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                    {
                        continue;
                    }

                    result.Add(new TimedPair(slot, FixZero((byte)(value >> 8)), FixZero((byte)value)));
                    slot++;
                }

                nextFreeSlot = slot;
            }

            return result;
        }

        /// <summary>
        /// The byte pairs moved by <paramref name="milliseconds"/> (negative = earlier), in whole
        /// slots so the pairs of a line stay on consecutive slots. Pairs that would come before the
        /// start of the video are dropped - what a broadcast file with its first caption at
        /// 01:00:00:00 sends before the program starts is nothing but the encoder's null pairs.
        /// </summary>
        public static List<TimedPair> Shift(IEnumerable<TimedPair> pairs, double milliseconds)
        {
            var slots = (long)Math.Round(milliseconds / SlotMilliseconds);
            var result = new List<TimedPair>();
            foreach (var pair in pairs)
            {
                var slot = pair.Slot + slots;
                if (slot >= 0)
                {
                    result.Add(new TimedPair(slot, pair.Data1, pair.Data2));
                }
            }

            return result;
        }

        /// <summary>
        /// The timecode of the first caption line in a Scenarist file, read like
        /// <see cref="FromSccLines"/> reads it (the time of day plus frames at 29.97), or null when
        /// the file has no timecode line.
        /// </summary>
        public static TimeSpan? GetFirstTimeCode(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                var match = TimeCodeLine.Match(line.Trim());
                if (match.Success)
                {
                    return TimeSpan.FromMilliseconds(ToMilliseconds(match, SlotMilliseconds));
                }
            }

            return null;
        }

        /// <summary>
        /// hh:mm:ss:ff of a timecode <paramref name="match"/> (groups 1-4) in milliseconds, with the
        /// frames at <paramref name="frameMilliseconds"/>.
        /// </summary>
        internal static double ToMilliseconds(Match match, double frameMilliseconds)
        {
            return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * 3_600_000.0 +
                   int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) * 60_000.0 +
                   int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture) * 1_000.0 +
                   int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture) * frameMilliseconds;
        }

        /// <summary>
        /// 0x00 has even parity, so it is no valid CEA-608 byte - and in the video it could make a
        /// start code (00 00 01). It is sent as 0x80, the null byte with odd parity.
        /// </summary>
        private static byte FixZero(byte b) => b == 0 ? (byte)0x80 : b;
    }
}
