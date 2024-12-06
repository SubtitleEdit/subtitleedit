using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    public static class GetCcDataHelper
    {
        public static List<CcData> GetCcData(Stream fs, ulong startPos, ulong size)
        {
            var fieldData = new List<CcData>();
            for (var i = startPos; i < startPos + size - 5; i++)
            {
                var buffer = new byte[4];
                fs.Seek((long)i, SeekOrigin.Begin);
                fs.Read(buffer, 0, buffer.Length);
                var nalSize = GetUInt32(buffer, 0);
                var flag = fs.ReadByte();
                if (IsRbspNalUnitType(flag & 0x1F) && nalSize < 10_000)
                {
                    var seiData = GetSeiData(fs, i + 5, i + nalSize + 3);
                    ParseCcDataFromSei(seiData, fieldData);
                }

                i += nalSize + 3;
            }

            return fieldData;
        }

        private static bool IsRbspNalUnitType(int unitType)
        {
            return unitType == 0x06;
        }

        public static byte[] GetSeiData(Stream fs, ulong startPos, ulong endPos)
        {
            var data = new List<byte>();
            var buffer = new byte[endPos - startPos];
            fs.Seek((long)startPos, SeekOrigin.Begin);
            fs.Read(buffer, 0, buffer.Length);

            for (var x = startPos; x < endPos; x++)
            {
                var idx = x - startPos;

                if (x + 2 < endPos && buffer[idx] == 0x00 && buffer[idx + 1] == 0x00 && buffer[idx + 2] == 0x03)
                {
                    data.Add(0x00);
                    data.Add(0x00);
                    x += 2;
                }
                else
                {
                    data.Add(buffer[idx]);
                }
            }

            return data.ToArray();
        }

        public static void ParseCcDataFromSei(byte[] buffer, List<CcData> fieldData)
        {
            var x = 0;
            while (x < buffer.Length -1)
            {
                var payloadType = 0;
                var payloadSize = 0;
                int now;

                do
                {
                    now = buffer[x++];
                    payloadType += now;
                } while (now == 0xFF);

                do
                {
                    now = buffer[x++];
                    payloadSize += now;
                } while (now == 0xFF && x < buffer.Length -1);

                if (IsStartOfCcDataHeader(payloadType, buffer, x))
                {
                    var pos = x + 10;
                    var ccCount = pos + (buffer[pos - 2] & 0x1F) * 3;
                    for (var i = pos; i < ccCount; i += 3)
                    {
                        var b = buffer[i];
                        if ((b & 0x4) > 0)
                        {
                            var ccType = b & 0x3;
                            if (IsCcType(ccType))
                            {
                                var ccData1 = buffer[i + 1];
                                var ccData2 = buffer[i + 2];
                                if (IsNonEmptyCcData(ccData1, ccData2))
                                {
                                    fieldData.Add(new CcData(ccType, ccData1, ccData2));
                                }
                            }
                        }
                    }
                }

                x += payloadSize;
            }
        }

        private static bool IsCcType(int type)
        {
            return type == 0 || type == 1;
        }

        private static bool IsNonEmptyCcData(int ccData1, int ccData2)
        {
            return (ccData1 & 0x7f) > 0 || (ccData2 & 0x7f) > 0;
        }

        private static bool IsStartOfCcDataHeader(int payloadType, byte[] buffer, int pos)
        {
            return payloadType == 4 &&
                   GetUInt32(buffer, pos) == 3036688711 &&
                   GetUInt32(buffer, pos + 4) == 1094267907;
        }

        private static uint GetUInt32(byte[] buffer, int pos)
        {
            return (uint)((buffer[pos + 0] << 24) + (buffer[pos + 1] << 16) + (buffer[pos + 2] << 8) + buffer[pos + 3]);
        }
    }
}
