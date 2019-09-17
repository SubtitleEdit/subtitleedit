using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class VobSubParser
    {
        public bool IsPal { get; private set; }
        public List<VobSubPack> VobSubPacks { get; private set; }
        public List<Color> IdxPalette { get; private set; } = new List<Color>();
        public List<string> IdxLanguages { get; private set; } = new List<string>();

        private const int PacketizedElementaryStreamMaximumLength = 2028;

        public VobSubParser(bool isPal)
        {
            IsPal = isPal;
            VobSubPacks = new List<VobSubPack>();
        }

        public void Open(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Open(fs);
            }
        }

        /// <summary>
        /// Can be used with e.g. MemoryStream or FileStream
        /// </summary>
        /// <param name="ms"></param>
        public void Open(Stream ms)
        {
            VobSubPacks = new List<VobSubPack>();
            ms.Position = 0;
            var buffer = new byte[0x800]; // 2048
            long position = 0;
            while (position < ms.Length)
            {
                ms.Seek(position, SeekOrigin.Begin);
                ms.Read(buffer, 0, 0x0800);
                if (IsSubtitlePack(buffer))
                {
                    VobSubPacks.Add(new VobSubPack(buffer, null));
                }

                position += 0x800;
            }
        }

        public void OpenSubIdx(string vobSubFileName, string idxFileName)
        {
            VobSubPacks = new List<VobSubPack>();

            if (!string.IsNullOrEmpty(idxFileName) && File.Exists(idxFileName))
            {
                var idx = new Idx(idxFileName);
                IdxPalette = idx.Palette;
                IdxLanguages = idx.Languages;
                if (idx.IdxParagraphs.Count > 0)
                {
                    var buffer = new byte[0x800]; // 2048
                    using (var fs = new FileStream(vobSubFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        foreach (var p in idx.IdxParagraphs)
                        {
                            if (p.FilePosition + 100 < fs.Length)
                            {
                                long position = p.FilePosition;
                                fs.Seek(position, SeekOrigin.Begin);
                                fs.Read(buffer, 0, 0x0800);
                                if (IsSubtitlePack(buffer) || IsPrivateStream1(buffer, 0))
                                {
                                    var vsp = new VobSubPack(buffer, p);
                                    VobSubPacks.Add(vsp);

                                    if (IsPrivateStream1(buffer, 0))
                                    {
                                        position += vsp.PacketizedElementaryStream.Length + 6;
                                    }
                                    else
                                    {
                                        position += 0x800;
                                    }

                                    int currentSubPictureStreamId = 0;
                                    if (vsp.PacketizedElementaryStream.SubPictureStreamId != null)
                                    {
                                        currentSubPictureStreamId = vsp.PacketizedElementaryStream.SubPictureStreamId.Value;
                                    }

                                    while (vsp.PacketizedElementaryStream != null &&
                                           vsp.PacketizedElementaryStream.SubPictureStreamId.HasValue &&
                                           (vsp.PacketizedElementaryStream.Length == PacketizedElementaryStreamMaximumLength ||
                                            currentSubPictureStreamId != vsp.PacketizedElementaryStream.SubPictureStreamId.Value) && position < fs.Length)
                                    {
                                        fs.Seek(position, SeekOrigin.Begin);
                                        fs.Read(buffer, 0, 0x800);
                                        vsp = new VobSubPack(buffer, p); // idx position?

                                        if (vsp.PacketizedElementaryStream != null && vsp.PacketizedElementaryStream.SubPictureStreamId.HasValue && currentSubPictureStreamId == vsp.PacketizedElementaryStream.SubPictureStreamId.Value)
                                        {
                                            VobSubPacks.Add(vsp);

                                            if (IsPrivateStream1(buffer, 0))
                                            {
                                                position += vsp.PacketizedElementaryStream.Length + 6;
                                            }
                                            else
                                            {
                                                position += 0x800;
                                            }
                                        }
                                        else
                                        {
                                            position += 0x800;
                                            fs.Seek(position, SeekOrigin.Begin);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
            }

            // No valid idx file found - just open like vob file
            Open(vobSubFileName);
        }

        /// <summary>
        /// Demultiplex multiplexed packs together each streamId at a time + removing bad packs + fixing displaytimes
        /// </summary>
        /// <returns>List of complete packs each with a complete sub image</returns>
        public List<VobSubMergedPack> MergeVobSubPacks()
        {
            var list = new List<VobSubMergedPack>();
            var pts = new TimeSpan();
            var ms = new MemoryStream();
            int streamId = 0;

            float ticksPerMillisecond = 90.000F;
            if (!IsPal)
            {
                ticksPerMillisecond = 90.090F * (23.976F / 24F);
            }

            // get unique streamIds
            var uniqueStreamIds = new List<int>();
            foreach (var p in VobSubPacks)
            {
                if (p.PacketizedElementaryStream != null &&
                    p.PacketizedElementaryStream.SubPictureStreamId.HasValue &&
                    !uniqueStreamIds.Contains(p.PacketizedElementaryStream.SubPictureStreamId.Value))
                {
                    uniqueStreamIds.Add(p.PacketizedElementaryStream.SubPictureStreamId.Value);
                }
            }

            IdxParagraph lastIdxParagraph = null;
            foreach (int uniqueStreamId in uniqueStreamIds) // packets must be merged in streamId order (so they don't get mixed)
            {
                foreach (var p in VobSubPacks)
                {
                    if (p.PacketizedElementaryStream != null && p.PacketizedElementaryStream.SubPictureStreamId.HasValue &&
                        p.PacketizedElementaryStream.SubPictureStreamId.Value == uniqueStreamId)
                    {
                        if (p.PacketizedElementaryStream.PresentationTimestampDecodeTimestampFlags > 0)
                        {
                            if (lastIdxParagraph == null || p.IdxLine.FilePosition != lastIdxParagraph.FilePosition)
                            {
                                if (ms.Length > 0)
                                {
                                    list.Add(new VobSubMergedPack(ms.ToArray(), pts, streamId, lastIdxParagraph));
                                }

                                ms.Close();
                                ms = new MemoryStream();
                                pts = TimeSpan.FromMilliseconds(Convert.ToDouble(p.PacketizedElementaryStream.PresentationTimestamp / ticksPerMillisecond)); //90000F * 1000)); (PAL)
                                streamId = p.PacketizedElementaryStream.SubPictureStreamId.Value;
                            }
                        }
                        lastIdxParagraph = p.IdxLine;
                        p.PacketizedElementaryStream.WriteToStream(ms);
                    }
                }
                if (ms.Length > 0)
                {
                    list.Add(new VobSubMergedPack(ms.ToArray(), pts, streamId, lastIdxParagraph));
                    ms.Close();
                    ms = new MemoryStream();
                }
            }
            ms.Close();

            // Remove any bad packs
            for (int i = list.Count - 1; i >= 0; i--)
            {
                VobSubMergedPack pack = list[i];
                if (pack.SubPicture == null || pack.SubPicture.ImageDisplayArea.Width <= 3 || pack.SubPicture.ImageDisplayArea.Height <= 2)
                {
                    list.RemoveAt(i);
                }
                else if (pack.EndTime.TotalSeconds - pack.StartTime.TotalSeconds < 0.1 && pack.SubPicture.ImageDisplayArea.Width <= 10 && pack.SubPicture.ImageDisplayArea.Height <= 10)
                {
                    list.RemoveAt(i);
                }
            }

            // Fix subs with no duration (completely normal) or negative duration or duration > 10 seconds
            for (int i = 0; i < list.Count; i++)
            {
                VobSubMergedPack pack = list[i];
                if (pack.SubPicture.Delay.TotalMilliseconds > 0)
                {
                    pack.EndTime = pack.StartTime.Add(pack.SubPicture.Delay);
                }

                if (pack.EndTime < pack.StartTime || pack.EndTime.TotalMilliseconds - pack.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    if (i + 1 < list.Count)
                    {
                        pack.EndTime = TimeSpan.FromMilliseconds(list[i + 1].StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines);
                        if (pack.EndTime.TotalMilliseconds - pack.StartTime.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            pack.EndTime = TimeSpan.FromMilliseconds(pack.StartTime.TotalMilliseconds + Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds);
                        }
                    }
                     else
                    {
                        pack.EndTime = TimeSpan.FromMilliseconds(pack.StartTime.TotalMilliseconds + 3000);
                    }
                }
            }

            return list;
        }

        public static bool IsMpeg2PackHeader(byte[] buffer)
        {
            return buffer.Length >= 4 &&
                   buffer[0] == 0 &&
                   buffer[1] == 0 &&
                   buffer[2] == 1 &&
                   buffer[3] == 0xba; // 0xba == 186 - MPEG-2 Pack Header
        }

        public static bool IsPrivateStream1(byte[] buffer, int index)
        {
            return buffer.Length >= index + 4 &&
                   buffer[index + 0] == 0 &&
                   buffer[index + 1] == 0 &&
                   buffer[index + 2] == 1 &&
                   buffer[index + 3] == 0xbd; // 0xbd == 189 - MPEG-2 Private stream 1 (non MPEG audio, subpictures)
        }

        public static bool IsPrivateStream2(byte[] buffer, int index)
        {
            return buffer.Length >= index + 4 &&
                   buffer[index + 0] == 0 &&
                   buffer[index + 1] == 0 &&
                   buffer[index + 2] == 1 &&
                   buffer[index + 3] == 0xbf; // 0xbf == 191 - MPEG-2 Private stream 2
        }

        public static bool IsSubtitlePack(byte[] buffer)
        {
            if (IsMpeg2PackHeader(buffer) && IsPrivateStream1(buffer, Mpeg2Header.Length))
            {
                int pesHeaderDataLength = buffer[Mpeg2Header.Length + 8];
                int streamId = buffer[Mpeg2Header.Length + 8 + 1 + pesHeaderDataLength];
                if (streamId >= 0x20 && streamId <= 0x3f) // Subtitle IDs allowed (or x3f to x40?)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
