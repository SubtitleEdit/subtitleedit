using Nikse.SubtitleEdit.Core.ContainerFormats;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.VobSub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// File related utilities.
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// Opens a binary file in read/write shared mode, reads the contents of the file into a
        /// byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading. </param>
        /// <returns>A byte array containing the contents of the file.</returns>
        public static byte[] ReadAllBytesShared(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var index = 0;
                var fileLength = fs.Length;
                if (fileLength > int.MaxValue)
                {
                    throw new IOException("File too long");
                }

                var count = (int)fileLength;
                var bytes = new byte[count];
                while (count > 0)
                {
                    var n = fs.Read(bytes, index, count);
                    if (n == 0)
                    {
                        throw new InvalidOperationException("End of file reached before expected");
                    }

                    index += n;
                    count -= n;
                }
                return bytes;
            }
        }

        public static List<string> ReadAllLinesShared(string path, Encoding encoding)
        {
            return encoding.GetString(ReadAllBytesShared(path)).SplitToLines();
        }

        public static string ReadAllTextShared(string path, Encoding encoding)
        {
            return encoding.GetString(ReadAllBytesShared(path));
        }

        public static bool IsZip(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                var count = fs.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                {
                    return false;
                }

                return buffer[0] == 0x50  // P
                    && buffer[1] == 0x4B  // K
                    && buffer[2] == 0x03  // (ETX)
                    && buffer[3] == 0x04; // (EOT)
            }
        }
        public static bool Is7Zip(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[6];
                var count = fs.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                {
                    return false;
                }

                return buffer[0] == 0x37     // 7
                       && buffer[1] == 0x7a  // z
                       && buffer[2] == 0xbc
                       && buffer[3] == 0xaf
                       && buffer[4] == 0x27
                       && buffer[5] == 0x1c;
            }
        }

        public static bool IsRar(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                var count = fs.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                {
                    return false;
                }

                return buffer[0] == 0x52  // R
                    && buffer[1] == 0x61  // a
                    && buffer[2] == 0x72  // r
                    && buffer[3] == 0x21; // !
            }
        }

        public static bool IsPng(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[8];
                var count = fs.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                {
                    return false;
                }

                return buffer[0] == 137
                    && buffer[1] == 80
                    && buffer[2] == 78
                    && buffer[3] == 71
                    && buffer[4] == 13
                    && buffer[5] == 10
                    && buffer[6] == 26
                    && buffer[7] == 10;
            }
        }

        public static bool IsSrr(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[3];
                var count = fs.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                {
                    return false;
                }

                return buffer[0] == 0x69
                    && buffer[1] == 0x69
                    && buffer[2] == 0x69;
            }
        }

        public static bool IsJpg(string fileName)
        {
            // jpeg header - always starts with FFD8 (Start Of Image marker) + FF + a unknown byte (most often E0 or E1 though)
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[3];
                var count = fs.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                {
                    return false;
                }

                return buffer[0] == 0xFF
                    && buffer[1] == 0xD8
                    && buffer[2] == 0xFF;
            }
        }

        public static bool IsTorrentFile(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[11];
                fs.Read(buffer, 0, buffer.Length);
                return Encoding.ASCII.GetString(buffer, 0, buffer.Length) == "d8:announce";
            }
        }

        public static bool IsBluRaySup(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[2];
                fs.Read(buffer, 0, buffer.Length);
                return buffer[0] == 0x50  // P
                    && buffer[1] == 0x47; // G
            }
        }

        public static bool IsTransportStream(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[3761];
                var count = fs.Read(buffer, 0, buffer.Length);
                if (count != buffer.Length)
                {
                    return false;
                }

                // allow for some random bytes in the beginning
                for (int i = 0; i < 255; i++)
                {
                    if (buffer[i] == Packet.SynchronizationByte && buffer[i + 188] == Packet.SynchronizationByte && buffer[i + 188 * 2] == Packet.SynchronizationByte)
                    {
                        return true;
                    }
                }

                return buffer[0] == 0x54 && buffer[1] == 0x46 && buffer[2] == 0x72 && buffer[3760] == Packet.SynchronizationByte; // Topfield REC TS file
            }
        }

        public static bool IsM2TransportStream(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return TransportStreamParser.IsM3TransportStream(fs);
            }
        }

        public static bool IsMpeg2PrivateStream2(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                fs.Read(buffer, 0, buffer.Length);
                return VobSubParser.IsPrivateStream2(buffer, 0);
            }
        }

        public static bool IsVobSub(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                fs.Read(buffer, 0, buffer.Length);
                return VobSubParser.IsMpeg2PackHeader(buffer)
                    || VobSubParser.IsPrivateStream1(buffer, 0);
            }
        }

        public static bool IsSpDvdSup(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[SpHeader.SpHeaderLength];
                if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return false;
                }

                var header = new SpHeader(buffer);
                if (header.Identifier != "SP" || header.NextBlockPosition < 5)
                {
                    return false;
                }

                buffer = new byte[header.NextBlockPosition];
                if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return false;
                }

                buffer = new byte[SpHeader.SpHeaderLength];
                if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return false;
                }

                header = new SpHeader(buffer);
                return header.Identifier == "SP";
            }
        }

        /// <summary>
        /// Checks if file is an MXF file
        /// </summary>
        /// <param name="fileName">Input file</param>
        /// <returns>true if file is an MXF file, otherwise false</returns>
        public static bool IsMaterialExchangeFormat(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[65536];
                var count = fs.Read(buffer, 0, buffer.Length);
                if (count < 100)
                {
                    return false;
                }

                for (int i = 0; i < count - 11; i++)
                {
                    //Header Partition PackId = 06 0E 2B 34 02 05 01 01 0D 01 02
                    if (buffer[i + 00] == 0x06 &&
                        buffer[i + 01] == 0x0E &&
                        buffer[i + 02] == 0x2B &&
                        buffer[i + 03] == 0x34 &&
                        buffer[i + 04] == 0x02 &&
                        buffer[i + 05] == 0x05 &&
                        buffer[i + 06] == 0x01 &&
                        buffer[i + 07] == 0x01 &&
                        buffer[i + 08] == 0x0D &&
                        buffer[i + 09] == 0x01 &&
                        buffer[i + 10] == 0x02)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsRtf(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[10];
                if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return false;
                }

                var text = Encoding.ASCII.GetString(buffer);
                var textUtf8 = Encoding.ASCII.GetString(buffer, 3, 7);
                return text.Trim().StartsWith("{\\rtf1\\", StringComparison.Ordinal) ||
                       textUtf8.Trim().StartsWith("{\\rtf1\\", StringComparison.Ordinal);
            }
        }

        public static bool HasUtf8Bom(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[3];
                fs.Read(buffer, 0, buffer.Length);
                return buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf;
            }
        }

        public static bool IsSubtitleFileAllBinaryZeroes(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length < 10)
                {
                    return false; // too short to be a proper subtitle file
                }

                int numberOfBytes = 1;
                var buffer = new byte[1024];
                while (numberOfBytes > 0)
                {
                    numberOfBytes = fs.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < numberOfBytes; i++)
                    {
                        if (buffer[i] > 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static bool IsFile(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                return false;
            }

            return (File.GetAttributes(path) & FileAttributes.Directory) != FileAttributes.Directory;
        }

        public static bool IsDirectory(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                return false;
            }

            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public static bool IsPlainText(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Length < 20)
            {
                return false; // too short to be plain text
            }

            if (fileInfo.Length > 5000000)
            {
                return false; // too large to be plain text
            }

            var enc = LanguageAutoDetect.GetEncodingFromFile(fileName);
            var s = ReadAllTextShared(fileName, enc);

            int numberCount = 0;
            int letterCount = 0;
            int len = s.Length;

            for (int i = 0; i < len; i++)
            {
                var ch = s[i];
                if (char.IsLetter(ch) || " -,.!?[]()\r\n".Contains(ch))
                {
                    letterCount++;
                }
                else if (char.IsControl(ch) && ch != '\t') // binary found
                {
                    return false;
                }
                else if (CharUtils.IsDigit(ch))
                {
                    numberCount++;
                }
            }
            if (len < 100)
            {
                return numberCount < 5 && letterCount > 20;
            }

            var numberPatternMatches = new Regex(@"\d+[.:,; -]\d+").Matches(s);
            if (numberPatternMatches.Count > 30)
            {
                return false; // looks like time codes
            }

            var largeBlocksOfLargeNumbers = new Regex(@"\d{3,8}").Matches(s);
            if (largeBlocksOfLargeNumbers.Count > 30)
            {
                return false; // looks like time codes
            }

            if (len < 1000 && largeBlocksOfLargeNumbers.Count > 10)
            {
                return false; // looks like time codes
            }

            var partsWithMoreThan100CharsOfNonNumbers = new Regex(@"[^\d]{150,100000}").Matches(s);
            if (partsWithMoreThan100CharsOfNonNumbers.Count > 10)
            {
                return true; // looks like text
            }

            var numberThreshold = len * 0.015 + 25;
            var letterThreshold = len * 0.8;
            return numberCount < numberThreshold && letterCount > letterThreshold;
        }

        public static VideoInfo TryReadVideoInfoViaMatroskaHeader(string fileName)
        {
            var info = new VideoInfo { Success = false };
            using (var matroska = new MatroskaFile(fileName))
            {
                if (matroska.IsValid)
                {
                    matroska.GetInfo(out var frameRate, out var width, out var height, out var milliseconds, out var videoCodec);

                    info.Width = width;
                    info.Height = height;
                    info.FramesPerSecond = frameRate;
                    info.Success = true;
                    info.TotalMilliseconds = milliseconds;
                    info.TotalSeconds = milliseconds / TimeCode.BaseUnit;
                    info.TotalFrames = info.TotalSeconds * frameRate;
                    info.VideoCodec = videoCodec;
                }
            }
            return info;
        }

        public static VideoInfo TryReadVideoInfoViaAviHeader(string fileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                using (var rp = new RiffParser())
                {
                    if (rp.TryOpenFile(fileName) && rp.FileType == RiffParser.CkidAvi)
                    {
                        var dh = new RiffDecodeHeader(rp);
                        dh.ProcessMainAvi();
                        info.FileType = RiffParser.FromFourCc(rp.FileType);
                        info.Width = dh.Width;
                        info.Height = dh.Height;
                        info.FramesPerSecond = dh.FrameRate;
                        info.TotalFrames = dh.TotalFrames;
                        info.TotalMilliseconds = dh.TotalMilliseconds;
                        info.TotalSeconds = info.TotalMilliseconds / TimeCode.BaseUnit;
                        info.VideoCodec = dh.VideoHandler;
                        info.Success = true;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return info;
        }

        public static VideoInfo TryReadVideoInfoViaMp4(string fileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                var mp4Parser = new MP4Parser(fileName);
                if (mp4Parser.Moov != null && mp4Parser.VideoResolution.X > 0)
                {
                    info.Width = mp4Parser.VideoResolution.X;
                    info.Height = mp4Parser.VideoResolution.Y;
                    info.TotalMilliseconds = mp4Parser.Duration.TotalSeconds;
                    info.VideoCodec = "MP4";
                    info.FramesPerSecond = mp4Parser.FrameRate;
                    info.Success = true;
                }
            }
            catch
            {
                // ignored
            }

            return info;
        }

        public static string GetTempFileName(string extension)
        {
            return Path.GetTempPath() + Guid.NewGuid() + extension;
        }

        public static void WriteAllText(string fileName, string contents, TextEncoding encoding)
        {
            if (encoding.DisplayName == TextEncoding.Utf8WithoutBom)
            {
                var outputEnc = new UTF8Encoding(false); // create encoding with no BOM
                using (var file = new StreamWriter(fileName, false, outputEnc)) // open file with encoding
                {
                    file.Write(contents);
                }
            }
            else
            {
                File.WriteAllText(fileName, contents, encoding.Encoding);
            }
        }
    }
}
