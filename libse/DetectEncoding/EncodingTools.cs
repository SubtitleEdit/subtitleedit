// Ripped from http://www.codeproject.com/KB/recipes/DetectEncoding.aspx

using Nikse.SubtitleEdit.Core.DetectEncoding.Multilang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Nikse.SubtitleEdit.Core.DetectEncoding
{
    public static class EncodingTools
    {
        // this only contains ascii, default windows code page and unicode
        private static readonly int[] PreferredEncodingsForStream;

        // this contains all codepages, sorted by preference and byte usage
        private static readonly int[] PreferredEncodings;

        /// <summary>
        /// Static constructor that fills the default preferred codepages
        /// </summary>
        static EncodingTools()
        {
            List<int> streamEncodings = new List<int>();
            List<int> allEncodings = new List<int>();
            List<int> mimeEncodings = new List<int>();

            // asscii - most simple so put it in first place...
            streamEncodings.Add(Encoding.ASCII.CodePage);
            mimeEncodings.Add(Encoding.ASCII.CodePage);
            allEncodings.Add(Encoding.ASCII.CodePage);

            // add default 2nd for all encodings
            allEncodings.Add(Encoding.Default.CodePage);
            // default is single byte?
            if (Encoding.Default.IsSingleByte)
            {
                // put it in second place
                streamEncodings.Add(Encoding.Default.CodePage);
                mimeEncodings.Add(Encoding.Default.CodePage);
            }

            // prefer JIS over JIS-SHIFT (JIS is detected better than JIS-SHIFT)
            // this one does include cyrilic (strange but true)
            allEncodings.Add(50220);
            mimeEncodings.Add(50220);

            // always allow unicode flavours for streams (they all have a preamble)
            streamEncodings.Add(Encoding.Unicode.CodePage);
            foreach (var encoding in Configuration.AvailableEncodings)
            {
                if (!streamEncodings.Contains(encoding.CodePage))
                {
                    if (encoding.GetPreamble().Length > 0)
                    {
                        streamEncodings.Add(encoding.CodePage);
                    }
                }
            }

            // stream is done here
            PreferredEncodingsForStream = streamEncodings.ToArray();

            // all singlebyte encodings
            foreach (var encoding in Configuration.AvailableEncodings)
            {
                if (encoding.IsSingleByte)
                {
                    if (!allEncodings.Contains(encoding.CodePage))
                    {
                        allEncodings.Add(encoding.CodePage);
                    }

                    // only add iso and IBM encodings to mime encodings
                    if (encoding.CodePage <= 1258)
                    {
                        mimeEncodings.Add(encoding.CodePage);
                    }
                }
            }

            // add the rest (multibyte)
            foreach (var encoding in Configuration.AvailableEncodings)
            {
                if (!encoding.IsSingleByte)
                {
                    if (!allEncodings.Contains(encoding.CodePage))
                    {
                        allEncodings.Add(encoding.CodePage);
                    }

                    // only add iso and IBM encodings to mime encodings
                    if (encoding.CodePage <= 1258)
                    {
                        mimeEncodings.Add(encoding.CodePage);
                    }
                }
            }

            // add unicodes
            mimeEncodings.Add(Encoding.Unicode.CodePage);

            PreferredEncodings = mimeEncodings.ToArray();
        }

        /// <summary>
        /// Gets the best Encoding for usage in mime encodings
        /// </summary>
        /// <param name="input">text to detect</param>
        /// <returns>the suggested encoding</returns>
        public static Encoding GetMostEfficientEncoding(string input)
        {
            return GetMostEfficientEncoding(input, PreferredEncodings);
        }

        /// <summary>
        /// Gets the best ISO Encoding for usage in a stream
        /// </summary>
        /// <param name="input">text to detect</param>
        /// <returns>the suggested encoding</returns>
        public static Encoding GetMostEfficientEncodingForStream(string input)
        {
            return GetMostEfficientEncoding(input, PreferredEncodingsForStream);
        }

        /// <summary>
        /// Gets the best fitting encoding from a list of possible encodings
        /// </summary>
        /// <param name="input">text to detect</param>
        /// <param name="preferredEncodings">an array of codepages</param>
        /// <returns>the suggested encoding</returns>
        public static Encoding GetMostEfficientEncoding(string input, int[] preferredEncodings)
        {
            Encoding enc = DetectOutgoingEncoding(input, preferredEncodings, true);
            // unicode.. hmmm... check for smallest encoding
            if (enc.CodePage == Encoding.Unicode.CodePage)
            {
                int byteCount = Encoding.UTF7.GetByteCount(input);
                enc = Encoding.UTF7;
                int bestByteCount = byteCount;

                // utf8 smaller?
                byteCount = Encoding.UTF8.GetByteCount(input);
                if (byteCount < bestByteCount)
                {
                    enc = Encoding.UTF8;
                    bestByteCount = byteCount;
                }

                // unicode smaller?
                byteCount = Encoding.Unicode.GetByteCount(input);
                if (byteCount < bestByteCount)
                {
                    enc = Encoding.Unicode;
                }
            }

            return enc;
        }

        public static Encoding DetectOutgoingEncoding(string input)
        {
            return DetectOutgoingEncoding(input, PreferredEncodings, true);
        }

        public static Encoding DetectOutgoingStreamEncoding(string input)
        {
            return DetectOutgoingEncoding(input, PreferredEncodingsForStream, true);
        }

        public static Encoding[] DetectOutgoingEncodings(string input)
        {
            return DetectOutgoingEncodings(input, PreferredEncodings, true);
        }

        public static Encoding[] DetectOutgoingStreamEncodings(string input)
        {
            return DetectOutgoingEncodings(input, PreferredEncodingsForStream, true);
        }

        private static Encoding DetectOutgoingEncoding(string input, int[] preferredEncodings, bool preserveOrder)
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

            Encoding result = Encoding.ASCII;

            // get the IMultiLanguage3 interface
            IMultiLanguage3 multilang3 = new CMultiLanguageClass();
            try
            {
                int[] resultCodePages = new int[preferredEncodings?.Length ?? Encoding.GetEncodings().Length];
                uint detectedCodepages = (uint)resultCodePages.Length;
                ushort specialChar = '?';

                // get unmanaged arrays
                IntPtr pPrefEncs = preferredEncodings == null ? IntPtr.Zero : Marshal.AllocCoTaskMem(sizeof(uint) * preferredEncodings.Length);
                IntPtr pDetectedEncs = Marshal.AllocCoTaskMem(sizeof(uint) * resultCodePages.Length);

                try
                {
                    if (preferredEncodings != null)
                    {
                        Marshal.Copy(preferredEncodings, 0, pPrefEncs, preferredEncodings.Length);
                    }

                    Marshal.Copy(resultCodePages, 0, pDetectedEncs, resultCodePages.Length);

                    MLCPF options = MLCPF.MLDETECTF_VALID_NLS;
                    if (preserveOrder)
                    {
                        options |= MLCPF.MLDETECTF_PRESERVE_ORDER;
                    }

                    if (preferredEncodings != null)
                    {
                        options |= MLCPF.MLDETECTF_PREFERRED_ONLY;
                    }

                    multilang3.DetectOutboundCodePage(options,
                        input, (uint)input.Length,
                        pPrefEncs, (uint)(preferredEncodings?.Length ?? 0),

                        pDetectedEncs, ref detectedCodepages,
                        ref specialChar);

                    // get result
                    if (detectedCodepages > 0)
                    {
                        int[] theResult = new int[detectedCodepages];
                        Marshal.Copy(pDetectedEncs, theResult, 0, theResult.Length);
                        result = Encoding.GetEncoding(theResult[0]);
                    }
                }
                finally
                {
                    if (pPrefEncs != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pPrefEncs);
                    }

                    Marshal.FreeCoTaskMem(pDetectedEncs);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(multilang3);
            }
            return result;
        }

        public static Encoding[] DetectOutgoingEncodings(string input, int[] preferredEncodings, bool preserveOrder)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // empty strings can always be encoded as ASCII
            if (input.Length == 0)
            {
                return new[] { Encoding.ASCII };
            }

            List<Encoding> result = new List<Encoding>();

            // get the IMultiLanguage3 interface
            IMultiLanguage3 multilang3 = new CMultiLanguageClass();
            try
            {
                int[] resultCodePages = new int[preferredEncodings.Length];
                uint detectedCodepages = (uint)resultCodePages.Length;
                ushort specialChar = '?';

                // get unmanaged arrays
                IntPtr pPrefEncs = Marshal.AllocCoTaskMem(sizeof(uint) * preferredEncodings.Length);
                IntPtr pDetectedEncs = Marshal.AllocCoTaskMem(sizeof(uint) * resultCodePages.Length);

                try
                {
                    Marshal.Copy(preferredEncodings, 0, pPrefEncs, preferredEncodings.Length);

                    Marshal.Copy(resultCodePages, 0, pDetectedEncs, resultCodePages.Length);

                    MLCPF options = MLCPF.MLDETECTF_VALID_NLS | MLCPF.MLDETECTF_PREFERRED_ONLY;
                    if (preserveOrder)
                    {
                        options |= MLCPF.MLDETECTF_PRESERVE_ORDER;
                    }

                    options |= MLCPF.MLDETECTF_PREFERRED_ONLY;

                    // finally... call to DetectOutboundCodePage
                    multilang3.DetectOutboundCodePage(options,
                        input, (uint)input.Length,
                        pPrefEncs, (uint)preferredEncodings.Length,
                        pDetectedEncs, ref detectedCodepages,
                        ref specialChar);

                    // get result
                    if (detectedCodepages > 0)
                    {
                        int[] theResult = new int[detectedCodepages];
                        Marshal.Copy(pDetectedEncs, theResult, 0, theResult.Length);

                        // get the encodings for the codepages
                        for (int i = 0; i < detectedCodepages; i++)
                        {
                            result.Add(Encoding.GetEncoding(theResult[i]));
                        }
                    }
                }
                finally
                {
                    if (pPrefEncs != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pPrefEncs);
                    }

                    Marshal.FreeCoTaskMem(pDetectedEncs);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(multilang3);
            }
            // nothing found
            return result.ToArray();
        }

        /// <summary>
        /// Detect the most probable codepage from an byte array
        /// </summary>
        /// <param name="input">array containing the raw data</param>
        /// <returns>the detected encoding or the default encoding if the detection failed</returns>
        public static Encoding DetectInputCodepage(byte[] input)
        {
            try
            {
                Encoding[] detected = DetectInputCodepages(input, 1);

                if (detected.Length > 0)
                {
                    return detected[0];
                }

                return Encoding.Default;
            }
            catch (COMException)
            {
                // return default codepage on error
                return Encoding.Default;
            }
        }

        /// <summary>
        /// Returns up to maxEncodings codepages that are assumed to be apropriate
        /// </summary>
        /// <param name="input">array containing the raw data</param>
        /// <param name="maxEncodings">maxiumum number of encodings to detect</param>
        /// <returns>an array of Encoding with assumed encodings</returns>
        public static Encoding[] DetectInputCodepages(byte[] input, int maxEncodings)
        {
            if (maxEncodings < 1)
            {
                throw new ArgumentOutOfRangeException("maxEncodings", "at least one encoding must be returned");
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

            // expand the string to be at least 256 bytes
            if (input.Length < 256)
            {
                byte[] newInput = new byte[256];
                int steps = 256 / input.Length;
                for (int i = 0; i < steps; i++)
                {
                    Array.Copy(input, 0, newInput, input.Length * i, input.Length);
                }

                int rest = 256 % input.Length;
                if (rest > 0)
                {
                    Array.Copy(input, 0, newInput, steps * input.Length, rest);
                }

                input = newInput;
            }

            List<Encoding> result = new List<Encoding>();

            // get the IMultiLanguage" interface
            IMultiLanguage2 multilang2 = new CMultiLanguageClass();
            try
            {
                DetectEncodingInfo[] detectedEncdings = new DetectEncodingInfo[maxEncodings];

                int scores = detectedEncdings.Length;
                int srcLen = input.Length;

                // setup options (none)
                const MLDETECTCP options = MLDETECTCP.MLDETECTCP_NONE;

                // finally... call to DetectInputCodepage
                multilang2.DetectInputCodepage(options, 0,
                    ref input[0], ref srcLen, ref detectedEncdings[0], ref scores);

                // get result
                if (scores > 0)
                {
                    for (int i = 0; i < scores; i++)
                    {
                        // add the result
                        result.Add(Encoding.GetEncoding((int)detectedEncdings[i].nCodePage));
                    }
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(multilang2);
            }
            // nothing found
            return result.ToArray();
        }

        /// <summary>
        /// Opens a text file and returns the content
        /// encoded in the most probable encoding
        /// </summary>
        /// <param name="path">path to the souce file</param>
        /// <returns>the text content of the file</returns>
        public static string ReadTextFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            using (Stream fs = File.Open(path, FileMode.Open))
            {
                byte[] rawData = new byte[fs.Length];
                Encoding enc = DetectInputCodepage(rawData);
                return enc.GetString(rawData);
            }
        }

        /// <summary>
        /// Returns a stream reader for the given
        /// text file with the best encoding applied
        /// </summary>
        /// <param name="path">path to the file</param>
        /// <returns>a StreamReader for the file</returns>
        public static StreamReader OpenTextFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return OpenTextStream(File.Open(path, FileMode.Open));
        }

        /// <summary>
        /// Creates a stream reader from a stream and detects
        /// the encoding form the first bytes in the stream
        /// </summary>
        /// <param name="stream">a stream to wrap</param>
        /// <returns>the newly created StreamReader</returns>
        public static StreamReader OpenTextStream(Stream stream)
        {
            // check stream parameter
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new ArgumentException("the stream must support seek operations", nameof(stream));
            }

            // assume default encoding at first place

            // seek to stream start
            stream.Seek(0, SeekOrigin.Begin);

            // buffer for preamble and up to 512b sample text for dection
            byte[] buf = new byte[Math.Min(stream.Length, 512)];

            stream.Read(buf, 0, buf.Length);
            var detectedEncoding = DetectInputCodepage(buf);
            // seek back to stream start
            stream.Seek(0, SeekOrigin.Begin);

            return new StreamReader(stream, detectedEncoding);
        }

    }
}
