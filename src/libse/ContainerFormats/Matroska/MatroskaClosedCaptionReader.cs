using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Matroska
{
    /// <summary>
    /// Reads CEA-608/708 closed captions embedded in the video track of a Matroska file: cc_data in
    /// H.264/H.265 SEI messages and in MPEG-2 video user data (e.g. a broadcast recording remuxed
    /// from .ts to .mkv, where the captions travel inside the video).
    /// </summary>
    public static class MatroskaClosedCaptionReader
    {
        /// <summary>
        /// Stop reading when this much video (from its first frame) had no cc_data - a file without
        /// captions is then not read to the end.
        /// </summary>
        public const long DefaultProbeMilliseconds = 60_000;

        /// <summary>
        /// The first video track that can carry cc_data, or null.
        /// </summary>
        public static MatroskaTrackInfo GetVideoTrack(MatroskaFile matroska)
        {
            return matroska.GetTracks().FirstOrDefault(p => p.IsVideo && GetCodec(p) != null);
        }

        /// <summary>
        /// Reads the closed captions of the first video track.
        /// </summary>
        /// <param name="matroska">Matroska file</param>
        /// <param name="probeMilliseconds">Give up when no cc_data was found in this much video</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Paragraphs per track key (1-4 = CC1-CC4, 100 + n = CEA-708 service n), empty if none</returns>
        public static SortedDictionary<int, List<Paragraph>> Read(MatroskaFile matroska, long probeMilliseconds, MatroskaFile.LoadMatroskaCallback progressCallback)
        {
            var track = GetVideoTrack(matroska);
            if (track == null)
            {
                return new SortedDictionary<int, List<Paragraph>>();
            }

            var codec = GetCodec(track).Value;
            var nalLengthSize = GetNalLengthSize(track, codec);
            var decoder = new ClosedCaptionDecoder();
            var ccData = new List<CcData>();
            var parseState = new CcDataParseState();
            long? firstFrameMs = null;
            try
            {
                matroska.ReadTrackFrames(track, (timeMs, frame) =>
                {
                    if (firstFrameMs == null)
                    {
                        firstFrameMs = timeMs;
                    }

                    ccData.Clear();
                    if (codec == CcVideoCodec.Mpeg2)
                    {
                        GetCcDataHelper.ParseCcDataFromStartCodeStream(frame, codec, ccData, parseState);
                    }
                    else
                    {
                        GetCcDataHelper.ParseCcDataFromLengthPrefixedSample(frame, codec == CcVideoCodec.H265, nalLengthSize, ccData, parseState);
                    }

                    decoder.AddFrame(timeMs, ccData.ToArray());

                    // keep reading once caption data was seen - also if it was only padding so far
                    return parseState.CaptionDataSeen || decoder.HasData || timeMs - firstFrameMs.Value < probeMilliseconds;
                }, progressCallback);
            }
            catch (Exception exception)
            {
                // corrupt video data - keep what was decoded so far
                SeLogger.Error(exception, "MatroskaClosedCaptionReader: reading video frames failed");
            }

            return decoder.HasData
                ? decoder.Finish(0) // block timestamps are on the same timeline as everything else in the file
                : new SortedDictionary<int, List<Paragraph>>();
        }

        private static CcVideoCodec? GetCodec(MatroskaTrackInfo track)
        {
            switch (track.CodecId)
            {
                case "V_MPEG4/ISO/AVC":
                    return CcVideoCodec.H264;
                case "V_MPEGH/ISO/HEVC":
                    return CcVideoCodec.H265;
                case "V_MPEG2":
                case "V_MPEG1":
                    return CcVideoCodec.Mpeg2;
                default:
                    return null;
            }
        }

        /// <summary>
        /// lengthSizeMinusOne + 1 from the avcC/hvcC decoder configuration in CodecPrivate.
        /// </summary>
        private static int GetNalLengthSize(MatroskaTrackInfo track, CcVideoCodec codec)
        {
            var codecPrivate = track.CodecPrivateRaw;
            var offset = codec == CcVideoCodec.H265 ? 21 : 4;
            if (codecPrivate == null || codecPrivate.Length <= offset)
            {
                return 4;
            }

            var size = (codecPrivate[offset] & 0x03) + 1;
            return size == 3 ? 4 : size;
        }
    }
}
