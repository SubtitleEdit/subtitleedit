using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveUnicodeCharacters;

public class RemoveUnicodeCharactersWindow : Window
{
    public RemoveUnicodeCharactersWindow(RemoveUnicodeCharactersViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.RemoveUnicodeCharacters.Title;
        CanResize = true;
        Width = 800;
        Height = 600;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Tools.RemoveUnicodeCharacters;

        // Sorting dropped in the DataGrid -> TableView conversion: the characters are
        // listed in order of first appearance in the subtitle.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Characters;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(55),
                    CellTemplate = new FuncDataTemplate<RemoveUnicodeCharacterItem>((_, _) =>
                        new CheckBox
                        {
                            Focusable = false,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            [!CheckBox.IsCheckedProperty] = new Binding(nameof(RemoveUnicodeCharacterItem.IsChecked)) { Mode = BindingMode.TwoWay },
                        }),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Character,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.Character)),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(90),
                },
                new SeTableViewColumn
                {
                    Header = "Unicode",
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.CodeDisplay)),
                    Width = new GridLength(100),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Count,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.Count)),
                    Width = new GridLength(70),
                },
                new SeTableViewColumn
                {
                    // The DataGrid edited this text column in place; TableView has no cell
                    // editing, so the cell hosts an always-editable TextBox instead.
                    Header = Se.Language.General.ReplaceWith,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(120),
                    CellTemplate = new FuncDataTemplate<RemoveUnicodeCharacterItem>((_, _) =>
                        new TextBox
                        {
                            [!TextBox.TextProperty] = new Binding(nameof(RemoveUnicodeCharacterItem.ReplaceWith)) { Mode = BindingMode.TwoWay },
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Margin = new Thickness(2),
                        }),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Lines,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(RemoveUnicodeCharacterItem.LinesDisplay)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
        });

        // Extended selection is native ListBox behavior on TableView; only the
        // Space-toggles-checkbox piece of the old CheckboxMultiSelect needs wiring.
        // Hand-rolled instead of TableViewExtras.AddSpaceToggle because Space typed in
        // the ReplaceWith TextBox must keep inserting a space, not toggle checkboxes.
        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key != Key.Space ||
                (e.Source is Visual source && source.FindAncestorOfType<TextBox>(includeSelf: true) != null))
            {
                return;
            }

            var selected = dataGrid.SelectedItems?.OfType<RemoveUnicodeCharacterItem>().ToList();
            if (selected == null || selected.Count == 0)
            {
                return;
            }

            var newValue = !selected.All(item => item.IsChecked);
            foreach (var item in selected)
            {
                item.IsChecked = newValue;
            }

            e.Handled = true;
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        var buttonSelectAll = UiUtil.MakeButton(Se.Language.General.SelectAll, vm.SelectAllCommand);
        var buttonInvertSelection = UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.InvertSelectionCommand);
        var panelSelection = UiUtil.MakeHorizontalPanel(buttonSelectAll, buttonInvertSelection);

        var labelStatus = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.StatusText)).WithAlignmentTop();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 0);
        grid.Add(panelSelection, 1);
        grid.Add(labelStatus, 2);
        grid.Add(panelButtons, 2);

        Content = grid;

        Activated += delegate { TableViewExtras.FocusRow(dataGrid); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
