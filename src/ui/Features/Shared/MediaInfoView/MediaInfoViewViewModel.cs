using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Shared.MediaInfoView;

public partial class MediaInfoViewViewModel : ObservableObject
{
    [ObservableProperty] private string _text;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public ITextBoxWrapper SourceViewTextBox { get; set; }
    public Border TextBoxContainer { get; set; }

    private readonly IFolderHelper _folderHelper;

    private string _videoFileName = string.Empty;

    public MediaInfoViewViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
        SourceViewTextBox = new TextBoxWrapper(new TextBox());
        Text = string.Empty;
        TextBoxContainer = new Border();
    }

    internal void Initialize(string videoFileName, FfmpegMediaInfo2? mediaInfo)
    {
        _videoFileName = videoFileName;

        // Show basic file info immediately so the window can open without waiting
        // for container/ffmpeg parsing on large files.
        Text = BuildBasicInfoText(videoFileName, includeLoadingHint: true);

        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Delay(50); // Slight delay to ensure control is ready

            SourceViewTextBox = CreateAdvancedTextBoxWrapper();

            TextBoxContainer.Child = SourceViewTextBox.ContentControl;

            await Task.Delay(50); // Slight delay to ensure control is ready
            SourceViewTextBox.Focus();
            SourceViewTextBox.CaretIndex = 0;
        }, DispatcherPriority.Input);

        // Build the full report in the background — MatroskaFile/MP4Parser and
        // FfmpegMediaInfo2.Parse can be slow on large files.
        Task.Run(() =>
        {
            var info = mediaInfo ?? FfmpegMediaInfo2.Parse(videoFileName);
            var fullText = BuildFullInfoText(videoFileName, info);
            Dispatcher.UIThread.Post(() => Text = fullText);
        });
    }

    private static string BuildBasicInfoText(string videoFileName, bool includeLoadingHint)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"File name: {videoFileName}");
        try
        {
            sb.AppendLine($"File size: {Utilities.FormatBytesToDisplayFileSize(new FileInfo(videoFileName).Length)}");
        }
        catch
        {
            // ignored — file may be inaccessible
        }

        if (includeLoadingHint)
        {
            sb.AppendLine();
            sb.AppendLine("Loading media information...");
        }

        return sb.ToString().Trim();
    }

    private static string BuildFullInfoText(string videoFileName, FfmpegMediaInfo2 mediaInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine(BuildBasicInfoText(videoFileName, includeLoadingHint: false));

        if (mediaInfo.Duration != null)
        {
            sb.AppendLine($"Duration: {new TimeCode(mediaInfo.Duration.TotalMilliseconds).ToShortDisplayString()}");
        }

        if (mediaInfo.Dimension.Width > 0 && mediaInfo.Dimension.Height > 0)
        {
            sb.AppendLine($"Resolution: {mediaInfo.Dimension.Width}x{mediaInfo.Dimension.Height}");
        }

        if (mediaInfo.FramesRate > 0)
        {
            sb.AppendLine($"Framerate: {mediaInfo.FramesRate:0.###}");
        }

        if (FileUtil.IsWav(videoFileName))
        {
            sb.AppendLine($"Codec: WAVE");
        }

        if (FileUtil.IsMp3(videoFileName))
        {
            sb.AppendLine($"Codec: MP3");
        }

        var mkvParser = new MatroskaFile(videoFileName);
        if (mkvParser.IsValid)
        {
            sb.AppendLine($"Container: Matroska (mkv/webm)");
            var chapters = mkvParser.GetChapters();
            if (chapters.Count > 0)
            {
                sb.AppendLine($" - Chapters: {chapters.Count}");
            }
        }
        else
        {
            var mp4Parser = new MP4Parser(videoFileName);
            if (mp4Parser.Duration.TotalMilliseconds > 0)
            {
                sb.AppendLine($"Container: MP4");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Tracks:");
        var trackNo = 1;
        foreach (var ffmpegTrackInfo in mediaInfo.Tracks)
        {
            sb.AppendLine($"#{trackNo} - {ffmpegTrackInfo.TrackType}");
            sb.AppendLine(ffmpegTrackInfo.TrackInfo);
            sb.AppendLine();
            trackNo++;
        }

        return sb.ToString().Trim();
    }

    private TextBoxWrapper CreateAdvancedTextBoxWrapper()
    {
        var textBox = new SyntaxHighlightingTextBox
        {
            SourceHighlighter = new MediaInfoSyntaxHighlighting(),
            Text = Text ?? string.Empty,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            IsReadOnly = true,
            IsUndoEnabled = false,
            [ScrollViewer.VerticalScrollBarVisibilityProperty] = ScrollBarVisibility.Auto,
            [ScrollViewer.HorizontalScrollBarVisibilityProperty] = ScrollBarVisibility.Disabled,
        };

        // The full report arrives from a background parse, so the box follows the view model.
        // One way only - the box is read-only, nothing can edit it back.
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Text))
            {
                textBox.Text = Text ?? string.Empty;
            }
        };

        return new TextBoxWrapper(textBox);
    }

    [RelayCommand]
    private void Ok()
    {
        if (Window == null)
        {
            return;
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task OpenContainingFolder()
    {
        if (string.IsNullOrEmpty(_videoFileName) || Window == null)
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window, _videoFileName);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        OnKeyDown(e);
    }
}