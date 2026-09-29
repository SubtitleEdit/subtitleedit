using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Cea708;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat
{
    public class MxfParser
    {
        public string FileName { get; }
        public bool IsValid { get; private set; }

        private readonly List<string> _subtitleList = new List<string>();
        private readonly List<byte[]> _images = new List<byte[]>();

        public List<string> GetSubtitles() => _subtitleList;

        public List<byte[]> GetImages() => _images;

        /// <summary>
        /// CEA-608/708 closed captions from a SMPTE 436M ANC data track (caption distribution
        /// packets in VANC): paragraphs per track key (1-4 = CC1-CC4, 100 + n = CEA-708 service n,
        /// see <see cref="ClosedCaptionDecoder"/>). Times are from the start of the essence.
        /// </summary>
        public SortedDictionary<int, List<Paragraph>> ClosedCaptionTracks { get; private set; } = new SortedDictionary<int, List<Paragraph>>();

        private long _startPosition;
        private double _editRate; // frames per second from the header metadata, 0 = not found yet
        private byte[] _ancElementKey; // the first ANC data element key seen - one track is decoded
        private int _ancFrameIndex;
        private ClosedCaptionDecoder _closedCaptionDecoder;

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
                    if (IsHeaderMetadataSet(klv.Key) && _editRate == 0 && klv.DataSize < 65536)
                    {
                        ReadEditRate(stream, klv);
                    }

                    if (IsAncDataElement(klv.Key) && klv.DataSize < 65536)
                    {
                        ReadAncDataElement(stream, klv);
                        continue;
                    }

                    // Picture and sound never hold subtitles - reading and text sniffing every video
                    // frame made opening an hour of 50 Mbit/s broadcast MXF take minutes.
                    if (IsPictureOrSoundElement(klv.Key))
                    {
                        continue;
                    }

                    if ((klv.IdentifierType == KeyIdentifier.EssenceElement || klv.IdentifierType == KeyIdentifier.Unknown) && klv.DataSize < 500000)
                    {
                        stream.Seek(klv.DataPosition, SeekOrigin.Begin);
                        var buffer = new byte[klv.DataSize];
                        var bytesRead = stream.Read(buffer, 0, buffer.Length);

                        // A zero- or short-length value is legal MXF (an empty fill/local set),
                        // so the magic test needs four bytes to actually be there.
                        if (bytesRead < 4)
                        {
                            continue;
                        }

                        if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47) // PNG header
                        {
                            _images.Add(buffer);
                            continue;
                        }

                        if (buffer.Length >= 12 && bytesRead >= 12)
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

                if (_closedCaptionDecoder?.HasData == true)
                {
                    ClosedCaptionTracks = _closedCaptionDecoder.Finish(0);
                }
            }
        }

        /// <summary>
        /// Header metadata local set (06 0E 2B 34 02 53 01 01 0D 01 01 01 ...).
        /// </summary>
        private static bool IsHeaderMetadataSet(byte[] key)
        {
            return key[4] == 0x02 && key[5] == 0x53 && key[8] == 0x0D && key[9] == 0x01 && key[10] == 0x01 && key[11] == 0x01;
        }

        /// <summary>
        /// Essence element of a picture or sound item: content package (SMPTE 331M, 0x05/0x06) or
        /// generic container (SMPTE 379M, 0x15/0x16). Subtitles (e.g. SMPTE 429-5 timed text) and
        /// SMPTE 436M ANC/VBI data are data items (0x17).
        /// </summary>
        private static bool IsPictureOrSoundElement(byte[] key)
        {
            return key[4] == 0x01 && key[5] == 0x02 && key[8] == 0x0D && key[9] == 0x01 && key[10] == 0x03 && key[11] == 0x01 &&
                   (key[12] == 0x05 || key[12] == 0x06 || key[12] == 0x15 || key[12] == 0x16);
        }

        /// <summary>
        /// Generic container data item (0x17) ANC data element (0x02) - SMPTE 436M VANC/HANC packets.
        /// </summary>
        private static bool IsAncDataElement(byte[] key)
        {
            return key[4] == 0x01 && key[5] == 0x02 && key[8] == 0x0D && key[9] == 0x01 && key[10] == 0x03 && key[11] == 0x01 &&
                   key[12] == 0x17 && key[14] == 0x02;
        }

        /// <summary>
        /// Takes the edit rate from a descriptor's sample rate (local tag 3001) or a track's edit
        /// rate (local tag 4B01) - both are an 8-byte rational.
        /// </summary>
        private void ReadEditRate(Stream stream, KlvPacket klv)
        {
            stream.Seek(klv.DataPosition, SeekOrigin.Begin);
            var buffer = new byte[klv.DataSize];
            var read = stream.ReadFully(buffer, 0, buffer.Length);
            var i = 0;
            while (i + 4 <= read)
            {
                var tag = (buffer[i] << 8) | buffer[i + 1];
                var length = (buffer[i + 2] << 8) | buffer[i + 3];
                i += 4;
                if ((tag == 0x3001 || tag == 0x4B01) && length == 8 && i + 8 <= read)
                {
                    var numerator = (buffer[i] << 24) | (buffer[i + 1] << 16) | (buffer[i + 2] << 8) | buffer[i + 3];
                    var denominator = (buffer[i + 4] << 24) | (buffer[i + 5] << 16) | (buffer[i + 6] << 8) | buffer[i + 7];
                    if (numerator > 0 && denominator > 0 && numerator / (double)denominator < 1000)
                    {
                        _editRate = numerator / (double)denominator;
                        return;
                    }
                }

                i += length;
            }
        }

        /// <summary>
        /// One frame-wrapped SMPTE 436M ANC element: a packet count, then per packet line number,
        /// wrapping type, payload sample coding, sample count and the payload array. 8-bit coded
        /// caption distribution packets (DID 0x61, SDID 0x01) are decoded.
        /// </summary>
        private void ReadAncDataElement(Stream stream, KlvPacket klv)
        {
            if (_ancElementKey == null)
            {
                _ancElementKey = klv.Key;
                _closedCaptionDecoder = new ClosedCaptionDecoder();
            }
            else if (!klv.Key.SequenceEqual(_ancElementKey))
            {
                return;
            }

            var frameRate = _editRate > 0 ? _editRate : Configuration.Settings.General.CurrentFrameRate;
            var timeMs = (long)Math.Round(_ancFrameIndex * 1000.0 / frameRate);
            _ancFrameIndex++;

            stream.Seek(klv.DataPosition, SeekOrigin.Begin);
            var buffer = new byte[klv.DataSize];
            var read = stream.ReadFully(buffer, 0, buffer.Length);
            if (read < 2)
            {
                return;
            }

            var ccData = new List<Cea608.CcData>();
            var packetCount = (buffer[0] << 8) | buffer[1];
            var i = 2;
            for (var packet = 0; packet < packetCount && i + 14 <= read; packet++)
            {
                var sampleCoding = buffer[i + 3];
                var arrayCount = (buffer[i + 6] << 24) | (buffer[i + 7] << 16) | (buffer[i + 8] << 8) | buffer[i + 9];
                var arrayElementSize = (buffer[i + 10] << 24) | (buffer[i + 11] << 16) | (buffer[i + 12] << 8) | buffer[i + 13];
                var payloadStart = i + 14;
                var payloadLength = (long)arrayCount * arrayElementSize;
                if (arrayCount < 0 || arrayElementSize < 0 || payloadStart + payloadLength > read)
                {
                    break;
                }

                // 8-bit sample codings: 4 = luma, 5 = color difference, 6 = both, 10-12 = with parity error
                var isEightBit = sampleCoding == 4 || sampleCoding == 5 || sampleCoding == 6 || (sampleCoding >= 10 && sampleCoding <= 12);
                if (isEightBit && arrayElementSize == 1 && payloadLength >= 12 &&
                    buffer[payloadStart] == 0x61 && buffer[payloadStart + 1] == 0x01) // DID/SDID = CEA-708 caption distribution packet
                {
                    ccData.AddRange(GetCcData(buffer, payloadStart, (int)payloadLength));
                }

                // the payload array is padded to a 4-byte boundary
                i = payloadStart + (int)((payloadLength + 3) / 4 * 4);
            }

            _closedCaptionDecoder.AddFrame(timeMs, ccData.ToArray());
        }

        private static IEnumerable<Cea608.CcData> GetCcData(byte[] buffer, int index, int length)
        {
            try
            {
                var packet = new byte[length];
                Array.Copy(buffer, index, packet, 0, length);
                return new Smpte291M(packet).CcDataSectionCcData.CcData
                    .Where(cc => cc.Valid)
                    .Select(cc => new Cea608.CcData(cc.Type, cc.Data1, cc.Data2))
                    .ToList();
            }
            catch
            {
                return Enumerable.Empty<Cea608.CcData>();
            }
        }

        private static bool IsSubtitle(string s)
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

            for (var i = 0; i < count - 11; i++)
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
