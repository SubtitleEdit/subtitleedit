using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Box
    {
        public byte[] Buffer;
        public ulong Position;
        public string Name;
        public ulong Size;

        public static uint GetUInt(byte[] buffer, int index)
        {
            return (uint)((buffer[index] << 24) + (buffer[index + 1] << 16) + (buffer[index + 2] << 8) + buffer[index + 3]);
        }

        public uint GetUInt(int index)
        {
            return (uint)((Buffer[index] << 24) + (Buffer[index + 1] << 16) + (Buffer[index + 2] << 8) + Buffer[index + 3]);
        }

        public ulong GetUInt64(int index)
        {
            return (ulong)Buffer[index] << 56 | (ulong)Buffer[index + 1] << 48 | (ulong)Buffer[index + 2] << 40 | (ulong)Buffer[index + 3] << 32 |
                   (ulong)Buffer[index + 4] << 24 | (ulong)Buffer[index + 5] << 16 | (ulong)Buffer[index + 6] << 8 | Buffer[index + 7];
        }

        public static ulong GetUInt64(byte[] buffer, int index)
        {
            return (ulong)buffer[index] << 56 | (ulong)buffer[index + 1] << 48 | (ulong)buffer[index + 2] << 40 | (ulong)buffer[index + 3] << 32 |
                   (ulong)buffer[index + 4] << 24 | (ulong)buffer[index + 5] << 16 | (ulong)buffer[index + 6] << 8 | buffer[index + 7];
        }

        public static int GetWord(byte[] buffer, int index)
        {
            return (buffer[index] << 8) + buffer[index + 1];
        }

        public int GetWord(int index)
        {
            return (Buffer[index] << 8) + Buffer[index + 1];
        }

        public string GetString(int index, int count)
        {
            return Encoding.UTF8.GetString(Buffer, index, count);
        }

        public static string GetString(byte[] buffer, int index, int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(buffer, index, count);
        }

        internal bool InitializeSizeAndName(System.IO.Stream fs)
        {
            Buffer = new byte[8];
            var bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return false;
            }

            Size = GetUInt(0);
            Name = GetString(4, 4);

            if (Size == 0)
            {
                Size = (ulong)(fs.Length - fs.Position);
            }
            if (Size == 1)
            {
                bytesRead = fs.Read(Buffer, 0, Buffer.Length);
                if (bytesRead < Buffer.Length)
                {
                    return false;
                }

                Size = GetUInt64(0) - 8;
            }
            Position = (ulong)fs.Position + Size - 8;
            return true;
        }

    }
}
