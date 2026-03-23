using Nikse.SubtitleEdit.Core.ContainerFormats.Ebml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Matroska
{
    public sealed class MatroskaFile : IDisposable
    {
        public delegate void LoadMatroskaCallback(long position, long total);

        private readonly FileStream _stream;
        private readonly byte[] _buffer = new byte[8];
        private int _pixelWidth, _pixelHeight;
        private double _frameRate;
        private string _videoCodecId;

        private int _subtitleRipTrackNumber;
        private readonly List<MatroskaSubtitle> _subtitleRip = new List<MatroskaSubtitle>();
        private List<MatroskaTrackInfo> _tracks;
        private List<MatroskaChapter> _chapters;

        private readonly Element _segmentElement;
        private long _timeCodeScale = 1000000;
        private double _duration;

        public bool IsValid { get; }

        public string Path { get; }

        public MatroskaFile(string path)
        {
            Path = path;
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536);

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

        public List<MatroskaTrackInfo> GetTracks(bool subtitleOnly = false)
        {
            ReadSegmentInfoAndTracks();

            if (_tracks == null)
            {
                return new List<MatroskaTrackInfo>();
            }

            if (!subtitleOnly)
            {
                return _tracks;
            }

            var subtitleTracks = new List<MatroskaTrackInfo>(_tracks.Count);
            for (var i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].IsSubtitle)
                {
                    subtitleTracks.Add(_tracks[i]);
                }
            }
            return subtitleTracks;
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

            const int maxClustersToSeek = 100;
            var clusterNo = 0;

            Element element;
            while (_stream.Position < _stream.Length &&
                   clusterNo < maxClustersToSeek &&
                   (element = ReadElement()) != null)
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
                        clusterNo++;
                        var startTime = FindTrackStartInCluster(element, trackNumber, out var found);
                        if (found)
                        {
                            return startTime;
                        }

                        break;
                }

                _stream.Seek(element.EndPosition, SeekOrigin.Begin);
            }

            return 0;
        }

        public long GetAudioTrackDelayMilliseconds(int audioTrackNumber)
        {
            var tracks = GetTracks();
            var videoTrack = tracks.Find(p => p.IsVideo && p.IsDefault) ?? tracks.Find(p => p.IsVideo);
            long videoDelay = 0;
            if (videoTrack != null)
            {
                videoDelay = GetTrackStartTime(videoTrack.TrackNumber);
            }

            return GetTrackStartTime(audioTrackNumber) - videoDelay;
        }

        private long FindTrackStartInCluster(Element cluster, int targetTrackNumber, out bool found)
        {
            found = false;
            var clusterTimeCode = 0L;
            var trackStartTime = -1L;
            var done = false;

            Element element;
            while (_stream.Position < cluster.EndPosition && (element = ReadElement()) != null && !done)
            {
                switch (element.Id)
                {
                    case ElementId.None:
                        done = true;
                        break;
                    case ElementId.Timecode:
                        // Absolute timestamp of the cluster (based on TimeCodeScale)
                        clusterTimeCode = ReadUIntAsLong(element.DataSize);
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
                            found = true;
                        }
                        break;
                }
                _stream.Seek(element.EndPosition, SeekOrigin.Begin);
            }

            return (long)Math.Round(GetTimeScaledToMilliseconds(clusterTimeCode + trackStartTime));
        }

        private void ReadVideoElement(Element videoElement)
        {
            Element element;
            while (_stream.Position < videoElement.EndPosition && (element = ReadElement()) != null)
            {
                switch (element.Id)
                {
                    case ElementId.PixelWidth:
                        _pixelWidth = ReadUIntAsInt(element.DataSize);
                        break;
                    case ElementId.PixelHeight:
                        _pixelHeight = ReadUIntAsInt(element.DataSize);
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
            bool isDefault = true;
            bool isForced = false;
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
                        defaultDuration = ReadUIntAsLong(element.DataSize);
                        break;
                    case ElementId.Video:
                        ReadVideoElement(element);
                        isVideo = true;
                        break;
                    case ElementId.Audio:
                        isAudio = true;
                        break;
                    case ElementId.TrackNumber:
                        trackNumber = ReadUIntAsInt(element.DataSize);
                        break;
                    case ElementId.Name:
                        name = ReadString(element.DataSize, Encoding.UTF8);
                        break;
                    case ElementId.Language:
                        language = ReadString(element.DataSize, Encoding.ASCII);
                        break;
                    case ElementId.CodecId:
                        codecId = ReadString(element.DataSize, Encoding.ASCII);
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
                    case ElementId.FlagDefault:
                        isDefault = ReadUIntAsInt(element.DataSize) == 1;
                        break;
                    case ElementId.FlagForced:
                        isForced = ReadUIntAsInt(element.DataSize) == 1;
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
                ContentEncodingScope = contentEncodingScope,
                IsDefault = isDefault,
                IsForced = isForced,
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
                        var contentEncodingOrder = ReadUIntAsInt(element.DataSize);
                        System.Diagnostics.Debug.WriteLine("ContentEncodingOrder: " + contentEncodingOrder);
                        break;
                    case ElementId.ContentEncodingScope:
                        contentEncodingScope = (uint)ReadUIntAsInt(element.DataSize);
                        System.Diagnostics.Debug.WriteLine("ContentEncodingScope: " + contentEncodingScope);
                        break;
                    case ElementId.ContentEncodingType:
                        contentEncodingType = ReadUIntAsInt(element.DataSize);
                        break;
                    case ElementId.ContentCompression:
                        Element compElement;
                        while (_stream.Position < element.EndPosition && (compElement = ReadElement()) != null)
                        {
                            switch (compElement.Id)
                            {
                                case ElementId.ContentCompAlgo:
                                    contentCompressionAlgorithm = ReadUIntAsInt(compElement.DataSize);
                                    break;
                                case ElementId.ContentCompSettings:
                                    var contentCompSettings = ReadUIntAsInt(compElement.DataSize);
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
                        _timeCodeScale = ReadUIntAsLong(element.DataSize);
                        break;
                    case ElementId.Duration:
                        // Duration of the segment (based on TimeCodeScale)
                        _duration = element.DataSize == 4 ? ReadFloat32() : ReadFloat64();
                        _duration = GetTimeScaledToMilliseconds(_duration);
                        break;
                    default:
                        _stream.Seek(element.DataSize, SeekOrigin.Current);
                        break;
                }
            }
        }

        private double GetTimeScaledToMilliseconds(double time)
        {
            return time * _timeCodeScale / 1000000.0;
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

        public List<MatroskaChapter> GetChapters()
        {
            ReadChapters();

            return _chapters ?? new List<MatroskaChapter>();
        }

        private void ReadChapters()
        {
            // go to segment
            _stream.Seek(_segmentElement.DataPosition, SeekOrigin.Begin);

            Element element;
            while (_stream.Position < _segmentElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.Chapters)
                {
                    ReadChaptersElement(element);
                }
                else
                {
                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                }
            }
        }

        private void ReadChaptersElement(Element chaptersElement)
        {
            _chapters = new List<MatroskaChapter>();

            Element element;
            while (_stream.Position < chaptersElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.EditionEntry)
                {
                    ReadEditionEntryElement(element);
                }
                else
                {
                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                }
            }
        }

        private void ReadEditionEntryElement(Element editionEntryElement)
        {
            Element element;
            while (_stream.Position < editionEntryElement.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.ChapterAtom)
                {
                    ReadChapterTimeStart(element);
                }
                else
                {
                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                }
            }
        }

        private void ReadChapterTimeStart(Element chpaterAtom)
        {
            var chapter = new MatroskaChapter();

            Element element;
            while (_stream.Position < chpaterAtom.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.ChapterTimeStart)
                {
                    chapter.StartTime = ReadUIntAsLong(element.DataSize) / 1000000000.0;
                }
                else if (element.Id == ElementId.ChapterDisplay)
                {
                    chapter.Name = GetChapterName(element);
                }
                else if (element.Id == ElementId.ChapterAtom)
                {
                    ReadNestedChaptersTimeStart(element);
                }
                else
                {
                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                }
            }

            _chapters.Add(chapter);
        }

        private void ReadNestedChaptersTimeStart(Element nestedChpaterAtom)
        {
            var chapter = new MatroskaChapter
            {
                Nested = true
            };

            Element element;
            while (_stream.Position < nestedChpaterAtom.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.ChapterTimeStart)
                {
                    chapter.StartTime = ReadUIntAsLong(element.DataSize) / 1000000000.0;
                }
                else if (element.Id == ElementId.ChapterDisplay)
                {
                    chapter.Name = GetChapterName(element);
                }
                else
                {
                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                }
            }

            _chapters.Add(chapter);
        }

        private string GetChapterName(Element chapterDisplay)
        {
            Element element;
            while (_stream.Position < chapterDisplay.EndPosition && (element = ReadElement()) != null)
            {
                if (element.Id == ElementId.ChapString)
                {
                    return ReadString(element.DataSize, Encoding.UTF8);
                }
                else
                {
                    _stream.Seek(element.DataSize, SeekOrigin.Current);
                }
            }

            return null;
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
                        clusterTimeCode = ReadUIntAsLong(element.DataSize);
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
                        var duration = ReadUIntAsLong(element.DataSize);
                        if (subtitle != null)
                        {
                            subtitle.Duration = (long)Math.Round(GetTimeScaledToMilliseconds(duration));
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

            return new MatroskaSubtitle(data, (long)Math.Round(GetTimeScaledToMilliseconds(clusterTimeCode + timeCode)));
        }

        public List<MatroskaSubtitle> GetSubtitle(int trackNumber, LoadMatroskaCallback progressCallback)
        {
            _subtitleRip.Clear();
            _subtitleRipTrackNumber = trackNumber;
            ReadSegmentCluster(progressCallback);
            return _subtitleRip;
        }

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream?.Dispose();
            }
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

            while (_stream.Position < _segmentElement.EndPosition)
            {
                var beforeReadElementIdPosition = _stream.Position;
                var id = (ElementId)ReadVariableLengthUInt(false);
                if (id == ElementId.None && beforeReadElementIdPosition + 1000 < _stream.Length)
                {
                    // Error mode: search for start of next cluster, will be very slow
                    const int maxErrors = 5_000_000;
                    var errors = 0;
                    var max = _stream.Length;
                    while (id != ElementId.Cluster && beforeReadElementIdPosition + 1000 < max)
                    {
                        errors++;
                        if (errors > maxErrors)
                        {
                            return; // we give up
                        }

                        beforeReadElementIdPosition++;
                        _stream.Seek(beforeReadElementIdPosition, SeekOrigin.Begin);
                        id = (ElementId)ReadVariableLengthUInt(false);
                    }
                }

                var size = (long)ReadVariableLengthUInt();
                var element = new Element(id, _stream.Position, size);

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
            if (id == ElementId.None)
            {
                return null;
            }

            var size = (long)ReadVariableLengthUInt();
            return new Element(id, _stream.Position, size);
        }

        private ulong ReadVariableLengthUInt(bool unsetFirstBit = true)
        {
            var first = _stream.ReadByte();
            if (first == -1)
            {
                return 0;
            }

            var length = 0;
            var mask = 0x80;
            for (var i = 0; i < 8; i++)
            {
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

            var result = (ulong)(unsetFirstBit ? first & (0xFF >> length) : first);
            if (length > 1)
            {
                _stream.Read(_buffer, 0, length - 1);
                for (var i = 0; i < length - 1; i++)
                {
                    result = (result << 8) | _buffer[i];
                }
            }
            return result;
        }

        /// <summary>
        /// Reads a fixed length unsigned integer as int from the current stream.
        /// </summary>
        /// <param name="length">The length in bytes of the integer.</param>
        /// <returns>An integer.</returns>
        private int ReadUIntAsInt(long length)
        {
            _stream.Read(_buffer, 0, (int)length);
            var result = 0;
            for (var i = 0; i < length; i++)
            {
                result = (result << 8) | _buffer[i];
            }
            return result;
        }

        /// <summary>
        /// Reads a fixed length unsigned integer as long from the current stream.
        /// </summary>
        /// <param name="length">The length in bytes of the integer.</param>
        /// <returns>A long integer.</returns>
        private long ReadUIntAsLong(long length)
        {
            _stream.Read(_buffer, 0, (int)length);
            var result = 0L;
            for (var i = 0; i < length; i++)
            {
                result = (result << 8) | _buffer[i];
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
            _stream.Read(_buffer, 0, 2);
            return (short)(_buffer[0] << 8 | _buffer[1]);
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream and advances the current
        /// position of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte floating point value read from the current stream.</returns>
        private float ReadFloat32()
        {
            _stream.Read(_buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Span<byte> reversed = stackalloc byte[4] { _buffer[3], _buffer[2], _buffer[1], _buffer[0] };
                return BitConverter.ToSingle(reversed);
            }
            return BitConverter.ToSingle(_buffer.AsSpan(0, 4));
        }

        /// <summary>
        /// Reads a 8-byte floating point value from the current stream and advances the current
        /// position of the stream by eight bytes.
        /// </summary>
        /// <returns>A 8-byte floating point value read from the current stream.</returns>
        private double ReadFloat64()
        {
            _stream.Read(_buffer, 0, 8);
            if (BitConverter.IsLittleEndian)
            {
                Span<byte> reversed = stackalloc byte[8] { _buffer[7], _buffer[6], _buffer[5], _buffer[4], _buffer[3], _buffer[2], _buffer[1], _buffer[0] };
                return BitConverter.ToDouble(reversed);
            }
            return BitConverter.ToDouble(_buffer.AsSpan(0, 8));
        }

        /// <summary>
        /// Reads a fixed length string from the current stream using the specified encoding.
        /// </summary>
        /// <param name="length">The length in bytes of the string.</param>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns>The string being read.</returns>
        private string ReadString(long length, Encoding encoding)
        {
            if (length <= _buffer.Length)
            {
                _stream.Read(_buffer, 0, (int)length);
                return encoding.GetString(_buffer.AsSpan(0, (int)length));
            }

            var buffer = new byte[length];
            _stream.Read(buffer, 0, (int)length);
            return encoding.GetString(buffer.AsSpan()); 
        }
    }
}
