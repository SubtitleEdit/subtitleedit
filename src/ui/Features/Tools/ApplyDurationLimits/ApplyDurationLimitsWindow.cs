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
using System.Collections;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

/// <summary>
/// Apply duration limits, in the editor look (see <see cref="EditorChrome"/>): the limits on top,
/// the fixes (with their Apply check boxes) and the lines that cannot be fixed below, Cancel/OK in
/// the button bar.
/// </summary>
public class ApplyDurationLimitsWindow : Window
{
    private ToggleSwitch _toggleFixMinDuration = null!;

    public ApplyDurationLimitsWindow(ApplyDurationLimitsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.ApplyDurationLimits.Title);
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };

        sections.Add(EditorChrome.MakeEditorSection(Se.Language.General.Options, MakeControlsView(vm), first: true), 0);
        sections.Add(MakeFixesView(vm), 1);
        sections.Add(MakeCannotFixView(vm), 2);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, _toggleFixMinDuration); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Grid MakeControlsView(ApplyDurationLimitsViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 14,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var switchFixMinDuration = EditorChrome.MakeToggleRow(nameof(vm.FixMinDurationMs), vm.FixMinDurationLabel, out var toggleFixMinDuration);
        _toggleFixMinDuration = toggleFixMinDuration;
        toggleFixMinDuration.IsCheckedChanged += (s, e) => vm.SetChanged();
        var numericUpDownMinDuration = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(1, 10000, 1000, 90, vm, nameof(vm.MinDurationMsOrFrames)), vm.FixMinDurationLabel, 90, center: true)
            .WithBindEnabled(nameof(vm.FixMinDurationMs));
        numericUpDownMinDuration.ValueChanged += (s, e) => vm.SetChanged();
        var switchDoNotGoPastShotChange = EditorChrome.MakeToggleRow(nameof(vm.DoNotGoPastShotChange), Se.Language.Tools.ApplyDurationLimits.DoNotGoPastShotChange, out var toggleDoNotGoPastShotChange)
            .WithBindIsEnabled(nameof(vm.FixMinDurationMs))
            .WithMarginLeft(14)
            .BindIsVisible(vm, nameof(vm.IsDoNotGoPastShotChangeVisible));
        toggleDoNotGoPastShotChange.IsCheckedChanged += (s, e) => vm.SetChanged();
        var panelMin = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { numericUpDownMinDuration, switchDoNotGoPastShotChange },
        };

        var switchFixMaxDuration = EditorChrome.MakeToggleRow(nameof(vm.FixMaxDurationMs), vm.FixMaxDurationLabel, out var toggleFixMaxDuration);
        toggleFixMaxDuration.IsCheckedChanged += (s, e) => vm.SetChanged();
        var numericUpDownMaxDuration = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(1, 10000, 1000, 90, vm, nameof(vm.MaxDurationMsOrFrames)), vm.FixMaxDurationLabel, 90, center: true)
            .WithBindEnabled(nameof(vm.FixMaxDurationMs));
        numericUpDownMaxDuration.ValueChanged += (s, e) => vm.SetChanged();

        grid.Add(switchFixMinDuration, 0, 0);
        grid.Add(panelMin, 0, 1);

        grid.Add(switchFixMaxDuration, 1, 0);
        grid.Add(numericUpDownMaxDuration, 1, 1);

        return grid;
    }

    private static Border MakeFixesView(ApplyDurationLimitsViewModel vm)
    {
        var labelFixesAvailable = EditorChrome.MakeBoundSectionHeader(nameof(vm.FixesInfo));

        // No header-click sorting (the DataGrid's CanUserSortColumns is not carried
        // over): the fixes are previews in subtitle order. The DataGrid-era
        // DataGridCheckboxMultiSelect is replaced by native extended selection plus
        // TableViewExtras.AddSpaceToggle for the Space-toggles-checkbox piece.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Fixes;
        dataGrid.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Apply,
                CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = new FuncDataTemplate<ApplyDurationLimitItem>((item, _) =>
                    new Border
                    {
                        Background = Brushes.Transparent, // Prevents highlighting
                        Padding = new Thickness(4),
                        Child = new CheckBox
                        {
                            Focusable = false,
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ApplyDurationLimitItem.Apply)),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }),
                Width = new GridLength(80), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                Binding = new Binding(nameof(ApplyDurationLimitItem.Number)),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(60),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Fix,
                Binding = new Binding(nameof(ApplyDurationLimitItem.Fix)),
                Width = new GridLength(1, GridUnitType.Star),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            },
        });
        TableViewExtras.AddSpaceToggle<ApplyDurationLimitItem>(dataGrid,
            item => item.Apply, (item, v) => item.Apply = v);

        return EditorChrome.MakeFillSection(labelFixesAvailable, EditorChrome.MakeListFrame(dataGrid));
    }

    private static Border MakeCannotFixView(ApplyDurationLimitsViewModel vm)
    {
        var labelFixesAvailable = EditorChrome.MakeBoundSectionHeader(nameof(vm.FixesSkippedInfo));

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // No header-click sorting here either: lines that cannot be fixed, in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Subtitles;
        dataGrid.Columns.AddRange(new[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Width = new GridLength(60), // content-sized (Auto) on the DataGrid; TableView treats Auto as star
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
                Width = new GridLength(120),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
                Width = new GridLength(120),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Text,
                CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
                Width = new GridLength(1, GridUnitType.Star),
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            },
        });

        dataGrid.AddHandler(InputElement.KeyDownEvent, (object? _, KeyEventArgs e) =>
        {
            if (e.Key is Key.Home or Key.End && dataGrid.ItemsSource is IList items && items.Count > 0)
            {
                var target = e.Key == Key.Home ? items[0] : items[^1];
                if (target == null)
                {
                    return;
                }

                dataGrid.SelectedItem = target;
                dataGrid.ScrollIntoView(target);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        return EditorChrome.MakeFillSection(labelFixesAvailable, EditorChrome.MakeListFrame(dataGrid));
    }
}
