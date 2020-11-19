using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class TarReader : IDisposable
    {
        public List<TarHeader> Files { get; private set; }
        private Stream _stream;

        public TarReader(string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            OpenTarFile(fs);
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
                Files.Add(th);
                pos += TarHeader.HeaderSize + th.FileSizeInBytes;
                if (pos % TarHeader.HeaderSize > 0)
                {
                    pos += 512 - (pos % TarHeader.HeaderSize);
                }
            }
        }

        public void Close()
        {
            _stream.Close();
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

    }
}
