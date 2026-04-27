using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Features.Sync.VisualSync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public class OpenSecondarySubtitleWindow : Window
{
    public OpenSecondarySubtitleWindow(OpenSecondarySubtitleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.OpenSecondarySubtitleOnVideoPlayer;
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        vm.Window = this;
        DataContext = vm;

        var labelWidth = 100;

        // Color row
        var labelColor = UiUtil.MakeLabel(Se.Language.General.Color).WithMinWidth(labelWidth);
        var buttonColor = UiUtil.MakeButton(Se.Language.General.ChooseColorDotDotDot, vm.ChooseColorCommand).WithMinWidth(120);
        var colorPreview = new Border
        {
            Width = 30,
            Height = 20,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black),
        };
        colorPreview.Bind(Border.BackgroundProperty, new Binding(nameof(vm.SubtitleColor))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            Converter = new ColorToBrushConverter(),
        });
        var panelColor = UiUtil.MakeHorizontalPanel(labelColor, buttonColor, colorPreview);

        // Font size row
        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize).WithMinWidth(labelWidth);
        var numericFontSize = UiUtil.MakeNumericUpDownInt(6, 200, 46, 120, vm, nameof(vm.FontSize));
        var panelFontSize = UiUtil.MakeHorizontalPanel(labelFontSize, numericFontSize);

        // Border style row
        var labelBorderStyle = UiUtil.MakeLabel(Se.Language.General.BorderStyle).WithMinWidth(labelWidth);
        var comboBoxBorderStyle = UiUtil.MakeComboBox(vm.FontBoxTypes, vm, nameof(vm.SelectedFontBoxType)).WithMinWidth(160);
        var panelBorderStyle = UiUtil.MakeHorizontalPanel(labelBorderStyle, comboBoxBorderStyle);

        // Alignment row
        var labelAlignment = UiUtil.MakeLabel(Se.Language.General.Alignment).WithMinWidth(labelWidth);
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.FontAlignments, vm, nameof(vm.SelectedFontAlignment)).WithMinWidth(160);
        var panelAlignment = UiUtil.MakeHorizontalPanel(labelAlignment, comboBoxAlignment);

        // Left panel with settings
        var leftPanel = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                panelColor,
                panelFontSize,
                panelBorderStyle,
                panelAlignment,
            },
        };

        // Right panel with video player
        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;

        var comboBoxParagraphs = UiUtil.MakeComboBoxBindText(
            vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphIndex));
        comboBoxParagraphs.Width = double.NaN;
        comboBoxParagraphs.HorizontalAlignment = HorizontalAlignment.Stretch;
        vm.ComboBoxParagraphs = comboBoxParagraphs;
        comboBoxParagraphs.SelectionChanged += vm.ComboBoxParagraphsChanged;

        var buttonPlay = UiUtil.MakeButton(Se.Language.General.PlayCurrent, vm.PlayAndBackCommand)
            .WithLeftAlignment();

        var videoGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto,Auto"),
            RowSpacing = 8,
        };
        videoGrid.Add(vm.VideoPlayerControl, 0);
        videoGrid.Add(comboBoxParagraphs, 1);
        videoGrid.Add(buttonPlay, 2);

        // Main layout
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("260,*"),
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 12,
            RowSpacing = 10,
        };
        mainGrid.Add(leftPanel, 0, 0);
        mainGrid.Add(UiUtil.MakeBorderForControl(videoGrid), 0, 1);
        mainGrid.Add(buttonPanel, 1, 0, 1, 2);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        AddHandler(KeyDownEvent, (_, e) => vm.OnKeyDown(e),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, _) => vm.OnClosing();
    }
}

