using Avalonia.Skia;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Ocr;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConverter : IBatchConverter, IFixCallbacks
{
    public static readonly string FormatAyato = new Ayato().Name;
    public const string FormatBdnXml = "BDN-XML";
    public const string FormatBluRaySup = "Blu-ray sup";
    public static readonly string FormatCavena890 = new Cavena890().Name;
    public const string FormatDCinemaInterop = "D-Cinema interop/png";
    public const string FormatDCinemaSmpte2014 = "D-Cinema SMPTE 2014/png";
    public const string FormatCustomTextFormat = "Custom text format";
    public static readonly string FormatDostImage = "DOST/image";
    public static readonly string FormatEbuStl = new Ebu().Name;
    public const string FormatFcpImage = "FCP/image";
    public const string FormatImagesWithTimeCodesInFileName = "Images with time codes in file name";
    public static readonly string FormatPac = new Pac().Name;
    public static readonly string FormatPacUnicode = new PacUnicode().Name;
    public const string FormatPlainText = "Plain text";
    public const string FormatVobSub = "VobSub";

    private BatchConvertConfig _config;
    private List<SubtitleFormat> _subtitleFormats;

    public SubtitleFormat Format { get; set; } = new SubRip();

    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public string Language { get; set; } = "en";

    private readonly Dictionary<string, Regex> _compiledRegExList;

    private readonly INOcrCaseFixer _nOcrCaseFixer;

    public BatchConverter(INOcrCaseFixer nOcrCaseFixer)
    {
        _nOcrCaseFixer = nOcrCaseFixer;
        _config = new BatchConvertConfig();
        _subtitleFormats = new List<SubtitleFormat>();
        _compiledRegExList = new Dictionary<string, Regex>();
    }

    public void Initialize(BatchConvertConfig config)
    {
        _config = config;
        _subtitleFormats = SubtitleFormatHelper.GetSubtitleFormatsWithFavoritesAtTop();
    }

    public async Task Convert(BatchConvertItem item, CancellationToken cancellationToken)
    {
        if (_subtitleFormats.Count == 0)
        {
            throw new InvalidOperationException("Initialize not called?");
        }

        IOcrSubtitle? imageSubtitle = null;
        if (item.Format == FormatBluRaySup)
        {
            var log = new StringBuilder();
            var pcsData = BluRaySupParser.ParseBluRaySup(item.FileName, log);
            imageSubtitle = new OcrSubtitleBluRay(pcsData);
        }
        else if (item.Format == FormatBdnXml && item.Subtitle != null)
        {
            imageSubtitle = new OcrSubtitleBdn(item.Subtitle, item.FileName, false);
        }
        else if (item.Format == FormatVobSub)
        {
            var vobSubParser = new VobSubParser(true);
            string idxFileName = Path.ChangeExtension(item.FileName, ".idx");
            vobSubParser.OpenSubIdx(item.FileName, idxFileName);
            var vobSubMergedPackList = vobSubParser.MergeVobSubPacks();
            var palette = vobSubParser.IdxPalette;
            vobSubParser.VobSubPacks.Clear();
            imageSubtitle = new OcrSubtitleVobSub(vobSubMergedPackList, palette);
            //TODO: multi track
        }
        else if ((item.FileName.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) || item.FileName.EndsWith(".mks", StringComparison.OrdinalIgnoreCase)) && item.Format.StartsWith("Matroska", StringComparison.Ordinal))
        {
            using (var matroska = new MatroskaFile(item.FileName))
            {
                if (matroska.IsValid)
                {
                    var trackId = item.Format;
                    if (trackId.Contains('#'))
                    {
                        trackId = trackId.Remove(0, trackId.IndexOf('#') + 1);
                    }

                    foreach (var track in matroska.GetTracks(true))
                    {
                        if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
                        {
                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                            {
                                var vobSubs = LoadVobSubFromMatroska(track, matroska, out var idx);
                                imageSubtitle = new OcrSubtitleVobSub(vobSubs);
                                var fileName = Path.GetFileName(item.FileName);
                                item.OutputFileName = fileName.Substring(0, fileName.LastIndexOf('.')).TrimEnd('.') + ".mkv";
                                break;
                            }
                        }
                        else if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
                        {
                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                            {
                                var bluRaySubtitles = LoadBluRaySupFromMatroska(track, matroska);
                                imageSubtitle = new OcrSubtitleMkvBluRay(track, bluRaySubtitles);
                                var fileName = Path.GetFileName(item.FileName);
                                item.OutputFileName = fileName.Substring(0, fileName.LastIndexOf('.')).TrimEnd('.') + ".mkv";
                                break;
                            }
                        }
                        else if (track.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
                        {
                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                            {
                                var sub = new Subtitle();
                                var binaryParagraphs = LoadDvbFromMatroska(track, matroska, ref sub);
                                imageSubtitle = new OcrSubtitleIBinaryParagraph(binaryParagraphs);
                                var fileName = Path.GetFileName(item.FileName);
                                item.OutputFileName = fileName.Substring(0, fileName.LastIndexOf('.')).TrimEnd('.') + ".mkv";
                                break;
                            }
                        }
                        else if (track.CodecId.Equals("S_TEXT/UTF8", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/SSA", StringComparison.OrdinalIgnoreCase) || track.CodecId.Equals("S_TEXT/ASS", StringComparison.OrdinalIgnoreCase))
                        {
                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                            {
                                var mkvSub = matroska.GetSubtitle(track.TrackNumber, null);
                                item.Subtitle = new Subtitle();
                                Utilities.LoadMatroskaTextSubtitle(track, matroska, mkvSub, item.Subtitle);
                                var fileName = Path.GetFileName(item.FileName);
                                item.OutputFileName = fileName.Substring(0, fileName.LastIndexOf('.')).TrimEnd('.') + ".mkv";

                                if (track.CodecId.Equals("S_TEXT/UTF8", StringComparison.OrdinalIgnoreCase))
                                {
                                    item.Subtitle.OriginalFormat = new SubRip();
                                }
                                else if (track.CodecId.Equals("S_TEXT/SSA", StringComparison.OrdinalIgnoreCase))
                                {
                                    item.Subtitle.OriginalFormat = new SubStationAlpha();
                                }
                                else if (track.CodecId.Equals("S_TEXT/ASS", StringComparison.OrdinalIgnoreCase))
                                {
                                    item.Subtitle.OriginalFormat = new AdvancedSubStationAlpha();
                                }

                                break;
                            }
                        }
                        else if (track.CodecId.Equals("S_HDMV/TEXTST", StringComparison.OrdinalIgnoreCase))
                        {
                            if (trackId == track.TrackNumber.ToString(CultureInfo.InvariantCulture))
                            {
                                var mkvSub = matroska.GetSubtitle(track.TrackNumber, null);
                                item.Subtitle = new Subtitle();
                                Utilities.LoadMatroskaTextSubtitle(track, matroska, mkvSub, item.Subtitle);
                                Utilities.ParseMatroskaTextSt(track, mkvSub, item.Subtitle);
                                item.Subtitle.OriginalFormat = new SubRip();
                                var fileName = Path.GetFileName(item.FileName);
                                item.OutputFileName = fileName.Substring(0, fileName.LastIndexOf('.')).TrimEnd('.') + ".mkv";
                                break;
                            }
                        }
                    }
                }
            }
        }
        else if ((item.FileName.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                  item.FileName.EndsWith(".m2ts", StringComparison.OrdinalIgnoreCase) ||
                  item.FileName.EndsWith(".mts", StringComparison.OrdinalIgnoreCase) ||
                  item.FileName.EndsWith(".mpg", StringComparison.OrdinalIgnoreCase) ||
                  item.FileName.EndsWith(".mpeg", StringComparison.OrdinalIgnoreCase)) &&
                  item.Format!.StartsWith("Transport Stream", StringComparison.Ordinal))
        {
            if (item.ImageSubtitle != null)
            {
                imageSubtitle = item.ImageSubtitle;
            }
        }
        else if (item.Format == "MP4" &&
                 (item.FileName.EndsWith(".mp4") || item.FileName.EndsWith(".m4v") || item.FileName.EndsWith(".m4s")))
        {
            var mp4Files = new List<string>();
            var mp4Parser = new MP4Parser(item.FileName);
            var mp4SubtitleTracks = mp4Parser.GetSubtitleTracks();
            if (mp4Parser.VttcSubtitle?.Paragraphs.Count > 0)
            {
                mp4Files.Add("MP4/WebVTT - " + mp4Parser.VttcLanguage);
            }

            foreach (var track in mp4SubtitleTracks)
            {
                if (track.Mdia.IsTextSubtitle || track.Mdia.IsClosedCaption)
                {
                    mp4Files.Add($"MP4/#{mp4SubtitleTracks.IndexOf(track)} {track.Mdia.HandlerType} - {track.Mdia.Mdhd.Iso639ThreeLetterCode ?? track.Mdia.Mdhd.LanguageString}");
                }
            }

            if (mp4Files.Count <= 0)
            {
                item.Status = Se.Language.General.NoSubtitlesFound;
            }
            else
            {
                var trackId = item.Format;
                if (mp4Parser.VttcSubtitle?.Paragraphs.Count > 0 && trackId == "MP4/WebVTT - " + mp4Parser.VttcLanguage)
                {
                    item.Subtitle = mp4Parser.VttcSubtitle;
                    var fileName = Path.GetFileName(item.FileName);
                    item.OutputFileName = fileName.Substring(0, fileName.LastIndexOf('.')).TrimEnd('.') + ".mp4";
                }
                else
                {
                    foreach (var track in mp4SubtitleTracks)
                    {
                        if (track.Mdia.IsTextSubtitle || track.Mdia.IsClosedCaption)
                        {
                            var language = track.Mdia.Mdhd.Iso639ThreeLetterCode ?? track.Mdia.Mdhd.LanguageString;
                            if (trackId == $"MP4/#{mp4SubtitleTracks.IndexOf(track)} {track.Mdia.HandlerType} - {language}")
                            {
                                item.Subtitle = new Subtitle();
                                item.Subtitle.Paragraphs.AddRange(track.Mdia.Minf.Stbl.GetParagraphs());
                                var fileName = Path.GetFileName(item.FileName);
                                item.OutputFileName = fileName.Substring(0, fileName.LastIndexOf('.')).TrimEnd('.') + ".mp4";
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (imageSubtitle != null && !_config.IsTargetFormatImageBased)
        {
            item.Status = Se.Language.General.OcrDotDotDot;
            if (Se.Settings.Tools.BatchConvert.OcrEngine.Equals("nOcr", StringComparison.OrdinalIgnoreCase))
            {
                RunNOcr(imageSubtitle, item, cancellationToken);
            }
            else if (Se.Settings.Tools.BatchConvert.OcrEngine.Equals("PaddleOCR", StringComparison.OrdinalIgnoreCase))
            {
                await RunPaddleOcr(imageSubtitle, item, cancellationToken);
            }
            else
            {
                await RunOcrTesseract(imageSubtitle, item, cancellationToken);
            }
        }

        // Run convert functions (remove formatting, etc.)
        var imageToImage = _config.IsTargetFormatImageBased && imageSubtitle != null;
        if (item.Subtitle != null)
        {
            item.Subtitle = await RunConvertFunctions(item, imageToImage, cancellationToken);
        }

        // Save text based formats
        foreach (var format in _subtitleFormats)
        {
            if (format.Name == _config.TargetFormatName && item.Subtitle != null)
            {
                await SaveSubtitleFormat(item, format, cancellationToken);
                return;
            }
        }

        // Save binary formats
        var binaryFormats = new Dictionary<string, SubtitleFormat>
        {
            { FormatPac, new Pac() },
            { FormatPacUnicode, new PacUnicode() },
            { FormatCavena890, new Cavena890() },
            { FormatEbuStl, new Ebu() },
            { FormatAyato, new Ayato() },
        };
        foreach (var kvp in binaryFormats)
        {
            if (kvp.Key == _config.TargetFormatName && item.Subtitle != null)
            {
                var format = kvp.Value;
                if (format is IBinaryPersistableSubtitle binaryPersistableSubtitle)
                {
                    SaveSubtitleFormat(item, binaryPersistableSubtitle, format, cancellationToken);
                    return;
                }
                else if (format is Ayato ayato) //TODO: make Ayato implement IBinaryPersistableSubtitle
                {
                    var path = MakeOutputFileName(item, format.Extension);
                    ayato.Save(path, string.Empty, item.Subtitle);
                    return;
                }

                await SaveSubtitleFormat(item, format, cancellationToken);
                return;
            }
        }

        if (_config.TargetFormatName == FormatPlainText && item.Subtitle != null)
        {
            var path = MakeOutputFileName(item, ".txt");
            var sb = new StringBuilder();
            foreach (var p in item.Subtitle.Paragraphs)
            {
                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text, true));
            }

            await File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
            return;
        }

        if (_config.TargetFormatName == FormatCustomTextFormat && item.Subtitle != null)
        {
            await SaveCustomSubtitleFormat(item, cancellationToken);
            return;
        }

        if (_config.IsTargetFormatImageBased && imageSubtitle == null)
        {
            imageSubtitle = CreateImageSubtitles(item); // text to image (create bitmaps from text)
        }

        WriteToImageBasedFormat(item, imageSubtitle, cancellationToken);
    }

    public class TransportStreamResult
    {
        public bool IsImage { get; set; }
        public IOcrSubtitle? OcrSubtitle { get; set; }
        public Subtitle? Subtitle { get; set; }
    }

    private List<TransportStreamResult> LoadTransportStream(BatchConvertItem item, CancellationToken cancellationToken)
    {
        var result = new List<TransportStreamResult>();

        var tsParser = new TransportStreamParser();
        tsParser.Parse(item.FileName, null);

        var programMapTableParser = new ProgramMapTableParser();
        programMapTableParser.Parse(item.FileName); // get languages

        foreach (var packetId in tsParser.SubtitlePacketIds)
        {
            var language = string.Empty;
            if (programMapTableParser.GetSubtitlePacketIds().Count > 0)
            {
                language = programMapTableParser.GetSubtitleLanguage(packetId);
            }

            var subtitles = tsParser.GetDvbSubtitles(packetId);
            if (subtitles.Count > 0)
            {
                result.Add(new TransportStreamResult { IsImage = true, OcrSubtitle = new OcrSubtitleTransportStream(tsParser, subtitles, item.FileName) });
            }
        }

        foreach (var i in tsParser.TeletextSubtitlesLookup.Keys)
        {
            var pid = tsParser.TeletextSubtitlesLookup[i];
            var paragraphs = pid.Values.First();
            if (paragraphs.Count > 0)
            {
                result.Add(new TransportStreamResult { IsImage = false, Subtitle = new Subtitle(paragraphs) });
            }
        }

        return result;
    }

    public static List<IBinaryParagraphWithPosition> LoadDvbFromMatroska(MatroskaTrackInfo track, MatroskaFile matroska, ref Subtitle subtitle)
    {
        var sub = matroska.GetSubtitle(track.TrackNumber, null);
        var subtitleImages = new List<DvbSubPes>();
        var subtitles = new List<IBinaryParagraphWithPosition>();
        for (var index = 0; index < sub.Count; index++)
        {
            try
            {
                var msub = sub[index];
                DvbSubPes? pes = null;
                var data = msub.GetData(track);
                if (data != null && data.Length > 9 && data[0] == 15 && data[1] >= SubtitleSegment.PageCompositionSegment && data[1] <= SubtitleSegment.DisplayDefinitionSegment) // sync byte + segment id
                {
                    var buffer = new byte[data.Length + 3];
                    Buffer.BlockCopy(data, 0, buffer, 2, data.Length);
                    buffer[0] = 32;
                    buffer[1] = 0;
                    buffer[buffer.Length - 1] = 255;
                    pes = new DvbSubPes(0, buffer);
                }
                else if (VobSubParser.IsMpeg2PackHeader(data))
                {
                    pes = new DvbSubPes(data, Mpeg2Header.Length);
                }
                else if (VobSubParser.IsPrivateStream1(data, 0))
                {
                    pes = new DvbSubPes(data, 0);
                }
                else if (data != null && (data.Length > 9 && data[0] == 32 && data[1] == 0 && data[2] == 14 && data[3] == 16))
                {
                    pes = new DvbSubPes(0, data);
                }

                if (pes == null && subtitle.Paragraphs.Count > 0)
                {
                    var last = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                    if (last.DurationTotalMilliseconds < 100)
                    {
                        last.EndTime.TotalMilliseconds = msub.Start;
                        if (last.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + 3000;
                        }
                    }
                }

                if (pes?.PageCompositions != null && pes.PageCompositions.Any(p => p.Regions.Count > 0))
                {
                    subtitleImages.Add(pes);
                    subtitle.Paragraphs.Add(new Paragraph(string.Empty, msub.Start, msub.End));
                    subtitles.Add(new TransportStreamSubtitle { Pes = pes, StartMilliseconds = (ulong)msub.Start, EndMilliseconds = (ulong)msub.End });
                }
            }
            catch
            {
                // continue
            }
        }

        if (subtitleImages.Count == 0)
        {
            return new List<IBinaryParagraphWithPosition>();
        }

        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
        {
            var p = subtitle.Paragraphs[index];
            if (p.DurationTotalMilliseconds < 200)
            {
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 3000;
            }

            var next = subtitle.GetParagraphOrDefault(index + 1);
            if (next != null && next.StartTime.TotalMilliseconds < p.EndTime.TotalMilliseconds)
            {
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            }
        }

        return subtitles;
    }

    internal static List<VobSubMergedPack> LoadVobSubFromMatroska(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska, out Core.VobSub.Idx? idx)
    {
        var mergedVobSubPacks = new List<VobSubMergedPack>();
        if (matroskaSubtitleInfo.ContentEncodingType == 1)
        {
            idx = null;
            return mergedVobSubPacks;
        }

        var sub = matroska.GetSubtitle(matroskaSubtitleInfo.TrackNumber, null);
        idx = new Core.VobSub.Idx(matroskaSubtitleInfo.GetCodecPrivate().SplitToLines());
        foreach (var p in sub)
        {
            mergedVobSubPacks.Add(new VobSubMergedPack(p.GetData(matroskaSubtitleInfo), TimeSpan.FromMilliseconds(p.Start), 32, null));
            if (mergedVobSubPacks.Count > 0)
            {
                mergedVobSubPacks[mergedVobSubPacks.Count - 1].EndTime = TimeSpan.FromMilliseconds(p.End);
            }

            // fix overlapping (some versions of Handbrake makes overlapping time codes - thx Hawke)
            if (mergedVobSubPacks.Count > 1 && mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime > mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime)
            {
                mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime = TimeSpan.FromMilliseconds(mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime.TotalMilliseconds - 1);
            }
        }

        return mergedVobSubPacks;
    }


    private List<BluRaySupParser.PcsData> LoadBluRaySupFromMatroska(MatroskaTrackInfo track, MatroskaFile matroska)
    {
        if (track.ContentEncodingType == 1)
        {
            return new List<BluRaySupParser.PcsData>();
        }

        var sub = matroska.GetSubtitle(track.TrackNumber, null);
        var subtitles = new List<BluRaySupParser.PcsData>();
        var log = new StringBuilder();
        var clusterStream = new MemoryStream();
        var lastPalettes = new Dictionary<int, List<PaletteInfo>>();
        var lastBitmapObjects = new Dictionary<int, List<BluRaySupParser.OdsData>>();
        foreach (var p in sub)
        {
            byte[] buffer = p.GetData(track);
            if (buffer != null && buffer.Length > 2)
            {
                clusterStream.Write(buffer, 0, buffer.Length);
                if (ContainsBluRayStartSegment(buffer))
                {
                    if (subtitles.Count > 0 && subtitles[subtitles.Count - 1].StartTime == subtitles[subtitles.Count - 1].EndTime)
                    {
                        subtitles[subtitles.Count - 1].EndTime = (long)((p.Start - 1) * 90.0);
                    }
                    clusterStream.Position = 0;
                    var list = BluRaySupParser.ParseBluRaySup(clusterStream, log, true, lastPalettes, lastBitmapObjects);
                    foreach (var sup in list)
                    {
                        sup.StartTime = (long)((p.Start - 1) * 90.0);
                        sup.EndTime = (long)((p.End - 1) * 90.0);
                        subtitles.Add(sup);

                        // fix overlapping
                        if (subtitles.Count > 1 && sub[subtitles.Count - 2].End > sub[subtitles.Count - 1].Start)
                        {
                            subtitles[subtitles.Count - 2].EndTime = subtitles[subtitles.Count - 1].StartTime - 1;
                        }
                    }
                    clusterStream = new MemoryStream();
                }
            }
            else if (subtitles.Count > 0)
            {
                var lastSub = subtitles[subtitles.Count - 1];
                if (lastSub.StartTime == lastSub.EndTime)
                {
                    lastSub.EndTime = (long)((p.Start - 1) * 90.0);
                    if (lastSub.EndTime - lastSub.StartTime > 1000000)
                    {
                        lastSub.EndTime = lastSub.StartTime;
                    }
                }
            }
        }

        clusterStream.Dispose();
        return subtitles;
    }

    private static bool ContainsBluRayStartSegment(byte[] buffer)
    {
        const int epochStart = 0x80;
        var position = 0;
        while (position + 3 <= buffer.Length)
        {
            var segmentType = buffer[position];
            if (segmentType == epochStart)
            {
                return true;
            }

            int length = BluRaySupParser.BigEndianInt16(buffer, position + 1) + 3;
            position += length;
        }
        return false;
    }

    private async Task SaveCustomSubtitleFormat(BatchConvertItem item, CancellationToken cancellationToken)
    {
        if (item.Subtitle == null)
        {
            item.Status = string.Format(Se.Language.General.ErrorX, Se.Language.General.Error);
            return;
        }

        var customFormats = new List<CustomFormatItem>();
        foreach (var customFormat in Se.Settings.File.ExportCustomFormats)
        {
            customFormats.Add(new CustomFormatItem(customFormat));
        }

        var subtitles = new List<SubtitleLineViewModel>();
        foreach (var p in item.Subtitle.Paragraphs)
        {
            var sv = new SubtitleLineViewModel(p, new SubRip());
            subtitles.Add(sv);
        }

        var selectedCustomFormat = customFormats.FirstOrDefault();
        if (selectedCustomFormat == null)
        {
            item.Status = string.Format(Se.Language.General.ErrorX, Se.Language.General.Error);
            return;
        }

        var text = CustomTextFormatter.GenerateCustomText(selectedCustomFormat, subtitles, item.FileName, string.Empty);
        var path = MakeOutputFileName(item, selectedCustomFormat.Extension);
        await File.WriteAllTextAsync(path, text, cancellationToken);
    }

    private static async Task RunOcrTesseract(IOcrSubtitle imageSubtitles, BatchConvertItem item, CancellationToken cancellationToken)
    {
        var tesseractOcr = new TesseractOcr();
        var language = string.IsNullOrEmpty(Se.Settings.Tools.BatchConvert.TesseractLanguage) ? "eng" : Se.Settings.Tools.BatchConvert.TesseractLanguage;
        item.Subtitle = new Subtitle();
        for (var i = 0; i < imageSubtitles.Count; i++)
        {
            var pct = (i + 1) * 100 / imageSubtitles.Count;
            item.Status = string.Format(Se.Language.General.OcrPercentX, pct);
            var bitmap = imageSubtitles.GetBitmap(i);
            var text = await tesseractOcr.Ocr(bitmap, language, cancellationToken);
            var p = new Paragraph(text, imageSubtitles.GetStartTime(i).TotalMilliseconds, imageSubtitles.GetEndTime(i).TotalMilliseconds);
            item.Subtitle.Paragraphs.Add(p);

            if (cancellationToken.IsCancellationRequested)
            {
                item.Status = Se.Language.General.Cancelled;
                break;
            }
        }
    }

    private void RunNOcr(IOcrSubtitle imageSubtitles, BatchConvertItem item, CancellationToken cancellationToken)
    {
        var fileName = Path.Combine(Se.OcrFolder, Se.Settings.Ocr.NOcrDatabase + ".nocr");
        var nOcrDb = new NOcrDb(fileName);
        item.Subtitle = new Subtitle();

        var totalCount = imageSubtitles.Count;
        var results = new Paragraph[totalCount];
        var processedCount = 0;
        var lockObj = new object();

        Parallel.For(0, totalCount, new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        }, i =>
        {
            var bitmap = imageSubtitles.GetBitmap(i);
            var parentBitmap = new NikseBitmap2(bitmap);
            parentBitmap.MakeTwoColor(200);
            parentBitmap.CropTop(0, new SKColor(0, 0, 0, 0));
            var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parentBitmap, 12,
                false, true, 20, true);
            int index = 0;
            var matches = new List<NOcrChar>();
            while (index < letters.Count)
            {
                var splitterItem = letters[index];
                if (splitterItem.NikseBitmap == null)
                {
                    if (splitterItem.SpecialCharacter != null)
                    {
                        matches.Add(new NOcrChar { Text = splitterItem.SpecialCharacter });
                    }
                }
                else
                {
                    var match = nOcrDb!.GetMatch(parentBitmap, letters, splitterItem, splitterItem.Top, true, 100);
                    if (match is { ExpandCount: > 0 })
                    {
                        index += match.ExpandCount - 1;
                    }

                    if (match == null)
                    {
                        matches.Add(new NOcrChar { Text = "*" });
                    }
                    else
                    {
                        matches.Add(new NOcrChar { Text = _nOcrCaseFixer.FixUppercaseLowercaseIssues(splitterItem, match), Italic = match.Italic });
                    }
                }

                index++;
            }

            var text = ItalicTextMerger.MergeWithItalicTags(matches).Trim();
            results[i] = new Paragraph(text, imageSubtitles.GetStartTime(i).TotalMilliseconds, imageSubtitles.GetEndTime(i).TotalMilliseconds);

            lock (lockObj)
            {
                processedCount++;
                var pct = processedCount * 100 / totalCount;
                item.Status = string.Format(Se.Language.General.OcrPercentX, pct);
            }
        });

        if (cancellationToken.IsCancellationRequested)
        {
            item.Status = Se.Language.General.Cancelled;
            return;
        }

        foreach (var paragraph in results)
        {
            item.Subtitle.Paragraphs.Add(paragraph);
        }
    }

    private readonly Lock _paddleLock = new Lock();

    private async Task RunPaddleOcr(IOcrSubtitle imageSubtitles, BatchConvertItem item, CancellationToken cancellationToken)
    {
        var numberOfImages = imageSubtitles.Count;
        var ocrEngine = new PaddleOcr();
        var language = string.IsNullOrEmpty(Se.Settings.Tools.BatchConvert.PaddleLanguage) ? "en" : Se.Settings.Tools.BatchConvert.PaddleLanguage;
        var mode = Se.Settings.Ocr.PaddleOcrMode;
        var ocrCount = 0;
        item.Subtitle = new Subtitle();

        var batchImages = new List<PaddleOcrBatchInput>(numberOfImages);
        item.Status = "Preparing OCR...";
        for (var i = 0; i < imageSubtitles.Count; i++)
        {
            batchImages.Add(new PaddleOcrBatchInput
            {
                Bitmap = imageSubtitles.GetBitmap(i),
                Index = i,
                Text = $"{i + 1} / {numberOfImages}: {imageSubtitles.GetStartTime(i)} - {imageSubtitles.GetEndTime(i)}"
            });

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }

        var ocrProgress = new Progress<PaddleOcrBatchProgress>(p =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            lock (_paddleLock)
            {
                var number = p.Index;
                var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                item.Status = string.Format(Se.Language.General.OcrPercentX, percentage);

                var paragraph = new Paragraph(p.Text, imageSubtitles.GetStartTime(number).TotalMilliseconds, imageSubtitles.GetEndTime(number).TotalMilliseconds);
                item.Subtitle.Paragraphs.Add(paragraph);

                ocrCount++;
            }
        });

        item.Status = Se.Language.General.OcrDotDotDot;
        await ocrEngine.OcrBatch(OcrEngineType.PaddleOcrStandalone, batchImages, language, mode, ocrProgress, cancellationToken);
        var checkCount = 0;
        while (ocrCount < numberOfImages && checkCount < 100)
        {
            await Task.Delay(100);
            checkCount++;
        }
    }

    private void WriteToImageBasedFormat(BatchConvertItem item, IOcrSubtitle? imageSubtitle, CancellationToken cancellationToken)
    {
        if (imageSubtitle == null)
        {
            item.Status = string.Format(Se.Language.General.ErrorX, Se.Language.General.Error);
            return;
        }

        var imageParameters = new List<ImageParameter>();
        for (var i = 0; i < imageSubtitle.Count; i++)
        {
            var param = new ImageParameter
            {
                Alignment = ExportAlignment.BottomCenter,
                ContentAlignment = ExportContentAlignment.Center,
                PaddingLeftRight = 0,
                PaddingTopBottom = 0,
                Index = i,
                Text = string.Empty,
                StartTime = imageSubtitle.GetStartTime(i),
                EndTime = imageSubtitle.GetEndTime(i),
                FontColor = SKColors.White,
                FontName = "Arial",
                FontSize = 24,
                IsBold = false,
                OutlineColor = SKColors.Black,
                OutlineWidth = 2,
                ShadowColor = SKColors.Black,
                ShadowWidth = 2,
                BackgroundColor = SKColors.Transparent,
                BackgroundCornerRadius = 0,
                ScreenWidth = imageSubtitle.GetScreenSize(i).Width,
                ScreenHeight = imageSubtitle.GetScreenSize(i).Height,
                BottomTopMargin = 0,
                LeftRightMargin = 0,
                Bitmap = imageSubtitle.GetBitmap(i),
            };
            var position = imageSubtitle.GetPosition(i);
            if (position.X >= 0 && position.Y >= 0)
            {
                param.OverridePosition = position;
            }
            imageParameters.Add(param);

            if (cancellationToken.IsCancellationRequested)
            {
                item.Status = Se.Language.General.Cancelled;
                break;
            }
        }

        IExportHandler? exportHandler = null;
        string extension = string.Empty;
        if (_config.TargetFormatName == FormatBluRaySup)
        {
            exportHandler = new ExportHandlerBluRaySup();
            extension = ".sup";
        }

        if (_config.TargetFormatName == FormatBdnXml)
        {
            exportHandler = new ExportHandlerBdnXml();
            extension = string.Empty; // folder
        }

        if (_config.TargetFormatName == FormatVobSub)
        {
            exportHandler = new ExportHandlerVobSub();
            extension = ".sub";
        }

        if (_config.TargetFormatName == FormatImagesWithTimeCodesInFileName)
        {
            exportHandler = new ExportHandlerImagesWithTimeCode();
            extension = string.Empty; // folder
        }

        if (_config.TargetFormatName == FormatDostImage)
        {
            exportHandler = new ExportHandlerDost();
            extension = string.Empty; // folder
        }

        if (_config.TargetFormatName == FormatFcpImage)
        {
            exportHandler = new ExportHandlerFcp();
            extension = string.Empty; // folder
        }

        if (_config.TargetFormatName == FormatDCinemaInterop)
        {
            exportHandler = new ExportHandlerDCinemaInteropPng();
            extension = string.Empty; // folder
        }

        if (_config.TargetFormatName == FormatDCinemaSmpte2014)
        {
            exportHandler = new ExportHandlerDCinemaSmpte2014Png();
            extension = string.Empty; // folder
        }

        if (exportHandler == null || imageParameters.Count == 0)
        {
            item.Status = string.Format(Se.Language.General.ErrorX, Se.Language.General.Error);
            return;
        }

        var path = MakeOutputFileName(item, extension);
        exportHandler.WriteHeader(path, imageParameters[0]);
        for (var i = 0; i < imageParameters.Count; i++)
        {
            exportHandler.CreateParagraph(imageParameters[i]);
            exportHandler.WriteParagraph(imageParameters[i]);
        }
        exportHandler.WriteFooter();
        item.Status = Se.Language.General.Converted;
    }

    private async Task SaveSubtitleFormat(BatchConvertItem item, SubtitleFormat targetFormat, CancellationToken cancellationToken)
    {
        try
        {
            var s = new Subtitle(item.Subtitle);

            if (s.OriginalFormat != null && s.OriginalFormat.Name != targetFormat.Name)
            {
                s.OriginalFormat.RemoveNativeFormatting(item.Subtitle, targetFormat);
            }

            if (targetFormat.Name == AdvancedSubStationAlpha.NameOfFormat)
            {
                if (!_config.AssaUseSourceStylesIfPossible || s.OriginalFormat?.Name != AdvancedSubStationAlpha.NameOfFormat)
                {
                    if (!string.IsNullOrEmpty(_config.AssaHeader))
                    {
                        s.Header = _config.AssaHeader;
                        s.Footer = _config.AssaFooter;
                    }
                }
            }

            var converted = targetFormat.ToText(s, _config.TargetEncoding);
            var path = MakeOutputFileName(item, targetFormat.Extension);
            await File.WriteAllTextAsync(path, converted, cancellationToken);
            item.Status = Se.Language.General.Converted;
        }
        catch (Exception exception)
        {
            item.Status = string.Format(Se.Language.General.ErrorX, exception.Message);
        }
    }

    private void SaveSubtitleFormat(BatchConvertItem item, IBinaryPersistableSubtitle format, SubtitleFormat f, CancellationToken cancellationToken)
    {
        try
        {
            if (item.Subtitle != null && item.Subtitle.OriginalFormat != null && item.Subtitle.OriginalFormat.Name != f.Name)
            {
                item.Subtitle.OriginalFormat.RemoveNativeFormatting(item.Subtitle, f);
            }

            var path = MakeOutputFileName(item, f.Extension);
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            format.Save(path, fileStream, item.Subtitle, true);
            item.Status = Se.Language.General.Converted;
        }
        catch (Exception exception)
        {
            item.Status = string.Format(Se.Language.General.ErrorX, exception.Message);
        }
    }

    private IOcrSubtitle? CreateImageSubtitles(BatchConvertItem item)
    {
        var profile = Se.Settings.File.ExportImages.Profiles.FirstOrDefault(p => p.ProfileName == Se.Settings.File.ExportImages.LastProfileName);
        if (profile == null)
        {
            profile = Se.Settings.File.ExportImages.Profiles.FirstOrDefault();
        }

        if (profile == null)
        {
            profile = new SeExportImagesProfile();
        }

        if (item.Subtitle == null)
        {
            return null;
        }

        var imageParameters = new List<ImageParameter>();
        for (var i = 0; i < item.Subtitle.Paragraphs.Count; i++)
        {
            Paragraph? subtitle = item.Subtitle.Paragraphs[i];
            var imageParameter = new ImageParameter
            {
                Alignment = ExportAlignment.BottomCenter,
                ContentAlignment = ExportContentAlignment.Center,
                PaddingLeftRight = profile.PaddingLeftRight,
                PaddingTopBottom = profile.PaddingTopBottom,
                Index = i,
                Text = HtmlUtil.RemoveAssAlignmentTags(subtitle.Text),
                StartTime = subtitle.StartTime.TimeSpan,
                EndTime = subtitle.EndTime.TimeSpan,
                FontColor = profile.FontColor.FromHexToColor().ToSKColor(),
                FontName = profile.FontName,
                FontSize = profile.FontSize,
                IsBold = profile.IsBold,
                LineSpacingPercent = profile.LineSpacingPercent,
                OutlineColor = profile.OutlineColor.FromHexToColor().ToSKColor(),
                OutlineWidth = profile.OutlineWidth,
                ShadowColor = profile.ShadowColor.FromHexToColor().ToSKColor(),
                ShadowWidth = profile.ShadowWidth,
                BackgroundColor = profile.BackgroundColor.FromHexToColor().ToSKColor(),
                BackgroundCornerRadius = profile.BackgroundCornerRadius,
                ScreenWidth = profile.ScreenWidth,
                ScreenHeight = profile.ScreenHeight,
                BottomTopMargin = profile.BottomTopMargin,
                LeftRightMargin = profile.LeftRightMargin,
            };

            imageParameter.Bitmap = ExportImageBasedViewModel.GenerateBitmap(imageParameter);
            imageParameters.Add(imageParameter);
        }

        return new OcrSubtitleImageParameter(imageParameters);
    }

    private async Task<Subtitle> RunConvertFunctions(BatchConvertItem item, bool imageToImage, CancellationToken cancellationToken)
    {
        var s = new Subtitle(item.Subtitle, false);
        Language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(s) ?? "en";

        if (imageToImage)
        {
            s = DeleteLines(s);
            s = OffsetTimeCodes(s);
            s = AdjustDisplayDuration(s);
            s = ChangeFrameRate(s);
            s = ChangeSpeed(s);
            s = BridgeGaps(s);
            s = ApplyMinGap(s);
        }
        else
        {
            s = DeleteLines(s);
            s = RemoveFormatting(s);
            s = AddFormatting(s);
            s = SplitBreakLongLines(s, Language);
            s = AdjustDisplayDuration(s);
            s = await AutoTranslate(s, cancellationToken);
            s = ChangeCasing(s, Language);
            s = OffsetTimeCodes(s);
            s = ChangeFrameRate(s);
            s = ChangeSpeed(s);
            s = BridgeGaps(s);
            s = ApplyMinGap(s);
            s = FixCommonErrors(s);
            s = MergeLinesWithSameText(s);
            s = MergeLinesWithSameTimeCodes(s, Language);
            s = MultipleReplace(s);
            s = RemoveLineBreaks(s);
            s = RemoveTextForHearingImpaired(s, Language);
            s = FixRightToLeft(s);
        }

        return s;
    }

    private Subtitle MultipleReplace(Subtitle subtitle)
    {
        if (!_config.MultipleReplace.IsActive)
        {
            return subtitle;
        }

        var replaceExpressions = BuildReplaceExpressions();
        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            var hit = false;
            var newText = p.Text;
            var ruleInfo = string.Empty;
            foreach (var item in replaceExpressions)
            {
                if (item.SearchType == ReplaceExpression.SearchCaseSensitive)
                {
                    if (newText.Contains(item.FindWhat))
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        newText = newText.Replace(item.FindWhat, item.ReplaceWith);
                    }
                }
                else if (item.SearchType == ReplaceExpression.SearchRegEx)
                {
                    var r = _compiledRegExList[item.FindWhat];
                    if (r.IsMatch(newText))
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        newText = RegexUtils.ReplaceNewLineSafe(r, newText, item.ReplaceWith);
                    }
                }
                else
                {
                    var index = newText.IndexOf(item.FindWhat, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        do
                        {
                            newText = newText.Remove(index, item.FindWhat.Length).Insert(index, item.ReplaceWith);
                            index = newText.IndexOf(item.FindWhat, index + item.ReplaceWith.Length,
                                StringComparison.OrdinalIgnoreCase);
                        } while (index >= 0);
                    }
                }
            }

            if (hit && newText != p.Text)
            {
                p.Text = newText;
            }
        }

        return subtitle;
    }

    private HashSet<ReplaceExpression> BuildReplaceExpressions()
    {
        var replaceExpressions = new HashSet<ReplaceExpression>();
        foreach (var category in Se.Settings.Edit.MultipleReplace.Categories.Where(p => p.IsActive))
        {
            foreach (var rule in category.Rules.Where(p => p.Active && !string.IsNullOrEmpty(p.Find)))
            {
                var findWhat = RegexUtils.FixNewLine(rule.Find);
                var replaceWith = RegexUtils.FixNewLine(rule.ReplaceWith);

                var mpi = new ReplaceExpression(findWhat, replaceWith, rule.Type.ToString(), category.Name + ": " + rule.Description);
                replaceExpressions.Add(mpi);
                if (mpi.SearchType == ReplaceExpression.SearchRegEx && !_compiledRegExList.ContainsKey(findWhat))
                {
                    _compiledRegExList.Add(findWhat,
                        new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline));
                }
            }
        }

        return replaceExpressions;
    }

    private Subtitle RemoveFormatting(Subtitle subtitle)
    {
        if (!_config.RemoveFormatting.IsActive)
        {
            return subtitle;
        }

        foreach (var p in subtitle.Paragraphs)
        {
            if (_config.RemoveFormatting.RemoveAll)
            {
                p.Text = HtmlUtil.RemoveHtmlTags(p.Text, true);
            }
            else
            {
                if (_config.RemoveFormatting.RemoveItalic)
                {
                    p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagItalic);
                    p.Text = p.Text
                        .Replace("{\\i}", string.Empty)
                        .Replace("{\\i0}", string.Empty)
                        .Replace("{\\i1}", string.Empty);
                }

                if (_config.RemoveFormatting.RemoveBold)
                {
                    p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagBold);
                    p.Text = p.Text
                        .Replace("{\\b}", string.Empty)
                        .Replace("{\\b0}", string.Empty)
                        .Replace("{\\b1}", string.Empty);
                }

                if (_config.RemoveFormatting.RemoveUnderline)
                {
                    p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagUnderline);
                    p.Text = p.Text
                        .Replace("{\\u}", string.Empty)
                        .Replace("{\\u0}", string.Empty)
                        .Replace("{\\u1}", string.Empty);
                }

                if (_config.RemoveFormatting.RemoveColor)
                {
                    p.Text = HtmlUtil.RemoveColorTags(p.Text);
                    if (p.Text.Contains("\\c") || p.Text.Contains("\\1c"))
                    {
                        p.Text = HtmlUtil.RemoveAssaColor(p.Text);
                    }
                }

                if (_config.RemoveFormatting.RemoveFontName)
                {
                    p.Text = HtmlUtil.RemoveFontName(p.Text);
                }

                if (_config.RemoveFormatting.RemoveAlignment)
                {
                    if (p.Text.Contains('{'))
                    {
                        p.Text = HtmlUtil.RemoveAssAlignmentTags(p.Text);
                    }
                }
            }
        }

        return subtitle;
    }

    private Subtitle AddFormatting(Subtitle subtitle)
    {
        if (!_config.AddFormatting.IsActive)
        {
            return subtitle;
        }

        var isAssa = _config.TargetFormatName == AdvancedSubStationAlpha.NameOfFormat;
        foreach (var p in subtitle.Paragraphs)
        {
            if (_config.AddFormatting.AddItalic)
            {
                p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagItalic);
                p.Text = p.Text
                    .Replace("{\\i}", string.Empty)
                    .Replace("{\\i0}", string.Empty)
                    .Replace("{\\i1}", string.Empty)
                    .Replace("\\i0", string.Empty)
                    .Replace("\\i1", string.Empty);
                if (isAssa)
                {
                    p.Text = "{\\i1}" + p.Text + "{\\i0}";
                }
                else
                {
                    p.Text = "<i>" + p.Text + "</i>";
                }
            }

            if (_config.AddFormatting.AddBold)
            {
                p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagBold);
                p.Text = p.Text
                    .Replace("{\\b}", string.Empty)
                    .Replace("{\\b0}", string.Empty)
                    .Replace("{\\b1}", string.Empty)
                    .Replace("\\b0", string.Empty)
                    .Replace("\\b1", string.Empty);

                if (isAssa)
                {
                    p.Text = "{\\b1}" + p.Text + "{\\b0}";
                }
                else
                {
                    p.Text = "<b>" + p.Text + "</b>";
                }
            }

            if (_config.AddFormatting.AddUnderline)
            {
                p.Text = HtmlUtil.RemoveOpenCloseTags(p.Text, HtmlUtil.TagUnderline);
                p.Text = p.Text
                    .Replace("{\\u}", string.Empty)
                    .Replace("{\\u0}", string.Empty)
                    .Replace("{\\u1}", string.Empty)
                    .Replace("\\u0", string.Empty)
                    .Replace("\\u1", string.Empty);

                if (isAssa)
                {
                    p.Text = "{\\u1}" + p.Text + "{\\u0}";
                }
                else
                {
                    p.Text = "<u>" + p.Text + "</u>";
                }
            }

            if (_config.AddFormatting.AddColor)
            {
                p.Text = HtmlUtil.RemoveColorTags(p.Text);
                if (p.Text.Contains("\\c") || p.Text.Contains("\\1c"))
                {
                    p.Text = HtmlUtil.RemoveAssaColor(p.Text);
                }

                if (isAssa)
                {
                    var color = AdvancedSubStationAlpha.GetSsaColorString(_config.AddFormatting.AddColorValue.ToSKColor());
                    p.Text = $"{{\\c{color}}}{p.Text}";
                }
                else
                {
                    var color = _config.AddFormatting.AddColorValue.FromColorToHex(false);
                    p.Text = $"<font color=\"{color}\">{p.Text}</font>";
                }
            }

            if (_config.AddFormatting.AddAlignment)
            {
                if (p.Text.Contains('{'))
                {
                    p.Text = HtmlUtil.RemoveAssAlignmentTags(p.Text);
                }

                p.Text = _config.AddFormatting.AddAlignmentValue + p.Text;
            }
        }

        return subtitle;
    }

    private Subtitle SplitBreakLongLines(Subtitle subtitle, string language)
    {
        if (!_config.AddFormatting.IsActive)
        {
            return subtitle;
        }

        var c = _config.SplitBreakLongLines;
        var subtitles = new List<SubtitleLineViewModel>(subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, subtitle.OriginalFormat)));
        var subtitlesFixed = new List<SubtitleLineViewModel>();
        var maxCharactersPerSubtitle = c.MaxNumberOfLines * c.SingleLineMaxLength;
        if (c.SplitLongLines)
        {
            for (var index = 0; index < subtitles.Count; index++)
            {
                var item = new SubtitleLineViewModel(subtitles[index]);

                var splitLines = SplitBreakLongLinesViewModel.Split(item, maxCharactersPerSubtitle, c.SingleLineMaxLength);
                foreach (var s in splitLines)
                {
                    subtitlesFixed.Add(s);
                }
            }
        }

        if (c.RebalanceLongLines)
        {
            for (var index = 0; index < subtitlesFixed.Count; index++)
            {
                var item = subtitlesFixed[index];
                var rebalancedText = item.Text = Utilities.AutoBreakLine(item.Text, c.SingleLineMaxLength, Se.Settings.General.UnbreakLinesShorterThan, language);
                if (rebalancedText != item.Text)
                {
                    item.Text = rebalancedText;
                }
            }
        }

        subtitle.Paragraphs.Clear();
        foreach (var sv in subtitlesFixed)
        {
            subtitle.Paragraphs.Add(sv.ToParagraph());
        }

        return subtitle;
    }

    private Subtitle OffsetTimeCodes(Subtitle subtitle)
    {
        if (!_config.OffsetTimeCodes.IsActive)
        {
            return subtitle;
        }

        var totalMilliseconds = _config.OffsetTimeCodes.Milliseconds;
        if (!_config.OffsetTimeCodes.Forward)
        {
            totalMilliseconds *= -1;
        }

        subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(totalMilliseconds));
        return subtitle;
    }

    private Subtitle BridgeGaps(Subtitle subtitle)
    {
        if (!_config.BridgeGaps.IsActive)
        {
            return subtitle;
        }

        var dic = new Dictionary<string, string>();
        var fixedIndexes = new List<int>(subtitle.Paragraphs.Count);
        var minMsBetweenLines = _config.BridgeGaps.MinGapMs;
        var maxMs = _config.BridgeGaps.BridgeGapsSmallerThanMs;
        if (Configuration.Settings.General.UseTimeFormatHHMMSSFF)
        {
            minMsBetweenLines = SubtitleFormat.FramesToMilliseconds(minMsBetweenLines);
            maxMs = SubtitleFormat.FramesToMilliseconds(maxMs);
        }

        var subtitles = new ObservableCollection<SubtitleLineViewModel>(subtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, subtitle.OriginalFormat)));
        var fixedCount = DurationsBridgeGaps2.BridgeGaps(subtitles, minMsBetweenLines, _config.BridgeGaps.PercentForLeft, maxMs, fixedIndexes, dic, Configuration.Settings.General.UseTimeFormatHHMMSSFF);

        for (var i = 0; i < subtitles.Count && i < subtitle.Paragraphs.Count; i++)
        {
            subtitle.Paragraphs[i].StartTime.TotalMilliseconds = subtitles[i].StartTime.TotalMilliseconds;
            subtitle.Paragraphs[i].EndTime.TotalMilliseconds = subtitles[i].EndTime.TotalMilliseconds;
        }

        return subtitle;
    }

    private Subtitle ApplyMinGap(Subtitle subtitle)
    {
        if (!_config.ApplyMinGap.IsActive)
        {
            return subtitle;
        }

        var minMsBetweenLines = _config.ApplyMinGap.MinGapMs;
        for (var index = 0; index < subtitle.Paragraphs.Count - 1; index++)
        {
            var current = subtitle.Paragraphs[index];
            var next = subtitle.Paragraphs[index + 1];
            var gapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
            if (gapMs < minMsBetweenLines)
            {
                var newEndMs = next.StartTime.TotalMilliseconds - minMsBetweenLines;
                var newDuration = newEndMs - current.StartTime.TotalMilliseconds;
                if (newDuration > Se.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    current.EndTime.TotalMilliseconds = newEndMs;
                    var newGapMs = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                }
            }
        }

        return subtitle;
    }

    private Subtitle ChangeFrameRate(Subtitle subtitle)
    {
        if (!_config.ChangeFrameRate.IsActive)
        {
            return subtitle;
        }

        subtitle.ChangeFrameRate(_config.ChangeFrameRate.FromFrameRate, _config.ChangeFrameRate.ToFrameRate);

        return subtitle;
    }

    private Subtitle ChangeSpeed(Subtitle subtitle)
    {
        if (!_config.ChangeSpeed.IsActive)
        {
            return subtitle;
        }

        foreach (var p in subtitle.Paragraphs)
        {
            p.StartTime.TotalMilliseconds = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds * (100.0 / _config.ChangeSpeed.SpeedPercent)).TotalMilliseconds;
            p.EndTime.TotalMilliseconds = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds * (100.0 / _config.ChangeSpeed.SpeedPercent)).TotalMilliseconds;
        }

        return subtitle;
    }


    private Subtitle ChangeCasing(Subtitle subtitle, string language)
    {
        if (!_config.ChangeCasing.IsActive)
        {
            return subtitle;
        }

        var fixCasing = new FixCasing(language)
        {
            FixNormal = _config.ChangeCasing.NormalCasing,
            FixNormalOnlyAllUppercase = _config.ChangeCasing.NormalCasingOnlyUpper,
            FixMakeUppercase = _config.ChangeCasing.AllUppercase,
            FixMakeLowercase = _config.ChangeCasing.AllLowercase,
            FixMakeProperCase = false,
            FixProperCaseOnlyAllUppercase = false,
            Format = subtitle.OriginalFormat,
        };
        fixCasing.Fix(subtitle);

        return subtitle;
    }

    private Subtitle FixCommonErrors(Subtitle subtitle)
    {
        if (!_config.FixCommonErrors.IsActive || _config.FixCommonErrors.Profile == null)
        {
            return subtitle;
        }

        foreach (var fix in _config.FixCommonErrors.Profile.FixRules)
        {
            if (fix.IsSelected)
            {
                var fixCommonError = fix.GetFixCommonErrorFunction();
                fixCommonError.Fix(subtitle, this);
            }
        }

        return subtitle;
    }

    private Subtitle RemoveLineBreaks(Subtitle subtitle)
    {
        if (!_config.OffsetTimeCodes.IsActive)
        {
            return subtitle;
        }

        foreach (var paragraph in subtitle.Paragraphs)
        {
            paragraph.Text = Utilities.UnbreakLine(paragraph.Text);
        }

        return subtitle;
    }

    private Subtitle DeleteLines(Subtitle subtitle)
    {
        if (!_config.DeleteLines.IsActive)
        {
            return subtitle;
        }

        var c = _config.DeleteLines;
        if (c.DeleteXFirst == 0 && c.DeleteXLast == 0 && string.IsNullOrWhiteSpace(c.DeleteContains))
        {
            return subtitle;
        }

        var paragraphs = subtitle.Paragraphs.Skip(c.DeleteXFirst).ToList();
        paragraphs = paragraphs.Take(paragraphs.Count - c.DeleteXLast).ToList();

        if (!string.IsNullOrWhiteSpace(c.DeleteContains))
        {
            paragraphs = paragraphs.Where(p => !p.Text.Contains(c.DeleteContains)).ToList();
        }

        var actorsOrSpeakers = c.DeleteActorsOrStyles.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim()).ToList();
        foreach (var actor in actorsOrSpeakers)
        {
            paragraphs = paragraphs.Where(p => !p.Actor.Equals(actor, StringComparison.OrdinalIgnoreCase)).ToList();
            paragraphs = paragraphs.Where(p => !p.Style.Equals(actor, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        subtitle.Paragraphs.Clear();
        subtitle.Paragraphs.AddRange(paragraphs);
        subtitle.Renumber();

        return subtitle;
    }

    private Subtitle AdjustDisplayDuration(Subtitle subtitle)
    {
        if (!_config.AdjustDuration.IsActive)
        {
            return subtitle;
        }

        var shotChanges = new List<double>();
        var c = _config.AdjustDuration;
        if (c.AdjustmentType == AdjustDurationType.Percent)
        {
            subtitle.AdjustDisplayTimeUsingPercent(c.Percentage, null, shotChanges, true);
        }
        else if (c.AdjustmentType == AdjustDurationType.Recalculate)
        {
            subtitle.RecalculateDisplayTimes(c.MaxCharsPerSecond, null, c.OptimalCharsPerSecond, true, shotChanges, true);
        }
        else if (c.AdjustmentType == AdjustDurationType.Fixed)
        {
            subtitle.SetFixedDuration(null, c.FixedMilliseconds * 1000.0);
        }
        else if (c.AdjustmentType == AdjustDurationType.Seconds)
        {
            subtitle.AdjustDisplayTimeUsingSeconds(c.Seconds, null, shotChanges, true);
        }

        return subtitle;
    }

    private async Task<Subtitle> AutoTranslate(Subtitle subtitle, CancellationToken cancellationToken)
    {
        if (!_config.AutoTranslate.IsActive)
        {
            return subtitle;
        }

        Configuration.Settings.Tools.OllamaPrompt = Se.Settings.AutoTranslate.OllamaPrompt;
        Configuration.Settings.Tools.OllamaApiUrl = Se.Settings.AutoTranslate.OllamaUrl;
        Configuration.Settings.Tools.OllamaModel = Se.Settings.AutoTranslate.OllamaModel;

        Configuration.Settings.Tools.AutoTranslateLibreUrl = Se.Settings.AutoTranslate.LibreTranslateUrl;
        Configuration.Settings.Tools.AutoTranslateLibreApiKey = Se.Settings.AutoTranslate.LibreTranslateApiKey;

        Configuration.Settings.Tools.AutoTranslateNllbApiUrl = Se.Settings.AutoTranslate.NnlbApiUrl;

        Configuration.Settings.Tools.AutoTranslateNllbServeUrl = Se.Settings.AutoTranslate.NnlbServeUrl;

        var doAutoTranslate = new DoAutoTranslate();
        var translatedSubtitle = await doAutoTranslate.DoTranslate(subtitle, _config.AutoTranslate.SourceLanguage, _config.AutoTranslate.TargetLanguage,
            _config.AutoTranslate.Translator, cancellationToken);

        for (var i = 0; i < subtitle.Paragraphs.Count && i < translatedSubtitle.Count; i++)
        {
            subtitle.Paragraphs[i].Text = translatedSubtitle[i].TranslatedText;
        }

        return subtitle;
    }

    private Subtitle RemoveTextForHearingImpaired(Subtitle subtitle, string language)
    {
        if (!_config.RemoveTextForHearingImpaired.IsActive)
        {
            return subtitle;
        }

        var s = Se.Settings.Tools.RemoveTextForHi;
        var settings = new RemoveTextForHISettings(subtitle)
        {
            OnlyIfInSeparateLine = s.IsOnlySeparateLine,
            RemoveIfAllUppercase = s.IsRemoveTextUppercaseLineOn,
            RemoveTextBeforeColon = s.IsRemoveTextBeforeColonOn,
            RemoveTextBeforeColonOnlyUppercase = s.IsRemoveTextBeforeColonUppercaseOn,
            ColonSeparateLine = s.IsRemoveTextBeforeColonSeparateLineOn,
            RemoveWhereContains = s.IsRemoveTextContainsOn,
            RemoveIfTextContains = new List<string>(),
            RemoveTextBetweenCustomTags = s.IsRemoveCustomOn,
            RemoveInterjections = s.IsRemoveInterjectionsOn,
            RemoveInterjectionsOnlySeparateLine = s.IsRemoveInterjectionsOn && s.IsInterjectionsSeparateLineOn,
            RemoveTextBetweenSquares = s.IsRemoveBracketsOn,
            RemoveTextBetweenBrackets = s.IsRemoveCurlyBracketsOn,
            RemoveTextBetweenQuestionMarks = false,
            RemoveTextBetweenParentheses = s.IsRemoveParenthesesOn,
            RemoveIfOnlyMusicSymbols = s.IsRemoveOnlyMusicSymbolsOn,
            CustomStart = s.CustomStart,
            CustomEnd = s.CustomEnd,
        };

        foreach (var item in s.TextContains.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries))
        {
            settings.RemoveIfTextContains.Add(item.Trim());
        }

        var removeTextForHiLib = new RemoveTextForHI(settings);
        removeTextForHiLib.Warnings = [];
        removeTextForHiLib.ReloadInterjection(language);

        for (var index = 0; index < subtitle.Paragraphs.Count; index++)
        {
            var p = subtitle.Paragraphs[index];
            var newText = removeTextForHiLib.RemoveTextFromHearImpaired(p.Text, subtitle, index, language);
            if (p.Text.RemoveChar(' ') != newText.RemoveChar(' '))
            {
                p.Text = newText;
            }
        }

        return subtitle;
    }

    private Subtitle FixRightToLeft(Subtitle subtitle)
    {
        if (!_config.RightToLeft.IsActive)
        {
            return subtitle;
        }

        if (_config.RightToLeft.FixViaUnicode)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = Utilities.FixRtlViaUnicodeChars(p.Text);
            }
        }
        else if (_config.RightToLeft.RemoveUnicode)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = Utilities.RemoveUnicodeControlChars(p.Text);
            }
        }
        else if (_config.RightToLeft.ReverseStartEnd)
        {
            foreach (var p in subtitle.Paragraphs)
            {
                p.Text = Utilities.ReverseStartAndEndingForRightToLeft(p.Text);
            }
        }

        return subtitle;
    }

    private Subtitle MergeLinesWithSameTimeCodes(Subtitle subtitle, string language)
    {
        if (!_config.MergeLinesWithSameTimeCodes.IsActive)
        {
            return subtitle;
        }

        var reBreak = _config.MergeLinesWithSameTimeCodes.AutoBreak;
        var makeDialog = _config.MergeLinesWithSameTimeCodes.MergeDialog;
        var singleMergeSubtitles = new List<Paragraph>();
        var mergedText = string.Empty;

        for (var i = 1; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i - 1];

            var next = subtitle.Paragraphs[i];
            if (MergeSameTimeCodesViewModel.QualifiesForMerge(new SubtitleLineViewModel(p, subtitle.OriginalFormat), new SubtitleLineViewModel(next, subtitle.OriginalFormat),
                    _config.MergeLinesWithSameTimeCodes.MaxMillisecondsDifference))
            {
                if (!singleMergeSubtitles.Contains(p))
                {
                    singleMergeSubtitles.Add(p);
                }

                if (!singleMergeSubtitles.Contains(next))
                {
                    singleMergeSubtitles.Add(next);
                }

                var nextText = next.Text
                    .Replace("{\\an1}", string.Empty)
                    .Replace("{\\an2}", string.Empty)
                    .Replace("{\\an3}", string.Empty)
                    .Replace("{\\an4}", string.Empty)
                    .Replace("{\\an5}", string.Empty)
                    .Replace("{\\an6}", string.Empty)
                    .Replace("{\\an7}", string.Empty)
                    .Replace("{\\an8}", string.Empty)
                    .Replace("{\\an9}", string.Empty);

                mergedText = p.Text;
                if (mergedText.StartsWith("<i>", StringComparison.Ordinal) && mergedText.EndsWith("</i>", StringComparison.Ordinal) &&
                    nextText.StartsWith("<i>", StringComparison.Ordinal) && nextText.EndsWith("</i>", StringComparison.Ordinal))
                {
                    mergedText = MergeSameTimeCodesViewModel.GetMergedLines(mergedText.Remove(mergedText.Length - 4), nextText.Remove(0, 3), makeDialog);
                }
                else
                {
                    mergedText = MergeSameTimeCodesViewModel.GetMergedLines(mergedText, nextText, makeDialog);
                }

                if (reBreak)
                {
                    mergedText = Utilities.AutoBreakLine(mergedText, language);
                }
            }
            else
            {
                if (singleMergeSubtitles.Count > 0)
                {
                    singleMergeSubtitles.Clear();
                    mergedText = string.Empty;
                }
            }
        }

        return subtitle;
    }

    private Subtitle MergeLinesWithSameText(Subtitle subtitle)
    {
        if (!_config.MergeLinesWithSameTexts.IsActive)
        {
            return subtitle;
        }

        var mergedIndexes = new List<int>();
        var removed = new HashSet<int>();
        var maxMsBetween = _config.MergeLinesWithSameTexts.MaxMillisecondsBetweenLines;
        var fixIncrementing = _config.MergeLinesWithSameTexts.IncludeIncrementingLines;
        var numberOfMerges = 0;
        Paragraph? p = null;
        var lineNumbers = new List<int>();
        for (var i = 1; i < subtitle.Paragraphs.Count; i++)
        {
            if (removed.Contains(i - 1))
            {
                continue;
            }

            p = subtitle.Paragraphs[i - 1];

            for (var j = i; j < subtitle.Paragraphs.Count; j++)
            {
                if (removed.Contains(j))
                {
                    continue;
                }

                var next = subtitle.Paragraphs[j];
                var incrementText = string.Empty;
                if ((MergeLinesSameTextUtils.QualifiesForMerge(p, next, maxMsBetween) ||
                     fixIncrementing && MergeLinesSameTextUtils.QualifiesForMergeIncrement(p, next, maxMsBetween, out incrementText)))
                {
                    p.Text = next.Text;
                    p.EndTime.TotalMilliseconds = next.EndTime.TotalMilliseconds;
                    if (!string.IsNullOrEmpty(incrementText))
                    {
                        p.Text = incrementText;
                    }

                    if (lineNumbers.Count > 0)
                    {
                        lineNumbers.Add(next.Number);
                    }
                    else
                    {
                        lineNumbers.Add(p.Number);
                        lineNumbers.Add(next.Number);
                    }

                    removed.Add(j);
                    numberOfMerges++;
                    if (!mergedIndexes.Contains(j))
                    {
                        mergedIndexes.Add(j);
                    }

                    if (!mergedIndexes.Contains(i - 1))
                    {
                        mergedIndexes.Add(i - 1);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        return subtitle;
    }

    private string MakeOutputFileName(BatchConvertItem item, string extension)
    {
        var outputFolder = _config.SaveInSourceFolder || string.IsNullOrEmpty(_config.OutputFolder)
            ? Path.GetDirectoryName(item.FileName)
            : _config.OutputFolder;
        if (string.IsNullOrEmpty(outputFolder))
        {
            throw new InvalidOperationException("Output folder is not set");
        }

        var fileName = Path.GetFileNameWithoutExtension(item.FileName);
        if (!string.IsNullOrEmpty(item.OutputFileName))
        {
            fileName = Path.GetFileNameWithoutExtension(item.OutputFileName);
        }

        var targetExtension = extension;

        if (!string.IsNullOrEmpty(item.LanguageCode))
        {
            if (Se.Settings.Tools.BatchConvert.LanguagePostFix == Se.Language.General.TwoLetterLanguageCode)
            {
                var code = item.LanguageCode;
                if (code.Length == 3)
                {
                    code = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(code);
                }
                else if (code.Length > 3)
                {
                    code = Iso639Dash2LanguageCode.GetTwoLetterCodeFromEnglishName(code);
                }

                if (code.Length == 2 && !fileName.EndsWith("." + code, StringComparison.InvariantCultureIgnoreCase))
                {
                    fileName += "." + code;
                }
            }
            else if (Se.Settings.Tools.BatchConvert.LanguagePostFix == Se.Language.General.ThreeLetterLanguageCode)
            {
                var code = item.LanguageCode;
                if (code.Length == 2)
                {
                    code = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(code);
                }
                else if (code.Length > 3)
                {
                    code = Iso639Dash2LanguageCode.GetTwoLetterCodeFromEnglishName(code);
                    code = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(code);
                }

                if (code.Length == 3 && !fileName.EndsWith("." + code, StringComparison.InvariantCultureIgnoreCase))
                {
                    fileName += "." + code;
                }
            }
        }

        var outputFileName = Path.Combine(outputFolder, fileName + targetExtension);
        if (targetExtension != string.Empty && !File.Exists(outputFileName) && Directory.Exists(outputFolder))
        {
            return outputFileName;
        }

        if (targetExtension == string.Empty && Directory.Exists(outputFileName))
        {
            return outputFileName;
        }

        if (targetExtension != string.Empty && _config.Overwrite && File.Exists(outputFileName))
        {
            File.Delete(outputFileName);
        }
        else
        {
            var counter = 1;
            do
            {
                outputFileName = Path.Combine(outputFolder, fileName + $"_{counter}" + targetExtension);
                counter++;
            } while (File.Exists(outputFileName) || Directory.Exists(outputFileName));
        }

        return outputFileName;
    }

    public bool AllowFix(Paragraph p, string action)
    {
        return true;
    }

    public void AddFixToListView(Paragraph p, string action, string before, string after)
    {
    }

    public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked)
    {
    }

    public void LogStatus(string sender, string message)
    {
    }

    public void LogStatus(string sender, string message, bool isImportant)
    {
    }

    public void UpdateFixStatus(int fixes, string message)
    {
    }

    public bool IsName(string candidate)
    {
        return false; //TODO: fix name checking
    }

    public HashSet<string> GetAbbreviations()
    {
        return new HashSet<string>(); //TODO: fix abbreviation checking
    }

    public void AddToTotalErrors(int count)
    {
    }

    public void AddToDeleteIndices(int index)
    {
    }
}