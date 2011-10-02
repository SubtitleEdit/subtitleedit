using System;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Box
    {
        internal byte[] buffer;
        internal ulong pos;
        internal string name;
        internal UInt64 size;

        public uint GetUInt(byte[] buffer, int index)
        {
            return (uint)((buffer[index] << 24) + (buffer[index + 1] << 16) + (buffer[index + 2] << 8) + buffer[index + 3]);
        }

        public uint GetUInt(int index)
        {
            return (uint)((buffer[index] << 24) + (buffer[index + 1] << 16) + (buffer[index + 2] << 8) + buffer[index + 3]);
        }

        public UInt64 GetUInt64(int index)
        {
            return (UInt64)((buffer[index] << 56) + (buffer[index + 1] << 48) + (buffer[index + 2] << 40) + (buffer[index + 3] << 32) +
                            (buffer[index+4] << 24) + (buffer[index + 5] << 16) + (buffer[index + 6] << 8) + buffer[index + 7]);
        }

        public UInt64 GetUInt64(byte[] buffer, int index)
        {
            return (UInt64)((buffer[index] << 56) + (buffer[index + 1] << 48) + (buffer[index + 2] << 40) + (buffer[index + 3] << 32) +
                            (buffer[index + 4] << 24) + (buffer[index + 5] << 16) + (buffer[index + 6] << 8) + buffer[index + 7]);
        }

        public int GetWord(byte[] buffer, int index)
        {
            return (buffer[index] << 8) + buffer[index + 1];
        }

        public int GetWord(int index)
        {
            return (buffer[index] << 8) + buffer[index + 1];
        }

        public string GetString(int index, int count)
        {
            return Encoding.UTF8.GetString(buffer, index, count);
        }

        public string GetString(byte[] buffer, int index, int count)
        {
            if (count <= 0)
                return string.Empty;
            return Encoding.UTF8.GetString(buffer, index, count);
        }

        internal bool InitializeSizeAndName(System.IO.FileStream fs)
        {
            buffer = new byte[8];
            var bytesRead = fs.Read(buffer, 0, buffer.Length);
            if (bytesRead < buffer.Length)
                return false;
            size = GetUInt(0);
            name = GetString(4, 4);

            if (size == 0)
            {
                size = (UInt64)(fs.Length - fs.Position);
            }
            if (size == 1)
            {
                bytesRead = fs.Read(buffer, 0, buffer.Length);
                if (bytesRead < buffer.Length)
                    return false;
                size = GetUInt64(0);
            }
            pos = ((ulong)(fs.Position)) + size - 8;
            return true;
        }
    }
}
