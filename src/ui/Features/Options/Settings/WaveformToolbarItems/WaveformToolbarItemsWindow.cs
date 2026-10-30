using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;

/// <summary>
/// Order, visibility and sizes of the waveform toolbar items in the editor look (see
/// <see cref="EditorChrome"/>): the reorderable list, the selected item's settings to the right,
/// Cancel + OK in the button bar.
/// </summary>
public class WaveformToolbarItemsWindow : Window
{
    private const double LabelWidth = 90;

    public WaveformToolbarItemsWindow(WaveformToolbarItemsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.WaveformToolbarItems;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        vm.Window = this;
        DataContext = vm;

        var listBox = new ListBox
        {
            Width = 340,
            Height = 380,
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding(nameof(WaveformToolbarItemsViewModel.ToolbarItems)));
        listBox.Bind(ListBox.SelectedItemProperty, new Binding(nameof(WaveformToolbarItemsViewModel.SelectedToolbarItem)) { Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(listBox, Se.Language.Options.Settings.WaveformToolbarItems);
        listBox.ItemTemplate = new FuncDataTemplate<ToolbarItemDisplay>((_, _) =>
        {
            var checkBox = new CheckBox { MinWidth = 0 };
            checkBox.Bind(CheckBox.IsCheckedProperty, new Binding(nameof(ToolbarItemDisplay.IsVisible)) { Mode = BindingMode.TwoWay });
            // Shift+Tab back into the list lands on this check box rather than the row, and a
            // check box with no content is announced as a nameless "check box" (#12087).
            checkBox.Bind(AutomationProperties.NameProperty, new Binding(nameof(ToolbarItemDisplay.Name)));

            var textBlock = new TextBlock { Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(ToolbarItemDisplay.Name)));

            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { checkBox, textBlock },
            };
        }, true);

        var moveButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                EditorChrome.MakeTextToolButton(IconNames.ArrowUpThin, Se.Language.General.MoveUp, vm.MoveUpCommand),
                EditorChrome.MakeTextToolButton(IconNames.ArrowDownThin, Se.Language.General.MoveDown, vm.MoveDownCommand),
            },
        };

        // Only the items that have a width (the initial text box) show the setting.
        var numericUpDownWidth = UiUtil.MakeNumericUpDownInt(100, 3000, 400, 100, vm, nameof(vm.SelectedWidth))
            .AsField(Se.Language.General.Width, themeForeground: true);
        numericUpDownWidth.Increment = 10;
        var rowWidth = EditorChrome.MakeLabeledRow(Se.Language.General.Width, numericUpDownWidth, LabelWidth);
        rowWidth.Bind(IsVisibleProperty, new Binding(nameof(vm.IsWidthVisible)));

        var sidePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Margin = new Thickness(14, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                moveButtons,
                EditorChrome.MakeLabeledRow(Se.Language.General.FontSize, UiUtil.MakeNumericUpDownInt(6, 72, 12, 100, vm, nameof(vm.SelectedFontSize)).AsField(Se.Language.General.FontSize, themeForeground: true), LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.LeftMargin, UiUtil.MakeNumericUpDownInt(0, 100, 5, 100, vm, nameof(vm.SelectedLeftMargin)).AsField(Se.Language.General.LeftMargin, themeForeground: true), LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.RightMargin, UiUtil.MakeNumericUpDownInt(0, 100, 5, 100, vm, nameof(vm.SelectedRightMargin)).AsField(Se.Language.General.RightMargin, themeForeground: true), LabelWidth),
                rowWidth,
            },
        };

        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        contentGrid.Add(EditorChrome.MakeCompactListFrame(listBox), 0, 0);
        contentGrid.Add(sidePanel, 0, 1);

        var section = EditorChrome.MakeEditorSection(Se.Language.Options.Settings.WaveformToolbarItems, contentGrid, first: true);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, listBox); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
