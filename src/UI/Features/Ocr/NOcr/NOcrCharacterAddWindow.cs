using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrCharacterAddWindow : Window
{
    public NOcrCharacterAddWindow(NOcrCharacterAddViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title)));
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Whole image for subtitle
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

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 0, 0, 10),
            MaxHeight = 350,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.SentenceBitmap)));

        var controlsView = MakeControlsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonInspectAdditions = UiUtil.MakeButton(Se.Language.General.InspectAdditions, vm.InspectAdditionsCommand).WithBindIsVisible(nameof(vm.IsInspectAdditionsVisible));
        var buttonUseOnce = UiUtil.MakeButton(Se.Language.General.UseOnce, vm.UseOnceCommand).WithBindIsVisible(nameof(vm.ShowUseOnce));
        var buttonSkip = UiUtil.MakeButton(Se.Language.General.Skip, vm.SkipCommand).WithBindIsVisible(nameof(vm.ShowSkip));
        var buttonAbort = UiUtil.MakeButton(Se.Language.General.Abort, vm.AbortCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonInspectAdditions, buttonUseOnce, buttonSkip, buttonAbort);

        grid.Add(image, 0, 0);
        grid.Add(controlsView, 1, 0);
        grid.Add(buttonBar, 2, 0);

        Content = grid;

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;
        vm.TextBoxNew.KeyUp += vm.TextBoxNewOnKeyUp;
        vm.TextBoxNew.PointerReleased += vm.TextBoxMacPointerReleased;
        var menuFlyout = new MenuFlyout();
        CharactersFlyoutMenuHelper.MakeFlyoutLetters(menuFlyout, vm.InsertSpecialCharacterCommand);
        vm.TextBoxNew.ContextFlyout = menuFlyout;

        Activated += delegate
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        };
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
        PointerWheelChanged += vm.PointerWheelChanged;
        KeyDown += (sender, args) => vm.KeyDown(args);
        KeyUp += (sender, args) => vm.KeyUp(args);
    }

    private static Grid MakeControlsView(NOcrCharacterAddViewModel vm)
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
        };

        vm.TextBoxNew = UiUtil.MakeTextBox(100, vm, nameof(vm.NewText));
        var image = new Image
        {
            Margin = new Thickness(5),
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));


        var panelCurrentImage = new StackPanel
        {
            Background = new SolidColorBrush(Colors.LightGray),
            Children = { image },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(5, 2, 0, 5),
        };

        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsNewTextItalic));
        checkBoxItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var checkBoAutoSubmitFirsChar = UiUtil.MakeCheckBox(Se.Language.Ocr.AutoSubmitFirstCharacter, vm, nameof(vm.SubmitOnFirstLetter));

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.CurrentImage).WithBold(),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                panelCurrentImage,
                vm.TextBoxNew,
                checkBoxItalic,
                checkBoAutoSubmitFirsChar,
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

        var comboBoxLinesToAutoDraw = UiUtil.MakeComboBox(vm.NoOfLinesToAutoDrawList, vm, nameof(vm.SelectedNoOfLinesToAutoDraw));
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

        var panelDrawControls = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.LinesToDraw).WithBold(),
                panelLinesToDraw,
                UiUtil.MakeButton(Se.Language.Ocr.AutoDrawAgain, vm.DrawAgainCommand).WithMinWidth(100).WithMarginTop(10).WithLeftAlignment().WithMarginLeft(0),
                buttonClear.WithMinWidth(100).WithMarginTop(5).WithLeftAlignment().WithMarginLeft(0),
            }
        };

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
                UiUtil.MakeButton(vm.ZoomOutCommand, IconNames.Minus),
                UiUtil.MakeButton(vm.ZoomInCommand, IconNames.Plus),
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
        };

        var buttonShrink = UiUtil.MakeButton(Se.Language.General.Shrink, vm.ShrinkCommand)
            .WithMinWidth(100).WithBindEnabled(nameof(vm.CanShrink));
        var buttonExpand = UiUtil.MakeButton(Se.Language.General.Expand, vm.ExpandCommand)
            .WithMinWidth(100).WithBindEnabled(nameof(vm.CanExpand));
        var panelButtons = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Width = double.NaN,
            Margin = new Thickness(0, 0, 0, 5),
            Children =
            {
                buttonShrink,
                buttonExpand,
            }
        };

        grid.Add(panelCurrent, 0, 0);
        grid.Add(panelDrawControls, 0, 1);
        grid.Add(panelImage, 0, 2);
        grid.Add(panelButtons, 0, 2, 1, 2);

        return grid;
    }
}
