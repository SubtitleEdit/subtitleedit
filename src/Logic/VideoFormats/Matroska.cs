using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Logic
{
    public class SubtitleSequence
    {
        public long StartMilliseconds { get; set; }
        public long EndMilliseconds { get; set; }
        public byte[] BinaryData { get; set; }

        public SubtitleSequence(byte[] data, long startMilliseconds, long endMilliseconds)
        {
            BinaryData = data;
            StartMilliseconds = startMilliseconds;
            EndMilliseconds = endMilliseconds;
        }

        public string Text
        {
            get
            {
                if (BinaryData != null)
                    return System.Text.Encoding.UTF8.GetString(BinaryData).Replace("\\N", Environment.NewLine);
                return string.Empty;
            }
        }
    }

    public class MatroskaSubtitleInfo
    {
        public long TrackNumber { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public string CodecId { get; set; }
        public string CodecPrivate { get; set; }
        public int ContentCompressionAlgorithm { get; set; }
        public int ContentEncodingType { get; set; }
    }

    public class Matroska
    {

        public delegate void LoadMatroskaCallback(long position, long total);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Pack = 2)]
        private struct ByteLayout16
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public byte B1;

            [System.Runtime.InteropServices.FieldOffset(1)]
            public byte B2;

            [System.Runtime.InteropServices.FieldOffset(0)]
            public Int16 IntData16; //16-bit signed int
        }


        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Pack = 2)]
        private struct FloatLayout32
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public byte B1;

            [System.Runtime.InteropServices.FieldOffset(1)]
            public byte B2;

            [System.Runtime.InteropServices.FieldOffset(2)]
            public byte B3;

            [System.Runtime.InteropServices.FieldOffset(3)]
            public byte B4;

            [System.Runtime.InteropServices.FieldOffset(0)]
            public Single FloatData32; //32-bit

            [System.Runtime.InteropServices.FieldOffset(0)]
            public UInt32 UintData32; //32-bit
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Pack = 2)]
        private struct FloatLayout64
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public byte B1;

            [System.Runtime.InteropServices.FieldOffset(1)]
            public byte B2;

            [System.Runtime.InteropServices.FieldOffset(2)]
            public byte B3;

            [System.Runtime.InteropServices.FieldOffset(3)]
            public byte B4;

            [System.Runtime.InteropServices.FieldOffset(4)]
            public byte B5;

            [System.Runtime.InteropServices.FieldOffset(5)]
            public byte B6;

            [System.Runtime.InteropServices.FieldOffset(6)]
            public byte B7;

            [System.Runtime.InteropServices.FieldOffset(7)]
            public byte B8;

            [System.Runtime.InteropServices.FieldOffset(0)]
            public Double FloatData64; //64-bit
        }


        FileStream f;
        int _pixelWidth, _pixelHeight;
        double _frameRate;
        string _videoCodecId;
        double _durationInMilliseconds;

        List<MatroskaSubtitleInfo> _subtitleList;
        int _subtitleRipTrackNumber = 0;
        List<SubtitleSequence> _subtitleRip = new List<SubtitleSequence>();
        //Subtitle _subtitleRip = new Subtitle();


        private static int GetMatroskaVariableIntLength(byte b)
        {
            byte result = 0;

            int a = 255; // 11111111
            a = a | b;
            if (a == 255)
                result = 1;

            a = 127; // 01111111
            a = a | b;
            if (a == 127)
                result = 2;

            a = 63; // 00111111
            a = a | b;
            if (a == 63)
                result = 3;

            a = 31; // 00011111
            a = a | b;
            if (a == 31)
                result = 4;

            a = 15; // 00001111
            a = a | b;
            if (a == 15)
                result = 5;

            a = 7; // 00000111
            a = a | b;
            if (a == 7)
                result = 6;

            a = 3; // 00000011
            a = a | b;
            if (a == 3)
                result = 7;

            a = 1; // 00000001
            a = a | b;
            if (a == 1)
                result = 8;

            return result;
        }

        private UInt32 GetUInt32(byte firstByte)
        {
            var floatLayout = new FloatLayout32();

            floatLayout.B4 = firstByte;
            floatLayout.B3 = (byte)f.ReadByte();
            floatLayout.B2 = (byte)f.ReadByte();
            floatLayout.B1 = (byte)f.ReadByte();

            return floatLayout.UintData32;
        }

        private UInt32 GetMatroskaId()
        {
            byte b = (byte)f.ReadByte();

            if (b == 0xEC || // Void
                b == 0xBF)   // CRC-32
                return b;

            UInt32 x = GetUInt32(b);
            if (x == 0x1A45DFA3 || // ebml header
                x == 0x18538067 || // segment
                x == 0x114D9B74 || // seekhead
                x == 0x1549A966 || // segment info
                x == 0x1654AE6B || // track
                x == 0x1F43B675 || // cluster
                x == 0x1C53BB6B || // Cues
                x == 0x1941A469 || // Attachments
                x == 0x1043A770 || // Chapters
                x == 0x1254C367)   // Tags
                return x;

            return 0;
        }

        private long GetMatroskaDataSize(long sizeOfSize, byte firstByte)
        {
            byte b4, b5, b6, b7, b8;
            long result = 0;

            if (sizeOfSize == 8)
            {
                long i = (long)f.ReadByte() << 48;
                i += (long)f.ReadByte() << 40;
                i += (long)f.ReadByte() << 32;
                i += (long)f.ReadByte() << 24;
                i += (long)f.ReadByte() << 16;
                i += (long)f.ReadByte() << 8;
                i += f.ReadByte();
                return i;
            }
            else if (sizeOfSize == 7)
            {
                long i = firstByte << 48;
                i += (long)f.ReadByte() << 40;
                i += (long)f.ReadByte() << 32;
                i += (long)f.ReadByte() << 24;
                i += (long)f.ReadByte() << 16;
                i += (long)f.ReadByte() << 8;
                i += f.ReadByte();
                return i;
            }
            else if (sizeOfSize == 6)
            {
                firstByte = (byte)(firstByte & 3); // 00000011
                b4 = (byte)f.ReadByte();
                b5 = (byte)f.ReadByte();
                b6 = (byte)f.ReadByte();
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                result = b8 + (b7 * 256) + (b6 * 65536) + (b5 * 16777216) + (b4 * 4294967296) + (firstByte * 1099511627776);
            }
            else if (sizeOfSize == 5)
            {
                firstByte = (byte)(firstByte & 7); // 00000111
                b5 = (byte)f.ReadByte();
                b6 = (byte)f.ReadByte();
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                result = b8 + (b7 * 256) + (b6 * 65536) + (b5 * 16777216) + (firstByte * 4294967296);
            }
            else if (sizeOfSize == 4)
            {
                firstByte = (byte)(firstByte & 15); // 00001111
                b6 = (byte)f.ReadByte();
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                return(long)( b8 + (b7 << 8) + (b6 << 16) + (firstByte << 24));
            }
            else if (sizeOfSize == 3)
            {
                firstByte = (byte)(firstByte & 31); // 00011111
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                return (long) (b8 + (b7 << 8) + (firstByte << 16));
            }
            else if (sizeOfSize == 2)
            {
                firstByte = (byte)(firstByte & 63); // 00111111
                b8 = (byte)f.ReadByte();
                return b8 + (firstByte << 8);
            }
            else if (sizeOfSize == 1)
            {
                return firstByte & 127; //01111111
            }

            return result;
        }

        private long GetMatroskaVariableSizeUnsignedInt(long sizeOfSize)
        {
            byte firstByte = (byte)f.ReadByte();
            byte b3, b4, b5, b6, b7, b8;
            long result = 0;

            if (sizeOfSize >= 8)
            {
                throw new NotImplementedException();
            }
            else if (sizeOfSize == 7)
            {
                b3 = (byte)f.ReadByte();
                b4 = (byte)f.ReadByte();
                b5 = (byte)f.ReadByte();
                b6 = (byte)f.ReadByte();
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                result = b8 + (b7 * 256) + (b6 * 65536) + (b5 * 16777216) + (b4 * 4294967296) + (b3 * 1099511627776) +
                         (firstByte * 281474976710656);

            }
            else if (sizeOfSize == 6)
            {
                b4 = (byte)f.ReadByte();
                b5 = (byte)f.ReadByte();
                b6 = (byte)f.ReadByte();
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                result = b8 + (b7 * 256) + (b6 * 65536) + (b5 * 16777216) + (b4 * 4294967296) + (firstByte * 1099511627776);
            }
            else if (sizeOfSize == 5)
            {
                b5 = (byte)f.ReadByte();
                b6 = (byte)f.ReadByte();
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                result = b8 + (b7 * 256) + (b6 * 65536) + (b5 * 16777216) + (firstByte * 4294967296);
            }
            else if (sizeOfSize == 4)
            {
                b6 = (byte)f.ReadByte();
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                result = b8 + (b7 * 256) + (b6 * 65536) + (firstByte * 16777216);
            }
            else if (sizeOfSize == 3)
            {
                b7 = (byte)f.ReadByte();
                b8 = (byte)f.ReadByte();
                result = b8 + (b7 * 256) + (firstByte * 65536);
            }
            else if (sizeOfSize == 2)
            {
                b8 = (byte)f.ReadByte();
                result = b8 + (firstByte * 256);
            }
            else if (sizeOfSize == 1)
            {
                result = firstByte;
            }

            return result;
        }

        private UInt32 GetMatroskaTracksId()
        {
            byte b = (byte)f.ReadByte();

            if (b == 0xEC || // Void
                b == 0xBF || // CRC-32
                b == 0xAE)   // TrackEntry
                return b;

            return 0;
        }

        private UInt32 GetMatroskaTrackEntryId()
        {
            UInt32 s = (UInt32)f.ReadByte();

            if (s == 0xEC || // Void
                s == 0xBF || // CRC-32
                s == 0xD7 || // Track number
                s == 0x83 || // Track type
                s == 0xB9 || // Flag enabled
                s == 0x88 || // Flag default
                s == 0x9C || // Flag lacing
                s == 0x4F || // Track Time Code Scale
                s == 0xAA || // CodecDecodeAll
                s == 0xE0 || // Track Video
                s == 0xE1 || // Track Audio
                s == 0x86)   // Codec Id
            {
                return s;
            }

            s = s * 256 + (byte)f.ReadByte();
            if (s == 0x73C5 || // TrackUID
                s == 0x55AA || // FlagForced
                s == 0x6DE7 || // MinCache
                s == 0x6DF8 || // MaxCache
                s == 0x55EE || // MaxBlockAdditionID
                s == 0x63A2 || // CodecPrivate
                s == 0x7446 || // AttachmentLink
                s == 0x6D80 || // ContentEncodings
                s == 0x537F || // TrackOffset
                s == 0x6FAB || // TrackOverlay
                s == 0x536E || // Name
                s == 0x6624 || // TrackTranslate
                s == 0x66FC || // TrackTranslateEditionUID
                s == 0x66BF || // TrackTranslateCodec
                s == 0x66A5)   // TrackTranslateTrackID
            {
                return s;
            }

            s = s * 256 + (byte)f.ReadByte();
            if (s == 0x23E383 || // Default Duration
                s == 0x22B59C || // Language
                s == 0x258688 || // CodecName
                s == 0x23314F)   // TrackTimeCodeScale
                return s;

            return 0;
        }

        private UInt32 GetMatroskaTrackVideoId()
        {
            UInt32 s = (byte)f.ReadByte();

            if (s == 0xEC || // Void
                s == 0xBF || // CRC-32
                s == 0xB0 || // PixelWidth
                s == 0xBA || // PixelHeight
                s == 0x9A)   // FlagInterlaced
            {
                return s;
            }

            s = s * 256 + (byte)f.ReadByte();
            if (s == 0x54B0 || // DisplayWidth
                s == 0x54BA || // DisplayHeight
                s == 0x54BA || // DisplayHeight
                s == 0x54AA || // PixelCropButton
                s == 0x54BB || // PixelCropTop
                s == 0x54CC || // PixelCropLeft
                s == 0x54DD || // PixelCropRight
                s == 0x54DD || // PixelCropRight
                s == 0x54B2 || // DisplayUnit
                s == 0x54B3)   // AspectRatioType
                return s;
            s = s * 256 + (byte)f.ReadByte();

            if (s == 0x2EB524)// ColourSpace
                return s;

            return 0;
        }

        private UInt32 GetMatroskaSegmentId()
        {
            byte b = (byte)f.ReadByte();

            if (b == 0xEC)// Void
                return b;

            if (b == 0xBF)// CRC-32
                return b;

            UInt32 s = (UInt32)b * 256 + (byte)f.ReadByte();
            if (s == 0x73A4 || // SegmentUID
                s == 0x7384 || // SegmentFilename
                s == 0x4444 || // SegmentFamily
                s == 0x6924 || // ChapterTranslate
                s == 0x69FC || // ChapterTranslateEditionUID
                s == 0x69BF || // ChapterTranslateCodec
                s == 0x69A5 || // ChapterTranslateID
                s == 0x4489 || // Duration
                s == 0x4461 || // DateUTC
                s == 0x7BA9 || // Title
                s == 0x4D80 || // MuxingApp
                s == 0x5741)   // WritingApp
            {
                return s;
            }

            s = (UInt32)b * 256 + (byte)f.ReadByte();

            if (s == 0x3CB923 || // PrevUID
                s == 0x3C83AB || // PrevFilename
                s == 0x3EB923 || // NextUID
                s == 0x3E83BB || // NextFilename
                s == 0x2AD7B1)   // TimecodeScale
                return s;

            return 0;
        }

        private void AnalyzeMatroskaTrackVideo(long endPosition)
        {
            byte b;
            bool done = false;
            UInt32 matroskaId;
            int sizeOfSize;
            long dataSize;
            long afterPosition;

            while (f.Position < endPosition && done == false)
            {
                matroskaId = GetMatroskaTrackVideoId();
                if (matroskaId == 0)
                    done = true;
                else
                {
                    b = (byte)f.ReadByte();
                    sizeOfSize = GetMatroskaVariableIntLength(b);
                    dataSize = GetMatroskaDataSize(sizeOfSize, b);

                    if (matroskaId == 0xB0) //// PixelWidth
                    {
                        afterPosition = f.Position + dataSize;
                        b = (byte)f.ReadByte();
                        dataSize = GetMatroskaDataSize(dataSize, b);
                        _pixelWidth = (int)dataSize;
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0xBA) // // PixelHeight
                    {
                        afterPosition = f.Position + dataSize;
                        b = (byte)f.ReadByte();
                        dataSize = GetMatroskaDataSize(dataSize, b);
                        _pixelHeight = (int)dataSize;
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else
                        f.Seek(dataSize, SeekOrigin.Current);
                }
            }
        }

        private string GetMatroskaString(long size)
        {
            try
            {
                byte[] buffer = new byte[size];
                f.Read(buffer, 0, (int)size);
                return System.Text.Encoding.UTF8.GetString(buffer);
            }
            catch
            {
                return string.Empty;
            }
        }

        private void AnalyzeMatroskaTrackEntry()
        {
            byte b;
            bool done = false;
            UInt32 matroskaId;
            int sizeOfSize;
            long dataSize;
            long defaultDuration = 0;
            long afterPosition;
            bool isVideo = false;
            bool isSubtitle = false;
            long trackNumber = 0;
            string name = string.Empty;
            string language = string.Empty;
            string codecId = string.Empty;
            string codecPrivate = string.Empty;
            string biCompression = string.Empty;
            int contentCompressionAlgorithm = -1;
            int contentEncodingType = -1;


            while (f.Position < f.Length && done == false)
            {
                matroskaId = GetMatroskaTrackEntryId();
                if (matroskaId == 0)
                    done = true;
                else
                {
                    b = (byte)f.ReadByte();
                    sizeOfSize = GetMatroskaVariableIntLength(b);
                    dataSize = GetMatroskaDataSize(sizeOfSize, b);

                    if (matroskaId == 0x23E383)// Default Duration
                    {
                        afterPosition = f.Position + dataSize;

                        b = (byte)f.ReadByte();
                        defaultDuration = GetMatroskaDataSize(dataSize, b);

                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0xE0)// Track Video
                    {
                        afterPosition = f.Position + dataSize;
                        AnalyzeMatroskaTrackVideo(afterPosition);
                        f.Seek(afterPosition, SeekOrigin.Begin);
                        isVideo = true;
                    }
                    else if (matroskaId == 0xD7) // Track number
                    {
                        afterPosition = f.Position + dataSize;
                        if (dataSize == 1)
                        {
                            trackNumber = (byte)f.ReadByte();
                        }
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0x536E) // Name
                    {
                        afterPosition = f.Position + dataSize;
                        name = GetMatroskaString(dataSize);
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0x22B59C) // Language
                    {
                        afterPosition = f.Position + dataSize;
                        language = GetMatroskaString(dataSize);
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0x86) // CodecId
                    {
                        afterPosition = f.Position + dataSize;
                        codecId = GetMatroskaString(dataSize);
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0x83) // Track type
                    {
                        afterPosition = f.Position + dataSize;
                        if (dataSize == 1)
                        {
                            byte trackType = (byte)f.ReadByte();
                            if (trackType == 0x11) // subtitle
                                isSubtitle = true;
                        }
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0x63A2) // CodecPrivate
                    {
                        afterPosition = f.Position + dataSize;
                        codecPrivate = GetMatroskaString(dataSize);
                        if (codecPrivate.Length > 20)
                            biCompression = codecPrivate.Substring(16, 4);
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0x6D80) // ContentEncodings
                    {
                        afterPosition = f.Position + dataSize;

                        contentCompressionAlgorithm = 0; // default value
                        contentEncodingType = 0; // default value

                        int contentEncoding1  = f.ReadByte();
                        int contentEncoding2  = f.ReadByte();

                        if (contentEncoding1 == 0x62 && contentEncoding2 == 0x40)
                        {
                            AnalyzeMatroskaContentEncoding(afterPosition, ref contentCompressionAlgorithm, ref contentEncodingType);
                        }
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }

                    else
                        f.Seek(dataSize, SeekOrigin.Current);
                }
            }
            if (isVideo)
            {
                if (defaultDuration > 0)
                    _frameRate = 1.0 / (defaultDuration / 1000000000.0);
                _videoCodecId = codecId + " " + biCompression.Trim();
            }
            else if (isSubtitle)
            {
                _subtitleList.Add(new MatroskaSubtitleInfo
                {
                    Name = name,
                    TrackNumber = trackNumber,
                    CodecId = codecId,
                    Language = language,
                    CodecPrivate = codecPrivate,
                    ContentEncodingType = contentEncodingType,
                    ContentCompressionAlgorithm = contentCompressionAlgorithm
                } );
            }
        }

        private void AnalyzeMatroskaContentEncoding(long endPosition, ref int contentCompressionAlgorithm, ref int contentEncodingType)
        {
            bool done = false;

            while (f.Position < endPosition && done == false)
            {
                int ebmlId = f.ReadByte() * 256 + f.ReadByte();

                if (ebmlId == 0)
                    done = true;
                else
                {
                    if (ebmlId == 0x5031)// ContentEncodingOrder
                    {
                        int contentEncodingOrder = f.ReadByte() * 256 + f.ReadByte();
                        System.Diagnostics.Debug.WriteLine("ContentEncodingOrder: " + contentEncodingOrder.ToString());
                    }
                    else if (ebmlId == 0x5032)// ContentEncodingScope
                    {
                        int contentEncodingScope = f.ReadByte() * 256 + f.ReadByte();
                        System.Diagnostics.Debug.WriteLine("ContentEncodingScope: " + contentEncodingScope.ToString());
                    }
                    else if (ebmlId == 0x5033)// ContentEncodingType
                    {
                        contentEncodingType = f.ReadByte() * 256 + f.ReadByte();
                    }
                    else if (ebmlId == 0x5034)// ContentCompression
                    {
                        byte b = (byte)f.ReadByte();
                        long sizeOfSize = GetMatroskaVariableIntLength(b);
                        long dataSize = GetMatroskaDataSize(sizeOfSize, b);
                        long afterPosition = f.Position + dataSize;
                        while (f.Position < afterPosition)
                        {
                            int contentCompressionId = f.ReadByte() * 256 + f.ReadByte();
                            if (contentCompressionId == 0x4254)
                            {
                                contentCompressionAlgorithm = f.ReadByte() * 256 + f.ReadByte();
                            }
                            else if (contentCompressionId == 0x4255)
                            {
                                int contentCompSettings = f.ReadByte() * 256 + f.ReadByte();
                                System.Diagnostics.Debug.WriteLine("contentCompSettings: " + contentCompSettings.ToString());
                            }

                        }
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                }
            }
        }


        private void AnalyzeMatroskaSegmentInformation(long endPosition)
        {
            byte b;
            bool done = false;
            UInt32 matroskaId;
            int sizeOfSize;
            long dataSize;
            long afterPosition;
            long timeCodeScale = 0;
            double duration8b = 0;

            while (f.Position < endPosition && done == false)
            {
                matroskaId = GetMatroskaSegmentId();
                if (matroskaId == 0)
                    done = true;
                else
                {
                    b = (byte)f.ReadByte();
                    sizeOfSize = GetMatroskaVariableIntLength(b);
                    dataSize = GetMatroskaDataSize(sizeOfSize, b);

                    if (matroskaId == 0x2AD7B1)// TimecodeScale - u-integer     Timecode scale in nanoseconds (1.000.000 means all timecodes in the segment are expressed in milliseconds).
                    {
                        afterPosition = f.Position + dataSize;
                        b = (byte)f.ReadByte();
                        timeCodeScale = GetMatroskaDataSize(dataSize, b);
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0x4489)// Duration (float)
                    {
                        afterPosition = f.Position + dataSize;

                        if (dataSize == 4)
                        {
                            duration8b = GetFloat32();
                        }
                        else
                        {
                            duration8b = GetFloat64();
                        }

                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else
                        f.Seek(dataSize, SeekOrigin.Current);
                }
            }
            if (timeCodeScale > 0 && duration8b > 0)
                _durationInMilliseconds = duration8b / timeCodeScale * 1000000.0;
            else if (duration8b > 0)
                _durationInMilliseconds = duration8b;
        }

        private double GetFloat32()
        {
            FloatLayout32 floatLayout = new FloatLayout32();

            // reverse byte ordering
            floatLayout.B4 = (byte)f.ReadByte();
            floatLayout.B3 = (byte)f.ReadByte();
            floatLayout.B2 = (byte)f.ReadByte();
            floatLayout.B1 = (byte)f.ReadByte();

            return floatLayout.FloatData32;
        }

        private double GetFloat64()
        {
            FloatLayout64 floatLayout = new FloatLayout64();

            // reverse byte ordering
            floatLayout.B8 = (byte)f.ReadByte();
            floatLayout.B7 = (byte)f.ReadByte();
            floatLayout.B6 = (byte)f.ReadByte();
            floatLayout.B5 = (byte)f.ReadByte();
            floatLayout.B4 = (byte)f.ReadByte();
            floatLayout.B3 = (byte)f.ReadByte();
            floatLayout.B2 = (byte)f.ReadByte();
            floatLayout.B1 = (byte)f.ReadByte();

            return floatLayout.FloatData64;
        }

        private void AnalyzeMatroskaTracks()
        {
            byte b;
            bool done = false;
            UInt32 matroskaId;
            int sizeOfSize;
            long dataSize;
            long afterPosition;
            _subtitleList = new List<MatroskaSubtitleInfo>();

            while (f.Position < f.Length && done == false)
            {
                matroskaId = GetMatroskaTracksId();
                if (matroskaId == 0)
                    done = true;
                else
                {
                    b = (byte)f.ReadByte();
                    sizeOfSize = GetMatroskaVariableIntLength(b);
                    dataSize = GetMatroskaDataSize(sizeOfSize, b);

                    if (matroskaId == 0xAE)
                    {
                        afterPosition = f.Position + dataSize;
                        AnalyzeMatroskaTrackEntry();
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else
                        f.Seek(dataSize, SeekOrigin.Current);
                }
            }
        }

        public void GetMatroskaInfo(string fileName,
                                   ref bool isValid,
                                   ref bool hasConstantFrameRate,
                                   ref double frameRate,
                                   ref int pixelWidth,
                                   ref int pixelHeight,
                                   ref double millisecsDuration,
                                   ref string videoCodec)
        {
            byte b;
            bool done;
            UInt32 matroskaId;
            int sizeOfSize;
            long dataSize;
            long afterPosition;
            bool endOfFile;

            _durationInMilliseconds = 0;

            f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            matroskaId = GetMatroskaId();
            if (matroskaId != 0x1A45DFA3) // matroska file must start with ebml header
            {
                isValid = false;
            }
            else
            {
                isValid = true;
                b = (byte)f.ReadByte();
                sizeOfSize = GetMatroskaVariableIntLength(b);
                dataSize = GetMatroskaDataSize(sizeOfSize, b);

                f.Seek(dataSize, SeekOrigin.Current);

                done = false;
                endOfFile = false;
                while (endOfFile == false && done == false)
                {
                    matroskaId = GetMatroskaId();
                    if (matroskaId == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        b = (byte)f.ReadByte();
                        sizeOfSize = GetMatroskaVariableIntLength(b);
                        dataSize = GetMatroskaDataSize(sizeOfSize, b);

                        if (matroskaId == 0x1549A966) // segment info
                        {
                            afterPosition = f.Position + dataSize;
                            AnalyzeMatroskaSegmentInformation(afterPosition);
                            f.Seek(afterPosition, SeekOrigin.Begin);
                        }
                        else if (matroskaId == 0x1654AE6B)  // tracks
                        {
                            afterPosition = f.Position + dataSize;
                            AnalyzeMatroskaTracks();
                            f.Seek(afterPosition, SeekOrigin.Begin);
                            done = true;
                        }
                        else if (matroskaId == 0x1F43B675) // cluster
                        {
                            afterPosition = f.Position + dataSize;
                            //if (f.Position > 8000000)
                            //    System.Windows.Forms.MessageBox.Show("8mb");
                            AnalyzeMatroskaCluster();
                            f.Seek(afterPosition, SeekOrigin.Begin);
                        }
                        else if (matroskaId != 0x18538067) // segment
                        {
                            f.Seek(dataSize, SeekOrigin.Current);
                        }
                    }

                    endOfFile = f.Position >= f.Length;
                }

            }
            f.Close();
            f.Dispose();
            f = null;

            pixelWidth = _pixelWidth;
            pixelHeight = _pixelHeight;
            frameRate = _frameRate;
            hasConstantFrameRate = _frameRate > 0;
            millisecsDuration = _durationInMilliseconds;
            videoCodec = _videoCodecId;
        }

        private UInt32 GetMatroskaClusterId()
        {
            UInt32 s = (byte)f.ReadByte();

            if (s == 0xE7 || // TimeCode
                s == 0xA7 || // Position
                s == 0xAB || // PrevSize
                s == 0xA0 || // BlockGroup
                s == 0xA1 || // Block
                s == 0xA2 || // BlockVirtual
                s == 0xA6 || // BlockMore
                s == 0xEE || // BlockAddID
                s == 0xA5 || // BlockAdditional
                s == 0x9B || // BlockDuration
                s == 0xFA || // ReferencePriority
                s == 0xFB || // ReferenceBlock
                s == 0xFD || // ReferenceVirtual
                s == 0xA4 || // CodecState
                s == 0x8E || // Slices
                s == 0x8E || // TimeSlice
                s == 0xCC || // LaceNumber
                s == 0xCD || // FrameNumber
                s == 0xCB || // BlockAdditionID
                s == 0xCE || // Delay
                s == 0xCF || // Duration
                s == 0xA3)   // SimpleBlock
            {
                return s;
            }

            s = s * 256 + (byte)f.ReadByte();

            if (s == 0x5854 || // SilentTracks
                s == 0x58D7 || // SilentTrackNumber
                s == 0x75A1) // BlockAdditions
                return s;

            return 0;
        }

        private void AnalyzeMatroskaCluster()
        {
            byte b;
            bool done = false;
            UInt32 matroskaId;
            int sizeOfSize;
            long dataSize;
            long afterPosition;
            long clusterTimeCode = 0;
            long duration = 0;

            while (f.Position < f.Length && done == false)
            {
                matroskaId = GetMatroskaClusterId();

                if (matroskaId == 0)
                    done = true;
                else
                {
                    b = (byte)f.ReadByte();
                    sizeOfSize = GetMatroskaVariableIntLength(b);
                    dataSize = GetMatroskaDataSize(sizeOfSize, b);

                    if (matroskaId == 0xE7) // Timecode
                    {
                        afterPosition = f.Position + dataSize;
                        clusterTimeCode = GetMatroskaVariableSizeUnsignedInt(dataSize);
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0xA0) // BlockGroup
                    {
                        afterPosition = f.Position + dataSize;
                        AnalyzeMatroskaBlock(clusterTimeCode);
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else if (matroskaId == 0xA3) // SimpleBlock
                    {
                        afterPosition = f.Position + dataSize;
                        long before = f.Position;
                        b = (byte)f.ReadByte();
                        int sizeOftrackNumber = GetMatroskaVariableIntLength(b);
                        long trackNumber = GetMatroskaDataSize(sizeOftrackNumber, b);

                        if (trackNumber == _subtitleRipTrackNumber)
                        {
                            int timeCode = GetInt16();

                            // lacing
                            byte flags = (byte)f.ReadByte();
                            byte numberOfFrames = 0;
                            switch ((flags & 6))  // 6 = 00000110
                            {
                                case 0: System.Diagnostics.Debug.Print("No lacing");   // No lacing
                                    break;
                                case 2: System.Diagnostics.Debug.Print("Xiph lacing"); // 2 = 00000010 = Xiph lacing
                                    numberOfFrames = (byte)f.ReadByte();
                                    numberOfFrames++;
                                    break;
                                case 4: System.Diagnostics.Debug.Print("fixed-size");  // 4 = 00000100 = Fixed-size lacing
                                    numberOfFrames = (byte)f.ReadByte();
                                    numberOfFrames++;
                                    for (int i = 1; i <= numberOfFrames; i++)
                                        b = (byte)f.ReadByte(); // frames
                                    break;
                                case 6: System.Diagnostics.Debug.Print("EBML");        // 6 = 00000110 = EMBL
                                    numberOfFrames = (byte)f.ReadByte();
                                    numberOfFrames++;
                                    break;
                            }

                            byte[] buffer = new byte[dataSize - (f.Position - before)];
                            f.Read(buffer, 0, buffer.Length);
                            _subtitleRip.Add(new SubtitleSequence(buffer, timeCode + clusterTimeCode, timeCode + clusterTimeCode + duration));

                        }
                        f.Seek(afterPosition, SeekOrigin.Begin);
                    }
                    else
                        f.Seek(dataSize, SeekOrigin.Current);
                }
            }
        }

        private void AnalyzeMatroskaBlock(long clusterTimeCode)
        {
            long duration = 0;
            byte b = (byte)f.ReadByte();
            if (b == 0xA1) // Block
            {
                b = (byte)f.ReadByte();
                int sizeOfSize = GetMatroskaVariableIntLength(b);
                long dataSize = GetMatroskaDataSize(sizeOfSize, b);

                long afterPosition = f.Position + dataSize;

                // track number
                b = (byte)f.ReadByte();
                int sizeOfTrackNo = GetMatroskaVariableIntLength(b);
                long trackNo = GetMatroskaDataSize(sizeOfTrackNo, b);

                // time code
                Int16 timeCode = GetInt16();

                // lacing
                byte flags = (byte)f.ReadByte();
                byte numberOfFrames = 0;
                switch ((flags & 6))  // 6 = 00000110
                {
                    case 0: System.Diagnostics.Debug.Print("No lacing");   // No lacing
                     break;
                    case 2: System.Diagnostics.Debug.Print("Xiph lacing"); // 2 = 00000010 = Xiph lacing
                     numberOfFrames = (byte)f.ReadByte();
                     numberOfFrames++;
                     break;
                    case 4: System.Diagnostics.Debug.Print("fixed-size");  // 4 = 00000100 = Fixed-size lacing
                     numberOfFrames = (byte)f.ReadByte();
                     numberOfFrames++;
                     for (int i = 1; i <= numberOfFrames; i++)
                         b = (byte)f.ReadByte(); // frames
                     break;
                    case 6: System.Diagnostics.Debug.Print("EBML");        // 6 = 00000110 = EMBL
                     numberOfFrames = (byte)f.ReadByte();
                     numberOfFrames++;
                     break;
                }

                // save subtitle data
                if (trackNo == _subtitleRipTrackNumber)
                {
                    long sublength = afterPosition - f.Position;
                    if (sublength > 0)
                    {
                        byte[] buffer = new byte[sublength];
                        f.Read(buffer, 0, (int)sublength);

                        //string s = GetMatroskaString(sublength);
                        //s = s.Replace("\\N", Environment.NewLine);

                        f.Seek(afterPosition, SeekOrigin.Begin);
                        b = (byte)f.ReadByte();
                        if (b == 0x9B) // BlockDuration
                        {
                            b = (byte)f.ReadByte();
                            sizeOfSize = GetMatroskaVariableIntLength(b);
                            dataSize = GetMatroskaDataSize(sizeOfSize, b);
                            duration = GetMatroskaVariableSizeUnsignedInt(dataSize);
                        }

                        _subtitleRip.Add(new SubtitleSequence(buffer, timeCode + clusterTimeCode, timeCode + clusterTimeCode + duration));
                    }
                }

            }
        }

        private Int16 GetInt16()
        {
            ByteLayout16 l16 = new ByteLayout16();
            l16.B2 = (byte)f.ReadByte();
            l16.B1 = (byte)f.ReadByte();
            return l16.IntData16;
        }

        public List<MatroskaSubtitleInfo> GetMatroskaSubtitleTracks(string fileName, out bool isValid)
        {
            byte b;
            bool done;
            UInt32 matroskaId;
            int sizeOfSize;
            long dataSize;
            long afterPosition;
            bool endOfFile;

            f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            matroskaId = GetMatroskaId();
            if (matroskaId != 0x1A45DFA3) // matroska file must start with ebml header
            {
                isValid = false;
            }
            else
            {
                isValid = true;
                b = (byte)f.ReadByte();
                sizeOfSize = GetMatroskaVariableIntLength(b);
                dataSize = GetMatroskaDataSize(sizeOfSize, b);

                f.Seek(dataSize, SeekOrigin.Current);

                done = false;
                endOfFile = false;
                while (endOfFile == false && done == false)
                {
                    matroskaId = GetMatroskaId();
                    if (matroskaId == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        b = (byte)f.ReadByte();
                        sizeOfSize = GetMatroskaVariableIntLength(b);
                        dataSize = GetMatroskaDataSize(sizeOfSize, b);

                        if (matroskaId == 0x1549A966) // segment info
                        {
                            afterPosition = f.Position + dataSize;
                            AnalyzeMatroskaSegmentInformation(afterPosition);
                            f.Seek(afterPosition, SeekOrigin.Begin);
                        }
                        else if (matroskaId == 0x1654AE6B)  // tracks
                        {
                            afterPosition = f.Position + dataSize;
                            AnalyzeMatroskaTracks();
                            f.Seek(afterPosition, SeekOrigin.Begin);
                            done = true;
                        }
                        else if (matroskaId != 0x18538067) // segment
                        {
                            f.Seek(dataSize, SeekOrigin.Current);
                        }
                    }

                    endOfFile = f.Position >= f.Length;
                }

            }
            f.Close();
            f.Dispose();
            f = null;

            return _subtitleList;
        }

        public List<SubtitleSequence> GetMatroskaSubtitle(string fileName, int trackNumber, out bool isValid, Matroska.LoadMatroskaCallback callback)
        {
            byte b;
            bool done;
            UInt32 matroskaId;
            int sizeOfSize;
            long dataSize;
            long afterPosition;
            bool endOfFile;
            _subtitleRipTrackNumber = trackNumber;

            f = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            matroskaId = GetMatroskaId();
            if (matroskaId != 0x1A45DFA3) // matroska file must start with ebml header
            {
                isValid = false;
            }
            else
            {
                isValid = true;
                b = (byte)f.ReadByte();
                sizeOfSize = GetMatroskaVariableIntLength(b);
                dataSize = GetMatroskaDataSize(sizeOfSize, b);

                f.Seek(dataSize, SeekOrigin.Current);

                done = false;
                endOfFile = false;
                while (endOfFile == false && done == false)
                {
                    matroskaId = GetMatroskaId();
                    if (matroskaId == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        b = (byte)f.ReadByte();
                        sizeOfSize = GetMatroskaVariableIntLength(b);
                        dataSize = GetMatroskaDataSize(sizeOfSize, b);

                        if (matroskaId == 0x1549A966) // segment info
                        {
                            afterPosition = f.Position + dataSize;
                            AnalyzeMatroskaSegmentInformation(afterPosition);
                            f.Seek(afterPosition, SeekOrigin.Begin);
                        }
                        else if (matroskaId == 0x1654AE6B)  // tracks
                        {
                            afterPosition = f.Position + dataSize;
                            AnalyzeMatroskaTracks();
                            f.Seek(afterPosition, SeekOrigin.Begin);
                        }
                        else if (matroskaId == 0x1F43B675) // cluster
                        {
                            afterPosition = f.Position + dataSize;
                            AnalyzeMatroskaCluster();
                            f.Seek(afterPosition, SeekOrigin.Begin);
                        }
                        else if (matroskaId != 0x18538067) // segment
                        {
                            f.Seek(dataSize, SeekOrigin.Current);
                        }
                    }
                    if (callback != null)
                        callback.Invoke(f.Position, f.Length);
                    endOfFile = f.Position >= f.Length;
                }
            }
            f.Close();
            f.Dispose();
            f = null;

            return _subtitleRip;
        }

     }
}
