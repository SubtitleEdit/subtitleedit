using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using DtvccServiceDecoder = Nikse.SubtitleEdit.Core.Cea708.DtvccServiceDecoder;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    /// <summary>
    /// Decodes the cc_data of a video stream (CEA-608 fields 1 and 2, CEA-708 DTVCC) into caption
    /// tracks. Frames may be added in decode order - they are put in presentation order first.
    /// </summary>
    public class ClosedCaptionDecoder
    {
        /// <summary>Track keys 1-4 are CEA-608 CC1-CC4, 100 + n is CEA-708 service n.</summary>
        public const int Cea708TrackKeyOffset = 100;

        // Video frames are coded out of display order (B-frames); keep this many frames sorted by
        // time before handing their cc_data on, so the decoders see display order.
        private const int ReorderDepth = 32;

        private readonly List<KeyValuePair<long, CcData[]>> _reorder = new List<KeyValuePair<long, CcData[]>>();
        private long _lastMs = -1;
        private long _frameDurationMs = long.MaxValue;
        private readonly CcDataC608Parser[] _cea608Parsers = new CcDataC608Parser[2];
        private readonly bool[] _inXds = new bool[2];
        private readonly DtvccServiceDecoder _cea708Decoder = new DtvccServiceDecoder();
        private readonly SortedDictionary<int, List<Paragraph>> _cea608Paragraphs = new SortedDictionary<int, List<Paragraph>>();

        public ClosedCaptionDecoder()
        {
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
        /// True once any frame with cc_data was added.
        /// </summary>
        public bool HasData { get; private set; }

        /// <summary>
        /// Display name of a track key, e.g. "CEA-608 CC1" or "CEA-708 service 1".
        /// </summary>
        public static string GetTrackName(int trackKey)
        {
            return trackKey > Cea708TrackKeyOffset
                ? "CEA-708 service " + (trackKey - Cea708TrackKeyOffset)
                : "CEA-608 CC" + trackKey;
        }

        /// <summary>
        /// Adds the cc_data of one video frame.
        /// </summary>
        /// <param name="timeMs">Presentation time of the frame</param>
        /// <param name="ccData">cc_data triplets of the frame</param>
        public void AddFrame(long timeMs, CcData[] ccData)
        {
            if (ccData.Length == 0)
            {
                return;
            }

            HasData = true;

            // insert sorted by time (stable for equal timestamps)
            var index = _reorder.Count;
            while (index > 0 && _reorder[index - 1].Key > timeMs)
            {
                index--;
            }

            _reorder.Insert(index, new KeyValuePair<long, CcData[]>(timeMs, ccData));
            if (_reorder.Count > ReorderDepth)
            {
                DecodeOldest();
            }
        }

        /// <summary>
        /// Decodes what is left and returns the caption tracks found.
        /// </summary>
        /// <param name="offsetMs">Subtracted from all times (e.g. first video timestamp)</param>
        /// <returns>Paragraphs per track key (1-4 = CC1-CC4, 100 + n = CEA-708 service n)</returns>
        public SortedDictionary<int, List<Paragraph>> Finish(long offsetMs)
        {
            while (_reorder.Count > 0)
            {
                DecodeOldest();
            }

            // Captions still on screen at the end of the stream are only emitted when the screen
            // changes - erase displayed memory on both channels of both fields to flush them,
            // when the last frame ends.
            var frameDurationMs = _frameDurationMs == long.MaxValue ? 33 : _frameDurationMs;
            var flushTime = (int)Math.Min(int.MaxValue, _lastMs + frameDurationMs);
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

            foreach (var service in _cea708Decoder.Finish(flushTime))
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

        private void DecodeOldest()
        {
            var entry = _reorder[0];
            _reorder.RemoveAt(0);

            var ms = entry.Key;
            if (_lastMs >= 0 && ms > _lastMs)
            {
                _frameDurationMs = Math.Min(_frameDurationMs, ms - _lastMs);
            }

            _lastMs = Math.Max(_lastMs, ms);
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

            var trackKey = field * 2 + data.Channel; // CC1/CC2 on field 1, CC3/CC4 on field 2
            if (!_cea608Paragraphs.TryGetValue(trackKey, out var paragraphs))
            {
                paragraphs = new List<Paragraph>();
                _cea608Paragraphs.Add(trackKey, paragraphs);
            }

            Cea608CueBuilder.Add(paragraphs, text, data.Start, data.End);
        }
    }
}
