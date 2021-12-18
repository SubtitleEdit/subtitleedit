using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using System;
using System.Collections.Generic;
using System.IO;
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
            if (Moov?.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                {
                    if (trak.Mdia != null && (trak.Mdia.IsTextSubtitle || trak.Mdia.IsVobSubSubtitle || trak.Mdia.IsClosedCaption) && trak.Mdia.Minf?.Stbl != null)
                    {
                        if (trak.Mdia.Minf.Stbl.GetParagraphs().Count > 0)
                        {
                            list.Add(trak);
                        }
                    }
                }
            }

            return list;
        }

        public List<Trak> GetAudioTracks()
        {
            var list = new List<Trak>();
            if (Moov?.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                {
                    if (trak.Mdia != null && trak.Mdia.IsAudio)
                    {
                        list.Add(trak);
                    }
                }
            }
            return list;
        }

        public List<Trak> GetVideoTracks()
        {
            var list = new List<Trak>();
            if (Moov?.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                {
                    if (trak.Mdia != null && trak.Mdia.IsVideo)
                    {
                        list.Add(trak);
                    }
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
                if (Moov?.Tracks != null)
                {
                    foreach (var trak in Moov.Tracks)
                    {
                        if (trak?.Mdia != null && trak.Tkhd != null)
                        {
                            if (trak.Mdia.IsVideo)
                            {
                                return new System.Drawing.Point((int)trak.Tkhd.Width, (int)trak.Tkhd.Height);
                            }
                        }
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
            int count = 0;
            Position = 0;
            fs.Seek(0, SeekOrigin.Begin);
            bool moreBytes = true;
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
                    if (mdat.Payloads.Count > 0)
                    {
                        if (Moof.Traf?.Trun?.Samples.Count > 0 && Moof?.Traf?.Trun?.Samples.Count >= mdat.Payloads.Count)
                        {
                            if (VttcSubtitle == null)
                            {
                                VttcSubtitle = new Subtitle();
                            }

                            var timeScale = (double)(Moov?.Mvhd?.TimeScale ?? 1000.0);
                            var sampleIdx = 0;
                            foreach (var payload in mdat.Payloads)
                            {
                                var presentation = Moof.Traf.Trun.Samples[sampleIdx];
                                if (presentation.Duration.HasValue)
                                {
                                    var before = timeTotalMs;
                                    timeTotalMs += presentation.Duration.Value / timeScale * 1000.0;
                                    sampleIdx++;
                                    if (payload != null)
                                    {
                                        VttcSubtitle.Paragraphs.Add(new Paragraph(payload, before, timeTotalMs));
                                    }
                                }
                            }
                        }

                        Moof = null;
                    }
                }

                count++;
                if (count > 1000)
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

        internal double FrameRate
        {
            get
            {
                // Formula: moov.mdia.stbl.stsz.samplecount / (moov.trak.tkhd.duration / moov.mvhd.timescale) - http://www.w3.org/2008/WebVideo/Annotations/drafts/ontology10/CR/test.php?table=containerMPEG4
                if (Moov?.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                {
                    var videoTracks = GetVideoTracks();
                    if (videoTracks.Count > 0 && videoTracks[0].Tkhd != null && videoTracks[0].Mdia?.Minf?.Stbl != null)
                    {
                        double duration = videoTracks[0].Tkhd.Duration;
                        double sampleCount = videoTracks[0].Mdia.Minf.Stbl.StszSampleCount;
                        return sampleCount / (duration / Moov.Mvhd.TimeScale);
                    }
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
                bool moreBytes = true;
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
                        var readLength = ((long)Position) - before;
                        if (readLength > 10)
                        {
                            var buffer = new byte[readLength];
                            fs.Read(buffer, 0, (int)readLength);
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
