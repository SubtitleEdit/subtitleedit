using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertFixCommonErrorsSettingsWindow : Window
{
    public BatchConvertFixCommonErrorsSettingsWindow(BatchConvertFixCommonErrorsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.FixCommonErrorsSettingsTitle;
        CanResize = false;
        Width = 1000;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var rulesView = MakeRulesView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var comboProfile = UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile));
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(rulesView, 0, 0);
        grid.Add(comboProfile, 1, 0);   
        grid.Add(panelButtons, 1, 0);
        Content = grid;

        Activated += delegate { comboProfile.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeRulesView(BatchConvertFixCommonErrorsSettingsViewModel vm)
    {
        // No header-click sorting (the DataGrid's CanUserSortColumns is not carried
        // over): the fix rules are a settings checklist in their fixed rule order.
        // The DataGrid-era DataGridCheckboxMultiSelect is replaced by native extended
        // selection plus TableViewExtras.AddSpaceToggle for the Space-toggles-checkbox piece.
        var rulesGrid = TableViewExtras.MakeTableView();
        rulesGrid.Width = double.NaN;
        rulesGrid.Height = double.NaN;
        rulesGrid[!TableView.ItemsSourceProperty] = new Binding($"{nameof(vm.SelectedProfile)}.{nameof(ProfileDisplayItem.FixRules)}");
        rulesGrid.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Enabled,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<FixRuleDisplayItem>((item, _) =>
                    new Border
                    {
                        Background = Brushes.Transparent, // Prevents highlighting
                        Padding = new Thickness(4),
                        Child = new CheckBox
                        {
                            Focusable = false,
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixRuleDisplayItem.IsSelected)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }),
                Width = new GridLength(80), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Name,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(FixRuleDisplayItem.Name)),
                Width = new GridLength(340), // content-sized (Auto) on the DataGrid; rule names are long
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Example,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(FixRuleDisplayItem.Example)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        TableViewExtras.AddSpaceToggle<FixRuleDisplayItem>(rulesGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);

        return UiUtil.MakeBorderForControl(rulesGrid);
    }
}