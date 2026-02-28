using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// http://soundfile.sapp.org/doc/WaveFormat
/// </summary>
public class WaveHeader2
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

    public WaveHeader2(Stream stream)
    {
        stream.Position = 0;

        // Read constant header
        Span<byte> buffer = stackalloc byte[ConstantHeaderSize];
        int bytesRead = stream.Read(buffer);
        if (bytesRead < buffer.Length)
        {
            throw new ArgumentException("Stream is too small");
        }

        // Parse constant header - use Span slicing to avoid array indexing
        ChunkId = Encoding.UTF8.GetString(buffer.Slice(0, 4));
        ChunkSize = BitConverter.ToUInt32(buffer.Slice(4));
        Format = Encoding.UTF8.GetString(buffer.Slice(8, 4));
        FmtId = Encoding.UTF8.GetString(buffer.Slice(12, 4));
        FmtChunkSize = BitConverter.ToInt32(buffer.Slice(16));

        // Read fmt chunk - allocate only if needed (usually 16-18 bytes, max ~40)
        Span<byte> fmtBuffer = FmtChunkSize <= 128
            ? stackalloc byte[FmtChunkSize]
            : new byte[FmtChunkSize];
        _ = stream.Read(fmtBuffer);

        // Parse fmt chunk
        AudioFormat = BitConverter.ToInt16(fmtBuffer);
        NumberOfChannels = BitConverter.ToInt16(fmtBuffer.Slice(2));
        SampleRate = BitConverter.ToInt32(fmtBuffer.Slice(4));
        ByteRate = BitConverter.ToInt32(fmtBuffer.Slice(8));
        BlockAlign = BitConverter.ToInt16(fmtBuffer.Slice(12));
        BitsPerSample = BitConverter.ToInt16(fmtBuffer.Slice(14));

        // Read data chunk header
        Span<byte> dataHeader = stackalloc byte[8];
        stream.Position = ConstantHeaderSize + FmtChunkSize;
        _ = stream.Read(dataHeader);

        DataId = Encoding.UTF8.GetString(dataHeader.Slice(0, 4));
        DataChunkSize = BitConverter.ToUInt32(dataHeader.Slice(4));
        DataStartPosition = ConstantHeaderSize + FmtChunkSize + 8;

        // Search for 'data' chunk if not found immediately
        long currentPos = ConstantHeaderSize + FmtChunkSize;
        while (DataId != "data" && currentPos + DataChunkSize + 16 < stream.Length)
        {
            currentPos += DataChunkSize + 8;
            stream.Position = currentPos;
            _ = stream.Read(dataHeader);

            DataId = Encoding.UTF8.GetString(dataHeader.Slice(0, 4));
            DataChunkSize = BitConverter.ToUInt32(dataHeader.Slice(4));
            DataStartPosition = (int)currentPos + 8;
        }

        // Recalculate BlockAlign (older versions wrote incorrect values)
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

public struct WavePeak2
{
    public readonly short Max;
    public readonly short Min;

    public WavePeak2(short max, short min)
    {
        Max = max;
        Min = min;
    }

    public int Abs
    {
        get { return Math.Max(Math.Abs((int)Max), Math.Abs((int)Min)); }
    }
}

public class WavePeakData2
{
    public WavePeakData2(int sampleRate, IList<WavePeak2> peaks)
    {
        SampleRate = sampleRate;
        LengthInSeconds = (double)peaks.Count / sampleRate;
        Peaks = peaks;
        CalculateHighestPeak();
    }

    public int SampleRate { get; private set; }

    public double LengthInSeconds { get; private set; }

    public IList<WavePeak2> Peaks { get; private set; }

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

    public static WavePeakData2 FromDisk(string peakFileName)
    {
        using (var peakGenerator = new WavePeakGenerator2(peakFileName))
        {
            return peakGenerator.LoadPeaks();
        }
    }

    public static WavePeakData2 FromStream(Stream stream)
    {
        using (var peakGenerator = new WavePeakGenerator2(stream))
        {
            return peakGenerator.LoadPeaks();
        }
    }
}

public class SpectrogramData2 : IDisposable
{
    private string? _loadFromFilePath;
    private float[]? _rawSamples;

    public SpectrogramData2(int fftSize, int imageWidth, double sampleDuration, IList<SKBitmap> images)
    {
        FftSize = fftSize;
        ImageWidth = imageWidth;
        SampleDuration = sampleDuration;
        Images = images;
    }

    internal SpectrogramData2(int fftSize, int imageWidth, double sampleDuration, float[] rawSamples)
    {
        FftSize = fftSize;
        ImageWidth = imageWidth;
        SampleDuration = sampleDuration;
        _rawSamples = rawSamples;
        Images = [];
    }

    private SpectrogramData2(string loadFromFilePath)
    {
        _loadFromFilePath = loadFromFilePath;
        Images = [];
    }

    public int FftSize { get; private set; }

    public int ImageWidth { get; private set; }

    public double SampleDuration { get; private set; }

    public IList<SKBitmap> Images { get; private set; }

    public bool IsLoaded
    {
        get { return _loadFromFilePath == null && _rawSamples == null; }
    }

    public void Load()
    {
        // Load from raw data if available
        if (_rawSamples != null)
        {
            GenerateImagesFromRawData();
            _rawSamples = null;
            return;
        }

        // Load from binary file if path is set
        if (_loadFromFilePath != null)
        {
            string filePath = _loadFromFilePath;
            _loadFromFilePath = null;

            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                LoadFromBinaryFile(filePath);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, $"Unable to load spectrogram from {filePath}");
            }
        }
    }

    private void LoadFromBinaryFile(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs);

        // Read metadata
        FftSize = br.ReadInt32();
        ImageWidth = br.ReadInt32();
        SampleDuration = br.ReadDouble();

        // Read raw samples
        var sampleCount = (int)((fs.Length - 20) / sizeof(float)); // 20 bytes = 3 ints (12 bytes) + 1 double (8 bytes)
        _rawSamples = new float[sampleCount];

        var byteSpan = MemoryMarshal.AsBytes(_rawSamples.AsSpan());
        _ = fs.Read(byteSpan);

        // Generate images from raw data
        GenerateImagesFromRawData();
        _rawSamples = null;
    }

    private void GenerateImagesFromRawData()
    {
        if (_rawSamples == null)
        {
            return;
        }

        var chunkSampleCount = FftSize * ImageWidth;
        var chunkCount = _rawSamples.Length / chunkSampleCount;

        var images = new SKBitmap[chunkCount];

        var sharedPalette = WavePeakGenerator2.SpectrogramDrawer.GeneratePaletteForCurrentStyle();
        Parallel.For(0, chunkCount, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, () =>
        {
            return new WavePeakGenerator2.SpectrogramDrawer(FftSize, sharedPalette);
        },
        (iChunk, loopState, drawer) =>
        {
            var offset = iChunk * chunkSampleCount;
            var chunkSamples = new double[chunkSampleCount];

            for (var i = 0; i < chunkSampleCount; i++)
            {
                chunkSamples[i] = _rawSamples[offset + i];
            }

            images[iChunk] = drawer.Draw(chunkSamples);
            return drawer;
        },
        _ => { /* No cleanup needed */ });

        Images = images;
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
                // ignore
            }
        }
        Images = Array.Empty<SKBitmap>();
    }

    public static SpectrogramData2 FromDisk(string spectrogramFilePath)
    {
        return new SpectrogramData2(spectrogramFilePath);
    }

    public static void SaveToBinaryFile(string filePath, int fftSize, int imageWidth, double sampleDuration, float[] samples)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        // Write metadata
        bw.Write(fftSize);
        bw.Write(imageWidth);
        bw.Write(sampleDuration);

        // Write raw samples
        ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(samples.AsSpan());
        fs.Write(byteSpan);
    }
}

