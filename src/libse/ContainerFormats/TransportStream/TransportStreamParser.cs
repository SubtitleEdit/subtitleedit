using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// MPEG transport stream parser
    /// </summary>
    public class TransportStreamParser
    {
        public delegate void LoadTransportStreamCallback(long position, long total);

        public int NumberOfNullPackets { get; private set; }
        public long TotalNumberOfPackets { get; private set; }
        public long TotalNumberOfPrivateStream1 { get; private set; }
        public List<int> SubtitlePacketIds { get; private set; }
        private HashSet<int> _subtitlePacketIdsLookup;
        public SortedDictionary<int, SortedDictionary<int, List<Paragraph>>> TeletextSubtitlesLookup { get; set; } // teletext
        public SortedDictionary<int, SortedDictionary<int, List<Paragraph>>> AribSubtitlesLookup { get; set; } // ARIB STD-B24 captions: pid -> language index -> paragraphs
        public Dictionary<int, Dictionary<int, string>> AribLanguageLookup { get; set; } // pid -> language index -> ISO 639-2 code
        private Dictionary<int, List<DvbSubPes>> _aribPesLookup;
        public SortedDictionary<int, SortedDictionary<int, List<Paragraph>>> ClosedCaptionSubtitlesLookup { get; set; } // CEA-608/708 from the video stream: video pid -> track key (see ClosedCaptionExtractor) -> paragraphs
        private Dictionary<int, ClosedCaptionExtractor> _closedCaptionExtractors;
        private HashSet<int> _nonVideoPacketIds;
        private ProgramMapTableParser _programMapTableParser;
        private Dictionary<int, int> _streamTypes;
        private Dictionary<int, ulong> _firstVideoPtsByPid; // video pid -> first PTS (90 kHz)
        private ulong? _firstVideoPts; // first video PTS in the file, any program
        private Dictionary<int, ulong> _firstAudioPtsByPid; // audio pid -> first PTS (90 kHz)
        private Dictionary<int, ulong> _lastPcrByPid; // PCR pid -> last PCR base (90 kHz)
        private ulong? _lastPcr;
        private Dictionary<int, bool> _pgsPacketIds; // pid -> carries Blu-ray PGS (HDMV presentation graphics)

        /// <summary>
        /// Packets of the PES packet being received, per subtitle PID. Kept apart per PID - one
        /// shared list had to be scanned for every PES start, and with a subtitle PID that sends
        /// thousands of packets between starts alongside private stream 1 audio (AC-3), that was
        /// quadratic: 25 s for a 3 GB film.
        /// </summary>
        private Dictionary<int, List<Packet>> _pendingPackets;
        private SortedDictionary<int, List<DvbSubPes>> SubtitlesLookup { get; set; }
        private SortedDictionary<int, List<TransportStreamSubtitle>> DvbSubtitlesLookup { get; set; } // images
        private bool _isM2TransportStream;
        private bool _isRs204TransportStream;

        /// <summary>
        /// How far a subtitle may start before the first video frame and still be treated as the
        /// same timeline (shown at zero) rather than as a stream whose subtitle timestamps belong
        /// to a different epoch. Anything under a second is muxer jitter - it must not trigger the
        /// rebase in the DVB timing loop, because that computes a replacement offset from the
        /// previous subtitle and keeps it for every cue that follows.
        /// </summary>
        private const long MaxSubtitleLeadOverVideoMs = 1000;

        /// <summary>
        /// PES time stamps are 33-bit counters of a 90 kHz clock, they wrap every ~26.5 hours.
        /// </summary>
        private const ulong TimestampWrap = 1UL << 33;

        /// <summary>HDMV presentation graphics (Blu-ray PGS) stream_type in the PMT.</summary>
        private const int StreamTypePgs = 0x90;

        public void Parse(string fileName, LoadTransportStreamCallback callback)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Parse(fs, callback);
            }
        }

        /// <summary>
        /// Can be used with e.g. MemoryStream or FileStream
        /// </summary>
        /// <param name="ms">Input stream</param>
        /// <param name="callback">Optional callback event to follow progress</param>
        public void Parse(Stream ms, LoadTransportStreamCallback callback)
        {
            _isM2TransportStream = false;
            _isRs204TransportStream = false;
            NumberOfNullPackets = 0;
            TotalNumberOfPackets = 0;
            TotalNumberOfPrivateStream1 = 0;
            SubtitlePacketIds = new List<int>();
            _subtitlePacketIdsLookup = new HashSet<int>();
            _pendingPackets = new Dictionary<int, List<Packet>>();
            ms.Position = 0;
            const int packetLength = 188;
            _isM2TransportStream = IsM2TransportStream(ms);
            _isRs204TransportStream = !_isM2TransportStream && IsRs204TransportStream(ms);

            // Reed-Solomon parity trailing each packet; of no use once the stream is on disk
            const int rs204ParityLength = 16;
            var packetBuffer = new byte[packetLength];
            var m2TsTimeCodeBuffer = new byte[4];
            long position = 0;
            SubtitlesLookup = new SortedDictionary<int, List<DvbSubPes>>();
            TeletextSubtitlesLookup = new SortedDictionary<int, SortedDictionary<int, List<Paragraph>>>();
            AribSubtitlesLookup = new SortedDictionary<int, SortedDictionary<int, List<Paragraph>>>();
            AribLanguageLookup = new Dictionary<int, Dictionary<int, string>>();
            _aribPesLookup = new Dictionary<int, List<DvbSubPes>>();
            ClosedCaptionSubtitlesLookup = new SortedDictionary<int, SortedDictionary<int, List<Paragraph>>>();
            _closedCaptionExtractors = new Dictionary<int, ClosedCaptionExtractor>();
            _nonVideoPacketIds = new HashSet<int>();
            _firstVideoPtsByPid = new Dictionary<int, ulong>();
            _firstAudioPtsByPid = new Dictionary<int, ulong>();
            _lastPcrByPid = new Dictionary<int, ulong>();
            _lastPcr = null;
            _firstVideoPts = null;
            _pgsPacketIds = new Dictionary<int, bool>();
            var teletextPesList = new Dictionary<int, List<DvbSubPes>>();
            var teletextPages = new Dictionary<int, List<int>>();
            ulong? firstMs = null;

            // stream types (video codec for closed captions) and ARIB data component ids
            _programMapTableParser = new ProgramMapTableParser();
            _programMapTableParser.Parse(ms);
            var streamTypes = _programMapTableParser.GetStreamTypes(); // partial results are fine
            _streamTypes = streamTypes;
            var pcrPidBySubtitlePid = new Dictionary<int, int?>();

            // check for Topfield .rec file
            ms.Seek(position, SeekOrigin.Begin);
            ms.ReadFully(m2TsTimeCodeBuffer, 0, 3);
            var topfieldCheck = m2TsTimeCodeBuffer.AsSpan(0, 3);
            if (topfieldCheck[0] == 0x54 && topfieldCheck[1] == 0x46 && topfieldCheck[2] == 0x72)
            {
                position = 3760;
            }

            long transportStreamLength = ms.Length;
            ms.Seek(position, SeekOrigin.Begin);
            while (position < transportStreamLength)
            {
                if (_isM2TransportStream)
                {
                    ms.ReadFully(m2TsTimeCodeBuffer, 0, m2TsTimeCodeBuffer.Length);
                    position += m2TsTimeCodeBuffer.Length;
                }

                var bytesRead = ms.Read(packetBuffer, 0, packetLength);
                if (bytesRead < packetLength)
                {
                    break; // incomplete packet at end-of-file
                }

                if (packetBuffer[0] == Packet.SynchronizationByte)
                {
                    // Constructing a Packet always copies the payload into a fresh byte[], and
                    // video/audio content packets - the vast majority of a real file - are of no
                    // interest beyond their first PTS and the PCR, so those are peeked from the raw
                    // buffer and only subtitle packets are materialized.
                    var packetId = Packet.PeekPacketId(packetBuffer);
                    AddClosedCaptionPacket(packetId, packetBuffer, streamTypes);
                    if (packetId == Packet.NullPacketId)
                    {
                        NumberOfNullPackets++;
                    }
                    else
                    {
                        if (Packet.TryPeekProgramClockReference(packetBuffer, out var programClockReference))
                        {
                            _lastPcrByPid[packetId] = programClockReference;
                            _lastPcr = programClockReference;
                        }

                        // the PMT's stream type first - VC-1 video and Blu-ray audio use stream ids
                        // outside the MPEG ranges - else the stream id (and sync word for AC-3/DTS)
                        var streamKind = GetStreamKind(packetId);
                        if (streamKind != StreamKind.Video && !_firstAudioPtsByPid.ContainsKey(packetId) &&
                            (streamKind == StreamKind.Audio
                                ? Packet.TryPeekPresentationTimestamp(packetBuffer, out var audioPts)
                                : Packet.TryPeekAudioPresentationTimestamp(packetBuffer, out audioPts)))
                        {
                            _firstAudioPtsByPid.Add(packetId, audioPts);
                        }

                        if (streamKind != StreamKind.Audio && !_firstVideoPtsByPid.ContainsKey(packetId) &&
                            (streamKind == StreamKind.Video
                                ? Packet.TryPeekPresentationTimestamp(packetBuffer, out var videoPts)
                                : Packet.TryPeekVideoPresentationTimestamp(packetBuffer, out videoPts)))
                        {
                            // every program of a multi-program stream has its own clock
                            _firstVideoPtsByPid.Add(packetId, videoPts);
                            if (!_firstVideoPts.HasValue)
                            {
                                _firstVideoPts = videoPts;
                            }
                        }
                        else if (_subtitlePacketIdsLookup.Contains(packetId) || Packet.PeekIsPrivateStream1(packetBuffer))
                        {
                            var packet = new Packet(packetBuffer);
                            if (packet.IsPrivateStream1 || _subtitlePacketIdsLookup.Contains(packet.PacketId))
                            {
                                packet.ArrivalTimestamp = GetArrivalTimestamp(packetId, pcrPidBySubtitlePid);
                                AddSubtitlePacket(packet, teletextPages, teletextPesList, ref firstMs);
                            }
                        }
                    }
                    TotalNumberOfPackets++;
                    position += packetLength;

                    if (_isRs204TransportStream)
                    {
                        position += rs204ParityLength;
                        ms.Seek(position, SeekOrigin.Begin);
                    }

                    if (TotalNumberOfPackets % 100000 == 0)
                    {
                        callback?.Invoke(position, transportStreamLength);
                    }
                }
                else
                {
                    // sync byte not found - search for it (will be very slow!)
                    if (_isM2TransportStream)
                    {
                        position -= m2TsTimeCodeBuffer.Length;
                    }
                    position++;
                    ms.Seek(position, SeekOrigin.Begin);
                }
            }
            foreach (var pending in _pendingPackets)
            {
                if (pending.Value.Count > 0)
                {
                    firstMs = ProcessPackages(pending.Key, pending.Value, teletextPages, teletextPesList, firstMs);
                }
            }
            _pendingPackets.Clear();
            callback?.Invoke(transportStreamLength, transportStreamLength);

            // time code zero: where the file starts, as players and ffmpeg see it
            var fileStartMs = GetFileStartPts() / 90;

            foreach (var packetId in teletextPesList.Keys) // teletext from PES packets
            {
                if (!teletextPages.ContainsKey(packetId))
                {
                    continue;
                }

                // Teletext sent without PTS is timed by the program clock - relative to the
                // program's video like the DVB subtitles, as the clock differs from the PTS of the
                // first subtitle packet that is used otherwise.
                var videoPts = GetProgramStartPts(packetId);
                var hasEstimatedTimestamps = videoPts.HasValue && teletextPesList[packetId].Any(p => p.HasEstimatedTimestamp);
                if (hasEstimatedTimestamps)
                {
                    UnwrapTimestamps(teletextPesList[packetId], videoPts);
                }

                foreach (var page in teletextPages[packetId].OrderBy(p => p))
                {
                    var pageBcd = Teletext.DecToBec(page);
                    Teletext.InitializeStaticFields(packetId, pageBcd);
                    var teletextRunSettings = hasEstimatedTimestamps
                        ? new TeletextRunSettings(videoPts.Value / 90, alwaysSubtractStartMs: true)
                        : new TeletextRunSettings(EarliestMs(videoPts / 90, firstMs));
                    foreach (var pes in teletextPesList[packetId])
                    {
                        var textDictionary = pes.GetTeletext(teletextRunSettings, page, pageBcd);
                        AddToTeletextDictionary(textDictionary, packetId);
                    }

                    var lastTextDictionary = Teletext.ProcessTelxPacketPendingLeftovers(teletextRunSettings, page);
                    AddToTeletextDictionary(lastTextDictionary, packetId);
                }
            }
            if (Configuration.Settings.SubtitleSettings.TeletextItalicFix)
            {
                FixTeletextItalics(TeletextSubtitlesLookup);
            }

            foreach (var id in TeletextSubtitlesLookup.Keys)
            {
                SubtitlePacketIds.Remove(id);
                _subtitlePacketIdsLookup.Remove(id);
                SubtitlesLookup.Remove(id);
            }

            ParseAribCaptions(fileStartMs ?? firstMs);
            FinishClosedCaptions(fileStartMs ?? firstMs);

            DvbSubtitlesLookup = new SortedDictionary<int, List<TransportStreamSubtitle>>();
            var sb = new StringBuilder();
            foreach (int pid in SubtitlesLookup.Keys)
            {
                // Blu-ray images from PES packets - in .m2ts, and PGS remuxed into a plain .ts
                if (!_isM2TransportStream && !IsPgsPacketId(pid))
                {
                    continue;
                }

                var bdMs = new MemoryStream();
                var list = SubtitlesLookup[pid];
                var videoPts = GetProgramStartPts(pid);
                UnwrapTimestamps(list, videoPts);
                var currentList = new List<DvbSubPes>();
                sb.Clear();
                var subList = new List<TransportStreamSubtitle>();
                var offset = GetVideoOffsetMs(videoPts, fileStartMs);
                var lastPalettes = new Dictionary<int, List<PaletteInfo>>();
                var lastBitmapObjects = new Dictionary<int, List<BluRaySupParser.OdsData>>();
                for (var index = 0; index < list.Count; index++)
                {
                    var item = list[index];
                    item.WriteToStream(bdMs);
                    currentList.Add(item);
                    if (item.DataIdentifier == 0x80)
                    {
                        bdMs.Position = 0;
                        var bdList = BluRaySupParser.ParseBluRaySup(bdMs, sb, true, lastPalettes, lastBitmapObjects);
                        if (bdList.Count > 0)
                        {
                            var startMs = currentList.First().PresentationTimestampToMilliseconds();
                            var endMs = index + 1 < list.Count ? list[index + 1].PresentationTimestampToMilliseconds() : startMs + (ulong)Configuration.Settings.General.NewEmptyDefaultMs;
                            subList.Add(new TransportStreamSubtitle(bdList[0], ToVideoTime(startMs, offset), ToVideoTime(endMs, offset)));
                        }
                        bdMs.Dispose();
                        bdMs = new MemoryStream();
                        currentList.Clear();
                    }
                    else if (bdMs.Length > 2_000_000_000) // Avoid crashing on very large files
                    {
                        bdMs.Dispose();
                        bdMs = new MemoryStream();
                        currentList.Clear();
                    }
                }

                if (currentList.Count > 0)
                {
                    // the recording was cut before the last display set's END segment
                    bdMs.Position = 0;
                    var bdList = BluRaySupParser.ParseBluRaySup(bdMs, sb, true, lastPalettes, lastBitmapObjects);
                    if (bdList.Count > 0)
                    {
                        var startMs = currentList.First().PresentationTimestampToMilliseconds();
                        var endMs = startMs + (ulong)Configuration.Settings.General.NewEmptyDefaultMs;
                        subList.Add(new TransportStreamSubtitle(bdList[0], ToVideoTime(startMs, offset), ToVideoTime(endMs, offset)));
                    }
                }

                bdMs.Dispose();

                if (subList.Count > 0)
                {
                    DvbSubtitlesLookup.Add(pid, subList);
                    SubtitlePacketIds.Remove(pid);
                    _subtitlePacketIdsLookup.Remove(pid);
                }
            }

            SubtitlePacketIds.Clear();
            _subtitlePacketIdsLookup.Clear();
            SubtitlePacketIds.AddRange(SubtitlesLookup.Keys);
            foreach (var key in SubtitlesLookup.Keys)
            {
                _subtitlePacketIdsLookup.Add(key);
            }
            SubtitlePacketIds.Sort();

            // Merge packets and set start/end time
            foreach (int pid in SubtitlePacketIds.Where(p => !DvbSubtitlesLookup.ContainsKey(p) && !IsPgsPacketId(p)))
            {
                var subtitles = new List<TransportStreamSubtitle>();
                var list = ParseAndRemoveEmpty(GetSubtitlePesPackets(pid));
                var videoPts = GetProgramStartPts(pid);
                UnwrapTimestamps(list, videoPts);
                var offset = GetVideoOffsetMs(videoPts, fileStartMs);
                ComposeNormalCasePageUpdates(list);
                for (int i = 0; i < list.Count; i++)
                {
                    var pes = list[i];
                    pes.ParseSegments();
                    if (pes.ObjectDataList.Count > 0)
                    {
                        var sub = new TransportStreamSubtitle { StartMilliseconds = pes.PresentationTimestampToMilliseconds(), Pes = pes };
                        var endFound = false;
                        var ndx = i + 1;
                        while (ndx < list.Count)
                        {
                            var entry = list[ndx]; ndx++;
                            if (entry.SubtitleSegments.Count == 0)
                            {
                                continue;
                            }

                            // The next display set ends this one. It is normally closed by an end of
                            // display set segment, but older encoders send none - a page composition
                            // (e.g. the empty one that clears the screen) starts a new display set
                            // too. Anything else (stuffing, a lone CLUT) is skipped.
                            if (!entry.SubtitleSegments.Any(p => p.SegmentType == SubtitleSegment.EndOfDisplaySetSegment) &&
                                entry.PageCompositions.Count == 0)
                            {
                                continue;
                            }

                            sub.EndMilliseconds = entry.PresentationTimestampToMilliseconds();
                            endFound = true;
                            break;
                        }

                        // page_time_out: the page is erased when it is not updated within this time
                        var pageTimeOutMs = pes.PageCompositions.Count > 0 ? (ulong)pes.PageCompositions[0].PageTimeOut * 1000UL : 0UL;
                        if (pageTimeOutMs > 0 && (!endFound || sub.EndMilliseconds > sub.StartMilliseconds + pageTimeOutMs))
                        {
                            sub.EndMilliseconds = sub.StartMilliseconds + pageTimeOutMs;
                        }

                        if (sub.EndMilliseconds < sub.StartMilliseconds || sub.EndMilliseconds - sub.StartMilliseconds > (ulong)Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            sub.EndMilliseconds = sub.StartMilliseconds + (ulong)Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
                        }

                        if (offset <= (long)sub.StartMilliseconds || offset < 0)
                        {
                            sub.StartMilliseconds = (ulong)((long)sub.StartMilliseconds - offset);
                            sub.EndMilliseconds = (ulong)((long)sub.EndMilliseconds - offset);
                        }
                        else if (offset - (long)sub.StartMilliseconds <= MaxSubtitleLeadOverVideoMs)
                        {
                            // Starts a hair before the first video frame - ffmpeg for one puts the
                            // opening DVB page a millisecond ahead of the first picture. Show it at
                            // zero and leave the stream's offset alone; the rebase below must not be
                            // reached by that kind of jitter.
                            var end = (long)sub.EndMilliseconds - offset;
                            sub.StartMilliseconds = 0;
                            sub.EndMilliseconds = end > 0 ? (ulong)end : 0;
                        }
                        else if (subtitles.Count > 0)
                        {
                            // A time stamp discontinuity: continue one second after the previous
                            // subtitle, and keep that offset for the ones that follow.
                            var duration = sub.EndMilliseconds - sub.StartMilliseconds;
                            offset = (long)sub.StartMilliseconds - ((long)subtitles[subtitles.Count - 1].EndMilliseconds + 1000);
                            sub.StartMilliseconds = (ulong)((long)sub.StartMilliseconds - offset);
                            sub.EndMilliseconds = sub.StartMilliseconds + duration;
                        }

                        subtitles.Add(sub);
                    }
                }
                if (subtitles.Count > 0)
                {
                    DvbSubtitlesLookup.Add(pid, subtitles);
                }
            }

            SubtitlePacketIds.Clear();
            _subtitlePacketIdsLookup.Clear();
            foreach (var key in DvbSubtitlesLookup.Keys)
            {
                if (DvbSubtitlesLookup[key].Count > 0)
                {
                    SubtitlePacketIds.Add(key);
                    _subtitlePacketIdsLookup.Add(key);
                }
            }
            SubtitlePacketIds.Sort();
        }

        /// <summary>
        /// Decode collected ARIB STD-B24 caption PES packets (ISDB broadcasts) into text paragraphs
        /// </summary>
        private void ParseAribCaptions(ulong? firstMs)
        {
            if (_aribPesLookup.Count == 0)
            {
                return;
            }

            // the ARIB data_component_descriptor in the PMT tells profile A (full-seg, 0x0008)
            // and profile C (one-seg, 0x0012) caption streams apart
            var dataComponentIds = _programMapTableParser.GetAribDataComponentIds(); // partial results are fine

            foreach (var pid in _aribPesLookup.Keys)
            {
                var videoPts = GetProgramStartPts(pid);
                var offset = videoPts.HasValue ? (long)(videoPts.Value / 90) : (long)(firstMs ?? 0);
                var profile = AribB24Decoder.AribProfile.ProfileA;
                if (dataComponentIds.TryGetValue(pid, out var dataComponentId) && dataComponentId == 0x12)
                {
                    profile = AribB24Decoder.AribProfile.ProfileC;
                }

                var parser = new AribCaptionParser(profile);
                foreach (var pes in _aribPesLookup[pid])
                {
                    if (pes.PresentationTimestamp.HasValue)
                    {
                        parser.ParsePesPayload(pes.GetAribCaptionData(), pes.PresentationTimestampToMilliseconds());
                    }
                }
                parser.Flush();

                var languages = new SortedDictionary<int, List<Paragraph>>();
                foreach (var language in parser.ParagraphsByLanguage)
                {
                    foreach (var paragraph in language.Value)
                    {
                        if (offset <= paragraph.StartTime.TotalMilliseconds)
                        {
                            paragraph.StartTime.TotalMilliseconds -= offset;
                            paragraph.EndTime.TotalMilliseconds -= offset;
                        }
                    }

                    if (language.Value.Count > 0)
                    {
                        languages.Add(language.Key, language.Value);
                    }
                }

                if (languages.Count > 0)
                {
                    AribSubtitlesLookup.Add(pid, languages);
                    AribLanguageLookup.Add(pid, new Dictionary<int, string>(parser.LanguageCodes));
                }
            }

            _aribPesLookup.Clear();
        }

        /// <summary>
        /// Feeds packets of video PIDs to a CEA-608/708 extractor. A PID counts as video when its
        /// first PES packet has a video stream id; the codec comes from the PMT when it is known.
        /// </summary>
        private void AddClosedCaptionPacket(int packetId, byte[] packetBuffer, Dictionary<int, int> streamTypes)
        {
            if (_closedCaptionExtractors.TryGetValue(packetId, out var extractor))
            {
                extractor.AddPacket(packetBuffer);
                return;
            }

            if ((packetBuffer[1] & 0x40) == 0 || packetId == Packet.NullPacketId || _nonVideoPacketIds.Contains(packetId))
            {
                return;
            }

            if (!ClosedCaptionExtractor.IsVideoPesStart(packetBuffer))
            {
                _nonVideoPacketIds.Add(packetId);
                return;
            }

            var codec = Cea608.CcVideoCodec.Unknown;
            if (streamTypes.TryGetValue(packetId, out var streamType) && !ClosedCaptionExtractor.IsVideoStreamType(streamType, out codec))
            {
                _nonVideoPacketIds.Add(packetId); // e.g. a video codec without closed captions support
                return;
            }

            extractor = new ClosedCaptionExtractor(codec);
            _closedCaptionExtractors.Add(packetId, extractor);
            extractor.AddPacket(packetBuffer);
        }

        private void FinishClosedCaptions(ulong? firstMs)
        {
            foreach (var extractor in _closedCaptionExtractors)
            {
                try
                {
                    // the captions travel inside this video stream - its own clock is the reference
                    var startPts = GetProgramStartPts(extractor.Key);
                    var offset = startPts.HasValue ? (long)(startPts.Value / 90) : (long)(firstMs ?? 0);
                    var tracks = extractor.Value.Finish(offset);
                    if (tracks.Count > 0)
                    {
                        ClosedCaptionSubtitlesLookup.Add(extractor.Key, tracks);
                    }
                }
                catch (Exception e)
                {
                    SeLogger.Error(e, "Error while parsing transport stream closed captions");
                }
            }

            _closedCaptionExtractors.Clear();
        }

        /// <summary>
        /// Converts a starting '&lt;' char to italic style (can be preceded by a font tag)
        /// E.g. "&lt;Hi there." to "&lt;i&gt;Hi there.&lt;/i&gt;"
        /// </summary>
        private static void FixTeletextItalics(SortedDictionary<int, SortedDictionary<int, List<Paragraph>>> dictionary)
        {
            var sb = new StringBuilder();
            foreach (var dic in dictionary)
            {
                foreach (var inner in dic.Value)
                {
                    foreach (var p in inner.Value)
                    {
                        if (p.Text.IndexOf('<') < 0)
                        {
                            continue;
                        }

                        sb.Clear();
                        foreach (var line in p.Text.SplitToLines())
                        {
                            var s = line.Trim();
                            if (s.StartsWith("<font", StringComparison.Ordinal))
                            {
                                var fontRemoved = HtmlUtil.RemoveOpenCloseTags(s, HtmlUtil.TagFont);
                                if (!fontRemoved.StartsWith('<'))
                                {
                                    sb.AppendLine(s); // no italic, only font tag
                                    continue;
                                }

                                // italic and font tag
                                var indexOfEnd = s.IndexOf('>');
                                if (indexOfEnd > 0 && s.Length > indexOfEnd + 2 && s[indexOfEnd + 1] == '<' &&
                                    !s.Remove(0, indexOfEnd).StartsWith("<font", StringComparison.Ordinal))
                                {
                                    s = s.Remove(indexOfEnd + 1, 1);
                                    sb.Append("<i>").Append(s).AppendLine("</i>"); // italic + font tag
                                    continue;
                                }

                                sb.AppendLine(line.Trim()); // no italic, only font tag
                                continue;
                            }

                            if (s.StartsWith('<'))
                            {
                                sb.Append("<i>").Append(s.Remove(0, 1)).AppendLine("</i>");
                            }
                            else
                            {
                                sb.AppendLine(s);
                            }
                        }
                        p.Text = sb.ToString().Trim();
                    }
                }
            }
        }

        private void AddToTeletextDictionary(Dictionary<int, Paragraph> textDictionary, int packetId)
        {
            foreach (var dic in textDictionary)
            {
                if (!string.IsNullOrEmpty(dic.Value.Text))
                {
                    if (TeletextSubtitlesLookup.TryGetValue(packetId, out var innerDic))
                    {
                        if (innerDic.TryGetValue(dic.Key, out var paragraphs))
                        {
                            paragraphs.Add(dic.Value);
                        }
                        else
                        {
                            innerDic.Add(dic.Key, new List<Paragraph> { dic.Value });
                        }
                    }
                    else
                    {
                        TeletextSubtitlesLookup.Add(packetId, new SortedDictionary<int, List<Paragraph>> { { dic.Key, new List<Paragraph> { dic.Value } } });
                    }
                }
            }
        }

        /// <summary>Shared by both branches of the main packet loop that keep a subtitle packet.</summary>
        private void AddSubtitlePacket(Packet packet, Dictionary<int, List<int>> teletextPages, Dictionary<int, List<DvbSubPes>> teletextPesList, ref ulong? firstMs)
        {
            TotalNumberOfPrivateStream1++;

            if (_subtitlePacketIdsLookup.Add(packet.PacketId))
            {
                SubtitlePacketIds.Add(packet.PacketId);
            }

            var pending = GetOrAddList(_pendingPackets, packet.PacketId);
            if (packet.PayloadUnitStartIndicator)
            {
                if (pending.Count > 0)
                {
                    firstMs = ProcessPackages(packet.PacketId, pending, teletextPages, teletextPesList, firstMs);
                }

                pending.Clear();
            }

            pending.Add(packet);
        }

        private ulong? ProcessPackages(int packetId, List<Packet> packets, Dictionary<int, List<int>> teletextPages, Dictionary<int, List<DvbSubPes>> teletextPesList, ulong? firstVideoMs)
        {
            var list = MakeSubtitlePesPackets(packetId, packets);
            if (list.Count == 0)
            {
                return firstVideoMs;
            }

            if (!firstVideoMs.HasValue)
            {
                foreach (var pes in list)
                {
                    if (pes.PresentationTimestamp.HasValue)
                    {
                        firstVideoMs = pes.PresentationTimestampToMilliseconds();
                        break;
                    }
                }
            }

            if (_isM2TransportStream || list.Any(p => p.IsDvbSubPicture) || IsPgsPacketId(packetId, list))
            {
                if (SubtitlesLookup.TryGetValue(packetId, out var existing))
                {
                    existing.AddRange(list);
                }
                else
                {
                    SubtitlesLookup.Add(packetId, list);
                }
            }

            foreach (var item in list.Where(p => p.IsTeletext))
            {
                foreach (var pageNumber in item.PrepareTeletext().Where(p => p > 0))
                {
                    var pages = GetOrAddList(teletextPages, packetId);
                    if (!pages.Contains(pageNumber))
                    {
                        pages.Add(pageNumber);
                    }
                }

                GetOrAddList(teletextPesList, packetId).Add(item);
            }

            if (!_isM2TransportStream)
            {
                foreach (var item in list.Where(p => p.IsAribCaption))
                {
                    GetOrAddList(_aribPesLookup, packetId).Add(item);
                }
            }

            return firstVideoMs;
        }

        private static List<T> GetOrAddList<T>(Dictionary<int, List<T>> dictionary, int key)
        {
#if NET8_0_OR_GREATER
            ref var list = ref System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out _);
            return list ??= new List<T>();
