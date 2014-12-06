using System;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.ContainerFormats.MaterialExchangeFormat
{
    public class MxfParser
    {

        public string FileName { get; private set; }
        public bool IsValid { get; private set; }

        internal byte[] Buffer;
        internal ulong Position;
        internal string Name;
        internal UInt64 Size;

        public MxfParser(string fileName)
        {
            FileName = fileName;
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ParseMxf(fs);
            }
        }

        public MxfParser(Stream stream)
        {
            FileName = null;
            ParseMxf(stream);
        }

        private void ParseMxf(Stream stream)
        {
            Position = 0;
            stream.Seek(0, SeekOrigin.Begin);
            ReadHeaderPartitionPack(stream);
        }

        private void ReadHeaderPartitionPack(Stream stream)
        {
            IsValid = false;
            var buffer = new byte[65536];
            var count = stream.Read(buffer, 0, buffer.Length);
            if (count < 100)
            {
                return;
            }

            for (int i = 0; i < count - 11; i++)
            {
                //Header Partition PackId = 06 0E 2B 34 02 05 01 01 0D 01 02
                if (buffer[i + 00] == 0x06 &&
                    buffer[i + 01] == 0x0E &&
                    buffer[i + 02] == 0x2B &&
                    buffer[i + 03] == 0x34 &&
                    buffer[i + 04] == 0x02 &&
                    buffer[i + 05] == 0x05 &&
                    buffer[i + 06] == 0x01 &&
                    buffer[i + 07] == 0x01 &&
                    buffer[i + 08] == 0x0D &&
                    buffer[i + 09] == 0x01 &&
                    buffer[i + 10] == 0x02)
                {
                    IsValid = true;
                }
            }
        }

    }
}
