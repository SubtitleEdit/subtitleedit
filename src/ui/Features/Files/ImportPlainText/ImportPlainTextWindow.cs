using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Platform.Progress;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public class ImportPlainTextWindow : Window
{
    public ImportPlainTextWindow(ImportPlainTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Import.TitleImportPlainText;
        CanResize = true;
        Width = 1200;
        Height = 850;
        MinWidth = 1100;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var buttonImportFiles = EditorChrome.MakeTextToolButton("fa-regular fa-folder-open", Se.Language.File.Import.ImportFilesDotDotDot, vm.FilesImportCommand);
        buttonImportFiles.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm });
        var buttonImportFile = EditorChrome.MakeTextToolButton("fa-regular fa-folder-open", Se.Language.General.ImportDotDotDot, vm.FileImportCommand);
        buttonImportFile.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm, Converter = InverseBooleanConverter.Instance });
        var rowImportFiles = EditorChrome.MakeToggleRow(nameof(vm.IsImportFilesVisible), Se.Language.File.Import.MultipleFiles, out var switchImportFiles);
        switchImportFiles.IsCheckedChanged += (s, e) => vm.CheckBoxImportFilesChanged();
        var panelImport = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                rowImportFiles,
                buttonImportFiles,
                buttonImportFile,
            },
            Spacing = 14,
        };

        var labelNumberOfSubtitles = EditorChrome.MakeDimText();
        labelNumberOfSubtitles.DataContext = vm;
        labelNumberOfSubtitles.Bind(TextBlock.TextProperty, new Binding($"{nameof(vm.Subtitles)}.{nameof(vm.Subtitles.Count)}")
        {
            StringFormat = Se.Language.File.Import.NumberOfSubtitlesX,
        });

        // Progress sits above the button bar; forced alignment of a long video runs for
        // minutes, so the window must show that something is happening.
        var labelAlignProgress = EditorChrome.MakeDimText();
        labelAlignProgress.DataContext = vm;
        labelAlignProgress.Bind(TextBlock.TextProperty, new Binding(nameof(vm.AlignProgress)));

        // Alignment on a long video runs for minutes over many chunks, so show how far it
        // has actually got rather than just that something is happening.
        var progressBarAlign = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Width = 220,
            Height = 6,
            MinHeight = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
        };
        progressBarAlign.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.AlignPercent)));
        progressBarAlign.WithPlatformProgress(this, nameof(vm.IsAligning));

        var panelAlignProgress = new DockPanel();
        DockPanel.SetDock(progressBarAlign, Dock.Left);
        panelAlignProgress.Children.Add(progressBarAlign);
        panelAlignProgress.Children.Add(labelAlignProgress);
        var progressStrip = new Border
        {
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
            Child = panelAlignProgress,
        };
        progressStrip.Bind(IsVisibleProperty, new Binding(nameof(vm.IsAligning)));

        // While aligning, Cancel stops the alignment; nothing else may start or close the window
        // with lines the aligner is still writing to.
        var buttonAlignViaWhisper = EditorChrome.MakeTextToolButton("fa-solid fa-microphone", Se.Language.File.Import.AlignViaSpeechToText, vm.AlignScriptWithTimestampsFromWhisperCommand)
            .WithBindEnabled(nameof(vm.IsAligning), InverseBooleanConverter.Instance);
        var buttonAlignViaForcedAligner = EditorChrome.MakeTextToolButton(IconNames.Waveform, Se.Language.File.Import.AlignViaForcedAligner, vm.AlignScriptWithForcedAlignerCommand)
            .WithBindEnabled(nameof(vm.IsAligning), InverseBooleanConverter.Instance);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand)
            .WithBindEnabled(nameof(vm.IsAligning), InverseBooleanConverter.Instance);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelAlign = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonAlignViaForcedAligner, buttonAlignViaWhisper },
        };

        var sections = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 180 },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 150 },
            },
        };
        sections.Add(MakeTextBoxAndControlsView(vm, panelImport), 0);
        sections.Add(MakeSubtitleGridView(vm, panelAlign), 1);

        buttonOk.Classes.Add("primary");
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)),
                new RowDefinition(GridLength.Auto), // progress, while aligning
                new RowDefinition(GridLength.Auto), // buttons
            },
        };
        root.Add(new Border { Background = EditorChrome.PanelBrush, Child = sections }, 0);
        root.Add(progressStrip, 1);
        root.Add(EditorChrome.MakeDockedButtonBar(labelNumberOfSubtitles, buttonCancel, buttonOk), 2);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, switchImportFiles); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Grid MakeTextBoxAndControlsView(ImportPlainTextViewModel vm, Control importActions)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };

        var textBox = EditorChrome.AsTextArea(new TextBox
        {
            IsReadOnly = false,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
        });
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PlainText)) { Mode = BindingMode.TwoWay });
        textBox.WithAccessibleName(Se.Language.General.Text); // the pasted plain text, no visible label (#12087)
        textBox.Bind(TextBox.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm, Converter = InverseBooleanConverter.Instance });
        textBox.TextChanged += (s, e) => vm.PlainTextChanged();
        var sizeConverter = new FileSizeConverter();

        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // the preview builds one subtitle line per file in collection order, so
        // reordering the backing collection would reorder the imported lines.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.SubtitleFiles);
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Files;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.FileName,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(DisplayFile.FileName)),
                // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                Width = new GridLength(1, GridUnitType.Star),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Size,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(DisplayFile.Size)) { Converter = sizeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(100),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedFile)) { Source = vm });

        // hack to make drag and drop work on the grid - also on empty rows
        var dropHost = new Border
        {
            Background = Brushes.Transparent,
            Child = dataGrid,
        };
        DragDrop.SetAllowDrop(dropHost, true);
        dropHost.AddHandler(DragDrop.DragOverEvent, vm.FileGridOnDragOver, RoutingStrategies.Bubble);
        dropHost.AddHandler(DragDrop.DropEvent, vm.FileGridOnDrop, RoutingStrategies.Bubble);
        dropHost.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm });

        var flyout = new MenuFlyout();
        dropHost.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dropHost);
        var menuItemImport = new MenuItem
        {
            Header = Se.Language.General.ImportDotDotDot,
            DataContext = vm,
            Command = vm.FilesImportCommand,
        };
        flyout.Items.Add(menuItemImport);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.Clear,
            DataContext = vm,
            Command = vm.FilesClearCommand,
        };
        flyout.Items.Add(menuItemClear);


        var source = new Grid();
        source.Children.Add(textBox);
        var filesFrame = EditorChrome.MakeListFrame(dropHost, clearListBackground: true);
        filesFrame.Bind(Border.IsVisibleProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm });
        source.Children.Add(filesFrame);

        grid.Add(EditorChrome.MakeFillSection(Se.Language.General.Text, source, importActions, first: true), 0, 0);
        grid.Add(MakeControlsView(vm), 0, 1);

        return grid;
    }

    private static Border MakeControlsView(ImportPlainTextViewModel vm)
    {
        var comboBoxSplit = new ComboBox
        {
            DataContext = vm,
            MinWidth = 160,
        }.WithAccessibleName(Se.Language.File.Import.SplitTextAt);
        comboBoxSplit.Bind(ComboBox.ItemsSourceProperty, new Binding(nameof(vm.SplitAtOptions)) { Source = vm });
        comboBoxSplit.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(vm.SelectedSplitAtOption)) { Source = vm, Mode = BindingMode.TwoWay });
        comboBoxSplit.Bind(ComboBox.IsEnabledProperty, new Binding(nameof(vm.IsImportFilesVisible)) { Source = vm, Converter = InverseBooleanConverter.Instance });
        comboBoxSplit.SelectionChanged += (s, e) => vm.SplitAtOptionChanged();

        var numericUpDownGap = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(0, 100000, Se.Settings.Tools.BridgeGaps.MinGapMs, 130, vm, nameof(vm.MinGapMs)), Se.Language.File.Import.GapMs.TrimEnd(':'), 100, alignLeft: true, center: true);
        numericUpDownGap.ValueChanged += vm.GapChanged;

        var rowUseFixedDuration = EditorChrome.MakeToggleRow(nameof(vm.UseFixedDuration), Se.Language.File.Import.UseFixedDuration, out _);
        var numericUpDownFixedDurationMs = EditorChrome.AsField(UiUtil.MakeNumericUpDownInt(0, 100000, (int)Se.Settings.Tools.AdjustDurations.AdjustDurationFixed, 130, vm, nameof(vm.FixedDurationMs)), Se.Language.File.Import.FixedDurationMs.TrimEnd(':'), 100, alignLeft: true, center: true);
        numericUpDownFixedDurationMs.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.UseFixedDuration)));

        var panel = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                EditorChrome.MakeFieldGrid((Se.Language.File.Import.SplitTextAt, comboBoxSplit), (Se.Language.File.Import.GapMs, numericUpDownGap)),
                rowUseFixedDuration,
                EditorChrome.MakeFieldGrid((Se.Language.File.Import.FixedDurationMs, numericUpDownFixedDurationMs)),
            },
        };

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Options, panel, first: true);
        section.BorderThickness = new Thickness(1, 0, 0, 0);
        section.MinWidth = 320;
        return section;
    }

    private static Border MakeSubtitleGridView(ImportPlainTextViewModel vm, Control alignActions)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        // No header sorting (the DataGrid's CanUserSortColumns is not carried over):
        // this is the subtitle preview, consumed by the caller in collection order.
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Preview);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Subtitles;
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
            new SeTableViewColumn
            {
                Header = Se.Language.General.NumberSymbol,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
                // Content-sized (Auto) on the DataGrid; TableView treats Auto as star.
                Width = new GridLength(60),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Show,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(130),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Hide,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(130),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Duration,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter, Mode = BindingMode.OneWay },
                Width = new GridLength(110),
            },
            new SeTableViewColumn
            {
                Header = Se.Language.General.Text,
                CellTheme = UiUtil.TableViewCellTheme,
                HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
                NameBinding = new Binding(nameof(SubtitleLineViewModel.Text)),
                Width = new GridLength(1, GridUnitType.Star),
            },
        });
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)) { Source = vm });

        return EditorChrome.MakeListSection(Se.Language.General.Preview, dataGrid, alignActions);
    }
}
