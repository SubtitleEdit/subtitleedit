using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core
{
    public class TarReader
    {
        public List<TarHeader> Files { get; private set; }

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
            Files = new List<TarHeader>();
            long length = stream.Length;
            long pos = 0;
            stream.Position = 0;
            while (pos + TarHeader.HeaderSize < length)
            {
                stream.Seek(pos, SeekOrigin.Begin);
                var th = new TarHeader(stream);
                if (th.FileSizeInBytes > 0)
                    Files.Add(th);
                pos += TarHeader.HeaderSize + th.FileSizeInBytes;
                if (pos % TarHeader.HeaderSize > 0)
                    pos += TarHeader.HeaderSize - (pos % TarHeader.HeaderSize);
            }
        }

    }
}
