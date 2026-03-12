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

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public class OpenSecondarySubtitleWindow : Window
{
    public OpenSecondarySubtitleWindow(OpenSecondarySubtitleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Secondary Subtitle Appearance";
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        vm.Window = this;
        DataContext = vm;

        var labelWidth = 100;

        // Color row
        var labelColor = UiUtil.MakeLabel("Color").WithMinWidth(labelWidth);
        var buttonColor = UiUtil.MakeButton("Choose color...", vm.ChooseColorCommand).WithMinWidth(120);
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
        var labelFontSize = UiUtil.MakeLabel("Font size").WithMinWidth(labelWidth);
        var numericFontSize = UiUtil.MakeNumericUpDownInt(6, 200, 20, 120, vm, nameof(vm.FontSize));
        var panelFontSize = UiUtil.MakeHorizontalPanel(labelFontSize, numericFontSize);

        // Border style row
        var labelBorderStyle = UiUtil.MakeLabel("Border style").WithMinWidth(labelWidth);
        var comboBoxBorderStyle = UiUtil.MakeComboBox(vm.FontBoxTypes, vm, nameof(vm.SelectedFontBoxType)).WithMinWidth(160);
        comboBoxBorderStyle.SelectionChanged += vm.BoxTypeChanged;
        var panelBorderStyle = UiUtil.MakeHorizontalPanel(labelBorderStyle, comboBoxBorderStyle);

        // Alignment grid (an7 an8 an9 / an4 an5 an6 / an1 an2 an3)
        var alignmentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto"),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto"),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        alignmentGrid.Add(UiUtil.MakeLabel("Position"), 0, 0, 1, 3);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn7), "align"), 1, 0);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn8), "align"), 1, 1);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn9), "align"), 1, 2);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn4), "align"), 2, 0);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn5), "align"), 2, 1);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn6), "align"), 2, 2);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn1), "align"), 3, 0);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn2), "align"), 3, 1);
        alignmentGrid.Add(UiUtil.MakeRadioButton(string.Empty, vm, nameof(vm.AlignmentAn3), "align"), 3, 2);
        var alignmentBorder = UiUtil.MakeBorderForControl(alignmentGrid);

        // Left panel with settings
        var leftPanel = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                panelColor,
                panelFontSize,
                panelBorderStyle,
                alignmentBorder,
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

        var buttonPlay = UiUtil.MakeButton("Play current", vm.PlayAndBackCommand)
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

