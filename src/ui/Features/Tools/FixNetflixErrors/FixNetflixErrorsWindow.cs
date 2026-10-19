using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public class FixNetflixErrorsWindow : Window
{
    private readonly FixNetflixErrorsViewModel _vm;
    private ComboBox _comboBoxLanguage = null!;

    public FixNetflixErrorsWindow(FixNetflixErrorsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.NetflixCheckAndFix.Title);
        Width = 1100;
        Height = 680;
        MinWidth = 900;
        MinHeight = 640;
        CanResize = true;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettingsView(vm);
        var fixesView = MakeFixesView(vm);

        var buttonGenerateReport = UiUtil.MakeButton(Se.Language.Tools.NetflixCheckAndFix.GenerateReport, vm.GenerateReportCommand)
            .WithIconLeft(IconNames.Netflix);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        var summaryText = EditorChrome.MakeBoundHint(nameof(vm.FixesSummaryText));
        summaryText.FontSize = 12;

        var panelButtons = EditorChrome.MakeButtonBar(summaryText, buttonGenerateReport, UiUtil.MakeButtonCancel(vm.CancelCommand), buttonOk);

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(320, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        columns.Add(settingsView, 0, 0);
        columns.Add(new Border
        {
            Child = fixesView,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        Content = EditorChrome.MakeRoot(columns, panelButtons);

        UiUtil.FocusOnFirstActivation(this, _comboBoxLanguage); // initial focus on an input, not an action button - a focused button clicks on bare Space

        Closing += delegate { UiUtil.SaveWindowPosition(this); };
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
    }

    private Border MakeSettingsView(FixNetflixErrorsViewModel vm)
    {
        _comboBoxLanguage = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage));
        _comboBoxLanguage.HorizontalAlignment = HorizontalAlignment.Stretch;
        _comboBoxLanguage.Width = double.NaN;
        AutomationProperties.SetName(_comboBoxLanguage, Se.Language.General.Language);

        // Netflix allows higher reading speeds for SDH and requires lower ones for
        // children's programs, so both change which limits the checks run against.
        var panelTop = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.Language, _comboBoxLanguage, 80, spacing: 6),
                EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(nameof(vm.IsChildrenProgram), Se.Language.Tools.NetflixCheckAndFix.ChildrensProgram), Se.Language.Tools.NetflixCheckAndFix.ChildrensProgram),
                EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(nameof(vm.IsSdh), Se.Language.Tools.NetflixCheckAndFix.Sdh), Se.Language.Tools.NetflixCheckAndFix.Sdh),
            }
        };

        // Grid with list of checks. Sorting dropped in the DataGrid -> TableView
        // conversion: the checks run in list order (RunChecks gets them in collection
        // order), so the list must not be reordered.
        // Stretch (the MakeTableView default) instead of the DataGrid's Left/Top: the
        // star-sized Name column needs the control to fill its fixed-width grid cell.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid[!TableView.ItemsSourceProperty] = new Binding(nameof(vm.Checks));

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Enabled,
            CellTheme = UiUtil.TableViewNoPaddingCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            CellTemplate = new FuncDataTemplate<NetflixCheckDisplayItem>((item, _) =>
            {
                var cb = new CheckBox
                {
                    Focusable = false,
                    [!ToggleButton.IsCheckedProperty] = new Binding(nameof(NetflixCheckDisplayItem.IsSelected)) { Mode = BindingMode.TwoWay },
                    [!AutomationProperties.NameProperty] = new Binding(nameof(NetflixCheckDisplayItem.Name)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                cb.IsCheckedChanged += (_, __) => vm.SetDirty();
                return new Border
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(1),
                    Child = cb
                };
            }),
            // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
            Width = new GridLength(70)
        });
        dataGrid.WithAccessibleName(Se.Language.General.Rules); // the list of checks has no heading (#12087)

        dataGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Name,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(NetflixCheckDisplayItem.Name)),
            Width = new GridLength(1, GridUnitType.Star)
        });

        // Extended selection is native ListBox behavior on TableView; only the
        // Space-toggles-checkbox piece of the old CheckboxMultiSelect needs wiring.
        TableViewExtras.AddSpaceToggle<NetflixCheckDisplayItem>(dataGrid,
            item => item.IsSelected, (item, v) => item.IsSelected = v);

        var checksActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                EditorChrome.CreateToolButton("fa-solid fa-square-check", Se.Language.General.SelectAll, vm.ChecksSelectAllCommand, 26),
                EditorChrome.CreateToolButton("fa-solid fa-right-left", Se.Language.General.InvertSelection, vm.ChecksInverseSelectionCommand, 26),
            },
        };
        var checksHeader = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(checksActions, Dock.Right);
        checksHeader.Children.Add(checksActions);
        checksHeader.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Rules));

        var checksPanel = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = new Thickness(14, 10, 14, 14),
        };
        checksPanel.Add(checksHeader, 0);
        checksPanel.Add(EditorChrome.MakeListFrame(dataGrid, clearListBackground: true), 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };

        grid.Add(EditorChrome.MakeEditorSection(Se.Language.General.Settings, panelTop, first: true), 0, 0);
        grid.Add(new Border
        {
            Child = checksPanel,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        }, 1, 0);

        return EditorChrome.MakePanel(grid);
    }

    private Border MakeFixesView(FixNetflixErrorsViewModel vm)
    {
        // Sorting dropped in the DataGrid -> TableView conversion: the grid previews
        // fixes in subtitle order.
        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = _vm;
        dataGrid.ItemsSource = _vm.Fixes;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Apply,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        var cb = new CheckBox
                        {
                            Focusable = false,
                            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(FixNetflixErrorsItem.Apply)) { Mode = BindingMode.TwoWay },
                            [AutomationProperties.NameProperty] = Se.Language.General.Apply,
                            HorizontalAlignment = HorizontalAlignment.Center,
                        };
                        cb.IsEnabled = item.CanBeFixed;

                        return new Border
                        {
                            Background = Brushes.Transparent, // Prevents highlighting
                            Padding = new Thickness(4),
                            Child = cb,
                        };
                    }),
                    // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                    Width = new GridLength(70)
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.IndexDisplay)),
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Before,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    NameBinding = new Binding(nameof(FixNetflixErrorsItem.Before)),
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var (beforeBlock, _) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = beforeBlock,
                        };
                    }),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.After,
                    CellTheme = UiUtil.TableViewNoPaddingCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    NameBinding = new Binding(nameof(FixNetflixErrorsItem.After)),
                    CellTemplate = new FuncDataTemplate<FixNetflixErrorsItem>((item, _) =>
                    {
                        if (item == null)
                        {
                            return new Border();
                        }

                        var (_, afterBlock) = TextDiffHighlighter.CompareReplacement(item.Before, item.After);
                        return new Border
                        {
                            Background = Brushes.Transparent,
                            Padding = new Thickness(4),
                            Child = afterBlock,
                        };
                    }),
                    Width = new GridLength(1, GridUnitType.Star),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Reason,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(FixNetflixErrorsItem.Reason)),
                    Width = new GridLength(1, GridUnitType.Star),
                },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(_vm.SelectedFix)));
        dataGrid.WithAccessibleName(Se.Language.Tools.FixCommonErrors.Fixes); // no heading above the fixes list (#12087)

        // Extended selection is native ListBox behavior on TableView; only the
        // Space-toggles-checkbox piece of the old CheckboxMultiSelect needs wiring.
        // Non-fixable rows count as "checked" so they never block the all-checked
        // toggle, and the setter leaves them alone (the old helper's canToggle).
        TableViewExtras.AddSpaceToggle<FixNetflixErrorsItem>(dataGrid,
            item => !item.CanBeFixed || item.Apply,
            (item, v) =>
            {
                if (item.CanBeFixed)
                {
                    item.Apply = v;
                }
            });

        var fixesPanel = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = new Thickness(14, 12, 14, 14),
        };
        var fixesHeader = EditorChrome.MakeSectionHeader(Se.Language.Tools.FixCommonErrors.Fixes);
        fixesHeader.Margin = new Thickness(0, 0, 0, 10);
        fixesPanel.Add(fixesHeader, 0);
        fixesPanel.Add(EditorChrome.MakeListFrame(dataGrid, clearListBackground: true), 1);

        return EditorChrome.MakePanel(fixesPanel);
    }

    protected override void OnKeyDown(KeyEventArgs e)
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
