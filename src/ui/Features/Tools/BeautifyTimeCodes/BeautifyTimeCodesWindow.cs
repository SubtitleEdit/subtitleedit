using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

/// <summary>
/// Beautify time codes, in the editor look (see <see cref="EditorChrome"/>): the exact time codes
/// toolbar on top, the original and beautified waveforms, the change navigator under them, Edit
/// profile and Cancel/OK in the button bar.
/// </summary>
public class BeautifyTimeCodesWindow : Window
{
    public BeautifyTimeCodesWindow(BeautifyTimeCodesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.BeautifyTimeCodes.Title);
        CanResize = true;
        Width = 1100;
        Height = 600;
        MinWidth = 900;
        MinHeight = 460;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        // The waveforms give up height first, so the change navigator and the buttons stay
        // reachable on a short screen.
        var sections = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
        };
        sections.Add(BuildTopBar(vm), 0, 0);
        sections.Add(BuildVisualizerArea(vm), 1, 0);
        sections.Add(BuildChangeNavBar(vm), 2, 0);

        Content = EditorChrome.MakeRoot(sections, BuildButtonBar(vm));

        // Change navigation keys (arrows, PageUp/Down, Home/End) are handled at window level, so
        // they work whichever control has focus - nothing here needs to own them.
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += (_, __) => vm.Dispose();
    }

    private static Control BuildTopBar(BeautifyTimeCodesViewModel vm)
    {
        var stats = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12,
            Margin = new Thickness(12, 0, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatsLine)) { Source = vm },
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
        };
        grid.Add(BuildTimeCodesBar(vm), 0, 0);
        grid.Add(stats, 0, 1);
        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(14, 8),
        };
    }

    /// <summary>
    /// Opt-in for snapping against the video's real frame times rather than an assumed n/fps grid,
    /// plus the extraction that produces them (a full decode, so it is explicit and cancellable).
    /// </summary>
    private static Control BuildTimeCodesBar(BeautifyTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.BeautifyTimeCodes;

        var toggleExact = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.UseExactTimeCodes))
            {
                Source = vm,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            },
        };
        AutomationProperties.SetName(toggleExact, l.UseExactTimeCodes);
        var labelExact = new TextBlock { Text = l.UseExactTimeCodes, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center };

        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(toggleExact, l.TimeCodesHint);
            ToolTip.SetTip(labelExact, l.TimeCodesHint);
        }

        var status = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12,
            Margin = new Thickness(6, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TimeCodesStatus)) { Source = vm },
        };

        var buttonExtract = EditorChrome.MakeTextToolButton("fa-solid fa-film", l.ExtractTimeCodes, vm.ExtractTimeCodesCommand);
        buttonExtract.VerticalAlignment = VerticalAlignment.Center;
        buttonExtract.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.CanExtractTimeCodes)) { Source = vm });

        var progress = UiUtil.MakeProgressBar(6);
        progress.Width = 140;
        progress.VerticalAlignment = VerticalAlignment.Center;
        progress.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ExtractProgressValue)) { Source = vm });
        progress.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsExtractingTimeCodes)) { Source = vm });

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { toggleExact, labelExact, status, buttonExtract, progress },
        };
    }

    private static Control BuildVisualizerArea(BeautifyTimeCodesViewModel vm)
    {
        var l = Se.Language.Tools.BeautifyTimeCodes;

        var (labelOriginal, borderOriginal, avOriginal) = BuildLabeledVisualizer(Se.Language.General.Original);
        var (labelBeautified, borderBeautified, avBeautified) = BuildLabeledVisualizer(l.Beautified);

        vm.AudioVisualizerOriginal = avOriginal;
        vm.AudioVisualizerBeautified = avBeautified;

        avOriginal.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == "StartPositionSeconds")
            {
                avBeautified.StartPositionSeconds = avOriginal.StartPositionSeconds;
            }
            else if (e.Property.Name == "ZoomFactor")
            {
                avBeautified.ZoomFactor = avOriginal.ZoomFactor;
            }
            else if (e.Property.Name == "VerticalZoomFactor")
            {
                avBeautified.VerticalZoomFactor = avOriginal.VerticalZoomFactor;
            }
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,3*,14,Auto,3*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = new Thickness(14, 12, 14, 14),
        };
        grid.Add(labelOriginal, 0, 0);
        grid.Add(borderOriginal, 1, 0);
        grid.Add(labelBeautified, 3, 0);
        grid.Add(borderBeautified, 4, 0);
        return grid;
    }

    private static (TextBlock label, Border border, Controls.AudioVisualizerControl.AudioVisualizer av) BuildLabeledVisualizer(string title)
    {
        var label = EditorChrome.MakeSectionHeader(title);
        label.Margin = new Thickness(0, 0, 0, 8);

        var av = new Controls.AudioVisualizerControl.AudioVisualizer
        {
            IsReadOnly = true,
            DrawGridLines = true,
            WaveformGridColor = Se.Settings.Waveform.WaveformGridColor.FromHexToColor(),
            // Stronger tints so paragraphs stand out on the dark waveform
            ParagraphBackground = Color.FromArgb(140, 70, 110, 180),       // regular: blue
            ParagraphSelectedBackground = Color.FromArgb(210, 230, 160, 40), // current change: amber
        };

        // The waveform itself stays dark in both themes; only its frame follows the editor palette.
        var border = new Border
        {
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
            Background = Brushes.Black,
            Child = av,
        };
        return (label, border, av);
    }

    private static Control BuildChangeNavBar(BeautifyTimeCodesViewModel vm)
    {
        // The same steps are on the keyboard (see BeautifyTimeCodesViewModel.OnKeyDown); the
        // tooltips are the only place that advertises it, so name the keys there.
        var buttonPrev = EditorChrome.CreateToolButton("fa-solid fa-chevron-up", $"{Se.Language.General.PreviousChange} (Up / PageUp)", vm.PreviousChangeCommand, 28);
        buttonPrev.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.CanGoPrevious)) { Source = vm });

        var buttonNext = EditorChrome.CreateToolButton("fa-solid fa-chevron-down", $"{Se.Language.General.NextChange} (Down / PageDown)", vm.NextChangeCommand, 28);
        buttonNext.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.CanGoNext)) { Source = vm });

        var labelPosition = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            FontSize = 12.5,
            MinWidth = 120,
            TextAlignment = TextAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ChangePositionLabel)) { Source = vm },
        };

        var detail = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 12.5,
            Margin = new Thickness(4, 4, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ChangeDetail)) { Source = vm },
        };

        var notes = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 12,
            FontStyle = FontStyle.Italic,
            Margin = new Thickness(4, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ChangeNotes)) { Source = vm },
        };

        var navRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonPrev, labelPosition, buttonNext },
        };

        var inner = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children = { navRow, detail, notes },
        };

        return new Border
        {
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(10, 8, 14, 10),
            Child = inner,
        };
    }

    private static Border BuildButtonBar(BeautifyTimeCodesViewModel vm)
    {
        var buttonEditProfile = EditorChrome.MakeTextToolButton("fa-solid fa-sliders", Se.Language.Tools.BeautifyTimeCodes.EditProfile, vm.EditProfileCommand);
        buttonEditProfile.HorizontalAlignment = HorizontalAlignment.Left;
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        return EditorChrome.MakeDockedButtonBar(buttonEditProfile, buttonCancel, buttonOk);
    }
}
