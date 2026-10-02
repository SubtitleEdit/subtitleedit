using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// Reads a DVD IFO file. For a video title set (VTS_xx_0.IFO) it reads the video, audio and
    /// subpicture attributes and every program chain (duration, cells with their sector ranges,
    /// palette and which 0x20-0x3f subpicture stream carries each language's 4:3/wide/letterbox/
    /// pan&amp;scan variant). A video manager (VIDEO_TS.IFO) is recognized, so callers can say that the
    /// title set IFO is needed instead.
    /// Layout: http://dvd.sourceforge.net/dvdinfo/ifo.html and http://dvd.sourceforge.net/dvdinfo/pgc.html
    /// </summary>
    public class IfoParser
    {
        public enum IfoType
        {
            Unknown,
            VideoTitleSet,
            VideoManager,
        }

        public enum SubtitleVariant
        {
            Normal,
            Wide,
            Letterbox,
            PanScan,
        }

        public class VideoAttributes
        {
            /// <summary>"MPEG-1" or "MPEG-2".</summary>
            public string CodingMode { get; set; }
            public bool IsPal { get; set; }
            public bool IsWideScreen { get; set; }
            public bool IsLetterboxed { get; set; }
            public bool PanScanAllowed { get; set; }
            public bool LetterboxAllowed { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public double FrameRate => IsPal ? 25 : 29.97;
            public string AspectRatio => IsWideScreen ? "16:9" : "4:3";
            public string Standard => IsPal ? "PAL" : "NTSC";

            public override string ToString()
            {
                return $"{CodingMode} {Standard} {Width}x{Height} {AspectRatio}";
            }
        }

        public class AudioStream
        {
            public int Index { get; set; }

            /// <summary>"AC3", "MPEG-1", "MPEG-2", "LPCM", "DTS" (or "Unknown").</summary>
            public string CodingMode { get; set; }
            public int Channels { get; set; }
            public int SampleRate { get; set; }
            public string LanguageCode { get; set; }
            public string Language { get; set; }

            /// <summary>1 = normal, 2 = for visually impaired, 3/4 = director's comments.</summary>
            public int CodeExtension { get; set; }

            public override string ToString()
            {
                return $"{Language} {CodingMode} {Channels}ch";
            }
        }

        public class SubtitleStream
        {
            /// <summary>Index in the IFO subpicture attribute table (the "subtitle number" of a player).</summary>
            public int Index { get; set; }
            public string LanguageCode { get; set; }
            public string Language { get; set; }

            /// <summary>
            /// 1 normal, 2 large, 3 children, 5-7 closed caption, 9 forced, 13-15 director's comments.
            /// </summary>
            public int CodeExtension { get; set; }

            public bool IsForced => CodeExtension == 9;
            public bool IsClosedCaption => CodeExtension >= 5 && CodeExtension <= 7;
            public bool IsDirectorsComments => CodeExtension >= 13 && CodeExtension <= 15;

            /// <summary>
            /// The subpicture stream id (0x20-0x3f) used per display variant, from the first program
            /// chain that has the stream. A 4:3 title only has <see cref="SubtitleVariant.Normal"/>.
            /// </summary>
            public Dictionary<SubtitleVariant, int> StreamIds { get; } = new Dictionary<SubtitleVariant, int>();

            public string CodeExtensionName
            {
                get
                {
                    if (IsForced)
                    {
                        return "forced";
                    }

                    if (IsClosedCaption)
                    {
                        return "closed caption";
                    }

                    if (IsDirectorsComments)
                    {
                        return "director's comments";
                    }

                    if (CodeExtension == 2)
                    {
                        return "large";
                    }

                    return CodeExtension == 3 ? "children" : string.Empty;
                }
            }

            public override string ToString()
            {
                var extension = CodeExtensionName;
                return string.IsNullOrEmpty(extension) ? Language : $"{Language} ({extension})";
            }
        }

        public class Cell
        {
            public int VobId { get; set; }
            public int CellId { get; set; }
            public TimeSpan Duration { get; set; }
            public TimeSpan Start { get; set; }

            /// <summary>Sector (2048 bytes) of the cell's first VOBU, counted from the start of the title VOBs (VTS_xx_1.VOB).</summary>
            public long FirstSector { get; set; }
            public long LastSector { get; set; }
        }

        public class ProgramChain
        {
            public int Number { get; set; }
            public TimeSpan Duration { get; set; }
            public int ProgramCount { get; set; }
            public List<Cell> Cells { get; } = new List<Cell>();

            /// <summary>Start of each program (chapter) in the chain.</summary>
            public List<TimeSpan> ProgramStarts { get; } = new List<TimeSpan>();
            public List<SKColor> Palette { get; set; } = new List<SKColor>();

            /// <summary>Subpicture stream control, 32 x 4 bytes (available bit + stream number per variant).</summary>
            public byte[] SubtitleControl { get; set; } = new byte[32 * 4];

            public bool HasSubtitleStream(int index)
            {
                return index >= 0 && index < 32 && (SubtitleControl[index * 4] & 0x80) != 0;
            }

            public override string ToString()
            {
                return $"PGC {Number}: {Duration:hh\\:mm\\:ss}, {ProgramCount} chapters, {Cells.Count} cells";
            }
        }

        private const int SectorSize = 2048;
        private const string VtsIdentifier = "DVDVIDEO-VTS";
        private const string VmgIdentifier = "DVDVIDEO-VMG";

        public IfoType Type { get; private set; }
        public VideoAttributes Video { get; } = new VideoAttributes { CodingMode = "MPEG-2", IsPal = true, Width = 720, Height = 576 };
        public List<AudioStream> AudioStreams { get; } = new List<AudioStream>();
        public List<SubtitleStream> SubtitleStreams { get; } = new List<SubtitleStream>();
        public List<ProgramChain> ProgramChains { get; } = new List<ProgramChain>();

        /// <summary>Number of title sets on the disc (only for a video manager, VIDEO_TS.IFO).</summary>
        public int TitleSetCount { get; private set; }

        public string ErrorMessage { get; private set; }

        public bool IsPal => Video.IsPal;

        /// <summary>The palette of the first program chain that has one (empty if none).</summary>
        public List<SKColor> Palette => ProgramChains.Select(p => p.Palette).FirstOrDefault(p => p.Count > 0) ?? new List<SKColor>();

        public IfoParser(string fileName)
        {
            try
            {
                Parse(FileUtil.ReadAllBytesShared(fileName));
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        public IfoParser(byte[] buffer)
        {
            try
            {
                Parse(buffer);
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message;
            }
        }

        public static bool IsIfo(string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var buffer = new byte[VtsIdentifier.Length];
                    if (fs.Read(buffer, 0, buffer.Length) != buffer.Length)
                    {
                        return false;
                    }

                    var id = Encoding.ASCII.GetString(buffer);
                    return id == VtsIdentifier || id == VmgIdentifier;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Every subpicture stream id with a display name in the idx language format
        /// "{Language} (0x{id:x})" - e.g. "English (0x20)", "English, letterbox (0x21)".
        /// </summary>
        public List<string> GetLanguages()
        {
            var list = new List<string>();
            var used = new HashSet<int>();

            // primary variant first, so a letterbox/pan&scan id shared with another language keeps that language
            foreach (var primary in new[] { true, false })
            {
                foreach (var stream in SubtitleStreams)
                {
                    foreach (var pair in stream.StreamIds)
                    {
                        var isPrimary = pair.Key == SubtitleVariant.Normal || pair.Key == SubtitleVariant.Wide;
                        if (isPrimary != primary || !used.Add(pair.Value))
                        {
                            continue;
                        }

                        var name = stream.ToString();
                        if (pair.Key == SubtitleVariant.Letterbox)
                        {
                            name += ", letterbox";
                        }
                        else if (pair.Key == SubtitleVariant.PanScan)
                        {
                            name += ", pan&scan";
                        }

                        list.Add($"{name} (0x{pair.Value:x})");
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// The two letter language code of a subpicture stream id (or null).
        /// </summary>
        public string GetLanguageCode(int streamId)
        {
            return SubtitleStreams.FirstOrDefault(p => p.StreamIds.ContainsValue(streamId))?.LanguageCode;
        }

        /// <summary>
        /// The title's VOB files, in play order: VTS_01_0.IFO → VTS_01_1.VOB, VTS_01_2.VOB, ...
        /// (VTS_01_0.VOB is the title set menu and is not included).
        /// </summary>
        public static List<string> GetTitleVobFiles(string ifoFileName)
        {
            var list = new List<string>();
            var folder = Path.GetDirectoryName(ifoFileName) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(ifoFileName);
            if (name.Length < 2 || !name.EndsWith("0", StringComparison.Ordinal))
            {
                return list;
            }

            var prefix = name.Substring(0, name.Length - 1);
            string[] folderFiles = null;
            for (var i = 1; i < 30; i++)
            {
                var vobFileName = FindFile(folder, prefix + i.ToString(CultureInfo.InvariantCulture) + ".VOB", ref folderFiles);
                if (vobFileName == null)
                {
                    break;
                }

                list.Add(vobFileName);
            }

            return list;
        }

        /// <summary>
        /// The IFO of a title VOB: VTS_01_3.VOB → VTS_01_0.IFO (or null when there is none).
        /// </summary>
        public static string GetIfoFileName(string vobFileName)
        {
            var folder = Path.GetDirectoryName(vobFileName) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(vobFileName);
            var lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore <= 0)
            {
                return null;
            }

            // a backup copy works just as well
            string[] folderFiles = null;
            var baseName = name.Substring(0, lastUnderscore + 1) + "0";
            return FindFile(folder, baseName + ".IFO", ref folderFiles) ?? FindFile(folder, baseName + ".BUP", ref folderFiles);
        }

        /// <summary>
        /// A file in <paramref name="folder"/> named <paramref name="fileName"/> ignoring case (a DVD
        /// copied to a case sensitive file system may be all lowercase: video_ts/vts_01_1.vob), or null.
        /// </summary>
        /// <param name="folderFiles">The folder's files, listed on the first miss and reused.</param>
        internal static string FindFile(string folder, string fileName, ref string[] folderFiles)
        {
            var path = Path.Combine(folder, fileName);
            if (File.Exists(path))
            {
                return path;
            }

            if (folderFiles == null)
            {
                try
                {
                    folderFiles = Directory.GetFiles(string.IsNullOrEmpty(folder) ? "." : folder);
                }
                catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
                {
                    folderFiles = Array.Empty<string>();
                }
            }

            foreach (var file in folderFiles)
            {
                if (string.Equals(Path.GetFileName(file), fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            return null;
        }

        private void Parse(byte[] buffer)
        {
            if (buffer.Length < 0x400)
            {
                ErrorMessage = "File too small for a DVD IFO file";
                return;
            }

            var id = Encoding.ASCII.GetString(buffer, 0, VtsIdentifier.Length);
            if (id == VmgIdentifier)
            {
                Type = IfoType.VideoManager;
                TitleSetCount = GetWord(buffer, 0x3E);
                return;
            }

            if (id != VtsIdentifier)
            {
                ErrorMessage = "Not a DVD IFO file";
                return;
            }

            Type = IfoType.VideoTitleSet;
            ParseVideoAttributes(GetWord(buffer, 0x200));
            ParseAudioAttributes(buffer);
            ParseSubtitleAttributes(buffer);

            var cellAddresses = ParseCellAddressTable(buffer);
            var pgciSector = GetDWord(buffer, 0xCC);
            if (pgciSector > 0)
            {
                ParseProgramChains(buffer, pgciSector * SectorSize, cellAddresses);
            }

            ResolveSubtitleStreamIds();
        }

        /// <summary>
        /// Video attributes (2 bytes): coding mode (2 bits), standard (2), aspect (2), automatic
        /// pan&amp;scan / letterbox disallowed (1+1), CC fields (2), bit rate (1), reserved (1),
        /// resolution (3), letterboxed (1), film/camera (1).
        /// </summary>
        private void ParseVideoAttributes(int video)
        {
            Video.CodingMode = ((video >> 14) & 0b11) == 0 ? "MPEG-1" : "MPEG-2";
            Video.IsPal = ((video >> 12) & 0b11) == 1;
            Video.IsWideScreen = ((video >> 10) & 0b11) == 3;
            Video.PanScanAllowed = (video & 0x200) == 0;
            Video.LetterboxAllowed = (video & 0x100) == 0;
            Video.IsLetterboxed = (video & 0b100) != 0;
            Video.Height = Video.IsPal ? 576 : 480;
            switch ((video >> 3) & 0b111)
            {
                case 1:
                    Video.Width = 704;
                    break;
                case 2:
                    Video.Width = 352;
                    break;
                case 3:
                    Video.Width = 352;
                    Video.Height /= 2;
                    break;
                default:
                    Video.Width = 720;
                    break;
            }
        }

        /// <summary>
        /// Audio attributes: count at 0x202, then 8 entries of 8 bytes.
        /// </summary>
        private void ParseAudioAttributes(byte[] buffer)
        {
            var count = Math.Min(8, GetWord(buffer, 0x202));
            for (var i = 0; i < count; i++)
            {
                var index = 0x204 + i * 8;
                var codingMode = buffer[index] >> 5;
                var hasLanguage = ((buffer[index] >> 2) & 0b11) == 1;
                var languageCode = hasLanguage ? GetLanguageCode(buffer, index + 2) : string.Empty;
                AudioStreams.Add(new AudioStream
                {
                    Index = i,
                    CodingMode = codingMode == 0 ? "AC3" : codingMode == 2 ? "MPEG-1" : codingMode == 3 ? "MPEG-2" : codingMode == 4 ? "LPCM" : codingMode == 6 ? "DTS" : "Unknown",
                    Channels = (buffer[index + 1] & 0b111) + 1,
                    SampleRate = ((buffer[index + 1] >> 4) & 0b11) == 1 ? 96000 : 48000,
                    LanguageCode = languageCode,
                    Language = GetLanguageName(languageCode),
                    CodeExtension = buffer[index + 5],
                });
            }
        }

        /// <summary>
        /// Subpicture attributes: count at 0x254, then 32 entries of 6 bytes
        /// (coding mode + language type, reserved, language code (2), reserved, code extension).
        /// </summary>
        private void ParseSubtitleAttributes(byte[] buffer)
        {
            var count = Math.Min(32, GetWord(buffer, 0x254));
            for (var i = 0; i < count; i++)
            {
                var index = 0x256 + i * 6;
                var hasLanguage = (buffer[index] & 0b11) == 1;
                var languageCode = hasLanguage || IsLanguageCode(buffer, index + 2) ? GetLanguageCode(buffer, index + 2) : string.Empty;
                SubtitleStreams.Add(new SubtitleStream
                {
                    Index = i,
                    LanguageCode = languageCode,
                    Language = GetLanguageName(languageCode),
                    CodeExtension = buffer[index + 5],
                });
            }
        }

        /// <summary>
        /// VTS_C_ADT (sector at 0xE0): VOB id + cell id → first/last sector of the cell.
        /// </summary>
        private static Dictionary<(int VobId, int CellId), (long First, long Last)> ParseCellAddressTable(byte[] buffer)
        {
            var result = new Dictionary<(int, int), (long, long)>();
            var start = (long)GetDWord(buffer, 0xE0) * SectorSize;
            if (start <= 0 || start + 8 > buffer.Length)
            {
                return result;
            }

            var end = start + GetDWord(buffer, (int)start + 4);
            for (var index = start + 8; index + 12 <= end + 1 && index + 12 <= buffer.Length; index += 12)
            {
                var key = (GetWord(buffer, (int)index), (int)buffer[index + 2]);
                if (!result.ContainsKey(key))
                {
                    result.Add(key, (GetDWord(buffer, (int)index + 4), GetDWord(buffer, (int)index + 8)));
                }
            }

            return result;
        }

        private void ParseProgramChains(byte[] buffer, long tableStart, Dictionary<(int VobId, int CellId), (long First, long Last)> cellAddresses)
        {
            if (tableStart + 8 > buffer.Length)
            {
                return;
            }

            var count = GetWord(buffer, (int)tableStart);
            for (var i = 0; i < count; i++)
            {
                var searchPointer = tableStart + 8 + i * 8;
                if (searchPointer + 8 > buffer.Length)
                {
                    break;
                }

                var pgcStart = tableStart + GetDWord(buffer, (int)searchPointer + 4);
                if (pgcStart + 0xEC > buffer.Length)
                {
                    break;
                }

                ProgramChains.Add(ParseProgramChain(buffer, (int)pgcStart, i + 1, cellAddresses));
            }
        }

        private ProgramChain ParseProgramChain(byte[] buffer, int pgc, int number, Dictionary<(int VobId, int CellId), (long First, long Last)> cellAddresses)
        {
            var programChain = new ProgramChain
            {
                Number = number,
                ProgramCount = buffer[pgc + 2],
                Duration = DecodeTime(buffer, pgc + 4),
            };
            Buffer.BlockCopy(buffer, pgc + 0x1C, programChain.SubtitleControl, 0, programChain.SubtitleControl.Length);
            programChain.Palette = ParsePalette(buffer, pgc + 0xA4);

            var cellCount = buffer[pgc + 3];
            var programMap = pgc + GetWord(buffer, pgc + 0xE6);
            var playbackInfo = pgc + GetWord(buffer, pgc + 0xE8);
            var positionInfo = pgc + GetWord(buffer, pgc + 0xEA);
            if (playbackInfo == pgc || positionInfo == pgc)
            {
                return programChain;
            }

            // cell playback info (24 bytes each): category (4), playback time (4), first VOBU start sector (4), ...,
            // last VOBU end sector (4) - cell position info (4 bytes each): VOB id (2), reserved (1), cell id (1)
            // The first category byte holds the block mode (bits 7-6: 01 first cell of a block, 10 in a block,
            // 11 last cell) and block type (bits 5-4: 01 angle block). An angle block lists one cell per angle,
            // all covering the same time, so only the first angle's cell is kept and the time advances once.
            var elapsed = TimeSpan.Zero;
            var cellStarts = new List<TimeSpan>();
            var inAngleBlock = false;
            for (var i = 0; i < cellCount; i++)
            {
                var play = playbackInfo + i * 24;
                var position = positionInfo + i * 4;
                if (play + 24 > buffer.Length || position + 4 > buffer.Length)
                {
                    break;
                }

                var blockMode = (buffer[play] >> 6) & 0b11;
                var isAngleCell = ((buffer[play] >> 4) & 0b11) == 1 && blockMode != 0;
                if (isAngleCell && blockMode != 1 && inAngleBlock)
                {
                    // another angle of the current block
                    cellStarts.Add(programChain.Cells[programChain.Cells.Count - 1].Start);
                    if (blockMode == 3)
                    {
                        inAngleBlock = false;
                    }

                    continue;
                }

                inAngleBlock = isAngleCell && blockMode != 3;
                var cell = new Cell
                {
                    VobId = GetWord(buffer, position),
                    CellId = buffer[position + 3],
                    Duration = DecodeTime(buffer, play + 4),
                    Start = elapsed,
                    FirstSector = GetDWord(buffer, play + 8),
                    LastSector = GetDWord(buffer, play + 20),
                };
                if (cellAddresses.TryGetValue((cell.VobId, cell.CellId), out var address) && cell.LastSector == 0)
                {
                    cell.FirstSector = address.First;
                    cell.LastSector = address.Last;
                }

                programChain.Cells.Add(cell);
                cellStarts.Add(cell.Start);
                elapsed += cell.Duration;
            }

            // program map: the entry cell number (1-based) of each program
            for (var i = 0; i < programChain.ProgramCount && programMap != pgc && programMap + i < buffer.Length; i++)
            {
                var cellIndex = buffer[programMap + i] - 1;
                if (cellIndex >= 0 && cellIndex < cellStarts.Count)
                {
                    programChain.ProgramStarts.Add(cellStarts[cellIndex]);
                }
            }

            return programChain;
        }

        /// <summary>
        /// Stream control (4 bytes per subpicture stream): byte 0 = available bit + stream number
        /// for 4:3, then stream numbers for wide, letterbox and pan&amp;scan (used for 16:9 video).
        /// </summary>
        private void ResolveSubtitleStreamIds()
        {
            foreach (var stream in SubtitleStreams)
            {
                var programChain = ProgramChains.FirstOrDefault(p => p.HasSubtitleStream(stream.Index));
                if (programChain == null)
                {
                    // no program chain says - the DVD default is stream n = 0x20 + n
                    stream.StreamIds[Video.IsWideScreen ? SubtitleVariant.Wide : SubtitleVariant.Normal] = 0x20 + stream.Index;
                    continue;
                }

                var control = stream.Index * 4;
                var c = programChain.SubtitleControl;
                if (!Video.IsWideScreen)
                {
                    stream.StreamIds[SubtitleVariant.Normal] = 0x20 + (c[control] & 0x1F);
                    continue;
                }

                var wide = 0x20 + (c[control + 1] & 0x1F);
                stream.StreamIds[SubtitleVariant.Wide] = wide;
                var letterbox = 0x20 + (c[control + 2] & 0x1F);
                if (letterbox != wide && Video.LetterboxAllowed)
                {
                    stream.StreamIds[SubtitleVariant.Letterbox] = letterbox;
                }

                var panScan = 0x20 + (c[control + 3] & 0x1F);
                if (panScan != wide && panScan != letterbox && Video.PanScanAllowed)
                {
                    stream.StreamIds[SubtitleVariant.PanScan] = panScan;
                }
            }
        }

        /// <summary>
        /// Color lookup table: 16 x (0, Y, Cr, Cb) - empty when all zero.
        /// </summary>
        private static List<SKColor> ParsePalette(byte[] buffer, int index)
        {
            var palette = new List<SKColor>(16);
            var allZero = true;
            for (var i = 0; i < 16; i++, index += 4)
            {
                allZero &= buffer[index + 1] == 0 && buffer[index + 2] == 0 && buffer[index + 3] == 0;
                var y = buffer[index + 1] - 16;
                var cr = buffer[index + 2] - 128;
                var cb = buffer[index + 3] - 128;
                palette.Add(new SKColor(
                    ClampToByte(1.1644 * y + 1.596 * cr),
                    ClampToByte(1.1644 * y - 0.813 * cr - 0.391 * cb),
                    ClampToByte(1.1644 * y + 2.018 * cb)));
            }

            return allZero ? new List<SKColor>() : palette;
        }

        /// <summary>
        /// BCD playback time: hours, minutes, seconds, then 2 bits frame rate (01 = 25, 11 = 29.97) + 6 bits BCD frames.
        /// </summary>
        private static TimeSpan DecodeTime(byte[] buffer, int index)
        {
            var hours = FromBcd(buffer[index]);
            var minutes = FromBcd(buffer[index + 1]);
            var seconds = FromBcd(buffer[index + 2]);
            var frames = FromBcd(buffer[index + 3] & 0x3F);
            var frameRate = (buffer[index + 3] >> 6) == 3 ? 29.97 : 25.0;
            return new TimeSpan(0, hours, minutes, seconds).Add(TimeSpan.FromMilliseconds(Math.Round(frames * 1000.0 / frameRate)));
        }

        private static int FromBcd(int value)
        {
            return (value >> 4) * 10 + (value & 0x0F);
        }

        private static bool IsLanguageCode(byte[] buffer, int index)
        {
            return buffer[index] >= 'a' && buffer[index] <= 'z' && buffer[index + 1] >= 'a' && buffer[index + 1] <= 'z';
        }

        private static string GetLanguageCode(byte[] buffer, int index)
        {
            return IsLanguageCode(buffer, index) ? Encoding.ASCII.GetString(buffer, index, 2) : string.Empty;
        }

        private static string GetLanguageName(string languageCode)
        {
            return string.IsNullOrEmpty(languageCode)
                ? DvdSubtitleLanguage.Language.NotSpecified
                : DvdSubtitleLanguage.GetNativeLanguageName(languageCode);
        }

        private static byte ClampToByte(double value)
        {
            return (byte)Math.Min(Math.Max(Math.Round(value), 0), 255);
        }

        private static int GetWord(byte[] buffer, int index)
        {
            return (buffer[index] << 8) | buffer[index + 1];
        }

        private static uint GetDWord(byte[] buffer, int index)
        {
            return Helper.GetEndian(buffer, index, 4);
        }
    }
}
