using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;

/// <summary>
/// The audio encoding of the TTS video, in the editor look (see <see cref="EditorChrome"/>): one
/// section with the encoding and a stereo switch, Cancel/OK in the button bar.
/// </summary>
public class EncodingSettingsWindow : Window
{
    private readonly EncodingSettingsViewModel _vm;

    public EncodingSettingsWindow(EncodingSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.VideoEncodingSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 340;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var comboEncoding = new ComboBox
        {
            ItemsSource = vm.Encodings,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 150,
        }.WithBindSelected(nameof(vm.SelectedEncoding));
        AutomationProperties.SetName(comboEncoding, Se.Language.General.Encoding);

        var switchStereo = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            IsChecked = vm.IsStereo,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleSwitch.IsCheckedProperty] = new Binding(nameof(vm.IsStereo)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };
        AutomationProperties.SetName(switchStereo, Se.Language.General.Stereo);
        var stereoRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                switchStereo,
                new TextBlock { Text = Se.Language.General.Stereo, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center },
            },
            // Not every encoding can do stereo - the row greys out with its switch
            [!IsEnabledProperty] = new Binding(nameof(vm.SelectedEncoding) + "." + nameof(EncodingDisplayItem.IsStereoEnabled)) { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var rows = new StackPanel { Spacing = 10 };
        rows.Children.Add(comboEncoding);
        rows.Children.Add(stereoRow);

        var sections = new StackPanel { Background = EditorChrome.PanelBrush };
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Encoding, rows, first: true));

        var root = new DockPanel();
        var buttonBar = MakeButtonBar(vm);
        DockPanel.SetDock(buttonBar, Dock.Bottom);
        root.Children.Add(buttonBar);
        root.Children.Add(sections);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, comboEncoding); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    /// <summary>
    /// Compact combo boxes.
    /// </summary>
    private void AddStyles()
    {
        Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });
    }

    private static Border MakeButtonBar(EncodingSettingsViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return new Border
        {
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Children = { buttonCancel, buttonOk },
            },
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
