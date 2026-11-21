using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Features.Shared.PickMp4Track;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared;

/// <summary>
/// Loads one subtitle track from an mkv/mp4 (track picker when there are more) or a Blu-ray sup,
/// for features that only need the lines or their time codes - point sync via other (#15911),
/// import time codes (#17089). Image tracks (PGS, VobSub, DVB) are not OCR'ed.
/// </summary>
public class EmbeddedSubtitleLoader
{
    // Files smaller than this parse fast enough that a progress window would only flash.
    private const long ProgressWindowMinFileSize = 25 * 1024 * 1024;

    private readonly IWindowService _windowService;
    private readonly Window? _window;

    public EmbeddedSubtitleLoader(IWindowService windowService, Window? window)
    {
        _windowService = windowService;
        _window = window;
    }

    public sealed class Result
    {
        /// <summary>Text track lines (null for an image track).</summary>
        public List<Paragraph>? Paragraphs { get; init; }

        public SubtitleFormat? Format { get; init; }

        /// <summary>Image track (null for a text track) - times via GetStartTime/GetEndTime.</summary>
        public IOcrSubtitle? ImageSubtitle { get; init; }

        public string? Error { get; init; }

        /// <summary>The user cancelled the track picker.</summary>
        public bool Cancelled { get; init; }

        /// <summary>Start/end of every line, text or image.</summary>
        public Subtitle ToTimeCodeSubtitle()
        {
            var subtitle = new Subtitle();
            if (Paragraphs != null)
            {
                foreach (var p in Paragraphs)
                {
                    subtitle.Paragraphs.Add(new Paragraph(p));
                }
            }
            else if (ImageSubtitle != null)
            {
                for (var i = 0; i < ImageSubtitle.Count; i++)
                {
                    subtitle.Paragraphs.Add(new Paragraph(string.Empty, ImageSubtitle.GetStartTime(i).TotalMilliseconds, ImageSubtitle.GetEndTime(i).TotalMilliseconds));
                }
            }

            return subtitle;
        }
    }

    public static bool IsMp4File(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".mp4" or ".m4v" or ".mov";
    }

    public static bool IsMatroskaFile(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is ".mkv" or ".mks" or ".webm")
        {
            return true;
        }

        try
        {
            return BluRayHelper.IsMatroskaFileFast(fileName);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Null when the file is not an mkv/mp4/sup (load it as a subtitle file).
    /// </summary>
    public async Task<Result?> TryLoad(string fileName, bool includeBluRaySup = true)
    {
        if (IsMatroskaFile(fileName))
        {
            return await LoadFromMatroska(fileName);
        }

        if (IsMp4File(fileName))
        {
            return await LoadFromMp4(fileName);
        }

        if (includeBluRaySup && FileUtil.IsBluRaySup(fileName))
        {
            var pcsData = await RunWithProgress(fileName, _ => BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder()));
            return MakeImageResult(new OcrSubtitleBluRay(pcsData));
        }

        return null;
    }

    private async Task<Result> LoadFromMatroska(string fileName)
    {
        using var matroska = new MatroskaFile(fileName);
        var tracks = matroska.GetTracks(true);
        if (tracks.Count == 0)
        {
            return new Result { Error = Se.Language.General.NoSubtitlesFound };
        }

        var track = tracks[0];
        List<BluRaySupParser.PcsData>? preParsedPgs = null;
        if (tracks.Count > 1)
        {
            var pick = await _windowService.ShowDialogAsync<PickMatroskaTrackWindow, PickMatroskaTrackViewModel>(_window!, vm =>
            {
                vm.Initialize(matroska, tracks, fileName);
            });

            if (!pick.OkPressed || pick.SelectedMatroskaTrack == null)
            {
                return new Result { Cancelled = true };
            }

            track = pick.SelectedMatroskaTrack;
            preParsedPgs = pick.GetPreParsedBluRaySubtitles(track.TrackNumber);
        }

        if (track.ContentEncodingType == 1 && IsImageCodec(track.CodecId))
        {
            return new Result { Error = "Encrypted content not supported" };
        }

        if (track.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase))
        {
            var pcsData = preParsedPgs ?? await RunWithProgress(fileName, cb => BluRaySupParser.ParseBluRaySupFromMatroska(track, matroska, cb));
            return MakeImageResult(new OcrSubtitleMkvBluRay(track, pcsData));
        }

        var blocks = await RunWithProgress(fileName, cb => matroska.GetSubtitle(track.TrackNumber, cb));

