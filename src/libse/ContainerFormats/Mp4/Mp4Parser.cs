using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
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

        /// <summary>
        /// CEA-608/708 closed captions from the video track: paragraphs per track key
        /// (1-4 = CC1-CC4, 100 + n = CEA-708 service n, see <see cref="ClosedCaptionDecoder"/>).
        /// Empty for a progressive file that has a subtitle track - its video is not scanned.
        /// </summary>
        public SortedDictionary<int, List<Paragraph>> ClosedCaptionTracks { get; private set; } = new SortedDictionary<int, List<Paragraph>>();

        public Subtitle TrunCea608Subtitle { get; private set; }
        public Subtitle TrunCea708Subtitle { get; private set; }
        private List<Cea608.CcData> _trunCcData = new List<Cea608.CcData>();
        public string DebugInfo { get; private set; }

        public List<Trak> GetSubtitleTracks()
        {
            var list = new List<Trak>();
            if (Moov?.Tracks == null)
            {
                return list;
            }

            // A QuickTime chapter track is a plain text track, so without this it would be offered
            // as a subtitle track - the chapter titles, listed as if they were dialogue.
            var chapterTrackIds = GetChapterTrackIds();

            foreach (var trak in Moov.Tracks)
            {
                if (trak.Tkhd != null && chapterTrackIds.Contains(trak.Tkhd.TrackId))
                {
                    continue;
                }

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

        /// <summary>
        /// Chapters from the two ways MP4 stores them: the Nero "chpl" box, and a QuickTime chapter
        /// track that a video track points at with a "chap" track reference. Files written by ffmpeg
        /// usually contain both, so "chpl" is preferred and the chapter track is only a fallback.
        /// </summary>
        public List<Chapter> GetChapters()
        {
            if (Moov?.Chpl != null && Moov.Chpl.Chapters.Count > 0)
            {
                return Moov.Chpl.Chapters.OrderBy(p => p.StartMilliseconds).ToList();
            }

            return GetChapterTrackChapters();
        }

        private List<Chapter> GetChapterTrackChapters()
        {
            var chapters = new List<Chapter>();
            if (Moov?.Tracks == null)
            {
                return chapters;
            }

            var chapterTrackIds = Moov.Tracks
                .Where(t => t.Tref != null)
                .SelectMany(t => t.Tref.ChapterTrackIds)
                .ToList();

            if (chapterTrackIds.Count == 0)
            {
                return chapters;
            }

            foreach (var trak in Moov.Tracks)
            {
                if (trak.Tkhd == null || !chapterTrackIds.Contains(trak.Tkhd.TrackId))
                {
                    continue;
                }

                var paragraphs = trak.Mdia?.Minf?.Stbl?.GetParagraphs();
                if (paragraphs == null)
                {
                    continue;
                }

                foreach (var p in paragraphs)
                {
                    chapters.Add(new Chapter(p.StartTime.TotalMilliseconds, p.Text));
                }
            }

            return chapters.OrderBy(p => p.StartMilliseconds).ToList();
        }

        /// <summary>
        /// Track ids a "chap" reference points at. Those tracks carry chapter titles, not subtitles,
        /// so callers listing subtitle tracks can leave them out.
        /// </summary>
        public HashSet<uint> GetChapterTrackIds()
        {
            var ids = new HashSet<uint>();
            if (Moov?.Tracks == null)
            {
                return ids;
            }

            foreach (var trak in Moov.Tracks)
            {
                if (trak.Tref == null)
                {
                    continue;
                }

                foreach (var id in trak.Tref.ChapterTrackIds)
                {
                    ids.Add(id);
                }
            }

            return ids;
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

            ApplyEditListsToMoovSubtitleTracks();

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

                // The moov track header still carries the edit list for a fragmented track
                ShiftParagraphs(merged.Paragraphs, GetEditListOffsetMs(FindTrack(fragmentedTrack.TrackId)));
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
            CheckForClcpCea708();

            // Finding CEA-608/708 in a progressive file reads every video sample - seconds on a
            // multi-GB movie - and callers only offer the captions when there is no subtitle
            // track, so skip the scan when there is one.
            if (GetSubtitleTracks().Count == 0)
            {
                CheckForMoovVideoCea608();
            }
        }

        private void ApplyEditListsToMoovSubtitleTracks()
        {
            if (Moov?.Tracks == null)
            {
                return;
            }

            foreach (var trak in Moov.Tracks)
            {
                var mdia = trak?.Mdia;
                if (mdia == null || !(mdia.IsTextSubtitle || mdia.IsVobSubSubtitle || mdia.IsClosedCaption))
                {
                    continue;
                }

                var paragraphs = mdia.Minf?.Stbl?.Paragraphs;
                if (paragraphs == null || paragraphs.Count == 0)
                {
                    continue;
                }

                // A VobSub track's paragraphs are index-paired with its sub pictures, so
                // dropping one there would misalign every bitmap after it.
                ShiftParagraphs(paragraphs, GetEditListOffsetMs(trak), dropBeforeZero: !mdia.IsVobSubSubtitle);
            }
        }

        /// <summary>
        /// Offset in milliseconds that the track's edit list (elst) puts between the media
        /// timeline the samples are timed on and the presentation timeline the player shows.
        /// Leading empty edits (media time -1) delay the track; a media start time on the
        /// first real edit moves it earlier. Anything more elaborate than that cannot be
        /// expressed as a single offset, so only the leading edits are honoured.
        /// </summary>
        private double GetEditListOffsetMs(Trak trak)
        {
            var entries = trak?.Edts?.Elst?.Entries;
            if (entries == null || entries.Count == 0)
            {
                return 0;
            }

            var movieTimeScale = Moov?.Mvhd?.TimeScale > 0 ? Moov.Mvhd.TimeScale : 1000UL;
            var mediaTimeScale = trak.Mdia?.Mdhd?.TimeScale > 0 ? trak.Mdia.Mdhd.TimeScale : movieTimeScale;

            var offsetMs = 0.0;
            var index = 0;
            while (index < entries.Count && entries[index].MediaTime < 0)
            {
                offsetMs += entries[index].SegmentDuration / (double)movieTimeScale * 1000.0;
                index++;
            }

            if (index < entries.Count)
            {
                offsetMs -= entries[index].MediaTime / (double)mediaTimeScale * 1000.0;
            }

            return offsetMs;
        }

        /// <summary>
        /// Moves paragraphs onto the presentation timeline. Anything the edit list pushes
        /// before zero is not presented, so such cues are dropped (or clipped when they
        /// straddle zero).
        /// </summary>
        private static void ShiftParagraphs(List<Paragraph> paragraphs, double offsetMs, bool dropBeforeZero = true)
        {
            if (paragraphs == null || Math.Abs(offsetMs) < 0.001)
            {
                return;
            }

            for (var i = paragraphs.Count - 1; i >= 0; i--)
            {
                var p = paragraphs[i];
                var start = p.StartTime.TotalMilliseconds + offsetMs;
                var end = p.EndTime.TotalMilliseconds + offsetMs;
                if (end <= 0 && dropBeforeZero)
                {
                    paragraphs.RemoveAt(i);
                    continue;
                }

                p.StartTime.TotalMilliseconds = start < 0 ? 0 : start;
                p.EndTime.TotalMilliseconds = end < 0 ? 0 : end;
            }
        }

        /// <summary>
        /// A QuickTime "c708" closed caption track carries both CEA-608 channels and CEA-708
        /// services; they are decoded into <see cref="ClosedCaptionTracks"/> like the captions
        /// of a video stream (the video scan then has nothing to add).
        /// </summary>
        private void CheckForClcpCea708()
        {
            if (Moov?.Tracks == null || TrunCea608Subtitle?.Paragraphs.Count > 0 || TrunCea708Subtitle?.Paragraphs.Count > 0)
            {
                return;
            }

            foreach (var trak in Moov.Tracks)
            {
                var stbl = trak?.Mdia?.Minf?.Stbl;
                if (trak?.Mdia?.IsClosedCaption == true && stbl?.C708CcData.Count > 0)
                {
                    var timeScale = stbl.TimeScale > 0 ? stbl.TimeScale : (Moov.Mvhd?.TimeScale ?? 1000UL);
                    DecodeCcData(stbl.C708CcData, timeScale, trak);
                    return;
                }
            }
        }

        private void CheckForMoovVideoCea608()
        {
            try
            {
                if (TrunCea608Subtitle?.Paragraphs.Count > 0 || TrunCea708Subtitle?.Paragraphs.Count > 0)
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
                var isHevc = stbl.Stsd?.IsHevc == true;
                var nalLengthSize = stbl.Stsd?.GetNalLengthSize() ?? 4;
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
                                var ccData = GetCcDataHelper.GetCcData(fs, chunkOffset, scanSize, isHevc, nalLengthSize);
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

                DecodeCcData(ccDataList, timeScale, videoTracks[0]);

                //debugInfo.AppendLine($"CheckForMoovVideoCea608: paragraphs={TrunCea608Subtitle?.Paragraphs.Count ?? 0}, cea708 paragraphs={TrunCea708Subtitle?.Paragraphs.Count ?? 0}");
            }
            catch (Exception e)
            {
                SeLogger.Error(e, "Error while parsing MP4 moov video track CEA-608");
            }
        }

        /// <summary>
        /// Decodes cc_data (timestamped in video track ticks) into ClosedCaptionTracks,
        /// TrunCea608Subtitle and TrunCea708Subtitle - shifted by the video track's edit list, like
        /// the samples of the video they are embedded in.
        /// </summary>
        private void DecodeCcData(List<CcData> ccDataList, double timeScale, Trak videoTrak)
        {
            if (ccDataList.Count == 0)
            {
                return;
            }

            try
            {
                // one frame per timestamp; the decoder puts frames in presentation order itself
                var decoder = new ClosedCaptionDecoder();
                var frame = new List<CcData>();
                var frameTime = ccDataList[0].Time;
                foreach (var cc in ccDataList)
                {
                    if (cc.Time != frameTime)
                    {
                        decoder.AddFrame((long)Math.Round(frameTime / timeScale * 1000.0), frame.ToArray());
                        frame.Clear();
                        frameTime = cc.Time;
                    }

                    frame.Add(cc);
                }

                decoder.AddFrame((long)Math.Round(frameTime / timeScale * 1000.0), frame.ToArray());
                ClosedCaptionTracks = decoder.Finish(0);
                var editListOffsetMs = GetEditListOffsetMs(videoTrak);
                if (editListOffsetMs != 0)
                {
                    foreach (var key in ClosedCaptionTracks.Keys.ToList())
                    {
                        ShiftParagraphs(ClosedCaptionTracks[key], editListOffsetMs);
                        if (ClosedCaptionTracks[key].Count == 0)
                        {
                            ClosedCaptionTracks.Remove(key);
                        }
                    }
                }

                // CC1 (else the first CEA-608 channel) and CEA-708 service 1 (else the first service)
                var cea608 = ClosedCaptionTracks.Where(p => p.Key < ClosedCaptionDecoder.Cea708TrackKeyOffset).Select(p => p.Value).FirstOrDefault();
                var cea708 = ClosedCaptionTracks.Where(p => p.Key > ClosedCaptionDecoder.Cea708TrackKeyOffset).Select(p => p.Value).FirstOrDefault();
                TrunCea608Subtitle = cea608 != null ? new Subtitle(cea608) : null;
                TrunCea708Subtitle = cea708 != null ? new Subtitle(cea708) : null;
            }
            catch (Exception e)
            {
                SeLogger.Error(e, "Error while parsing MP4 video track closed captions");
            }
        }

        private const double MissingMoovVideoTimeScale = 90000.0;

        private void CheckForTrunCea608()
        {
            try
            {
                // Fragment ticks are media-track times, so prefer the video track's mdhd
                // timescale; the movie (mvhd) timescale is only a fallback. A bare media
                // segment without its init segment (no moov) has neither, so assume the
                // 90 kHz MPEG clock that DASH/HLS video uses - 1000 made every time ~90x too large.
                double timeScale = Moov == null ? MissingMoovVideoTimeScale : Moov.Mvhd?.TimeScale ?? 1000.0;
                var videoTrack = GetVideoTracks().FirstOrDefault();
                if (videoTrack?.Mdia?.Mdhd?.TimeScale > 0)
                {
                    timeScale = videoTrack.Mdia.Mdhd.TimeScale;
                }

                DecodeCcData(_trunCcData, timeScale, videoTrack);
                _trunCcData.Clear();
            }
            catch (Exception e)
            {
                SeLogger.Error(e, "Error while parsing MP4 TRUN CEA 608");
            }
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

                var stsd = trak?.Mdia?.Minf?.Stbl?.Stsd;
                var isHevc = stsd?.IsHevc == true;
                var nalLengthSize = stsd?.GetNalLengthSize() ?? 4;
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
                            // A frame carries several cc_data triplets - the CEA-608 pairs of both
                            // fields and up to ~30 CEA-708 packet bytes - all at the frame's
                            // presentation time (decode time + composition offset).
                            var ccData = GetCcDataHelper.GetCcData(fs, startPosition, sample.Size.Value, isHevc, nalLengthSize);
                            var pts = (long)dts + (sample.TimeOffset ?? 0);
                            foreach (var cc in ccData)
                            {
                                cc.Time = (ulong)Math.Max(0, pts);
                                _trunCcData.Add(cc);
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

                    var startMs = startTicks * 1000.0 / timeScale;
                    var durationMs = durationTicks * 1000.0 / timeScale;

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
