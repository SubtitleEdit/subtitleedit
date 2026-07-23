using System;
using System.Text;
using UtfUnknown;

namespace Nikse.SubtitleEdit.Core.DetectEncoding
{
    public static class EncodingTools
    {
        /// <summary>
        /// Detects the most probable code page from a byte array.
        /// </summary>
        /// <param name="input">Array containing the raw data.</param>
        /// <returns>The detected encoding, or <see cref="Encoding.Default"/> if detection fails.</returns>
        public static Encoding DetectInputCodepage(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // empty input can always be encoded as ASCII
            if (input.Length == 0)
            {
                return Encoding.ASCII;
            }

            // UTF.Unknown orders the detection details by confidence, so Detected is the best match (null if none)
            return CharsetDetector.DetectFromBytes(input).Detected?.Encoding ?? Encoding.Default;
        }
    }
}
