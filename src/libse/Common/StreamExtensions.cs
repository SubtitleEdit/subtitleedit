using System.IO;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads <paramref name="count"/> bytes from the stream into <paramref name="buffer"/>,
        /// looping until the requested count is read or the end of the stream is reached.
        /// Unlike a single <see cref="Stream.Read(byte[], int, int)"/> call, this never returns
        /// fewer bytes than requested while more data is available.
        /// </summary>
        /// <returns>The total number of bytes read; less than <paramref name="count"/> only at end of stream.</returns>
        public static int ReadFully(this Stream stream, byte[] buffer, int offset, int count)
        {
            var total = 0;
            while (total < count)
            {
                var bytesRead = stream.Read(buffer, offset + total, count - total);
                if (bytesRead == 0)
                {
                    break;
                }

                total += bytesRead;
            }

            return total;
        }
    }
}
