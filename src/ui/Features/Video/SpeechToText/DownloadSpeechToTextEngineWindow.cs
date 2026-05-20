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
        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = Se.Language.Video.AudioToText.DownloadingSpeechToTextEngine,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };
        titleText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.TitleText)));

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CommandCancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(20),
            Children =
            {
                titleText,
                progressBar,
                statusText,
                buttonBar,
            }
        };

        Activated += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        };
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}