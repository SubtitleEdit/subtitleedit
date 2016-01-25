// (c) Giora Tamir (giora@gtamir.com), 2005

using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    public class RiffDecodeHeader
    {

        #region private members

        private RiffParser m_parser;

        private double m_frameRate;
        private int m_maxBitRate;
        private int m_totalFrames;
        private int m_numStreams;
        private int m_width;
        private int m_height;

        private string m_isft;

        private double m_vidDataRate;
        private string m_vidHandler;
        private double m_audDataRate;
        private string m_audHandler;

        private int m_numChannels;
        private int m_samplesPerSec;
        private int m_bitsPerSec;
        private int m_bitsPerSample;

        #endregion private members

        #region public members

        /// <summary>
        /// Access the internal parser object
        /// </summary>
        public RiffParser Parser
        {
            get
            {
                return m_parser;
            }
        }

        public double FrameRate
        {
            get
            {
                double rate = 0.0;
                if (m_frameRate > 0.0)
                    rate = 1000000.0 / m_frameRate;
                return rate;
            }
        }

        public string MaxBitRate
        {
            get
            {
                return String.Format("{0:N} Kb/Sec", m_maxBitRate / 128);
            }
        }

        public int TotalFrames
        {
            get
            {
                return m_totalFrames;
            }
        }

        public double TotalMilliseconds
        {
            get
            {
                double totalTime = 0.0;
                if (m_frameRate > 0.0)
                {
                    totalTime = m_totalFrames * m_frameRate / TimeCode.BaseUnit;
                }
                return totalTime;
            }
        }

        public string NumStreams
        {
            get
            {
                return String.Format("Streams in file: {0:G}", m_numStreams);
            }
        }

        public string FrameSize
        {
            get
            {
                return String.Format("{0:G} x {1:G} pixels per frame", m_width, m_height);
            }
        }

        public int Width
        {
            get
            {
                return m_width;
            }
        }

        public int Height
        {
            get
            {
                return m_height;
            }
        }

        public string VideoDataRate
        {
            get
            {
                return String.Format("Video rate {0:N2} frames/Sec", m_vidDataRate);
            }
        }

        public string AudioDataRate
        {
            get
            {
                return String.Format("Audio rate {0:N2} Kb/Sec", m_audDataRate / TimeCode.BaseUnit);
            }
        }

        public string VideoHandler
        {
            get
            {
                return m_vidHandler;
            }
        }

        public string AudioHandler
        {
            get
            {
                return String.Format("Audio handler 4CC code: {0}", m_audHandler);
            }
        }

        public string ISFT
        {
            get
            {
                return m_isft;
            }
        }

        public string NumChannels
        {
            get
            {
                return String.Format("Audio channels: {0}", m_numChannels);
            }
        }

        public string SamplesPerSec
        {
            get
            {
                return String.Format("Audio rate: {0:N0} Samples/Sec", m_samplesPerSec);
            }
        }

        public string BitsPerSec
        {
            get
            {
                return String.Format("Audio rate: {0:N0} Bytes/Sec", m_bitsPerSec);
            }
        }

        public string BitsPerSample
        {
            get
            {
                return String.Format("Audio data: {0:N0} bits/Sample", m_bitsPerSample);
            }
        }

        #endregion public members

        #region Constructor

        public RiffDecodeHeader(RiffParser rp)
        {
            m_parser = rp;
        }

        private void Clear()
        {
            m_frameRate = 0;
            m_height = 0;
            m_maxBitRate = 0;
            m_numStreams = 0;
            m_totalFrames = 0;
            m_width = 0;

            m_isft = String.Empty;

            m_vidDataRate = 0;
            m_audDataRate = 0;
            m_vidHandler = String.Empty;
            m_audHandler = String.Empty;

            m_numChannels = 0;
            m_samplesPerSec = 0;
            m_bitsPerSample = 0;
            m_bitsPerSec = 0;
        }

        #endregion Constructor

        #region Default element processing

        /// <summary>
        /// Default list element handler - skip the entire list
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="FourCC"></param>
        /// <param name="length"></param>
        private void ProcessList(RiffParser rp, int FourCC, int length)
        {
            rp.SkipData(length);
        }

        #endregion Default element processing

        #region Decode AVI

        /// <summary>
        /// Handle chunk elements found in the AVI file. Ignores unknown chunks and
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="FourCC"></param>
        /// <param name="unpaddedLength"></param>
        /// <param name="paddedLength"></param>
        private void ProcessAVIChunk(RiffParser rp, int FourCC, int unpaddedLength, int paddedLength)
        {
            if (AviRiffData.ckidMainAVIHeader == FourCC)
            {
                // Main AVI header
                DecodeAVIHeader(rp, paddedLength);
            }
            else if (AviRiffData.ckidAVIStreamHeader == FourCC)
            {
                // Stream header
                DecodeAVIStream(rp, paddedLength);
            }
            else if (AviRiffData.ckidAVIISFT == FourCC)
            {
                Byte[] ba = new byte[paddedLength];
                rp.ReadData(ba, 0, paddedLength);
                StringBuilder sb = new StringBuilder(unpaddedLength);
                for (int i = 0; i < unpaddedLength; ++i)
                {
                    if (0 != ba[i]) sb.Append((char)ba[i]);
                }

                m_isft = sb.ToString();
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
        /// <param name="FourCC"></param>
        /// <param name="length"></param>
        private void ProcessAVIList(RiffParser rp, int FourCC, int length)
        {
            RiffParser.ProcessChunkElement pac = ProcessAVIChunk;
            RiffParser.ProcessListElement pal = ProcessAVIList;

            // Is this the header?
            if ((AviRiffData.ckidAVIHeaderList == FourCC)
                || (AviRiffData.ckidAVIStreamList == FourCC)
                || (AviRiffData.ckidINFOList == FourCC))
            {
                while (length > 0)
                {
                    if (false == rp.ReadElement(ref length, pac, pal)) break;
                }
            }
            else
            {
                // Unknown lists - ignore
                rp.SkipData(length);
            }
        }

        public void ProcessMainAVI()
        {
            Clear();
            int length = Parser.DataSize;

            RiffParser.ProcessChunkElement pdc = ProcessAVIChunk;
            RiffParser.ProcessListElement pal = ProcessAVIList;

            while (length > 0)
            {
                if (false == Parser.ReadElement(ref length, pdc, pal)) break;
            }
        }

        private unsafe void DecodeAVIHeader(RiffParser rp, int length)
        {
            //if (length < sizeof(AVIMAINHEADER))
            //{
            //  throw new RiffParserException(String.Format("Header size mismatch. Needed {0} but only have {1}",
            //      sizeof(AVIMAINHEADER), length));
            //}

            byte[] ba = new byte[length];

            if (rp.ReadData(ba, 0, length) != length)
            {
                throw new RiffParserException("Problem reading AVI header.");
            }

            fixed (Byte* bp = &ba[0])
            {
                AVIMAINHEADER* avi = (AVIMAINHEADER*)bp;
                m_frameRate = avi->dwMicroSecPerFrame;
                m_height = avi->dwHeight;
                m_maxBitRate = avi->dwMaxBytesPerSec;
                m_numStreams = avi->dwStreams;
                m_totalFrames = avi->dwTotalFrames;
                m_width = avi->dwWidth;
            }
        }

        private unsafe void DecodeAVIStream(RiffParser rp, int length)
        {
            byte[] ba = new byte[length];

            if (rp.ReadData(ba, 0, length) != length)
            {
                throw new RiffParserException("Problem reading AVI header.");
            }

            fixed (Byte* bp = &ba[0])
            {
                AVISTREAMHEADER* avi = (AVISTREAMHEADER*)bp;

                if (AviRiffData.streamtypeVIDEO == avi->fccType)
                {
                    m_vidHandler = RiffParser.FromFourCC(avi->fccHandler);
                    if (avi->dwScale > 0)
                    {
                        m_vidDataRate = (double)avi->dwRate / (double)avi->dwScale;
                    }
                    else
                    {
                        m_vidDataRate = 0.0;
                    }
                }
                else if (AviRiffData.streamtypeAUDIO == avi->fccType)
                {
                    if (AviRiffData.ckidMP3 == avi->fccHandler)
                    {
                        m_audHandler = "MP3";
                    }
                    else
                    {
                        m_audHandler = RiffParser.FromFourCC(avi->fccHandler);
                    }
                    if (avi->dwScale > 0)
                    {
                        m_audDataRate = 8.0 * (double)avi->dwRate / (double)avi->dwScale;
                        if (avi->dwSampleSize > 0)
                        {
                            m_audDataRate /= (double)avi->dwSampleSize;
                        }
                    }
                    else
                    {
                        m_audDataRate = 0.0;
                    }
                }
            }
        }

        #endregion Decode AVI

        #region WAVE processing

        private void ProcessWaveChunk(RiffParser rp, int FourCC, int unpaddedLength, int length)
        {
            // Is this a 'fmt' chunk?
            if (AviRiffData.ckidWaveFMT == FourCC)
            {
                DecodeWave(rp, length);
            }
            else
            {
                rp.SkipData(length);
            }
        }

        private unsafe void DecodeWave(RiffParser rp, int length)
        {
            byte[] ba = new byte[length];
            rp.ReadData(ba, 0, length);

            fixed (byte* bp = &ba[0])
            {
                WAVEFORMATEX* wave = (WAVEFORMATEX*)bp;
                m_numChannels = wave->nChannels;
                m_bitsPerSec = wave->nAvgBytesPerSec;
                m_bitsPerSample = wave->wBitsPerSample;
                m_samplesPerSec = wave->nSamplesPerSec;
            }
        }

        public void ProcessMainWAVE()
        {
            Clear();
            int length = Parser.DataSize;

            RiffParser.ProcessChunkElement pdc = ProcessWaveChunk;
            RiffParser.ProcessListElement pal = ProcessList;

            while (length > 0)
            {
                if (false == Parser.ReadElement(ref length, pdc, pal)) break;
            }
        }

        #endregion WAVE processing

    }
}
