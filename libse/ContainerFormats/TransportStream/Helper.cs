using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public static class Helper
    {
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

        public static string GetBinaryString(byte[] buffer, int index, int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(Convert.ToString(buffer[index + i], 2).PadLeft(8, '0'));
            }

            return sb.ToString();
        }
    }
}
