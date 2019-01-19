using Nikse.SubtitleEdit.Core.ContainerFormats.Ebml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Matroska
{
    public class MatroskaFile : IDisposable
    {
        public delegate void LoadMatroskaCallback(long position, long total);

        private readonly FileStream _stream;
        private int _pixelWidth, _pixelHeight;
        private double _frameRate;
        private string _videoCodecId;

        private int _subtitleRipTrackNumber;
        private readonly List<MatroskaSubtitle> _subtitleRip = new List<MatroskaSubtitle>();
        private List<MatroskaTrackInfo> _tracks;

        private readonly Element _segmentElement;
        private long _timecodeScale = 1000000;
        private double _duration;

        public MatroskaFile(string path)
        {
            Path = path;
            _stream = new FastFileStream(path);

            // read header
            var headerElement = ReadElement();
            if (headerElement != null && headerElement.Id == ElementId.Ebml)
            {
                // read segment
                _stream.Seek(headerElement.DataSize, SeekOrigin.Current);
                _segmentElement = ReadElement();
                if (_segmentElement != null && _segmentElement.Id == ElementId.Segment)
                {
                    IsValid = true; // matroska file must start with ebml header and segment
                }
            }
        }

        public bool IsValid { get; }

        public string Path { get; }

        public List<MatroskaTrackInfo> GetTracks(bool subtitleOnly = false)
        {
            ReadSegmentInfoAndTracks();

            if (_tracks == null)
            {
                return new List<MatroskaTrackInfo>();
            }

            return subtitleOnly
                ? _tracks.Where(t => t.IsSubtitle).ToList()
                : _tracks;
        }

        /// <summary>
        /// Get first time of track
        /// </summary>
        /// <param name="trackNumber">Track number</param>
        /// <returns>Start time in milliseconds</returns>
        public long GetTrackStartTime(int trackNumber)
        {
            // go to segment
            _stream.Seek(_segmentElement.DataPosition, SeekOrigin.Begin);

            Element element;
            while (_stream.Position < _stream.Length && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.Info:
                        ReadInfoElement(element);
                        break;
                    case ElementId.Tracks:
                        ReadTracksElement(element);
                        break;
                    case ElementId.Cluster:
                        return FindTrackStartInCluster(element, trackNumber);
                }
                _stream.Seek(element.EndPosition, SeekOrigin.Begin);
            }

            return 0;
        }

        private long FindTrackStartInCluster(Element cluster, int targetTrackNumber)
        {
            long clusterTimeCode = 0L;
            long trackStartTime = -1L;
            bool done = false;

            Element element;
            while (_stream.Position < cluster.EndPosition && (element = ReadElement()) != null && !done)
            {
                switch (element.Id)
                {
                    case ElementId.None:
                        done = true;
                        break;
                    case ElementId.Timecode:
                        // Absolute timestamp of the cluster (based on TimecodeScale)
                        clusterTimeCode = (long)ReadUInt((int)element.DataSize);
                        break;
                    case ElementId.BlockGroup:
                        ReadBlockGroupElement(element, clusterTimeCode);
                        break;
                    case ElementId.SimpleBlock:
                        var trackNumber = (int)ReadVariableLengthUInt();
                        if (trackNumber == targetTrackNumber)
                        {
                            // Timecode (relative to Cluster timecode, signed int16)
                            trackStartTime = ReadInt16();
                            done = true;
                        }
                        break;
                }
                _stream.Seek(element.EndPosition, SeekOrigin.Begin);
            }

            return (long)Math.Round(GetTimescaledToMilliseconds(clusterTimeCode + trackStartTime));
        }

        private void ReadVideoElement(Element videoElement)
        {
            Element element;
            while (_stream.Position < videoElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.PixelWidth:
                        _pixelWidth = (int)ReadUInt((int)element.DataSize);
                        break;
                    case ElementId.PixelHeight:
                        _pixelHeight = (int)ReadUInt((int)element.DataSize);
                        break;
                    default:
                        _stream.Seek(element.DataSize, SeekOrigin.Current);
                        break;
                }
            }
        }

        private void ReadTrackEntryElement(Element trackEntryElement)
        {
            long defaultDuration = 0;
            bool isVideo = false;
            bool isAudio = false;
            bool isSubtitle = false;
            var trackNumber = 0;
            string name = string.Empty;
            string language = "eng"; // default value
            string codecId = string.Empty;
            byte[] codecPrivateRaw = null;
            int contentCompressionAlgorithm = -1;
            int contentEncodingType = -1;
            uint contentEncodingScope = 1;

            Element element;
            while (_stream.Position < trackEntryElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.DefaultDuration:
                        defaultDuration = (int)ReadUInt((int)element.DataSize);
                        break;
                    case ElementId.Video:
                        ReadVideoElement(element);
                        isVideo = true;
                        break;
                    case ElementId.Audio:
                        isAudio = true;
                        break;
                    case ElementId.TrackNumber:
                        trackNumber = (int)ReadUInt((int)element.DataSize);
                        break;
                    case ElementId.Name:
                        name = ReadString((int)element.DataSize, Encoding.UTF8);
                        break;
                    case ElementId.Language:
                        language = ReadString((int)element.DataSize, Encoding.ASCII);
                        break;
                    case ElementId.CodecId:
                        codecId = ReadString((int)element.DataSize, Encoding.ASCII);
                        break;
                    case ElementId.TrackType:
                        switch (_stream.ReadByte())
                        {
                            case 1:
                                isVideo = true;
                                break;
                            case 2:
                                isAudio = true;
                                break;
                            case 17:
                                isSubtitle = true;
                                break;
                        }
                        break;
                    case ElementId.CodecPrivate:
                        codecPrivateRaw = new byte[element.DataSize];
                        _stream.Read(codecPrivateRaw, 0, codecPrivateRaw.Length);
                        break;
                    case ElementId.ContentEncodings:
                        contentCompressionAlgorithm = 0; // default value
                        contentEncodingType = 0; // default value

                        var contentEncodingElement = ReadElement();
                        if (contentEncodingElement != null && contentEncodingElement.Id == ElementId.ContentEncoding)
                        {
                            ReadContentEncodingElement(element, ref contentCompressionAlgorithm, ref contentEncodingType, ref contentEncodingScope);
                        }
                        break;
                }
                _stream.Seek(element.EndPosition, SeekOrigin.Begin);
            }

            _tracks.Add(new MatroskaTrackInfo
            {
                TrackNumber = trackNumber,
                IsVideo = isVideo,
                IsAudio = isAudio,
                IsSubtitle = isSubtitle,
                Language = language,
                CodecId = codecId,
                CodecPrivateRaw = codecPrivateRaw,
                Name = name,
                ContentEncodingType = contentEncodingType,
                ContentCompressionAlgorithm = contentCompressionAlgorithm,
                ContentEncodingScope = contentEncodingScope
            });

            if (isVideo)
            {
                if (defaultDuration > 0)
                {
                    _frameRate = 1.0 / (defaultDuration / 1000000000.0);
                }
                _videoCodecId = codecId;
            }
        }

        private void ReadContentEncodingElement(Element contentEncodingElement, ref int contentCompressionAlgorithm, ref int contentEncodingType, ref uint contentEncodingScope)
        {
            Element element;
            while (_stream.Position < contentEncodingElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.ContentEncodingOrder:
                        var contentEncodingOrder = ReadUInt((int)element.DataSize);
                        System.Diagnostics.Debug.WriteLine("ContentEncodingOrder: " + contentEncodingOrder);
                        break;
                    case ElementId.ContentEncodingScope:
                        contentEncodingScope = (uint)ReadUInt((int)element.DataSize);
                        System.Diagnostics.Debug.WriteLine("ContentEncodingScope: " + contentEncodingScope);
                        break;
                    case ElementId.ContentEncodingType:
                        contentEncodingType = (int)ReadUInt((int)element.DataSize);
                        break;
                    case ElementId.ContentCompression:
                        Element compElement;
                        while (_stream.Position < element.EndPosition && (compElement = ReadElement()) != null)
                        {
                            switch (compElement.Id)
                            {
                                case ElementId.ContentCompAlgo:
                                    contentCompressionAlgorithm = (int)ReadUInt((int)compElement.DataSize);
                                    break;
                                case ElementId.ContentCompSettings:
                                    var contentCompSettings = ReadUInt((int)compElement.DataSize);
                                    System.Diagnostics.Debug.WriteLine("ContentCompSettings: " + contentCompSettings);
                                    break;
                                default:
                                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                                    break;
                            }
                        }
                        break;
                    default:
                        _stream.Seek(element.DataSize, SeekOrigin.Current);
                        break;
                }
            }
        }

        private void ReadInfoElement(Element infoElement)
        {
            Element element;
            while (_stream.Position < infoElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.TimecodeScale:
                        // Timestamp scale in nanoseconds (1.000.000 means all timestamps in the segment are expressed in milliseconds)
                        _timecodeScale = (int)ReadUInt((int)element.DataSize);
                        break;
                    case ElementId.Duration:
                        // Duration of the segment (based on TimecodeScale)
                        _duration = element.DataSize == 4 ? ReadFloat32() : ReadFloat64();
                        _duration = GetTimescaledToMilliseconds(_duration);
                        break;
                    default:
                        _stream.Seek(element.DataSize, SeekOrigin.Current);
                        break;
                }
            }
        }

        private double GetTimescaledToMilliseconds(double time)
        {
            return time * _timecodeScale / 1000000.0;
        }

        private void ReadTracksElement(Element tracksElement)
        {
            _tracks = new List<MatroskaTrackInfo>();

            Element element;
            while (_stream.Position < tracksElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.TrackEntry)
                {
                    ReadTrackEntryElement(element);
                }
                else
                {
                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                }
            }
        }

        /// <summary>
        /// Get info about matroska file
        /// </summary>
        /// <param name="frameRate">Frame rate</param>
        /// <param name="pixelWidth">Width in pixels</param>
        /// <param name="pixelHeight">Height in pixels</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <param name="videoCodec">Codec</param>
        public void GetInfo(out double frameRate, out int pixelWidth, out int pixelHeight, out double duration, out string videoCodec)
        {
            ReadSegmentInfoAndTracks();

            pixelWidth = _pixelWidth;
            pixelHeight = _pixelHeight;
            frameRate = _frameRate;
            duration = _duration;
            videoCodec = _videoCodecId;
        }

        private void ReadCluster(Element clusterElement)
        {
            long clusterTimeCode = 0;

            Element element;
            while (_stream.Position < clusterElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.Timecode:
                        clusterTimeCode = (long)ReadUInt((int)element.DataSize);
                        break;
                    case ElementId.BlockGroup:
                        ReadBlockGroupElement(element, clusterTimeCode);
                        break;
                    case ElementId.SimpleBlock:
                        var subtitle = ReadSubtitleBlock(element, clusterTimeCode);
                        if (subtitle != null)
                        {
                            _subtitleRip.Add(subtitle);
                        }
                        break;
                    default:
                        _stream.Seek(element.DataSize, SeekOrigin.Current);
                        break;
                }
            }
        }

        private void ReadBlockGroupElement(Element clusterElement, long clusterTimeCode)
        {
            MatroskaSubtitle subtitle = null;

            Element element;
            while (_stream.Position < clusterElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.Block:
                        subtitle = ReadSubtitleBlock(element, clusterTimeCode);
                        if (subtitle == null)
                        {
                            return;
                        }
                        _subtitleRip.Add(subtitle);
                        break;
                    case ElementId.BlockDuration:
                        var duration = (long)ReadUInt((int)element.DataSize);
                        if (subtitle != null)
                        {
                            subtitle.Duration = (long)Math.Round(GetTimescaledToMilliseconds(duration));
                        }
                        break;
                    default:
                        _stream.Seek(element.DataSize, SeekOrigin.Current);
                        break;
                }
            }
        }

        private MatroskaSubtitle ReadSubtitleBlock(Element blockElement, long clusterTimeCode)
        {
            var trackNumber = (int)ReadVariableLengthUInt();
            if (trackNumber != _subtitleRipTrackNumber)
            {
                _stream.Seek(blockElement.EndPosition, SeekOrigin.Begin);
                return null;
            }

            var timeCode = ReadInt16();

            // lacing
            var flags = (byte)_stream.ReadByte();
            int frames;
            switch (flags & 6)
            {
                case 0: // 00000000 = No lacing
                    System.Diagnostics.Debug.Print("No lacing");
                    break;
                case 2: // 00000010 = Xiph lacing
                    frames = _stream.ReadByte() + 1;
                    System.Diagnostics.Debug.Print("Xiph lacing ({0} frames)", frames);
                    break;
                case 4: // 00000100 = Fixed-size lacing
                    frames = _stream.ReadByte() + 1;
                    for (var i = 0; i < frames; i++)
                    {
                        _stream.ReadByte(); // frames
                    }
                    System.Diagnostics.Debug.Print("Fixed-size lacing ({0} frames)", frames);
                    break;
                case 6: // 00000110 = EMBL lacing
                    frames = _stream.ReadByte() + 1;
                    System.Diagnostics.Debug.Print("EBML lacing ({0} frames)", frames);
                    break;
            }

            // save subtitle data
            var dataLength = (int)(blockElement.EndPosition - _stream.Position);
            var data = new byte[dataLength];
            _stream.Read(data, 0, dataLength);

            return new MatroskaSubtitle(data, (long)Math.Round(GetTimescaledToMilliseconds(clusterTimeCode + timeCode)));
        }

        public List<MatroskaSubtitle> GetSubtitle(int trackNumber, LoadMatroskaCallback progressCallback)
        {
            _subtitleRip.Clear();
            _subtitleRipTrackNumber = trackNumber;
            ReadSegmentCluster(progressCallback);
            return _subtitleRip;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }

        private void ReadSegmentInfoAndTracks()
        {
            // go to segment
            _stream.Seek(_segmentElement.DataPosition, SeekOrigin.Begin);

            Element element;
            while (_stream.Position < _segmentElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.Info:
                        ReadInfoElement(element);
                        break;
                    case ElementId.Tracks:
                        ReadTracksElement(element);
                        return;
                    default:
                        _stream.Seek(element.DataSize, SeekOrigin.Current);
                        break;
                }
            }
        }

        private void ReadSegmentCluster(LoadMatroskaCallback progressCallback)
        {
            // go to segment
            _stream.Seek(_segmentElement.DataPosition, SeekOrigin.Begin);

            Element element;
            while (_stream.Position < _segmentElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.Cluster)
                {
                    ReadCluster(element);
                }
                else
                {
                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                }

                progressCallback?.Invoke(element.EndPosition, _stream.Length);
            }
        }

        private Element ReadElement()
        {
            var id = (ElementId)ReadVariableLengthUInt(false);
            if (id <= ElementId.None)
            {
                return null;
            }

            var size = (long)ReadVariableLengthUInt();
            return new Element(id, _stream.Position, size);
        }

        private ulong ReadVariableLengthUInt(bool unsetFirstBit = true)
        {
            // Begin loop with byte set to newly read byte
            var first = _stream.ReadByte();
            var length = 0;

            // Begin by counting the bits unset before the highest set bit
            var mask = 0x80;
            for (var i = 0; i < 8; i++)
            {
                // Start at left, shift to right
                if ((first & mask) == mask)
                {
                    length = i + 1;
                    break;
                }
                mask >>= 1;
            }
            if (length == 0)
            {
                return 0;
            }

            // Read remaining big endian bytes and convert to 64-bit unsigned integer.
            var result = (ulong)(unsetFirstBit ? first & (0xFF >> length) : first);
            result <<= --length * 8;
            for (var i = 1; i <= length; i++)
            {
                result |= (ulong)_stream.ReadByte() << (length - i) * 8;
            }
            return result;
        }

        /// <summary>
        /// Reads a fixed length unsigned integer from the current stream and advances the current
        /// position of the stream by the integer length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the integer.</param>
        /// <returns>A 64-bit unsigned integer.</returns>
        private ulong ReadUInt(int length)
        {
            var data = new byte[length];
            _stream.Read(data, 0, length);

            // Convert the big endian byte array to a 64-bit unsigned integer.
            var result = 0UL;
            var shift = 0;
            for (var i = length - 1; i >= 0; i--)
            {
                result |= (ulong)data[i] << shift;
                shift += 8;
            }
            return result;
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current position
        /// of the stream by two bytes.
        /// </summary>
        /// <returns>A 2-byte signed integer read from the current stream.</returns>
        private short ReadInt16()
        {
            var data = new byte[2];
            _stream.Read(data, 0, 2);
            return (short)(data[0] << 8 | data[1]);
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream and advances the current
        /// position of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte floating point value read from the current stream.</returns>
        private float ReadFloat32()
        {
            var data = new byte[4];
            _stream.Read(data, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                var data2 = new[] { data[3], data[2], data[1], data[0] };
                return BitConverter.ToSingle(data2, 0);
            }
            return BitConverter.ToSingle(data, 0);
        }

        /// <summary>
        /// Reads a 8-byte floating point value from the current stream and advances the current
        /// position of the stream by eight bytes.
        /// </summary>
        /// <returns>A 8-byte floating point value read from the current stream.</returns>
        private double ReadFloat64()
        {
            var data = new byte[8];
            _stream.Read(data, 0, 8);
            if (BitConverter.IsLittleEndian)
            {
                var data2 = new[] { data[7], data[6], data[5], data[4], data[3], data[2], data[1], data[0] };
                return BitConverter.ToDouble(data2, 0);
            }
            return BitConverter.ToDouble(data, 0);
        }

        /// <summary>
        /// Reads a fixed length string from the current stream using the specified encoding.
        /// </summary>
        /// <param name="length">The length in bytes of the string.</param>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns>The string being read.</returns>
        private string ReadString(int length, Encoding encoding)
        {
            var buffer = new byte[length];
            _stream.Read(buffer, 0, length);
            return encoding.GetString(buffer);
        }
    }
}
