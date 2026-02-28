using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;

public partial class AssaResolutionResamplerViewModel : ObservableObject
{
    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    [ObservableProperty] private int _sourceWidth = 384;
    [ObservableProperty] private int _sourceHeight = 288;
    [ObservableProperty] private int _targetWidth = 1920;
    [ObservableProperty] private int _targetHeight = 1080;
    [ObservableProperty] private bool _changeMargins = true;
    [ObservableProperty] private bool _changeFontSize = true;
    [ObservableProperty] private bool _changePositions = true;
    [ObservableProperty] private bool _changeDrawing = true;

    private Subtitle _subtitle = new();
    private string? _videoFileName;

    public Subtitle ResultSubtitle => _subtitle;

    public AssaResolutionResamplerViewModel()
    {
    }

    public void Initialize(Subtitle subtitle, string? videoFileName, int? videoWidth, int? videoHeight)
    {
        _subtitle = new Subtitle(subtitle, false);
        _videoFileName = videoFileName;

        if (string.IsNullOrEmpty(_subtitle.Header))
        {
            _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        // Get source resolution from subtitle header
        var oldPlayResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", _subtitle.Header);
        if (int.TryParse(oldPlayResX, out var w) && w > 0)
        {
            SourceWidth = w;
        }

        var oldPlayResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", _subtitle.Header);
        if (int.TryParse(oldPlayResY, out var h) && h > 0)
        {
            SourceHeight = h;
        }

        // Set target resolution from video if available
        if (videoWidth.HasValue && videoWidth.Value > 0)
        {
            TargetWidth = videoWidth.Value;
        }
        else
        {
            TargetWidth = SourceWidth;
        }

        if (videoHeight.HasValue && videoHeight.Value > 0)
        {
            TargetHeight = videoHeight.Value;
        }
        else
        {
            TargetHeight = SourceHeight;
        }
    }

    [RelayCommand]
    private async Task SourceFromVideo()
    {
        if (Window == null)
        {
            return;
        }

        var files = await Window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Se.Language.General.OpenVideoFile,
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("Video files") { Patterns = new[] { "*.mp4", "*.mkv", "*.avi", "*.mov", "*.wmv", "*.webm", "*.ts", "*.m2ts" } },
                new("All files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0)
        {
            var fileName = files[0].Path.LocalPath;
            var info = FfmpegMediaInfo2.Parse(fileName);
            if (info?.Dimension is { Width: > 0, Height: > 0 })
            {
                SourceWidth = info.Dimension.Width;
                SourceHeight = info.Dimension.Height;
            }
        }
    }

    [RelayCommand]
    private async Task TargetFromVideo()
    {
        if (Window == null)
        {
            return;
        }

        var files = await Window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Se.Language.General.OpenVideoFile,
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new("Video files") { Patterns = new[] { "*.mp4", "*.mkv", "*.avi", "*.mov", "*.wmv", "*.webm", "*.ts", "*.m2ts" } },
                new("All files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0)
        {
            var fileName = files[0].Path.LocalPath;
            var info = FfmpegMediaInfo2.Parse(fileName);
            if (info?.Dimension is { Width: > 0, Height: > 0 })
            {
                TargetWidth = info.Dimension.Width;
                TargetHeight = info.Dimension.Height;
            }
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (!ChangeMargins && !ChangeFontSize && !ChangePositions && !ChangeDrawing)
        {
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.Assa.ResolutionResamplerTitle, Se.Language.Assa.ResolutionResamplerNothingSelected);
            }
            return;
        }

        if (SourceWidth == TargetWidth && SourceHeight == TargetHeight)
        {
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.Assa.ResolutionResamplerTitle, Se.Language.Assa.ResolutionResamplerSourceAndTargetEqual);
            }
            return;
        }

        if (SourceWidth == 0 || SourceHeight == 0 || TargetWidth == 0 || TargetHeight == 0)
        {
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.Assa.ResolutionResamplerTitle, "Video width/height cannot be zero.");
            }
            return;
        }

        AssaResamplerHelper.
                ApplyResampling(_subtitle, SourceWidth, SourceHeight, TargetWidth, TargetHeight, ChangeMargins, ChangeFontSize, ChangeDrawing, ChangePositions);
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
