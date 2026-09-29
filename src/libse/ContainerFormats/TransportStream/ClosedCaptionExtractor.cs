using Nikse.SubtitleEdit.Core.Cea608;
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
        /// <summary>Track keys 1-4 are CEA-608 CC1-CC4, 100 + n is CEA-708 service n.</summary>
        public const int Cea708TrackKeyOffset = ClosedCaptionDecoder.Cea708TrackKeyOffset;

        private const long PtsWrap = 1L << 33;

        private readonly CcVideoCodec _codec;
        private readonly byte[] _pesBuffer;
        private int _pesLength;
        private bool _pesStarted;
        private readonly List<CcData> _scratch = new List<CcData>();
        private readonly CcDataParseState _parseState = new CcDataParseState();
        private long _lastPts = -1;
        private long _ptsWrapOffset;
        private readonly ClosedCaptionDecoder _decoder = new ClosedCaptionDecoder();

        public ClosedCaptionExtractor(CcVideoCodec codec)
        {
            _codec = codec;
            _pesBuffer = new byte[4 * 1024 * 1024]; // larger access units are cut off - cc_data sits before the slice data anyway
        }

        /// <summary>
        /// Display name of a track key, e.g. "CEA-608 CC1" or "CEA-708 service 1".
        /// </summary>
        public static string GetTrackName(int trackKey) => ClosedCaptionDecoder.GetTrackName(trackKey);

        public static bool IsVideoStreamType(int streamType, out CcVideoCodec codec)
        {
            switch (streamType)
            {
                case 0x01: // MPEG-1 video
                case 0x02: // MPEG-2 video
                case 0x80: // DigiCipher II video (some ATSC/cable muxers)
                    codec = CcVideoCodec.Mpeg2;
                    return true;
                case 0x1B:
                    codec = CcVideoCodec.H264;
                    return true;
                case 0x24:
                    codec = CcVideoCodec.H265;
                    return true;
                default:
                    codec = CcVideoCodec.Unknown;
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
            return _decoder.Finish(offsetMs);
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
            GetCcDataHelper.ParseCcDataFromStartCodeStream(pes.Slice(dataStart), _codec, _scratch, _parseState);
            _decoder.AddFrame(pts / 90, _scratch.ToArray());
        }
    }
}
