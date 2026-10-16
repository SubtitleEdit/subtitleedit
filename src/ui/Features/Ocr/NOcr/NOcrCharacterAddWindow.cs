using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// Add an nOCR character in the editor look (see <see cref="EditorChrome"/>): the whole subtitle
/// image on top, then the current image and its text, the auto-draw settings and the drawing
/// canvas as columns; the actions in the button bar with OK as the accent.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.TextToolToggles | EditorWindowStyles.CompactSplitButtons);
        DataContext = vm;

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            MaxHeight = 350,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.SentenceBitmap)));

        // subtitle bitmaps are light or dark text on transparent pixels, so without a backdrop they
        // vanish into the window - white text on the light theme (#15111)
        var imageBackdrop = new Border
        {
            Background = ImagePreviewBackground.CreateBrush(),
            CornerRadius = new CornerRadius(6),
            Child = image,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        var imageSection = new Border
        {
            Child = imageBackdrop,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(14, 12),
        };

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // Whole image for subtitle
                new RowDefinition { Height = GridLength.Star }, // Controls
            },
        };
        sections.Add(imageSection, 0, 0);
        sections.Add(MakeControlsView(vm), 1, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonInspectAdditions = UiUtil.MakeButton(Se.Language.General.InspectAdditions, vm.InspectAdditionsCommand).WithBindIsVisible(nameof(vm.IsInspectAdditionsVisible));
        var buttonUseOnce = UiUtil.MakeButton(Se.Language.General.UseOnce, vm.UseOnceCommand).WithBindIsVisible(nameof(vm.ShowUseOnce));
        var buttonSkip = UiUtil.MakeButton(Se.Language.General.Skip, vm.SkipCommand).WithBindIsVisible(nameof(vm.ShowSkip));
        var buttonAbort = UiUtil.MakeButton(Se.Language.General.Abort, vm.AbortCommand).WithBindIsVisible(nameof(vm.ShowAbort));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.AbortCommand).WithBindIsVisible(nameof(vm.ShowCancel));
        var buttonBar = EditorChrome.MakeDockedButtonBar(buttonInspectAdditions, buttonUseOnce, buttonSkip, buttonAbort, buttonCancel, buttonOk);
        buttonInspectAdditions.HorizontalAlignment = HorizontalAlignment.Left;

        Content = EditorChrome.MakeRoot(sections, buttonBar);

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;
        vm.TextBoxNew.KeyUp += vm.TextBoxNewOnKeyUp;
        vm.TextBoxNew.PointerReleased += vm.TextBoxMacPointerReleased;
        var menuFlyout = new MenuFlyout();
        CharactersFlyoutMenuHelper.MakeFlyoutLetters(menuFlyout, vm.InsertSpecialCharacterCommand);
        vm.TextBoxNew.ContextFlyout = menuFlyout;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        });
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
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
            },
        };

        vm.TextBoxNew = EditorChrome.AsTextField(UiUtil.MakeTextBox(100, vm, nameof(vm.NewText)), Se.Language.General.Text);
        vm.TextBoxNew.Width = double.NaN;
        vm.TextBoxNew.Margin = new Thickness(0);
        vm.TextBoxNew.HorizontalAlignment = HorizontalAlignment.Stretch;
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxNew.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }
        vm.TextBoxNew.FontStyle = vm.IsNewTextItalic ? FontStyle.Italic : FontStyle.Normal;

        var image = new Image
        {
            Margin = new Thickness(5),
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));

        var panelCurrentImage = new Border
        {
            Background = ImagePreviewBackground.CreateBrush(),
            CornerRadius = new CornerRadius(6),
            Child = image,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        // Italic stays a check box: its label lights up (orange, bold, italic) while on, so a
        // wrong italic flag is hard to miss before pressing Enter.
        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsNewTextItalic));
        checkBoxItalic.Margin = new Thickness(0);
        var italicActiveBrush = new SolidColorBrush(Colors.Orange);
        void UpdateItalicLabel()
        {
            var isItalic = checkBoxItalic.IsChecked == true;
            checkBoxItalic.FontStyle = isItalic ? FontStyle.Italic : FontStyle.Normal;
            checkBoxItalic.FontWeight = isItalic ? FontWeight.Bold : FontWeight.Normal;
            if (isItalic)
            {
                checkBoxItalic.Foreground = italicActiveBrush;
            }
            else
            {
                checkBoxItalic.ClearValue(TemplatedControl.ForegroundProperty);
            }
        }
        UpdateItalicLabel();
        checkBoxItalic.IsCheckedChanged += vm.ItalicCheckChanged;
        checkBoxItalic.IsCheckedChanged += (_, _) => UpdateItalicLabel();

        var (_, autoSubmitRow) = NOcrChrome.MakeSwitchRow(nameof(vm.SubmitOnFirstLetter), Se.Language.Ocr.AutoSubmitFirstCharacter);

        var panelCurrent = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Width = 220,
            Children =
            {
                EditorChrome.MakeBoundInfo(nameof(vm.ResolutionAndTopMargin)),
                panelCurrentImage,
                vm.TextBoxNew,
                checkBoxItalic,
                autoSubmitRow,
            },
        };

        var comboBoxLinesToAutoDraw = UiUtil.MakeComboBox(vm.NoOfLinesToAutoDrawList, vm, nameof(vm.SelectedNoOfLinesToAutoDraw));
        comboBoxLinesToAutoDraw.MinWidth = 100;
        comboBoxLinesToAutoDraw.Margin = new Thickness(0);
        AutomationProperties.SetName(comboBoxLinesToAutoDraw, Se.Language.Ocr.LinesToDraw);
        var panelLinesToDraw = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children =
            {
                comboBoxLinesToAutoDraw,
                NOcrChrome.MakeDrawingTipsButton(vm.ShowDrawingTips),
            }
        };

        var comboBoxAlgorithm = UiUtil.MakeComboBox(vm.LineAlgorithms, vm, nameof(vm.SelectedLineAlgorithm));
        comboBoxAlgorithm.Margin = new Thickness(0);
        comboBoxAlgorithm.HorizontalAlignment = HorizontalAlignment.Stretch;
        AutomationProperties.SetName(comboBoxAlgorithm, Se.Language.Ocr.NOcrLineAlgorithm);
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(comboBoxAlgorithm, Se.Language.Ocr.NOcrLineAlgorithmHint);
        }

        var buttonClear = new SeSplitButton
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
            },
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var buttonDrawAgain = EditorChrome.MakeTextToolButton(IconNames.Refresh, Se.Language.Ocr.AutoDrawAgain, vm.DrawAgainCommand);
        buttonDrawAgain.HorizontalAlignment = HorizontalAlignment.Left;
        buttonDrawAgain.Margin = new Thickness(-8, 4, 0, 0);

        var panelDrawControls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Width = 200,
            Children =
            {
                panelLinesToDraw,
                EditorChrome.MakeDimLabel(Se.Language.Ocr.NOcrLineAlgorithm, new Thickness(0, 4, 0, 0)),
                comboBoxAlgorithm,
                EditorChrome.MakeBoundInfo(nameof(vm.LinesGeneratedInfo)),
                buttonDrawAgain,
                buttonClear,
            }
        };

        vm.NOcrDrawingCanvas.SetStrokeWidth(1);

        var toggleButtonForeground = NOcrChrome.MakeToggle(Se.Language.General.Foreground, nameof(vm.IsNewLinesForegroundActive), vm);
        toggleButtonForeground.IsCheckedChanged += vm.DrawModeForegroundChanged;
        var toggleButtonBackground = NOcrChrome.MakeToggle(Se.Language.General.Background, nameof(vm.IsNewLinesBackgroundActive), vm);
        toggleButtonBackground.IsCheckedChanged += vm.DrawModeBackgroundChanged;

        var buttonShrink = EditorChrome.MakeTextToolButton(IconNames.ArrowCollapseVertical, Se.Language.General.Shrink, vm.ShrinkCommand);
        buttonShrink.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.CanShrink)));
        var buttonExpand = EditorChrome.MakeTextToolButton(IconNames.ArrowExpandRight, Se.Language.General.Expand, vm.ExpandCommand);
        buttonExpand.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.CanExpand)));
        var panelShrinkExpand = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children = { buttonShrink, buttonExpand },
        };

        var canvasContent = NOcrChrome.MakeCanvasContent(
            NOcrChrome.MakeCanvasFrame(vm.NOcrDrawingCanvas),
            NOcrChrome.MakeZoomBar(vm.ZoomOutCommand, vm.ZoomInCommand, nameof(vm.ZoomFactorInfo)),
            NOcrChrome.MakeDrawModeBar(toggleButtonForeground, toggleButtonBackground),
            panelShrinkExpand);

        grid.Add(NOcrChrome.MakeScrollingColumn(Se.Language.Ocr.CurrentImage, panelCurrent, first: true), 0, 0);
        grid.Add(NOcrChrome.MakeFillColumn(Se.Language.Ocr.LinesToDraw, panelDrawControls), 0, 1);
        grid.Add(NOcrChrome.MakeFillColumn(Se.Language.General.Preview, canvasContent), 0, 2);

        return grid;
    }
}
