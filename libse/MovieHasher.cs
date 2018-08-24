using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// Hash from OpenSubtitles: http://trac.opensubtitles.org/projects/opensubtitles/wiki/HashSourceCodes
    /// </summary>
    public static class MovieHasher
    {

        public static string GenerateHash(string videoFileName)
        {
            return ToHexadecimal(ComputeMovieHash(videoFileName));
        }

        private static byte[] ComputeMovieHash(string videoFileName)
        {
            byte[] result;
            using (Stream input = new FileStream(videoFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                result = ComputeMovieHash(input);
            }
            return result;
        }

        private static byte[] ComputeMovieHash(Stream input)
        {
            long streamsize = input.Length;
            long lhash = streamsize;

            long i = 0;
            var buffer = new byte[sizeof(long)];
            const int c = 65536;
            while (i < c / sizeof(long) && input.Read(buffer, 0, sizeof(long)) > 0)
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }

            input.Position = Math.Max(0, streamsize - c);
            i = 0;
            while (i < 65536 / sizeof(long) && input.Read(buffer, 0, sizeof(long)) > 0)
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            var result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }

        private static string ToHexadecimal(byte[] bytes)
        {
            var hexBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }
    }
}
