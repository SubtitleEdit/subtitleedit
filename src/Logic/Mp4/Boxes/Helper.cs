using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public static class Helper
    {
        public static uint GetUInt(byte[] buffer, int index)
        {
            return (uint)((buffer[index] << 24) + (buffer[index + 1] << 16) + (buffer[index + 2] << 8) + buffer[index + 3]);
        }

        public static UInt64 GetUInt64(byte[] buffer, int index)
        {
            return (UInt64)((buffer[index] << 56) + (buffer[index + 1] << 48) + (buffer[index + 2] << 40) + (buffer[index + 3] << 32) +
                            (buffer[index] << 24) + (buffer[index + 1] << 16) + (buffer[index + 2] << 8) + buffer[index + 3]);

        }

        public static int GetWord(byte[] buffer, int index)
        {
            return (buffer[index] << 8) + buffer[index + 1];
        }

        public static string GetString(byte[] buffer, int index, int count)
        {
            return Encoding.UTF8.GetString(buffer, index, count);
        }


        internal static ulong ReadSize(byte[] buffer, out int bytesRead, System.IO.FileStream fs)
        {
            bytesRead = fs.Read(buffer, 0, 8);
            UInt64 size = Helper.GetUInt(buffer, 0);
            if (size == 0)
            {
                return (UInt64) (fs.Length - fs.Position);
            }
            else if (size == 1)
            {
                byte[] size64Buffer = new byte[8];
                fs.Read(size64Buffer, 0, 8);
                size = Helper.GetUInt64(size64Buffer, 0);
            }
            return size;
        }
    }
}
