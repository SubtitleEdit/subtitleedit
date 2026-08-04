using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Optris.Icons.Avalonia;

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

        var tabControl = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Items =
            {
                new TabItem
                {
                    Header = MakeTabHeader(IconNames.ClosedCaption, Se.Language.Assa.FontCollectorCurrentSubtitle),
                    Content = MakeCurrentSubtitleView(vm),
                },
                new TabItem
                {
                    Header = MakeTabHeader(IconNames.CaseSensitiveAlt, Se.Language.Tools.PickFontNameInstalledFonts),
                    Content = MakeInstalledFontsView(vm),
                },
                new TabItem
                {
                    Header = MakeTabHeader(IconNames.Folder, Se.Language.Tools.PickFontNameCollectedFonts),
                    Content = MakeCollectedFontsView(vm),
                },
            },
        };
        tabControl.Bind(TabControl.SelectedIndexProperty, new Binding(nameof(vm.SelectedTabIndex)) { Source = vm, Mode = BindingMode.TwoWay });

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
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(tabControl, 0);
        grid.Add(UiUtil.MakeButtonBar(UiUtil.MakeButtonDone(vm.CloseCommand)), 1);

        Content = grid;
    }

    /// <summary>The fonts the current subtitle uses, with found/not-found status and copy actions.</summary>
    private static Grid MakeCurrentSubtitleView(FontCollectorViewModel vm)
    {
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
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedFontItem)) { Source = vm });
        dataGrid.SelectionChanged += vm.FontItemsGridSelectionChanged;

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

        var buttonEmbedFonts = UiUtil.MakeButton(Se.Language.Assa.FontCollectorEmbedFontsDotDotDot, vm.EmbedFontsInSubtitleCommand)
            .WithIconLeft(IconNames.Paperclip);
        var buttonCopyToSeFolder = UiUtil.MakeButton(Se.Language.Assa.FontCollectorCopyFontsToSeFontsFolder, vm.CopyFontsToSeFontsFolderCommand)
            .WithIconLeft(IconNames.FormatFont);
        var buttonBar = UiUtil.MakeButtonBar(buttonEmbedFonts, buttonCopyToSeFolder);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowSpacing = 5,
            ColumnSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(dataGrid, 0, 0, 1, 2);
        grid.Add(statusText, 1, 0);
        grid.Add(buttonBar, 1, 1);
        grid.Add(MakeFontPreviewView(vm), 2, 0, 1, 2);

        return grid;
    }

    /// <summary>All fonts installed on this machine, with a sample-text preview.</summary>
    private static Grid MakeInstalledFontsView(FontCollectorViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.InstalledFontNames;

        var fontNameColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.FontName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding("."),
            Width = new GridLength(1, GridUnitType.Star),
        };
        dataGrid.Columns.Add(fontNameColumn);
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedInstalledFontName)));

        var buttonCopyToSeFolder = UiUtil.MakeButton(Se.Language.Assa.FontCollectorCopyFontsToSeFontsFolder, vm.CopyInstalledFontToSeFontsFolderCommand)
            .WithIconLeft(IconNames.FormatFont);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 0);
        grid.Add(UiUtil.MakeButtonBar(buttonCopyToSeFolder), 1);
        grid.Add(MakeFontPreviewView(vm), 2);

        return grid;
    }

    /// <summary>The fonts collected in SE's own Fonts folder, with a sample-text preview.</summary>
    private static Grid MakeCollectedFontsView(FontCollectorViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.DataContext = vm;
        dataGrid.ItemsSource = vm.CollectedFonts;

        var fontNameColumn = new SeTableViewColumn
        {
            Header = Se.Language.General.FontName,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(CollectedFont.Name)),
            Width = new GridLength(220),
        };
        var fileColumn = new SeTableViewColumn
        {
            Header = Se.Language.Assa.FontCollectorFontFiles,
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
            Binding = new Binding(nameof(CollectedFont.FilePath)),
            Width = new GridLength(1, GridUnitType.Star),
        };
        dataGrid.Columns.Add(fontNameColumn);
        dataGrid.Columns.Add(fileColumn);
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedCollectedFont)));
        dataGrid.KeyDown += vm.CollectedFontsGridKeyDown;

        var flyout = new MenuFlyout();
        flyout.Opening += vm.CollectedFontsContextMenuOpening;
        dataGrid.ContextFlyout = flyout;
        UiUtil.AttachMacContextFlyoutHandler(dataGrid);

        var menuItemDelete = new Avalonia.Controls.MenuItem
        {
            Header = Se.Language.General.Delete,
            DataContext = vm,
            Command = vm.DeleteCollectedFontCommand,
        };
        menuItemDelete.Bind(Avalonia.Controls.MenuItem.IsVisibleProperty, new Binding(nameof(vm.IsDeleteCollectedFontVisible)) { Source = vm });
        flyout.Items.Add(menuItemDelete);

        var buttonImportFont = UiUtil.MakeButton(Se.Language.Assa.FontCollectorImportFontDotDotDot, vm.ImportFontCommand)
            .WithIconLeft(IconNames.Import);
        var buttonOpenFolder = UiUtil.MakeButton(Se.Language.Assa.FontCollectorOpenFontsFolder, vm.OpenSeFontsFolderCommand)
            .WithIconLeft(IconNames.FolderOpen);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 0);
        grid.Add(UiUtil.MakeButtonBar(buttonImportFont, buttonOpenFolder), 1);
        grid.Add(MakeFontPreviewView(vm), 2);

        return grid;
    }

    private static StackPanel MakeTabHeader(string iconName, string text)
    {
        var icon = new ContentControl { VerticalAlignment = VerticalAlignment.Center };
        Attached.SetIcon(icon, iconName);

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                icon,
                new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center },
            },
        };
    }

    private static Border MakeFontPreviewView(FontCollectorViewModel vm)
    {
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.FontPreview)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        return UiUtil.MakeBorderForControl(image);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