        if (track.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
        {
            var dvbSubtitle = new Subtitle();
            var images = MatroskaDvbHelper.ParseImages(track, blocks, dvbSubtitle);
            return MakeImageResult(new OcrSubtitleMkvDvb(track, dvbSubtitle, images));
        }

        if (track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
        {
            return MakeImageResult(MakeMatroskaVobSub(track, blocks));
        }

        var subtitle = new Subtitle();
        var format = Utilities.LoadMatroskaTextSubtitle(track, matroska, blocks, subtitle);
        if (track.CodecId.Equals("S_HDMV/TEXTST", StringComparison.OrdinalIgnoreCase))
        {
            Utilities.ParseMatroskaTextSt(track, blocks, subtitle);
        }

        return MakeTextResult(subtitle.Paragraphs, format);
    }

    private static bool IsImageCodec(string codecId) =>
        codecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase) ||
        codecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase) ||
        codecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase);

    /// <summary>Same pack repair as the main window's VobSub-in-Matroska import.</summary>
    private static OcrSubtitleVobSub MakeMatroskaVobSub(MatroskaTrackInfo track, List<MatroskaSubtitle> blocks)
    {
        var idx = new Core.VobSub.Idx(track.GetCodecPrivate().SplitToLines());
        var packs = new List<VobSubMergedPack>();
        foreach (var block in blocks)
        {
            var pack = new VobSubMergedPack(block.GetData(track), TimeSpan.FromMilliseconds(block.Start), 32, null!)
            {
                EndTime = TimeSpan.FromMilliseconds(block.End),
            };
            packs.Add(pack);

            if (packs.Count > 1 && packs[packs.Count - 2].EndTime > pack.StartTime)
            {
                packs[packs.Count - 2].EndTime = TimeSpan.FromMilliseconds(pack.StartTime.TotalMilliseconds - 1);
            }
        }

        packs.RemoveAll(p => p.SubPicture.SubPictureDateSize <= 2 ||
                             (p.SubPicture.SubPictureDateSize <= 67 && p.SubPicture.Delay.TotalMilliseconds < 35));
        VobSubParser.FixPackTimes(packs);
        return new OcrSubtitleVobSub(packs, idx.Palette);
    }

    private async Task<Result> LoadFromMp4(string fileName)
    {
        var mp4Parser = await Task.Run(() => new MP4Parser(fileName));
        var tracks = mp4Parser.GetSubtitleTracks();
        if (tracks.Count == 0)
        {
            var fragmented = mp4Parser.FragmentedSubtitleTracks;
            if (fragmented.Count == 0)
            {
                return new Result { Error = Se.Language.General.NoSubtitlesFound };
            }

            var fragmentedTrack = fragmented[0];
            if (fragmented.Count > 1)
            {
                var pick = await _windowService.ShowDialogAsync<PickMp4TrackWindow, PickMp4TrackViewModel>(_window!, vm =>
                {
                    vm.Initialize(fragmented, fileName);
                });

                if (!pick.OkPressed || pick.SelectedTrack?.FragmentedTrack == null)
                {
                    return new Result { Cancelled = true };
                }

                fragmentedTrack = pick.SelectedTrack.FragmentedTrack;
            }

            return MakeTextResult(fragmentedTrack.Subtitle.Paragraphs, new SubRip());
        }

        var track = tracks[0];
        if (tracks.Count > 1)
        {
            var pick = await _windowService.ShowDialogAsync<PickMp4TrackWindow, PickMp4TrackViewModel>(_window!, vm =>
            {
                vm.Initialize(tracks, fileName);
            });

            if (!pick.OkPressed || pick.SelectedTrack?.Track == null)
            {
                return new Result { Cancelled = true };
            }

            track = pick.SelectedTrack.Track;
        }

        var paragraphs = track.Mdia.Minf.Stbl.GetParagraphs();
        if (track.Mdia.IsVobSubSubtitle)
        {
            return MakeImageResult(new OcrSubtitleMp4VobSub(track, paragraphs));
        }

        return MakeTextResult(paragraphs, new SubRip());
    }

    private static Result MakeTextResult(List<Paragraph> paragraphs, SubtitleFormat format)
    {
        return paragraphs.Count == 0
            ? new Result { Error = Se.Language.General.NoSubtitlesFound }
            : new Result { Paragraphs = paragraphs, Format = format };
    }

    private static Result MakeImageResult(IOcrSubtitle ocrSubtitle)
    {
        return ocrSubtitle.Count == 0
            ? new Result { Error = Se.Language.General.NoSubtitlesFound }
            : new Result { ImageSubtitle = ocrSubtitle };
    }

    /// <summary>
    /// Runs a parse off the UI thread; big files get a "Please wait" window with progress.
    /// </summary>
    private async Task<T> RunWithProgress<T>(string fileName, Func<MatroskaFile.LoadMatroskaCallback, T> work)
    {
        PleaseWaitViewModel? pleaseWait = null;
        try
        {
            if (_window != null && new FileInfo(fileName).Length >= ProgressWindowMinFileSize)
            {
                pleaseWait = _windowService.ShowWindow<PleaseWaitWindow, PleaseWaitViewModel>(_window);
                pleaseWait.StatusText = string.Format(Se.Language.General.ParsingXDotDotDot, Path.GetFileName(fileName));
            }
        }
        catch (Exception e)
        {
            Se.LogError(e);
        }

        try
        {
            var vm = pleaseWait;
            return await Task.Run(() => work((pos, total) => vm?.ReportProgress(pos, total)));
        }
        finally
        {
            pleaseWait?.Close();
        }
    }
}
