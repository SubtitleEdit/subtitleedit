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

        /// <summary>
        /// Sample codec of <see cref="VttcSubtitle"/>: "wvtt", "stpp", "tx3g", "stxt" or "sbtt".
        /// </summary>
        public string VttcCodec { get; private set; }

        /// <summary>
        /// All text subtitle tracks found in movie fragments (DASH/CMAF), in file order.
        /// <see cref="VttcSubtitle"/> is the first of these.
        /// </summary>
        public List<Mp4FragmentedSubtitleTrack> FragmentedSubtitleTracks { get; } = new List<Mp4FragmentedSubtitleTrack>();

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

        /// <summary>
        /// Upper bound on top-level boxes to walk. A DASH/CMAF file holds styp+moof+mdat per
        /// segment, so a long subtitle representation legitimately has many thousands of them -
        /// a flat cap silently truncated the extraction (a 2-second-segment file stopped after
        /// ~33 minutes). Real segments are at least a few hundred bytes, so scale with the file
        /// size and keep a ceiling so a malformed file still cannot spin for long.
        /// </summary>
        private static int GetMaxTopLevelBoxes(long fileLength)
        {
            const long minBoxes = 3000;
            const long maxBoxes = 200_000;
            var scaled = fileLength / 32;
            return (int)(scaled < minBoxes ? minBoxes : scaled > maxBoxes ? maxBoxes : scaled);
        }

        private void ParseMp4(Stream fs)
        {
            var count = 0;
            var maxBoxes = GetMaxTopLevelBoxes(fs.Length);
            Position = 0;
            fs.Seek(0, SeekOrigin.Begin);
            var moreBytes = true;
            while (moreBytes)
            {
                moreBytes = InitializeSizeAndName(fs);
                if (Size < 8)
                {
                    return;
                }

                if (Name == "moov" && Moov == null)
                {
                    Moov = new Moov(fs, Position); // only scan first "moov" element
                }
                else if (Name == "moof")
                {
                    Moof = new Moof(fs, Position);
                    ApplyTrexDefaults();
                    ReadFragmentedCcSamples(fs);
                }
                else if (Name == "mdat" && Moof != null)
                {
                    ReadFragmentedTextSamples(fs, (ulong)fs.Position, Position);
                    Moof = null;
                }

                count++;
                if (count > maxBoxes)
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

            // Surface the fragmented text tracks (DASH/CMAF subtitle representations, or
            // the subtitle tracks of a muxed fMP4); VttcSubtitle is the first of them.
            foreach (var fragmentedTrack in _fragmentedTextTracks)
            {
                if (fragmentedTrack.Subtitle.Paragraphs.Count == 0)
                {
                    continue;
                }

                var sorted = fragmentedTrack.Subtitle.Paragraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
                fragmentedTrack.Subtitle.Paragraphs.Clear();
                fragmentedTrack.Subtitle.Paragraphs.AddRange(sorted);

                var merged = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(fragmentedTrack.Subtitle, false, 250);
                merged.Header = fragmentedTrack.Subtitle.Header;
                merged.Renumber();

                FragmentedSubtitleTracks.Add(new Mp4FragmentedSubtitleTrack
                {
                    TrackId = fragmentedTrack.TrackId,
                    Language = fragmentedTrack.Language,
                    Codec = fragmentedTrack.Codec,
                    Subtitle = merged,
                });
            }

            var firstFragmentedTrack = FragmentedSubtitleTracks.FirstOrDefault();
            if (firstFragmentedTrack != null)
            {
                VttcSubtitle = firstFragmentedTrack.Subtitle;
                VttcLanguage = firstFragmentedTrack.Language;
                VttcCodec = firstFragmentedTrack.Codec;
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

                using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    uint samplesPerChunk = 1;
                    var index = 0;
                    ulong totalTicks = 0;
                    var stscLookup = stbl.GetStscLookup();
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
                                }
                            }

                            totalTicks += sampleTicks;
                            samplesScanned++;
                            index++;
                            chunkOffset += sampleSize;
                        }
                    }
                }

                //debugInfo.AppendLine($"CheckForMoovVideoCea608: scanned={samplesScanned}, cea608entries={ccDataList.Count}");

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
            // Fragment ticks are media-track times, so prefer the video track's mdhd
            // timescale; the movie (mvhd) timescale is only a fallback.
            var timeScale = Moov?.Mvhd?.TimeScale ?? 1000.0;
            var videoTrack = GetVideoTracks().FirstOrDefault();
            if (videoTrack?.Mdia?.Mdhd?.TimeScale > 0)
            {
                timeScale = videoTrack.Mdia.Mdhd.TimeScale;
            }

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

        private sealed class FragmentedTextTrack
        {
            public uint? TrackId { get; set; }
            public Subtitle Subtitle { get; } = new Subtitle();
            public string Language { get; set; }
            public string Codec { get; set; } // "wvtt", "stpp", "tx3g", "stxt" or "sbtt"
            public double LegacyTimeTotalMs; // running clock for fragments without data offsets
            public long NextTicks; // running decode time for fragments without tfdt
            public HashSet<string> AddedTtmlCues { get; } = new HashSet<string>();
        }

        private readonly List<FragmentedTextTrack> _fragmentedTextTracks = new List<FragmentedTextTrack>();

        private Trak FindTrack(uint? trackId)
        {
            if (trackId == null || Moov?.Tracks == null)
            {
                return null;
            }

            foreach (var trak in Moov.Tracks)
            {
                if (trak.Tkhd?.TrackId == trackId.Value)
                {
                    return trak;
                }
            }

            return Moov.Tracks.Count == 1 ? Moov.Tracks[0] : null;
        }

        /// <summary>
        /// Timescale for a fragment's ticks (tfdt/sample durations). Those are media-track
        /// times, so the track's mdhd timescale applies - the movie (mvhd) timescale is a
        /// different clock and using it skewed all DASH/CMAF cue times by their ratio.
        /// </summary>
        private double GetTrackTimeScale(Trak trak)
        {
            if (trak?.Mdia?.Mdhd?.TimeScale > 0)
            {
                return trak.Mdia.Mdhd.TimeScale;
            }

            if (Moov?.Tracks?.Count == 1 && Moov.Tracks[0].Mdia?.Mdhd?.TimeScale > 0)
            {
                return Moov.Tracks[0].Mdia.Mdhd.TimeScale;
            }

            if (Moov?.Mvhd?.TimeScale > 0)
            {
                return Moov.Mvhd.TimeScale;
            }

            return 1000.0;
        }

        /// <summary>
        /// Fills fragment samples that carry no duration/size in trun or tfhd with the
        /// per-track defaults from moov/mvex/trex (common for the last DASH segment,
        /// whose tfhd often omits default-sample-duration).
        /// </summary>
        private void ApplyTrexDefaults()
        {
            if (Moov == null || Moov.Trexs.Count == 0)
            {
                return;
            }

            foreach (var traf in Moof.Trafs)
            {
                var trackId = traf.Tfhd?.TrackId;
                Trex trex = null;
                foreach (var t in Moov.Trexs)
                {
                    if (trackId == null || t.TrackId == trackId.Value)
                    {
                        trex = t;
                        break;
                    }
                }

                if (trex == null)
                {
                    continue;
                }

                foreach (var trun in traf.Truns)
                {
                    foreach (var sample in trun.Samples)
                    {
                        if (sample.Duration == null && trex.DefaultSampleDuration > 0)
                        {
                            sample.Duration = trex.DefaultSampleDuration;
                        }

                        if (sample.Size == null && trex.DefaultSampleSize > 0)
                        {
                            sample.Size = trex.DefaultSampleSize;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// CEA-608/708 byte pairs from the pending moof's sample runs (H.264 SEI in
        /// fragmented video). Only video/clcp tracks can carry them; text and audio
        /// track fragments are skipped.
        /// </summary>
        private void ReadFragmentedCcSamples(Stream fs)
        {
            foreach (var traf in Moof.Trafs)
            {
                var trak = FindTrack(traf.Tfhd?.TrackId);
                if (trak?.Mdia != null && !trak.Mdia.IsVideo && !trak.Mdia.IsClosedCaption)
                {
                    continue;
                }

                if (traf.Tfdt == null)
                {
                    continue;
                }

                var dts = traf.Tfdt.BaseMediaDecodeTime;
                // trun data offsets are relative to tfhd's base-data-offset when present
                // (PIFF/Smooth Streaming sets it), and to the moof start otherwise.
                var baseOffset = traf.Tfhd?.BaseDataOffset ?? Moof.StartPosition;
                var haveStartPosition = false;
                ulong startPosition = 0;
                foreach (var trun in traf.Truns)
                {
                    if (trun.DataOffset != null)
                    {
                        startPosition = (ulong)((long)baseOffset + trun.DataOffset.Value);
                        haveStartPosition = true;
                    }

                    if (!haveStartPosition)
                    {
                        break;
                    }

                    for (var index = 0; index < trun.Samples.Count; index++)
                    {
                        var sample = trun.Samples[index];
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
        }

        /// <summary>
        /// Text subtitle samples (wvtt/stpp/tx3g/stxt/sbtt) from the pending moof's track fragments.
        /// Slices exact sample byte ranges via trun data offsets and sizes, so it also works
        /// for muxed fMP4 where the mdat interleaves video/audio/subtitle data. Falls back
        /// to scanning the mdat for WebVTT cue boxes when no offsets are available.
        /// </summary>
        private void ReadFragmentedTextSamples(Stream fs, ulong mdatDataStart, ulong mdatEnd)
        {
            foreach (var traf in Moof.Trafs)
            {
                var trackId = traf.Tfhd?.TrackId;
                var trak = FindTrack(trackId);
                if (trak?.Mdia != null && !trak.Mdia.IsTextSubtitle)
                {
                    continue; // audio/video/vobsub; CEA captions are handled at moof time
                }

                var samples = new List<TimeSegment>();
                foreach (var trun in traf.Truns)
                {
                    samples.AddRange(trun.Samples);
                }

                if (samples.Count == 0)
                {
                    continue;
                }

                var stsdCodec = trak?.Mdia?.Minf?.Stbl?.Stsd?.Name;
                var timeScale = GetTrackTimeScale(trak);
                var track = GetFragmentedTextTrack(trackId, trak);

                var haveOffsets = traf.Tfhd?.BaseDataOffset != null || traf.Truns[0].DataOffset != null;
                var haveSizes = samples.All(p => p.Size != null);
                if (haveOffsets && haveSizes)
                {
                    ReadFragmentedTextSamplesPrecise(fs, traf, stsdCodec, timeScale, track);
                }
                else
                {
                    fs.Seek((long)mdatDataStart, SeekOrigin.Begin);
                    var mdat = new Mdat(fs, mdatEnd);
                    if (track.Codec == null && mdat.Vtts.Count > 0)
                    {
                        track.Codec = "wvtt";
                    }

                    if (haveSizes)
                    {
                        ReadVttWithSize(track, mdat, samples, timeScale);
                    }
                    else
                    {
                        ReadVttWithoutSize(track, mdat.Vtts, samples, timeScale);
                    }
                }
            }
        }

        private FragmentedTextTrack GetFragmentedTextTrack(uint? trackId, Trak trak)
        {
            foreach (var existing in _fragmentedTextTracks)
            {
                if (existing.TrackId == trackId)
                {
                    return existing;
                }
            }

            var track = new FragmentedTextTrack { TrackId = trackId };
            var mdhd = trak?.Mdia?.Mdhd ?? Moov?.Tracks?.FirstOrDefault()?.Mdia?.Mdhd;
            if (mdhd != null)
            {
                track.Language = mdhd.Iso639ThreeLetterCode;
                if (string.IsNullOrEmpty(track.Language))
                {
                    track.Language = mdhd.LanguageString;
                }
            }

            _fragmentedTextTracks.Add(track);
            return track;
        }

        private void ReadFragmentedTextSamplesPrecise(Stream fs, Traf traf, string stsdCodec, double timeScale, FragmentedTextTrack track)
        {
            const uint maxSampleSize = 10_000_000; // subtitle samples are small; guard against malformed sizes

            // without a tfdt, fragment times continue from the previous fragment of this track
            var ticks = traf.Tfdt != null ? (long)traf.Tfdt.BaseMediaDecodeTime : track.NextTicks;
            var baseOffset = traf.Tfhd?.BaseDataOffset ?? Moof.StartPosition;
            var samplePosition = baseOffset;
            foreach (var trun in traf.Truns)
            {
                if (trun.DataOffset != null)
                {
                    samplePosition = (ulong)((long)baseOffset + trun.DataOffset.Value);
                }

                foreach (var sample in trun.Samples)
                {
                    var size = sample.Size ?? 0;
                    var startTicks = ticks + (sample.TimeOffset ?? 0);
                    var durationTicks = sample.Duration ?? 0;
                    if (sample.Duration.HasValue)
                    {
                        ticks += sample.Duration.Value;
                    }

                    var startMs = startTicks / timeScale * 1000.0;
                    var durationMs = durationTicks / timeScale * 1000.0;

                    if (size > 2 && size <= maxSampleSize && samplePosition + size <= (ulong)fs.Length)
                    {
                        var buffer = new byte[size];
                        fs.Seek((long)samplePosition, SeekOrigin.Begin);
                        if (fs.Read(buffer, 0, buffer.Length) == buffer.Length)
                        {
                            AddFragmentedTextSample(buffer, stsdCodec, startMs, durationMs, track);
                        }
                    }

                    samplePosition += size;
                }
            }

            track.NextTicks = ticks;
        }

        private static void AddFragmentedTextSample(byte[] sample, string stsdCodec, double startMs, double durationMs, FragmentedTextTrack track)
        {
            var kind = stsdCodec ?? SniffTextSampleKind(sample);
            if (track.Codec == null && kind != null)
            {
                track.Codec = kind;
            }

            if (kind == "wvtt")
            {
                ReadWvttSample(sample, startMs, durationMs, track);
            }
            else if (kind == "stpp")
            {
                ReadStppSample(sample, startMs, durationMs, track);
            }
            else if (Mp4TextSampleHelper.IsSimpleTextCodec(kind))
            {
                ReadSimpleTextSample(sample, startMs, durationMs, track);
            }
            else if (kind != null)
            {
                ReadTx3gSample(sample, startMs, durationMs, track); // tx3g / generic MPEG-4 timed text
            }
        }

        /// <summary>
        /// Codec guess for a subtitle sample when no moov is available (a lone DASH .m4s
        /// segment without its init file): wvtt samples are ISO-BMFF boxes, stpp samples
        /// are TTML XML documents, tx3g samples are a 16-bit length + UTF-8 text, and
        /// anything left that is readable text is treated as an stxt/sbtt text stream.
        /// </summary>
        private static string SniffTextSampleKind(byte[] sample)
        {
            if (sample.Length >= 8)
            {
                var boxSize = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(sample.AsSpan(0, 4));
                if (boxSize >= 8 && boxSize <= (uint)sample.Length)
                {
                    var boxName = Encoding.ASCII.GetString(sample, 4, 4);
                    if (boxName == "vttc" || boxName == "vtte" || boxName == "vtta")
                    {
                        return "wvtt";
                    }
                }
            }

            var i = 0;
            if (sample.Length >= 3 && sample[0] == 0xEF && sample[1] == 0xBB && sample[2] == 0xBF)
            {
                i = 3; // skip UTF-8 BOM
            }

            while (i < sample.Length && (sample[i] == (byte)' ' || sample[i] == (byte)'\t' || sample[i] == (byte)'\r' || sample[i] == (byte)'\n'))
            {
                i++;
            }

            if (i < sample.Length && sample[i] == (byte)'<')
            {
                return "stpp";
            }

            if (sample.Length >= 2)
            {
                int textSize = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(sample.AsSpan(0, 2));
                if (textSize == sample.Length - 2)
                {
                    return "tx3g";
                }

                // text followed by tx3g modifier boxes (styl/hlit/hclr/...)
                if (textSize > 0 && textSize < sample.Length - 2 - 8)
                {
                    var boxStart = 2 + textSize;
                    var boxSize = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(sample.AsSpan(boxStart, 4));
                    var boxName = Encoding.ASCII.GetString(sample, boxStart + 4, 4);
                    if (boxSize >= 8 && boxStart + (long)boxSize <= sample.Length &&
                        (boxName == "styl" || boxName == "hlit" || boxName == "hclr" || boxName == "krok" || boxName == "dlay" || boxName == "href" || boxName == "tbox" || boxName == "blnk" || boxName == "twrp"))
                    {
                        return "tx3g";
                    }
                }
            }

            return IsPlainText(sample) ? "stxt" : null;
        }

        /// <summary>
        /// Whether the whole sample is readable text, i.e. valid UTF-8 without control
        /// characters. Last resort of <see cref="SniffTextSampleKind"/>, so it must not
        /// accept the binary samples of the codecs checked before it.
        /// </summary>
        private static bool IsPlainText(byte[] sample)
        {
            if (sample.Length == 0)
            {
                return false;
            }

            string text;
            try
            {
                text = new UTF8Encoding(false, true).GetString(sample);
            }
            catch (ArgumentException)
            {
                return false;
            }

            var letters = 0;
            foreach (var ch in text)
            {
                if (char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t')
                {
                    return false;
                }

                if (!char.IsWhiteSpace(ch))
                {
                    letters++;
                }
            }

            return letters > 0;
        }

        private static void ReadWvttSample(byte[] sample, double startMs, double durationMs, FragmentedTextTrack track)
        {
            if (durationMs <= 0)
            {
                return;
            }

            string payloadText = null;
            string style = null;
            var pos = 0;
            while (pos + 8 <= sample.Length)
            {
                var boxSize = (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(sample.AsSpan(pos, 4));
                if (boxSize < 8 || pos + boxSize > sample.Length)
                {
                    break;
                }

                var boxName = Encoding.ASCII.GetString(sample, pos + 4, 4);
                if (boxName == "vttc")
                {
                    var inner = pos + 8;
                    var end = pos + boxSize;
                    while (inner + 8 <= end)
                    {
                        var innerSize = (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(sample.AsSpan(inner, 4));
                        if (innerSize < 8 || inner + innerSize > end)
                        {
                            break;
                        }

                        var innerName = Encoding.ASCII.GetString(sample, inner + 4, 4);
                        var contentLength = innerSize - 8;
                        if (innerName == "payl" && contentLength > 0 && contentLength < 5000)
                        {
                            var s = GetString(sample, inner + 8, contentLength).Trim();
                            payloadText = payloadText == null ? s : payloadText + Environment.NewLine + s;
                        }
                        else if (innerName == "sttg" && contentLength > 0 && contentLength < 5000)
                        {
                            style = GetString(sample, inner + 8, contentLength).Trim();
                        }

                        inner += innerSize;
                    }
                }
                // vtte = empty cue (gap marker), vtta = additional text - skip both

                pos += boxSize;
            }

            if (string.IsNullOrEmpty(payloadText))
            {
                return;
            }

            var p = new Paragraph(payloadText, startMs, startMs + durationMs);
            var positionInfo = WebVTT.GetPositionInfo(style);
            if (!string.IsNullOrEmpty(positionInfo))
            {
                p.Text = positionInfo + p.Text;
                p.Extra = style;
                track.Subtitle.Header = "WEBVTT";
            }

            track.Subtitle.Paragraphs.Add(p);
        }

        private static void ReadStppSample(byte[] sample, double startMs, double durationMs, FragmentedTextTrack track)
        {
            var xml = Encoding.UTF8.GetString(sample);
            foreach (var doc in Mp4TtmlHelper.SplitTtmlDocuments(xml))
            {
                var docParagraphs = Mp4TtmlHelper.ParseTtmlDocument(doc);
                if (docParagraphs.Count == 0)
                {
                    continue;
                }

                if (Mp4TtmlHelper.AreTimesSampleRelative(docParagraphs, startMs, durationMs))
                {
                    foreach (var p in docParagraphs)
                    {
                        p.StartTime.TotalMilliseconds += startMs;
                        p.EndTime.TotalMilliseconds += startMs;
                    }
                }

                foreach (var p in docParagraphs)
                {
                    // documents repeat cues that span segment boundaries - add each cue once
                    if (track.AddedTtmlCues.Add($"{p.StartTime.TotalMilliseconds:0}|{p.EndTime.TotalMilliseconds:0}|{p.Text}"))
                    {
                        track.Subtitle.Paragraphs.Add(p);
                    }
                }
            }
        }

        private static void ReadTx3gSample(byte[] sample, double startMs, double durationMs, FragmentedTextTrack track)
        {
            if (durationMs <= 0)
            {
                return;
            }

            var text = Mp4TextSampleHelper.ReadTx3gSampleText(sample);
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            track.Subtitle.Paragraphs.Add(new Paragraph(text, startMs, startMs + durationMs));
        }

        /// <summary>
        /// "stxt"/"sbtt" text stream samples (ISO/IEC 14496-30) - the sample is the text
        /// itself, with no 16-bit length in front of it like tx3g has.
        /// </summary>
        private static void ReadSimpleTextSample(byte[] sample, double startMs, double durationMs, FragmentedTextTrack track)
        {
            if (durationMs <= 0)
            {
                return;
            }

            var text = Mp4TextSampleHelper.ReadSimpleTextSample(sample);
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            track.Subtitle.Paragraphs.Add(new Paragraph(text, startMs, startMs + durationMs));
        }

        private void ReadVttWithSize(FragmentedTextTrack track, Mdat mdat, List<TimeSegment> trunSamples, double timeScale)
        {
            var payloadIndex = 0;
            foreach (var timeSegment in trunSamples)
            {
                var before = track.LegacyTimeTotalMs;
                if (timeSegment.Duration.HasValue)
                {
                    track.LegacyTimeTotalMs += timeSegment.Duration.Value / timeScale * 1000.0;
                }

                var timeSegmentSize = timeSegment.Size;
                if (payloadIndex < mdat.Vtts?.Count && timeSegmentSize > 8)
                {
                    var payloadSize = mdat.Vtts[payloadIndex].PayloadSize;
                    var payload = mdat.Vtts[payloadIndex].Payload;
                    var style = mdat.Vtts[payloadIndex].Style;

                    if (timeSegment.Duration.HasValue && payload != null)
                    {
                        AddVttParagraph(track, track.LegacyTimeTotalMs, payload, before, style);
                    }

                    while (payloadIndex + 1 < mdat.Vtts.Count && timeSegmentSize >= payloadSize + mdat.Vtts[payloadIndex + 1].PayloadSize)
                    {
                        payloadIndex++;
                        payload = mdat.Vtts[payloadIndex].Payload;
                        style = mdat.Vtts[payloadIndex].Style;

                        if (timeSegment.Duration.HasValue && payload != null)
                        {
                            AddVttParagraph(track, track.LegacyTimeTotalMs, payload, before, style);
                        }

                        payloadSize += mdat.Vtts[payloadIndex].PayloadSize; // add 8
                    }
                }

                payloadIndex++;
            }
        }

        private static void AddVttParagraph(FragmentedTextTrack track, double timeTotalMs, string payload, double before, string style)
        {
            var p = new Paragraph(payload, before, timeTotalMs);
            var positionInfo = WebVTT.GetPositionInfo(style);
            if (!string.IsNullOrEmpty(positionInfo))
            {
                p.Text = positionInfo + p.Text;
                p.Extra = style;
                track.Subtitle.Header = "WEBVTT";
            }

            track.Subtitle.Paragraphs.Add(p);
        }

        private static void ReadVttWithoutSize(FragmentedTextTrack track, List<Vttc.VttData> vtts, List<TimeSegment> trunSamples, double timeScale)
        {
            if (vtts == null || trunSamples.Count <= 0 || trunSamples.Count < vtts.Count)
            {
                return;
            }

            var sampleIdx = 0;
            foreach (var vtt in vtts)
            {
                var presentation = trunSamples[sampleIdx];
                if (presentation.Duration.HasValue)
                {
                    var before = track.LegacyTimeTotalMs;
                    track.LegacyTimeTotalMs += presentation.Duration.Value / timeScale * 1000.0;
                    sampleIdx++;
                    if (vtt.Payload != null)
                    {
                        track.Subtitle.Paragraphs.Add(new Paragraph(vtt.Payload, before, track.LegacyTimeTotalMs));
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
                            fs.ReadFully(buffer, 0, readLength);
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
