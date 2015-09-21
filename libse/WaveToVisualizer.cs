using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// http://soundfile.sapp.org/doc/WaveFormat
    /// </summary>
    public class WaveHeader
    {
        private const int ConstantHeaderSize = 20;
        public const int AudioFormatPcm = 1;

        public string ChunkId { get; private set; }
        public uint ChunkSize { get; private set; }
        public string Format { get; private set; }
        public string FmtId { get; private set; }
        public int FmtChunkSize { get; private set; }

        /// <summary>
        /// 1 = PCM (uncompressed)
        /// 0x0101 = IBM mu-law format
        /// 0x0102 = IBM a-law format
        /// 0x0103 = IBM AVC Adaptive Differential Pulse Code Modulation format
        /// 0xFFFE = WAVE_FORMAT_EXTENSIBLE, Determined by SubFormat
        /// </summary>
        public int AudioFormat { get; private set; }

        public int NumberOfChannels { get; private set; }

        /// <summary>
        /// Number of samples per second
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// Should be SampleRate * BlockAlign
        /// </summary>
        public int ByteRate { get; private set; }

        /// <summary>
        /// 8 bytes per block (32 bit); 6 bytes per block (24 bit); 4 bytes per block (16 bit)
        /// </summary>
        public int BlockAlign { get; private set; }

        public int BitsPerSample { get; private set; }

        public string DataId { get; private set; }

        /// <summary>
        /// Size of sound data
        /// </summary>
        public uint DataChunkSize { get; private set; }
        public int DataStartPosition { get; private set; }

        public WaveHeader(Stream stream)
        {
            stream.Position = 0;
            var buffer = new byte[ConstantHeaderSize];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead < buffer.Length)
                throw new ArgumentException("Stream is too small");

            // constant header
            ChunkId = Encoding.UTF8.GetString(buffer, 0, 4); // Chunk ID: "RIFF" (Resource Interchange File Format), RF64 = new 64-bit format - see http://tech.ebu.ch/docs/tech/tech3306-2009.pdf
            ChunkSize = BitConverter.ToUInt32(buffer, 4); // Chunk size: 16 or 18 or 40
            Format = Encoding.UTF8.GetString(buffer, 8, 4); // Format code - "WAVE"
            FmtId = Encoding.UTF8.GetString(buffer, 12, 4); // Contains the letters "fmt "
            FmtChunkSize = BitConverter.ToInt32(buffer, 16); // 16 for PCM.  This is the size of the rest of the Subchunk which follows this number.

            // fmt data
            buffer = new byte[FmtChunkSize];
            stream.Read(buffer, 0, buffer.Length);
            AudioFormat = BitConverter.ToInt16(buffer, 0); // PCM = 1
            NumberOfChannels = BitConverter.ToInt16(buffer, 2);
            SampleRate = BitConverter.ToInt32(buffer, 4); // 8000, 44100, etc.
            ByteRate = BitConverter.ToInt32(buffer, 8); // SampleRate * NumChannels * BitsPerSample/8
            BlockAlign = BitConverter.ToInt16(buffer, 12);
            BitsPerSample = BitConverter.ToInt16(buffer, 14); // 8 bits = 8, 16 bits = 16, etc.

            // data
            buffer = new byte[8];
            stream.Position = ConstantHeaderSize + FmtChunkSize;
            stream.Read(buffer, 0, buffer.Length);
            DataId = Encoding.UTF8.GetString(buffer, 0, 4);
            DataChunkSize = BitConverter.ToUInt32(buffer, 4);
            DataStartPosition = ConstantHeaderSize + FmtChunkSize + 8;

            // if some other ChunckId than 'data' (e.g. LIST) we search for 'data'
            long oldPos = ConstantHeaderSize + FmtChunkSize;
            while (DataId != "data" && oldPos + DataChunkSize + 16 < stream.Length)
            {
                oldPos = oldPos + DataChunkSize + 8;
                stream.Position = oldPos;
                stream.Read(buffer, 0, buffer.Length);
                DataId = Encoding.UTF8.GetString(buffer, 0, 4);
                DataChunkSize = BitConverter.ToUInt32(buffer, 4);
                DataStartPosition = (int)oldPos + 8;
            }

            // recalculate BlockAlign (older versions wrote incorrect values)
            BlockAlign = BytesPerSample * NumberOfChannels;
        }

        public int BytesPerSample
        {
            get
            {
                // round up to the next byte (20 bit WAVs are like 24 bit WAVs with the 4 least significant bits unused)
                return (BitsPerSample + 7) / 8;
            }
        }

        public long BytesPerSecond
        {
            get
            {
                return (long)SampleRate * BlockAlign;
            }
        }

        public double LengthInSeconds
        {
            get
            {
                return (double)DataChunkSize / BytesPerSecond;
            }
        }

        public long LengthInSamples
        {
            get
            {
                return DataChunkSize / BlockAlign;
            }
        }

        internal static void WriteHeader(Stream toStream, int sampleRate, int numberOfChannels, int bitsPerSample, int dataSize)
        {
            const int headerSize = 44;
            int bytesPerSample = (bitsPerSample + 7) / 8;
            int blockAlign = numberOfChannels * bytesPerSample;
            int byteRate = sampleRate * blockAlign;
            byte[] header = new byte[headerSize];
            WriteStringToByteArray(header, 0, "RIFF");
            WriteInt32ToByteArray(header, 4, headerSize + dataSize - 8); // size of RIFF chunk's data
            WriteStringToByteArray(header, 8, "WAVE");
            WriteStringToByteArray(header, 12, "fmt ");
            WriteInt32ToByteArray(header, 16, 16); // size of fmt chunk's data
            WriteInt16ToByteArray(header, 20, 1); // format, 1 = PCM
            WriteInt16ToByteArray(header, 22, numberOfChannels);
            WriteInt32ToByteArray(header, 24, sampleRate);
            WriteInt32ToByteArray(header, 28, byteRate);
            WriteInt16ToByteArray(header, 32, blockAlign);
            WriteInt16ToByteArray(header, 34, bitsPerSample);
            WriteStringToByteArray(header, 36, "data");
            WriteInt32ToByteArray(header, 40, dataSize);
            toStream.Write(header, 0, headerSize);
        }

        private static void WriteInt16ToByteArray(byte[] headerData, int index, int value)
        {
            byte[] buffer = BitConverter.GetBytes((short)value);
            Buffer.BlockCopy(buffer, 0, headerData, index, buffer.Length);
        }

        private static void WriteInt32ToByteArray(byte[] headerData, int index, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Buffer.BlockCopy(buffer, 0, headerData, index, buffer.Length);
        }

        private static void WriteStringToByteArray(byte[] headerData, int index, string value)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(value);
            Buffer.BlockCopy(buffer, 0, headerData, index, buffer.Length);
        }
    }

    public class WavePeakGenerator : IDisposable
    {
        private Stream _stream;
        private byte[] _data;

        private delegate int ReadSampleDataValueDelegate(ref int index);

        public WaveHeader Header { get; private set; }

        /// <summary>
        /// Lowest data value
        /// </summary>
        public int DataMinValue { get; private set; }

        /// <summary>
        /// Highest data value
        /// </summary>
        public int DataMaxValue { get; private set; }

        /// <summary>
        /// Number of peaks per second (should be divideable by SampleRate)
        /// </summary>
        public int PeaksPerSecond { get; private set; }

        /// <summary>
        /// List of all peak samples (channels are merged)
        /// </summary>
        public List<int> PeakSamples { get; private set; }

        /// <summary>
        /// List of all samples (channels are merged)
        /// </summary>
        public List<int> AllSamples { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Wave file name</param>
        public WavePeakGenerator(string fileName)
        {
            Initialize(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Stream of a wave file</param>
        public WavePeakGenerator(Stream stream)
        {
            Initialize(stream);
        }

        /// <summary>
        /// Returns true if the current wave file can be processed. Compressed wave files or 32-bit files are not supported
        /// </summary>
        public bool IsSupported
        {
            get
            {
                return Header.AudioFormat == WaveHeader.AudioFormatPcm && Header.Format == "WAVE" && Header.BytesPerSample < 4;
            }
        }

        /// <summary>
        /// Generate peaks (samples with some interval) for an uncompressed wave file
        /// </summary>
        /// <param name="delayInMilliseconds">Delay in milliseconds (normally zero)</param>
        public void GeneratePeakSamples(int delayInMilliseconds)
        {
            if (Header.BytesPerSample == 4)
            {
                // Can't handle 32-bit samples due to the way the channel averaging is done
                throw new Exception("32-bit samples are unsupported.");
            }

            PeaksPerSecond = Math.Min(Configuration.Settings.VideoControls.WaveformMinimumSampleRate, Header.SampleRate);

            // Ensure that peaks per second is a factor of the sample rate
            while (Header.SampleRate % PeaksPerSecond != 0)
                PeaksPerSecond++;

            ReadSampleDataValueDelegate readSampleDataValue = GetSampleDataReader();
            DataMinValue = int.MaxValue;
            DataMaxValue = int.MinValue;
            PeakSamples = new List<int>();

            if (delayInMilliseconds > 0)
            {
                for (int i = 0; i < PeaksPerSecond * delayInMilliseconds / 1000; i++)
                    PeakSamples.Add(0);
            }

            int bytesInterval = (int)Header.BytesPerSecond / PeaksPerSecond;
            _data = new byte[Header.BytesPerSecond];
            _stream.Position = Header.DataStartPosition;
            int bytesRead = _stream.Read(_data, 0, _data.Length);
            while (bytesRead > 0)
            {
                for (int i = 0; i < bytesRead; i += bytesInterval)
                {
                    int index = i;
                    int value = 0;
                    for (int channelNumber = 0; channelNumber < Header.NumberOfChannels; channelNumber++)
                    {
                        value += readSampleDataValue.Invoke(ref index);
                    }
                    value /= Header.NumberOfChannels;
                    if (value < DataMinValue)
                        DataMinValue = value;
                    if (value > DataMaxValue)
                        DataMaxValue = value;
                    PeakSamples.Add(value);
                }
                bytesRead = _stream.Read(_data, 0, _data.Length);
            }
        }

        public void GenerateAllSamples()
        {
            if (Header.BytesPerSample == 4)
            {
                // Can't handle 32-bit samples due to the way the channel averaging is done
                throw new Exception("32-bit samples are unsupported.");
            }

            // determine how to read sample values
            ReadSampleDataValueDelegate readSampleDataValue = GetSampleDataReader();

            // load data
            _data = new byte[Header.DataChunkSize];
            _stream.Position = Header.DataStartPosition;
            _stream.Read(_data, 0, _data.Length);

            // read sample values
            DataMinValue = int.MaxValue;
            DataMaxValue = int.MinValue;
            AllSamples = new List<int>();
            int index = 0;
            while (index < Header.DataChunkSize)
            {
                int value = 0;
                for (int channelNumber = 0; channelNumber < Header.NumberOfChannels; channelNumber++)
                {
                    value += readSampleDataValue.Invoke(ref index);
                }
                value /= Header.NumberOfChannels;
                if (value < DataMinValue)
                    DataMinValue = value;
                if (value > DataMaxValue)
                    DataMaxValue = value;
                AllSamples.Add(value);
            }
        }

        public void WritePeakSamples(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                WritePeakSamples(fs);
            }
        }

        public void WritePeakSamples(Stream stream)
        {
            WaveHeader.WriteHeader(stream, PeaksPerSecond, 1, Header.BytesPerSample * 8, PeakSamples.Count * Header.BytesPerSample);
            WritePeakData(stream);
            stream.Flush();
        }

        private void WritePeakData(Stream stream)
        {
            var writeSample = GetSampleDataWriter();
            byte[] buffer = new byte[4];
            int bytesPerSample = Header.BytesPerSample;
            foreach (var value in PeakSamples)
            {
                writeSample(buffer, value);
                stream.Write(buffer, 0, bytesPerSample);
            }
        }

        private void Initialize(Stream stream)
        {
            _stream = stream;
            Header = new WaveHeader(_stream);
        }

        private int ReadValue8Bit(ref int index)
        {
            int result = sbyte.MinValue + _data[index];
            index += 1;
            return result;
        }

        private int ReadValue16Bit(ref int index)
        {
            int result = (short)
                ((_data[index    ]     ) |
                 (_data[index + 1] << 8));
            index += 2;
            return result;
        }

        private int ReadValue24Bit(ref int index)
        {
            int result =
                ((_data[index    ] <<  8) |
                 (_data[index + 1] << 16) |
                 (_data[index + 2] << 24)) >> 8;
            index += 3;
            return result;
        }

        private int ReadValue32Bit(ref int index)
        {
            int result =
                (_data[index    ]      ) |
                (_data[index + 1] <<  8) |
                (_data[index + 2] << 16) |
                (_data[index + 3] << 24);
            index += 4;
            return result;
        }

        private void WriteValue8Bit(byte[] buffer, int value)
        {
            buffer[0] = (byte)(value - sbyte.MinValue);
        }

        private void WriteValue16Bit(byte[] buffer, int value)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
        }

        private void WriteValue24Bit(byte[] buffer, int value)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
        }

        private void WriteValue32Bit(byte[] buffer, int value)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
        }

        /// <summary>
        /// Determine how to read sample values
        /// </summary>
        /// <returns>Sample data reader that matches bits per sample</returns>
        private ReadSampleDataValueDelegate GetSampleDataReader()
        {
            switch (Header.BytesPerSample)
            {
                case 1:
                    return ReadValue8Bit;
                case 2:
                    return ReadValue16Bit;
                case 3:
                    return ReadValue24Bit;
                case 4:
                    return ReadValue32Bit;
                default:
                    throw new InvalidDataException("Cannot read bits per sample of " + Header.BitsPerSample);
            }
        }

        private Action<byte[], int> GetSampleDataWriter()
        {
            switch (Header.BytesPerSample)
            {
                case 1:
                    return WriteValue8Bit;
                case 2:
                    return WriteValue16Bit;
                case 3:
                    return WriteValue24Bit;
                case 4:
                    return WriteValue32Bit;
                default:
                    throw new InvalidDataException("Cannot write bits per sample of " + Header.BitsPerSample);
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (_stream != null)
                _stream.Close();
        }

        //////////////////////////////////////// SPECTRUM ///////////////////////////////////////////////////////////

        public List<Bitmap> GenerateFourierData(int nfft, string spectrogramDirectory, int delayInMilliseconds)
        {
            if (Header.BytesPerSample == 4)
            {
                // Can't handle 32-bit samples due to the way the channel averaging is done
                throw new Exception("32-bit samples are unsupported.");
            }

            const int bitmapWidth = 1024;

            List<Bitmap> bitmaps = new List<Bitmap>();
            SpectrogramDrawer drawer = new SpectrogramDrawer(nfft);
            Task saveImageTask = null;
            ReadSampleDataValueDelegate readSampleDataValue = GetSampleDataReader();
            double sampleScale = 1.0 / (Math.Pow(2.0, Header.BytesPerSample * 8 - 1) * Header.NumberOfChannels);

            int delaySampleCount = (int)(Header.SampleRate * (delayInMilliseconds / TimeCode.BaseUnit));

            // other code (e.g. generating peaks) doesn't handle negative delays, so we'll do the same for now
            delaySampleCount = Math.Max(delaySampleCount, 0);

            long fileSampleCount = Header.LengthInSamples;
            long fileSampleOffset = -delaySampleCount;
            int chunkSampleCount = nfft * bitmapWidth;
            int chunkCount = (int)Math.Ceiling((double)(fileSampleCount + delaySampleCount) / chunkSampleCount);
            double[] chunkSamples = new double[chunkSampleCount];

            Directory.CreateDirectory(spectrogramDirectory);

            _data = new byte[chunkSampleCount * Header.BlockAlign];
            _stream.Seek(Header.DataStartPosition, SeekOrigin.Begin);

            // for negative delays, skip samples at the beginning
            if (fileSampleOffset > 0)
            {
                _stream.Seek(fileSampleOffset * Header.BlockAlign, SeekOrigin.Current);
            }

            for (int iChunk = 0; iChunk < chunkCount; iChunk++)
            {
                // calculate padding at the beginning (for positive delays)
                int startPaddingSampleCount = 0;
                if (fileSampleOffset < 0)
                {
                    startPaddingSampleCount = (int)Math.Min(-fileSampleOffset, chunkSampleCount);
                    fileSampleOffset += startPaddingSampleCount;
                }

                // calculate how many samples to read from the file
                long fileSamplesRemaining = fileSampleCount - Math.Max(fileSampleOffset, 0);
                int fileReadSampleCount = (int)Math.Min(fileSamplesRemaining, chunkSampleCount - startPaddingSampleCount);

                // calculate padding at the end (when the data isn't an even multiple of our chunk size)
                int endPaddingSampleCount = chunkSampleCount - startPaddingSampleCount - fileReadSampleCount;

                int chunkSampleOffset = 0;

                // add padding at the beginning
                if (startPaddingSampleCount > 0)
                {
                    Array.Clear(chunkSamples, chunkSampleOffset, startPaddingSampleCount);
                    chunkSampleOffset += startPaddingSampleCount;
                }

                // read samples from the file
                if (fileReadSampleCount > 0)
                {
                    int fileReadByteCount = fileReadSampleCount * Header.BlockAlign;
                    _stream.Read(_data, 0, fileReadByteCount);
                    fileSampleOffset += fileReadSampleCount;

                    int dataByteOffset = 0;
                    while (dataByteOffset < fileReadByteCount)
                    {
                        int value = 0;
                        for (int iChannel = 0; iChannel < Header.NumberOfChannels; iChannel++)
                        {
                            value += readSampleDataValue(ref dataByteOffset);
                        }
                        chunkSamples[chunkSampleOffset] = value * sampleScale;
                        chunkSampleOffset += 1;
                    }
                }

                // add padding at the end
                if (endPaddingSampleCount > 0)
                {
                    Array.Clear(chunkSamples, chunkSampleOffset, endPaddingSampleCount);
                    chunkSampleOffset += endPaddingSampleCount;
                }

                // generate spectrogram for this chunk
                Bitmap bmp = drawer.Draw(chunkSamples);
                bitmaps.Add(bmp);

                // wait for previous image to finish saving
                if (saveImageTask != null)
                    saveImageTask.Wait();

                // save image
                string imagePath = Path.Combine(spectrogramDirectory, iChunk + ".gif");
                saveImageTask = Task.Factory.StartNew(() =>
                {
                    bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Gif);
                });
            }

            // wait for last image to finish saving
            if (saveImageTask != null)
                saveImageTask.Wait();

            var doc = new XmlDocument();
            var culture = CultureInfo.InvariantCulture;
            doc.LoadXml("<SpectrogramInfo><SampleDuration/><NFFT/><ImageWidth/><SecondsPerImage/></SpectrogramInfo>");
            doc.DocumentElement.SelectSingleNode("SampleDuration").InnerText = ((double)nfft / Header.SampleRate).ToString(culture);
            doc.DocumentElement.SelectSingleNode("NFFT").InnerText = nfft.ToString(culture);
            doc.DocumentElement.SelectSingleNode("ImageWidth").InnerText = bitmapWidth.ToString(culture);
            doc.DocumentElement.SelectSingleNode("SecondsPerImage").InnerText = ((double)chunkSampleCount / Header.SampleRate).ToString(culture); // currently unused; for backwards compatibility
            doc.Save(Path.Combine(spectrogramDirectory, "Info.xml"));

            return bitmaps;
        }

        private class SpectrogramDrawer
        {
            private const double RaisedCosineWindowScale = 0.5;
            private const int MagnitudeIndexRange = 256;

            private readonly int _nfft;
            private readonly MagnitudeToIndexMapper _mapper;
            private readonly RealFFT _fft;
            private readonly FastBitmap.PixelData[] _palette;
            private readonly double[] _segment;
            private readonly double[] _window;
            private readonly double[] _magnitude1;
            private readonly double[] _magnitude2;

            public SpectrogramDrawer(int nfft)
            {
                _nfft = nfft;
                _mapper = new MagnitudeToIndexMapper(100.0, MagnitudeIndexRange - 1);
                _fft = new RealFFT(nfft);
                _palette = GeneratePalette();
                _segment = new double[nfft];
                _window = CreateRaisedCosineWindow(nfft);
                _magnitude1 = new double[nfft / 2];
                _magnitude2 = new double[nfft / 2];

                double scaleCorrection = 1.0 / (RaisedCosineWindowScale * _fft.ForwardScaleFactor);
                for (int i = 0; i < _window.Length; i++)
                {
                    _window[i] *= scaleCorrection;
                }
            }

            public Bitmap Draw(double[] samples)
            {
                int width = samples.Length / _nfft;
                int height = _nfft / 2;
                var bmp = new FastBitmap(new Bitmap(width, height));
                bmp.LockImage();
                for (int x = 0; x < width; x++)
                {
                    // process 2 segments offset by -1/4 and 1/4 fft size, resulting in 1/2 fft size
                    // window spacing (the minimum overlap to avoid discarding parts of the signal)
                    ProcessSegment(samples, (x * _nfft) - (x > 0 ? _nfft / 4 : 0), _magnitude1);
                    ProcessSegment(samples, (x * _nfft) + (x < width - 1 ? _nfft / 4 : 0), _magnitude2);

                    // draw
                    for (int y = 0; y < height; y++)
                    {
                        int colorIndex = _mapper.Map((_magnitude1[y] + _magnitude2[y]) / 2.0);
                        bmp.SetPixel(x, height - y - 1, _palette[colorIndex]);
                    }
                }
                bmp.UnlockImage();
                return bmp.GetBitmap();
            }

            private void ProcessSegment(double[] samples, int offset, double[] magnitude)
            {
                // read a segment of the recorded signal
                for (int i = 0; i < _nfft; i++)
                    _segment[i] = samples[offset + i] * _window[i];

                // transform to the frequency domain
                _fft.ComputeForward(_segment);

                // compute the magnitude of the spectrum
                MagnitudeSpectrum(_segment, magnitude);
            }

            private static double[] CreateRaisedCosineWindow(int n)
            {
                double twoPiOverN = Math.PI * 2.0 / n;
                double[] dst = new double[n];
                for (int i = 0; i < n; i++)
                    dst[i] = 0.5 * (1.0 - Math.Cos(twoPiOverN * i));
                return dst;
            }

            private static void MagnitudeSpectrum(double[] segment, double[] magnitude)
            {
                magnitude[0] = Math.Sqrt(SquareSum(segment[0], segment[1]));
                for (int i = 2; i < segment.Length; i += 2)
                    magnitude[i / 2] = Math.Sqrt(SquareSum(segment[i], segment[i + 1]) * 2.0);
            }

            private static double SquareSum(double a, double b)
            {
                return a * a + b * b;
            }

            private static FastBitmap.PixelData[] GeneratePalette()
            {
                var palette = new FastBitmap.PixelData[MagnitudeIndexRange];
                if (Configuration.Settings.VideoControls.SpectrogramAppearance == "Classic")
                {
                    for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
                        palette[colorIndex] = new FastBitmap.PixelData(PaletteValue(colorIndex, MagnitudeIndexRange));
                }
                else
                {
                    var list = SmoothColors(0, 0, 0, Configuration.Settings.VideoControls.WaveformColor.R,
                                                     Configuration.Settings.VideoControls.WaveformColor.G,
                                                     Configuration.Settings.VideoControls.WaveformColor.B, MagnitudeIndexRange);
                    for (int i = 0; i < MagnitudeIndexRange; i++)
                        palette[i] = new FastBitmap.PixelData(list[i]);
                }
                return palette;
            }

            private static Color PaletteValue(int x, int range)
            {
                double g;
                double r;
                double b;

                double r4 = range / 4.0;
                const double u = 255;

                if (x < r4)
                {
                    b = x / r4;
                    g = 0;
                    r = 0;
                }
                else if (x < 2 * r4)
                {
                    b = (1 - (x - r4) / r4);
                    g = 1 - b;
                    r = 0;
                }
                else if (x < 3 * r4)
                {
                    b = 0;
                    g = (2 - (x - r4) / r4);
                    r = 1 - g;
                }
                else
                {
                    b = (x - 3 * r4) / r4;
                    g = 0;
                    r = 1 - b;
                }

                r = ((int)(Math.Sqrt(r) * u)) & 0xff;
                g = ((int)(Math.Sqrt(g) * u)) & 0xff;
                b = ((int)(Math.Sqrt(b) * u)) & 0xff;

                return Color.FromArgb((int)r, (int)g, (int)b);
            }

            private static List<Color> SmoothColors(int fromR, int fromG, int fromB, int toR, int toG, int toB, int count)
            {
                while (toR < 255 && toG < 255 && toB < 255)
                {
                    toR++;
                    toG++;
                    toB++;
                }

                var list = new List<Color>();
                double r = fromR;
                double g = fromG;
                double b = fromB;
                double diffR = (toR - fromR) / (double)count;
                double diffG = (toG - fromG) / (double)count;
                double diffB = (toB - fromB) / (double)count;

                for (int i = 0; i < count; i++)
                {
                    list.Add(Color.FromArgb((int)r, (int)g, (int)b));
                    r += diffR;
                    g += diffG;
                    b += diffB;
                }
                return list;
            }

            /// Maps magnitudes in the range [-decibelRange .. 0] dB to palette index values in the range [0 .. indexMax]
            private class MagnitudeToIndexMapper
            {
                private readonly double _minMagnitude;
                private readonly double _multiplier;
                private readonly double _addend;

                public MagnitudeToIndexMapper(double decibelRange, int indexMax)
                {
                    double mappingScale = indexMax / decibelRange;
                    _minMagnitude = Math.Pow(10.0, -decibelRange / 20.0);
                    _multiplier = 20.0 * mappingScale;
                    _addend = decibelRange * mappingScale;
                }

                public int Map(double magnitude)
                {
                    return magnitude >= _minMagnitude ? (int)(Math.Log10(magnitude) * _multiplier + _addend) : 0;
                }

                // Less optimized but readable version of the above
                public static int Map(double magnitude, double decibelRange, int indexMax)
                {
                    if (magnitude == 0) return 0;
                    double decibelLevel = 20.0 * Math.Log10(magnitude);
                    return decibelLevel >= -decibelRange ? (int)(indexMax * (decibelLevel + decibelRange) / decibelRange) : 0;
                }
            }
        }
    }
}