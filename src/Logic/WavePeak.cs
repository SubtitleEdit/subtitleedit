using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Nikse.SubtitleEdit.Logic
{
    public class WaveHeader
    {
        const int ConstantHeaderSize = 20;
        private byte[] _headerData;

        public string ChunkId { get; private set; }
        public int ChunkSize { get; private set; }
        public string Format { get; private set; }
        public string FmtId { get; private set; }
        public int FmtChunkSize { get; private set; }

        /// <summary>
        /// 1 = PCM (uncompressed)
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
        public int DataChunkSize { get; private set; }
        public int DataStartPosition { get; private set; }

        public WaveHeader(Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[ConstantHeaderSize];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead < buffer.Length)
                throw new ArgumentException("Stream is too small");

            // constant header
            ChunkId = Encoding.UTF8.GetString(buffer, 0, 4);
            ChunkSize = BitConverter.ToInt32(buffer, 4);
            Format = Encoding.UTF8.GetString(buffer, 8, 4);
            FmtId = Encoding.UTF8.GetString(buffer, 12, 4);
            FmtChunkSize = BitConverter.ToInt32(buffer, 16);

            // fmt data
            buffer = new byte[FmtChunkSize];
            stream.Read(buffer, 0, buffer.Length);
            AudioFormat = BitConverter.ToInt16(buffer, 0);
            NumberOfChannels = BitConverter.ToInt16(buffer, 2);
            SampleRate = BitConverter.ToInt32(buffer, 4);
            ByteRate = BitConverter.ToInt32(buffer, 8);
            BlockAlign = BitConverter.ToInt16(buffer, 12);
            BitsPerSample = BitConverter.ToInt16(buffer, 14);

            // data 
            buffer = new byte[8];
            stream.Position = ConstantHeaderSize + FmtChunkSize;
            stream.Read(buffer, 0, buffer.Length);
            DataId = Encoding.UTF8.GetString(buffer, 0, 4);
            DataChunkSize = BitConverter.ToInt32(buffer, 4);
            DataStartPosition = (int)(ConstantHeaderSize + FmtChunkSize + 8);

            _headerData = new byte[DataStartPosition];
            stream.Position = 0;
            stream.Read(_headerData, 0, _headerData.Length);
        }

        public long BytesPerSecond
        {
            get
            {
                return SampleRate * (BitsPerSample / 8) * NumberOfChannels;
            }
        }

        public double LengthInSeconds
        {
            get
            {
                return DataChunkSize / BytesPerSecond;
            }
        }

        internal void WriteHeader(Stream fromStream, Stream toStream, int sampleRate, int numberOfChannels, int bitsPerSample, int dataSize)
        {
            int byteRate = sampleRate * (bitsPerSample / 8) * numberOfChannels;

            WriteInt32ToByteArray(_headerData, 4, dataSize + DataStartPosition - 8);
            WriteInt16ToByteArray(_headerData, ConstantHeaderSize + 2, numberOfChannels);
            WriteInt32ToByteArray(_headerData, ConstantHeaderSize + 4, sampleRate);
            WriteInt32ToByteArray(_headerData, ConstantHeaderSize + 8, byteRate);
            WriteInt16ToByteArray(_headerData, ConstantHeaderSize + 14, bitsPerSample);
            WriteInt32ToByteArray(_headerData, ConstantHeaderSize + FmtChunkSize + 4, dataSize);
            toStream.Write(_headerData, 0, _headerData.Length);
        }

        private void WriteInt16ToByteArray(byte[] _headerData, int index, int value)
        {
            byte[] buffer = BitConverter.GetBytes((short)value);
            for (int i = 0; i < buffer.Length; i++)
                _headerData[index + i] = buffer[i];
        }
        private void WriteInt32ToByteArray(byte[] _headerData, int index, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            for (int i = 0; i < buffer.Length; i++)
                _headerData[index + i] = buffer[i];
        }
    }

    public class WavePeakGenerator
    {
        private Stream _stream = null;
        private byte[] _data = null;

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
            Initialize(new FileStream(fileName, FileMode.Open, FileAccess.Read));
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
        /// Generate peaks (samples with some interval) for an uncompressed wave file 
        /// </summary>
        /// <param name="peaksPerSecond">Sampeles per second / sample rate</param>
        public void GeneratePeakSamples(int peaksPerSecond)
        {
            PeaksPerSecond = peaksPerSecond;

            ReadSampleDataValueDelegate readSampleDataValue = GetSampleDataRerader();
            DataMinValue = int.MaxValue;
            DataMaxValue = int.MinValue;
            PeakSamples = new List<int>();
            int bytesInterval = (int)Header.BytesPerSecond / PeaksPerSecond;
            _data = new byte[Header.BytesPerSecond];
            _stream.Position = Header.DataStartPosition;
            int bytesRead = _stream.Read(_data, 0, _data.Length);
            while (bytesRead == Header.BytesPerSecond)
            {
                for (int i = 0; i < Header.BytesPerSecond; i += bytesInterval)
                {
                    int index = i;
                    int value = 0;
                    for (int channelNumber = 0; channelNumber < Header.NumberOfChannels; channelNumber++)
                    {
                        value += readSampleDataValue.Invoke(ref index);
                    }
                    value = value / Header.NumberOfChannels;
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
            // determine how to read sample values
            ReadSampleDataValueDelegate readSampleDataValue = GetSampleDataRerader();

            // load data
            _data = new byte[Header.DataChunkSize];
            _stream.Position = Header.DataStartPosition;
            int bytesRead = _stream.Read(_data, 0, _data.Length);

            // read sample values
            DataMinValue = int.MaxValue;
            DataMaxValue = int.MinValue;
            AllSamples = new List<int>();
            int index = 0;
            while (index + Header.NumberOfChannels < Header.DataChunkSize)
            {
                int value = 0;
                for (int channelNumber = 0; channelNumber < Header.NumberOfChannels; channelNumber++)
                {
                    value += readSampleDataValue.Invoke(ref index);
                }
                value = value / Header.NumberOfChannels;
                if (value < DataMinValue)
                    DataMinValue = value;
                if (value > DataMaxValue)
                    DataMaxValue = value;
                AllSamples.Add(value);
            }
        }

        public void WritePeakSamples(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            WritePeakSamples(fs);
            fs.Close();
        }

        public void WritePeakSamples(Stream stream)
        {
            Header.WriteHeader(_stream, stream, PeaksPerSecond, 1, 16, PeakSamples.Count * 2);
            WritePeakData(stream);
            stream.Flush();
        }

        private void WritePeakData(Stream stream)
        {
            foreach (var value in PeakSamples)
            {
                byte[] buffer = BitConverter.GetBytes((short)(value));
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        private void Initialize(Stream stream)
        {
            _stream = stream;
            Header = new WaveHeader(_stream);
        }

        private int ReadValue8Bit(ref int index)
        {
            int result = _data[index];
            index += 2;
            return result;
        }

        private int ReadValue16Bit(ref int index)
        {
            int result = BitConverter.ToInt16(_data, index);
            index += 2;
            return result;
        }

        private int ReadValue24Bit(ref int index)
        {
            byte[] buffer = new byte[4];
            buffer[0] = 0;
            buffer[1] = _data[index];
            buffer[2] = _data[index + 1];
            buffer[3] = _data[index + 2];
            int result = BitConverter.ToInt32(buffer, 0);
            index += 3;
            return result;
        }

        private int ReadValue32Bit(ref int index)
        {
            int result = BitConverter.ToInt32(_data, index);
            index += 4;
            return result;
        }

        /// <summary>
        /// Determine how to read sample values
        /// </summary>
        /// <returns>Sample data reader that matches bits per sample</returns>
        private ReadSampleDataValueDelegate GetSampleDataRerader()
        {
            ReadSampleDataValueDelegate readSampleDataValue = null;
            switch (Header.BitsPerSample)
            {
                case 8:
                    readSampleDataValue = ReadValue8Bit;
                    break;
                case 16:
                    readSampleDataValue = ReadValue16Bit;
                    break;
                case 24:
                    readSampleDataValue = ReadValue24Bit;
                    break;
                case 32:
                    readSampleDataValue = ReadValue32Bit;
                    break;
                default:
                    throw new InvalidDataException("Cannot read bits per sample of " + Header.BitsPerSample);
            }
            return readSampleDataValue;
        }

        public void Dispose()
        {
            if (_stream != null)
                _stream.Close();
        }

        public void Close()
        {
            if (_stream != null)
                _stream.Close();
        }
    }
}
