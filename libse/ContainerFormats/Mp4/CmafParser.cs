using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    /// <summary>
    /// http://wiki.multimedia.cx/index.php?title=QuickTime_container
    /// </summary>
    public class CmafParser : Box
    {
        public string FileName { get; private set; }
        public Moov Moov { get; private set; }
        public Moof Moof { get; private set; }
        public Subtitle Subtitle { get; private set; }

        public List<Trak> GetSubtitleTracks()
        {
            var list = new List<Trak>();
            if (Moov?.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                {
                    if (trak.Mdia != null && (trak.Mdia.IsTextSubtitle || trak.Mdia.IsVobSubSubtitle || trak.Mdia.IsClosedCaption) && trak.Mdia.Minf?.Stbl != null)
                    {
                        list.Add(trak);
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
                    return TimeSpan.FromSeconds((double)Moov.Mvhd.Duration / Moov.Mvhd.TimeScale);
                return new TimeSpan();
            }
        }

        public DateTime CreationDate
        {
            get
            {
                if (Moov?.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                    return new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(Moov.Mvhd.CreationTime));
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
                                return new System.Drawing.Point((int)trak.Tkhd.Width, (int)trak.Tkhd.Height);
                        }
                    }
                }
                return new System.Drawing.Point(0, 0);
            }
        }

        public CmafParser(string fileName)
        {
            FileName = fileName;
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ParseCmaf(fs);
                fs.Close();
            }
        }

        public CmafParser(FileStream fs)
        {
            FileName = null;
            ParseCmaf(fs);
        }

        private void ParseCmaf(FileStream fs)
        {
            Subtitle = new Subtitle();
            int count = 0;
            Position = 0;
            fs.Seek(0, SeekOrigin.Begin);
            bool moreBytes = true;
            var samples = new List<TimeSegment>();
            var payloads = new List<string>();
            while (moreBytes)
            {
                moreBytes = InitializeSizeAndName(fs);
                if (Size < 8)
                    return;

                if (Name == "moov" && Moov == null)
                {
                    Moov = new Moov(fs, Position); // only scan first "moov" element
                }
                else if (Name == "mdat")
                {
                    var mdat = new Mdat(fs, Position);
                    if (mdat.Payloads.Count > 0)
                    {
                        payloads.AddRange(mdat.Payloads);
                    }
                }
                else if (Name == "moof")
                {
                    Moof = new Moof(fs, Position);
                    if (Moof.Traf?.Trun?.Samples.Count > 0)
                    {
                        samples.AddRange(Moof.Traf.Trun.Samples);
                    }
                }

                count++;
                if (count > 10000)
                    break;

                if (Position > (ulong)fs.Length)
                    break;
                fs.Seek((long)Position, SeekOrigin.Begin);
            }
            fs.Close();


            ulong timePeriodStart = 0; // ???
            ulong timeScale = 0; // ??
            if (Moov?.Tracks[0].Mdia.Mdhd.TimeScale > 0)
                timeScale = Moov.Tracks[0].Mdia.Mdhd.TimeScale;

            if (Moov?.Mvhd?.TimeScale > 0)
                timeScale = Moov.Mvhd.TimeScale;

            int max = Math.Min(samples.Count, payloads.Count);

            for (var i = 0; i < max; i++)
            {
                var presentation = samples[i];
                var payload = payloads[i];

                if (presentation.Duration.HasValue)
                {
                    var startTime = presentation.TimeOffset.HasValue ? presentation.BaseMediaDecodeTime + presentation.TimeOffset : presentation.BaseMediaDecodeTime;
                    var currentTime = (ulong)startTime + presentation.Duration.Value;

                    // The payload can be null as that would mean that it was a VTTE and
                    // was only inserted to keep the presentation times in sync with the
                    // payloads.
                    if (payload != null)
                    {
                        Subtitle.Paragraphs.Add(new Paragraph(payload, (double)(timePeriodStart + startTime / timeScale), (double)(timePeriodStart + currentTime / timeScale)));
                    }
                }
            }
            Subtitle.Renumber();
        }

        internal double FrameRate
        {
            get
            {
                // Formula: moov.mdia.stbl.stsz.samplecount / (moov.trak.tkhd.duration / moov.mvhd.timescale) - http://www.w3.org/2008/WebVideo/Annotations/drafts/ontology10/CR/test.php?table=containerMPEG4
                if (Moov?.Mvhd != null && Moov.Mvhd.TimeScale > 0)
                {
                    var videoTracks = GetVideoTracks();
                    if (videoTracks.Count > 0 && videoTracks[0].Tkhd != null && videoTracks[0].Mdia != null && videoTracks[0].Mdia.Minf != null && videoTracks[0].Mdia.Minf.Stbl != null)
                    {
                        double duration = videoTracks[0].Tkhd.Duration;
                        double sampleCount = videoTracks[0].Mdia.Minf.Stbl.StszSampleCount;
                        return sampleCount / (duration / Moov.Mvhd.TimeScale);
                    }
                }
                return 0;
            }
        }

    }
}
