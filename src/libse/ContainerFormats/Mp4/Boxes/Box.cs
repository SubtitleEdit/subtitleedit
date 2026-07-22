using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    public class Box
    {
        private static readonly char[] LineBreakChars = { '\r', '\n' };

        public byte[] Buffer;
        public ulong Position;
        public ulong StartPosition;
        public string Name;
        public ulong Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetUInt(int index)
        {
            return BinaryPrimitives.ReadUInt32BigEndian(Buffer.AsSpan(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt(int index)
        {
            return BinaryPrimitives.ReadInt32BigEndian(Buffer.AsSpan(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetUInt64(int index)
        {
            return BinaryPrimitives.ReadUInt64BigEndian(Buffer.AsSpan(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWord(int index)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(Buffer.AsSpan(index));
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
            if (text.IndexOfAny(LineBreakChars) >= 0)
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
            Size = BinaryPrimitives.ReadUInt32BigEndian(span);
            Name = Encoding.UTF8.GetString(span.Slice(4, 4));

            if (Size == 0)
            {
                // Box extends to end of file; +8 offsets the header the Position formula below subtracts, so Position lands at EOF
                Size = (ulong)(fs.Length - fs.Position) + 8;
            }
            else if (Size == 1)
            {
                bytesRead = fs.Read(Buffer, 0, 8);
                if (bytesRead < 8)
                {
                    return false;
                }

                Size = BinaryPrimitives.ReadUInt64BigEndian(Buffer.AsSpan(0, 8)) - 8;
            }

            Position = (ulong)fs.Position + Size - 8;

            // Guarantee forward progress so a malformed size cannot make the caller re-read the same bytes forever
            if (Position < (ulong)fs.Position)
            {
                Position = (ulong)fs.Position;
            }

            return true;
        }
    }
}
