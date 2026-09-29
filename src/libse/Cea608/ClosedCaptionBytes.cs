using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    /// <summary>
    /// The caption bytes to embed in a video: CEA-608 byte pairs for field 1 (CC1/CC2) and field 2
    /// (CC3/CC4) on 29.97 Hz caption slots, and CEA-708 DTVCC cc_data in the order it is sent.
    /// </summary>
    public sealed class ClosedCaptionBytes
    {
        private static readonly Regex MccTimeCodeLine = new Regex(@"^(\d\d):(\d\d):(\d\d)[:;,.](\d\d)(?:\.\d)?\s+(\S+)", RegexOptions.Compiled);

        public readonly struct DtvccTriplet
        {
            public DtvccTriplet(double milliseconds, int type, byte data1, byte data2)
            {
                Milliseconds = milliseconds;
                Type = type;
                Data1 = data1;
                Data2 = data2;
            }

            /// <summary>
            /// When the triplet is sent at the earliest.
            /// </summary>
            public double Milliseconds { get; }

            /// <summary>
            /// cc_type: 3 = DTVCC packet start, 2 = packet data.
            /// </summary>
            public int Type { get; }

            public byte Data1 { get; }
            public byte Data2 { get; }
        }

        public List<SccBytePairs.TimedPair> Field1 { get; set; } = new List<SccBytePairs.TimedPair>();
        public List<SccBytePairs.TimedPair> Field2 { get; set; } = new List<SccBytePairs.TimedPair>();
        public List<DtvccTriplet> Dtvcc { get; set; } = new List<DtvccTriplet>();

        public bool HasCea708 => Dtvcc.Count > 0;

        /// <summary>
        /// A MacCaption (.mcc) file with all its caption data (CEA-608 fields 1 and 2, CEA-708),
        /// or any other subtitle file as CEA-608 field 1 (see <see cref="SccBytePairs.FromFile"/>).
        /// </summary>
        public static ClosedCaptionBytes FromFile(string fileName)
        {
            var lines = new List<string>(File.ReadAllLines(fileName));
            if (lines.Count > 0 && lines[0].TrimStart('﻿').StartsWith("File Format=MacCaption_MCC", StringComparison.OrdinalIgnoreCase))
            {
                return FromMccLines(lines);
            }

            return new ClosedCaptionBytes { Field1 = SccBytePairs.FromFile(fileName) };
        }

        /// <summary>
        /// The cc_data of an MCC file: each line is one caption distribution packet at its
        /// timecode. CEA-608 null pairs are left out (the writer fills empty slots with them), and a
        /// pair that would come before the previous one of its field waits for it. Timecodes are
        /// read like the Scenarist ones: the time of day plus frames at the time code rate.
        /// </summary>
        public static ClosedCaptionBytes FromMccLines(IEnumerable<string> lines)
        {
            var result = new ClosedCaptionBytes();
            var frameMilliseconds = 1001.0 / 30;
            var nextFreeSlot = new long[2];
            foreach (var line in lines)
            {
                var s = line.Trim();
                if (s.StartsWith("Time Code Rate=", StringComparison.OrdinalIgnoreCase))
                {
                    frameMilliseconds = GetFrameMilliseconds(s.Substring("Time Code Rate=".Length).Trim());
                    continue;
                }

                var match = MccTimeCodeLine.Match(s);
                if (!match.Success)
                {
                    continue;
                }

                var milliseconds = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * 3_600_000.0 +
                                   int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) * 60_000.0 +
                                   int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture) * 1_000.0 +
                                   int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture) * frameMilliseconds;
                foreach (var cc in MacCaption10.GetAllCcData(match.Groups[5].Value))
                {
                    if (!cc.Valid)
                    {
                        continue;
                    }

                    if (cc.Type >= 2)
                    {
                        result.Dtvcc.Add(new DtvccTriplet(milliseconds, cc.Type, cc.Data1, cc.Data2));
                    }
                    else if ((cc.Data1 & 0x7F) != 0 || (cc.Data2 & 0x7F) != 0)
                    {
                        var field = cc.Type;
                        var slot = Math.Max(nextFreeSlot[field], (long)Math.Round(milliseconds / SccBytePairs.SlotMilliseconds));
                        nextFreeSlot[field] = slot + 1;
                        var pair = new SccBytePairs.TimedPair(slot, FixZero(cc.Data1), FixZero(cc.Data2));
                        (field == 0 ? result.Field1 : result.Field2).Add(pair);
                    }
                }
            }

            return result;
        }

        private static double GetFrameMilliseconds(string timeCodeRate)
        {
            switch (timeCodeRate.ToUpperInvariant())
            {
                case "24": return 1001.0 / 24;
                case "25": return 1000.0 / 25;
                case "50": return 1000.0 / 50;
                case "60":
                case "60DF": return 1001.0 / 60;
                default: return 1001.0 / 30; // 30, 30DF
            }
        }

        private static byte FixZero(byte b) => b == 0 ? (byte)0x80 : b;
    }
}
