using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Data;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;

public sealed class DownloadTtsWindow : Window
{
    private readonly DownloadTtsViewModel _vm;

    public DownloadTtsWindow(DownloadTtsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TtsDownloadEngineTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;
        _vm = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsUncentered | EditorWindowStyles.MuteDisabledPrimary);

        var titleText = new TextBlock
        {
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 460,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TitleText)),
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.MinWidth = 400;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var statusText = new TextBlock
        {
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 460,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        // The view models set Error on every failure path, but nothing rendered it - the user saw
        // only the generic "Download failed" while the real cause was written to a property no
        // window bound. DownloadVideoFromUrlWindow and DownloadCrispEmbedWindow show it this way.
        var errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            MaxWidth = 460,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Error)));
        errorText.Bind(IsVisibleProperty, new Binding(nameof(vm.Error))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)),
        });

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeFlushButtonBar(null, buttonCancel);

        var body = new Border
        {
            Background = EditorChrome.PanelBrush,
            Padding = new Thickness(20, 18),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    titleText,
                    progressBar,
                    statusText,
                    errorText,
                },
            },
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };
        root.Add(body, 0, 0);
        root.Add(buttonBar, 1, 0);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        });
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing();
    }
}