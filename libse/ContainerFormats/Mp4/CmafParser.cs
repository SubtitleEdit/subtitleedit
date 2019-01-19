using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4
{
    /// <summary>
    /// Common Media Application Format
    /// </summary>
    public class CmafParser : Box
    {
        public string FileName { get; private set; }
        internal Moov Moov { get; private set; }
        internal Moof Moof { get; private set; }
        public Subtitle Subtitle { get; private set; }

        public CmafParser(string fileName)
        {
            FileName = fileName;
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ParseCmaf(fs);
                fs.Close();
            }
        }

        public CmafParser(Stream fs)
        {
            FileName = null;
            ParseCmaf(fs);
        }

        private void ParseCmaf(Stream fs)
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
                {
                    return;
                }

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

            ulong timePeriodStart = 0; // ???
            ulong timeScale = 0;
            if (Moov?.Tracks[0].Mdia.Mdhd.TimeScale > 0)
            {
                timeScale = Moov.Tracks[0].Mdia.Mdhd.TimeScale;
            }

            if (Moov?.Mvhd?.TimeScale > 0)
            {
                timeScale = Moov.Mvhd.TimeScale;
            }

            int max = Math.Min(samples.Count, payloads.Count);

            for (var i = 0; i < max; i++)
            {
                var presentation = samples[i];
                var payload = payloads[i];

                if (presentation.Duration.HasValue)
                {
                    var startTime = presentation.TimeOffset.HasValue ? presentation.BaseMediaDecodeTime + presentation.TimeOffset.Value : presentation.BaseMediaDecodeTime;
                    var currentTime = startTime + presentation.Duration.Value;

                    // The payload can be null as that would mean that it was a VTTE and
                    // was only inserted to keep the presentation times in sync with the
                    // payloads.
                    if (payload != null)
                    {
                        Subtitle.Paragraphs.Add(new Paragraph(payload, timePeriodStart + startTime / timeScale, timePeriodStart + currentTime / timeScale));
                    }
                }
            }
            Subtitle.Renumber();
        }

    }
}