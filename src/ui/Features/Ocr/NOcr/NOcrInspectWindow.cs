using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// Inspect the nOCR matches of one image in the editor look (see <see cref="EditorChrome"/>):
/// the matched letters and the subtitle image on top, then the selected letter, the auto-draw
/// settings and the drawing canvas as columns; OK in the button bar.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.TextToolToggles | EditorWindowStyles.CompactSplitButtons);
        AddLetterButtonStyles();
        DataContext = vm;

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // Lines
                new RowDefinition { Height = GridLength.Star }, // Controls
            },
        };
        sections.Add(MakeLinesView(vm), 0, 0);
        sections.Add(MakeControlsView(vm), 1, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonOk));

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        });

        PointerWheelChanged += vm.PointerWheelChanged;
    }

    /// <summary>
    /// The letter buttons are built by the view model; give them the field look here.
    /// </summary>
    private void AddLetterButtonStyles()
    {
        Styles.Add(new Style(x => x.OfType<StackPanel>().Class("letters").Descendant().OfType<Button>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, EditorChrome.FieldBrush),
                new Setter(TemplatedControl.BorderBrushProperty, EditorChrome.FieldBorderBrush),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
                new Setter(TemplatedControl.FontSizeProperty, 15.0),
                new Setter(Layoutable.MinWidthProperty, 34.0),
                new Setter(ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Center),
            },
        });
    }

    private static Border MakeLinesView(NOcrInspectViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 14,
        };

        vm.PanelLines.Orientation = Orientation.Vertical;
        vm.PanelLines.Margin = new Thickness(-4, 0, 0, 0);
        vm.PanelLines.Classes.Add("letters");

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.SentenceBitmap)),
            Stretch = Stretch.Uniform,
            MaxWidth = 300,
            MaxHeight = 200,
        };

        // subtitle bitmaps are light or dark text on transparent pixels, so without a backdrop they
        // vanish into the window - white text on the light theme (#15111)
        var imageBackdrop = new Border
        {
            Background = ImagePreviewBackground.CreateBrush(),
            CornerRadius = new CornerRadius(6),
            Child = image,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        grid.Add(vm.PanelLines, 0, 0);
        grid.Add(imageBackdrop, 0, 1);

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Match, grid, first: true);
        section.BorderThickness = new Thickness(0, 0, 0, 1);
        return section;
    }

    private static Grid MakeControlsView(NOcrInspectViewModel vm)
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

        vm.TextBoxNew = EditorChrome.AsTextField(UiUtil.MakeTextBox(100, vm, nameof(vm.NewText)), Se.Language.General.Text)
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));
        vm.TextBoxNew.Width = double.NaN;
        vm.TextBoxNew.Margin = new Thickness(0);
        vm.TextBoxNew.HorizontalAlignment = HorizontalAlignment.Stretch;
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxNew.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        var image = new Image
        {
            Margin = new Thickness(5),
            [!Image.SourceProperty] = new Binding(nameof(vm.CurrentBitmap)),
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var panelCurrentImage = new Border
        {
            Background = ImagePreviewBackground.CreateBrush(),
            CornerRadius = new CornerRadius(6),
            Child = image,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var (switchItalic, italicRow) = NOcrChrome.MakeSwitchRow(nameof(vm.IsNewTextItalic), Se.Language.General.Italic);
        switchItalic.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsEditControlsEnabled)));
        switchItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var buttonUpdate = EditorChrome.MakeTextToolButton(IconNames.Check, Se.Language.General.Update, vm.UpdateCommand);
        buttonUpdate.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsEditControlsEnabled)));
        var buttonDelete = EditorChrome.MakeTextToolButton(IconNames.Trash, Se.Language.General.Delete, vm.DeleteCommand);
        buttonDelete.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsEditControlsEnabled)));
        var buttonAddBetterMatch = EditorChrome.MakeTextToolButton(IconNames.Plus, Se.Language.Ocr.AddBetterMatch, vm.AddBetterMatchCommand);
        var actions = new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(-8, 6, 0, 0),
            Children = { buttonUpdate, buttonDelete, buttonAddBetterMatch },
        };
        foreach (var action in actions.Children)
        {
            action.HorizontalAlignment = HorizontalAlignment.Left;
        }

        var panelCurrent = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Width = 220,
            Children =
            {
                panelCurrentImage,
                EditorChrome.MakeBoundInfo(nameof(vm.ResolutionAndTopMargin)),
                EditorChrome.MakeSectionHeader(Se.Language.General.Match).WithMarginTop(10),
                vm.TextBoxNew,
                italicRow,
                EditorChrome.MakeBoundInfo(nameof(vm.MatchResolutionAndTopMargin)),
                actions,
            },
        };

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
        }.WithBindEnabled(nameof(vm.IsEditControlsEnabled));

        var comboBoxLinesToAutoDraw = UiUtil.MakeComboBox(vm.NoOfLinesToAutoDrawList, vm, nameof(vm.SelectedNoOfLinesToAutoDraw))
            .WithBindEnabled(nameof(vm.IsEditControlsEnabled));
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

        var buttonDrawAgain = EditorChrome.MakeTextToolButton(IconNames.Refresh, Se.Language.Ocr.AutoDrawAgain, vm.DrawAgainCommand);
        buttonDrawAgain.HorizontalAlignment = HorizontalAlignment.Left;
        buttonDrawAgain.Margin = new Thickness(-8, 4, 0, 0);
        buttonDrawAgain.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsEditControlsEnabled)));

        var panelDrawControls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Width = 200,
            Children =
            {
                panelLinesToDraw,
                buttonDrawAgain,
                buttonClear,
            }
        };

        vm.NOcrDrawingCanvas.SetStrokeWidth(1);

        var toggleButtonForeground = NOcrChrome.MakeToggle(Se.Language.General.Foreground, nameof(vm.IsNewLinesForegroundActive), vm);
        toggleButtonForeground.IsCheckedChanged += vm.DrawModeForegroundChanged;
        var toggleButtonBackground = NOcrChrome.MakeToggle(Se.Language.General.Background, nameof(vm.IsNewLinesBackgroundActive), vm);
        toggleButtonBackground.IsCheckedChanged += vm.DrawModeBackgroundChanged;

        var canvasContent = NOcrChrome.MakeCanvasContent(
            NOcrChrome.MakeCanvasFrame(vm.NOcrDrawingCanvas),
            NOcrChrome.MakeZoomBar(vm.ZoomOutCommand, vm.ZoomInCommand, nameof(vm.ZoomFactorInfo), nameof(vm.IsEditControlsEnabled)),
            NOcrChrome.MakeDrawModeBar(toggleButtonForeground, toggleButtonBackground));

        var drawColumn = NOcrChrome.MakeFillColumn(Se.Language.Ocr.LinesToDraw, panelDrawControls);
        drawColumn.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsEditControlsEnabled)));
        var canvasColumn = NOcrChrome.MakeFillColumn(Se.Language.General.Preview, canvasContent);
        canvasColumn.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsEditControlsEnabled)));

        grid.Add(NOcrChrome.MakeScrollingColumn(Se.Language.Ocr.CurrentImage, panelCurrent, first: true), 0, 0);
        grid.Add(drawColumn, 0, 1);
        grid.Add(canvasColumn, 0, 2);

        return grid;
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
