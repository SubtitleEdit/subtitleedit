using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Translate;

public class CopyPasteTranslateWindow : Window
{
    public CopyPasteTranslateWindow(CopyPasteTranslateViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Translate.TranslateViaCopyPaste;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        grid.Add(MakeControlsView(vm), 0);
        grid.Add(MakeSubtitlesView(vm), 1);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { TableViewExtras.FocusRow(vm.SubtitleGrid); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private StackPanel MakeControlsView(CopyPasteTranslateViewModel vm)
    {
        var labelMaxBlockSize = new TextBlock
        {
            Text = Se.Language.Translate.MaxBlockSize,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownMaxBlockSize = UiUtil.MakeNumericUpDownInt(100, 500_000, 5000, 125, vm, nameof(vm.MaxBlockSize));

        var labelLineSeparator = new TextBlock
        {
            Text = Se.Language.Translate.LineSeparator,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };
        var textBoxLineSeparator = new TextBox
        {
            Width = 80,
        };
        textBoxLineSeparator.Bind(TextBox.TextProperty, new Binding(nameof(vm.LineSeparator))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });

        var buttonTranslate = UiUtil.MakeButton(Se.Language.General.Translate, vm.TranslateCommand)
            .WithMarginLeft(10);

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 5,
            Children =
            {
                labelMaxBlockSize,
                numericUpDownMaxBlockSize,
                labelLineSeparator,
                textBoxLineSeparator,
                buttonTranslate,
            }
        };

        return panel;
    }

    private static Border MakeSubtitlesView(CopyPasteTranslateViewModel vm)
    {
        vm.SubtitleGrid = TableViewExtras.MakeTableView(multiSelect: false);
        vm.SubtitleGrid.Height = double.NaN; // auto size inside scroll viewer
        vm.SubtitleGrid.Margin = new Thickness(2);
        vm.SubtitleGrid.ItemsSource = vm.Subtitles;
        vm.SubtitleGrid.DataContext = vm.Subtitles;

        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();

        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            Width = new GridLength(50),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter },
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Duration,
            Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter },
            Width = new GridLength(120),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Text,
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.OriginalText)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.SubtitleGrid.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.Translation,
            CellTemplate = TableViewExtras.MakeTextCellTemplate(nameof(SubtitleLineViewModel.Text)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });

        vm.SubtitleGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle))
        {
            Source = vm,
            Mode = BindingMode.TwoWay
        });

        return UiUtil.MakeBorderForControl(vm.SubtitleGrid);
    }
}
