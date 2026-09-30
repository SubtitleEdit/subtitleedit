using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class ProgramMapTableParser
    {
        private List<ProgramMapTable> _programMapTables;
        public Exception Exception { get; set; }
        public ProgramMapTableParser()
        {
            _programMapTables = new List<ProgramMapTable>();
        }

        public void Parse(string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    Parse(fs);
                }
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        private const int MaxScanSize = 5000000;

        /// <summary>
        /// Get Program Map Tables for a Transport Stream, especially language for subtitle tracks
        /// </summary>
        /// <param name="ms">Input stream</param>
        public void Parse(Stream ms)
        {
            try
            {

                ms.Position = 0;
                const int packetLength = 188;
                var isM2TransportStream = TransportStreamParser.IsM2TransportStream(ms);
                var packetBuffer = new byte[packetLength];
                var m2TsTimeCodeBuffer = new byte[4];
                long position = 0;

                // check for Topfield .rec file
                ms.Seek(position, SeekOrigin.Begin);
                ms.ReadFully(m2TsTimeCodeBuffer, 0, 3);
                if (m2TsTimeCodeBuffer[0] == 0x54 && m2TsTimeCodeBuffer[1] == 0x46 && m2TsTimeCodeBuffer[2] == 0x72)
                {
                    position = 3760;
                }

                var pmtPids = new List<int>();
                _programMapTables = new List<ProgramMapTable>();
                long transportStreamLength = ms.Length;
                var max = Math.Min(transportStreamLength, MaxScanSize + position);
                while (position < max)
                {
                    ms.Seek(position, SeekOrigin.Begin);

                    if (isM2TransportStream)
                    {
                        ms.ReadFully(m2TsTimeCodeBuffer, 0, m2TsTimeCodeBuffer.Length);
                        position += m2TsTimeCodeBuffer.Length;
                    }

                    ms.ReadFully(packetBuffer, 0, packetLength);
                    if (packetBuffer[0] == Packet.SynchronizationByte)
                    {
                        var packet = new Packet(packetBuffer);

                        if (pmtPids.Contains(packet.PacketId))
                        {
                            if (packet.PayloadUnitStartIndicator)
                            {
                                try
                                {
                                    var pmt = new ProgramMapTable(packet.Payload, 0);
                                    _programMapTables.Add(pmt);
                                }
                                catch
                                {
                                    // sections longer than one TS packet are not assembled - parsing
                                    // the truncated payload can run out of bounds, skip those
                                }
                            }
                        }
                        else if (packet.IsProgramAssociationTable)
                        {
                            var pat = new ProgramAssociationTable(packet.Payload, 0);
                            pmtPids.AddRange(pat.ProgramIds.Where(p => !pmtPids.Contains(p)));
                        }

                        position += packetLength;
                    }
                    else
                    {
                        position++;
                    }
                }
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        public List<int> GetSubtitlePacketIds()
        {
            var list = new List<int>();
            foreach (var programMapTable in _programMapTables)
            {
                foreach (var stream in programMapTable.Streams)
                {
                    if (stream.StreamType == ProgramMapTableStream.StreamTypePrivateData && !list.Contains(stream.ElementaryPid))
                    {
                        list.Add(stream.ElementaryPid);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Get stream_type per elementary PID
        /// </summary>
        public Dictionary<int, int> GetStreamTypes()
        {
            var result = new Dictionary<int, int>();
            foreach (var programMapTable in _programMapTables)
            {
                foreach (var stream in programMapTable.Streams)
                {
                    if (!result.ContainsKey(stream.ElementaryPid))
                    {
                        result.Add(stream.ElementaryPid, stream.StreamType);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get data_component_id per elementary PID from ARIB data_component_descriptors (tag 0xFD) -
        /// identifies ISDB caption streams: 0x0008 = ARIB profile A captions, 0x0012 = profile C (one-seg)
        /// </summary>
        public Dictionary<int, int> GetAribDataComponentIds()
        {
            var result = new Dictionary<int, int>();
            foreach (var programMapTable in _programMapTables)
            {
                foreach (var stream in programMapTable.Streams)
                {
                    foreach (var descriptor in stream.Descriptors)
                    {
                        if (descriptor.Tag == 0xfd && descriptor.Content?.Length >= 2 && !result.ContainsKey(stream.ElementaryPid))
                        {
                            result.Add(stream.ElementaryPid, (descriptor.Content[0] << 8) | descriptor.Content[1]);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Video PID of the program that carries the elementary stream <paramref name="packetId"/> -
        /// in a multi-program stream (e.g. a whole DVB-T mux) every program has its own clock, so a
        /// subtitle's times must be taken relative to its own program's video.
        /// </summary>
        /// <returns>The video PID, or the program's PCR PID if no video stream is listed, or null
        /// if no program map table lists the stream</returns>
        public int? GetProgramVideoPacketId(int packetId)
        {
            var programMapTable = GetProgramMapTable(packetId);
            if (programMapTable == null)
            {
                return null;
            }

            foreach (var stream in programMapTable.Streams)
            {
                if (IsVideoStreamType(stream.StreamType))
                {
                    return stream.ElementaryPid;
                }
            }

            return programMapTable.PcrId;
        }

        /// <summary>
        /// PCR PID of the program that carries the elementary stream <paramref name="packetId"/>.
        /// </summary>
        public int? GetProgramClockReferencePacketId(int packetId)
        {
            return GetProgramMapTable(packetId)?.PcrId;
        }

        private ProgramMapTable GetProgramMapTable(int packetId)
        {
            foreach (var programMapTable in _programMapTables)
            {
                foreach (var stream in programMapTable.Streams)
                {
                    if (stream.ElementaryPid == packetId)
                    {
                        return programMapTable;
                    }
                }
            }

            return null;
        }

        public static bool IsVideoStreamType(int streamType)
        {
            switch (streamType)
            {
                case 0x01: // MPEG-1 video
                case 0x02: // MPEG-2 video
                case 0x10: // MPEG-4 part 2 video
                case 0x1B: // H.264
                case 0x20: // H.264 MVC sub-bitstream
                case 0x24: // H.265
                case 0x33: // H.266
                case 0x42: // AVS
                case 0xD1: // Dirac
                case 0xEA: // VC-1
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Audio stream types of MPEG-TS/ATSC and Blu-ray. 0x06 (private data, DVB AC-3 and the
        /// like) needs its descriptors or payload to tell, and 0x82 is DTS on Blu-ray but SCTE 27
        /// subtitles in ATSC, so a caller must rule out subtitle PIDs.
        /// </summary>
        public static bool IsAudioStreamType(int streamType)
        {
            switch (streamType)
            {
                case 0x03: // MPEG-1 audio
                case 0x04: // MPEG-2 audio
                case 0x0F: // AAC (ADTS)
                case 0x11: // AAC (LATM)
                case 0x1C: // MPEG-4 audio
                case 0x80: // Blu-ray LPCM
                case 0x81: // AC-3
                case 0x82: // DTS (Blu-ray)
                case 0x83: // Dolby TrueHD
                case 0x84: // E-AC-3 (Blu-ray)
                case 0x85: // DTS-HD
                case 0x86: // DTS-HD MA
                case 0x87: // E-AC-3 (ATSC)
                case 0xA1: // E-AC-3 secondary audio
                case 0xA2: // DTS-HD secondary audio
                    return true;
                default:
                    return false;
            }
        }

        public string GetSubtitleLanguage(int packetId)
        {
            foreach (var programMapTable in _programMapTables)
            {
                foreach (var stream in programMapTable.Streams)
                {
                    if (stream.ElementaryPid == packetId)
                    {
                        return stream.GetLanguage();
                    }
                }
            }
            return string.Empty;
        }

        public string GetSubtitleLanguageTwoLetter(int packetId)
        {
            var language = GetSubtitleLanguage(packetId);
            var uppercaseLanguage = language.ToUpperInvariant();
            if (IsoCountryCodes.ThreeToTwoLetterLookup.ContainsKey(uppercaseLanguage))
            {
                return IsoCountryCodes.ThreeToTwoLetterLookup[uppercaseLanguage].ToLowerInvariant();
            }
            return language;
        }
    }
}