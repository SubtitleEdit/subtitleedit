using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public static class Helper
    {

        #region Binary constants

        public const int B00000000 = 0;
        public const int B00000001 = 1;
        public const int B00000010 = 2;
        public const int B00000011 = 3;
        public const int B00000100 = 4;
        public const int B00000101 = 5;
        public const int B00000110 = 6;
        public const int B00000111 = 7;
        public const int B00001000 = 8;
        public const int B00001001 = 9;
        public const int B00001010 = 10;
        public const int B00001011 = 11;
        public const int B00001100 = 12;
        public const int B00001101 = 13;
        public const int B00001110 = 14;
        public const int B00001111 = 15;
        public const int B00010000 = 16;
        public const int B00010001 = 17;
        public const int B00010010 = 18;
        public const int B00010011 = 19;
        public const int B00010100 = 20;
        public const int B00010101 = 21;
        public const int B00010110 = 22;
        public const int B00010111 = 23;
        public const int B00011000 = 24;
        public const int B00011001 = 25;
        public const int B00011010 = 26;
        public const int B00011011 = 27;
        public const int B00011100 = 28;
        public const int B00011101 = 29;
        public const int B00011110 = 30;
        public const int B00011111 = 31;
        public const int B00100000 = 32;
        public const int B00100001 = 33;
        public const int B00100010 = 34;
        public const int B00100011 = 35;
        public const int B00100100 = 36;
        public const int B00100101 = 37;
        public const int B00100110 = 38;
        public const int B00100111 = 39;
        public const int B00101000 = 40;
        public const int B00101001 = 41;
        public const int B00101010 = 42;
        public const int B00101011 = 43;
        public const int B00101100 = 44;
        public const int B00101101 = 45;
        public const int B00101110 = 46;
        public const int B00101111 = 47;
        public const int B00110000 = 48;
        public const int B00110001 = 49;
        public const int B00110010 = 50;
        public const int B00110011 = 51;
        public const int B00110100 = 52;
        public const int B00110101 = 53;
        public const int B00110110 = 54;
        public const int B00110111 = 55;
        public const int B00111000 = 56;
        public const int B00111001 = 57;
        public const int B00111010 = 58;
        public const int B00111011 = 59;
        public const int B00111100 = 60;
        public const int B00111101 = 61;
        public const int B00111110 = 62;
        public const int B00111111 = 63;
        public const int B01000000 = 64;
        public const int B01000001 = 65;
        public const int B01000010 = 66;
        public const int B01000011 = 67;
        public const int B01000100 = 68;
        public const int B01000101 = 69;
        public const int B01000110 = 70;
        public const int B01000111 = 71;
        public const int B01001000 = 72;
        public const int B01001001 = 73;
        public const int B01001010 = 74;
        public const int B01001011 = 75;
        public const int B01001100 = 76;
        public const int B01001101 = 77;
        public const int B01001110 = 78;
        public const int B01001111 = 79;
        public const int B01010000 = 80;
        public const int B01010001 = 81;
        public const int B01010010 = 82;
        public const int B01010011 = 83;
        public const int B01010100 = 84;
        public const int B01010101 = 85;
        public const int B01010110 = 86;
        public const int B01010111 = 87;
        public const int B01011000 = 88;
        public const int B01011001 = 89;
        public const int B01011010 = 90;
        public const int B01011011 = 91;
        public const int B01011100 = 92;
        public const int B01011101 = 93;
        public const int B01011110 = 94;
        public const int B01011111 = 95;
        public const int B01100000 = 96;
        public const int B01100001 = 97;
        public const int B01100010 = 98;
        public const int B01100011 = 99;
        public const int B01100100 = 100;
        public const int B01100101 = 101;
        public const int B01100110 = 102;
        public const int B01100111 = 103;
        public const int B01101000 = 104;
        public const int B01101001 = 105;
        public const int B01101010 = 106;
        public const int B01101011 = 107;
        public const int B01101100 = 108;
        public const int B01101101 = 109;
        public const int B01101110 = 110;
        public const int B01101111 = 111;
        public const int B01110000 = 112;
        public const int B01110001 = 113;
        public const int B01110010 = 114;
        public const int B01110011 = 115;
        public const int B01110100 = 116;
        public const int B01110101 = 117;
        public const int B01110110 = 118;
        public const int B01110111 = 119;
        public const int B01111000 = 120;
        public const int B01111001 = 121;
        public const int B01111010 = 122;
        public const int B01111011 = 123;
        public const int B01111100 = 124;
        public const int B01111101 = 125;
        public const int B01111110 = 126;
        public const int B01111111 = 127;
        public const int B10000000 = 128;
        public const int B10000001 = 129;
        public const int B10000010 = 130;
        public const int B10000011 = 131;
        public const int B10000100 = 132;
        public const int B10000101 = 133;
        public const int B10000110 = 134;
        public const int B10000111 = 135;
        public const int B10001000 = 136;
        public const int B10001001 = 137;
        public const int B10001010 = 138;
        public const int B10001011 = 139;
        public const int B10001100 = 140;
        public const int B10001101 = 141;
        public const int B10001110 = 142;
        public const int B10001111 = 143;
        public const int B10010000 = 144;
        public const int B10010001 = 145;
        public const int B10010010 = 146;
        public const int B10010011 = 147;
        public const int B10010100 = 148;
        public const int B10010101 = 149;
        public const int B10010110 = 150;
        public const int B10010111 = 151;
        public const int B10011000 = 152;
        public const int B10011001 = 153;
        public const int B10011010 = 154;
        public const int B10011011 = 155;
        public const int B10011100 = 156;
        public const int B10011101 = 157;
        public const int B10011110 = 158;
        public const int B10011111 = 159;
        public const int B10100000 = 160;
        public const int B10100001 = 161;
        public const int B10100010 = 162;
        public const int B10100011 = 163;
        public const int B10100100 = 164;
        public const int B10100101 = 165;
        public const int B10100110 = 166;
        public const int B10100111 = 167;
        public const int B10101000 = 168;
        public const int B10101001 = 169;
        public const int B10101010 = 170;
        public const int B10101011 = 171;
        public const int B10101100 = 172;
        public const int B10101101 = 173;
        public const int B10101110 = 174;
        public const int B10101111 = 175;
        public const int B10110000 = 176;
        public const int B10110001 = 177;
        public const int B10110010 = 178;
        public const int B10110011 = 179;
        public const int B10110100 = 180;
        public const int B10110101 = 181;
        public const int B10110110 = 182;
        public const int B10110111 = 183;
        public const int B10111000 = 184;
        public const int B10111001 = 185;
        public const int B10111010 = 186;
        public const int B10111011 = 187;
        public const int B10111100 = 188;
        public const int B10111101 = 189;
        public const int B10111110 = 190;
        public const int B10111111 = 191;
        public const int B11000000 = 192;
        public const int B11000001 = 193;
        public const int B11000010 = 194;
        public const int B11000011 = 195;
        public const int B11000100 = 196;
        public const int B11000101 = 197;
        public const int B11000110 = 198;
        public const int B11000111 = 199;
        public const int B11001000 = 200;
        public const int B11001001 = 201;
        public const int B11001010 = 202;
        public const int B11001011 = 203;
        public const int B11001100 = 204;
        public const int B11001101 = 205;
        public const int B11001110 = 206;
        public const int B11001111 = 207;
        public const int B11010000 = 208;
        public const int B11010001 = 209;
        public const int B11010010 = 210;
        public const int B11010011 = 211;
        public const int B11010100 = 212;
        public const int B11010101 = 213;
        public const int B11010110 = 214;
        public const int B11010111 = 215;
        public const int B11011000 = 216;
        public const int B11011001 = 217;
        public const int B11011010 = 218;
        public const int B11011011 = 219;
        public const int B11011100 = 220;
        public const int B11011101 = 221;
        public const int B11011110 = 222;
        public const int B11011111 = 223;
        public const int B11100000 = 224;
        public const int B11100001 = 225;
        public const int B11100010 = 226;
        public const int B11100011 = 227;
        public const int B11100100 = 228;
        public const int B11100101 = 229;
        public const int B11100110 = 230;
        public const int B11100111 = 231;
        public const int B11101000 = 232;
        public const int B11101001 = 233;
        public const int B11101010 = 234;
        public const int B11101011 = 235;
        public const int B11101100 = 236;
        public const int B11101101 = 237;
        public const int B11101110 = 238;
        public const int B11101111 = 239;
        public const int B11110000 = 240;
        public const int B11110001 = 241;
        public const int B11110010 = 242;
        public const int B11110011 = 243;
        public const int B11110100 = 244;
        public const int B11110101 = 245;
        public const int B11110110 = 246;
        public const int B11110111 = 247;
        public const int B11111000 = 248;
        public const int B11111001 = 249;
        public const int B11111010 = 250;
        public const int B11111011 = 251;
        public const int B11111100 = 252;
        public const int B11111101 = 253;
        public const int B11111110 = 254;
        public const int B11111111 = 255;

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
