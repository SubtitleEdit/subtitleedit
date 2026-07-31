using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.PickVobSubLanguage;

public class PickVobSubLanguageWindow : Window
{
    public PickVobSubLanguageWindow(PickVobSubLanguageViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.WindowTitle;
        Width = 1024;
        Height = 600;
        MinWidth = 800;
        MinHeight = 500;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var languagesView = MakeLanguagesView(vm);
        var previewView = MakePreviewView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(languagesView, 0);
        grid.Add(previewView, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        AddHandler(KeyDownEvent, vm.OnKeyDownHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: false);

        Loaded += (_, _) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                vm.SelectAndScrollToRow(0);
                TableViewExtras.FocusRow(vm.LanguagesGrid);
            }, DispatcherPriority.Input);
        };
    }

    private static Border MakeLanguagesView(PickVobSubLanguageViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Languages;

        // Content-sized (Auto) on the DataGrid; TableView treats Auto as star, so the
        // stream-id and count columns get fixed widths (Language keeps the star).
        var streamIdColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(VobSubLanguageDisplay.StreamIdHex)),
            Width = new GridLength(80),
        };
        var languageColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Language,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(VobSubLanguageDisplay.Language)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var countColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Count,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(VobSubLanguageDisplay.Count)),
            Width = new GridLength(80),
        };

        dataGrid.Columns.Add(streamIdColumn);
        dataGrid.Columns.Add(languageColumn);
        dataGrid.Columns.Add(countColumn);

        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedLanguage)));
        dataGrid.SelectionChanged += vm.LanguagesGridSelectionChanged;
        dataGrid.DoubleTapped += (_, _) => vm.OkCommand.Execute(null);
        vm.LanguagesGrid = dataGrid;

        // Language list order is presentation-only (OK uses the selected item), so the
        // in-place header sorter is safe. The hex column sorts by the numeric stream id.
        new TableViewHeaderSorter(dataGrid)
            .AddSortable<VobSubLanguageDisplay, int>(streamIdColumn, x => x.StreamId)
            .AddSortable<VobSubLanguageDisplay, string>(languageColumn, x => x.Language)
            .AddSortable<VobSubLanguageDisplay, int>(countColumn, x => x.Count);

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }

    private static Border MakePreviewView(PickVobSubLanguageViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.Rows;

        // No sorter here: the preview shows subtitle cues in subtitle order (the old
        // DataGrid had sorting disabled too).
        dataGrid.Columns.AddRange(new TableViewColumn[]
        {
                new SeTableViewColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(VobSubLanguageCueDisplay.Number)),
                    Width = new GridLength(60),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(VobSubLanguageCueDisplay.Show)) { Converter = fullTimeConverter },
                    Width = new GridLength(120),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Duration,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Binding = new Binding(nameof(VobSubLanguageCueDisplay.Duration)) { Converter = shortTimeConverter },
                    Width = new GridLength(90),
                },
                new SeTableViewColumn
                {
                    Header = Se.Language.General.Image,
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<VobSubLanguageCueDisplay>((item, _) =>
                    {
                        if (item.Image == null)
                        {
                            return new TextBlock();
                        }

                        return new Image
                        {
                            Source = item.Image.Source,
                            MaxHeight = 100,
                            MaxWidth = 300,
                            Stretch = Avalonia.Media.Stretch.Uniform,
                        };
                    }),
                },
        });

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
