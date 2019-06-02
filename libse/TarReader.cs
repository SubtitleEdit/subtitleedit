using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core
{
    public class TarReader : IDisposable
    {
        public List<TarHeader> Files { get; }
        private readonly Stream _stream;

        public TarReader(string fileName) :
            this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
        }

        public TarReader(Stream stream)
        {
            Files = new List<TarHeader>();

            /// unreadable stream
            if (!stream.CanRead)
            {
                return;
            }

            long length = stream.Length;
            long pos = 0;
            stream.Position = 0;
            while (pos + 512 < length)
            {
                stream.Seek(pos, SeekOrigin.Begin);
                var th = new TarHeader(stream);
                Files.Add(th);
                pos += TarHeader.HeaderSize + th.FileSizeInBytes;
                if (pos % TarHeader.HeaderSize > 0)
                {
                    pos += 512 - (pos % TarHeader.HeaderSize);
                }
            }

            _stream = stream;
        }

        public void Close() => _stream.Close();

        public void Dispose() => _stream?.Dispose();

    }
}
