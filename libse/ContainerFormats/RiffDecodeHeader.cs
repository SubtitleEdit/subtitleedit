// (c) Giora Tamir (giora@gtamir.com), 2005

using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    public class RiffDecodeHeader
    {
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

        /// <summary>
        /// Access the internal parser object
        /// </summary>
        public RiffParser Parser { get; }

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

        public string MaxBitRate => $"{_maxBitRate / 128:N} Kb/Sec";

        public int TotalFrames => _totalFrames;

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

        public string NumStreams => $"Streams in file: {_numStreams:G}";

        public string FrameSize => $"{_width:G} x {_height:G} pixels per frame";

        public int Width => _width;

        public int Height => _height;

        public string VideoDataRate => $"Video rate {_vidDataRate:N2} frames/Sec";

        public string AudioDataRate => $"Audio rate {_audDataRate / TimeCode.BaseUnit:N2} Kb/Sec";

        public string VideoHandler => _vidHandler;

        public string AudioHandler => $"Audio handler 4CC code: {_audHandler}";

        public string ISFT => _isft;

        public string NumChannels => $"Audio channels: {_numChannels}";

        public string SamplesPerSec => $"Audio rate: {_samplesPerSec:N0} Samples/Sec";

        public string BitsPerSec => $"Audio rate: {_bitsPerSec:N0} Bytes/Sec";

        public string BitsPerSample => $"Audio data: {_bitsPerSample:N0} bits/Sample";

        public RiffDecodeHeader(RiffParser rp)
        {
            Parser = rp;
        }

        private void Clear()
        {
            _frameRate = 0;
            _height = 0;
            _maxBitRate = 0;
            _numStreams = 0;
            _totalFrames = 0;
            _width = 0;

            _isft = string.Empty;

            _vidDataRate = 0;
            _audDataRate = 0;
            _vidHandler = string.Empty;
            _audHandler = string.Empty;

            _numChannels = 0;
            _samplesPerSec = 0;
            _bitsPerSample = 0;
            _bitsPerSec = 0;
        }

        /// <summary>
        /// Default list element handler - skip the entire list
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="fourCc"></param>
        /// <param name="length"></param>
        private static void ProcessList(RiffParser rp, int fourCc, int length)
        {
            rp.SkipData(length);
        }

        /// <summary>
        /// Handle chunk elements found in the AVI file. Ignores unknown chunks and
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="fourCc"></param>
        /// <param name="unpaddedLength"></param>
        /// <param name="paddedLength"></param>
        private void ProcessAviChunk(RiffParser rp, int fourCc, int unpaddedLength, int paddedLength)
        {
            if (AviRiffData.MainAviHeader == fourCc)
            {
                DecodeAviHeader(rp, paddedLength); // Main AVI header
            }
            else if (AviRiffData.AviStreamHeader == fourCc)
            {
                DecodeAviStream(rp, paddedLength); // Stream header
            }
            else if (AviRiffData.AviIsft == fourCc)
            {
                var ba = new byte[paddedLength];
                rp.ReadData(ba, 0, paddedLength);
                var sb = new StringBuilder(unpaddedLength);
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
                rp.SkipData(paddedLength); // Unknown chunk - skip
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
            if (AviRiffData.AviHeaderList == fourCc || AviRiffData.AviStreamList == fourCc || AviRiffData.InfoList == fourCc)
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
                rp.SkipData(length); // Unknown lists - ignore
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

        public static short GetShort(byte[] buffer, int index)
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

            var aviStreamHeader = new AviStreamHeader
            {
                FccType = GetInt(ba, 0),
                FccHandler = GetInt(ba, 4),
                Scale = GetInt(ba, 20),
                Rate = GetInt(ba, 24),
                Start = GetInt(ba, 28),
                Length = GetInt(ba, 32),
                SampleSize = GetInt(ba, 44),
            };

            if (AviRiffData.StreamTypeVideo == aviStreamHeader.FccType)
            {
                _vidHandler = RiffParser.FromFourCc(aviStreamHeader.FccHandler);
                if (aviStreamHeader.Scale > 0)
                {
                    _vidDataRate = (double)aviStreamHeader.Rate / aviStreamHeader.Scale;
                }
                else
                {
                    _vidDataRate = 0.0;
                }
            }
            else if (AviRiffData.StreamTypeAudio == aviStreamHeader.FccType)
            {
                if (AviRiffData.Mp3 == aviStreamHeader.FccHandler)
                {
                    _audHandler = "MP3";
                }
                else
                {
                    _audHandler = RiffParser.FromFourCc(aviStreamHeader.FccHandler);
                }
                if (aviStreamHeader.Scale > 0)
                {
                    _audDataRate = 8.0 * aviStreamHeader.Rate / aviStreamHeader.Scale;
                    if (aviStreamHeader.SampleSize > 0)
                    {
                        _audDataRate /= aviStreamHeader.SampleSize;
                    }
                }
                else
                {
                    _audDataRate = 0.0;
                }
            }
        }

        private void ProcessWaveChunk(RiffParser rp, int fourCc, int unpaddedLength, int length)
        {
            // Is this a 'fmt' chunk?
            if (AviRiffData.WaveFmt == fourCc)
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
    }
}
