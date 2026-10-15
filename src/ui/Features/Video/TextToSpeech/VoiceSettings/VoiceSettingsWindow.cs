using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;
using System.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

/// <summary>
/// Voice sample text and voice import, in the editor look (see <see cref="EditorChrome"/>): the
/// sample text section, a drop area for importing a voice below it, import/refresh on the left of
/// the button bar and Cancel/OK on the right.
/// </summary>
public class VoiceSettingsWindow : Window
{
    private readonly VoiceSettingsViewModel _vm;
    private Border? _dropBorder;
    private Rectangle? _dropDashRect;
    private Icon? _dropIcon;
    private RowDefinition? _dropRow;

    public VoiceSettingsWindow(VoiceSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.VoiceSettings;
        Width = 520;
        Height = 420;
        MinWidth = 400;
        MinHeight = 320;
        CanResize = true;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var textBox = UiUtil.MakeTextBox(250, vm, nameof(vm.VoiceTestText));
        textBox.Width = double.NaN;
        textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBox.FontSize = 12.5;
        textBox.Background = EditorChrome.FieldBrush;
        textBox.BorderBrush = EditorChrome.FieldBorderBrush;
        textBox.CornerRadius = new CornerRadius(6);
        textBox.Padding = new Thickness(8, 6);
        AutomationProperties.SetName(textBox, Se.Language.Video.TextToSpeech.VoiceSampleText);

        // Multi-line and growing with the window, so a longer sample text is readable (#14726).
        // Line breaks are fine - the sample is unbroken before it is spoken.
        textBox.AcceptsReturn = true;
        textBox.TextWrapping = TextWrapping.Wrap;
        textBox.VerticalAlignment = VerticalAlignment.Stretch;
        textBox.VerticalContentAlignment = VerticalAlignment.Top;
        textBox.MinHeight = 60;

        // The sample text section: header on top, the text box takes the rest of the height
        var sampleHeader = EditorChrome.MakeSectionHeader(Se.Language.Video.TextToSpeech.VoiceSampleText);
        sampleHeader.Margin = new Thickness(0, 0, 0, 10);
        DockPanel.SetDock(sampleHeader, Dock.Top);
        var sampleSection = new Border
        {
            Padding = new Thickness(14, 12, 14, 14),
            Child = new DockPanel { Children = { sampleHeader, textBox } },
        };

        var dropArea = BuildDropArea(vm);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        _dropRow = grid.RowDefinitions[1];

        // The panel color behind both sections, whichever of them is showing
        var panel = new Border { Background = EditorChrome.PanelBrush };
        grid.Add(panel, 0, 0);
        Grid.SetRowSpan(panel, 2);
        grid.Add(sampleSection, 0, 0);
        grid.Add(dropArea, 1, 0);
        grid.Add(MakeButtonBar(vm), 2, 0);

        Content = grid;

        vm.PropertyChanged += OnViewModelPropertyChanged;
        Closed += (_, _) => vm.PropertyChanged -= OnViewModelPropertyChanged;
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
        ApplyDropVisualState(vm.IsDragOver);
        ApplyDropRowHeight(vm.IsImportVoiceVisible);

        UiUtil.FocusOnFirstActivation(this, textBox); // hack to make OnKeyDown work
    }

    private Control BuildDropArea(VoiceSettingsViewModel vm)
    {
        var icon = new Icon
        {
            Value = IconNames.Import,
            FontSize = 30,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = EditorChrome.FaintTextBrush,
        };
        _dropIcon = icon;

        var primaryText = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.DropAudioFileHereToImportVoice,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12.5,
        };

        var hintText = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.DropAudioFileHereHint,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 11.5,
            Foreground = EditorChrome.DimTextBrush,
        };

        var content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 6,
            Margin = new Thickness(24, 14),
            Children = { icon, primaryText, hintText },
        };

        var dashRect = new Rectangle
        {
            Stroke = EditorChrome.SeparatorBrush,
            StrokeThickness = 1.5,
            StrokeDashArray = new AvaloniaList<double> { 4, 3 },
            RadiusX = 8,
            RadiusY = 8,
            IsHitTestVisible = false,
        };
        _dropDashRect = dashRect;

        var inner = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children = { dashRect, content },
        };

        var border = new Border
        {
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(8),
            MinHeight = 96,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = inner,
        };
        _dropBorder = border;

        // The whole section takes drops, not only the dashed area inside its padding
        var section = new Border
        {
            Background = Brushes.Transparent,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14),
            Child = border,
        };

        DragDrop.SetAllowDrop(section, true);
        section.AddHandler(DragDrop.DragOverEvent, vm.OnDragOver, RoutingStrategies.Bubble);
        section.AddHandler(DragDrop.DragLeaveEvent, vm.OnDragLeave, RoutingStrategies.Bubble);
        section.AddHandler(DragDrop.DropEvent, vm.OnDrop, RoutingStrategies.Bubble);

        section.WithBindIsVisible(nameof(vm.IsImportVoiceVisible));

        return section;
    }

    private static Border MakeButtonBar(VoiceSettingsViewModel vm)
    {
        var buttonImport = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.ImportVoiceDotDotDot, vm.ImportVoiceCommand)
            .WithIconLeft(IconNames.Import)
            .WithBindIsVisible(nameof(vm.IsImportVoiceVisible));
        var buttonRefresh = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.RefreshVoices, vm.RefreshVoiceListCommand)
            .WithIconLeft(IconNames.Refresh);
        var left = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { buttonImport, buttonRefresh } };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var right = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, Children = { buttonCancel, buttonOk } };

        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        grid.Add(left, 0, 0);
        grid.Add(right, 0, 1);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.ButtonBarBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 10),
        };
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VoiceSettingsViewModel.IsDragOver))
        {
            ApplyDropVisualState(_vm.IsDragOver);
        }
        else if (e.PropertyName == nameof(VoiceSettingsViewModel.IsImportVoiceVisible))
        {
            ApplyDropRowHeight(_vm.IsImportVoiceVisible);
        }
    }

    // A star row keeps its share of the height even when its content is hidden, so without a
    // drop area the row collapses and the sample text box gets all the extra height.
    private void ApplyDropRowHeight(bool isDropAreaVisible)
    {
        if (_dropRow != null)
        {
            _dropRow.Height = isDropAreaVisible ? new GridLength(1, GridUnitType.Star) : GridLength.Auto;
        }
    }

    private void ApplyDropVisualState(bool isDragOver)
    {
        if (_dropBorder == null || _dropDashRect == null || _dropIcon == null)
        {
            return;
        }

        if (isDragOver)
        {
            _dropDashRect.Stroke = EditorChrome.AccentBrush;
            _dropDashRect.StrokeThickness = 2.0;
            _dropIcon.Foreground = EditorChrome.AccentBrush;
            _dropBorder.Background = EditorChrome.AccentSoftBrush;
        }
        else
        {
            _dropDashRect.Stroke = EditorChrome.SeparatorBrush;
            _dropDashRect.StrokeThickness = 1.5;
            _dropIcon.Foreground = EditorChrome.FaintTextBrush;
            _dropBorder.Background = Brushes.Transparent;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
