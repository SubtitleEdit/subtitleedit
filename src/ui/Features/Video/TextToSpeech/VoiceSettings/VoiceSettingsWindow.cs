using Avalonia;
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

public class VoiceSettingsWindow : Window
{
    private readonly VoiceSettingsViewModel _vm;
    private Border? _dropBorder;
    private Rectangle? _dropDashRect;
    private Icon? _dropIcon;

    public VoiceSettingsWindow(VoiceSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.VoiceSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.Video.TextToSpeech.VoiceSampleText,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var textBox = UiUtil.MakeTextBox(250, vm, nameof(vm.VoiceTestText));

        var dropArea = BuildDropArea(vm);

        var buttonImport = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.ImportVoiceDotDotDot, vm.ImportVoiceCommand).WithBindIsVisible(nameof(vm.IsImportVoiceVisible));
        var buttonRefresh = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.RefreshVoices, vm.RefreshVoiceListCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonImport, buttonRefresh, buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(label, 0, 0);
        grid.Add(textBox, 1, 0);
        grid.Add(dropArea, 2, 0);
        grid.Add(panelButtons, 3, 0);

        Content = grid;

        vm.PropertyChanged += OnViewModelPropertyChanged;
        Closed += (_, _) => vm.PropertyChanged -= OnViewModelPropertyChanged;
        ApplyDropVisualState(vm.IsDragOver);

        Activated += delegate { textBox.Focus(); }; // hack to make OnKeyDown work
    }

    private Control BuildDropArea(VoiceSettingsViewModel vm)
    {
        var icon = new Icon
        {
            Value = IconNames.Import,
            FontSize = 32,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = UiUtil.GetTextColor(0.55d),
        };
        _dropIcon = icon;

        var primaryText = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.DropAudioFileHereToImportVoice,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Foreground = UiUtil.GetTextColor(0.8d),
        };

        var hintText = new TextBlock
        {
            Text = Se.Language.Video.TextToSpeech.DropAudioFileHereHint,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            FontSize = 11,
            Foreground = UiUtil.GetTextColor(0.5d),
        };

        var content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Margin = new Thickness(24, 18),
            Children = { icon, primaryText, hintText },
        };

        var dashRect = new Rectangle
        {
            Stroke = UiUtil.GetTextColor(0.35d),
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
            Padding = new Thickness(16),
            MinHeight = 96,
            Child = inner,
        };
        _dropBorder = border;

        DragDrop.SetAllowDrop(border, true);
        border.AddHandler(DragDrop.DragOverEvent, vm.OnDragOver, RoutingStrategies.Bubble);
        border.AddHandler(DragDrop.DragLeaveEvent, vm.OnDragLeave, RoutingStrategies.Bubble);
        border.AddHandler(DragDrop.DropEvent, vm.OnDrop, RoutingStrategies.Bubble);

        border.WithBindIsVisible(nameof(vm.IsImportVoiceVisible));

        return border;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VoiceSettingsViewModel.IsDragOver))
        {
            ApplyDropVisualState(_vm.IsDragOver);
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
            var accent = new SolidColorBrush(Color.FromRgb(0x2E, 0x8B, 0xC0));
            _dropDashRect.Stroke = accent;
            _dropDashRect.StrokeThickness = 2.0;
            _dropIcon.Foreground = accent;
            _dropBorder.Background = new SolidColorBrush(Color.FromArgb(0x22, 0x2E, 0x8B, 0xC0));
        }
        else
        {
            _dropDashRect.Stroke = UiUtil.GetTextColor(0.35d);
            _dropDashRect.StrokeThickness = 1.5;
            _dropIcon.Foreground = UiUtil.GetTextColor(0.55d);
            _dropBorder.Background = Brushes.Transparent;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
