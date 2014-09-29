using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// File related utilities.
    /// </summary>
    internal static class FileUtil
    {
        /// <summary>
        /// Opens an existing file for reading, and allow the user to retry if it fails.
        /// </summary>
        /// <param name="path">The file to be opened for reading. </param>
        /// <returns>A read-only <see cref="FileStream"/> on the specified path.</returns>
        public static FileStream RetryOpenRead(string path)
        {
            FileStream fs = null;
            while (fs == null)
            {
                try
                {
                    fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch (IOException ex)
                {
                    var result = MessageBox.Show(string.Format("An error occured while opening file: {0}", ex.Message), string.Empty, MessageBoxButtons.RetryCancel);
                    if (result == DialogResult.Cancel)
                    {
                        return null;
                    }
                }
            }
            return fs;
        }

        public static bool IsZip(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                fs.Read(buffer, 0, 4);
                return buffer[0] == 0x50  // P
                    && buffer[1] == 0x4B  // K
                    && buffer[2] == 0x03  // (ETX)
                    && buffer[3] == 0x04; // (EOT)
            }
        }

        public static bool IsRar(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                fs.Read(buffer, 0, 4);
                return buffer[0] == 0x52  // R
                    && buffer[1] == 0x61  // a
                    && buffer[2] == 0x72  // r
                    && buffer[3] == 0x21; // !
            }
        }

        public static bool IsBluRaySup(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[2];
                fs.Read(buffer, 0, 2);
                return buffer[0] == 0x50  // P
                    && buffer[1] == 0x47; // G
            }
        }

        public static bool IsTransportStream(string fileName)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var buffer = new byte[3761];
                fs.Read(buffer, 0, 3761);
                return buffer[0] == 0x47 && buffer[188] == 0x47 // 47hex (71 dec or 'G') == TS sync byte
                    || buffer[0] == 0x54 && buffer[1] == 0x46 && buffer[2] == 0x72 && buffer[3760] == 0x47; // Topfield REC TS file
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        public static bool IsM2TransportStream(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var tsp = new Logic.TransportStream.TransportStreamParser();
                tsp.DetectFormat(fs);
                return tsp.IsM2TransportStream;
            }
        }

        public static bool IsVobSub(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[4];
                fs.Read(buffer, 0, 4);
                return Logic.VobSub.VobSubParser.IsMpeg2PackHeader(buffer)
                    || Logic.VobSub.VobSubParser.IsPrivateStream1(buffer, 0);
            }
        }

        public static bool IsSpDvdSup(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[Logic.VobSub.SpHeader.SpHeaderLength];
                if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return false;
                }

                var header = new Logic.VobSub.SpHeader(buffer);
                if (header.Identifier != "SP" || header.NextBlockPosition < 5)
                {
                    return false;
                }

                buffer = new byte[header.NextBlockPosition];
                if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return false;
                }

                buffer = new byte[Logic.VobSub.SpHeader.SpHeaderLength];
                if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                {
                    return false;
                }

                header = new Logic.VobSub.SpHeader(buffer);
                return header.Identifier == "SP";
            }
        }
    }
}
