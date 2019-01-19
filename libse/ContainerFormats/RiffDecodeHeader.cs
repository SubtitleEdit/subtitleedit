// (c) Giora Tamir (giora@gtamir.com), 2005

using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    public class RiffDecodeHeader
    {

        #region private members

        private readonly RiffParser _parser;

        private double _frameRate;
        private int _maxBitRate;
        private int _totalFrames;
        private int _numStreams;
        private int _width;
        private int _height;

        private string _isft;

        private double _vidDataRate;
        private string _vidHandler;
        private double _audDataRate;
        private string _audHandler;

        private int _numChannels;
        private int _samplesPerSec;
        private int _bitsPerSec;
        private int _bitsPerSample;

        #endregion private members

        #region public members

        /// <summary>
        /// Access the internal parser object
        /// </summary>
        public RiffParser Parser
        {
            get
            {
                return _parser;
            }
        }

        public double FrameRate
        {
            get
            {
                double rate = 0.0;
                if (_frameRate > 0.0)
                {
                    rate = 1000000.0 / _frameRate;
                }

                return rate;
            }
        }

        public string MaxBitRate
        {
            get
            {
                return String.Format("{0:N} Kb/Sec", _maxBitRate / 128);
            }
        }

        public int TotalFrames
        {
            get
            {
                return _totalFrames;
            }
        }

        public double TotalMilliseconds
        {
            get
            {
                double totalTime = 0.0;
                if (_frameRate > 0.0)
                {
                    totalTime = _totalFrames * _frameRate / TimeCode.BaseUnit;
                }
                return totalTime;
            }
        }

        public string NumStreams
        {
            get
            {
                return String.Format("Streams in file: {0:G}", _numStreams);
            }
        }

        public string FrameSize
        {
            get
            {
                return String.Format("{0:G} x {1:G} pixels per frame", _width, _height);
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        public string VideoDataRate
        {
            get
            {
                return String.Format("Video rate {0:N2} frames/Sec", _vidDataRate);
            }
        }

        public string AudioDataRate
        {
            get
            {
                return String.Format("Audio rate {0:N2} Kb/Sec", _audDataRate / TimeCode.BaseUnit);
            }
        }

        public string VideoHandler
        {
            get
            {
                return _vidHandler;
            }
        }

        public string AudioHandler
        {
            get
            {
                return String.Format("Audio handler 4CC code: {0}", _audHandler);
            }
        }

        public string ISFT
        {
            get
            {
                return _isft;
            }
        }

        public string NumChannels
        {
            get
            {
                return String.Format("Audio channels: {0}", _numChannels);
            }
        }

        public string SamplesPerSec
        {
            get
            {
                return String.Format("Audio rate: {0:N0} Samples/Sec", _samplesPerSec);
            }
        }

        public string BitsPerSec
        {
            get
            {
                return String.Format("Audio rate: {0:N0} Bytes/Sec", _bitsPerSec);
            }
        }

        public string BitsPerSample
        {
            get
            {
                return String.Format("Audio data: {0:N0} bits/Sample", _bitsPerSample);
            }
        }

        #endregion public members

        #region Constructor

        public RiffDecodeHeader(RiffParser rp)
        {
            _parser = rp;
        }

        private void Clear()
        {
            _frameRate = 0;
            _height = 0;
            _maxBitRate = 0;
            _numStreams = 0;
            _totalFrames = 0;
            _width = 0;

            _isft = String.Empty;

            _vidDataRate = 0;
            _audDataRate = 0;
            _vidHandler = String.Empty;
            _audHandler = String.Empty;

            _numChannels = 0;
            _samplesPerSec = 0;
            _bitsPerSample = 0;
            _bitsPerSec = 0;
        }

        #endregion Constructor

        #region Default element processing

        /// <summary>
        /// Default list element handler - skip the entire list
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="fourCc"></param>
        /// <param name="length"></param>
        private void ProcessList(RiffParser rp, int fourCc, int length)
        {
            rp.SkipData(length);
        }

        #endregion Default element processing

        #region Decode AVI

        /// <summary>
        /// Handle chunk elements found in the AVI file. Ignores unknown chunks and
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="fourCc"></param>
        /// <param name="unpaddedLength"></param>
        /// <param name="paddedLength"></param>
        private void ProcessAviChunk(RiffParser rp, int fourCc, int unpaddedLength, int paddedLength)
        {
            if (AviRiffData.ckidMainAVIHeader == fourCc)
            {
                // Main AVI header
                DecodeAviHeader(rp, paddedLength);
            }
            else if (AviRiffData.ckidAVIStreamHeader == fourCc)
            {
                // Stream header
                DecodeAviStream(rp, paddedLength);
            }
            else if (AviRiffData.ckidAVIISFT == fourCc)
            {
                Byte[] ba = new byte[paddedLength];
                rp.ReadData(ba, 0, paddedLength);
                StringBuilder sb = new StringBuilder(unpaddedLength);
                for (int i = 0; i < unpaddedLength; ++i)
                {
                    if (0 != ba[i])
                    {
                        sb.Append((char)ba[i]);
                    }
                }

                _isft = sb.ToString();
            }
            else
            {
                // Unknon chunk - skip
                rp.SkipData(paddedLength);
            }
        }

        /// <summary>
        /// Handle List elements found in the AVI file. Ignores unknown lists and recursively looks
        /// at the content of known lists.
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="fourCc"></param>
        /// <param name="length"></param>
        private void ProcessAviList(RiffParser rp, int fourCc, int length)
        {
            RiffParser.ProcessChunkElement pac = ProcessAviChunk;
            RiffParser.ProcessListElement pal = ProcessAviList;

            // Is this the header?
            if ((AviRiffData.ckidAVIHeaderList == fourCc)
                || (AviRiffData.ckidAVIStreamList == fourCc)
                || (AviRiffData.ckidINFOList == fourCc))
            {
                while (length > 0)
                {
                    if (false == rp.ReadElement(ref length, pac, pal))
                    {
                        break;
                    }
                }
            }
            else
            {
                // Unknown lists - ignore
                rp.SkipData(length);
            }
        }

        public void ProcessMainAvi()
        {
            Clear();
            int length = Parser.DataSize;

            RiffParser.ProcessChunkElement pdc = ProcessAviChunk;
            RiffParser.ProcessListElement pal = ProcessAviList;

            while (length > 0)
            {
                if (false == Parser.ReadElement(ref length, pdc, pal))
                {
                    break;
                }
            }
        }

        public static int GetInt(byte[] buffer, int index)
        {
            byte[] bytes = { buffer[index + 0], buffer[index + 1], buffer[index + 2], buffer[index + 3] };

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        public static Int16 GetShort(byte[] buffer, int index)
        {
            byte[] bytes = { buffer[index + 0], buffer[index + 1] };

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt16(bytes, 0);
        }

        private void DecodeAviHeader(RiffParser rp, int length)
        {
            var ba = new byte[length];
            if (rp.ReadData(ba, 0, length) != length)
            {
                throw new RiffParserException("Problem reading AVI header.");
            }

            _frameRate = GetInt(ba, 0);
            _maxBitRate = GetInt(ba, 4);
            _totalFrames = GetInt(ba, 16);
            _numStreams = GetInt(ba, 24);
            _width = GetInt(ba, 32);
            _height = GetInt(ba, 36);
        }

        private void DecodeAviStream(RiffParser rp, int length)
        {
            var ba = new byte[length];
            if (rp.ReadData(ba, 0, length) != length)
            {
                throw new RiffParserException("Problem reading AVI header.");
            }

            var aviStreamHeader = new AVISTREAMHEADER
            {
                fccType = GetInt(ba, 0),
                fccHandler = GetInt(ba, 4),
                dwScale = GetInt(ba, 20),
                dwRate = GetInt(ba, 24),
                dwStart = GetInt(ba, 28),
                dwLength = GetInt(ba, 32),
                dwSampleSize = GetInt(ba, 44),
            };

            if (AviRiffData.streamtypeVIDEO == aviStreamHeader.fccType)
            {
                _vidHandler = RiffParser.FromFourCc(aviStreamHeader.fccHandler);
                if (aviStreamHeader.dwScale > 0)
                {
                    _vidDataRate = (double)aviStreamHeader.dwRate / aviStreamHeader.dwScale;
                }
                else
                {
                    _vidDataRate = 0.0;
                }
            }
            else if (AviRiffData.streamtypeAUDIO == aviStreamHeader.fccType)
            {
                if (AviRiffData.ckidMP3 == aviStreamHeader.fccHandler)
                {
                    _audHandler = "MP3";
                }
                else
                {
                    _audHandler = RiffParser.FromFourCc(aviStreamHeader.fccHandler);
                }
                if (aviStreamHeader.dwScale > 0)
                {
                    _audDataRate = 8.0 * aviStreamHeader.dwRate / aviStreamHeader.dwScale;
                    if (aviStreamHeader.dwSampleSize > 0)
                    {
                        _audDataRate /= aviStreamHeader.dwSampleSize;
                    }
                }
                else
                {
                    _audDataRate = 0.0;
                }
            }
        }

        #endregion Decode AVI

        #region WAVE processing

        private void ProcessWaveChunk(RiffParser rp, int fourCc, int unpaddedLength, int length)
        {
            // Is this a 'fmt' chunk?
            if (AviRiffData.ckidWaveFMT == fourCc)
            {
                DecodeWave(rp, length);
            }
            else
            {
                rp.SkipData(length);
            }
        }

        private void DecodeWave(RiffParser rp, int length)
        {
            var ba = new byte[length];
            rp.ReadData(ba, 0, length);

            _numChannels = GetShort(ba, 2);
            _bitsPerSec = GetInt(ba, 8);
            _bitsPerSample = GetShort(ba, 14);
            _samplesPerSec = GetInt(ba, 4);
        }

        public void ProcessMainWave()
        {
            Clear();
            int length = Parser.DataSize;

            RiffParser.ProcessChunkElement pdc = ProcessWaveChunk;
            RiffParser.ProcessListElement pal = ProcessList;

            while (length > 0)
            {
                if (false == Parser.ReadElement(ref length, pdc, pal))
                {
                    break;
                }
            }
        }

        #endregion WAVE processing

    }
}
