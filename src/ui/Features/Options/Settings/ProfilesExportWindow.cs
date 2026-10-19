using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class ProfilesExportWindow : Window
{
    public ProfilesExportWindow(ProfilesExportViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.ExportProfiles;
        CanResize = true;
        Width = 700;
        Height = 550;
        MinWidth = 500;
        MinHeight = 400;
        SettingsChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
        };

        grid.Add(EditorChrome.MakePanel(MakeDataGrid(vm, out var dataGrid)), 0);
        grid.Add(SettingsChrome.MakeButtonBar(null, buttonCancel, buttonOk), 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(dataGrid); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Grid MakeDataGrid(ProfilesExportViewModel vm, out TableView tableView)
    {
        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // the caller writes result.Profiles to the exported .profile file in collection
        // order, so reordering the backing collection would reorder the export.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.DataContext = vm;
        AutomationProperties.SetName(dataGrid, Se.Language.General.Profiles);
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Enabled,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<ProfileDisplay>((item, _) =>
                new Border
                {
                    Background = Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(4),
                    Child = new CheckBox
                    {
                        [!CheckBox.IsCheckedProperty] = new Binding(nameof(ProfileDisplay.IsSelected)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }),
                // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                Width = new GridLength(80)
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Name,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ProfileDisplay.Name)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Profiles)) { Source = vm });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedProfile)) { Source = vm });

        tableView = dataGrid;

        var header = EditorChrome.MakeSectionHeader(Se.Language.General.Profiles);
        header.Margin = new Thickness(0, 0, 0, 8);

        var panel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
            },
            Margin = new Thickness(14, 12, 14, 14),
        };
        panel.Add(header, 0);
        panel.Add(EditorChrome.MakeListFrame(dataGrid), 1);
        return panel;
    }
}
