using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Encoding and decoding bytes to/from text. 
    /// Used for attachments in Advanced Sub Station alpha files.
    /// It originated for use between users of UNIX systems (it's name stood for "UNIX-to-UNIX encoding").
    /// See https://en.wikipedia.org/wiki/Uuencoding.
    /// </summary>
    public static class UUEncoding
    {
        /// <summary>
        /// UUEncode of bytes to text.
        /// </summary>
        /// <param name="bytes">Bytes to encode as text</param>
        /// <returns>Bytes encoded as text</returns>
        public static string UUEncode(byte[] bytes)
        {
            int i = 0;
            int lineElements = 0;
            var length = bytes.Length;
            var dest = new byte[4];
            var src = new byte[3];
            var sb = new StringBuilder();
            while (i < length)
            {
                if (i + 3 > length)
                {
                    src = new byte[3];
                    Array.Copy(bytes, i, src, 0, i + 2 - length);
                }
                else
                {
                    Array.Copy(bytes, i, src, 0, 3);
                }

                dest[0] = (byte)(src[0] >> 2);
                dest[1] = (byte)(((src[0] & 0x3) << 4) | ((src[1] & 0xF0) >> 4));
                dest[2] = (byte)(((src[1] & 0xF) << 2) | ((src[2] & 0xC0) >> 6));
                dest[3] = (byte)(src[2] & 0x3F);

                dest[0] += 33;
                dest[1] += 33;
                dest[2] += 33;
                dest[3] += 33;

                sb.Append(Encoding.ASCII.GetString(dest));

                lineElements += 4;
                if (lineElements == 80)
                {
                    sb.AppendLine();
                    lineElements = 0;
                }

                i += 3;
            }

            return sb.ToString();
        }

        /// <summary>
        /// UUDecode of text to bytes.
        /// </summary>
        /// <param name="text">Text to decode to bytes</param>
        /// <returns>Bytes decoded from text</returns>
        public static byte[] UUDecode(string text)
        {
            var len = text.Length;
            var byteArray = new byte[len];
            var bytearrayPosition = 0;
            for (var pos = 0; pos + 1 < len;)
            {
                var bytes = 0;
                var src = new byte[4];
                for (var i = 0; i < 4 && pos < len; ++pos)
                {
                    var c = text[pos];
                    if (c != '\n' && c != '\r')
                    {
                        src[i++] = (byte)(Convert.ToByte(c) - 33);
                        bytes++;
                    }
                }

                if (bytes > 1)
                {
                    byteArray[bytearrayPosition++] = (byte)((src[0] << 2) | (src[1] >> 4));
                }

                if (bytes > 2)
                {
                    byteArray[bytearrayPosition++] = (byte)(((src[1] & 0xF) << 4) | (src[2] >> 2));
                }

                if (bytes > 3)
                {
                    byteArray[bytearrayPosition++] = (byte)(((src[2] & 0x3) << 6) | (src[3]));
                }
            }

            Array.Resize(ref byteArray, bytearrayPosition);
            return byteArray;
        }
    }
}
