using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public class TarHeader
    {
        public const int HeaderSize = 512;

        private readonly Stream _stream;
        private readonly long _fileSizeInBytes;

        public TarHeader(Stream stream)
        {
            _stream = stream;
            byte[] buffer = new byte[HeaderSize];
            stream.Read(buffer, 0, HeaderSize);
            FilePosition = stream.Position;

            FileName = Encoding.ASCII.GetString(buffer, 0, 100).Replace("\0", string.Empty);
            if (!string.IsNullOrEmpty(FileName))
            {
                var sizeInBytes = Encoding.ASCII.GetString(buffer, 124, 11);
                long.TryParse(sizeInBytes, out _fileSizeInBytes);
            }
        }

        public string FileName { get; set; }

        public long FilePosition { get; set; }

        public long FileSizeInBytes
        {
            get
            {
                return _fileSizeInBytes;
            }
        }

        public void WriteData(string fileName)
        {
            var buffer = new byte[_fileSizeInBytes];
            _stream.Position = FilePosition;
            _stream.Read(buffer, 0, buffer.Length);
            File.WriteAllBytes(fileName, buffer);
        }
    }
}
