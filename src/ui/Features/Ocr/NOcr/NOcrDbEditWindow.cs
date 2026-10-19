using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

/// <summary>
/// The nOCR database editor in the editor look (see <see cref="EditorChrome"/>): the characters
/// and their images, the selected image's text, and its drawn lines, as columns; Cancel + OK in
/// the button bar.
/// </summary>
public class NOcrDbEditWindow : Window
{
    public NOcrDbEditWindow(NOcrDbEditViewModel vm)
    {
        Title = Se.Language.Ocr.EditNOcrDatabase;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.TextToolToggles | EditorWindowStyles.CompactSplitButtons);
        DataContext = vm;
        Width = 955;
        Height = 800;
        MinWidth = 950;
        MinHeight = 600;

        var sections = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(240) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
            },
        };

        sections.Add(MakeCharacterControlsView(vm), 0, 0);
        sections.Add(MakeCurrentItemView(vm), 0, 1);
        sections.Add(MakeCanvasView(vm), 0, 2);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        });
        PointerWheelChanged += vm.PointerWheelChanged;
        KeyDown += (_, e) => vm.KeyDown(e);
        KeyUp += (_, e) => vm.KeyUp(e);
        Loaded += (_, _) => Title = vm.Title;
    }

    private static Border MakeCharacterControlsView(NOcrDbEditViewModel vm)
    {
        var charactersComboBox = UiUtil.MakeComboBox(vm.Characters, vm, nameof(vm.SelectedCharacter));
        charactersComboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
        charactersComboBox.Margin = new Thickness(0, 0, 0, 10);
        AutomationProperties.SetName(charactersComboBox, Se.Language.General.Characters);
        charactersComboBox.SelectionChanged += vm.CharactersChanged;

        var listBoxCurrentItems = new ListBox();
        AutomationProperties.SetName(listBoxCurrentItems, Se.Language.General.Characters);
        listBoxCurrentItems.Bind(Avalonia.Controls.Primitives.SelectingItemsControl.SelectedItemProperty, new Binding(nameof(vm.SelectedCurrentCharacterItem)));
        listBoxCurrentItems.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.CurrentCharacterItems)));
        listBoxCurrentItems.SelectionChanged += vm.CurrentCharacterItemsChanged;

        var dock = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(charactersComboBox, Dock.Top);
        dock.Children.Add(charactersComboBox);
        dock.Children.Add(EditorChrome.MakeListFrame(listBoxCurrentItems, clearListBackground: true));

        return NOcrChrome.MakeFillColumn(Se.Language.General.Characters, dock, first: true);
    }

    private static Border MakeCurrentItemView(NOcrDbEditViewModel vm)
    {
        vm.TextBoxItem = EditorChrome.AsTextField(UiUtil.MakeTextBox(100, vm, nameof(vm.ItemText)), Se.Language.General.Text);
        vm.TextBoxItem.Width = double.NaN;
        vm.TextBoxItem.Margin = new Thickness(0);
        vm.TextBoxItem.HorizontalAlignment = HorizontalAlignment.Stretch;
        if (!string.IsNullOrEmpty(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName))
        {
            vm.TextBoxItem.FontFamily = FontFamilyHelper.Make(Se.Settings.Appearance.SubtitleTextBoxAndGridFontName);
        }

        var (_, italicRow) = NOcrChrome.MakeSwitchRow(nameof(vm.IsItemItalic), Se.Language.General.Italic);

        var actions = new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(-8, 6, 0, 0),
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.Check, Se.Language.General.Update, vm.UpdateCommand),
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
            Width = 220,
            Children =
            {
                vm.TextBoxItem,
                italicRow,
                EditorChrome.MakeBoundInfo(nameof(vm.ResolutionAndTopMargin)),
                EditorChrome.MakeBoundInfo(nameof(vm.ExpandInfo)),
                actions,
            },
        };

        return NOcrChrome.MakeFillColumn(Se.Language.Ocr.CurrentImage, panelCurrent);
    }

    private static Border MakeCanvasView(NOcrDbEditViewModel vm)
    {
        vm.NOcrDrawingCanvas.SetStrokeWidth(1);

        var toggleButtonForeground = NOcrChrome.MakeToggle(Se.Language.General.Foreground, nameof(vm.IsNewLinesForegroundActive), vm);
        toggleButtonForeground.IsCheckedChanged += vm.DrawModeForegroundChanged;
        var toggleButtonBackground = NOcrChrome.MakeToggle(Se.Language.General.Background, nameof(vm.IsNewLinesBackgroundActive), vm);
        toggleButtonBackground.IsCheckedChanged += vm.DrawModeBackgroundChanged;

        var content = NOcrChrome.MakeCanvasContent(
            NOcrChrome.MakeCanvasFrame(vm.NOcrDrawingCanvas),
            NOcrChrome.MakeZoomBar(vm.ZoomOutCommand, vm.ZoomInCommand, nameof(vm.ZoomFactorInfo)),
            NOcrChrome.MakeDrawModeBar(toggleButtonForeground, toggleButtonBackground));

        return NOcrChrome.MakeFillColumn(Se.Language.General.Preview, content);
    }
}
