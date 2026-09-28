using Nikse.SubtitleEdit.Core.Cea608;
using DtvccServiceDecoder = Nikse.SubtitleEdit.Core.Cea708.DtvccServiceDecoder;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// Extracts CEA-608 and CEA-708 closed captions embedded in the video elementary stream of a
    /// transport stream (ATSC/cable broadcasts): cc_data in H.264/H.265 SEI messages (ATSC A/72,
    /// SCTE 128) and in MPEG-2 video user data (ATSC A/53). One instance per video PID.
    /// </summary>
    public class ClosedCaptionExtractor
    {
        public enum VideoCodec
        {
            Unknown,
            Mpeg2,
            H264,
            H265,
        }

        /// <summary>Track keys 1-4 are CEA-608 CC1-CC4, 100 + n is CEA-708 service n.</summary>
        public const int Cea708TrackKeyOffset = 100;

        // Video frames are coded out of display order (B-frames); keep this many PES packets
        // sorted by PTS before handing their cc_data on, so the decoders see display order.
        private const int ReorderDepth = 32;
        private const long PtsWrap = 1L << 33;

        private readonly VideoCodec _codec;
        private readonly byte[] _pesBuffer;
        private int _pesLength;
        private bool _pesStarted;
        private readonly List<CcData> _scratch = new List<CcData>();
        private readonly List<KeyValuePair<long, CcData[]>> _reorder = new List<KeyValuePair<long, CcData[]>>();
        private long _lastPts = -1;
        private long _ptsWrapOffset;
        private long _lastPtsMs = -1;
        private long _frameDurationMs = long.MaxValue;
        private readonly CcDataC608Parser[] _cea608Parsers = new CcDataC608Parser[2];
        private readonly bool[] _inXds = new bool[2];
        private readonly DtvccServiceDecoder _cea708Decoder = new DtvccServiceDecoder();
        private readonly SortedDictionary<int, List<Paragraph>> _cea608Paragraphs = new SortedDictionary<int, List<Paragraph>>();

        public ClosedCaptionExtractor(VideoCodec codec)
        {
            _codec = codec;
            _pesBuffer = new byte[4 * 1024 * 1024]; // larger access units are cut off - cc_data sits before the slice data anyway
            for (var field = 0; field < 2; field++)
            {
                var fieldIndex = field;
                _cea608Parsers[field] = new CcDataC608Parser
                {
                    DisplayScreen = data => AddCea608Paragraph(fieldIndex, data),
                };
            }
        }

        /// <summary>
        /// Display name of a track key, e.g. "CEA-608 CC1" or "CEA-708 service 1".
        /// </summary>
        public static string GetTrackName(int trackKey)
        {
            return trackKey > Cea708TrackKeyOffset
                ? "CEA-708 service " + (trackKey - Cea708TrackKeyOffset)
                : "CEA-608 CC" + trackKey;
        }

        public static bool IsVideoStreamType(int streamType, out VideoCodec codec)
        {
            switch (streamType)
            {
                case 0x01: // MPEG-1 video
                case 0x02: // MPEG-2 video
                case 0x80: // DigiCipher II video (some ATSC/cable muxers)
                    codec = VideoCodec.Mpeg2;
                    return true;
                case 0x1B:
                    codec = VideoCodec.H264;
                    return true;
                case 0x24:
                    codec = VideoCodec.H265;
                    return true;
                default:
                    codec = VideoCodec.Unknown;
                    return false;
            }
        }

        /// <summary>
        /// True if the packet starts a PES packet with a video stream id (0xE0-0xEF).
        /// </summary>
        public static bool IsVideoPesStart(byte[] packetBuffer)
        {
            var payloadStart = GetPayloadStart(packetBuffer);
            return payloadStart >= 0 && (packetBuffer[1] & 0x40) != 0 && payloadStart + 4 <= packetBuffer.Length &&
                   packetBuffer[payloadStart] == 0 && packetBuffer[payloadStart + 1] == 0 && packetBuffer[payloadStart + 2] == 1 &&
                   (packetBuffer[payloadStart + 3] & 0xF0) == 0xE0;
        }

        /// <summary>
        /// Adds one 188-byte transport stream packet belonging to this video PID.
        /// </summary>
        public void AddPacket(byte[] packetBuffer)
        {
            if ((packetBuffer[1] & 0x80) != 0)
            {
                return; // transport_error_indicator
            }

            var payloadStart = GetPayloadStart(packetBuffer);
            if (payloadStart < 0)
            {
                return;
            }

            if ((packetBuffer[1] & 0x40) != 0) // payload_unit_start_indicator
            {
                ProcessPes();
                _pesStarted = true;
                _pesLength = 0;
            }

            if (!_pesStarted)
            {
                return;
            }

            var count = Math.Min(packetBuffer.Length - payloadStart, _pesBuffer.Length - _pesLength);
            if (count > 0)
            {
                Buffer.BlockCopy(packetBuffer, payloadStart, _pesBuffer, _pesLength, count);
                _pesLength += count;
            }
        }

        /// <summary>
        /// Decodes what is left and returns the caption tracks found.
        /// </summary>
        /// <param name="offsetMs">Subtracted from all times (first video timestamp)</param>
        /// <returns>Paragraphs per track key (1-4 = CC1-CC4, 100 + n = CEA-708 service n)</returns>
        public SortedDictionary<int, List<Paragraph>> Finish(long offsetMs)
        {
            ProcessPes();
            _pesStarted = false;
            while (_reorder.Count > 0)
            {
                DecodeOldest();
            }

            // Captions still on screen at the end of the stream are only emitted when the screen
            // changes - erase displayed memory on both channels of both fields to flush them,
            // when the last frame ends.
            var frameDurationMs = _frameDurationMs == long.MaxValue ? 33 : _frameDurationMs;
            var flushTime = (int)Math.Min(int.MaxValue, _lastPtsMs + frameDurationMs);
            foreach (var parser in _cea608Parsers)
            {
                parser.AddData(flushTime, new[] { 0x14, 0x2C });
                parser.AddData(flushTime, new[] { 0x1C, 0x2C });
            }

            var result = new SortedDictionary<int, List<Paragraph>>();
            foreach (var track in _cea608Paragraphs)
            {
                result.Add(track.Key, track.Value);
            }

            foreach (var service in _cea708Decoder.Finish())
            {
                result.Add(Cea708TrackKeyOffset + service.Key, service.Value);
            }

            foreach (var paragraphs in result.Values)
            {
                foreach (var p in paragraphs)
                {
                    if (offsetMs <= p.StartTime.TotalMilliseconds)
                    {
                        p.StartTime.TotalMilliseconds -= offsetMs;
                        p.EndTime.TotalMilliseconds -= offsetMs;
                    }
                }
            }

            return result;
        }

        private static int GetPayloadStart(byte[] packetBuffer)
        {
            var adaptationFieldControl = (packetBuffer[3] >> 4) & 0x03;
            if (adaptationFieldControl == 1)
            {
                return 4;
            }

            if (adaptationFieldControl == 3)
            {
                var start = 5 + packetBuffer[4];
                return start < packetBuffer.Length ? start : -1;
            }

            return -1; // no payload
        }

        private void ProcessPes()
        {
            if (!_pesStarted || _pesLength < 9)
            {
                return;
            }

            var pes = _pesBuffer.AsSpan(0, _pesLength);
            if (pes[0] != 0 || pes[1] != 0 || pes[2] != 1)
            {
                return;
            }

            var dataStart = 9 + pes[8];
            if (dataStart >= pes.Length)
            {
                return;
            }

            long pts;
            if ((pes[7] & 0x80) != 0 && pes.Length >= 14)
            {
                var rawPts = ((long)(pes[9] & 0x0E) << 29) | ((long)pes[10] << 22) | ((long)(pes[11] & 0xFE) << 14) |
                             ((long)pes[12] << 7) | ((long)pes[13] >> 1);

                // The 33-bit PTS wraps every ~26.5 hours - keep a running offset so a recording
                // across the wrap stays in order.
                pts = rawPts + _ptsWrapOffset;
                if (_lastPts >= 0 && pts < _lastPts - PtsWrap / 2)
                {
                    _ptsWrapOffset += PtsWrap;
                    pts += PtsWrap;
                }
                else if (_lastPts >= 0 && pts > _lastPts + PtsWrap / 2)
                {
                    pts -= PtsWrap; // a reordered frame from just before the wrap
                }

                _lastPts = pts;
            }
            else if (_lastPts >= 0)
            {
                pts = _lastPts; // no PTS on this PES - reuse the previous one
            }
            else
            {
                return;
            }

            _scratch.Clear();
            ScanElementaryStream(pes.Slice(dataStart));
            if (_scratch.Count == 0)
            {
                return;
            }

            // insert sorted by PTS (stable for equal timestamps)
            var entry = new KeyValuePair<long, CcData[]>(pts, _scratch.ToArray());
            var index = _reorder.Count;
            while (index > 0 && _reorder[index - 1].Key > pts)
            {
                index--;
            }

            _reorder.Insert(index, entry);
            if (_reorder.Count > ReorderDepth)
            {
                DecodeOldest();
            }
        }

        private static readonly byte[] StartCode = { 0, 0, 1 };

        private void ScanElementaryStream(ReadOnlySpan<byte> data)
        {
            var position = data.IndexOf(StartCode);
            while (position >= 0)
            {
                var unitStart = position + 3;
                if (unitStart >= data.Length)
                {
                    return;
                }

                var next = data.Slice(unitStart).IndexOf(StartCode);
                var unitEnd = next < 0 ? data.Length : unitStart + next;
                var unit = data.Slice(unitStart, unitEnd - unitStart);
                ParseUnit(unit);
                position = next < 0 ? -1 : unitEnd;
            }
        }

        private void ParseUnit(ReadOnlySpan<byte> unit)
        {
            if (unit.Length < 2)
            {
                return;
            }

            var b = unit[0];
            if ((_codec == VideoCodec.Mpeg2 || _codec == VideoCodec.Unknown) && b == 0xB2)
            {
                GetCcDataHelper.ParseCcDataFromAtscUserData(unit.Slice(1), _scratch);
            }
            else if ((_codec == VideoCodec.H264 || _codec == VideoCodec.Unknown) && (b & 0x9F) == 0x06)
            {
                GetCcDataHelper.ParseCcDataFromSeiNalPayload(unit.Slice(1), _scratch); // H.264 SEI
            }
            else if ((_codec == VideoCodec.H265 || _codec == VideoCodec.Unknown) && (b & 0x81) == 0 && ((b >> 1) == 39 || (b >> 1) == 40))
            {
                GetCcDataHelper.ParseCcDataFromSeiNalPayload(unit.Slice(2), _scratch); // H.265 prefix/suffix SEI
            }
        }

        private void DecodeOldest()
        {
            var entry = _reorder[0];
            _reorder.RemoveAt(0);

            var ms = entry.Key / 90;
            if (_lastPtsMs >= 0 && ms > _lastPtsMs)
            {
                _frameDurationMs = Math.Min(_frameDurationMs, ms - _lastPtsMs);
            }

            _lastPtsMs = Math.Max(_lastPtsMs, ms);
            var time = (int)Math.Min(int.MaxValue, ms);
            foreach (var cc in entry.Value)
            {
                if (cc.Type == 0 || cc.Type == 1)
                {
                    AddCea608(cc.Type, cc.Data1 & 0x7F, cc.Data2 & 0x7F, time);
                }
                else
                {
                    _cea708Decoder.Add(cc.Type, cc.Data1, cc.Data2, ms);
                }
            }
        }

        private void AddCea608(int field, int a, int b, int time)
        {
            if (field == 1)
            {
                // Field 2 carries XDS (extended data services) between the captions: a packet
                // starts with a 0x01-0x0E control byte and ends with 0x0F + checksum, and its
                // payload bytes look just like characters - keep them out of CC3/CC4.
                if (a >= 0x01 && a <= 0x0E)
                {
                    _inXds[field] = true;
                    return;
                }

                if (a == 0x0F)
                {
                    _inXds[field] = false;
                    return;
                }

                if (a >= 0x10 && a <= 0x1F)
                {
                    _inXds[field] = false;
                }
                else if (_inXds[field])
                {
                    return;
                }

                // CEA-608 lets field 2 send its miscellaneous control codes with 0x15/0x1D
                // instead of 0x14/0x1C.
                if ((a == 0x15 || a == 0x1D) && b >= 0x20 && b <= 0x2F)
                {
                    a--;
                }
            }

            _cea608Parsers[field].AddData(time, new[] { a, b });
        }

        private void AddCea608Paragraph(int field, DataOutput data)
        {
            var text = SerializedScreenText.GetText(data.Screen);
            if (string.IsNullOrWhiteSpace(text) || data.Channel < 1 || data.Channel > 2)
            {
                return;
            }

            text = text.Trim();
            var trackKey = field * 2 + data.Channel; // CC1/CC2 on field 1, CC3/CC4 on field 2
            if (!_cea608Paragraphs.TryGetValue(trackKey, out var paragraphs))
            {
                paragraphs = new List<Paragraph>();
                _cea608Paragraphs.Add(trackKey, paragraphs);
            }

            // Roll-up and paint-on captions change the screen with every character pair - let a
            // line that is still being written grow the cue before it instead of adding a cue per
            // character. A new row (roll-up scroll) starts a new cue.
            if (paragraphs.Count > 0)
            {
                var last = paragraphs[paragraphs.Count - 1];
                if (Math.Abs(last.EndTime.TotalMilliseconds - data.Start) < 0.5 &&
                    last.NumberOfLines == Utilities.GetNumberOfLines(text) &&
                    HtmlUtil.RemoveHtmlTags(text, true).StartsWith(HtmlUtil.RemoveHtmlTags(last.Text, true), StringComparison.Ordinal))
                {
                    last.Text = text;
                    last.EndTime.TotalMilliseconds = data.End;
                    return;
                }
            }

            paragraphs.Add(new Paragraph(text, data.Start, data.End));
        }
    }
}
