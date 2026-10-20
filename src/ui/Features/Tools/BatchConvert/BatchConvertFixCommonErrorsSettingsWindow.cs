using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

/// <summary>
/// The fix common errors rules for batch convert, in the editor look (see <see cref="EditorChrome"/>):
/// the rule checklist, the profile and OK in the button bar.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var comboProfile = UiUtil.MakeComboBox(vm.Profiles, vm, nameof(vm.SelectedProfile))
            .WithAccessibleName(Se.Language.General.Profile);
        comboProfile.MinWidth = 200;
        var profile = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { EditorChrome.MakeDimLabel(Se.Language.General.Profile, new Thickness(0)), comboProfile },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };

        grid.Add(MakeRulesView(vm), 0, 0);
        grid.Add(EditorChrome.MakeButtonBar(profile, buttonCancel, buttonOk), 1, 0);
        Content = grid;

        UiUtil.FocusOnFirstActivation(this, comboProfile); // initial focus on an input, not an action button - a focused button clicks on bare Space
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
                CellTemplate = TableViewExtras.MakeCheckBoxCellTemplate<FixRuleDisplayItem>(nameof(FixRuleDisplayItem.IsSelected)),
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
        rulesGrid.WithAccessibleName(Se.Language.General.Rules);

        var header = EditorChrome.MakeSectionHeader(Se.Language.General.Rules);
        var grid = new Grid
        {
            RowSpacing = 8,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(header, 0, 0);
        grid.Add(EditorChrome.MakeListFrame(rulesGrid), 1, 0);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14, 12, 14, 14),
        };
    }
}
