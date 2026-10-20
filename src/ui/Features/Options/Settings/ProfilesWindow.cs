using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class ProfilesWindow : Window
{
    private const double LabelWidth = 170;

    public ProfilesWindow(ProfilesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Profiles;
        CanResize = true;
        Width = 1100;
        Height = 750;
        MinWidth = 800;
        MinHeight = 560;
        SettingsChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // The rules of the selected profile on the side; they scroll on a short screen, so the
        // button bar stays reachable.
        var sideScroller = new ScrollViewer
        {
            Content = EditorChrome.MakeEditorSection(Se.Language.General.Rules, MakeControlsGrid(vm), first: true),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        var side = new Border
        {
            Child = sideScroller,
            Background = EditorChrome.PanelBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(440) },
            },
        };
        content.Add(MakeDataGrid(vm, out var dataGrid), 0, 0);
        content.Add(side, 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        grid.Add(content, 0);
        grid.Add(SettingsChrome.MakeButtonBar(null, buttonCancel, buttonOk), 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(dataGrid); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Grid MakeDataGrid(ProfilesViewModel vm, out TableView tableView)
    {
        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // the caller takes result.Profiles in collection order and saves it back to the
        // settings, so reordering the backing collection would reorder the saved profiles.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.DataContext = vm;
        AutomationProperties.SetName(dataGrid, Se.Language.General.Profiles);
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.Name,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ProfileDisplay.Name)),
                // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.Options.Settings.SingleLineMaxLength,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ProfileDisplay.SingleLineMaxLength)),
                Width = new GridLength(150),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.MaxCharactersPerSecond,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(ProfileDisplay.MaxCharsPerSec)),
                Width = new GridLength(150),
            },
        });
        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Profiles)) { Source = vm });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedProfile)) { Source = vm });

        tableView = dataGrid;

        // List actions as icon buttons in the list header
        var buttonNew = EditorChrome.CreateToolButton("fa-solid fa-plus", Se.Language.General.NewProfile, vm.NewCommand, 26);
        var buttonCopy = EditorChrome.CreateToolButton("fa-regular fa-copy", Se.Language.General.Copy, vm.CopyCommand, 26);
        var buttonDelete = EditorChrome.CreateToolButton("fa-solid fa-minus", Se.Language.General.Delete, vm.DeleteCommand, 26);
        var buttonClear = EditorChrome.CreateToolButton("fa-solid fa-trash", Se.Language.General.Clear, vm.ClearCommand, 26);
        var buttonImport = EditorChrome.CreateToolButton("fa-solid fa-file-import", Se.Language.General.ImportDotDotDot, vm.ImportCommand, 26);
        var buttonExport = EditorChrome.CreateToolButton("fa-solid fa-file-export", Se.Language.General.ExportDotDotDot, vm.ExportCommand, 26);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        actions.Children.Add(buttonNew);
        actions.Children.Add(buttonCopy);
        actions.Children.Add(buttonDelete);
        actions.Children.Add(buttonClear);
        actions.Children.Add(EditorChrome.MakeToolbarSeparator());
        actions.Children.Add(buttonImport);
        actions.Children.Add(buttonExport);

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(actions, Dock.Right);
        header.Children.Add(actions);
        header.Children.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Profiles));

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

    private static StackPanel MakeControlsGrid(ProfilesViewModel vm)
    {
        var textBoxName = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.Name));
        textBoxName.HorizontalAlignment = HorizontalAlignment.Stretch;

        var numericUpDownSingleLineMaxLength = UiUtil.MakeNumericUpDownInt(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.SingleLineMaxLength));
        var numericUpDownOptimalCharsPerSec = UiUtil.MakeNumericUpDownDouble(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.OptimalCharsPerSec));
        var numericUpDownMaxCharsPerSec = UiUtil.MakeNumericUpDownDouble(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MaxCharsPerSec));
        var numericUpDownMaxWordsPerMin = UiUtil.MakeNumericUpDownDouble(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MaxWordsPerMin));
        var numericUpDownMinDurationMs = UiUtil.MakeNumericUpDownInt(0, 10000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MinDurationMs));
        var numericUpDownMaxDurationMs = UiUtil.MakeNumericUpDownInt(0, 10000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MaxDurationMs));
        var numericUpDownMinGapMs = UiUtil.MakeNumericUpDownInt(0, 10000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MinGapMs));
        var numericUpDownMaxLines = UiUtil.MakeNumericUpDownInt(1, 10, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.MaxLines));
        var numericUpDownUnbreakLinesShorterThan = UiUtil.MakeNumericUpDownInt(0, 1000, 43, 150, vm, nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.UnbreakLinesShorterThan));

        var comboBoxDialogStyle = new ComboBox
        {
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.DialogStyles)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.DialogStyle)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };

        var comboBoxContinuationStyle = new ComboBox
        {
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.ContinuationStyles)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.ContinuationStyle)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };

        var comboBoxCpsLineLengthStyle = new ComboBox
        {
            DataContext = vm,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.CpsLineLengthStrategies)),
            [!SelectingItemsControl.SelectedItemProperty] =
                new Binding(nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplay.CpsLineLengthStrategy)) { Mode = BindingMode.TwoWay },
            ItemTemplate = new FuncDataTemplate<FormatViewModel>((f, _) =>
                new TextBlock { Text = f?.Name }, true)
        };

        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                Row(Se.Language.General.Name, textBoxName),
                Row(Se.Language.Options.Settings.SingleLineMaxLength, numericUpDownSingleLineMaxLength),
                Row(Se.Language.Options.Settings.OptimalCharsPerSec, numericUpDownOptimalCharsPerSec),
                Row(Se.Language.General.MaxCharactersPerSecond, numericUpDownMaxCharsPerSec),
                Row(Se.Language.Options.Settings.MaxWordsPerMin, numericUpDownMaxWordsPerMin),
                Row(Se.Language.Options.Settings.MinDurationMs, numericUpDownMinDurationMs),
                Row(Se.Language.Options.Settings.MaxDurationMs, numericUpDownMaxDurationMs),
                Row(Se.Language.Options.Settings.MinGapMs, numericUpDownMinGapMs),
                Row(Se.Language.General.MaxNumberOfLines, numericUpDownMaxLines),
                Row(Se.Language.Options.Settings.UnbreakSubtitlesShortThan, numericUpDownUnbreakLinesShorterThan),
                Row(Se.Language.Options.Settings.DialogStyle, comboBoxDialogStyle),
                Row(Se.Language.Options.Settings.ContinuationStyle, comboBoxContinuationStyle),
                Row(Se.Language.Options.Settings.CpsLineLengthStyle, comboBoxCpsLineLengthStyle),
            }
        };
    }

    /// <summary>
    /// "Label  [field]" with the label in the label column; the field is named after the label.
    /// </summary>
    private static Grid Row(string label, Control field)
    {
        var text = SettingsChrome.MakeLabel(label);
        SettingsChrome.Compact(field);
        if (field is ComboBox)
        {
            field.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        AccessibleLabels.LinkToLabel(field, text);

        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(field, 0, 1);
        return grid;
    }
}
