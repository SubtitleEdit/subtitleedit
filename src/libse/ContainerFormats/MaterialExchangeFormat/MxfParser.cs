using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat
{
    public class MxfParser
    {
        public string FileName { get; }
        public bool IsValid { get; private set; }

        private readonly List<string> _subtitleList = new List<string>();
        public List<string> GetSubtitles()
        {
            return _subtitleList;
        }

        private long _startPosition;

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
            stream.Seek(0, SeekOrigin.Begin);
            ReadHeaderPartitionPack(stream);
            if (IsValid)
            {
                var length = stream.Length;
                long next = _startPosition;
                while (next + 20 < length)
                {
                    stream.Seek(next, SeekOrigin.Begin);
                    var klv = new KlvPacket(stream);
                    next += klv.TotalSize;
                    if ((klv.IdentifierType == KeyIdentifier.EssenceElement || klv.IdentifierType == KeyIdentifier.Unknown) && klv.DataSize < 500000)
                    {
                        stream.Seek(klv.DataPosition, SeekOrigin.Begin);
                        var buffer = new byte[klv.DataSize];
                        stream.Read(buffer, 0, buffer.Length);
                        if (buffer.Length >= 12)
                        {
                            string s;
                            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                            {
                                s = System.Text.Encoding.UTF8.GetString(buffer, 3, buffer.Length - 3);
                            }
                            else if (buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0 && buffer[3] == 0)
                            {
                                s = System.Text.Encoding.GetEncoding(12000).GetString(buffer, 4, buffer.Length - 4); // UTF-32 (LE)
                            }
                            else if (buffer[0] == 0xff && buffer[1] == 0xfe)
                            {
                                s = System.Text.Encoding.Unicode.GetString(buffer, 2, buffer.Length - 2);
                            }
                            else if (buffer[0] == 0xfe && buffer[1] == 0xff) // utf-16 and ucs-2
                            {
                                s = System.Text.Encoding.BigEndianUnicode.GetString(buffer, 2, buffer.Length - 2);
                            }
                            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff) // ucs-4
                            {
                                s = System.Text.Encoding.GetEncoding(12001).GetString(buffer, 4, buffer.Length - 4); // UTF-32 (BE)
                            }
                            else
                            {
                                s = System.Text.Encoding.UTF8.GetString(buffer);
                            }

                            if (IsSubtitle(s))
                            {
                                _subtitleList.Add(s);
                            }
                        }
                    }
                }
            }
        }

        private bool IsSubtitle(string s)
        {
            if (s.Contains("\0"))
            {
                return false;
            }
            if (!s.Contains("xml") && !s.Contains(" --> ") && !s.Contains("00:00"))
            {
                return false;
            }

            var list = new List<string>(s.SplitToLines());
            var subtitle = new Subtitle();
            return subtitle.ReloadLoadSubtitle(list, null, null) != null;
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
            _startPosition = 0;

            for (int i = 0; i < count - 11; i++)
            {
                //Header Partition PackId = 06 0E 2B 34 02 05 01 01 0D 01 02
                if (buffer[i + 00] == 0x06 && // OID
                    buffer[i + 01] == 0x0E && // payload is 14 bytes
                    buffer[i + 02] == 0x2B && // 0x2B+34 lookup bytes
                    buffer[i + 03] == 0x34 &&
                    buffer[i + 04] == 0x02 &&
                    buffer[i + 05] == 0x05 &&
                    buffer[i + 06] == 0x01 &&
                    buffer[i + 07] == 0x01 &&
                    buffer[i + 08] == 0x0D &&
                    buffer[i + 09] == 0x01 &&
                    buffer[i + 10] == 0x02)
                {
                    _startPosition = i;
                    IsValid = true;
                    break;
                }
            }
        }

    }
}
