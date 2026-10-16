using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

/// <summary>
/// The binary OCR characters added in this session, in the editor look (see
/// <see cref="EditorChrome"/>): the history list on the left, the selected image with its text on
/// the right, OK in the button bar.
/// </summary>
public class BinaryOcrCharacterHistoryWindow : Window
{
    public BinaryOcrCharacterHistoryWindow(BinaryOcrCharacterHistoryViewModel vm)
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        DataContext = vm;

        var body = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(300) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        var listView = MakeListView(vm);
        var detailsView = MakeDetailsView(vm);

        body.Add(listView, 0, 0);
        body.Add(detailsView, 0, 1);

        var buttonOk = EditorChrome.AsFlushPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var labelInfo = EditorChrome.MakeDimStatusText("Binary OCR uses pixel-by-pixel comparison. No manual drawing required.");
        var buttonBar = EditorChrome.MakeDockedButtonBar(labelInfo, buttonOk);

        Content = EditorChrome.MakeRoot(body, buttonBar);

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
        });
        PointerWheelChanged += vm.PointerWheelChanged;
        KeyDown += (_, e) => vm.KeyDown(e);
        KeyUp += (_, e) => vm.KeyUp(e);
        Loaded += (_, e) => { Title = vm.Title; };
    }

    private static Border MakeListView(BinaryOcrCharacterHistoryViewModel vm)
    {
        var listBoxCurrentItems = new ListBox().WithAccessibleName(Se.Language.General.History);
        listBoxCurrentItems.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedHistoryItem)));
        listBoxCurrentItems.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.HistoryItems)));
        listBoxCurrentItems.SelectionChanged += vm.HistoryItemChanged;

        return EditorChrome.MakeDockedFillSection(Se.Language.General.History, EditorChrome.MakeListFrame(listBoxCurrentItems, clearListBackground: true), first: true);
    }

    private static Border MakeDetailsView(BinaryOcrCharacterHistoryViewModel vm)
    {
        vm.TextBoxNew = BinaryOcrChrome.AsField(UiUtil.MakeTextBox(100, vm, nameof(vm.NewText))).WithAccessibleName(Se.Language.General.Text);
        vm.TextBoxNew.Width = 220;
        vm.TextBoxNew.HorizontalAlignment = HorizontalAlignment.Left;
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxNew.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }
        var image = new Image
        {
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            StretchDirection = StretchDirection.DownOnly, // natural size, as in the old auto-sized column
            MinHeight = 30,
            MaxWidth = 400,
            MaxHeight = 400,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));
        var panelCurrentImage = BinaryOcrChrome.MakeImageFrame(image, ImagePreviewBackground.CreateBrush());
        panelCurrentImage.HorizontalAlignment = HorizontalAlignment.Left;

        var switchItalic = EditorChrome.MakeSwitch(nameof(vm.IsNewTextItalic), Se.Language.General.Italic);
        switchItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var panelCurrent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                BinaryOcrChrome.MakeInfo(nameof(vm.ResolutionAndTopMargin)),
                BinaryOcrChrome.MakeInfo(nameof(vm.LineIndex)),
                panelCurrentImage,
                vm.TextBoxNew,
                BinaryOcrChrome.MakeSwitchRow(switchItalic, Se.Language.General.Italic),
                new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    ItemSpacing = 6,
                    LineSpacing = 6,
                    Margin = new Thickness(0, 8, 0, 0),
                    Children =
                    {
                        BinaryOcrChrome.MakeActionButton(Se.Language.General.Update, IconNames.Check, vm.UpdateCommand),
                        BinaryOcrChrome.MakeActionButton(Se.Language.General.UpdateAndClose, IconNames.CheckBold, vm.UpdateAndCloseCommand),
                        BinaryOcrChrome.MakeActionButton(Se.Language.General.Delete, IconNames.Trash, vm.DeleteCommand),
                    },
                },
            },
        };

        var section = EditorChrome.MakeEditorSection(Se.Language.Ocr.CurrentImage, panelCurrent, first: true);
        return new Border
        {
            Child = new ScrollViewer { Content = section },
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };
    }
}
