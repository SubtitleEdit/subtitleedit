using System;
using System.Linq;
using System.Text;
using UtfUnknown;

namespace Nikse.SubtitleEdit.Core.DetectEncoding
{
    public static class EncodingTools
    {
        /// <summary>
        /// Detect the most probable codepage from an byte array
        /// </summary>
        /// <param name="input">array containing the raw data</param>
        /// <returns>the detected encoding or the default encoding if the detection failed</returns>
        public static Encoding DetectInputCodepage(byte[] input)
        {
            var detected = DetectInputCodePages(input, 1);
            return detected.Length > 0 ? detected[0] : Encoding.Default;
        }

        /// <summary>
        /// Returns up to maxEncodings code pages that are assumed to be appropriate
        /// </summary>
        /// <param name="input">array containing the raw data</param>
        /// <param name="maxEncodings">maximum number of encodings to detect</param>
        /// <returns>an array of Encoding with assumed encodings</returns>
        private static Encoding[] DetectInputCodePages(byte[] input, int maxEncodings)
        {
            if (maxEncodings < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxEncodings), "at least one encoding must be returned");
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // empty strings can always be encoded as ASCII
            if (input.Length == 0)
            {
                return new[] { Encoding.ASCII };
            }

            // use UTF.Unknown to detect from input byte string
            var detectionResult = CharsetDetector.DetectFromBytes(input);
            return detectionResult.Details.OrderByDescending(p => p.Confidence).Select(p => p.Encoding).Take(maxEncodings).ToArray();
        }
    }
}
