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

                if (Name == "moov" && Moov == null)
                {
                    Moov = new Moov(fs, Position); // only scan first "moov" element
                }
                else if (Name == "moof")
                {
                    Moof = new Moof(fs, Position);
                }
                else if (Name == "mdat" && Moof != null && Moof?.Traf?.Trun?.Samples?.Count > 0)
                {
                    var mdat = new Mdat(fs, Position);
                    if (Moof.Traf?.Trun?.Samples.Count > 0)
                    {
                        if (VttcSubtitle == null)
                        {
                            VttcSubtitle = new Subtitle();
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
            var positionInfo = WebVTT.GetPositionInfo(style);
            if (!string.IsNullOrEmpty(positionInfo))
            {
                p.Text = positionInfo + p.Text;
                p.Extra = style;
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
