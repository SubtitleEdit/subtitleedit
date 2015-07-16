﻿using System.IO;

namespace Nikse.SubtitleEdit.Logic.ContainerFormats.Mp4.Boxes
{
    public class Tkhd : Box
    {
        public readonly uint TrackId;
        public readonly ulong Duration;
        public readonly uint Width;
        public readonly uint Height;

        public Tkhd(FileStream fs)
        {
            Buffer = new byte[84];
            int bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
                return;

            int version = Buffer[0];
            int addToIndex64Bit = 0;
            if (version == 1)
                addToIndex64Bit = 8;

            TrackId = GetUInt(12 + addToIndex64Bit);
            if (version == 1)
            {
                Duration = GetUInt64(20 + addToIndex64Bit);
                addToIndex64Bit += 4;
            }
            else
            {
                Duration = GetUInt(20 + addToIndex64Bit);
            }

            Width = (uint)GetWord(76 + addToIndex64Bit); // skip decimals
            Height = (uint)GetWord(80 + addToIndex64Bit); // skip decimals
            //System.Windows.Forms.MessageBox.Show("Width: " + GetWord(76 + addToIndex64Bit).ToString() + "." + GetWord(78 + addToIndex64Bit).ToString());
            //System.Windows.Forms.MessageBox.Show("Height: " + GetWord(80 + addToIndex64Bit).ToString() + "." + GetWord(82 + addToIndex64Bit).ToString());
        }
    }
}
