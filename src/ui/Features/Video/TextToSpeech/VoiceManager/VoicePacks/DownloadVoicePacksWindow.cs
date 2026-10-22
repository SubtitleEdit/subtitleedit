using Avalonia;
using Avalonia.Automation;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsUncentered | EditorWindowStyles.MuteDisabledPrimary);

        var intro = EditorChrome.MakeHint(Se.Language.Video.TextToSpeech.VoicePackIntro);
        intro.FontSize = EditorChrome.FieldFontSize;

        var listBox = new ListBox
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Packs)),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = Brushes.Transparent,
            ItemTemplate = new FuncDataTemplate<VoicePackItem>((_, _) =>
            {
                var checkBox = new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(VoicePackItem.IsSelected)) { Mode = BindingMode.TwoWay },
                    // The pack name sits in a separate text block; without this the box is a bare "check box" (#12087).
                    [!AutomationProperties.NameProperty] = new Binding(nameof(VoicePackItem.Name)),
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 2, 6, 0),
                };

                var name = new TextBlock { FontWeight = FontWeight.SemiBold };
                name.Bind(TextBlock.TextProperty, new Binding(nameof(VoicePackItem.Name)));

                var description = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = EditorChrome.FieldFontSize, Foreground = EditorChrome.DimTextBrush };
                description.Bind(TextBlock.TextProperty, new Binding(nameof(VoicePackItem.Description)));

                var details = new TextBlock { FontSize = EditorChrome.ScaledFont(11), Foreground = EditorChrome.FaintTextBrush };
                details.Bind(TextBlock.TextProperty, new Binding(nameof(VoicePackItem.Details)));

                var status = new TextBlock { FontSize = UiUtil.ScaledFontSize(11), Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0x57)) };
                status.Bind(TextBlock.TextProperty, new Binding(nameof(VoicePackItem.Status)));
                status.Bind(IsVisibleProperty, new Binding(nameof(VoicePackItem.Status)) { Converter = new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)) });

                var text = new StackPanel { Spacing = 2, Children = { name, description, details, status } };
                // A grid rather than a horizontal stack panel, so the description wraps at the
                // list's width instead of running off its right edge.
                var row = new Grid
                {
                    Margin = new Thickness(2, 6),
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
                };
                row.Add(checkBox, 0, 0);
                row.Add(text, 0, 1);
                return row;
            }, true),
        };
        listBox.Bind(IsEnabledProperty, new Binding(nameof(vm.IsIdle)));
        ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);
        AutomationProperties.SetName(listBox, Se.Language.Video.TextToSpeech.DownloadVoicePacksTitle);

        var labelTarget = EditorChrome.MakeDimLabel(Se.Language.Video.TextToSpeech.InstallToEngine, new Thickness(0));
        var comboTarget = UiUtil.MakeComboBox(vm.TargetEngines, vm, nameof(vm.SelectedTargetEngine)).WithBindEnabled(nameof(vm.IsIdle));
        comboTarget.MinWidth = 260;
        comboTarget.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboTarget, Se.Language.Video.TextToSpeech.InstallToEngine);
        var panelTarget = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Children = { labelTarget, comboTarget } };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var progressStrip = EditorChrome.MakeProgressStrip(progressBar);
        progressStrip.Bind(IsVisibleProperty, new Binding(nameof(vm.IsDownloading)));

        // In the button bar rather than the strip: after the run it says how many packs were
        // installed, when the strip is gone.
        var progressText = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        progressText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        var errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
            TextWrapping = TextWrapping.Wrap,
            FontSize = EditorChrome.FieldFontSize,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Error)));
        errorText.Bind(IsVisibleProperty, new Binding(nameof(vm.Error)) { Converter = new FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)) });

        var buttonDownload = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.DownloadAndInstall, vm.DownloadAndInstallCommand)
            .WithIconLeft(IconNames.CloudDownload)
            .WithBindEnabled(nameof(vm.IsIdle));
        buttonDownload.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = EditorChrome.MakeFlushButtonBar(progressText, buttonCancel, buttonDownload);

        var packsHeader = EditorChrome.MakeSectionHeader(Se.Language.Video.Voices);
        var content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 10,
            Margin = new Thickness(14, 12, 14, 14),
        };
        content.Add(intro, 0);
        content.Add(packsHeader, 1);
        content.Add(EditorChrome.MakeListFrame(listBox), 2);
        content.Add(panelTarget, 3);
        content.Add(errorText, 4);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        root.Add(new Border { Background = EditorChrome.PanelBrush, Child = content }, 0);
        root.Add(progressStrip, 1);
        root.Add(buttonBar, 2);
        Content = root;

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
