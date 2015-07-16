﻿using System.IO;

namespace Nikse.SubtitleEdit.Logic.ContainerFormats.Mp4.Boxes
{
    public class Mvhd : Box
    {
        public readonly uint CreationTime;
        public readonly uint ModificationTime;
        public readonly uint Duration;
        public readonly uint TimeScale;

        public Mvhd(FileStream fs)
        {
            Buffer = new byte[20];
            int bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
                return;

            CreationTime = GetUInt(4);
            ModificationTime = GetUInt(8);
            TimeScale = GetUInt(12);
            Duration = GetUInt(16);
        }
    }
}
