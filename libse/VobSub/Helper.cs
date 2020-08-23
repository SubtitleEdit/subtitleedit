using System;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public static class Helper
    {
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

        public static int GetLittleEndian32(byte[] buffer, int index)
        {
            return (buffer[index + 3] << 24 | buffer[index + 2] << 16 | buffer[index + 1] << 8 | buffer[index + 0]);
        }
    }
}
