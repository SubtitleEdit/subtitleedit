using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// The nOCR additions history in the editor look (see <see cref="EditorChrome"/>): the history
/// list, the selected addition's image and text, and its drawn lines, as columns; OK in the
/// button bar.
/// </summary>
public class NOcrCharacterHistoryWindow : Window
{
    public NOcrCharacterHistoryWindow(NOcrCharacterHistoryViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Empty;
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.TextToolToggles | EditorWindowStyles.CompactSplitButtons);
        DataContext = vm;

        var sections = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(260) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
            },
        };

        sections.Add(MakeListView(vm), 0, 0);
        sections.Add(MakeCurrentView(vm), 0, 1);
        sections.Add(MakeCanvasView(vm), 0, 2);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonOk));

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        });
        PointerWheelChanged += vm.PointerWheelChanged;
        KeyDown += (_, e) => vm.KeyDown(e);
        KeyUp += (_, e) => vm.KeyUp(e);
        Loaded += (_, _) => Title = vm.Title;
    }

    private static Border MakeListView(NOcrCharacterHistoryViewModel vm)
    {
        var listBoxCurrentItems = new ListBox().WithAccessibleName(Se.Language.General.History);
        listBoxCurrentItems.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedHistoryItem)));
        listBoxCurrentItems.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.HistoryItems)));
        listBoxCurrentItems.SelectionChanged += vm.HistoryItemChanged;

        return NOcrChrome.MakeFillColumn(Se.Language.General.History, EditorChrome.MakeListFrame(listBoxCurrentItems, clearListBackground: true), first: true);
    }

    private static Border MakeCurrentView(NOcrCharacterHistoryViewModel vm)
    {
        vm.TextBoxNew = EditorChrome.AsTextField(UiUtil.MakeTextBox(100, vm, nameof(vm.NewText)), Se.Language.General.Text);
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
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));

        // subtitle bitmaps are light or dark text on transparent pixels, so they need a backdrop
        var panelCurrentImage = new Border
        {
            Background = ImagePreviewBackground.CreateBrush(),
            CornerRadius = new CornerRadius(6),
            Child = image,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var (switchItalic, italicRow) = NOcrChrome.MakeSwitchRow(nameof(vm.IsNewTextItalic), Se.Language.General.Italic);
        switchItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var actions = new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(-8, 6, 0, 0),
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.Check, Se.Language.General.Update, vm.UpdateCommand),
                EditorChrome.MakeTextToolButton(IconNames.CheckBold, Se.Language.General.UpdateAndClose, vm.UpdateAndCloseCommand),
                EditorChrome.MakeTextToolButton(IconNames.Trash, Se.Language.General.Delete, vm.DeleteCommand),
            },
        };
        foreach (var action in actions.Children)
        {
            action.HorizontalAlignment = HorizontalAlignment.Left;
        }

        var panelCurrent = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Width = 240,
            Children =
            {
                EditorChrome.MakeBoundInfo(nameof(vm.ResolutionAndTopMargin)),
                EditorChrome.MakeBoundInfo(nameof(vm.LineIndex)),
                panelCurrentImage,
                vm.TextBoxNew,
                italicRow,
                actions,
            },
        };

        return NOcrChrome.MakeFillColumn(Se.Language.Ocr.CurrentImage, panelCurrent);
    }

    private static Border MakeCanvasView(NOcrCharacterHistoryViewModel vm)
    {
        vm.NOcrDrawingCanvas.SetStrokeWidth(1);
        var zoomBar = NOcrChrome.MakeZoomBar(vm.ZoomOutCommand, vm.ZoomInCommand, nameof(vm.ZoomFactorInfo));
        return NOcrChrome.MakeFillColumn(Se.Language.General.Preview, NOcrChrome.MakeCanvasContent(NOcrChrome.MakeCanvasFrame(vm.NOcrDrawingCanvas), zoomBar));
    }
}
