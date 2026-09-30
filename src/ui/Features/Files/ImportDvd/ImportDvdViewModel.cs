using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ImportDvd;

/// <summary>
/// "Import subtitles from DVD": pick an IFO (which lists the titles and finds the VOB files) and/or
/// VOB files, then rip the subpicture streams - see <see cref="DvdSubtitleRipper"/>.
/// </summary>
public partial class ImportDvdViewModel : ObservableObject
{
    [ObservableProperty] private string _ifoFileName;
    [ObservableProperty] private ObservableCollection<DvdTitleDisplay> _titles;
    [ObservableProperty] private DvdTitleDisplay? _selectedTitle;
    [ObservableProperty] private bool _hasTitles;
    [ObservableProperty] private string _info;
    [ObservableProperty] private ObservableCollection<DvdVobFileItem> _vobFiles;
    [ObservableProperty] private DvdVobFileItem? _selectedVobFile;
    [ObservableProperty] private bool _isPal;
    [ObservableProperty] private bool _isNtsc;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isRipping;
    [ObservableProperty] private bool _isNotRipping;
    [ObservableProperty] private bool _canRip;
    [ObservableProperty] private bool _isVobListEmpty;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>The ripped subtitles (all streams) - set when <see cref="OkPressed"/>.</summary>
    public List<VobSubMergedPack> MergedPacks { get; private set; } = new();
    public List<SKColor> Palette { get; private set; } = new();

    /// <summary>Stream languages in the idx format "{Language} (0x{id:x})".</summary>
    public List<string> Languages { get; private set; } = new();
    public string FileName { get; private set; } = string.Empty;

    private readonly IFileHelper _fileHelper;
    private IfoParser? _ifo;
    private CancellationTokenSource? _cancellationTokenSource;
    private int _lastPercent = -1;

