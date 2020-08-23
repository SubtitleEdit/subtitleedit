using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public static class Helper
    {

        #region Binary constants

        public static byte B00000000 = 0;
        public static byte B00000001 = 1;
        public static byte B00000010 = 2;
        public static byte B00000011 = 3;
        public static byte B00000100 = 4;
        public static byte B00000101 = 5;
        public static byte B00000110 = 6;
        public static byte B00000111 = 7;
        public static byte B00001000 = 8;
        public static byte B00001001 = 9;
        public static byte B00001010 = 10;
        public static byte B00001011 = 11;
        public static byte B00001100 = 12;
        public static byte B00001101 = 13;
        public static byte B00001110 = 14;
        public static byte B00001111 = 15;
        public static byte B00010000 = 16;
        public static byte B00010001 = 17;
        public static byte B00010010 = 18;
        public static byte B00010011 = 19;
        public static byte B00010100 = 20;
        public static byte B00010101 = 21;
        public static byte B00010110 = 22;
        public static byte B00010111 = 23;
        public static byte B00011000 = 24;
        public static byte B00011001 = 25;
        public static byte B00011010 = 26;
        public static byte B00011011 = 27;
        public static byte B00011100 = 28;
        public static byte B00011101 = 29;
        public static byte B00011110 = 30;
        public static byte B00011111 = 31;
        public static byte B00100000 = 32;
        public static byte B00100001 = 33;
        public static byte B00100010 = 34;
        public static byte B00100011 = 35;
        public static byte B00100100 = 36;
        public static byte B00100101 = 37;
        public static byte B00100110 = 38;
        public static byte B00100111 = 39;
        public static byte B00101000 = 40;
        public static byte B00101001 = 41;
        public static byte B00101010 = 42;
        public static byte B00101011 = 43;
        public static byte B00101100 = 44;
        public static byte B00101101 = 45;
        public static byte B00101110 = 46;
        public static byte B00101111 = 47;
        public static byte B00110000 = 48;
        public static byte B00110001 = 49;
        public static byte B00110010 = 50;
        public static byte B00110011 = 51;
        public static byte B00110100 = 52;
        public static byte B00110101 = 53;
        public static byte B00110110 = 54;
        public static byte B00110111 = 55;
        public static byte B00111000 = 56;
        public static byte B00111001 = 57;
        public static byte B00111010 = 58;
        public static byte B00111011 = 59;
        public static byte B00111100 = 60;
        public static byte B00111101 = 61;
        public static byte B00111110 = 62;
        public static byte B00111111 = 63;
        public static byte B01000000 = 64;
        public static byte B01000001 = 65;
        public static byte B01000010 = 66;
        public static byte B01000011 = 67;
        public static byte B01000100 = 68;
        public static byte B01000101 = 69;
        public static byte B01000110 = 70;
        public static byte B01000111 = 71;
        public static byte B01001000 = 72;
        public static byte B01001001 = 73;
        public static byte B01001010 = 74;
        public static byte B01001011 = 75;
        public static byte B01001100 = 76;
        public static byte B01001101 = 77;
        public static byte B01001110 = 78;
        public static byte B01001111 = 79;
        public static byte B01010000 = 80;
        public static byte B01010001 = 81;
        public static byte B01010010 = 82;
        public static byte B01010011 = 83;
        public static byte B01010100 = 84;
        public static byte B01010101 = 85;
        public static byte B01010110 = 86;
        public static byte B01010111 = 87;
        public static byte B01011000 = 88;
        public static byte B01011001 = 89;
        public static byte B01011010 = 90;
        public static byte B01011011 = 91;
        public static byte B01011100 = 92;
        public static byte B01011101 = 93;
        public static byte B01011110 = 94;
        public static byte B01011111 = 95;
        public static byte B01100000 = 96;
        public static byte B01100001 = 97;
        public static byte B01100010 = 98;
        public static byte B01100011 = 99;
        public static byte B01100100 = 100;
        public static byte B01100101 = 101;
        public static byte B01100110 = 102;
        public static byte B01100111 = 103;
        public static byte B01101000 = 104;
        public static byte B01101001 = 105;
        public static byte B01101010 = 106;
        public static byte B01101011 = 107;
        public static byte B01101100 = 108;
        public static byte B01101101 = 109;
        public static byte B01101110 = 110;
        public static byte B01101111 = 111;
        public static byte B01110000 = 112;
        public static byte B01110001 = 113;
        public static byte B01110010 = 114;
        public static byte B01110011 = 115;
        public static byte B01110100 = 116;
        public static byte B01110101 = 117;
        public static byte B01110110 = 118;
        public static byte B01110111 = 119;
        public static byte B01111000 = 120;
        public static byte B01111001 = 121;
        public static byte B01111010 = 122;
        public static byte B01111011 = 123;
        public static byte B01111100 = 124;
        public static byte B01111101 = 125;
        public static byte B01111110 = 126;
        public static byte B01111111 = 127;
        public static byte B10000000 = 128;
        public static byte B10000001 = 129;
        public static byte B10000010 = 130;
        public static byte B10000011 = 131;
        public static byte B10000100 = 132;
        public static byte B10000101 = 133;
        public static byte B10000110 = 134;
        public static byte B10000111 = 135;
        public static byte B10001000 = 136;
        public static byte B10001001 = 137;
        public static byte B10001010 = 138;
        public static byte B10001011 = 139;
        public static byte B10001100 = 140;
        public static byte B10001101 = 141;
        public static byte B10001110 = 142;
        public static byte B10001111 = 143;
        public static byte B10010000 = 144;
        public static byte B10010001 = 145;
        public static byte B10010010 = 146;
        public static byte B10010011 = 147;
        public static byte B10010100 = 148;
        public static byte B10010101 = 149;
        public static byte B10010110 = 150;
        public static byte B10010111 = 151;
        public static byte B10011000 = 152;
        public static byte B10011001 = 153;
        public static byte B10011010 = 154;
        public static byte B10011011 = 155;
        public static byte B10011100 = 156;
        public static byte B10011101 = 157;
        public static byte B10011110 = 158;
        public static byte B10011111 = 159;
        public static byte B10100000 = 160;
        public static byte B10100001 = 161;
        public static byte B10100010 = 162;
        public static byte B10100011 = 163;
        public static byte B10100100 = 164;
        public static byte B10100101 = 165;
        public static byte B10100110 = 166;
        public static byte B10100111 = 167;
        public static byte B10101000 = 168;
        public static byte B10101001 = 169;
        public static byte B10101010 = 170;
        public static byte B10101011 = 171;
        public static byte B10101100 = 172;
        public static byte B10101101 = 173;
        public static byte B10101110 = 174;
        public static byte B10101111 = 175;
        public static byte B10110000 = 176;
        public static byte B10110001 = 177;
        public static byte B10110010 = 178;
        public static byte B10110011 = 179;
        public static byte B10110100 = 180;
        public static byte B10110101 = 181;
        public static byte B10110110 = 182;
        public static byte B10110111 = 183;
        public static byte B10111000 = 184;
        public static byte B10111001 = 185;
        public static byte B10111010 = 186;
        public static byte B10111011 = 187;
        public static byte B10111100 = 188;
        public static byte B10111101 = 189;
        public static byte B10111110 = 190;
        public static byte B10111111 = 191;
        public static byte B11000000 = 192;
        public static byte B11000001 = 193;
        public static byte B11000010 = 194;
        public static byte B11000011 = 195;
        public static byte B11000100 = 196;
        public static byte B11000101 = 197;
        public static byte B11000110 = 198;
        public static byte B11000111 = 199;
        public static byte B11001000 = 200;
        public static byte B11001001 = 201;
        public static byte B11001010 = 202;
        public static byte B11001011 = 203;
        public static byte B11001100 = 204;
        public static byte B11001101 = 205;
        public static byte B11001110 = 206;
        public static byte B11001111 = 207;
        public static byte B11010000 = 208;
        public static byte B11010001 = 209;
        public static byte B11010010 = 210;
        public static byte B11010011 = 211;
        public static byte B11010100 = 212;
        public static byte B11010101 = 213;
        public static byte B11010110 = 214;
        public static byte B11010111 = 215;
        public static byte B11011000 = 216;
        public static byte B11011001 = 217;
        public static byte B11011010 = 218;
        public static byte B11011011 = 219;
        public static byte B11011100 = 220;
        public static byte B11011101 = 221;
        public static byte B11011110 = 222;
        public static byte B11011111 = 223;
        public static byte B11100000 = 224;
        public static byte B11100001 = 225;
        public static byte B11100010 = 226;
        public static byte B11100011 = 227;
        public static byte B11100100 = 228;
        public static byte B11100101 = 229;
        public static byte B11100110 = 230;
        public static byte B11100111 = 231;
        public static byte B11101000 = 232;
        public static byte B11101001 = 233;
        public static byte B11101010 = 234;
        public static byte B11101011 = 235;
        public static byte B11101100 = 236;
        public static byte B11101101 = 237;
        public static byte B11101110 = 238;
        public static byte B11101111 = 239;
        public static byte B11110000 = 240;
        public static byte B11110001 = 241;
        public static byte B11110010 = 242;
        public static byte B11110011 = 243;
        public static byte B11110100 = 244;
        public static byte B11110101 = 245;
        public static byte B11110110 = 246;
        public static byte B11110111 = 247;
        public static byte B11111000 = 248;
        public static byte B11111001 = 249;
        public static byte B11111010 = 250;
        public static byte B11111011 = 251;
        public static byte B11111100 = 252;
        public static byte B11111101 = 253;
        public static byte B11111110 = 254;
        public static byte B11111111 = 255;

        #endregion Binary constants

        public static string IntToHex(UInt64 value, int digits)
        {
            return value.ToString("X").PadLeft(digits, '0');
        }

        public static string IntToHex(int value, int digits)
        {
            return value.ToString("X").PadLeft(digits, '0');
        }

        public static string IntToBin(long value, int digits)
        {
            return Convert.ToString(value, 2).PadLeft(digits, '0');
        }

        public static UInt32 GetEndian(byte[] buffer, int index, int count)
        {
            UInt32 result = 0;
            for (int i = 0; i < count; i++)
            {
                result = (result << 8) + buffer[index + i];
            }

            return result;
        }

        public static int GetLittleEndian32(byte[] buffer, int index)
        {
            return (buffer[index + 3] << 24 | (int)buffer[index + 2] << 16 | (int)buffer[index + 1] << 8 | (int)buffer[index + 0]);
        }

        //private int Swap4Bytes(byte[] b)
        //{
        //    return ((int)b[3] << 24 | (int)b[2] << 16 | (int)b[1] << 8 | (int)b[0]);
        //}

        /// <summary>
        /// Get two bytes word stored in endian order
        /// </summary>
        /// <param name="buffer">Byte array</param>
        /// <param name="index">Index in byte array</param>
        /// <returns>Word as int</returns>
        public static int GetEndianWord(byte[] buffer, int index)
        {
            if (index + 1 < buffer.Length)
            {
                return (buffer[index] << 8) | buffer[index + 1];
            }

            return 0;
        }

        public static string GetBinaryString(byte[] buffer, int index, int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(Convert.ToString(buffer[index + i], 2).PadLeft(8, '0'));
            }

            return sb.ToString();
        }

        public static UInt64 GetUInt32FromBinaryString(string binaryValue)
        {
            return Convert.ToUInt32(binaryValue, 2);
        }

    }
}
