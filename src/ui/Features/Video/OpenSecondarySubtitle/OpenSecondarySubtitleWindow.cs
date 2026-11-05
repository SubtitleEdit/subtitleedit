using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
        Title = vm.IsEditingSettings
            ? Se.Language.Video.EditSecondarySubtitleSettings
            : Se.Language.Video.OpenSecondarySubtitleOnVideoPlayer;
        CanResize = true;
        Width = 1100;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        vm.Window = this;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);
        const double labelWidth = 96;

        // Color row
        var buttonColor = UiUtil.MakeButton(Se.Language.General.ChooseColorDotDotDot, vm.ChooseColorCommand);
        var colorPreview = new Border
        {
            Width = 30,
            Height = 22,
            CornerRadius = new CornerRadius(4),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        colorPreview.Bind(Border.BackgroundProperty, new Binding(nameof(vm.SubtitleColor))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            Converter = new ColorToBrushConverter(),
        });
        var panelColor = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { colorPreview, buttonColor } };

        // Font name row
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName));
        comboBoxFontName.Width = double.NaN;
        comboBoxFontName.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBoxFontName, Se.Language.General.FontName);
        var buttonPickFont = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.Tools.PickFontNameTitle, vm.PickFontCommand, 28);
        var panelFontName = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        panelFontName.Add(comboBoxFontName, 0, 0);
        panelFontName.Add(buttonPickFont, 0, 1);

        // Font size row
        var numericFontSize = UiUtil.MakeNumericUpDownInt(6, 200, 46, 90, vm, nameof(vm.FontSize));
        numericFontSize.Classes.Add("field");
        AutomationProperties.SetName(numericFontSize, Se.Language.General.FontSize);

        var comboBoxBorderStyle = UiUtil.MakeComboBox(vm.FontBoxTypes, vm, nameof(vm.SelectedFontBoxType));
        comboBoxBorderStyle.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBoxBorderStyle, Se.Language.General.BorderStyle);
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.FontAlignments, vm, nameof(vm.SelectedFontAlignment));
        comboBoxAlignment.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBoxAlignment, Se.Language.General.Alignment);
        var comboBoxJustify = UiUtil.MakeComboBox(vm.JustifyItems, vm, nameof(vm.SelectedJustify));
        comboBoxJustify.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBoxJustify, Se.Language.Video.SecondarySubtitleJustifyLines);

        var styleRows = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Color, panelColor, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.FontName, panelFontName, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.FontSize, numericFontSize, labelWidth),
                EditorChrome.MakeSwitchRow(nameof(vm.FontBold), Se.Language.General.Bold),
                EditorChrome.MakeLabeledRow(Se.Language.General.BorderStyle, comboBoxBorderStyle, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.Alignment, comboBoxAlignment, labelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.Video.SecondarySubtitleJustifyLines, comboBoxJustify, labelWidth),
            },
        };

        var rowOverrideStyle = EditorChrome.MakeSwitchRow(nameof(vm.OverrideStyle), Se.Language.Video.SecondarySubtitleRememberSettings);
        var rowDoNotShowAgain = EditorChrome.MakeSwitchRow(nameof(vm.DoNotShowAgain), Se.Language.Video.SecondarySubtitleDoNotShowAgain);
        // Skipping the dialog applies the remembered settings, so there must be some.
        rowDoNotShowAgain.Bind(IsEnabledProperty, new Binding(nameof(vm.OverrideStyle)) { Source = vm });
        // "Do not show again" is about opening a file; editing the current second subtitle
        // always shows this dialog, so the option has no meaning there (#15110).
        rowDoNotShowAgain.IsVisible = !vm.IsEditingSettings;

        var optionRows = new StackPanel { Spacing = 8, Children = { rowOverrideStyle, rowDoNotShowAgain } };

        // Left panel with settings
        var leftPanel = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Style, styleRows, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Options, optionRows),
            },
        };
        var side = new Border
        {
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = new ScrollViewer
            {
                Content = leftPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            },
        };

        // Right panel with video player
        vm.VideoPlayerControl = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControl.FullScreenIsVisible = false;

        var comboBoxParagraphs = UiUtil.MakeComboBoxBindText(
            vm.Paragraphs, vm, nameof(SubtitleDisplayItem.Text), nameof(vm.SelectedParagraphIndex));
        comboBoxParagraphs.Width = double.NaN;
        comboBoxParagraphs.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBoxParagraphs.WithAccessibleName(Se.Language.General.Lines); // no visible label (#12087)
        vm.ComboBoxParagraphs = comboBoxParagraphs;
        comboBoxParagraphs.SelectionChanged += vm.ComboBoxParagraphsChanged;

        var buttonPlay = EditorChrome.MakeActionButton("fa-solid fa-play", Se.Language.General.PlayCurrent, vm.PlayAndBackCommand);

        var lineRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        lineRow.Add(comboBoxParagraphs, 0, 0);
        lineRow.Add(buttonPlay, 0, 1);

        var videoGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            RowSpacing = 10,
            Margin = new Thickness(14),
        };
        // A plain border around the player: rounded corners or clipping make the native video
        // surface disappear on macOS.
        videoGrid.Add(new Border
        {
            Child = vm.VideoPlayerControl,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1),
        }, 0);
        videoGrid.Add(lineRow, 1);

        // Main layout
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).AsPrimary();
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("320,*"),
        };
        content.Add(side, 0, 0);
        content.Add(videoGrid, 0, 1);

        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
        };
        mainGrid.Add(content, 0, 0);
        mainGrid.Add(EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk), 1, 0);

        Content = mainGrid;

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, numericFontSize);
        AddHandler(KeyDownEvent, (_, e) => vm.OnKeyDown(e),
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);
        Loaded += (_, _) => vm.OnLoaded();
        Closing += (_, _) => vm.OnClosing();
    }
}
