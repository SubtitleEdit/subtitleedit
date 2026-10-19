using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class DownloadSpeechToTextEngineWindow : Window
{
    public DownloadSpeechToTextEngineWindow(DownloadSpeechToTextEngineViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.DownloadingSpeechToTextEngine;
        Width = 500;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.SpeechToText);
        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = Se.Language.Video.AudioToText.DownloadingSpeechToTextEngine,
            FontSize = UiUtil.ScaledFontSize(16),
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
        };
        titleText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.TitleText)));

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Width = double.NaN;
        progressBar.Margin = new Thickness(0, 4, 0, 0);
        progressBar.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));

        var statusText = new TextBlock { FontSize = 12, Foreground = EditorChrome.DimTextBrush };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        // The view models set Error on every failure path, but nothing rendered it - the user saw
        // only the generic "Download failed" while the real cause was written to a property no
        // window bound. DownloadVideoFromUrlWindow and DownloadCrispEmbedWindow show it this way.
        var errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
            TextWrapping = TextWrapping.Wrap,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Error)));
        errorText.Bind(IsVisibleProperty, new Binding(nameof(vm.Error))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)),
        });

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CommandCancelCommand);
        var buttonBar = EditorChrome.MakeFlushButtonBar(null, buttonCancel);

        Content = new StackPanel
        {
            Children =
            {
                new StackPanel
                {
                    Spacing = 8,
                    Margin = new Thickness(20, 18, 20, 18),
                    Children =
                    {
                        titleText,
                        progressBar,
                        statusText,
                        errorText,
                    }
                },
                buttonBar,
            },
        };

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}