using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class TarHeader
    {
        public const int HeaderSize = 512;

        public string FileName { get; set; }
        public bool IsFolder { get; set; }
        public long FileSizeInBytes { get; set; }
        public long FilePosition { get; set; }

        private readonly Stream _stream;

        public TarHeader(Stream stream)
        {
            _stream = stream;
            var buffer = new byte[HeaderSize];
            stream.Read(buffer, 0, HeaderSize);
            FilePosition = stream.Position;

            FileName = Encoding.ASCII.GetString(buffer, 0, 100)
                .Replace("\0", string.Empty)
                .Replace('/', Path.DirectorySeparatorChar)
                .Trim();
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

    }
}
