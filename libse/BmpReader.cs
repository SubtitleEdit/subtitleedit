using System;

namespace Nikse.SubtitleEdit.Core
{
    public class BmpReader
    {
        public string HeaderId { get; private set; }
        public UInt32 HeaderFileSize { get; private set; }
        public UInt32 OffsetToPixelArray { get; private set; }

        public BmpReader(string fileName)
        {
            byte[] buffer = System.IO.File.ReadAllBytes(fileName);
            HeaderId = System.Text.Encoding.UTF8.GetString(buffer, 0, 2);
            HeaderFileSize = BitConverter.ToUInt32(buffer, 2);
            OffsetToPixelArray = BitConverter.ToUInt32(buffer, 0xa);
        }

    }
}
