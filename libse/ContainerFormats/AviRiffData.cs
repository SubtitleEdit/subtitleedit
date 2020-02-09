// (c) Giora Tamir (giora@gtamir.com), 2005

namespace Nikse.SubtitleEdit.Core.ContainerFormats
{
    internal class AviStreamHeader
    {
        public int FccType { get; set; }  // stream type codes
        public int FccHandler { get; set; }
        public int Scale { get; set; }
        public int Rate { get; set; }  // Rate/Scale is stream tick rate in ticks/s
        public int Start { get; set; }
        public int Length { get; set; }
        public int SampleSize { get; set; }
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
