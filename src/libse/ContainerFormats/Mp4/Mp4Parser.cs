using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    /// <summary>
    /// http://wiki.multimedia.cx/index.php?title=QuickTime_container
    /// https://gpac.github.io/mp4box.js/test/filereader.html
    /// </summary>
    public class MP4Parser : Box
    {
        public string FileName { get; }
        public Moov Moov { get; private set; }
        internal Moof Moof { get; private set; }
        public Subtitle VttcSubtitle { get; private set; }
        public string VttcLanguage { get; private set; }

        public Subtitle TrunCea608Subtitle { get; private set; }
        public Subtitle TrunCea708Subtitle { get; private set; }
        private List<Cea608.CcData> _trunCea608CcData = new List<Cea608.CcData>();
        public string DebugInfo { get; private set; }

        public List<Trak> GetSubtitleTracks()
        {
            var list = new List<Trak>();
            if (Moov?.Tracks == null)
            {
                return list;
            }

            foreach (var trak in Moov.Tracks)
            {
                if (trak.Mdia != null && (trak.Mdia.IsTextSubtitle || trak.Mdia.IsVobSubSubtitle || trak.Mdia.IsClosedCaption) &&
                    trak.Mdia.Minf?.Stbl != null && trak.Mdia.Minf.Stbl.GetParagraphs().Count > 0)
                {
                    list.Add(trak);
                }
            }

            return list;
        }

        public List<Trak> GetAudioTracks()
        {
            var list = new List<Trak>();
            if (Moov?.Tracks == null)
            {
                return list;
            }

            foreach (var trak in Moov.Tracks)
            {
                if (trak.Mdia != null && trak.Mdia.IsAudio)
                {
                    list.Add(trak);
                }
            }

            return list;
        }

        public List<Trak> GetVideoTracks()
        {
            var list = new List<Trak>();
            if (Moov?.Tracks == null)
            {
                return list;
            }

            foreach (var trak in Moov.Tracks)
            {
                if (trak.Mdia != null && trak.Mdia.IsVideo)
                {
                    list.Add(trak);
                }
            }

            return list;
        }

        public TimeSpan Duration
        {
            get
            {
                if (Moov?.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                {
                    return TimeSpan.FromSeconds((double)Moov.Mvhd.Duration / Moov.Mvhd.TimeScale);
                }

                return new TimeSpan();
            }
        }

        public DateTime CreationDate
        {
            get
            {
                if (Moov?.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                {
                    return new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(Moov.Mvhd.CreationTime));
                }

                return DateTime.Now;
            }
        }

        /// <summary>
        /// Resolution of first video track. If not present returns 0.0
        /// </summary>
        public System.Drawing.Point VideoResolution
        {
            get
            {
                if (Moov?.Tracks == null)
                {
                    return new System.Drawing.Point(0, 0);
                }

                foreach (var trak in Moov.Tracks)
                {
                    if (trak?.Mdia != null && trak.Tkhd != null && trak.Mdia.IsVideo)
                    {
                        return new System.Drawing.Point((int)trak.Tkhd.Width, (int)trak.Tkhd.Height);
                    }
                }

                return new System.Drawing.Point(0, 0);
            }
        }

        public MP4Parser(string fileName)
        {
            FileName = fileName;
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ParseMp4(fs);
                fs.Close();
            }
        }

        private void ParseMp4(Stream fs)
        {
            var count = 0;
            Position = 0;
            fs.Seek(0, SeekOrigin.Begin);
            var moreBytes = true;
            var timeTotalMs = 0d;
            while (moreBytes)
            {
                moreBytes = InitializeSizeAndName(fs);
                if (Size < 8)
                {
                    return;
                }

                var savedPos = fs.Position;
                var bytesToRead = (int)Math.Min(100L, Math.Max(0L, (long)Size - 8));
                if (bytesToRead > 0)
                {
                    var headerBytes = new byte[bytesToRead];
                    var headerBytesRead = fs.Read(headerBytes, 0, bytesToRead);
                    fs.Seek(savedPos, SeekOrigin.Begin);
                }

                if (Name == "moov" && Moov == null)
                {
                    Moov = new Moov(fs, Position); // only scan first "moov" element
                }
                else if (Name == "moof")
                {
                    Moof = new Moof(fs, Position);

                    if (Moof.Traf?.Trun?.DataOffset != null && Moof.Traf.Tfdt != null)
                    {
                        var dts = Moof.Traf.Tfdt.BaseMediaDecodeTime;
                        var startPosition = (uint)(Moof.StartPosition + Moof.Traf.Trun.DataOffset.Value);
                        for (var index = 0; index < Moof.Traf.Trun.Samples.Count; index++)
                        {
                            var sample = Moof.Traf.Trun.Samples[index];
                            if (sample.Size.HasValue)
                            {
                                var ccData = GetCcDataHelper.GetCcData(fs, startPosition, sample.Size.Value);
                                if (ccData.Count > 0)
                                {
                                    if (sample.TimeOffset.HasValue)
                                    {
                                        ccData[0].Time = (ulong)((long)dts + sample.TimeOffset.Value);
                                    }

                                    _trunCea608CcData.Add(ccData[0]); //TODO: can there be more than one?
                                }

                                startPosition += sample.Size.Value;
                            }

                            if (sample.Duration.HasValue)
                            {
                                dts += sample.Duration.Value;
                            }
                        }
                    }
                }
                else if (Name == "mdat" && Moof != null && Moof?.Traf?.Trun?.Samples?.Count > 0)
                {
                    var mdat = new Mdat(fs, Position);
                    if (Moof.Traf?.Trun?.Samples.Count > 0)
                    {
                        if (VttcSubtitle == null)
                        {
                            VttcSubtitle = new Subtitle();

                            if (Moov?.Tracks.FirstOrDefault()?.Mdia?.Mdhd != null)
                            {
                                var track = Moov.Tracks.FirstOrDefault();
                                VttcLanguage = track.Mdia.Mdhd.Iso639ThreeLetterCode;
                                if (string.IsNullOrEmpty(VttcLanguage))
                                {
                                    VttcLanguage = track.Mdia.Mdhd.LanguageString;
                                }
                            }
                        }

                        if (Moof.Traf.Trun.Samples.All(p => p.Size != null))
                        {
                            ReadVttWithSize(mdat, Moof.Traf.Trun.Samples, ref timeTotalMs);
                        }
                        else
                        {
                            ReadVttWithoutSize(mdat.Vtts, Moof.Traf.Trun.Samples, ref timeTotalMs);
                        }
                    }

                    Moof = null;
                }

                count++;
                if (count > 3000)
                {
                    break;
                }

                if (Position > (ulong)fs.Length)
                {
                    break;
                }

                fs.Seek((long)Position, SeekOrigin.Begin);
            }

            fs.Close();

            if (VttcSubtitle != null)
            {
                var merged = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(VttcSubtitle, false, 250);
                VttcSubtitle = merged;
            }

            CheckForTrunCea608();
            CheckForMoovVideoCea608();
        }

        private void CheckForMoovVideoCea608()
        {
            try
            {
                if (TrunCea608Subtitle?.Paragraphs.Count > 0)
                {
                    //debugInfo.AppendLine("CheckForMoovVideoCea608: skipped (fragmented path already found data)");
                    return;
                }

                var videoTracks = GetVideoTracks();
                if (videoTracks.Count == 0)
                {
                    //debugInfo.AppendLine("CheckForMoovVideoCea608: no video tracks found");
                    return;
                }

                var stbl = videoTracks[0].Mdia?.Minf?.Stbl;
                if (stbl?.ChunkOffsets == null || stbl.ChunkOffsets.Count == 0 ||
                    stbl.SampleSizes.Count == 0 || stbl.Ssts.Count == 0)
                {
                    //debugInfo.AppendLine($"CheckForMoovVideoCea608: stbl incomplete (chunks={stbl?.ChunkOffsets?.Count ?? 0}, sizes={stbl?.SampleSizes?.Count ?? 0}, ssts={stbl?.Ssts?.Count ?? 0})");
                    return;
                }

                //debugInfo.AppendLine($"CheckForMoovVideoCea608: chunks={stbl.ChunkOffsets.Count}, sizes={stbl.SampleSizes.Count}, ssts={stbl.Ssts.Count}, stsc={stbl.Stsc.Count}");

                var timeScale = stbl.TimeScale > 0 ? stbl.TimeScale : (Moov?.Mvhd?.TimeScale ?? 1000UL);
                var ccDataList = new List<CcData>();
                var samplesScanned = 0;
                var ccTypeCounts = new int[4];

                using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    uint samplesPerChunk = 1;
                    var index = 0;
                    ulong totalTicks = 0;
                    var stscLookup = stbl.Stsc.ToDictionary(p => p.FirstChunk);
                    var done = false;

                    for (var chunkIndex = 0; chunkIndex < stbl.ChunkOffsets.Count && !done; chunkIndex++)
                    {
                        if (stscLookup.TryGetValue((uint)chunkIndex + 1, out var newSpc))
                        {
                            samplesPerChunk = newSpc.SamplesPerChunk;
                        }

                        var chunkOffset = stbl.ChunkOffsets[chunkIndex];

                        for (var i = 0; i < samplesPerChunk; i++)
                        {
                            if (index >= stbl.SampleSizes.Count || index >= stbl.Ssts.Count)
                            {
                                done = true;
                                break;
                            }

                            var sampleSize = stbl.SampleSizes[index];
                            var sampleTicks = stbl.Ssts[index];

                            if (sampleSize > 4)
                            {
                                // Older code capped this at 1000 bytes, which assumed the SEI
                                // NAL was always at the very start of the access unit. Real
                                // H.264 encoders interleave SEI between/after slice NALs, so
                                // capping at 1 kB dropped most of the cc_data on larger
                                // samples (verified against a real CEA-708 sample: ~3400
                                // type-2 triplets in the file, only a handful seen with the
                                // old cap). Cap at the actual sample size — GetCcData stops
                                // at NAL boundaries so the cost is just a few extra reads.
                                var scanSize = (ulong)sampleSize;
                                var ccData = GetCcDataHelper.GetCcData(fs, chunkOffset, scanSize);
                                // Use presentation timestamp (DTS + ctts offset) so cc_data from B-frames lands in display order.
                                var cttsOffset = index < stbl.Ctts.Count ? stbl.Ctts[index] : 0;
                                var pts = (ulong)((long)totalTicks + cttsOffset);
                                foreach (var cc in ccData)
                                {
                                    cc.Time = pts;
                                    ccDataList.Add(cc);
                                    if (cc.Type >= 0 && cc.Type < ccTypeCounts.Length)
                                    {
                                        ccTypeCounts[cc.Type]++;
                                    }
                                }

                                if (samplesScanned == 0)
                                {
                                    CountRawCcTypes(fs, chunkOffset, scanSize, ccTypeCounts);
                                }
                            }

                            totalTicks += sampleTicks;
                            samplesScanned++;
                            index++;
                            chunkOffset += sampleSize;
                        }
                    }
                }

                //debugInfo.AppendLine($"CheckForMoovVideoCea608: scanned={samplesScanned}, cea608entries={ccDataList.Count} (type0={ccTypeCounts[0]}, type1={ccTypeCounts[1]}, type2={ccTypeCounts[2]}, type3={ccTypeCounts[3]})");

                if (ccDataList.Count == 0)
                {
                    return;
                }

                var sortedCcData = ccDataList.OrderBy(p => p.Time).ToList();

                // CEA-608 (NTSC fields 1 + 2). Isolated in its own try so a
                // failure in the 608 decoder doesn't suppress the 708 path.
                var cea608Entries = sortedCcData.Where(c => c.Type == 0 || c.Type == 1).ToList();
                if (cea608Entries.Count > 0)
                {
                    try
                    {
                        TrunCea608Subtitle = new Subtitle();
                        var cea608Parser = new CcDataC608Parser();
                        cea608Parser.DisplayScreen += data =>
                        {
                            var startMs = data.Start / (double)timeScale * 1000.0;
                            var endMs = data.End / (double)timeScale * 1000.0;
                            TrunCea608Subtitle.Paragraphs.Add(new Paragraph(GetText(data.Screen), startMs, endMs));
                        };
                        foreach (var cc in cea608Entries)
                        {
                            cea608Parser.AddData((int)cc.Time, new[] { cc.Data1, cc.Data2 });
                        }
                    }
                    catch (Exception e)
                    {
                        SeLogger.Error(e, "Error while parsing MP4 moov video track CEA-608");
                    }
                }

                // CEA-708 (DTVCC). Run regardless of 608's success/failure —
                // many real broadcast MP4s carry only one or the other.
                DecodeMoovVideoCea708(sortedCcData, timeScale);

                //debugInfo.AppendLine($"CheckForMoovVideoCea608: paragraphs={TrunCea608Subtitle?.Paragraphs.Count ?? 0}, cea708 paragraphs={TrunCea708Subtitle?.Paragraphs.Count ?? 0}");
            }
            catch (Exception e)
            {
                SeLogger.Error(e, "Error while parsing MP4 moov video track CEA-608");
            }
        }

        private void DecodeMoovVideoCea708(List<Cea608.CcData> sortedCcData, ulong timeScale)
        {
            try
            {
                // Step 1: assemble DTVCC packets from the cc_type 3 (PACKET_START)
                // and cc_type 2 (PACKET_DATA) triplet stream. Each PACKET_START
                // triplet carries: data1 = packet header (sequence<<6 | size_code),
                // data2 = first byte of packet content. Subsequent PACKET_DATA
                // triplets contribute 2 more content bytes each.
                var packets = new List<DtvccPacket>();
                DtvccPacket current = null;
                foreach (var cc in sortedCcData)
                {
                    if (cc.Type == 3)
                    {
                        if (current != null)
                        {
                            packets.Add(current);
                        }
                        current = new DtvccPacket { Time = cc.Time, Header = (byte)cc.Data1 };
                        current.Content.Add((byte)cc.Data2);
                    }
                    else if (cc.Type == 2 && current != null)
                    {
                        current.Content.Add((byte)cc.Data1);
                        current.Content.Add((byte)cc.Data2);
                    }
                }
                if (current != null)
                {
                    packets.Add(current);
                }

                if (packets.Count == 0)
                {
                    return;
                }

                // Step 2: for each packet, parse out service blocks. Service 1 is
                // the primary caption service; we collect its bytes per-packet
                // (timestamped at the packet's PTS) and feed them to Cea708.Decode.
                TrunCea708Subtitle = new Subtitle();
                var state = new Cea708.CommandState();
                var packetTimesMs = new List<double>();

                foreach (var packet in packets)
                {
                    var service1Bytes = ExtractService1(packet);
                    if (service1Bytes.Length == 0)
                    {
                        continue;
                    }

                    var packetMs = packet.Time / (double)timeScale * 1000.0;
                    packetTimesMs.Add(packetMs);
                    var lineIndex = packetTimesMs.Count - 1;

                    var text = Cea708.Cea708.Decode(lineIndex, service1Bytes, state, flush: false);
                    EmitCea708Paragraph(text, state, packetTimesMs, endMs: packetMs);
                }

                // Final flush so any text still buffered (no terminating display
                // command in the stream) still gets surfaced.
                if (packetTimesMs.Count > 0)
                {
                    var tailText = Cea708.Cea708.Decode(packetTimesMs.Count, Array.Empty<byte>(), state, flush: true);
                    EmitCea708Paragraph(tailText, state, packetTimesMs, endMs: packetTimesMs[packetTimesMs.Count - 1]);
                }

                if (TrunCea708Subtitle.Paragraphs.Count == 0)
                {
                    TrunCea708Subtitle = null;
                }
            }
            catch (Exception e)
            {
                SeLogger.Error(e, "Error while parsing MP4 moov video track CEA-708");
            }
        }

        // Walk a DTVCC packet's service blocks and return the concatenated bytes
        // belonging to service 1 (the primary caption service — by far the most
        // common; extended services 2..63 would carry alternate languages).
        // Service block header byte: bits 7-5 = service_number, bits 4-0 = block_size.
        // If service_number == 7, an extended_service_number byte follows.
        // service_number == 0 with block_size == 0 marks the end of the packet
        // (NULL service block / padding).
        private static byte[] ExtractService1(DtvccPacket packet)
        {
            var result = new List<byte>();
            var content = packet.Content;
            // packet_size_code in the low 6 bits of the header: 0 → 128 bytes,
            // n → n*2 bytes (TOTAL packet length including the header). Clamp
            // to what we actually have so a malformed truncated packet doesn't
            // walk past the buffer.
            var sizeCode = packet.Header & 0x3F;
            var declaredPacketBytes = sizeCode == 0 ? 128 : sizeCode * 2;
            var contentBytesFromHeader = declaredPacketBytes - 1; // minus 1-byte packet header
            var limit = Math.Min(content.Count, contentBytesFromHeader);

            var i = 0;
            while (i < limit)
            {
                var header = content[i++];
                var serviceNum = (header >> 5) & 0x07;
                var blockSize = header & 0x1F;

                if (serviceNum == 0 && blockSize == 0)
                {
                    break; // NULL service block — rest of packet is padding
                }

                if (serviceNum == 7)
                {
                    if (i >= limit) break;
                    serviceNum = content[i++] & 0x3F;
                }

                if (i + blockSize > limit)
                {
                    break; // malformed: declared block runs past packet
                }

                if (serviceNum == 1 && blockSize > 0)
                {
                    for (var j = 0; j < blockSize; j++)
                    {
                        result.Add(content[i + j]);
                    }
                }
                i += blockSize;
            }

            return result.ToArray();
        }

        private class DtvccPacket
        {
            public ulong Time;
            public byte Header;
            public List<byte> Content { get; } = new List<byte>();
        }

        private void EmitCea708Paragraph(string text, Cea708.CommandState state, List<double> frameTimesMs, double endMs)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // state.StartLineIndex is set by Cea708.FlushText to the lineIndex of
            // the first SetText command that contributed to the just-emitted
            // caption — i.e., when the text "started". Clamp defensively in case
            // the index isn't valid (e.g., flush with empty state).
            var startIndex = state.StartLineIndex >= 0 && state.StartLineIndex < frameTimesMs.Count
                ? state.StartLineIndex
                : frameTimesMs.Count - 1;
            var startMs = frameTimesMs[startIndex];
            TrunCea708Subtitle.Paragraphs.Add(new Paragraph(text.Trim(), startMs, endMs));
        }

        private static void CountRawCcTypes(Stream fs, ulong chunkOffset, ulong scanSize, int[] ccTypeCounts)
        {
            try
            {
                var atscId = new byte[] { 0xB5, 0x00, 0x31, 0x47, 0x41, 0x39, 0x34, 0x03 };
                var buf = new byte[scanSize];
                fs.Seek((long)chunkOffset, SeekOrigin.Begin);
                var read = fs.Read(buf, 0, buf.Length);

                for (var pos = 0; pos < read - 20; pos++)
                {
                    var match = true;
                    for (var k = 0; k < atscId.Length; k++)
                    {
                        if (buf[pos + k] != atscId[k]) { match = false; break; }
                    }

                    if (!match)
                    {
                        continue;
                    }

                    var flagsByte = buf[pos + 8];          // byte after 8-byte ATSC id: cc_data_flags
                    var ccCount = flagsByte & 0x1F;
                    var emDataPresent = (flagsByte >> 7) & 1; // process_em_data_flag
                    var ccStart = pos + 9 + emDataPresent; // skip id(8) + flags(1) + optional em_data(1)
                    var rawTypeCounts = new int[4];
                    for (var j = 0; j < ccCount && ccStart + j * 3 + 2 < read; j++)
                    {
                        var marker = buf[ccStart + j * 3];
                        var ccType = marker & 0x3;
                        if (ccType < 4) rawTypeCounts[ccType]++;
                    }

                    break;
                }
            }
            catch
            {
                // ignore debug errors
            }
        }

        private void CheckForTrunCea608()
        {
            try
            {
                TrunCea608Subtitle = new Subtitle();
                var sortedData = _trunCea608CcData.OrderBy(p => p.Time).ToList();
                var parser = new CcDataC608Parser();
                parser.DisplayScreen += DisplayScreen;
                foreach (var cc in sortedData)
                {
                    parser.AddData((int)cc.Time, new[] { cc.Data1, cc.Data2 });
                }
            }
            catch (Exception e)
            {
                SeLogger.Error(e, "Error while parsing MP4 TRUN CEA 608");
            }
        }

        private void DisplayScreen(DataOutput data)
        {
            var timeScale = Moov?.Mvhd?.TimeScale ?? 1000.0;
            var startMs = data.Start / timeScale * 1000.0;
            var endMs = data.End / timeScale * 1000.0;
            var p = new Paragraph(GetText(data.Screen), startMs, endMs);
            TrunCea608Subtitle.Paragraphs.Add(p);
        }

        private static string GetText(SerializedRow[] dataScreen)
        {
            var sb = new StringBuilder();

            foreach (var row in dataScreen)
            {
                foreach (var column in row.Columns)
                {
                    sb.Append(column.Character);
                }
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }

        private void ReadVttWithSize(Mdat mdat, List<TimeSegment> trunSamples, ref double timeTotalMs)
        {
            var payloadIndex = 0;
            var timeScale = Moov?.Mvhd?.TimeScale ?? 1000.0;
            foreach (var timeSegment in trunSamples)
            {
                var before = timeTotalMs;
                if (timeSegment.Duration.HasValue)
                {
                    timeTotalMs += timeSegment.Duration.Value / timeScale * 1000.0;
                }

                var timeSegmentSize = timeSegment.Size;
                if (payloadIndex < mdat.Vtts?.Count && timeSegmentSize > 8)
                {
                    var payloadSize = mdat.Vtts[payloadIndex].PayloadSize;
                    var payload = mdat.Vtts[payloadIndex].Payload;
                    var style = mdat.Vtts[payloadIndex].Style;

                    if (timeSegment.Duration.HasValue && payload != null)
                    {
                        AddVttParagraph(timeTotalMs, payload, before, style);
                    }

                    while (payloadIndex + 1 < mdat.Vtts.Count && timeSegmentSize >= payloadSize + mdat.Vtts[payloadIndex + 1].PayloadSize)
                    {
                        payloadIndex++;
                        payload = mdat.Vtts[payloadIndex].Payload;
                        style = mdat.Vtts[payloadIndex].Style;

                        if (timeSegment.Duration.HasValue && payload != null)
                        {
                            AddVttParagraph(timeTotalMs, payload, before, style);
                        }

                        payloadSize += mdat.Vtts[payloadIndex].PayloadSize; // add 8
                    }
                }

                payloadIndex++;
            }
        }

        private void AddVttParagraph(double timeTotalMs, string payload, double before, string style)
        {
            var p = new Paragraph(payload, before, timeTotalMs);
            var positionInfo = WebVTT.GetAssAlignmentTagFromCueSettings(style);
            if (!string.IsNullOrEmpty(positionInfo))
            {
                p.Text = positionInfo + p.Text;
            }

            p.Style = string.IsNullOrEmpty(positionInfo)
                ? WebVTT.GetPositionInfoRaw(style)
                : string.Empty;
            if (!string.IsNullOrEmpty(p.Style) || !string.IsNullOrEmpty(positionInfo))
            {
                VttcSubtitle.Header = "WEBVTT";
            }
            VttcSubtitle.Paragraphs.Add(p);
        }

        private void ReadVttWithoutSize(List<Vttc.VttData> vtts, List<TimeSegment> trunSamples, ref double timeTotalMs)
        {
            if (vtts == null || trunSamples.Count <= 0 || trunSamples.Count < vtts.Count)
            {
                return;
            }

            var timeScale = Moov?.Mvhd?.TimeScale ?? 1000.0;
            var sampleIdx = 0;
            foreach (var vtt in vtts)
            {
                var presentation = Moof.Traf.Trun.Samples[sampleIdx];
                if (presentation.Duration.HasValue)
                {
                    var before = timeTotalMs;
                    timeTotalMs += presentation.Duration.Value / timeScale * 1000.0;
                    sampleIdx++;
                    if (vtt.Payload != null)
                    {
                        VttcSubtitle.Paragraphs.Add(new Paragraph(vtt.Payload, before, timeTotalMs));
                    }
                }
            }
        }

        internal double FrameRate
        {
            get
            {
                // Formula: moov.mdia.stbl.stsz.samplecount / (moov.trak.tkhd.duration / moov.mvhd.timescale) - http://www.w3.org/2008/WebVideo/Annotations/drafts/ontology10/CR/test.php?table=containerMPEG4
                if (Moov?.Mvhd == null || Moov.Mvhd.TimeScale <= 0)
                {
                    return 0;
                }

                var videoTracks = GetVideoTracks();
                if (videoTracks.Count > 0 && videoTracks[0].Tkhd != null && videoTracks[0].Mdia?.Minf?.Stbl != null)
                {
                    double duration = videoTracks[0].Tkhd.Duration;
                    double sampleCount = videoTracks[0].Mdia.Minf.Stbl.StszSampleCount;
                    return sampleCount / (duration / Moov.Mvhd.TimeScale);
                }

                return 0;
            }
        }

        public List<string> GetMdatsAsStrings()
        {
            var list = new List<string>();
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Position = 0;
                fs.Seek(0, SeekOrigin.Begin);
                var moreBytes = true;
                while (moreBytes)
                {
                    moreBytes = InitializeSizeAndName(fs);
                    if (Size < 8)
                    {
                        return list;
                    }

                    if (Name == "mdat")
                    {
                        var before = fs.Position;
                        var readLength = (int)((long)Position - before);
                        if (readLength > 10 && readLength < 1_000_000)
                        {
                            var buffer = new byte[readLength];
                            fs.Read(buffer, 0, readLength);
                            list.Add(Encoding.UTF8.GetString(buffer));
                        }
                    }

                    if (Position > (ulong)fs.Length)
                    {
                        break;
                    }

                    fs.Seek((long)Position, SeekOrigin.Begin);
                }
                fs.Close();
            }
            return list;
        }
    }
}