public class WavePeakGenerator2 : IDisposable
{
    #region Movie Hasher -

    public static string GetPeakWaveFileName(string videoFileName, int trackNumber = -1)
    {
        var dir = Se.WaveformsFolder;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (!string.IsNullOrEmpty(videoFileName) &&
            (videoFileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
             videoFileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            return Path.Combine(dir, $"{MovieHasher.GenerateHashFromString(videoFileName)}.wav");
        }

        var hash = MovieHasher.GenerateHash(videoFileName);

        var files = Directory.GetFiles(dir, $"{hash}_*.wav")
            .OrderBy(p => p)
            .ToList();
        if (files.Count > 0 && trackNumber < 0)
        {
            return files[0];
        }

        var wavePeakName = trackNumber >= 0 ? $"{hash}-{trackNumber}.wav" : $"{hash}.wav";

        return Path.Combine(dir, wavePeakName);
    }

    #endregion Movie Hasher

    public static bool IsFileValidForVisualizer(string fileName)
    {
        if (!fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        using (var wpg = new WavePeakGenerator2(fileName))
        {
            return wpg.IsSupported;
        }
    }

    private readonly Stream _stream;
    public readonly WaveHeader2 Header;

    private delegate int ReadSampleDataValue(byte[] data, ref int index);

    private delegate void WriteSampleDataValue(byte[] buffer, int offset, int value);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileName">Wave file name</param>
    public WavePeakGenerator2(string fileName)
        : this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="stream">Stream of a wave file</param>
    public WavePeakGenerator2(Stream stream)
    {
        _stream = stream;
        Header = new WaveHeader2(_stream);
    }

    /// <summary>
    /// Returns true if the current wave file can be processed. Compressed wave files are not supported.
    /// </summary>
    public bool IsSupported => Header.AudioFormat == WaveHeader2.AudioFormatPcm && Header.Format == "WAVE";

    /// <summary>
    /// Generates peaks and saves them to disk.
    /// </summary>
    /// <param name="delayInMilliseconds">Delay in milliseconds (normally zero)</param>
    /// <param name="peakFileName">Path of the output file (writing is skipped if null/empty)</param>
    public WavePeakData2 GeneratePeaks(int delayInMilliseconds, string peakFileName)
    {
        int peaksPerSecond = Math.Min(Configuration.Settings.VideoControls.WaveformMinimumSampleRate, Header.SampleRate);

        // ensure that peaks per second is a factor of the sample rate
        while (Header.SampleRate % peaksPerSecond != 0)
        {
            peaksPerSecond++;
        }

        int delaySampleCount = (int)(Header.SampleRate * (delayInMilliseconds / TimeCode.BaseUnit));

        // ignore negative delays for now (pretty sure it can't happen in mkv and some places pass in -1 by mistake)
        delaySampleCount = Math.Max(delaySampleCount, 0);

        var peaks = new List<WavePeak2>();
        var readSampleDataValue = GetSampleDataReader();
        float sampleAndChannelScale = (float)GetSampleAndChannelScale();
        long fileSampleCount = Header.LengthInSamples;
        long fileSampleOffset = -delaySampleCount;
        int chunkSampleCount = Header.SampleRate / peaksPerSecond;
        byte[] data = new byte[chunkSampleCount * Header.BlockAlign];
        float[] chunkSamples = new float[chunkSampleCount * 2];

        _stream.Seek(Header.DataStartPosition, SeekOrigin.Begin);

        // for negative delays, skip samples at the beginning
        if (fileSampleOffset > 0)
        {
            _stream.Seek(fileSampleOffset * Header.BlockAlign, SeekOrigin.Current);
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
                int fileReadByteCount = fileReadSampleCount * Header.BlockAlign;
                _ = _stream.Read(data, 0, fileReadByteCount);
                fileSampleOffset += fileReadSampleCount;

                int chunkSampleOffset = 0;
                int dataByteOffset = 0;
                while (dataByteOffset < fileReadByteCount)
                {
                    float valuePositive = 0F;
                    float valueNegative = -0F;
                    for (int iChannel = 0; iChannel < Header.NumberOfChannels; iChannel++)
                    {
                        var v = readSampleDataValue(data, ref dataByteOffset);
                        if (v < 0)
                        {
                            valueNegative += v;
                        }
                        else
                        {
                            valuePositive += v;
                        }
                    }

                    chunkSamples[chunkSampleOffset] = valueNegative * sampleAndChannelScale;
                    chunkSampleOffset++;
                    chunkSamples[chunkSampleOffset] = valuePositive * sampleAndChannelScale;
                    chunkSampleOffset++;
                }
            }

            // calculate peaks
            peaks.Add(CalculatePeak(chunkSamples, fileReadSampleCount * 2));
        }

        // save results to file
        if (!string.IsNullOrWhiteSpace(peakFileName))
        {
            using (var stream = File.Create(peakFileName))
            {
                WriteWaveformData(stream, peaksPerSecond, peaks);
            }
        }

        return new WavePeakData2(peaksPerSecond, peaks);
    }

    public static void WriteWaveformData(Stream stream, int sampleRate, List<WavePeak2> peaks)
    {
        WaveHeader2.WriteHeader(stream, sampleRate, 2, 16, peaks.Count);
        var buffer = new byte[4];
        foreach (var peak in peaks)
        {
            WriteValue16Bit(buffer, 0, peak.Max);
            WriteValue16Bit(buffer, 2, peak.Min);
            stream.Write(buffer, 0, 4);
        }
    }

    public static WavePeakData2 GenerateEmptyPeaks(string peakFileName, int totalSeconds)
    {
        var peaksPerSecond = Configuration.Settings.VideoControls.WaveformMinimumSampleRate;
        var peaks = new List<WavePeak2>
        {
            new WavePeak2(1000, -1000)
        };
        var totalPeaks = peaksPerSecond * totalSeconds;
        for (var i = 0; i < totalPeaks; i++)
        {
            peaks.Add(new WavePeak2(1, -1));
        }
        peaks.Add(new WavePeak2(1000, -1000));

        // save results to file
        var dir = Path.GetDirectoryName(peakFileName);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (var stream = File.Create(peakFileName))
        {
            WaveHeader2.WriteHeader(stream, peaksPerSecond, 2, 16, peaks.Count);
            var buffer = new byte[4];
            foreach (var peak in peaks)
            {
                WriteValue16Bit(buffer, 0, peak.Max);
                WriteValue16Bit(buffer, 2, peak.Min);
                stream.Write(buffer, 0, 4);
            }
        }

        return new WavePeakData2(peaksPerSecond, peaks);
    }

    private static WavePeak2 CalculatePeak(float[] chunk, int count)
    {
        if (count == 0)
        {
            return new WavePeak2();
        }

        float max = chunk[0];
        float min = chunk[0];

        for (var i = 1; i < chunk.Length; i++)
        {
            float value = chunk[i];
            max = Math.Max(max, value);
            min = Math.Min(min, value);
        }

        return new WavePeak2((short)(short.MaxValue * max), (short)(short.MaxValue * min));
    }

    /// <summary>
    /// Loads previously generated peaks from disk.
    /// </summary>
    internal WavePeakData2 LoadPeaks()
    {
        if (Header.BitsPerSample != 16)
        {
            throw new Exception("Peaks file must be 16 bits per sample.");
        }

        if (Header.NumberOfChannels != 1 && Header.NumberOfChannels != 2)
        {
            throw new Exception("Peaks file must have 1 or 2 channels.");
        }

        // load data
        byte[] data = new byte[Header.DataChunkSize];
        _stream.Position = Header.DataStartPosition;
        _ = _stream.Read(data, 0, data.Length);

        // read peak values
        WavePeak2[] peaks = new WavePeak2[Header.LengthInSamples + 5];
        int peakIndex = 0;
        if (Header.NumberOfChannels == 2)
        {
            // max value in left channel, min value in right channel
            int byteIndex = 0;
            while (byteIndex < data.Length)
            {
                short max = (short)ReadValue16Bit(data, ref byteIndex);
                short min = (short)ReadValue16Bit(data, ref byteIndex);
                peaks[peakIndex++] = new WavePeak2(max, min);
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
                peaks[peakIndex++] = new WavePeak2(value, (short)-value);
            }
        }

        return new WavePeakData2(Header.SampleRate, peaks);
    }

    private static int ReadValue8Bit(byte[] data, ref int index)
    {
        int result = sbyte.MinValue + data[index];
        index += 1;
        return result;
    }

    private static int ReadValue16Bit(byte[] data, ref int index)
    {
        short result = Unsafe.ReadUnaligned<short>(ref data[index]);
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
        int result = Unsafe.ReadUnaligned<int>(ref data[index]);
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
        return (1.0 / Math.Pow(2.0, Header.BytesPerSample * 8 - 1));
    }

    private double GetSampleAndChannelScale()
    {
        return GetSampleScale() / Header.NumberOfChannels;
    }

    private ReadSampleDataValue GetSampleDataReader()
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

    private WriteSampleDataValue GetSampleDataWriter()
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
        {
            _stream.Close();
        }
    }

