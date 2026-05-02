using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAllTimes;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryApplyDurationLimits;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

public partial class BinaryEditViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private BinarySubtitleItem? _selectedSubtitle;
    [ObservableProperty] private int _screenWidth;
    [ObservableProperty] private int _screenHeight;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _currentPositionAndSize;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isInsertBeforeVisible;
    [ObservableProperty] private bool _isInsertAfterVisible;
    [ObservableProperty] private bool _isToggleForcedVisible;
    [ObservableProperty] private bool _isInsertSubtitleVisible;

    public IOcrSubtitle? OcrSubtitle { get; set; }

    public Window? Window { get; set; }
    public DataGrid? SubtitleGrid { get; set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    public Image? SubtitleOverlayImage { get; set; }
    public Border? VideoContentBorder { get; set; }
    public bool OkPressed { get; private set; }
    public ObservableCollection<BinarySubtitleItem> Subtitles { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;
    private readonly IShortcutManager _shortcutManager;
    private readonly IBluRayHelper _bluRayHelper;

    private string _loadFileName = string.Empty;

    public BinaryEditViewModel(IFileHelper fileHelper, IWindowService windowService, IFolderHelper folderHelper, IShortcutManager shortcutManager, IBluRayHelper bluRayHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _shortcutManager = shortcutManager;
        _bluRayHelper = bluRayHelper;
        _fileName = string.Empty;
        Subtitles = new ObservableCollection<BinarySubtitleItem>();
        StatusText = string.Empty;
        CurrentPositionAndSize = string.Empty;
    }

    public void Initialize(string fileName, IOcrSubtitle? subtitle)
    {
        _loadFileName = fileName;

        if (subtitle != null && string.IsNullOrEmpty(fileName) && subtitle.Count > 0)
        {
            ScreenWidth = subtitle.GetScreenSize(0).Width;
            ScreenHeight = subtitle.GetScreenSize(0).Height;
            var items = subtitle.MakeOcrSubtitleItems();
            foreach (var ocrItem in items)
            {
                var newItem = new BinarySubtitleItem(ocrItem, -1);
                newItem.StartTime = TimeSpan.FromMilliseconds(ocrItem.StartTime.TotalMilliseconds);
                newItem.EndTime = TimeSpan.FromMilliseconds(ocrItem.EndTime.TotalMilliseconds);
                Subtitles.Add(newItem);
            }
            Renumber();
        }
    }

    public void Initialize(IList<OcrSubtitleItem> ocrSubtitleItems)
    {
        if (ocrSubtitleItems == null || ocrSubtitleItems.Count == 0)
        {
            return;
        }

        var screenSize = ocrSubtitleItems[0].GetScreenSize();
        ScreenWidth = screenSize.Width;
        ScreenHeight = screenSize.Height;

        foreach (var ocrItem in ocrSubtitleItems)
        {
            var newItem = new BinarySubtitleItem(ocrItem, -1);
            newItem.StartTime = TimeSpan.FromMilliseconds(ocrItem.StartTime.TotalMilliseconds);
            newItem.EndTime = TimeSpan.FromMilliseconds(ocrItem.EndTime.TotalMilliseconds);
            Subtitles.Add(newItem);
        }

        Renumber();
    }

    public void RegisterVideoShortcuts()
    {
        var shortcuts = BinaryEditShortcuts.GetVideoShortcuts(this);

        foreach (var shortcut in shortcuts)
        {
            _shortcutManager.RegisterShortcut(shortcut);
        }
    }

    partial void OnSelectedSubtitleChanged(BinarySubtitleItem? value)
    {
        UpdateOverlayPosition();
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (SelectedSubtitle == null)
        {
            StatusText = string.Format(Se.Language.General.XSubtitles, Subtitles.Count);
            CurrentPositionAndSize = string.Empty;
        }
        else
        {
            var index = Subtitles.IndexOf(SelectedSubtitle);
            StatusText = string.Format(Se.Language.General.SubtitleXOfY, index + 1, Subtitles.Count);
            CurrentPositionAndSize = string.Format(Se.Language.General.PositionX, $"{SelectedSubtitle.X},{SelectedSubtitle.Y}") + Environment.NewLine +
                                     string.Format(Se.Language.General.SizeX, $"{SelectedSubtitle.Bitmap?.Size.Width}x{SelectedSubtitle.Bitmap?.Size.Height}");
        }
    }

    partial void OnScreenWidthChanged(int value)
    {
        if (value <= 0)
        {
            return;
        }

        foreach (var subtitle in Subtitles)
        {
            subtitle.ScreenWidth = value;
        }

        UpdateOverlayPosition();
    }

    partial void OnScreenHeightChanged(int value)
    {
        if (value <= 0)
        {
            return;
        }

        foreach (var subtitle in Subtitles)
        {
            subtitle.ScreenHeight = value;
        }

        UpdateOverlayPosition();
    }

    public void UpdateOverlayPosition()
    {
        if (VideoContentBorder == null || VideoPlayerControl == null || ScreenWidth <= 0 || ScreenHeight <= 0)
        {
            return;
        }

        // Calculate and update the green rectangle
        var videoPlayerWidth = VideoPlayerControl.Bounds.Width;
        var videoPlayerHeight = VideoPlayerControl.Bounds.Height;

        if (videoPlayerWidth <= 0 || videoPlayerHeight <= 0)
        {
            return;
        }

        const double controlsHeight = 55;
        var availableHeight = videoPlayerHeight - controlsHeight;
        if (availableHeight <= 0)
        {
            return;
        }

        var screenAspect = (double)ScreenWidth / ScreenHeight;
        var availableAspect = videoPlayerWidth / availableHeight;

        double rectWidth, rectHeight, rectX, rectY;

        if (availableAspect > screenAspect)
        {
            rectHeight = availableHeight;
            rectWidth = availableHeight * screenAspect;
            rectX = (videoPlayerWidth - rectWidth) / 2;
            rectY = 0;
        }
        else
        {
            rectWidth = videoPlayerWidth;
            rectHeight = videoPlayerWidth / screenAspect;
            rectX = 0;
            rectY = (availableHeight - rectHeight) / 2;
        }

        VideoContentBorder.Width = rectWidth;
        VideoContentBorder.Height = rectHeight;
        VideoContentBorder.Margin = new Avalonia.Thickness(rectX, rectY, 0, 0);

        // Update subtitle overlay
        if (SubtitleOverlayImage == null || SelectedSubtitle == null || SelectedSubtitle.Bitmap == null)
        {
            return;
        }

        var subtitleScreenWidth = SelectedSubtitle.ScreenSize.Width;
        var subtitleScreenHeight = SelectedSubtitle.ScreenSize.Height;

        if (subtitleScreenWidth <= 0 || subtitleScreenHeight <= 0)
        {
            return;
        }

        // Calculate scale based on rectangle
        var scaleX = rectWidth / subtitleScreenWidth;
        var scaleY = rectHeight / subtitleScreenHeight;

        // Get original bitmap dimensions and set scaled size on image
        var bitmapWidth = SelectedSubtitle.Bitmap.Size.Width;
        var bitmapHeight = SelectedSubtitle.Bitmap.Size.Height;
        SubtitleOverlayImage.Width = bitmapWidth * scaleX;
        SubtitleOverlayImage.Height = bitmapHeight * scaleY;

        // Position: rectangle position + scaled subtitle position
        var overlayX = rectX + (SelectedSubtitle.X * scaleX);
        var overlayY = rectY + (SelectedSubtitle.Y * scaleY);
        SubtitleOverlayImage.Margin = new Avalonia.Thickness(overlayX, overlayY, 0, 0);
    }

    [RelayCommand]
    private async Task FileOpen()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.General.OpenSubtitleFileTitle, Se.Language.General.ImagedBasedSubtitles, "*.sup;*.sub;*.ts;*.xml;*.mkv;*.mks", Se.Language.General.AllFiles, "*.*");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        await DoFileOpen(fileName);
    }

    [RelayCommand]
    private void VideoOneSecondBack()
    {
        MoveVideoPositionMs(-1000);
    }

    [RelayCommand]
    private void VideoOneSecondForward()
    {
        MoveVideoPositionMs(1000);
    }

    [RelayCommand]
    private void Video500MsBack()
    {
        MoveVideoPositionMs(-500);
    }

    [RelayCommand]
    private void Video500MsForward()
    {
        MoveVideoPositionMs(500);
    }

    [RelayCommand]
    private void Video100MsBack()
    {
        MoveVideoPositionMs(-100);
    }

    [RelayCommand]
    private void Video100MsForward()
    {
        MoveVideoPositionMs(100);
    }

    [RelayCommand]
    private void VideoOneFrameBack()
    {
        var vp = GetVideoPlayerControl();
        if (vp != null && vp.VideoPlayer is LibMpvDynamicPlayer mpv)
        {
            mpv.StepOneFrameBack();
            return;
        }

        if (Se.Settings.General.CurrentFrameRate >= 10)
        {
            var frameInMs = (int)Math.Round(1000.0 / Se.Settings.General.CurrentFrameRate, MidpointRounding.AwayFromZero);
            MoveVideoPositionMs(-frameInMs);
            return;
        }

        MoveVideoPositionMs(-40);
    }

    [RelayCommand]
    private void VideoOneFrameForward()
    {
        var vp = GetVideoPlayerControl();
        if (vp != null && vp.VideoPlayer is LibMpvDynamicPlayer mpv)
        {
            mpv.StepOneFrameForward();
            return;
        }

        if (Se.Settings.General.CurrentFrameRate >= 10)
        {
            var frameInMs = (int)Math.Round(1000.0 / Se.Settings.General.CurrentFrameRate, MidpointRounding.AwayFromZero);
            MoveVideoPositionMs(frameInMs);
            return;
        }

        MoveVideoPositionMs(40);
    }

    private VideoPlayerControl? GetVideoPlayerControl()
    {
        return VideoPlayerControl;
    }

    [RelayCommand]
    private void VideoMoveCustom1Back()
    {
        MoveVideoPositionMs(-Se.Settings.Video.MoveVideoPositionCustom1Back);
    }

    [RelayCommand]
    private void VideoMoveCustom1Forward()
    {
        MoveVideoPositionMs(Se.Settings.Video.MoveVideoPositionCustom1Forward);
    }

    [RelayCommand]
    private void VideoMoveCustom2Back()
    {
        MoveVideoPositionMs(-Se.Settings.Video.MoveVideoPositionCustom2Back);
    }

    [RelayCommand]
    private void VideoMoveCustom2Forward()
    {
        MoveVideoPositionMs(Se.Settings.Video.MoveVideoPositionCustom2Forward);
    }


    private void MoveVideoPositionMs(int ms)
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || string.IsNullOrEmpty(vp.VideoPlayer.FileName))
        {
            return;
        }

        var newPosition = vp.Position + (ms / 1000.0);
        if (newPosition < 0)
        {
            newPosition = 0;
        }

        if (newPosition > vp.Duration)
        {
            newPosition = vp.Duration;
        }

        vp.Position = newPosition;

        if (vp.IsPlaying)
        {
            return;
        }
    }

    private async Task DoFileOpen(string fileName)
    {
        if (Window == null)
        {
            return;
        }

        IOcrSubtitle? imageSubtitle = await LoadImageSubtitle(fileName);

        if (imageSubtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "Image based subtitle format not found/supported.",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        FileName = fileName;
        OcrSubtitle = imageSubtitle;

        Subtitles.Clear();
        List<Ocr.OcrSubtitleItem> list = imageSubtitle.MakeOcrSubtitleItems();
        for (int i = 0; i < list.Count; i++)
        {
            Ocr.OcrSubtitleItem? s = list[i];
            Subtitles.Add(new BinarySubtitleItem(s, i));
        }

        if (Subtitles.Count > 0)
        {
            SelectAndScrollToRow(0);
            ScreenWidth = Subtitles[0].ScreenSize.Width;
            ScreenHeight = Subtitles[0].ScreenSize.Height;
            UpdateStatusText();
            Window.Title = string.Format(Se.Language.Tools.ImageBasedEdit.EditImagedBaseSubtitleX, fileName);
        }

        var videoFileName = TryGetVideoFileName(fileName);
        if (!string.IsNullOrEmpty(videoFileName) && VideoPlayerControl != null)
        {
            await VideoPlayerControl.Open(videoFileName);
        }
    }

    private async Task<IOcrSubtitle?> LoadImageSubtitle(string fileName)
    {
        // Blu-ray SUP
        if (FileUtil.IsBluRaySup(fileName))
        {
            var subtitles = BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());
            if (subtitles.Count > 0)
            {
                return new OcrSubtitleBluRay(subtitles);
            }

            return null;
        }

        // VobSub (.sub + .idx)
        if (FileUtil.IsVobSub(fileName))
        {
            var vobSubParser = new VobSubParser(true);
            var idxFileName = Path.ChangeExtension(fileName, ".idx");
            vobSubParser.OpenSubIdx(fileName, idxFileName);
            var mergedPacks = vobSubParser.MergeVobSubPacks();
            var palette = vobSubParser.IdxPalette;
            if (mergedPacks.Count > 0)
            {
                return new OcrSubtitleVobSub(mergedPacks, palette);
            }

            return null;
        }

        // Transport Stream (.ts)
        if (FileUtil.IsTransportStream(fileName))
        {
            var tsParser = new TransportStreamParser();
            tsParser.Parse(fileName, null);
            var subtitles = tsParser.GetDvbSubtitles(0);
            if (subtitles.Count > 0)
            {
                return new OcrSubtitleTransportStream(tsParser, subtitles, fileName);
            }

            return null;
        }

        // Matroska (.mkv / .mks) - PGS / VobSub / DVB tracks
        if (FileUtil.IsMatroskaFileFast(fileName) && FileUtil.IsMatroskaFile(fileName))
        {
            return await LoadMatroskaImageSubtitle(fileName);
        }

        // BDN XML
        if (fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            var bdnXml = new Nikse.SubtitleEdit.Core.SubtitleFormats.BdnXml();
            var subtitle = new Subtitle();
            bdnXml.LoadSubtitle(subtitle, File.ReadAllLines(fileName).ToList(), fileName);
            if (subtitle.Paragraphs.Count > 0)
            {
                return new OcrSubtitleBdn(subtitle, fileName, false);
            }
        }

        return null;
    }

    private async Task<IOcrSubtitle?> LoadMatroskaImageSubtitle(string fileName)
    {
        if (Window == null)
        {
            return null;
        }

        var matroska = new MatroskaFile(fileName);
        try
        {
            var allTracks = matroska.GetTracks(true);
            var imageTracks = allTracks.Where(IsImageBasedMatroskaTrack).ToList();
            if (imageTracks.Count == 0)
            {
                return null;
            }

            MatroskaTrackInfo? selectedTrack;
            if (imageTracks.Count == 1)
            {
                selectedTrack = imageTracks[0];
            }
            else
            {
                var pickResult = await _windowService.ShowDialogAsync<PickMatroskaTrackWindow, PickMatroskaTrackViewModel>(
                    Window, vm => vm.Initialize(matroska, imageTracks, fileName));
                if (!pickResult.OkPressed || pickResult.SelectedMatroskaTrack == null)
                {
                    return null;
                }

                selectedTrack = pickResult.SelectedMatroskaTrack;
            }

            if (selectedTrack.CodecId.Equals(MatroskaTrackType.BluRay, StringComparison.OrdinalIgnoreCase))
            {
                var pcsData = _bluRayHelper.LoadBluRaySubFromMatroska(selectedTrack, matroska, out _);
                return pcsData.Count == 0 ? null : new OcrSubtitleMkvBluRay(selectedTrack, pcsData);
            }

            if (selectedTrack.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
            {
                if (selectedTrack.ContentEncodingType == 1)
                {
                    await MessageBox.Show(Window, Se.Language.General.Error,
                        "Encrypted VobSub subtitles are not supported.",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                var (mergedPacks, palette) = ExtractMkvVobSub(selectedTrack, matroska);
                return mergedPacks.Count == 0 ? null : new OcrSubtitleVobSub(mergedPacks, palette);
            }

            if (selectedTrack.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase))
            {
                var (subtitle, subtitleImages) = ExtractMkvDvb(selectedTrack, matroska);
                return subtitleImages.Count == 0 ? null : new OcrSubtitleMkvDvb(selectedTrack, subtitle, subtitleImages);
            }

            return null;
        }
        finally
        {
            matroska.Dispose();
        }
    }

    private static bool IsImageBasedMatroskaTrack(MatroskaTrackInfo track)
    {
        return track.CodecId.Equals(MatroskaTrackType.BluRay, StringComparison.OrdinalIgnoreCase)
            || track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase)
            || track.CodecId.Equals("S_DVBSUB", StringComparison.OrdinalIgnoreCase);
    }

    private static (List<VobSubMergedPack> mergedPacks, List<SKColor>? palette) ExtractMkvVobSub(
        MatroskaTrackInfo track, MatroskaFile matroska)
    {
        var sub = matroska.GetSubtitle(track.TrackNumber, null);
        var idx = new Idx(track.GetCodecPrivate().SplitToLines());
        var mergedVobSubPacks = new List<VobSubMergedPack>();

        foreach (var p in sub)
        {
            mergedVobSubPacks.Add(new VobSubMergedPack(p.GetData(track), TimeSpan.FromMilliseconds(p.Start), 32, null));
            mergedVobSubPacks[mergedVobSubPacks.Count - 1].EndTime = TimeSpan.FromMilliseconds(p.End);

            // fix overlapping (some Handbrake versions produce overlapping timecodes)
            if (mergedVobSubPacks.Count > 1 &&
                mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime > mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime)
            {
                mergedVobSubPacks[mergedVobSubPacks.Count - 2].EndTime =
                    TimeSpan.FromMilliseconds(mergedVobSubPacks[mergedVobSubPacks.Count - 1].StartTime.TotalMilliseconds - 1);
            }
        }

        for (var i = mergedVobSubPacks.Count - 1; i >= 0; i--)
        {
            if (mergedVobSubPacks[i].SubPicture.SubPictureDateSize <= 2)
            {
                mergedVobSubPacks.RemoveAt(i);
            }
            else if (mergedVobSubPacks[i].SubPicture.SubPictureDateSize <= 67 &&
                     mergedVobSubPacks[i].SubPicture.Delay.TotalMilliseconds < 35)
            {
                mergedVobSubPacks.RemoveAt(i);
            }
        }

        return (mergedVobSubPacks, idx.Palette);
    }

    private static (Subtitle subtitle, List<DvbSubPes> subtitleImages) ExtractMkvDvb(
        MatroskaTrackInfo track, MatroskaFile matroska)
    {
        var sub = matroska.GetSubtitle(track.TrackNumber, null);
        var subtitleImages = new List<DvbSubPes>();
        var subtitle = new Subtitle();

        for (var index = 0; index < sub.Count; index++)
        {
            try
            {
                var msub = sub[index];
                DvbSubPes? pes = null;
                var data = msub.GetData(track);
                if (data != null && data.Length > 9 && data[0] == 15 &&
                    data[1] >= SubtitleSegment.PageCompositionSegment &&
                    data[1] <= SubtitleSegment.DisplayDefinitionSegment)
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
                else if (data!.Length > 9 && data[0] == 32 && data[1] == 0 && data[2] == 14 && data[3] == 16)
                {
                    pes = new DvbSubPes(0, data);
                }

                if (pes == null && subtitle.Paragraphs.Count > 0)
                {
                    var last = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                    if (last.DurationTotalMilliseconds < 100)
                    {
                        last.EndTime.TotalMilliseconds = msub.Start;
                        if (last.DurationTotalMilliseconds > Se.Settings.General.SubtitleMaximumDisplayMilliseconds)
                        {
                            last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + 3000;
                        }
                    }
                }

                if (pes != null && pes.PageCompositions != null && pes.PageCompositions.Any(p => p.Regions.Count > 0))
                {
                    subtitleImages.Add(pes);
                    subtitle.Paragraphs.Add(new Paragraph(string.Empty, msub.Start, msub.End));
                }
            }
            catch
            {
                // continue
            }
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
                p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds -
                                              Se.Settings.General.MinimumMillisecondsBetweenLines;
            }
        }

        return (subtitle, subtitleImages);
    }

    private string? TryGetVideoFileName(string fileName)
    {
        var videoExtensions = new[] { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".ts", ".mpg", ".mpeg" };
        var baseName = Path.Combine(Path.GetDirectoryName(fileName) ?? string.Empty, Path.GetFileNameWithoutExtension(fileName));
        var baseName2 = Path.Combine(Path.GetDirectoryName(fileName) ?? string.Empty, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileName)));

        var baseNamesToTry = new[] { baseName, baseName2 };
        foreach (var ext in videoExtensions)
        {
            foreach (var bName in baseNamesToTry)
            {
                var testFileName = bName + ext;
                if (File.Exists(testFileName))
                {
                    return testFileName;
                }
            }
        }

        return null;
    }

    [RelayCommand]
    private async Task ExportBluRaySup()
    {
        await DoExport(new ExportHandlerBluRaySup(), ".sup");
    }

    [RelayCommand]
    private async Task ExportBdnXml()
    {
        await DoExport(new ExportHandlerBdnXml(), string.Empty, false);
    }

    [RelayCommand]
    private async Task ExportVobSub()
    {
        await DoExport(new ExportHandlerVobSub(), ".sub");
    }

    [RelayCommand]
    private async Task ExportImagesWithTimeCode()
    {
        await DoExport(new ExportHandlerImagesWithTimeCode(), string.Empty, false);
    }

    [RelayCommand]
    private async Task ExportWebVttThumbnail()
    {
        await DoExport(new ExportHandlerWebVttThumbnail(), string.Empty, false);
    }

    [RelayCommand]
    private async Task ExportDostPng()
    {
        await DoExport(new ExportHandlerDost(), string.Empty, false);
    }

    [RelayCommand]
    private async Task ExportFcpPng()
    {
        await DoExport(new ExportHandlerFcp(), string.Empty, false);
    }

    private async Task<bool> DoExport(IExportHandler exportHandler, string extension, bool isTargetFile = true)
    {
        if (Window == null)
        {
            return false;
        }

        if (Subtitles.Count == 0)
        {
            return false;
        }

        var fileOrFolderName = string.Empty;
        if (isTargetFile)
        {
            fileOrFolderName = await _fileHelper.PickSaveFile(Window, extension, "export", Se.Language.General.SaveFileAsTitle);
        }
        else
        {
            fileOrFolderName = await _folderHelper.PickFolderAsync(Window, Se.Language.General.SelectedAFolderToSaveTo);
        }

        if (string.IsNullOrEmpty(fileOrFolderName))
        {
            return false;
        }

        var imageParameter = new ImageParameter()
        {
            ScreenWidth = ScreenWidth,
            ScreenHeight = ScreenHeight,
        };

        exportHandler.WriteHeader(fileOrFolderName, imageParameter);
        for (var i = 0; i < Subtitles.Count; i++)
        {
            imageParameter.Bitmap = Subtitles[i].Bitmap!.ToSkBitmap();
            imageParameter.Text = Subtitles[i].Text;
            imageParameter.StartTime = Subtitles[i].StartTime;
            imageParameter.EndTime = Subtitles[i].EndTime;
            imageParameter.Index = i;
            imageParameter.IsForced = Subtitles[i].IsForced;
            imageParameter.OverridePosition = new SKPointI(Subtitles[i].X, Subtitles[i].Y);

            exportHandler.CreateParagraph(imageParameter);
            exportHandler.WriteParagraph(imageParameter);
        }

        exportHandler.WriteFooter();
        return true;
    }

    [RelayCommand]
    private async Task ImportTimeCodes()
    {
        if (Window == null)
        {
            return;
        }

        if (Subtitles.Count == 0)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.UnknownSubtitleFormat,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (subtitle.Paragraphs.Count != Subtitles.Count)
        {
            var message = "The time code import subtitle does not have the same number of lines as the current subtitle." + Environment.NewLine
                + "Imported lines: " + subtitle.Paragraphs.Count + Environment.NewLine
                + "Current lines: " + Subtitles.Count + Environment.NewLine
                + Environment.NewLine +
                "Do you want to continue anyway?";

            var answer = await MessageBox.Show(Window, Se.Language.General.Import, message, MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Error);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        for (var i = 0; i < Subtitles.Count && i < subtitle.Paragraphs.Count; i++)
        {
            Subtitles[i].StartTime = subtitle.Paragraphs[i].StartTime.TimeSpan;
            Subtitles[i].EndTime = subtitle.Paragraphs[i].EndTime.TimeSpan;
            Subtitles[i].Duration = Subtitles[i].EndTime - Subtitles[i].StartTime;
        }
    }

    [RelayCommand]
    private async Task AdjustDurations()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryAdjustDuration.BinaryAdjustDurationWindow, BinaryAdjustDuration.BinaryAdjustDurationViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        // Get selected indices from the grid
        var selectedIndices = new List<int>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    var index = Subtitles.IndexOf(binaryItem);
                    if (index >= 0)
                    {
                        selectedIndices.Add(index);
                    }
                }
            }
        }

        result.AdjustDuration(Subtitles.ToList(), selectedIndices.Count > 0 ? selectedIndices : null);
    }

    [RelayCommand]
    private async Task ApplyDurationLimits()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryApplyDurationLimitsWindow, BinaryApplyDurationLimitsViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        // Get selected indices from the grid
        var selectedIndices = new List<int>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    var index = Subtitles.IndexOf(binaryItem);
                    if (index >= 0)
                    {
                        selectedIndices.Add(index);
                    }
                }
            }
        }

        result.ApplyLimits(Subtitles.ToList(), selectedIndices.Count > 0 ? selectedIndices : null);
    }

    [RelayCommand]
    private async Task Alignment()
    {
        if (Window == null)
        {
            return;
        }

        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, nothing to do
        if (selectedItems.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Information,
                "Please select one or more subtitles to adjust alignment.",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Show alignment picker
        var result = await _windowService.ShowDialogAsync<PickAlignment.PickAlignmentWindow, PickAlignment.PickAlignmentViewModel>(
            Window, vm => vm.Initialize(null, selectedItems.Count));

        if (!result.OkPressed || string.IsNullOrEmpty(result.Alignment))
        {
            return;
        }

        // Apply alignment to selected subtitles
        ApplyAlignmentToSubtitles(selectedItems, result.Alignment);
    }

    private void ApplyAlignmentToSubtitles(List<BinarySubtitleItem> subtitles, string alignment)
    {
        var marginLeft = Se.Settings.Tools.BinEditLeftMargin;
        var marginTop = Se.Settings.Tools.BinEditTopMargin;
        var marginRight = Se.Settings.Tools.BinEditRightMargin;
        var marginBottom = Se.Settings.Tools.BinEditBottomMargin;

        foreach (var subtitle in subtitles)
        {
            if (subtitle.Bitmap == null)
            {
                continue;
            }

            var screenWidth = subtitle.ScreenSize.Width;
            var screenHeight = subtitle.ScreenSize.Height;
            var imageWidth = (int)subtitle.Bitmap.Size.Width;
            var imageHeight = (int)subtitle.Bitmap.Size.Height;

            // Calculate new position based on alignment with margins
            switch (alignment)
            {
                case "an1": // Bottom Left
                    subtitle.X = marginLeft;
                    subtitle.Y = screenHeight - imageHeight - marginBottom;
                    break;
                case "an2": // Bottom Center
                    subtitle.X = (screenWidth - imageWidth) / 2;
                    subtitle.Y = screenHeight - imageHeight - marginBottom;
                    break;
                case "an3": // Bottom Right
                    subtitle.X = screenWidth - imageWidth - marginRight;
                    subtitle.Y = screenHeight - imageHeight - marginBottom;
                    break;
                case "an4": // Middle Left
                    subtitle.X = marginLeft;
                    subtitle.Y = (screenHeight - imageHeight) / 2;
                    break;
                case "an5": // Middle Center
                    subtitle.X = (screenWidth - imageWidth) / 2;
                    subtitle.Y = (screenHeight - imageHeight) / 2;
                    break;
                case "an6": // Middle Right
                    subtitle.X = screenWidth - imageWidth - marginRight;
                    subtitle.Y = (screenHeight - imageHeight) / 2;
                    break;
                case "an7": // Top Left
                    subtitle.X = marginLeft;
                    subtitle.Y = marginTop;
                    break;
                case "an8": // Top Center
                    subtitle.X = (screenWidth - imageWidth) / 2;
                    subtitle.Y = marginTop;
                    break;
                case "an9": // Top Right
                    subtitle.X = screenWidth - imageWidth - marginRight;
                    subtitle.Y = marginTop;
                    break;
            }
        }

        // Update overlay position if the selected subtitle was adjusted
        UpdateOverlayPosition();
    }

    [RelayCommand]
    private async Task ResizeImages()
    {
        if (Window == null)
        {
            return;
        }

        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, work on all subtitles
        var itemsToResize = selectedItems.Count > 0 ? selectedItems : Subtitles.ToList();

        if (itemsToResize.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Information,
                "No subtitles to resize.",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryResizeImages.BinaryResizeImagesWindow, BinaryResizeImages.BinaryResizeImagesViewModel>(
            Window, vm => vm.Initialize(itemsToResize));

        if (!result.OkPressed)
        {
            return;
        }

        // Refresh grid to show updated bitmaps
        if (SubtitleGrid != null)
        {
            var currentIndex = SubtitleGrid.SelectedIndex;
            SubtitleGrid.ItemsSource = null;
            SubtitleGrid.ItemsSource = Subtitles;
            SubtitleGrid.SelectedIndex = currentIndex;
        }

        UpdateOverlayPosition();
    }

    [RelayCommand]
    private async Task AdjustBrightness()
    {
        if (Window == null)
        {
            return;
        }

        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, work on all subtitles
        var itemsToAdjust = selectedItems.Count > 0 ? selectedItems : Subtitles.ToList();

        if (itemsToAdjust.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Information,
                "No subtitles to adjust.",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryAdjustBrightness.BinaryAdjustBrightnessWindow, BinaryAdjustBrightness.BinaryAdjustBrightnessViewModel>(
            Window, vm => vm.Initialize(itemsToAdjust));

        if (!result.OkPressed)
        {
            return;
        }

        // Refresh grid to show updated bitmaps
        if (SubtitleGrid != null)
        {
            var currentIndex = SubtitleGrid.SelectedIndex;
            SubtitleGrid.ItemsSource = null;
            SubtitleGrid.ItemsSource = Subtitles;
            SubtitleGrid.SelectedIndex = currentIndex;
        }

        UpdateOverlayPosition();
    }

    [RelayCommand]
    private async Task AdjustAlpha()
    {
        if (Window == null)
        {
            return;
        }

        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, work on all subtitles
        var itemsToAdjust = selectedItems.Count > 0 ? selectedItems : Subtitles.ToList();

        if (itemsToAdjust.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Information,
                "No subtitles to adjust.",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryAdjustAlpha.BinaryAdjustAlphaWindow, BinaryAdjustAlpha.BinaryAdjustAlphaViewModel>(
            Window, vm => vm.Initialize(itemsToAdjust));

        if (!result.OkPressed)
        {
            return;
        }

        // Refresh grid to show updated bitmaps
        if (SubtitleGrid != null)
        {
            var currentIndex = SubtitleGrid.SelectedIndex;
            SubtitleGrid.ItemsSource = null;
            SubtitleGrid.ItemsSource = Subtitles;
            SubtitleGrid.SelectedIndex = currentIndex;
        }

        UpdateOverlayPosition();
    }

    [RelayCommand]
    private void CenterHorizontally()
    {
        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, work on all subtitles
        var itemsToResize = selectedItems.Count > 0 ? selectedItems : Subtitles.ToList();

        if (itemsToResize.Count == 0)
        {
            return;
        }

        foreach (var subtitle in itemsToResize)
        {
            if (subtitle.Bitmap == null)
            {
                continue;
            }

            var screenWidth = subtitle.ScreenSize.Width;
            var imageWidth = (int)subtitle.Bitmap.Size.Width;
            subtitle.X = (screenWidth - imageWidth) / 2;
        }

        UpdateOverlayPosition();
    }

    [RelayCommand]
    private void Crop()
    {
        // Get selected subtitles
        var selectedItems = new List<BinarySubtitleItem>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    selectedItems.Add(binaryItem);
                }
            }
        }

        // If no selection, work on all subtitles
        var itemsToResize = selectedItems.Count > 0 ? selectedItems : Subtitles.ToList();

        if (itemsToResize.Count == 0)
        {
            return;
        }

        foreach (var subtitle in itemsToResize)
        {
            if (subtitle.Bitmap == null)
            {
                continue;
            }

            using var skBitmap = subtitle.Bitmap.ToSkBitmap();
            using var cropped = skBitmap.CropTransparentColors(out var offsetX, out var offsetY);
            subtitle.Bitmap = cropped.ToAvaloniaBitmap();
            subtitle.X += offsetX;
            subtitle.Y += offsetY;
        }

        if (SubtitleGrid != null)
        {
            var currentIndex = SubtitleGrid.SelectedIndex;
            SubtitleGrid.ItemsSource = null;
            SubtitleGrid.ItemsSource = Subtitles;
            SubtitleGrid.SelectedIndex = currentIndex;
        }

        UpdateOverlayPosition();
        UpdateStatusText();
    }

    [RelayCommand]
    private async Task AdjustAllTimes()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinaryAdjustAllTimesWindow, BinaryAdjustAllTimesViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        // Get selected indices from the grid
        var selectedIndices = new List<int>();
        if (SubtitleGrid?.SelectedItems != null)
        {
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    var index = Subtitles.IndexOf(binaryItem);
                    if (index >= 0)
                    {
                        selectedIndices.Add(index);
                    }
                }
            }
        }

        result.AdjustTimes(Subtitles.ToList(), selectedIndices.Count > 0 ? selectedIndices : null);
    }

    [RelayCommand]
    private async Task ChangeFrameRate()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ChangeFrameRateWindow, ChangeFrameRateViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        var ratio = result.SelectedToFrameRate / result.SelectedFromFrameRate;

        // If there are selected items in the grid, apply only to them
        var appliedToSelected = false;
        if (SubtitleGrid?.SelectedItems != null && SubtitleGrid.SelectedItems.Count > 0)
        {
            var selectedIndices = new List<int>();
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    var index = Subtitles.IndexOf(binaryItem);
                    if (index >= 0)
                    {
                        selectedIndices.Add(index);
                    }
                }
            }

            if (selectedIndices.Count > 0)
            {
                foreach (var idx in selectedIndices)
                {
                    var s = Subtitles[idx];
                    s.StartTime = TimeSpan.FromMilliseconds(s.StartTime.TotalMilliseconds * ratio);
                    s.EndTime = TimeSpan.FromMilliseconds(s.EndTime.TotalMilliseconds * ratio);
                }

                appliedToSelected = true;
            }
        }

        if (!appliedToSelected)
        {
            // Apply to all subtitles
            foreach (var s in Subtitles)
            {
                s.StartTime = TimeSpan.FromMilliseconds(s.StartTime.TotalMilliseconds * ratio);
                s.EndTime = TimeSpan.FromMilliseconds(s.EndTime.TotalMilliseconds * ratio);
            }
        }
    }

    [RelayCommand]
    private async Task ChangeSpeed()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<ChangeSpeedWindow, ChangeSpeedViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }

        // result.SpeedPercent is percentage; factor is 100 / percent as used elsewhere
        var factor = 100.0 / result.SpeedPercent;

        var appliedToSelected = false;
        if (SubtitleGrid?.SelectedItems != null && SubtitleGrid.SelectedItems.Count > 0)
        {
            var selectedIndices = new List<int>();
            foreach (var item in SubtitleGrid.SelectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    var index = Subtitles.IndexOf(binaryItem);
                    if (index >= 0)
                    {
                        selectedIndices.Add(index);
                    }
                }
            }

            if (selectedIndices.Count > 0)
            {
                foreach (var idx in selectedIndices)
                {
                    var s = Subtitles[idx];
                    s.StartTime = TimeSpan.FromMilliseconds(s.StartTime.TotalMilliseconds * factor);
                    s.EndTime = TimeSpan.FromMilliseconds(s.EndTime.TotalMilliseconds * factor);
                }

                appliedToSelected = true;
            }
        }

        if (!appliedToSelected)
        {
            foreach (var s in Subtitles)
            {
                s.StartTime = TimeSpan.FromMilliseconds(s.StartTime.TotalMilliseconds * factor);
                s.EndTime = TimeSpan.FromMilliseconds(s.EndTime.TotalMilliseconds * factor);
            }
        }
    }

    [RelayCommand]
    private async Task OpenVideo()
    {
        if (Window == null)
        {
            return;
        }

        if (VideoPlayerControl == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(VideoPlayerControl.VideoPlayer.FileName))
        {
            VideoPlayerControl.VideoPlayer.CloseFile();
        }

        var videoFileName = await _fileHelper.PickOpenVideoFile(Window, Se.Language.General.OpenVideoFileTitle);
        if (string.IsNullOrEmpty(videoFileName))
        {
            return;
        }

        await VideoPlayerControl.Open(videoFileName);
    }

    [RelayCommand]
    private async Task Settings()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BinarySettings.BinarySettingsWindow, BinarySettings.BinarySettingsViewModel>(Window, vm => { });

        if (!result.OkPressed)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task ExportImage()
    {
        if (Window == null)
        {
            return;
        }

        var selectedItem = SelectedSubtitle;
        if (selectedItem == null || selectedItem.Bitmap == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".png", "export.png", Se.Language.General.SaveImageAs);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var skBitmap = selectedItem.Bitmap.ToSkBitmap();
        var pngBytes = skBitmap.ToPngArray();
        System.IO.File.WriteAllBytes(fileName, pngBytes);
    }

    [RelayCommand]
    private void InsertBefore()
    {
        if (Window == null || SubtitleGrid?.SelectedItems.Count != 1)
        {
            return;
        }

        var selectedItem = SelectedSubtitle;
        if (selectedItem == null || selectedItem.Bitmap == null)
        {
            return;
        }

        var newItem = new BinarySubtitleItem(selectedItem);
        newItem.EndTime = TimeSpan.FromMilliseconds(selectedItem.StartTime.TotalMilliseconds + Se.Settings.General.MinimumMillisecondsBetweenLines);
        newItem.StartTime = TimeSpan.FromMilliseconds(newItem.EndTime.TotalMilliseconds - Se.Settings.General.NewEmptyDefaultMs);
        var selectedIndex = SubtitleGrid.SelectedIndex;
        Subtitles.Insert(selectedIndex, newItem);
        Renumber();
        SelectAndScrollToRow(selectedIndex);
    }

    [RelayCommand]
    private void InsertAfter()
    {
        if (Window == null || SubtitleGrid?.SelectedItems.Count != 1)
        {
            return;
        }

        var selectedItem = SelectedSubtitle;
        if (selectedItem == null || selectedItem.Bitmap == null)
        {
            return;
        }

        var newItem = new BinarySubtitleItem(selectedItem);
        newItem.StartTime = TimeSpan.FromMilliseconds(selectedItem.EndTime.TotalMilliseconds + Se.Settings.General.MinimumMillisecondsBetweenLines);
        newItem.EndTime = TimeSpan.FromMilliseconds(newItem.StartTime.TotalMilliseconds + Se.Settings.General.NewEmptyDefaultMs);
        var selectedIndex = SubtitleGrid.SelectedIndex;
        Subtitles.Insert(selectedIndex + 1, newItem);
        Renumber();
        SelectAndScrollToRow(selectedIndex + 1);
    }

    [RelayCommand]
    private void ToggleForced()
    {
        if (Window == null || SubtitleGrid == null || SubtitleGrid.SelectedItems.Count <= 0)
        {
            return;
        }

        var selectedItems = SubtitleGrid.SelectedItems.Cast<BinarySubtitleItem>().ToList();
        foreach (var item in selectedItems)
        {
            item.IsForced = !item.IsForced;
        }
    }

    private void Renumber()
    {
        for (int i = 0; i < Subtitles.Count; i++)
        {
            Subtitles[i].Number = i + 1;
        }
    }

    [RelayCommand]
    private async Task ImportImage()
    {
        if (Window == null)
        {
            return;
        }

        var selectedItem = SelectedSubtitle;
        if (selectedItem == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenImageFile(Window, Se.Language.General.OpenImageFile);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            using var stream = File.OpenRead(fileName);
            var skBitmap = SkiaSharp.SKBitmap.Decode(stream);
            if (skBitmap == null)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, "Unable to load image file.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            selectedItem.Bitmap = skBitmap.ToAvaloniaBitmap();

            UpdateOverlayPosition();
            UpdateStatusText();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, $"Failed to import image: {ex.Message}",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    [RelayCommand]
    private async Task SetText()
    {
        if (Window == null)
        {
            return;
        }

        // Only allow if exactly one subtitle is selected
        if (SelectedSubtitle == null)
        {
            await MessageBox.Show(Window, "No subtitle selected", "Please select exactly one subtitle.",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Show the SetText dialog
        var result = await _windowService.ShowDialogAsync<SetText.SetTextWindow, SetText.SetTextViewModel>(Window!);

        if (result.OkPressed && result.ResultBitmap != null)
        {
            // Replace the selected subtitle's bitmap
            SelectedSubtitle.Bitmap?.Dispose();
            SelectedSubtitle.Bitmap = result.ResultBitmap.ToAvaloniaBitmap();

            // Update the overlay
            UpdateOverlayPosition();
            UpdateStatusText();

            // Clean up
            result.ResultBitmap.Dispose();
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task InsertSubtitle()
    {
        var selectedItem = SelectedSubtitle;
        if (Window == null || selectedItem == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.General.OpenSubtitleFileTitle, Se.Language.General.ImagedBasedSubtitles, "*.sup;*.sub;*.ts;*.xml;*.mkv;*.mks", Se.Language.General.AllFiles, "*.*");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var imageSubtitle = await LoadImageSubtitle(fileName);
        if (imageSubtitle == null)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "Image based subtitle format not found/supported.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var ocrItems = imageSubtitle.MakeOcrSubtitleItems();
        if (ocrItems.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "No subtitles found in the file.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var oldLastTime = selectedItem.EndTime.TotalMilliseconds;
        var newFirstTime = ocrItems.First().StartTime.TotalMilliseconds;
        var timeOffset = oldLastTime - newFirstTime + 1000;
        foreach (var ocrItem in ocrItems)
        {
            var newItem = new BinarySubtitleItem(ocrItem, -1);
            newItem.StartTime = TimeSpan.FromMilliseconds(ocrItem.StartTime.TotalMilliseconds + timeOffset);
            newItem.EndTime = TimeSpan.FromMilliseconds(ocrItem.EndTime.TotalMilliseconds + timeOffset);
            Subtitles.Add(newItem);
        }

        Renumber();
        UpdateStatusText();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
            return;
        }

        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            Ok();
            return;
        }

        // Handle shortcuts
        _shortcutManager.OnKeyPressed(this, e);
        if (_shortcutManager.GetActiveKeys().Count == 0)
        {
            return;
        }

        var relayCommand = _shortcutManager.CheckShortcuts(e, "General");
        if (relayCommand != null)
        {
            e.Handled = true;
            relayCommand.Execute(null);
        }
    }

    public void OnKeyUp(KeyEventArgs e)
    {
        _shortcutManager.OnKeyReleased(this, e);
    }

    public void Closing()
    {
        if (VideoPlayerControl == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(VideoPlayerControl.VideoPlayer.FileName))
        {
            return;
        }

        VideoPlayerControl.VideoPlayer.CloseFile();
        UiUtil.SaveWindowPosition(Window);
    }

    public void Loaded()
    {
        UiUtil.RestoreWindowPosition(Window);
        UiUtil.SetupWindowsSystemMenu(Window);

        RegisterVideoShortcuts();

        if (string.IsNullOrEmpty(_loadFileName))
        {
            return;
        }

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await DoFileOpen(_loadFileName);
        });
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || SubtitleGrid == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (SubtitleGrid.SelectedIndex != index)
            {
                SubtitleGrid.SelectedIndex = index;
            }

            SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem, null);
        });
    }

    internal void OnDataGridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            DeleteSectedLines();
        }
    }

    [RelayCommand]
    private void DeleteSectedLines()
    {
        Dispatcher.UIThread.Post(async void () =>
        {
            var selectedItems = SubtitleGrid?.SelectedItems?.Cast<BinarySubtitleItem>().ToList();
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            var answer = MessageBoxResult.Yes;

            if (Se.Settings.General.PromptBeforeDelete)
            {
                if (selectedItems.Count == 1)
                {
                    answer = await MessageBox.Show(
                        Window!,
                        Se.Language.General.DeleteLines,
                        $"Do you want to delete one line?",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                }
                else
                {
                    answer = await MessageBox.Show(
                        Window!,
                        Se.Language.General.DeleteLines,
                        $"Do you want to delete {selectedItems.Count} lines?",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                }
            }

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            var itemsToRemove = new List<BinarySubtitleItem>();
            foreach (var item in selectedItems)
            {
                if (item is BinarySubtitleItem binaryItem)
                {
                    itemsToRemove.Add(binaryItem);
                }
            }

            foreach (var item in itemsToRemove)
            {
                Subtitles.Remove(item);
                item.Bitmap?.Dispose();
            }

            UpdateStatusText();

            return;
        });
    }

    internal void OnContextMenuOpening()
    {
        var selectedCount = SubtitleGrid?.SelectedItems?.Count ?? 0;
        var selectedIndex = SubtitleGrid?.SelectedIndex ?? -1;
        IsDeleteVisible = selectedCount > 0;
        IsToggleForcedVisible = selectedCount > 0;
        IsInsertAfterVisible = selectedCount == 1;
        IsInsertBeforeVisible = selectedCount == 1;
        IsInsertSubtitleVisible = selectedCount == 1 && selectedIndex == Subtitles.Count - 1;
    }

    internal void OnDataGridDoubleTapped(TappedEventArgs e)
    {
        var vp = VideoPlayerControl;
        var item = SubtitleGrid?.SelectedItem as BinarySubtitleItem;
        if (vp == null || item == null)
        {
            return;
        }

        vp.Position = item.StartTime.TotalSeconds;
    }
}

