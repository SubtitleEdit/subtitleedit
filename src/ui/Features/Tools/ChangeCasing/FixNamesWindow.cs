using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

/// <summary>
/// The fix names step of change casing, in the editor look (see <see cref="EditorChrome"/>): the
/// names found (plus extra names) on top, the hits they give below, Cancel and OK at the bottom.
/// </summary>
public class FixNamesWindow : Window
{

    private FixNamesViewModel _vm;

    public FixNamesWindow(FixNamesViewModel vm)
    {
        var lang = Se.Language.Tools.ChangeCasing;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChangeCasing;
        Width = 900;
        Height = 800;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // Names
        var labelNamesCount = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        labelNamesCount.Bind(TextBlock.TextProperty, new Binding($"{nameof(vm.Names)}.{nameof(vm.Names.Count)}")
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            StringFormat = lang.NamesX,
        });

        var textBoxExtraNames = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.ExtraNames))
            .WithAccessibleName(lang.ExtraNames);
        textBoxExtraNames.Classes.Add("text");
        textBoxExtraNames.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxExtraNames.PlaceholderText = lang.EnterExtraNamesHint;
        var buttonAddExtraName = EditorChrome.MakeTextToolButton("fa-solid fa-rotate", Se.Language.General.Refresh, vm.AddExtraNameCommand);
        var gridExtraNames = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        gridExtraNames.Add(textBoxExtraNames, 0, 0);
        gridExtraNames.Add(buttonAddExtraName, 0, 1);

        var namesContent = new Grid
        {
            RowSpacing = 8,
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // names
                new RowDefinition(GridLength.Auto), // extra names
            },
        };
        namesContent.Add(MakeNamesView(vm), 0, 0);
        namesContent.Add(EditorChrome.MakeLabeledRow(lang.ExtraNames, gridExtraNames, 90), 1, 0);

        // Hits
        var labelHits = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        labelHits.Bind(TextBlock.TextProperty, new Binding($"{nameof(vm.Hits)}.{nameof(vm.Hits.Count)}")
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            StringFormat = lang.HitsX,
        });

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // names
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // hits
            },
        };
        sections.Add(MakeFillSection(lang.FixNames, labelNamesCount, namesContent, true), 0, 0);
        sections.Add(MakeFillSection(Se.Language.General.Preview, labelHits, MakeHitsView(vm), false), 1, 0);

        // OK and Cancel buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    /// <summary>
    /// A section that fills its row: header (with a count on the right), then the content.
    /// </summary>
    private static Border MakeFillSection(string title, Control headerRight, Control content, bool first)
    {
        var header = new DockPanel();
        DockPanel.SetDock(headerRight, Dock.Right);
        header.Children.Add(headerRight);
        header.Children.Add(EditorChrome.MakeSectionHeader(title));

        var grid = new Grid
        {
            RowSpacing = 10,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(header, 0, 0);
        grid.Add(content, 1, 0);
        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    private static Border MakeNamesView(FixNamesViewModel vm)
    {
        // The DataGrid this replaces used DataGridCheckboxMultiSelect for extended
        // selection + Space toggling; shift/ctrl multi-select is native ListBox behavior
        // on TableView, so only the Space toggle needs wiring (TableViewExtras.AddSpaceToggle).
        var dataGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.Tools.ChangeCasing.FixNames);
        dataGrid.Height = double.NaN;
        dataGrid.ItemsSource = vm.Names;
        dataGrid.DataContext = vm;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixNameItem>(static (item, _) =>
            new Border
            {
                Background = Brushes.Transparent, // Prevents highlighting
                Padding = new Thickness(4),
                Child = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNameItem.IsChecked)),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                }
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80)
        });

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            Binding = new Binding(nameof(FixNameItem.Name)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        TableViewExtras.AddSpaceToggle<FixNameItem>(dataGrid,
            item => item.IsChecked, (item, v) => item.IsChecked = v);

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.SelectAll, Command = vm.NamesSelectAllCommand });
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.InvertSelection, Command = vm.NamesInvertSelectionCommand });
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        return EditorChrome.MakeListFrame(dataGrid, clearListBackground: true);
    }

    private static Border MakeHitsView(FixNamesViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView().WithAccessibleName(Se.Language.General.Preview);
        dataGrid.Height = double.NaN;
        dataGrid.ItemsSource = vm.Hits;
        dataGrid.DataContext = vm;

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Apply,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<FixNameHitItem>(static (_, _) => new Border
            {
                Background = Brushes.Transparent, // Prevents highlighting
                Padding = new Thickness(4),
                Child = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNameHitItem.IsEnabled)),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                }
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(80)
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            Binding = new Binding(nameof(FixNameHitItem.LineIndexDisplay)),
            Width = new GridLength(140),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Before,
            Binding = new Binding(nameof(FixNameHitItem.Before)),
            Width = new GridLength(220),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.After,
            Binding = new Binding(nameof(FixNameHitItem.After)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        TableViewExtras.AddSpaceToggle<FixNameHitItem>(dataGrid,
            item => item.IsEnabled, (item, v) => item.IsEnabled = v);

        var flyout = new MenuFlyout();
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.SelectAll, Command = vm.HitsSelectAllCommand });
        flyout.Items.Add(new MenuItem { Header = Se.Language.General.InvertSelection, Command = vm.HitsInvertSelectionCommand });
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        return EditorChrome.MakeListFrame(dataGrid, clearListBackground: true);
    }


    protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }
}