    //////////////////////////////////////// SPECTRUM ///////////////////////////////////////////////////////////

    public SpectrogramData2 GenerateSpectrogram(int delayInMilliseconds, string spectrogramFilePath, System.Threading.CancellationToken token)
    {
        const int fftSize = 256; // image height = fft size / 2
        const int imageWidth = 1024;

        int delaySampleCount = (int)(Header.SampleRate * (delayInMilliseconds / TimeCode.BaseUnit));

        // ignore negative delays for now (pretty sure it can't happen in mkv and some places pass in -1 by mistake)
        delaySampleCount = Math.Max(delaySampleCount, 0);

        var readSampleDataValue = GetSampleDataReader();
        double sampleAndChannelScale = GetSampleAndChannelScale();
        long fileSampleCount = Header.LengthInSamples;
        long fileSampleOffset = -delaySampleCount;
        int chunkSampleCount = fftSize * imageWidth;
        int chunkCount = (int)Math.Ceiling((double)(fileSampleCount + delaySampleCount) / chunkSampleCount);
        byte[] data = new byte[chunkSampleCount * Header.BlockAlign];
        float[] allSamples = new float[chunkCount * chunkSampleCount];
        int allSamplesOffset = 0;

        _stream.Seek(Header.DataStartPosition, SeekOrigin.Begin);

        // for negative delays, skip samples at the beginning
        if (fileSampleOffset > 0)
        {
            _stream.Seek(fileSampleOffset * Header.BlockAlign, SeekOrigin.Current);
        }

        for (var iChunk = 0; iChunk < chunkCount; iChunk++)
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

            // add padding at the beginning
            if (startPaddingSampleCount > 0)
            {
                Array.Clear(allSamples, allSamplesOffset, startPaddingSampleCount);
                allSamplesOffset += startPaddingSampleCount;
            }

            // read samples from the file
            if (fileReadSampleCount > 0)
            {
                int fileReadByteCount = fileReadSampleCount * Header.BlockAlign;
                _ = _stream.Read(data, 0, fileReadByteCount);
                fileSampleOffset += fileReadSampleCount;

                int dataByteOffset = 0;
                while (dataByteOffset < fileReadByteCount)
                {
                    double value = 0D;
                    for (int iChannel = 0; iChannel < Header.NumberOfChannels; iChannel++)
                    {
                        value += readSampleDataValue(data, ref dataByteOffset);
                    }
                    allSamples[allSamplesOffset] = (float)(value * sampleAndChannelScale);
                    allSamplesOffset += 1;
                }
            }

            // add padding at the end
            if (endPaddingSampleCount > 0)
            {
                Array.Clear(allSamples, allSamplesOffset, endPaddingSampleCount);
                allSamplesOffset += endPaddingSampleCount;
            }

            if (token.IsCancellationRequested)
            {
                break;
            }
        }

