using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;

public sealed class DownloadVoicePacksWindow : Window
{
    private readonly DownloadVoicePacksViewModel _vm;

    public DownloadVoicePacksWindow(DownloadVoicePacksViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.DownloadVoicePacksTitle;
        Width = 620;
        Height = 560;
        MinWidth = 480;
        MinHeight = 400;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;
        _vm = vm;

        var intro = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.VoicePackIntro,
            TextWrapping = TextWrapping.Wrap,
            Foreground = UiUtil.GetTextColor(0.8d),
        };

        var listBox = new ListBox
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Packs)),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ItemTemplate = new FuncDataTemplate<VoicePackItem>((_, _) =>
            {
                var checkBox = new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(VoicePackItem.IsSelected)) { Mode = BindingMode.TwoWay },
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 2, 6, 0),
                };

                var name = new TextBlock { FontWeight = FontWeight.SemiBold };
                name.Bind(TextBlock.TextProperty, new Binding(nameof(VoicePackItem.Name)));

                var description = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = UiUtil.GetTextColor(0.8d) };
                description.Bind(TextBlock.TextProperty, new Binding(nameof(VoicePackItem.Description)));

                var details = new TextBlock { FontSize = UiUtil.ScaledFontSize(11), Foreground = UiUtil.GetTextColor(0.55d) };
                details.Bind(TextBlock.TextProperty, new Binding(nameof(VoicePackItem.Details)));

                var status = new TextBlock { FontSize = UiUtil.ScaledFontSize(11), Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57)) };
                status.Bind(TextBlock.TextProperty, new Binding(nameof(VoicePackItem.Status)));
                status.Bind(IsVisibleProperty, new Binding(nameof(VoicePackItem.Status)) { Converter = new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)) });

                var text = new StackPanel { Spacing = 2, Children = { name, description, details, status } };
                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(2, 4),
                    Children = { checkBox, text },
                };
            }, true),
        };
        listBox.Bind(IsEnabledProperty, new Binding(nameof(vm.IsIdle)));

        var labelTarget = new Label { Content = Se.Language.Video.TextToSpeech.InstallToEngine, VerticalAlignment = VerticalAlignment.Center };
        var comboTarget = UiUtil.MakeComboBox(vm.TargetEngines, vm, nameof(vm.SelectedTargetEngine)).WithBindEnabled(nameof(vm.IsIdle));
        comboTarget.MinWidth = 260;
        var panelTarget = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { labelTarget, comboTarget } };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(IsVisibleProperty, new Binding(nameof(vm.IsDownloading)));

        var progressText = new TextBlock();
        progressText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        var errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
            TextWrapping = TextWrapping.Wrap,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Error)));
        errorText.Bind(IsVisibleProperty, new Binding(nameof(vm.Error)) { Converter = new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)) });

        var buttonDownload = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.DownloadAndInstall, vm.DownloadAndInstallCommand)
            .WithIconLeft(IconNames.CloudDownload)
            .WithBindEnabled(nameof(vm.IsIdle));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonDownload, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
        };
        grid.Add(intro, 0);
        grid.Add(listBox, 1);
        grid.Add(panelTarget, 2);
        grid.Add(progressBar, 3);
        grid.Add(progressText, 4);
        grid.Add(errorText, 5);
        grid.Add(buttonBar, 6);
        Content = grid;

        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
        UiUtil.FocusOnFirstActivation(this, listBox);
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
