namespace Nikse.SubtitleEdit.Core.ContainerFormats.Ebml
{
    internal enum ElementId
    {
        None = 0,

        Ebml = 0x1A45DFA3,
        Segment = 0x18538067,

        Info = 0x1549A966,
        TimecodeScale = 0x2AD7B1,
        Duration = 0x4489,

        Tracks = 0x1654AE6B,
        TrackEntry = 0xAE,
        TrackNumber = 0xD7,
        TrackType = 0x83,
        FlagDefault = 0x88,
        FlagForced = 0x55AA,

        DefaultDuration = 0x23E383,
        Name = 0x536E,
        Language = 0x22B59C,
        CodecId = 0x86,
        CodecPrivate = 0x63A2,
        Video = 0xE0,
        PixelWidth = 0xB0,
        PixelHeight = 0xBA,
        Audio = 0xE1,
        ContentEncodings = 0x6D80,
        ContentEncoding = 0x6240,
        ContentEncodingOrder = 0x5031,
        ContentEncodingScope = 0x5032,
        ContentEncodingType = 0x5033,
        ContentCompression = 0x5034,
        ContentCompAlgo = 0x4254,
        ContentCompSettings = 0x4255,

        Cluster = 0x1F43B675,
        Timecode = 0xE7,
        SimpleBlock = 0xA3,
        BlockGroup = 0xA0,
        Block = 0xA1,
        BlockDuration = 0x9B,

        Chapters = 0x1043A770,
        EditionEntry = 0x45B9,
        ChapterAtom = 0xB6,
        ChapterTimeStart = 0x91,
        ChapterDisplay = 0x80,
        ChapString = 0x85
    }
}