        double sampleDuration = (double)fftSize / Header.SampleRate;

        // Save raw data to binary file
        if (!token.IsCancellationRequested)
        {
            SpectrogramData2.SaveToBinaryFile(spectrogramFilePath, fftSize, imageWidth, sampleDuration, allSamples);
        }

        var result = new SpectrogramData2(fftSize, imageWidth, sampleDuration, allSamples);
        result.Load(); // Generate images immediately for display
        return result;
    }

    public class SpectrogramDrawer
    {
        private const double RaisedCosineWindowScale = 0.5;
        private const int MagnitudeIndexRange = 256;

        private readonly int _nfft;
        private readonly MagnitudeToIndexMapper _mapper;
        private readonly RealFFT _fft;
        private readonly SKColor[] _palette;
        private readonly double[] _segment;
        private readonly double[] _window;
        private readonly double[] _magnitude1;
        private readonly double[] _magnitude2;

        public static string GetSpectrogramFileName(string videoFileName, int trackNumber = -1)
        {
            var dir = Se.SpectrogramsFolder;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!string.IsNullOrEmpty(videoFileName) &&
                (videoFileName.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                 videoFileName.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                return Path.Combine(dir, $"{MovieHasher.GenerateHashFromString(videoFileName)}.spectrogram");
            }

            var hash = MovieHasher.GenerateHash(videoFileName);

            var files = Directory.GetFiles(dir, $"{hash}_*.spectrogram")
                .OrderBy(p => p)
                .ToList();
            if (files.Count > 0 && trackNumber < 0)
            {
                return files[0];
            }

            var spectrogramFileName = trackNumber >= 0 ? $"{hash}-{trackNumber}.spectrogram" : $"{hash}.spectrogram";

            return Path.Combine(dir, spectrogramFileName);
        }

        public SpectrogramDrawer(int nfft, SKColor[] palette)
        {
            _nfft = nfft;
            _mapper = new MagnitudeToIndexMapper(100.0, MagnitudeIndexRange - 1);
            _fft = new RealFFT(nfft);
            _palette = palette;
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

        public unsafe SKBitmap Draw(double[] samples)
        {
            int width = samples.Length / _nfft;
            int height = _nfft / 2;
            var nnftQuarter = _nfft / 4;
            var bmp = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            IntPtr pixelsPtr = bmp.GetPixels();
            byte* pixels = (byte*)pixelsPtr.ToPointer();
            var stride = bmp.RowBytes;

            for (var x = 0; x < width; x++)
            {
                var offset = x * _nfft;
                ProcessSegment(samples, offset - (x > 0 ? nnftQuarter : 0), _magnitude1);
                ProcessSegment(samples, offset + (x < width - 1 ? nnftQuarter : 0), _magnitude2);

                var xOffset = x * 4;
                for (var y = 0; y < height; y++)
                {
                    int colorIndex = _mapper.Map((_magnitude1[y] + _magnitude2[y]) / 2.0);
                    SKColor color = _palette[colorIndex];

                    int pixelY = height - y - 1;
                    byte* pixel = pixels + (pixelY * stride) + xOffset;
                    pixel[0] = color.Red;
                    pixel[1] = color.Green;
                    pixel[2] = color.Blue;
                    pixel[3] = color.Alpha;
                }
            }

            bmp.NotifyPixelsChanged();
            return bmp;
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

        public static SKColor[] GeneratePaletteForCurrentStyle()
        {
            var palette = new SKColor[MagnitudeIndexRange];
            if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicViridis.ToString())
            {
                for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
                {
                    palette[colorIndex] = PaletteValueViridis(colorIndex, MagnitudeIndexRange);
                }
            }
            else if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicPlasma.ToString())
            {
                for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
                {
                    palette[colorIndex] = PaletteValuePlasma(colorIndex, MagnitudeIndexRange);
                }
            }
            else if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicInferno.ToString())
            {
                for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
                {
                    palette[colorIndex] = PaletteValueInferno(colorIndex, MagnitudeIndexRange);
                }
            }
            else if (Se.Settings.Waveform.SpectrogramStyle == SeSpectrogramStyle.ClassicTurbo.ToString())
            {
                for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
                {
                    palette[colorIndex] = PaletteValueTurbo(colorIndex, MagnitudeIndexRange);
                }
            }
            else // Classic
            {
                for (int colorIndex = 0; colorIndex < MagnitudeIndexRange; colorIndex++)
                {
                    palette[colorIndex] = PaletteValue(colorIndex, MagnitudeIndexRange);
                }
            }

            return palette;
        }

        private static SKColor PaletteValue(int x, int range)
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

            return new SKColor((byte)r, (byte)g, (byte)b);
        }

        /// <summary>
        /// Viridis color palette - perceptually uniform color scheme from dark purple to yellow-green
        /// </summary>
        private static SKColor PaletteValueViridis(int x, int range)
        {
            double t = (double)x / range;

            // Viridis polynomial approximation
            double r = 0.267004 + t * (0.281908 + t * (-1.135700 + t * (1.155175 + t * (-0.569888))));
            double g = 0.004874 + t * (1.336760 + t * (-0.747169 + t * (0.144650 + t * (0.239742))));
            double b = 0.329415 + t * (1.375486 + t * (-2.889028 + t * (2.790644 + t * (-1.283776))));

            // Clamp and convert to bytes
            r = Math.Max(0, Math.Min(1, r));
            g = Math.Max(0, Math.Min(1, g));
            b = Math.Max(0, Math.Min(1, b));

            return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        /// <summary>
        /// Plasma color palette - perceptually uniform color scheme from deep blue/purple through pink to yellow
        /// </summary>
        private static SKColor PaletteValuePlasma(int x, int range)
        {
            double t = (double)x / range;

            // Plasma polynomial approximation
            double r = 0.050383 + t * (2.176514 + t * (-2.689460 + t * (6.130348 + t * (-11.107290 + t * (10.024779 + t * (-3.657430))))));
            double g = 0.029803 + t * (0.280267 + t * (2.645293 + t * (-5.336825 + t * (4.481445 + t * (-1.355430)))));
            double b = 0.527975 + t * (0.600417 + t * (1.412440 + t * (-11.930240 + t * (20.434160 + t * (-12.791690)))));


            // Clamp and convert to bytes
            r = Math.Max(0, Math.Min(1, r));
            g = Math.Max(0, Math.Min(1, g));
            b = Math.Max(0, Math.Min(1, b));

            return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        /// <summary>
        /// Inferno color palette - high contrast black to white through red, orange, and yellow
        /// </summary>
        private static SKColor PaletteValueInferno(int x, int range)
        {
            double t = (double)x / range;

            // Inferno polynomial approximation
            double r = 0.001462 + t * (1.217761 + t * (1.795470 + t * (-7.361869 + t * (13.446884 + t * (-9.555991 + t * 2.455710)))));
            double g = 0.000466 + t * (0.125098 + t * (3.875940 + t * (-10.418160 + t * (11.001100 + t * (-4.909755)))));
            double b = 0.013866 + t * (2.565590 + t * (-6.945260 + t * (9.287860 + t * (-5.684940 + t * 1.316750))));

            // Clamp and convert to bytes
            r = Math.Max(0, Math.Min(1, r));
            g = Math.Max(0, Math.Min(1, g));
            b = Math.Max(0, Math.Min(1, b));

            return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        /// <summary>
        /// Turbo color palette - high contrast rainbow-like palette optimized for maximum perceptual contrast
        /// </summary>
        private static SKColor PaletteValueTurbo(int x, int range)
        {
            double t = (double)x / range;

            // Enhanced Turbo with more contrast and color saturation
            // Using a modified approach with wider color gamut
            double r, g, b;

            if (t < 0.125)
            {
                // Deep blue to cyan
                double localT = t / 0.125;
                r = 0.0;
                g = 0.3 * localT;
                b = 0.5 + 0.5 * localT;
            }
            else if (t < 0.25)
            {
                // Cyan to green
                double localT = (t - 0.125) / 0.125;
                r = 0.0;
                g = 0.3 + 0.7 * localT;
                b = 1.0 - 0.5 * localT;
            }
            else if (t < 0.375)
            {
                // Green to yellow-green
                double localT = (t - 0.25) / 0.125;
                r = 0.6 * localT;
                g = 1.0;
                b = 0.5 - 0.5 * localT;
            }
            else if (t < 0.5)
            {
                // Yellow-green to yellow
                double localT = (t - 0.375) / 0.125;
                r = 0.6 + 0.4 * localT;
                g = 1.0;
                b = 0.0;
            }
            else if (t < 0.625)
            {
                // Yellow to orange
                double localT = (t - 0.5) / 0.125;
                r = 1.0;
                g = 1.0 - 0.3 * localT;
                b = 0.0;
            }
            else if (t < 0.75)
            {
                // Orange to red-orange
                double localT = (t - 0.625) / 0.125;
                r = 1.0;
                g = 0.7 - 0.4 * localT;
                b = 0.2 * localT;
            }
            else if (t < 0.875)
            {
                // Red-orange to magenta
                double localT = (t - 0.75) / 0.125;
                r = 1.0;
                g = 0.3 - 0.3 * localT;
                b = 0.2 + 0.5 * localT;
            }
            else
            {
                // Magenta to bright pink/white
                double localT = (t - 0.875) / 0.125;
                r = 1.0;
                g = 0.4 * localT;
                b = 0.7 + 0.3 * localT;
            }

            // Apply gamma correction for better perceptual uniformity and increased contrast
            r = Math.Pow(r, 0.8);
            g = Math.Pow(g, 0.8);
            b = Math.Pow(b, 0.8);

            // Clamp and convert to bytes
            r = Math.Max(0, Math.Min(1, r));
            g = Math.Max(0, Math.Min(1, g));
            b = Math.Max(0, Math.Min(1, b));

            return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
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