#else
            if (!dictionary.TryGetValue(key, out var list))
            {
                list = new List<T>();
                dictionary.Add(key, list);
            }
            return list;
#endif
        }

        /// <summary>
        /// Start (90 kHz) of the program that carries <paramref name="packetId"/>: the earliest first
        /// PTS of its video and audio - libavformat's start time, which players (mpv, ffmpeg) and the
        /// waveform count from. Taking the video alone put every subtitle late by the audio's lead.
        /// Falls back to the start of the whole file when the PMT does not tell.
        /// </summary>
        private ulong? GetProgramStartPts(int packetId)
        {
            var videoPacketId = _programMapTableParser.GetProgramVideoPacketId(packetId);
            if (videoPacketId.HasValue)
            {
                ulong? start = _firstVideoPtsByPid.TryGetValue(videoPacketId.Value, out var videoPts) ? videoPts : (ulong?)null;
                foreach (var audio in _firstAudioPtsByPid)
                {
                    if (_programMapTableParser.GetProgramVideoPacketId(audio.Key) == videoPacketId)
                    {
                        start = Earliest(start, audio.Value);
                    }
                }

                if (start.HasValue)
                {
                    return start;
                }
            }

            return GetFileStartPts();
        }

        private enum StreamKind
        {
            Unknown,
            Video,
            Audio,
        }

        private StreamKind GetStreamKind(int packetId)
        {
            if (!_streamTypes.TryGetValue(packetId, out var streamType))
            {
                return StreamKind.Unknown;
            }

            if (ProgramMapTableParser.IsVideoStreamType(streamType))
            {
                return StreamKind.Video;
            }

            return ProgramMapTableParser.IsAudioStreamType(streamType) && !_subtitlePacketIdsLookup.Contains(packetId)
                ? StreamKind.Audio
                : StreamKind.Unknown;
        }

        /// <summary>Earliest first video or audio PTS (90 kHz) in the file, any program.</summary>
        private ulong? GetFileStartPts()
        {
            var start = _firstVideoPts;
            foreach (var audioPts in _firstAudioPtsByPid.Values)
            {
                start = Earliest(start, audioPts);
            }

            return start;
        }

        /// <summary>
        /// The earlier of two PTS values, across a 33-bit wrap: a value just after the wrap is later
        /// than one just before it.
        /// </summary>
        private static ulong? Earliest(ulong? a, ulong b)
        {
            if (!a.HasValue)
            {
                return b;
            }

            var bIsEarlier = ((a.Value - b) & (TimestampWrap - 1)) < TimestampWrap / 2 && a.Value != b;
            return bIsEarlier ? b : a;
        }

        private static ulong? EarliestMs(ulong? a, ulong? b)
        {
            if (!a.HasValue)
            {
                return b;
            }

            return b.HasValue && b.Value < a.Value ? b : a;
        }

        private static long GetVideoOffsetMs(ulong? firstVideoPts, ulong? fallbackMs)
        {
            if (firstVideoPts.HasValue)
            {
                return (long)(firstVideoPts.Value / 90);
            }

            return (long)(fallbackMs ?? 0);
        }

        /// <summary>
        /// Absolute time stamp to a time on the video's timeline; a hair before the first video
        /// frame is shown at zero, anything far outside it (a different epoch) is left as is.
        /// </summary>
        private static ulong ToVideoTime(ulong milliseconds, long offset)
        {
            if ((long)milliseconds >= offset)
            {
                return (ulong)((long)milliseconds - offset);
            }

            return offset - (long)milliseconds <= MaxSubtitleLeadOverVideoMs ? 0 : milliseconds;
        }

        /// <summary>
        /// PES time stamps are 33 bits and wrap after ~26.5 hours of stream clock (not stream
        /// length - broadcasters start the clock anywhere). A time stamp that drops by more than half
        /// the range is taken as a wrap, and it and everything after it is moved one range up.
        /// </summary>
        /// <param name="list">PES packets in stream order</param>
        /// <param name="referencePts">First video PTS of the program - a stream whose first
        /// subtitle comes after the wrap is unwrapped relative to it</param>
        private static void UnwrapTimestamps(List<DvbSubPes> list, ulong? referencePts)
        {
            ulong add = 0;
            var last = referencePts;
            foreach (var pes in list)
            {
                if (!pes.PresentationTimestamp.HasValue)
                {
                    continue;
                }

                var pts = pes.PresentationTimestamp.Value + add;
                if (last.HasValue && pts + TimestampWrap / 2 < last.Value)
                {
                    add += TimestampWrap;
                    pts += TimestampWrap;
                }

                pes.PresentationTimestamp = pts;
                last = pts;
            }
        }

        /// <summary>
        /// Program clock (PCR base, 90 kHz) of the program carrying <paramref name="packetId"/>,
        /// or of the last PCR in the file if the PMT does not tell.
        /// </summary>
        private ulong? GetArrivalTimestamp(int packetId, Dictionary<int, int?> pcrPidBySubtitlePid)
        {
            if (!pcrPidBySubtitlePid.TryGetValue(packetId, out var pcrPid))
            {
                pcrPid = _programMapTableParser.GetProgramClockReferencePacketId(packetId);
                pcrPidBySubtitlePid.Add(packetId, pcrPid);
            }

            if (pcrPid.HasValue && _lastPcrByPid.TryGetValue(pcrPid.Value, out var pcr))
            {
                return pcr;
            }

            return _lastPcr;
        }

        /// <summary>
        /// True if <paramref name="packetId"/> has been found to carry Blu-ray PGS.
        /// </summary>
        private bool IsPgsPacketId(int packetId)
        {
            return _pgsPacketIds.TryGetValue(packetId, out var isPgs) && isPgs;
        }

        /// <summary>
        /// Decides (once) whether <paramref name="packetId"/> carries Blu-ray PGS - by the PMT's
        /// stream_type, or when the PMT is unknown or says "private data", by the PES data being a
        /// chain of PGS segments. tsMuxeR and ffmpeg both write PGS into plain 188-byte streams.
        /// </summary>
        private bool IsPgsPacketId(int packetId, List<DvbSubPes> list)
        {
            if (_pgsPacketIds.TryGetValue(packetId, out var isPgs))
            {
                return isPgs;
            }

            if (_streamTypes.TryGetValue(packetId, out var streamType))
            {
                if (streamType == StreamTypePgs)
                {
                    _pgsPacketIds.Add(packetId, true);
                    return true;
                }

                if (streamType != ProgramMapTableStream.StreamTypePrivateData)
                {
                    _pgsPacketIds.Add(packetId, false);
                    return false;
                }
            }

            foreach (var pes in list)
            {
                if (pes.DataIdentifier != 0)
                {
                    isPgs = pes.IsPgsSegmentData;
                    _pgsPacketIds.Add(packetId, isPgs);
                    return isPgs;
                }
            }

            return false; // undecided - no data yet
        }

        public List<TransportStreamSubtitle> GetDvbSubtitles(int packetId)
        {
            return DvbSubtitlesLookup.ContainsKey(packetId) ? DvbSubtitlesLookup[packetId] : null;
        }

        internal static List<DvbSubPes> MakeSubtitlePesPackets(int packetId, List<Packet> subtitlePackets)
        {
            var list = new List<DvbSubPes>();
            int last = -1;
            var packetList = new List<Packet>();
            foreach (var packet in subtitlePackets)
            {
                if (packet.PacketId == packetId)
                {
                    if (packet.PayloadUnitStartIndicator)
                    {
                        if (packetList.Count > 0)
                        {
                            AddPesPacket(list, packetList);
                        }
                        packetList = new List<Packet>();
                    }
                    if (packet.Payload != null && last != packet.ContinuityCounter)
                    {
                        packetList.Add(packet);
                    }
                    last = packet.ContinuityCounter;
                }
            }
            if (packetList.Count > 0)
            {
                AddPesPacket(list, packetList);
            }
            return list;
        }

        public List<DvbSubPes> GetSubtitlePesPackets(int packetId)
        {
            if (SubtitlesLookup.ContainsKey(packetId))
            {
                return SubtitlesLookup[packetId];
            }

            return null;
        }

        /// <summary>
        /// DVB decoders keep a page's regions between display sets: an acquisition point or mode
        /// change (page_state 1/2) starts the page afresh, a "normal case" update (page_state 0)
        /// sends only what changed - objects are painted into regions that keep their earlier
        /// pixels unless the region is filled. Live subtitling paints a line word by word like
        /// this, so each such update is given the whole page to show.
        /// </summary>
        private static void ComposeNormalCasePageUpdates(List<DvbSubPes> list)
        {
            var regionContent = new Dictionary<int, List<DvbSubPes.PlacedObject>>(); // region id -> painted objects (region relative)
            var objects = new Dictionary<int, KeyValuePair<ObjectDataSegment, DvbSubPes>>(); // latest data per object id
            var cluts = new Dictionary<int, ClutDefinitionSegment>();
            foreach (var pes in list)
            {
                pes.ParseSegments();
                if (pes.SubtitleSegments.Count == 0)
                {
                    continue;
                }

                if (pes.ObjectDataList.Count > 0 && cluts.Count > 0)
                {
                    pes.SetInheritedClutDefinitions(cluts.Values.ToList());
                }

                foreach (var clut in pes.ClutDefinitions)
                {
                    cluts[clut.ClutId] = clut;
                }

                foreach (var ods in pes.ObjectDataList)
                {
                    objects[ods.ObjectId] = new KeyValuePair<ObjectDataSegment, DvbSubPes>(ods, pes);
                }

                var pageComposition = pes.PageCompositions.Count > 0 ? pes.PageCompositions[0] : null;
                if (pageComposition != null && pageComposition.PageState != 0)
                {
                    regionContent.Clear();
                }

                foreach (var region in pes.RegionCompositions)
                {
                    if (region.RegionFillFlag || !regionContent.TryGetValue(region.RegionId, out var painted))
                    {
                        painted = new List<DvbSubPes.PlacedObject>();
                        regionContent[region.RegionId] = painted;
                    }

                    foreach (var regionObject in region.Objects)
                    {
                        if (objects.TryGetValue(regionObject.ObjectId, out var data))
                        {
                            painted.Add(new DvbSubPes.PlacedObject
                            {
                                Object = data.Key,
                                Owner = data.Value,
                                X = regionObject.ObjectHorizontalPosition,
                                Y = regionObject.ObjectVerticalPosition,
                            });
                        }
                    }
                }

                if (pageComposition != null && pageComposition.PageState == 0 && pes.ObjectDataList.Count > 0)
                {
                    var composed = new List<DvbSubPes.PlacedObject>();
                    foreach (var pageRegion in pageComposition.Regions)
                    {
                        if (regionContent.TryGetValue(pageRegion.RegionId, out var painted))
                        {
                            foreach (var placed in painted)
                            {
                                composed.Add(new DvbSubPes.PlacedObject
                                {
                                    Object = placed.Object,
                                    Owner = placed.Owner,
                                    X = pageRegion.RegionHorizontalAddress + placed.X,
                                    Y = pageRegion.RegionVerticalAddress + placed.Y,
                                });
                            }
                        }
                    }

                    pes.SetComposedObjects(composed);
                }
            }
        }

        private static List<DvbSubPes> ParseAndRemoveEmpty(List<DvbSubPes> list)
        {
            var newList = new List<DvbSubPes>();
            foreach (var pes in list)
            {
                pes.ParseSegments();
                if (pes.ObjectDataList.Count > 0 || pes.PresentationTimestamp > 0)
                {
                    newList.Add(pes);
                }
            }
            return newList;
        }

        private static void AddPesPacket(List<DvbSubPes> list, List<Packet> packetList)
        {
            var bufferSize = 0;
            foreach (var p in packetList)
            {
                bufferSize += p.Payload.Length;
            }

            // DvbSubPes copies the data it keeps, so the concatenation buffer is
            // only needed while the constructor runs - rent it instead of
            // allocating per PES packet. Rented buffers can be larger than
            // requested, so the valid length is passed along explicitly.
            var pesData = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                var span = pesData.AsSpan();
                var offset = 0;
                foreach (var p in packetList)
                {
                    p.Payload.AsSpan().CopyTo(span.Slice(offset));
                    offset += p.Payload.Length;
                }

                DvbSubPes pes;
                if (bufferSize >= 4 && VobSubParser.IsMpeg2PackHeader(pesData))
                {
                    pes = new DvbSubPes(pesData, Mpeg2Header.Length, bufferSize);
                }
                else
                {
                    pes = new DvbSubPes(pesData, 0, bufferSize);
                }

                if (!pes.PresentationTimestamp.HasValue && packetList[0].ArrivalTimestamp.HasValue)
                {
                    // No PTS (e.g. teletext from some Topfield recorders) - the program clock at
                    // the time the packet arrived is the best estimate, as ffmpeg does
                    pes.PresentationTimestamp = packetList[0].ArrivalTimestamp;
                    pes.HasEstimatedTimestamp = true;
                }

                list.Add(pes);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(pesData);
            }
        }

        public static bool IsM2TransportStream(Stream ms)
        {
            const int requiredLength = 192 + 192 + 5;
            if (ms.Length <= requiredLength)
            {
                return false;
            }

            ms.Seek(0, SeekOrigin.Begin);
            var buffer = ArrayPool<byte>.Shared.Rent(requiredLength);
            try
            {
                ms.ReadFully(buffer, 0, requiredLength);
                var span = buffer.AsSpan(0, requiredLength);
                
                // Check for standard 188-byte packets
                if (span[0] == Packet.SynchronizationByte && span[188] == Packet.SynchronizationByte)
                {
                    return false;
                }

                // Check for M2TS 192-byte packets (4-byte timestamp + 188-byte packet)
                if (span[4] == Packet.SynchronizationByte && 
                    span[192 + 4] == Packet.SynchronizationByte && 
                    span[192 + 192 + 4] == Packet.SynchronizationByte)
                {
                    return true;
                }

                return false;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// True for a transport stream of 204-byte packets, i.e. the 188-byte packet followed by
        /// 16 bytes of Reed-Solomon parity. DVB capture cards and professional equipment write
        /// these; the parity is only of interest to a demodulator, so it is skipped when reading.
        /// </summary>
        public static bool IsRs204TransportStream(Stream ms)
        {
            const int packetLength = 204;
            const int requiredLength = packetLength * 2 + 1;
            if (ms.Length <= requiredLength)
            {
                return false;
            }

            ms.Seek(0, SeekOrigin.Begin);
            var buffer = ArrayPool<byte>.Shared.Rent(requiredLength);
            try
            {
                ms.ReadFully(buffer, 0, requiredLength);
                var span = buffer.AsSpan(0, requiredLength);

                // A plain 188-byte stream also has a sync byte at 0, so it has to be ruled out
                // first - otherwise every normal transport stream would look like RS204 as well.
                if (span[0] != Packet.SynchronizationByte || span[188] == Packet.SynchronizationByte)
                {
                    return false;
                }

                return span[packetLength] == Packet.SynchronizationByte &&
                       span[packetLength * 2] == Packet.SynchronizationByte;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static bool IsDvbSup(string fileName)
        {
            try
            {
                var pesData = File.ReadAllBytes(fileName);
                if (pesData.Length < 3)
                {
                    return false;
                }

                var header = pesData.AsSpan(0, 3);
                if (header[0] != 0x20 || header[1] != 0 || header[2] != 0x0F)
                {
                    return false;
                }

                var pes = new DvbSubPes(0, pesData);
                return pes.SubtitleSegments.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static List<TransportStreamSubtitle> GetDvbSup(string fileName)
        {
            byte[] pesData = File.ReadAllBytes(fileName);
            var list = new List<DvbSubPes>();
            int index = 0;
            while (index < pesData.Length - 10)
            {
                var pes = new DvbSubPes(index, pesData);
                index = pes.Length + 1;
                list.Add(pes);
            }

            var subtitles = new List<TransportStreamSubtitle>();
            int seconds = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var pes = list[i];
                pes.ParseSegments();
                if (pes.ObjectDataList.Count > 0)
                {
                    var sub = new TransportStreamSubtitle();
                    sub.StartMilliseconds = (ulong)seconds * 1000UL;
                    if (pes.PageCompositions.Count > 0)
                    {
                        // Only compute the end here - the shared clock is advanced once per PES by
                        // the block after this loop body. Doing it in both places double-counted
                        // the page time-out for every subtitle-bearing display set, so the start
                        // times drifted further and further ahead over the file.
                        sub.EndMilliseconds = sub.StartMilliseconds + (ulong)pes.PageCompositions[0].PageTimeOut * 1000UL;
                    }
                    else
                    {
                        sub.EndMilliseconds = sub.StartMilliseconds + 2500;
                    }

                    sub.Pes = pes;
                    subtitles.Add(sub);
                }
                if (pes.PageCompositions.Count > 0)
                {
                    seconds += pes.PageCompositions[0].PageTimeOut;
                }
            }
            return subtitles;
        }
    }
}
