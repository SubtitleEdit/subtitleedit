using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CaptionDistributionPacket
    {
        public int DataSection { get; set; }
        public bool ProcessEmData { get; set; }
        public bool ProcessCcData { get; set; }
        public bool AdditionalData { get; set; }
        public CcData[] CcData { get; set; }

        public int GetLength()
        {
            return 2 + CcData.Length * 3;
        }

        public CaptionDistributionPacket(byte[] bytes, int index)
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

        public string GetText()
        {
            var hex = new StringBuilder();
            for (var i = 0; i < CcData.Length; i++)
            {
                var cc = CcData[i];
                if (cc.Valid && cc.Type == 2)
                {
                    hex.Append($"{cc.Data1:X2}{cc.Data2:X2}");
                }
            }

           


            //if (hex.ToString() == "8AFF0000")
            //{
            //    return string.Empty;
            //}

            var text = Cea708.Decode(HexStringToByteArray(hex.ToString()));
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
    }
}
