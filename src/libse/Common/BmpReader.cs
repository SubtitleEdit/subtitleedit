using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class BmpReader
    {
        public string HeaderId { get; }
        public uint HeaderFileSize { get; }
        public uint OffsetToPixelArray { get; }

        public BmpReader(string fileName)
        {
            var buffer = System.IO.File.ReadAllBytes(fileName);
            HeaderId = System.Text.Encoding.UTF8.GetString(buffer, 0, 2);
            HeaderFileSize = BitConverter.ToUInt32(buffer, 2);
            OffsetToPixelArray = BitConverter.ToUInt32(buffer, 0xa);
        }
    }
}
