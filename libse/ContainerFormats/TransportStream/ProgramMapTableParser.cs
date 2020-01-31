﻿using System;
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
                var isM2TransportStream = TransportStreamParser.IsM3TransportStream(ms);
                var packetBuffer = new byte[packetLength];
                var m2TsTimeCodeBuffer = new byte[4];
                long position = 0;

                // check for Topfield .rec file
                ms.Seek(position, SeekOrigin.Begin);
                ms.Read(m2TsTimeCodeBuffer, 0, 3);
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
                        ms.Read(m2TsTimeCodeBuffer, 0, m2TsTimeCodeBuffer.Length);
                        position += m2TsTimeCodeBuffer.Length;
                    }

                    ms.Read(packetBuffer, 0, packetLength);
                    if (packetBuffer[0] == Packet.SynchronizationByte)
                    {
                        var packet = new Packet(packetBuffer);

                        if (pmtPids.Contains(packet.PacketId))
                        {
                            var pmt = new ProgramMapTable(packet.Payload, 0);
                            _programMapTables.Add(pmt);
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
            var lookupList = new HashSet<int>();
            foreach (var stream in _programMapTables.SelectMany(p => p.Streams))
            {
                if (stream.StreamType == ProgramMapTableStream.StreamTypePrivateData && !lookupList.Contains(stream.ElementaryPid))
                {
                    lookupList.Add(stream.ElementaryPid);
                }
            }
            return lookupList.ToList();
        }

        public string GetSubtitleLanguage(int packetId)
        {
            return _programMapTables.SelectMany(p => p.Streams)
                .FirstOrDefault(stream => stream.ElementaryPid == packetId)?
                .GetLanguage();
        }

        public string GetSubtitleLanguageTwoLetter(int packetId)
        {
            var language = GetSubtitleLanguage(packetId);
            var uppercaseLanguage = language.ToUpperInvariant();
            if (IsoCountryCodes.ThreeToTweLetterLookup.ContainsKey(uppercaseLanguage))
            {
                return IsoCountryCodes.ThreeToTweLetterLookup[uppercaseLanguage].ToLowerInvariant();
            }
            return language;
        }
    }
}