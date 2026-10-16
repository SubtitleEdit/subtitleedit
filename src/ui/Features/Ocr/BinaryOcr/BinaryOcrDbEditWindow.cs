using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;

/// <summary>
/// Edit the binary image compare database, in the editor look (see <see cref="EditorChrome"/>):
/// the characters and their images on the left, the selected image with its text on the right,
/// OK in the button bar.
/// </summary>
public class BinaryOcrDbEditWindow : Window
{
    public BinaryOcrDbEditWindow(BinaryOcrDbEditViewModel vm)
    {
        Title = Se.Language.Ocr.EditBinaryOcrDatabase;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        DataContext = vm;
        Width = 955;
        Height = 800;
        MinWidth = 950;
        MinHeight = 600;

        var body = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(280) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        var charactersView = MakeCharacterControlsView(vm);
        var currentItemView = MakeCurrentItemControlsView(vm);

        body.Add(charactersView, 0, 0);
        body.Add(currentItemView, 0, 1);

        var buttonOk = EditorChrome.AsFlushPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonBar = EditorChrome.MakeDockedButtonBar(null, buttonOk);

        Content = EditorChrome.MakeRoot(body, buttonBar);

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        });
        KeyDown += (_, e) => vm.KeyDown(e);
        Loaded += (_, _) => Title = vm.Title;
    }

    private static Border MakeCharacterControlsView(BinaryOcrDbEditViewModel vm)
    {
        var charactersComboBox = UiUtil.MakeComboBox(vm.Characters, vm, nameof(vm.SelectedCharacter))
            .WithAccessibleName(Se.Language.General.Characters);
        charactersComboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        charactersComboBox.Width = double.NaN;
        charactersComboBox.SelectionChanged += vm.CharactersChanged;
        var listBoxCurrentItems = new ListBox().WithAccessibleName(Se.Language.General.Images);
        listBoxCurrentItems.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedCurrentCharacterItem)));
        listBoxCurrentItems.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.CurrentCharacterItems)));
        listBoxCurrentItems.SelectionChanged += vm.CurrentCharacterItemsChanged;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
        };
        grid.Add(charactersComboBox, 0, 0);
        grid.Add(EditorChrome.MakeListFrame(listBoxCurrentItems, clearListBackground: true), 1, 0);

        return EditorChrome.MakeDockedFillSection(Se.Language.General.Characters, grid, first: true);
    }

    private static Border MakeCurrentItemControlsView(BinaryOcrDbEditViewModel vm)
    {
        vm.TextBoxItem = BinaryOcrChrome.AsField(UiUtil.MakeTextBox(100, vm, nameof(vm.ItemText))).WithAccessibleName(Se.Language.General.Text);
        vm.TextBoxItem.Width = 220;
        vm.TextBoxItem.HorizontalAlignment = HorizontalAlignment.Left;
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxItem.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }
        vm.TextBoxItem.Bind(TextBox.FontStyleProperty, new Binding(nameof(vm.IsItemItalic)) { Converter = new BoolToFontStyleConverter() });

        var image = new Image
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            StretchDirection = StretchDirection.DownOnly, // natural size, as in the old auto-sized column
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.ItemBitmap)));
        var imageBorder = BinaryOcrChrome.MakeImageFrame(image, ImagePreviewBackground.CreateBrush());
        imageBorder.HorizontalAlignment = HorizontalAlignment.Left;
        imageBorder.MinWidth = 40;
        imageBorder.MinHeight = 40;

        var switchItalic = EditorChrome.MakeSwitch(nameof(vm.IsItemItalic), Se.Language.General.Italic);

        var panelCurrent = new StackPanel
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                imageBorder,
                BinaryOcrChrome.MakeInfo(nameof(vm.ResolutionAndTopMargin)),
                BinaryOcrChrome.MakeInfo(nameof(vm.ExpandInfo)),
                vm.TextBoxItem,
                BinaryOcrChrome.MakeSwitchRow(switchItalic, Se.Language.General.Italic),
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Margin = new Thickness(0, 8, 0, 0),
                    Children =
                    {
                        BinaryOcrChrome.MakeActionButton(Se.Language.General.Update, IconNames.Check, vm.UpdateCommand),
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
