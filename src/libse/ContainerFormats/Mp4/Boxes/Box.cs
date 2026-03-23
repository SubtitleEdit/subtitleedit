using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Box
    {
        public byte[] Buffer;
        public ulong Position;
        public ulong StartPosition;
        public string Name;
        public ulong Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetUInt(ReadOnlySpan<byte> span)
        {
            // Big-endian byte order (network byte order)
            return (uint)(span[0] << 24 | span[1] << 16 | span[2] << 8 | span[3]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetUInt(int index)
        {
            return GetUInt(Buffer.AsSpan(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt(int index)
        {
            return (int)GetUInt(Buffer.AsSpan(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetUInt64(ReadOnlySpan<byte> span)
        {
            // Big-endian byte order
            return (ulong)span[0] << 56 | (ulong)span[1] << 48 | (ulong)span[2] << 40 | (ulong)span[3] << 32 |
                   (ulong)span[4] << 24 | (ulong)span[5] << 16 | (ulong)span[6] << 8 | span[7];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetUInt64(int index)
        {
            return GetUInt64(Buffer.AsSpan(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetWord(byte[] buffer, int index)
        {
            return GetWord(buffer.AsSpan(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetWord(ReadOnlySpan<byte> span)
        {
            return (span[0] << 8) | span[1];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWord(int index)
        {
            return GetWord(Buffer.AsSpan(index));
        }

        public string GetString(int index, int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }
            return Encoding.UTF8.GetString(Buffer.AsSpan(index, count));
        }

        public static string GetString(byte[] buffer, int index, int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }

            var text = Encoding.UTF8.GetString(buffer.AsSpan(index, count));

            // Only normalize line endings if they exist
            if (text.IndexOfAny(new[] { '\r', '\n' }) >= 0)
            {
                return string.Join(Environment.NewLine, text.SplitToLines());
            }

            return text;
        }

        internal bool InitializeSizeAndName(System.IO.Stream fs)
        {
            if (StartPosition == 0)
            {
                StartPosition = (ulong)fs.Position - 8;
            }

            // Reuse existing buffer if available and correct size
            if (Buffer == null || Buffer.Length != 8)
            {
                Buffer = new byte[8];
            }

            var bytesRead = fs.Read(Buffer, 0, 8);
            if (bytesRead < 8)
            {
                return false;
            }

            var span = Buffer.AsSpan(0, 8);
            Size = GetUInt(span);
            Name = Encoding.UTF8.GetString(span.Slice(4, 4));

            if (Size == 0)
            {
                Size = (ulong)(fs.Length - fs.Position);
            }
            else if (Size == 1)
            {
                bytesRead = fs.Read(Buffer, 0, 8);
                if (bytesRead < 8)
                {
                    return false;
                }

                Size = GetUInt64(Buffer.AsSpan(0, 8)) - 8;
            }

            Position = (ulong)fs.Position + Size - 8;
            return true;
        }
    }
}
