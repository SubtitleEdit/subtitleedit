using System;
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
            return DetectInputCodePages(input) ?? Encoding.Default;
        }

        /// <summary>
        /// Returns the most probable encoding from a byte array.
        /// </summary>
        /// <param name="input">Array containing the raw data.</param>
        /// <returns>The detected encoding with the highest confidence, or ASCII encoding if no confident detection is found.</returns>
        private static Encoding DetectInputCodePages(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // empty strings can always be encoded as ASCII
            if (input.Length == 0)
            {
                return Encoding.ASCII;
            }

            // use UTF.Unknown to detect from input byte string
            var detectionResult = CharsetDetector.DetectFromBytes(input);

            var confidence = 0f;
            Encoding encoding = Encoding.ASCII;
            for (var i = 0; i < detectionResult.Details.Count; i++)
            {
                if (detectionResult.Details[i].Confidence > confidence)
                {
                    confidence = detectionResult.Details[i].Confidence;
                    encoding = detectionResult.Details[i].Encoding;
                }
            }

            return encoding;
        }
    }
}