    public ImportDvdViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        _ifoFileName = string.Empty;
        _titles = new ObservableCollection<DvdTitleDisplay>();
        _info = string.Empty;
        _vobFiles = new ObservableCollection<DvdVobFileItem>();
        _statusText = Se.Language.File.Import.DvdDropHint;
        _isPal = true;
        _isNotRipping = true;
        _isVobListEmpty = true;
        VobFiles.CollectionChanged += (_, _) => UpdateCanRip();
    }

    /// <summary>
    /// Optionally starts with an IFO or a VOB file (e.g. one opened via File - Open).
    /// </summary>
    public void Initialize(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return;
        }

        if (IfoParser.IsIfo(fileName))
        {
            OpenIfo(fileName);
        }
        else
        {
            AddVobFiles(new[] { fileName });
        }
    }

    public string? GetLanguageCode(int streamId)
    {
        return _ifo?.GetLanguageCode(streamId);
    }

    partial void OnIsPalChanged(bool value)
    {
        IsNtsc = !value;
    }

    partial void OnIsNtscChanged(bool value)
    {
        IsPal = !value;
    }

    partial void OnIsRippingChanged(bool value)
    {
        IsNotRipping = !value;
        UpdateCanRip();
    }

    partial void OnSelectedTitleChanged(DvdTitleDisplay? value)
    {
        if (value == null)
        {
            return;
        }

        var title = value.Title;
        _ifo = title.Ifo;
        IsPal = title.Ifo.IsPal;
        SetVobFiles(title.VobFileNames);
        UpdateInfo();
    }

    private void OpenIfo(string fileName)
    {
        var ifo = new IfoParser(fileName);
        if (ifo.Type == IfoParser.IfoType.Unknown)
        {
            StatusText = string.Format(Se.Language.File.Import.DvdNotAnIfoFileX, Path.GetFileName(fileName));
            return;
        }

        IfoFileName = fileName;
        Titles.Clear();
        var titles = DvdTitle.Find(fileName);
        if (titles.Count == 0)
        {
            HasTitles = false;
            _ifo = ifo.Type == IfoParser.IfoType.VideoTitleSet ? ifo : null;
            SetVobFiles(ifo.Type == IfoParser.IfoType.VideoTitleSet ? IfoParser.GetTitleVobFiles(Path.ChangeExtension(fileName, ".IFO")) : new List<string>());
            StatusText = ifo.Type == IfoParser.IfoType.VideoManager
                ? string.Format(Se.Language.File.Import.DvdMenuIfoNoTitleSetsX, Path.GetFileName(fileName))
                : string.Empty;
            UpdateInfo();
            return;
        }

        var multipleTitleSets = titles.Select(p => p.TitleSetNumber).Distinct().Count() > 1;
        foreach (var title in titles)
        {
            var name = multipleTitleSets
                ? $"{title.TitleSetNumber}.{title.ProgramChain.Number}"
                : title.ProgramChain.Number.ToString(CultureInfo.InvariantCulture);
            var languages = string.Join(", ", title.Ifo.SubtitleStreams
                .Where(p => title.ProgramChain.HasSubtitleStream(p.Index))
                .Select(p => p.ToString())
                .Distinct());
            var status = title.IsComplete
                ? string.Empty
                : string.Format(Se.Language.File.DvdVobFilesMissingX, (int)Math.Round(title.AvailableShare * 100));
            Titles.Add(new DvdTitleDisplay(title, name, languages, status));
        }

        HasTitles = true;
        StatusText = string.Empty;
        var defaultTitle = DvdTitle.GetDefault(titles);
        SelectedTitle = Titles.FirstOrDefault(p => p.Title == defaultTitle) ?? Titles[0];
    }

    private void SetVobFiles(IEnumerable<string> fileNames)
    {
        VobFiles.Clear();
        foreach (var fileName in fileNames)
        {
            VobFiles.Add(new DvdVobFileItem(fileName));
        }
    }

    private void AddVobFiles(IEnumerable<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            if (VobFiles.All(p => !string.Equals(p.FileName, fileName, StringComparison.OrdinalIgnoreCase)))
            {
                VobFiles.Add(new DvdVobFileItem(fileName));
            }
        }

        // loose VOBs: palette, languages and PAL/NTSC from the title set's IFO when it is there
        if (_ifo == null && VobFiles.Count > 0)
        {
            var ifoFileName = IfoParser.GetIfoFileName(VobFiles[0].FileName);
            var ifo = ifoFileName == null ? null : new IfoParser(ifoFileName);
            if (ifo?.Type == IfoParser.IfoType.VideoTitleSet)
            {
                _ifo = ifo;
                IsPal = ifo.IsPal;
            }
        }

        UpdateInfo();
    }

    private void UpdateInfo()
    {
        if (_ifo == null)
        {
            Info = string.Empty;
            return;
        }

        var languages = string.Join(", ", _ifo.SubtitleStreams.Select(p => p.ToString()).Distinct());
        Info = string.IsNullOrEmpty(languages) ? _ifo.Video.ToString() : $"{_ifo.Video}  •  {languages}";
    }

    private void UpdateCanRip()
    {
        IsVobListEmpty = VobFiles.Count == 0;
        CanRip = VobFiles.Count > 0 && !IsRipping;
    }

    /// <summary>
    /// The chosen title's cells can only be ripped from the title set's own VOB files (sector
    /// numbers count from the start of VTS_xx_1.VOB) - after the list is edited, every VOB is read.
    /// </summary>
    private DvdTitle? GetTitleToRip(List<string> vobFileNames)
    {
        var title = SelectedTitle?.Title;
        if (title == null ||
            !title.VobFileNames.SequenceEqual(vobFileNames, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        return title;
    }

    [RelayCommand]
    private async Task BrowseIfo()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.File.Import.DvdOpenIfoFile, Se.Language.File.Import.DvdIfoFiles, "*.ifo", Se.Language.General.AllFiles, "*.*");
        if (!string.IsNullOrEmpty(fileName))
        {
            OpenIfo(fileName);
        }
    }

    [RelayCommand]
    private async Task AddVob()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenFiles(Window, Se.Language.File.Import.DvdAddVobFiles, Se.Language.File.Import.DvdVobFileType, new List<string> { "*.vob" }, Se.Language.General.AllFiles, new List<string> { "*.*" });
        AddVobFiles(fileNames.OrderBy(p => p, StringComparer.OrdinalIgnoreCase));
    }

    [RelayCommand]
    private void RemoveVob()
    {
        var item = SelectedVobFile;
        if (item == null)
        {
            return;
        }

        var index = VobFiles.IndexOf(item);
        VobFiles.Remove(item);
        SelectedVobFile = VobFiles.ElementAtOrDefault(Math.Min(index, VobFiles.Count - 1));
    }

    [RelayCommand]
    private void MoveVobUp()
    {
        var item = SelectedVobFile;
        var index = item == null ? -1 : VobFiles.IndexOf(item);
        if (index > 0)
        {
            VobFiles.Move(index, index - 1);
            SelectedVobFile = item;
        }
    }

    [RelayCommand]
    private void MoveVobDown()
    {
        var item = SelectedVobFile;
        var index = item == null ? -1 : VobFiles.IndexOf(item);
        if (index >= 0 && index < VobFiles.Count - 1)
        {
            VobFiles.Move(index, index + 1);
            SelectedVobFile = item;
        }
    }

    [RelayCommand]
    private void ClearVobs()
    {
        VobFiles.Clear();
        Titles.Clear();
        HasTitles = false;
        IfoFileName = string.Empty;
        _ifo = null;
        UpdateInfo();
        StatusText = Se.Language.File.Import.DvdDropHint;
    }

    [RelayCommand]
    private async Task StartRipping()
    {
        if (IsRipping || VobFiles.Count == 0)
        {
            return;
        }

        var vobFileNames = VobFiles.Select(p => p.FileName).ToList();
        var title = GetTitleToRip(vobFileNames);
        var isPal = IsPal;
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        _lastPercent = -1;
        ProgressValue = 0;
        StatusText = Se.Language.Main.ReadingDvdSubtitles;
        IsRipping = true;
        var packCount = 0;
        var encryptedPackCount = 0;
        try
        {
            MergedPacks = await Task.Run(() =>
            {
                var packs = title != null
                    ? DvdSubtitleRipper.Rip(vobFileNames, title.ProgramChain, ReportProgress, cancellationToken)
                    : DvdSubtitleRipper.Rip(vobFileNames, ReportProgress, cancellationToken);
                packCount = packs.Count;
                encryptedPackCount = DvdSubtitleRipper.CountEncrypted(packs);
                var parser = new VobSubParser(isPal);
                parser.VobSubPacks.AddRange(packs);
                return parser.MergeVobSubPacks();
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            StatusText = Se.Language.File.Import.DvdRippingAborted;
            ProgressValue = 0;
            return;
        }
        catch (Exception exception)
        {
            StatusText = exception.Message;
            return;
        }
        finally
        {
            IsRipping = false;
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        if (MergedPacks.Count == 0)
        {
            StatusText = Se.Language.General.NoSubtitlesFound;
            return;
        }

        // SE does not decrypt CSS - a VOB copied without decrypting gives garbled images
        if (encryptedPackCount > 0 && Window != null)
        {
            var message = string.Format(Se.Language.File.Import.DvdEncryptedXOfY, encryptedPackCount, packCount);
            StatusText = message.Split('\n')[0];
            var answer = await MessageBox.Show(Window, Se.Language.General.Warning, message, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        Palette = title?.ProgramChain.Palette is { Count: > 0 } titlePalette ? titlePalette : _ifo?.Palette ?? new List<SKColor>();
        Languages = _ifo?.GetLanguages() ?? new List<string>();
        FileName = vobFileNames[0];
        OkPressed = true;
        Close();
    }

    private void ReportProgress(long position, long total)
    {
        var percent = total <= 0 ? 0 : (int)(position * 100 / total);
        if (percent == _lastPercent)
        {
            return;
        }

        _lastPercent = percent;
        Dispatcher.UIThread.Post(() => ProgressValue = percent);
    }

    [RelayCommand]
    private void Abort()
    {
        _cancellationTokenSource?.Cancel();
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            if (IsRipping)
            {
                Abort();
            }
            else
            {
                Cancel();
            }
        }
    }

    internal void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = !IsRipping && e.DataTransfer.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    internal void OnDrop(object? sender, DragEventArgs e)
    {
        if (IsRipping)
        {
            return;
        }

        var fileNames = e.DataTransfer.TryGetFiles()?
            .Select(p => p.Path?.LocalPath)
            .Where(p => !string.IsNullOrEmpty(p) && File.Exists(p))
            .Cast<string>()
            .ToList() ?? new List<string>();

        var ifoFileName = fileNames.FirstOrDefault(p => p.EndsWith(".ifo", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".bup", StringComparison.OrdinalIgnoreCase));
        if (ifoFileName != null)
        {
            OpenIfo(ifoFileName);
            return;
        }

        AddVobFiles(fileNames.Where(p => p.EndsWith(".vob", StringComparison.OrdinalIgnoreCase)).OrderBy(p => p, StringComparer.OrdinalIgnoreCase));
    }
}
