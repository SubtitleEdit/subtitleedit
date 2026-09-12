using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Optris.Icons.Avalonia;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;

public class RestoreAutoBackupWindow : Window
{
    public RestoreAutoBackupWindow(RestoreAutoBackupViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.RestoreAutoBackup.Title;
        Width = 810;
        Height = 640;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.File.RestoreAutoBackup;

        var subtitleGrid = MakeSubtitleGrid(vm);
        var settingsGrid = MakeSettingsGrid(vm);

        var tabControl = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Items =
            {
                new TabItem
                {
                    Header = MakeTabHeader(IconNames.ClosedCaption, l.Subtitles),
                    Content = MakeSubtitlesView(vm, subtitleGrid),
                },
                new TabItem
                {
                    Header = MakeTabHeader(IconNames.Settings, l.Settings),
                    Content = MakeSettingsView(vm, settingsGrid),
                },
            },
        };
        tabControl.Bind(TabControl.SelectedIndexProperty, new Binding(nameof(vm.SelectedTabIndex)) { Source = vm, Mode = BindingMode.TwoWay });

        var buttonOk = UiUtil.MakeButtonOk(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 4,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Children.Add(tabControl);
        Grid.SetRow(tabControl, 0);

        grid.Children.Add(panelButtons);
        Grid.SetRow(panelButtons, 1);

        Content = grid;

        // Initial focus on an input, not an action button - a focused button clicks on bare Space.
        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(vm.SelectedTabIndex == 1 ? settingsGrid : subtitleGrid); });
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TableView MakeSubtitleGrid(RestoreAutoBackupViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;

        // The DataGrid sized these columns to content (Auto); TableView treats Auto as
        // star, so the narrow columns get fixed widths and the file name is the star column.
        dataGrid.Columns.AddRange(new[]
        {
                MakeDateColumn(),
                new SeTableViewColumn
                {
                    Header = Se.Language.General.FileName,
                    Binding = new Binding(nameof(DisplayFile.FileName)),
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                },
                MakeExtensionColumn(),
                MakeSizeColumn(),
        });

        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.Files)));
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedFile)));
        dataGrid.SelectionChanged += vm.GridSelectionChanged;
        return dataGrid;
    }

    private static TableView MakeSettingsGrid(RestoreAutoBackupViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false);
        dataGrid.Width = double.NaN;
        dataGrid.Height = double.NaN;
        dataGrid.DataContext = vm;

        dataGrid.Columns.AddRange(new[]
        {
                MakeDateColumn(),
                new SeTableViewColumn
                {
                    Header = Se.Language.General.FileName,
                    Binding = new Binding(nameof(DisplayFile.FileName)),
                    Width = new GridLength(1, GridUnitType.Star),
                    CellTheme = UiUtil.TableViewCellTheme,
                    HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
                },
                MakeExtensionColumn(),
                MakeSizeColumn(),
        });

        dataGrid.Bind(TableView.ItemsSourceProperty, new Binding(nameof(vm.SettingsFiles)));
        dataGrid.Bind(TableView.SelectedItemProperty, new Binding(nameof(vm.SelectedSettingsFile)));
        dataGrid.SelectionChanged += vm.SettingsGridSelectionChanged;
        return dataGrid;
    }

    private static Control MakeSubtitlesView(RestoreAutoBackupViewModel vm, TableView dataGrid)
    {
        var l = Se.Language.File.RestoreAutoBackup;

        var linkOpenFolder = UiUtil.MakeLink(l.OpenAutoBackupFolder, vm.OpenFolderCommand);
        var summaryText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            Margin = new Thickness(10, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.Files)}.{nameof(vm.Files.Count)}")
            {
                StringFormat = l.XBackups
            }
        };

        var buttonDeleteAll = UiUtil.MakeButton(l.DeleteAll, vm.DeleteAllFilesCommand)
            .WithBindIsVisible(nameof(vm.IsEmptyFilesVisible))
            .WithIconLeft("fa-solid fa-trash");
        var buttonRestore = UiUtil.MakeButton(l.RestoreAutoBackupFile, vm.RestoreFileCommand)
            .WithIconLeft("fa-solid fa-clock-rotate-left");
        buttonRestore.BindIsEnabled(vm, nameof(vm.IsOkButtonEnabled));

        return MakeTabLayout(dataGrid, null, linkOpenFolder, summaryText, buttonDeleteAll, buttonRestore);
    }

    private static Control MakeSettingsView(RestoreAutoBackupViewModel vm, TableView dataGrid)
    {
        var l = Se.Language.File.RestoreAutoBackup;

        var icon = new ContentControl
        {
            Width = 18,
            Height = 18,
            Opacity = 0.8,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 1, 0, 0),
        };
        Attached.SetIcon(icon, IconNames.Information);

        var infoText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SettingsBackupInfo)),
        };
        var lastBackupText = new TextBlock
        {
            Opacity = 0.75,
            FontSize = 12,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LastSettingsBackupText)),
        };

        var infoPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x18, 0x4c, 0x9c, 0xe8)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x50, 0x4c, 0x9c, 0xe8)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 10),
            Child = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
                ColumnSpacing = 10,
                Children =
                {
                    icon,
                    new StackPanel
                    {
                        Spacing = 3,
                        [Grid.ColumnProperty] = 1,
                        Children = { infoText, lastBackupText },
                    },
                },
            },
        };

        var linkOpenFolder = UiUtil.MakeLink(l.OpenSettingsBackupFolder, vm.OpenSettingsFolderCommand);
        var summaryText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            Margin = new Thickness(10, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding($"{nameof(vm.SettingsFiles)}.{nameof(vm.SettingsFiles.Count)}")
            {
                StringFormat = l.XBackups
            }
        };

        var buttonDeleteAll = UiUtil.MakeButton(l.DeleteAll, vm.DeleteAllSettingsFilesCommand)
            .WithBindIsVisible(nameof(vm.IsSettingsFilesVisible))
            .WithIconLeft("fa-solid fa-trash");
        var buttonBackupNow = UiUtil.MakeButton(l.BackUpSettingsNow, vm.BackupSettingsNowCommand)
            .WithIconLeft("fa-solid fa-floppy-disk");
        var buttonRestore = UiUtil.MakeButton(l.RestoreSettings, vm.RestoreSettingsCommand)
            .WithIconLeft("fa-solid fa-clock-rotate-left");
        buttonRestore.BindIsEnabled(vm, nameof(vm.IsRestoreSettingsEnabled));

        return MakeTabLayout(dataGrid, infoPanel, linkOpenFolder, summaryText, buttonDeleteAll, buttonBackupNow, buttonRestore);
    }

    /// <summary>Shared tab body: optional header, the list, then a bottom row with links left and actions right.</summary>
    private static Grid MakeTabLayout(TableView dataGrid, Control? header, Control link, Control summary, params Control[] buttons)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            Margin = new Thickness(0, 10, 0, 0),
            RowSpacing = 10,
            ColumnSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        if (header != null)
        {
            grid.Children.Add(header);
            Grid.SetRow(header, 0);
            Grid.SetColumnSpan(header, 2);
        }

        grid.Children.Add(dataGrid);
        Grid.SetRow(dataGrid, 1);
        Grid.SetColumnSpan(dataGrid, 2);

        var panelBottomLeft = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { link, summary },
        };
        grid.Children.Add(panelBottomLeft);
        Grid.SetRow(panelBottomLeft, 2);
        Grid.SetColumn(panelBottomLeft, 0);

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panelButtons.Children.AddRange(buttons);
        grid.Children.Add(panelButtons);
        Grid.SetRow(panelButtons, 2);
        Grid.SetColumn(panelButtons, 1);

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

    private static SeTableViewColumn MakeDateColumn() => new()
    {
        Header = Se.Language.General.DateAndTime,
        Binding = new Binding(nameof(DisplayFile.DateAndTime)),
        Width = new GridLength(160),
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
    };

    private static SeTableViewColumn MakeSizeColumn() => new()
    {
        Header = Se.Language.General.Size,
        Binding = new Binding(nameof(DisplayFile.Size)),
        Width = new GridLength(90),
        CellTheme = UiUtil.TableViewCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
    };

    private static SeTableViewColumn MakeExtensionColumn() => new()
    {
        Header = Se.Language.General.FileExtension,
        Width = new GridLength(110),
        CellTheme = UiUtil.TableViewNoPaddingCellTheme,
        HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        CellTemplate = new FuncDataTemplate<DisplayFile>((item, _) =>
        {
            if (item == null)
            {
                return new Border();
            }

            var color = GetExtensionColor(item.Extension);
            return new Border
            {
                Background = Brushes.Transparent,
                Padding = new Thickness(4, 2),
                Child = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(0x20, color.R, color.G, color.B)),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(7, 2),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = item.Extension,
                        FontSize = 12,
                        Foreground = new SolidColorBrush(color),
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                },
            };
        }),
    };

    private static readonly Color[] ExtensionPalette =
    {
        Color.FromRgb(0x5f, 0xc6, 0xd8), // cyan
        Color.FromRgb(0xb4, 0x8c, 0xe8), // violet
        Color.FromRgb(0xe8, 0xb0, 0x4c), // amber
        Color.FromRgb(0x6e, 0xcb, 0x87), // green
        Color.FromRgb(0xe8, 0x8c, 0xb0), // pink
        Color.FromRgb(0x4c, 0x9c, 0xe8), // blue
    };

    private static Color GetExtensionColor(string extension)
    {
        var hash = 0;
        foreach (var ch in extension.ToLowerInvariant())
        {
            hash = hash * 31 + ch;
        }

        return ExtensionPalette[System.Math.Abs(hash) % ExtensionPalette.Length];
    }
}
