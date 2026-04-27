using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

    internal void Initialize(string videoFileName, FfmpegMediaInfo2 mediaInfo)
    {
        _videoFileName = videoFileName;

        var sb = new StringBuilder();
        sb.AppendLine($"File name: {videoFileName}");
        sb.AppendLine($"File size: {Utilities.FormatBytesToDisplayFileSize(new FileInfo(videoFileName).Length)}");

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

        if (FileUtil.IsWav(_videoFileName))
        {
            sb.AppendLine($"Codec: WAVE");
        }

        if (FileUtil.IsMp3(_videoFileName))
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

        Text = sb.ToString().Trim();

        Dispatcher.UIThread.Post(() =>
        {
            Task.Delay(50).Wait(); // Slight delay to ensure control is ready  

            SourceViewTextBox = CreateAdvancedTextBoxWrapper(Text);

            TextBoxContainer.Child = SourceViewTextBox.ContentControl;

            Task.Delay(50).Wait(); // Slight delay to ensure control is ready  
            SourceViewTextBox.Focus();
            SourceViewTextBox.CaretIndex = 0;
        }, DispatcherPriority.Input);
    }

    private TextEditorWrapper CreateAdvancedTextBoxWrapper(string text)
    {
        var textBox = new TextEditor
        {
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ShowLineNumbers = false,
            WordWrap = true,
            IsReadOnly = true,
        };

        // Override the built-in link color with our softer pastel color
        textBox.TextArea.TextView.LinkTextForegroundBrush = UiUtil.MakeLinkForeground();

        // Add syntax highlighting for subtitle source formats
        textBox.TextArea.TextView.LineTransformers.Add(new MediaInfoSyntaxHighlighting());

        // Setup two-way binding manually since TextEditor doesn't support direct binding
        var isUpdatingFromViewModel = false;
        var isUpdatingFromEditor = false;

        void UpdateEditorFromViewModel()
        {
            if (isUpdatingFromEditor)
            {
                return;
            }

            isUpdatingFromViewModel = true;
            try
            {
                var text = Text ?? string.Empty;
                if (textBox.Text != text)
                {
                    textBox.Text = text;
                }
            }
            finally
            {
                isUpdatingFromViewModel = false;
            }
        }

        void UpdateViewModelFromEditor()
        {
            if (isUpdatingFromViewModel)
            {
                return;
            }

            isUpdatingFromEditor = true;
            try
            {
                if (Text != textBox.Text)
                {
                    Text = textBox.Text;
                }
            }
            finally
            {
                isUpdatingFromEditor = false;
            }
        }

        // Listen to ViewModel changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Text))
            {
                UpdateEditorFromViewModel();
            }
        };

        // Listen to TextEditor changes
        textBox.TextChanged += (s, e) => UpdateViewModelFromEditor();

        // Initial text load
        UpdateEditorFromViewModel();

        var textBoxBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Child = textBox,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        return new TextEditorWrapper(textBox, textBoxBorder);
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