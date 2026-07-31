using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.FontCollector;

public class FontCollectorWindow : Window
{
    private readonly FontCollectorViewModel _vm;

    public FontCollectorWindow(FontCollectorViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.FontCollectorTitle;
        CanResize = true;
        Width = 800;
        Height = 500;
        MinWidth = 600;
        MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var dataGrid = TableViewExtras.MakeTableView();
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.FontItems;

        var fontNameColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.FontName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(FontCollectorItem.FontName)),
            Width = new GridLength(180),
        };
        var usedInColumn = new SeTableViewColumn
        {
            Header = Se.Language.Assa.FontCollectorUsedIn,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(FontCollectorItem.UsedIn)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        var statusColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.Status,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(FontCollectorItem.Status)),
            Width = new GridLength(120),
        };
        var fontFilesColumn = new SeTableViewColumn
        {
            Header = Se.Language.Assa.FontCollectorFontFiles,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(FontCollectorItem.FileDisplay)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        dataGrid.Columns.Add(fontNameColumn);
        dataGrid.Columns.Add(usedInColumn);
        dataGrid.Columns.Add(statusColumn);
        dataGrid.Columns.Add(fontFilesColumn);

        // Header sorting is safe here: the font list is presentation-only (copying
        // fonts to a folder uses the found-file set, not the row order, and the
        // background scan holds item references, not indexes).
        var sorter = new TableViewHeaderSorter(dataGrid);
        sorter.AddSortable<FontCollectorItem, string>(fontNameColumn, x => x.FontName)
            .AddSortable<FontCollectorItem, string>(usedInColumn, x => x.UsedIn)
            .AddSortable<FontCollectorItem, string>(statusColumn, x => x.Status)
            .AddSortable<FontCollectorItem, string>(fontFilesColumn, x => x.FileDisplay);

        var statusText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusText)),
        };

        var buttonCopy = UiUtil.MakeButton(Se.Language.Assa.FontCollectorCopyFontsToFolderDotDotDot, vm.CopyFontsToFolderCommand);
        var buttonBar = UiUtil.MakeButtonBar(
            buttonCopy,
            UiUtil.MakeButtonDone(vm.CloseCommand));

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 5,
            ColumnSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(dataGrid, 0, 0, 1, 2);
        grid.Add(statusText, 1, 0);
        grid.Add(buttonBar, 1, 1);

        Content = grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
