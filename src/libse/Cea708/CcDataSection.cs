using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CcDataSection
    {
        /// <summary>
        /// Should be 0x72 (114).
        /// </summary>
        public int DataSection { get; set; }
        public bool ProcessEmData { get; set; }
        public bool ProcessCcData { get; set; }
        public bool AdditionalData { get; set; }
        public CcData[] CcData { get; set; }

        public int GetLength()
        {
            return 2 + CcData.Length * 3;
        }

        public CcDataSection(byte[] bytes, int index)
        {
            DataSection = bytes[index];
            ProcessEmData = (bytes[index + 1] & 0b10000000) > 0;
            ProcessCcData = (bytes[index + 1] & 0b01000000) > 0;
            AdditionalData = (bytes[index + 1] & 0b00100000) > 0;
            var ccCount = bytes[index + 1] & 0b00011111;
            CcData = new CcData[ccCount];
            for (var i = 0; i < ccCount; i++)
            {
                CcData[i] = new CcData
                {
                    Valid = (bytes[index + i * 3 + 2] & 0b00000100) > 0,
                    Type = bytes[index + i * 3 + 2] & 0b00000011,
                    Data1 = bytes[index + i * 3 + 3],
                    Data2 = bytes[index + i * 3 + 4]
                };
            }
        }

        public CcDataSection(int ccDataCount, byte[] bytes, int sequenceCount)
        {
            DataSection = 0x72;
            ProcessEmData = true;
            ProcessCcData = true;
            AdditionalData = true;

            if (bytes.Length / 2 > 16)
            {
                throw new Exception("Too many bytes for CCData!");
            }

            CcData = new CcData[ccDataCount];
            int bytesIndex = 0;
            var lastContent = true;
            for (int i = 0; i < ccDataCount; i++)
            {
                if (i == 0)
                {
                    CcData[i] = new CcData
                    {
                        Valid = true,
                        Type = 0,
                        Data1 = 0x80,
                        Data2 = 0x80,
                    };
                }
                else if (i == 1)
                {
                    CcData[i] = new CcData
                    {
                        Valid = true,
                        Type = 1,
                        Data1 = 0x80,
                        Data2 = 0x80,
                    };
                }
                else if (i == 2)
                {
                    var rollingSequence = sequenceCount % 4; // rolling sequence 0-3
                    var ccContentLength = bytes.Length / 2 + 2;
                    var sequenceAndLength = (byte)((rollingSequence << 6) + ccContentLength);
                    CcData[i] = new CcData
                    {
                        Valid = true,
                        Type = 3,
                        Data1 = sequenceAndLength,
                        Data2 = 0x00, // What is this?
                    };
                }
                else
                {
                    CcData[i] = new CcData { Type = 2 };
                    if (bytesIndex < bytes.Length)
                    {
                        CcData[i].Valid = true;

                        CcData[i].Data1 = bytes[bytesIndex];
                        bytesIndex++;

                        if (bytesIndex < bytes.Length)
                        {
                            CcData[i].Data2 = bytes[bytesIndex];
                        }

                        bytesIndex++;
                    }
                    else if (lastContent)
                    {
                        CcData[i].Valid = true;
                        lastContent = false;
                    }
                }
            }

        }

        public string GetText(int lineIndex, CommandState state, bool flush)
        {
            var hex = new StringBuilder();
            foreach (var cc in CcData)
            {
                if (cc.Valid && cc.Type == 2)
                {
                    hex.Append($"{cc.Data1:X2}{cc.Data2:X2}");
                }
            }

            var text = Cea708.Decode(lineIndex, HexStringToByteArray(hex.ToString()), state, flush);
            return text;
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        public byte[] GetBytes()
        {
            var ccDataBytes = new List<byte>();
            foreach (var ccData in CcData)
            {
                ccDataBytes.AddRange(ccData.GetBytes());
            }

            return new[]
            {
                (byte)DataSection,
                (byte)((ProcessEmData ? 0b10000000 : 0) |
                       (ProcessCcData ? 0b01000000 : 0) |
                       (AdditionalData ? 0b00100000 : 0) |
                       CcData.Length),
            }.Concat(ccDataBytes).ToArray();
        }
    }
}
