using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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
            {
                throw new ArgumentException("Stream is too small");
            }

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

        internal static void WriteHeader(Stream toStream, int sampleRate, int numberOfChannels, int bitsPerSample, int sampleCount)
        {
            const int headerSize = 44;
            int bytesPerSample = (bitsPerSample + 7) / 8;
            int blockAlign = numberOfChannels * bytesPerSample;
            int byteRate = sampleRate * blockAlign;
            int dataSize = sampleCount * bytesPerSample * numberOfChannels;
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

    public struct WavePeak
    {
        public readonly short Max;
        public readonly short Min;

        public WavePeak(short max, short min)
        {
            Max = max;
            Min = min;
        }

        public int Abs
        {
            get { return Math.Max(Math.Abs((int)Max), Math.Abs((int)Min)); }
        }
    }

    public class WavePeakData
    {
        public WavePeakData(int sampleRate, IList<WavePeak> peaks)
        {
            SampleRate = sampleRate;
            LengthInSeconds = (double)peaks.Count / sampleRate;
            Peaks = peaks;
            CalculateHighestPeak();
        }

        public int SampleRate { get; private set; }

        public double LengthInSeconds { get; private set; }

        public IList<WavePeak> Peaks { get; private set; }

        public int HighestPeak { get; private set; }

        private void CalculateHighestPeak()
        {
            HighestPeak = 0;
            foreach (var peak in Peaks)
            {
                int abs = peak.Abs;
                if (abs > HighestPeak)
                {
                    HighestPeak = abs;
                }
            }
        }

        public static WavePeakData FromDisk(string peakFileName)
        {
            using (var peakGenerator = new WavePeakGenerator(peakFileName))
            {
                return peakGenerator.LoadPeaks();
            }
        }
    }

    public class SpectrogramData : IDisposable
    {
        private string _loadFromDirectory;

        public SpectrogramData(int fftSize, int imageWidth, double sampleDuration, IList<Bitmap> images)
        {
            FftSize = fftSize;
            ImageWidth = imageWidth;
            SampleDuration = sampleDuration;
            Images = images;
        }

        private SpectrogramData(string loadFromDirectory)
        {
            _loadFromDirectory = loadFromDirectory;
            Images = new Bitmap[0];
        }

        public int FftSize { get; private set; }

        public int ImageWidth { get; private set; }

        public double SampleDuration { get; private set; }

        public IList<Bitmap> Images { get; private set; }

        public bool IsLoaded
        {
            get { return _loadFromDirectory == null; }
        }

        public void Load()
        {
            if (_loadFromDirectory == null)
            {
                return;
            }

            string directory = _loadFromDirectory;
            _loadFromDirectory = null;

            try
            {
                string xmlInfoFileName = Path.Combine(directory, "Info.xml");
                if (!File.Exists(xmlInfoFileName))
                {
                    return;
                }

                var doc = new XmlDocument();
                var culture = CultureInfo.InvariantCulture;
                doc.Load(xmlInfoFileName);
                FftSize = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("NFFT").InnerText, culture);
                ImageWidth = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("ImageWidth").InnerText, culture);
                SampleDuration = Convert.ToDouble(doc.DocumentElement.SelectSingleNode("SampleDuration").InnerText, culture);

                var images = new List<Bitmap>();
                var fileNames = Enumerable.Range(0, int.MaxValue)
                    .Select(n => Path.Combine(directory, n + ".gif"))
                    .TakeWhile(p => File.Exists(p));
                foreach (string fileName in fileNames)
                {
                    // important that this does not lock file (do NOT use Image.FromFile(fileName) or alike!!!)
                    using (var ms = new MemoryStream(File.ReadAllBytes(fileName)))
                    {
                        images.Add((Bitmap)Image.FromStream(ms));
                    }
                }
                Images = images;
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            foreach (var image in Images)
            {
                try
                {
                    image.Dispose();
                }
                catch
                {
                }
            }
            Images = new Bitmap[0];
        }

        public static SpectrogramData FromDisk(string spectrogramDirectory)
        {
            return new SpectrogramData(spectrogramDirectory);
        }
    }

    public class WavePeakGenerator : IDisposable
    {
        #region Movie Hasher -

        public static string GetPeakWaveFileName(string videofileName)
        {
            var dir = Configuration.WaveformsDirectory.TrimEnd(Path.DirectorySeparatorChar);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var wavePeakName = MovieHasher.GenerateHash(videofileName) + ".wav";
            return Path.Combine(dir, wavePeakName);
        }

        #endregion Movie Hasher

        public static bool IsFileValidForVisualizer(string fileName)
        {
            if (!fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            using (var wpg = new WavePeakGenerator(fileName))
            {
                return wpg.IsSupported;
            }
        }

        private readonly Stream _stream;
        private readonly WaveHeader _header;

        private delegate int ReadSampleDataValue(byte[] data, ref int index);

        private delegate void WriteSampleDataValue(byte[] buffer, int offset, int value);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Wave file name</param>
        public WavePeakGenerator(string fileName)
            : this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Stream of a wave file</param>
        public WavePeakGenerator(Stream stream)
        {
            _stream = stream;
            _header = new WaveHeader(_stream);
        }

        /// <summary>
        /// Returns true if the current wave file can be processed. Compressed wave files are not supported.
        /// </summary>
        public bool IsSupported
        {
            get
            {
                return _header.AudioFormat == WaveHeader.AudioFormatPcm && _header.Format == "WAVE";
            }
        }

        /// <summary>
        /// Generates peaks and saves them to disk.
        /// </summary>
        /// <param name="delayInMilliseconds">Delay in milliseconds (normally zero)</param>
        /// <param name="peakFileName">Path of the output file</param>
        public WavePeakData GeneratePeaks(int delayInMilliseconds, string peakFileName)
        {
            int peaksPerSecond = Math.Min(Configuration.Settings.VideoControls.WaveformMinimumSampleRate, _header.SampleRate);

            // ensure that peaks per second is a factor of the sample rate
            while (_header.SampleRate % peaksPerSecond != 0)
            {
                peaksPerSecond++;
            }

            int delaySampleCount = (int)(_header.SampleRate * (delayInMilliseconds / TimeCode.BaseUnit));

            // ignore negative delays for now (pretty sure it can't happen in mkv and some places pass in -1 by mistake)
            delaySampleCount = Math.Max(delaySampleCount, 0);

            var peaks = new List<WavePeak>();
            var readSampleDataValue = GetSampleDataReader();
            float sampleAndChannelScale = (float)GetSampleAndChannelScale();
            long fileSampleCount = _header.LengthInSamples;
            long fileSampleOffset = -delaySampleCount;
            int chunkSampleCount = _header.SampleRate / peaksPerSecond;
            byte[] data = new byte[chunkSampleCount * _header.BlockAlign];
            float[] chunkSamples = new float[chunkSampleCount];

            _stream.Seek(_header.DataStartPosition, SeekOrigin.Begin);

            // for negative delays, skip samples at the beginning
            if (fileSampleOffset > 0)
            {
                _stream.Seek(fileSampleOffset * _header.BlockAlign, SeekOrigin.Current);
            }

            while (fileSampleOffset < fileSampleCount)
            {
                // calculate how many samples to skip at the beginning (for positive delays)
                int startSkipSampleCount = 0;
                if (fileSampleOffset < 0)
                {
                    startSkipSampleCount = (int)Math.Min(-fileSampleOffset, chunkSampleCount);
                    fileSampleOffset += startSkipSampleCount;
                }

                // calculate how many samples to read from the file
                long fileSamplesRemaining = fileSampleCount - Math.Max(fileSampleOffset, 0);
                int fileReadSampleCount = (int)Math.Min(fileSamplesRemaining, chunkSampleCount - startSkipSampleCount);

                // read samples from the file
                if (fileReadSampleCount > 0)
                {
                    int fileReadByteCount = fileReadSampleCount * _header.BlockAlign;
                    _stream.Read(data, 0, fileReadByteCount);
                    fileSampleOffset += fileReadSampleCount;

                    int chunkSampleOffset = 0;
                    int dataByteOffset = 0;
                    while (dataByteOffset < fileReadByteCount)
                    {
                        float value = 0F;
                        for (int iChannel = 0; iChannel < _header.NumberOfChannels; iChannel++)
                        {
                            value += readSampleDataValue(data, ref dataByteOffset);
                        }
                        chunkSamples[chunkSampleOffset] = value * sampleAndChannelScale;
                        chunkSampleOffset += 1;
                    }
                }

                // calculate peaks
                peaks.Add(CalculatePeak(chunkSamples, fileReadSampleCount));
            }

            // save results to file
            using (var stream = File.Create(peakFileName))
            {
                WaveHeader.WriteHeader(stream, peaksPerSecond, 2, 16, peaks.Count);
                byte[] buffer = new byte[4];
                foreach (var peak in peaks)
                {
                    WriteValue16Bit(buffer, 0, peak.Max);
                    WriteValue16Bit(buffer, 2, peak.Min);
                    stream.Write(buffer, 0, 4);
                }
            }

            return new WavePeakData(peaksPerSecond, peaks);
        }

        private static WavePeak CalculatePeak(float[] chunk, int count)
        {
            if (count == 0)
            {
                return new WavePeak();
            }

            float max = chunk[0];
            float min = chunk[0];
            for (int i = 1; i < count; i++)
            {
                float value = chunk[i];
                if (value > max)
                {
                    max = value;
                }

                if (value < min)
                {
                    min = value;
                }
            }
            return new WavePeak((short)(short.MaxValue * max), (short)(short.MaxValue * min));
        }

        /// <summary>
        /// Loads previously generated peaks from disk.
        /// </summary>
        internal WavePeakData LoadPeaks()
        {
            if (_header.BitsPerSample != 16)
            {
                throw new Exception("Peaks file must be 16 bits per sample.");
            }

            if (_header.NumberOfChannels != 1 && _header.NumberOfChannels != 2)
            {
                throw new Exception("Peaks file must have 1 or 2 channels.");
            }

            // load data
            byte[] data = new byte[_header.DataChunkSize];
            _stream.Position = _header.DataStartPosition;
            _stream.Read(data, 0, data.Length);

            // read peak values
            WavePeak[] peaks = new WavePeak[_header.LengthInSamples];
            int peakIndex = 0;
            if (_header.NumberOfChannels == 2)
            {
                // max value in left channel, min value in right channel
                int byteIndex = 0;
                while (byteIndex < data.Length)
                {
                    short max = (short)ReadValue16Bit(data, ref byteIndex);
                    short min = (short)ReadValue16Bit(data, ref byteIndex);
                    peaks[peakIndex++] = new WavePeak(max, min);
                }
            }
            else
            {
                // single sample value (for backwards compatibility)
                int byteIndex = 0;
                while (byteIndex < data.Length)
                {
                    short value = (short)ReadValue16Bit(data, ref byteIndex);
                    if (value == short.MinValue)
                    {
                        value = -short.MaxValue;
                    }

                    value = Math.Abs(value);
                    peaks[peakIndex++] = new WavePeak(value, (short)-value);
                }
            }

            return new WavePeakData(_header.SampleRate, peaks);
        }

        private static int ReadValue8Bit(byte[] data, ref int index)
        {
            int result = sbyte.MinValue + data[index];
            index += 1;
            return result;
        }

        private static int ReadValue16Bit(byte[] data, ref int index)
        {
            int result = (short)
                ((data[index]) |
                 (data[index + 1] << 8));
            index += 2;
            return result;
        }

        private static int ReadValue24Bit(byte[] data, ref int index)
        {
            int result =
                ((data[index] << 8) |
                 (data[index + 1] << 16) |
                 (data[index + 2] << 24)) >> 8;
            index += 3;
            return result;
        }

        private static int ReadValue32Bit(byte[] data, ref int index)
        {
            int result =
                (data[index]) |
                (data[index + 1] << 8) |
                (data[index + 2] << 16) |
                (data[index + 3] << 24);
            index += 4;
            return result;
        }

        private static void WriteValue8Bit(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value - sbyte.MinValue);
        }

        private static void WriteValue16Bit(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        private static void WriteValue24Bit(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
        }

        private static void WriteValue32Bit(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }

        private double GetSampleScale()
        {
            return (1.0 / Math.Pow(2.0, _header.BytesPerSample * 8 - 1));
        }

        private double GetSampleAndChannelScale()
        {
            return GetSampleScale() / _header.NumberOfChannels;
        }

        private ReadSampleDataValue GetSampleDataReader()
        {
            switch (_header.BytesPerSample)
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
                    throw new InvalidDataException("Cannot read bits per sample of " + _header.BitsPerSample);
            }
        }

        private WriteSampleDataValue GetSampleDataWriter()
        {
            switch (_header.BytesPerSample)
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
                    throw new InvalidDataException("Cannot write bits per sample of " + _header.BitsPerSample);
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (_stream != null)
            {
                _stream.Close();
            }
        }

        //////////////////////////////////////// SPECTRUM ///////////////////////////////////////////////////////////

        public SpectrogramData GenerateSpectrogram(int delayInMilliseconds, string spectrogramDirectory)
        {
            const int fftSize = 256; // image height = fft size / 2
            const int imageWidth = 1024;

            int delaySampleCount = (int)(_header.SampleRate * (delayInMilliseconds / TimeCode.BaseUnit));

            // ignore negative delays for now (pretty sure it can't happen in mkv and some places pass in -1 by mistake)
            delaySampleCount = Math.Max(delaySampleCount, 0);

            var images = new List<Bitmap>();
            var drawer = new SpectrogramDrawer(fftSize);
            var readSampleDataValue = GetSampleDataReader();
            Task saveImageTask = null;
            double sampleAndChannelScale = GetSampleAndChannelScale();
            long fileSampleCount = _header.LengthInSamples;
            long fileSampleOffset = -delaySampleCount;
            int chunkSampleCount = fftSize * imageWidth;
            int chunkCount = (int)Math.Ceiling((double)(fileSampleCount + delaySampleCount) / chunkSampleCount);
            byte[] data = new byte[chunkSampleCount * _header.BlockAlign];
            double[] chunkSamples = new double[chunkSampleCount];

            Directory.CreateDirectory(spectrogramDirectory);

            _stream.Seek(_header.DataStartPosition, SeekOrigin.Begin);

            // for negative delays, skip samples at the beginning
            if (fileSampleOffset > 0)
            {
                _stream.Seek(fileSampleOffset * _header.BlockAlign, SeekOrigin.Current);
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
                    int fileReadByteCount = fileReadSampleCount * _header.BlockAlign;
                    _stream.Read(data, 0, fileReadByteCount);
                    fileSampleOffset += fileReadSampleCount;

                    int dataByteOffset = 0;
                    while (dataByteOffset < fileReadByteCount)
                    {
                        double value = 0D;
                        for (int iChannel = 0; iChannel < _header.NumberOfChannels; iChannel++)
                        {
                            value += readSampleDataValue(data, ref dataByteOffset);
                        }
                        chunkSamples[chunkSampleOffset] = value * sampleAndChannelScale;
                        chunkSampleOffset += 1;
                    }
                }

                // add padding at the end
                if (endPaddingSampleCount > 0)
                {
                    Array.Clear(chunkSamples, chunkSampleOffset, endPaddingSampleCount);
                }

                // generate spectrogram for this chunk
                Bitmap bmp = drawer.Draw(chunkSamples);
                images.Add(bmp);

                // wait for previous image to finish saving
                saveImageTask?.Wait();

                // save image
                string imagePath = Path.Combine(spectrogramDirectory, iChunk + ".gif");
                saveImageTask = Task.Factory.StartNew(() =>
                {
                    bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Gif);
                });
            }

            // wait for last image to finish saving
            saveImageTask?.Wait();

            var doc = new XmlDocument();
            var culture = CultureInfo.InvariantCulture;
            double sampleDuration = (double)fftSize / _header.SampleRate;
            doc.LoadXml("<SpectrogramInfo><SampleDuration/><NFFT/><ImageWidth/><SecondsPerImage/></SpectrogramInfo>");
            doc.DocumentElement.SelectSingleNode("SampleDuration").InnerText = sampleDuration.ToString(culture);
            doc.DocumentElement.SelectSingleNode("NFFT").InnerText = fftSize.ToString(culture);
            doc.DocumentElement.SelectSingleNode("ImageWidth").InnerText = imageWidth.ToString(culture);
            doc.DocumentElement.SelectSingleNode("SecondsPerImage").InnerText = ((double)chunkSampleCount / _header.SampleRate).ToString(culture); // currently unused; for backwards compatibility
            doc.Save(Path.Combine(spectrogramDirectory, "Info.xml"));

            return new SpectrogramData(fftSize, imageWidth, sampleDuration, images);
        }

        public class SpectrogramDrawer
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

            public static string GetSpectrogramFolder(string videoFileName)
            {
                var dir = Configuration.SpectrogramsDirectory.TrimEnd(Path.DirectorySeparatorChar);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return Path.Combine(dir, MovieHasher.GenerateHash(videoFileName));
            }

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
                {
                    _segment[i] = samples[offset + i] * _window[i];
                }

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
                {
                    dst[i] = 0.5 * (1.0 - Math.Cos(twoPiOverN * i));
                }

                return dst;
            }

            private static void MagnitudeSpectrum(double[] segment, double[] magnitude)
            {
                magnitude[0] = Math.Sqrt(SquareSum(segment[0], segment[1]));
                for (int i = 2; i < segment.Length; i += 2)
                {
                    magnitude[i / 2] = Math.Sqrt(SquareSum(segment[i], segment[i + 1]) * 2.0);
                }
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
                    {
                        palette[colorIndex] = new FastBitmap.PixelData(PaletteValue(colorIndex, MagnitudeIndexRange));
                    }
                }
                else
                {
                    var list = SmoothColors(0, 0, 0, Configuration.Settings.VideoControls.WaveformColor.R,
                                                     Configuration.Settings.VideoControls.WaveformColor.G,
                                                     Configuration.Settings.VideoControls.WaveformColor.B, MagnitudeIndexRange);
                    for (int i = 0; i < MagnitudeIndexRange; i++)
                    {
                        palette[i] = new FastBitmap.PixelData(list[i]);
                    }
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
                    if (Math.Abs(magnitude) < 0.01)
                    {
                        return 0;
                    }

                    double decibelLevel = 20.0 * Math.Log10(magnitude);
                    return decibelLevel >= -decibelRange ? (int)(indexMax * (decibelLevel + decibelRange) / decibelRange) : 0;
                }
            }
        }
    }
}
