using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core
{
    public class TarReader : IDisposable
    {
        public IList<TarHeader> Files { get; private set; }
        private Stream _stream;

        public TarReader(string fileName)
        {
            OpenTarFile(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        public TarReader(Stream stream)
        {
            OpenTarFile(stream);
        }

        private void OpenTarFile(Stream stream)
        {
            _stream = stream;
            Files = new List<TarHeader>();
            long length = stream.Length;
            long pos = 0;
            stream.Position = 0;
            while (pos + 512 < length)
            {
                stream.Seek(pos, SeekOrigin.Begin);
                var th = new TarHeader(stream);
                if (th.FileSizeInBytes > 0)
                    Files.Add(th);
                pos += TarHeader.HeaderSize + th.FileSizeInBytes;
                if (pos % TarHeader.HeaderSize > 0)
                    pos += 512 - (pos % TarHeader.HeaderSize);
            }
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
            if (Files != null && Files.Count > 0)
            {
                foreach (var tarheader in Files)
                {
                    tarheader.Dispose();
                }
                Files = null;
            }
        }

    }
}
