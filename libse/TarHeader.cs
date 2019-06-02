using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class TarHeader : IDisposable
    {
        public const int HeaderSize = 512;

        public string FileName { get; }
        public bool IsFolder { get; }
        public long FileSizeInBytes { get; }
        public long FilePosition { get; }

        private readonly Stream _stream;

        public TarHeader(Stream stream)
        {
            _stream = stream;
            var buffer = new byte[HeaderSize];
            FilePosition = stream.Position;
            stream.Read(buffer, 0, HeaderSize);

            FileName = Encoding.ASCII.GetString(buffer, 0, 100).Replace("\0", string.Empty).Trim();

            // use platform-specific directory separator
            if (Path.DirectorySeparatorChar != '/')
            {
                FileName = FileName.Replace('/', Path.DirectorySeparatorChar);
            }

            IsFolder = buffer[156] == 53;

            var sizeInBytes = Encoding.ASCII.GetString(buffer, 124, 11);
            if (!string.IsNullOrEmpty(FileName) && Utilities.IsInteger(sizeInBytes))
            {
                FileSizeInBytes = Convert.ToInt64(sizeInBytes.Trim(), 8);
            }
        }

        public void WriteData(string fileName)
        {
            var buffer = new byte[FileSizeInBytes];
            _stream.Position = FilePosition;
            _stream.Read(buffer, 0, buffer.Length);
            File.WriteAllBytes(fileName, buffer);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
