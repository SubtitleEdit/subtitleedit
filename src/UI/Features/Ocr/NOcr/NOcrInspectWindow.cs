using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrInspectWindow : Window
{
    private readonly NOcrInspectViewModel _vm;

    public NOcrInspectWindow(NOcrInspectViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.NOcrInspectImageMatches;
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Lines
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Controls
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var linesView = MakeLinesView(vm);
        var controlsView = MakeControlsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk);

        grid.Add(linesView, 0, 0);
        grid.Add(controlsView, 1, 0);
        grid.Add(buttonBar, 2, 0);

        Content = grid;

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;

        Activated += delegate
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        };

        PointerWheelChanged += vm.PointerWheelChanged;
    }

    private static Border MakeLinesView(NOcrInspectViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            Height = double.NaN,
        };

        vm.PanelLines = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Margin = new Thickness(5),
        };

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.SentenceBitmap)),
            Stretch = Stretch.Uniform,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            MaxWidth = 300,
            MaxHeight = 200,
        };

        grid.Add(vm.PanelLines, 0);
        grid.Add(image, 0, 1, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(10);
    }

    private static Border MakeControlsView(NOcrInspectViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
        };

        vm.TextBoxNew = UiUtil.MakeTextBox(100, vm, nameof(vm.NewText))
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));

        var image = new Image
        {
            Margin = new Thickness(5),
            [!Image.SourceProperty] = new Binding(nameof(vm.CurrentBitmap)),
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        var panelCurrentImage = new StackPanel
        {
            Background = new SolidColorBrush(Colors.LightGray),
            Children = { image },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 5),
        };

        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsNewTextItalic))
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));
        checkBoxItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.CurrentImage).WithBold(),
                panelCurrentImage,
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                UiUtil.MakeLabel(Se.Language.General.Match).WithBold().WithMarginTop(15),
                vm.TextBoxNew,
                checkBoxItalic,
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.MatchResolutionAndTopMargin)),
                UiUtil.MakeButton(Se.Language.General.Update, vm.UpdateCommand).WithMarginTop(25).WithLeftAlignment().WithBindEnabled(nameof(vm.IsEditControlsEnabled)),
                UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand).WithMarginTop(5).WithLeftAlignment().WithBindEnabled(nameof(vm.IsEditControlsEnabled)),
                UiUtil.MakeButton(Se.Language.Ocr.AddBetterMatch, vm.AddBetterMatchCommand).WithMarginTop(5).WithLeftAlignment(),
            },
        };

        var toggleButtonForeground = new ToggleButton
        {
            Content = Se.Language.General.Foreground,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsNewLinesForegroundActive))
            {
                Source = vm,
            },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0),
        };
        toggleButtonForeground.IsCheckedChanged += vm.DrawModeForegroundChanged;
        var toggleButtonBackground = new ToggleButton
        {
            Content = Se.Language.General.Background,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.IsNewLinesBackgroundActive))
            {
                Source = vm,
            },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        toggleButtonBackground.IsCheckedChanged += vm.DrawModeBackgroundChanged;

        var panelDrawMode = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children =
            {
                toggleButtonForeground,
                toggleButtonBackground,
            }
        };

        var buttonClear = new SplitButton
        {
            Content = Se.Language.General.Clear,
            Command = vm.ClearDrawCommand,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Ocr.ClearForeground,
                        Command = vm.ClearDrawForeGroundCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Ocr.ClearBackground,
                        Command = vm.ClearDrawBackgroundCommand,
                    },
                }
            }
        };

        var comboBoxLinesToAutoDraw = UiUtil.MakeComboBox(vm.NoOfLinesToAutoDrawList, vm, nameof(vm.SelectedNoOfLinesToAutoDraw))
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));

        var iconInfo = new Projektanker.Icons.Avalonia.Icon
        {
            Value = IconNames.Information,
            Margin = new Thickness(5, 0, 0, 0),
        };
        iconInfo.PointerPressed += (sender, args) =>
        {
            _ = vm.ShowDrawingTips();
        };
        ToolTip.SetTip(iconInfo, Se.Language.Ocr.NOcrDrawHelp);
        var panelLinesToDraw = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Children =
            {
                comboBoxLinesToAutoDraw,
                iconInfo,
            }
        };

        var panelDrawControls = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.LinesToDraw).WithBold(),
                panelLinesToDraw,
                UiUtil.MakeButton(Se.Language.Ocr.AutoDrawAgain, vm.DrawAgainCommand)
                    .WithMarginLeft(0)
                    .WithMinWidth(100)
                    .WithMarginTop(10)
                    .WithLeftAlignment()
                    .WithBindEnabled(nameof(vm.IsEditControlsEnabled)),
                buttonClear
                    .WithMinWidth(100)
                    .WithMarginTop(5)
                    .WithLeftAlignment()
                    .WithBindEnabled(nameof(vm.IsEditControlsEnabled)),
            }
        }.WithBindVisible(vm, nameof(vm.IsEditControlsEnabled));

        vm.NOcrDrawingCanvas.SetStrokeWidth(1);
        var borderDrawingCanvas = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black),
            Child = vm.NOcrDrawingCanvas,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };        

        var panelZoom = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5),
            Children =
            {
                UiUtil.MakeButton(vm.ZoomOutCommand, IconNames.Minus).WithFontSize(20).WithBindEnabled(nameof(vm.IsEditControlsEnabled)),
                UiUtil.MakeButton(vm.ZoomInCommand, IconNames.Plus).WithFontSize(20).WithBindEnabled(nameof(vm.IsEditControlsEnabled)),
                UiUtil.MakeLabel(string.Empty).WithMarginLeft(10).WithBindText(vm, nameof(vm.ZoomFactorInfo)),
                UiUtil.MakeLabel(Se.Language.Ocr.DrawMode).WithMarginLeft(10),
                panelDrawMode,
            }
        };

        var panelImage = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                panelZoom,
                borderDrawingCanvas,
            }
        }.WithBindVisible(vm, nameof(vm.IsEditControlsEnabled));

        grid.Add(panelCurrent, 0, 0);
        grid.Add(panelDrawControls, 0, 1);
        grid.Add(panelImage, 0, 2);

        return UiUtil.MakeBorderForControl(grid);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.KeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        _vm.KeyUp(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Title = _vm.Title;
        _vm.OnLoaded();
    }
}
