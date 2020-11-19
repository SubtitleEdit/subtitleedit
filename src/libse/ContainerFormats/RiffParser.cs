// (c) Giora Tamir (giora@gtamir.com), 2005

using System;
using System.IO;
using System.Runtime.Serialization;

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    #region RiffParserException

    [Serializable]
    public class RiffParserException : ApplicationException
    {
        public RiffParserException()
        {
        }

        public RiffParserException(string message)
            : base(message)
        {
        }

        public RiffParserException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public RiffParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    #endregion RiffParserException

    public class RiffParser : IDisposable
    {
        #region CONSTANTS

        public const int DWordSize = 4;
        public const int TwoDWordSize = 8;
        public const string Riff4Cc = "RIFF";
        public const string Rifx4Cc = "RIFX";
        public const string List4Cc = "LIST";

        // Known file types
        public static readonly int CkidAvi = ToFourCc("AVI ");
        public static readonly int CkidWav = ToFourCc("WAVE");
        public static readonly int CkidRmid = ToFourCc("RMID");

        #endregion CONSTANTS

        #region private members

        private string _fileName;
        private string _shortName;
        private long _fileSize;
        private int _dataSize;
        private FileStream _stream;
        private int _fileRiff;
        private int _fileType;

        #endregion private members

        #region Delegates

        /// <summary>
        /// Method to be called when a list element is found
        /// </summary>
        /// <param name="fourCcType"></param>
        /// <param name="length"></param>
        public delegate void ProcessListElement(RiffParser rp, int fourCcType, int length);

        /// <summary>
        /// Method to be called when a chunk element is found
        /// </summary>
        /// <param name="fourCcType"></param>
        /// <param name="unpaddedLength"></param>
        /// <param name="paddedLength"></param>
        public delegate void ProcessChunkElement(RiffParser rp, int fourCcType, int unpaddedLength, int paddedLength);

        #endregion Delegates

        #region public Members

        /// <summary>
        /// RIFF data segment size
        /// </summary>
        public int DataSize
        {
            get
            {
                return _dataSize;
            }
        }

        /// <summary>
        /// Current file name
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        /// <summary>
        /// Current short (name only) file name
        /// </summary>
        public string ShortName
        {
            get
            {
                return _shortName;
            }
        }

        /// <summary>
        /// Return the general file type (RIFF or RIFX);
        /// </summary>
        public int FileRiff
        {
            get
            {
                return _fileRiff;
            }
        }

        /// <summary>
        /// Return the specific file type (AVI/WAV...)
        /// </summary>
        public int FileType
        {
            get
            {
                return _fileType;
            }
        }

        #endregion public Members

        /// <summary>
        /// Determine if the file is a valid RIFF file
        /// </summary>
        /// <param name="filename">File to examine</param>
        /// <returns>True if file is a RIFF file</returns>
        public bool TryOpenFile(string filename)
        {
            // Sanity check
            if (null != _stream)
            {
                return false;
            }

            bool errorOccured = false;

            // Opening a new file
            try
            {
                FileInfo fi = new FileInfo(filename);
                _fileName = fi.FullName;
                _shortName = fi.Name;
                _fileSize = fi.Length;

                // Read the RIFF header
                _stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                int fourCc;
                int datasize;
                int fileType;

                ReadTwoInts(out fourCc, out datasize);
                ReadOneInt(out fileType);

                _fileRiff = fourCc;
                _fileType = fileType;

                // Check for a valid RIFF header
                string riff = FromFourCc(fourCc);
                if (riff != Riff4Cc && riff != Rifx4Cc)
                {
                    // Not a valid RIFF file
                    errorOccured = true;
                    return false;
                }

                // Good header. Check size
                _dataSize = datasize;
                if (_fileSize < _dataSize + TwoDWordSize)
                {
                    // Truncated file
                    errorOccured = true;
                    return false;
                }
            }
            catch
            {
                errorOccured = true;
            }
            finally
            {
                if (errorOccured && (null != _stream))
                {
                    _stream.Close();
                    _stream.Dispose();
                    _stream = null;
                }
            }

            return !errorOccured;
        }

        /// <summary>
        /// Read the next RIFF element invoking the correct delegate.
        /// Returns true if an element can be read
        /// </summary>
        /// <param name="bytesleft">Reference to number of bytes left in the current list</param>
        /// <param name="chunk">Method to invoke if a chunk is found</param>
        /// <param name="list">Method to invoke if a list is found</param>
        /// <returns></returns>
        public bool ReadElement(ref int bytesleft, ProcessChunkElement chunk, ProcessListElement list)
        {
            // Are we done?
            if (TwoDWordSize > bytesleft)
            {
                return false;
            }

            // We have enough bytes, read
            int fourCc;
            int size;

            ReadTwoInts(out fourCc, out size);

            // Reduce bytes left
            bytesleft -= TwoDWordSize;

            // Do we have enough bytes?
            if (bytesleft < size)
            {
                // Skip the bad data and throw an exception
                SkipData(bytesleft);
                bytesleft = 0;
                throw new RiffParserException("Element size mismatch for element " + FromFourCc(fourCc)
                + " need " + size + " but have only " + bytesleft);
            }

            // Examine the element, is it a list or a chunk
            string type = FromFourCc(fourCc);
            if (type == List4Cc)
            {
                // We have a list
                ReadOneInt(out fourCc);

                if (null == list)
                {
                    SkipData(size - 4);
                }
                else
                {
                    // Invoke the list method
                    list(this, fourCc, size - 4);
                }

                // Adjust size
                bytesleft -= size;
            }
            else
            {
                // Calculated padded size - padded to WORD boundary
                int paddedSize = size;
                if (0 != (size & 1))
                {
                    ++paddedSize;
                }

                if (null == chunk)
                {
                    SkipData(paddedSize);
                }
                else
                {
                    chunk(this, fourCc, size, paddedSize);
                }

                // Adjust size
                bytesleft -= paddedSize;
            }

            return true;
        }

        #region Stream access

        /// <summary>
        /// Non-thread-safe method to read two ints from the stream
        /// </summary>
        /// <param name="fourCc">Output FourCC int</param>
        /// <param name="size">Output chunk/list size</param>
        public void ReadTwoInts(out int fourCc, out int size)
        {
            try
            {
                var buffer = new byte[TwoDWordSize];
                int readsize = _stream.Read(buffer, 0, TwoDWordSize);

                if (TwoDWordSize != readsize)
                {
                    throw new RiffParserException("Unable to read. Corrupt RIFF file " + FileName);
                }
                fourCc = RiffDecodeHeader.GetInt(buffer, 0);
                size = RiffDecodeHeader.GetInt(buffer, 4);
            }
            catch (Exception ex)
            {
                throw new RiffParserException("Problem accessing RIFF file " + FileName, ex);
            }
        }

        /// <summary>
        /// Non-thread-safe read a single int from the stream
        /// </summary>
        /// <param name="fourCc">Output int</param>
        public void ReadOneInt(out int fourCc)
        {
            try
            {
                var buffer = new byte[DWordSize];
                int readsize = _stream.Read(buffer, 0, DWordSize);
                if (DWordSize != readsize)
                {
                    throw new RiffParserException("Unable to read. Corrupt RIFF file " + FileName);
                }
                fourCc = RiffDecodeHeader.GetInt(buffer, 0);
            }
            catch (Exception ex)
            {
                throw new RiffParserException("Problem accessing RIFF file " + FileName, ex);
            }
        }

        /// <summary>
        /// Skip the specified number of bytes
        /// </summary>
        /// <param name="skipBytes">Number of bytes to skip</param>
        public void SkipData(int skipBytes)
        {
            try
            {
                _stream.Seek(skipBytes, SeekOrigin.Current);
            }
            catch (Exception ex)
            {
                throw new RiffParserException("Problem seeking in file " + FileName, ex);
            }
        }

        /// <summary>
        /// Read the specified length into the byte array at the specified
        /// offset in the array
        /// </summary>
        /// <param name="data">Array of bytes to read into</param>
        /// <param name="offset">Offset in the array to start from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <returns>Number of bytes actually read</returns>
        public int ReadData(Byte[] data, int offset, int length)
        {
            try
            {
                return _stream.Read(data, offset, length);
            }
            catch (Exception ex)
            {
                throw new RiffParserException("Problem reading data in file " + FileName, ex);
            }
        }

        /// <summary>
        /// Close the RIFF file
        /// </summary>
        public void CloseFile()
        {
            if (null != _stream)
            {
                _stream.Close();
                _stream = null;
            }
        }

        #endregion Stream access

        #region FourCC conversion methods

        public static string FromFourCc(int fourCc)
        {
            var chars = new char[4];
            chars[0] = (char)(fourCc & 0xFF);
            chars[1] = (char)((fourCc >> 8) & 0xFF);
            chars[2] = (char)((fourCc >> 16) & 0xFF);
            chars[3] = (char)((fourCc >> 24) & 0xFF);

            return new string(chars);
        }

        public static int ToFourCc(string fourCc)
        {
            if (fourCc.Length != 4)
            {
                throw new Exception("FourCC strings must be 4 characters long " + fourCc);
            }

            int result = fourCc[3] << 24
                        | fourCc[2] << 16
                        | fourCc[1] << 8
                        | fourCc[0];

            return result;
        }

        public static int ToFourCc(char[] fourCc)
        {
            if (fourCc.Length != 4)
            {
                throw new Exception("FourCC char arrays must be 4 characters long " + new string(fourCc));
            }

            int result = fourCc[3] << 24
                        | fourCc[2] << 16
                        | fourCc[1] << 8
                        | fourCc[0];

            return result;
        }

        public static int ToFourCc(char c0, char c1, char c2, char c3)
        {
            int result = c3 << 24
                        | c2 << 16
                        | c1 << 8
                        | c0;

            return result;
        }

        #endregion FourCC conversion methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
            }
        }

    }
}
