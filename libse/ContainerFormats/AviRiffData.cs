// (c) Giora Tamir (giora@gtamir.com), 2005

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    internal struct AviStreamHeader
    {
        public int FccType; // stream type codes
        public int FccHandler;
        public short Language;
        public int Scale;
        public int Rate; // Rate/Scale is stream tick rate in ticks/s
        public int Start;
        public int Length;
        public int SampleSize;
    }

    internal static class AviRiffData
    {
        // AVI stream FourCC codes
        public static readonly int StreamTypeVideo = RiffParser.ToFourCc("vids");
        public static readonly int StreamTypeAudio = RiffParser.ToFourCc("auds");

        // AVI section FourCC codes
        public static readonly int AviHeaderList = RiffParser.ToFourCc("hdrl");
        public static readonly int MainAviHeader = RiffParser.ToFourCc("avih");
        public static readonly int AviStreamList = RiffParser.ToFourCc("strl");
        public static readonly int AviStreamHeader = RiffParser.ToFourCc("strh");
        public static readonly int InfoList = RiffParser.ToFourCc("INFO");
        public static readonly int AviIsft = RiffParser.ToFourCc("ISFT");
        public const int Mp3 = 0x0055;
        public static readonly int WaveFmt = RiffParser.ToFourCc("fmt ");
    }
}
