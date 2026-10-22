using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Data;
using Avalonia.Layout;
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        var subtitleGrid = MakeSubtitleGrid(vm);
        var settingsGrid = MakeSettingsGrid(vm);

        var tabControl = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(0),
            Items =
            {
                new TabItem
                {
                    Header = MakeTabHeader(IconNames.ClosedCaption, Se.Language.General.Subtitles),
                    Content = MakeSubtitlesView(vm, subtitleGrid),
                },
                new TabItem
                {
                    Header = MakeTabHeader(IconNames.Settings, Se.Language.General.Settings),
                    Content = MakeSettingsView(vm, settingsGrid),
                },
            },
        };
        tabControl.Bind(TabControl.SelectedIndexProperty, new Binding(nameof(vm.SelectedTabIndex)) { Source = vm, Mode = BindingMode.TwoWay });

        var buttonOk = UiUtil.MakeButtonOk(vm.CancelCommand);

        // The tabs on the panel color above the button bar
        var tabs = new Border
        {
            Padding = new Thickness(8, 4, 8, 0),
            Child = tabControl,
        };
        Content = EditorChrome.MakeRoot(tabs, EditorChrome.MakeDockedButtonBar(null, buttonOk));

        // Initial focus on an input, not an action button - a focused button clicks on bare Space.
        UiUtil.FocusOnFirstActivation(this, () => { TableViewExtras.FocusRow(vm.SelectedTabIndex == 1 ? settingsGrid : subtitleGrid); });
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static TableView MakeSubtitleGrid(RestoreAutoBackupViewModel vm)
    {
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Subtitles); // the tab header (#12087)
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
        var dataGrid = TableViewExtras.MakeTableView(multiSelect: false).WithAccessibleName(Se.Language.General.Settings);
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

        var buttonOpenFolder = EditorChrome.MakeTextToolButton("fa-regular fa-folder-open", l.OpenAutoBackupFolder, vm.OpenFolderCommand);
        var summaryText = MakeSummaryText($"{nameof(vm.Files)}.{nameof(vm.Files.Count)}");

        var buttonDeleteAll = EditorChrome.MakeTextToolButton("fa-solid fa-trash", l.DeleteAll, vm.DeleteAllFilesCommand)
            .WithBindIsVisible(nameof(vm.IsEmptyFilesVisible));
        var buttonRestore = UiUtil.MakeButton(l.RestoreAutoBackupFile, vm.RestoreFileCommand)
            .WithIconLeft("fa-solid fa-clock-rotate-left");
        buttonRestore.Classes.Add("primary");
        buttonRestore.BindIsEnabled(vm, nameof(vm.IsOkButtonEnabled));

        return MakeTabLayout(Se.Language.General.Subtitles, dataGrid, null, buttonOpenFolder, summaryText, buttonDeleteAll, buttonRestore);
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
            FontSize = EditorChrome.FieldFontSize,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SettingsBackupInfo)),
        };
        var lastBackupText = new TextBlock
        {
            Foreground = EditorChrome.DimTextBrush,
            FontSize = EditorChrome.FieldFontSize,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.LastSettingsBackupText)),
        };

        var infoPanel = new Border
        {
            Background = EditorChrome.AccentSoftBrush,
            CornerRadius = new CornerRadius(8),
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

        var buttonOpenFolder = EditorChrome.MakeTextToolButton("fa-regular fa-folder-open", l.OpenSettingsBackupFolder, vm.OpenSettingsFolderCommand);
        var summaryText = MakeSummaryText($"{nameof(vm.SettingsFiles)}.{nameof(vm.SettingsFiles.Count)}");

        var buttonDeleteAll = EditorChrome.MakeTextToolButton("fa-solid fa-trash", l.DeleteAll, vm.DeleteAllSettingsFilesCommand)
            .WithBindIsVisible(nameof(vm.IsSettingsFilesVisible));
        var buttonBackupNow = EditorChrome.MakeTextToolButton("fa-solid fa-floppy-disk", l.BackUpSettingsNow, vm.BackupSettingsNowCommand);
        var buttonRestore = UiUtil.MakeButton(l.RestoreSettings, vm.RestoreSettingsCommand)
            .WithIconLeft("fa-solid fa-clock-rotate-left");
        buttonRestore.Classes.Add("primary");
        buttonRestore.BindIsEnabled(vm, nameof(vm.IsRestoreSettingsEnabled));

        return MakeTabLayout(Se.Language.General.Settings, dataGrid, infoPanel, buttonOpenFolder, summaryText, buttonDeleteAll, buttonBackupNow, buttonRestore);
    }

    /// <summary>
    /// Shared tab body: optional info on top, the list section (open folder and the count in its
    /// header), then the actions with the restore button in the accent.
    /// </summary>
    private static Grid MakeTabLayout(string title, TableView dataGrid, Control? header, Control openFolder, Control summary, params Control[] buttons)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = new Thickness(6, 12, 6, 14),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        if (header != null)
        {
            grid.Add(header, 0, 0);
        }

        var headerRight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { summary },
        };
        var listHeader = new DockPanel { MinHeight = 26 };
        DockPanel.SetDock(headerRight, Dock.Right);
        listHeader.Children.Add(headerRight);
        listHeader.Children.Add(EditorChrome.MakeSectionHeader(title));

        var list = new Grid
        {
            RowSpacing = 8,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        list.Add(listHeader, 0, 0);
        list.Add(EditorChrome.MakeListFrame(dataGrid, clearListBackground: true), 1, 0);
        grid.Add(list, 1, 0);

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };
        panelButtons.Children.AddRange(buttons);
        var bottom = new DockPanel();
        DockPanel.SetDock(panelButtons, Dock.Right);
        bottom.Children.Add(panelButtons);
        openFolder.HorizontalAlignment = HorizontalAlignment.Left;
        bottom.Children.Add(openFolder);
        grid.Add(bottom, 2, 0);

        return grid;
    }

    private static TextBlock MakeSummaryText(string countPath)
    {
        var text = EditorChrome.MakeDimText();
        text.Bind(TextBlock.TextProperty, new Binding(countPath)
        {
            StringFormat = Se.Language.File.RestoreAutoBackup.XBackups,
        });
        return text;
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
        NameBinding = new Binding(nameof(DisplayFile.Extension)),
        CellTemplate = new FuncDataTemplate<DisplayFile>((item, _) =>
        {
            if (item == null)
            {
                return new Border();
            }

            var index = GetExtensionPaletteIndex(item.Extension);
            var color = ExtensionPalette[index];
            var textColor = UiTheme.IsDarkThemeEnabled() ? color : ExtensionPaletteLight[index];
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
                        FontSize = UiUtil.ScaledFontSize(12),
                        Foreground = new SolidColorBrush(textColor),
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

    // Darker shades of ExtensionPalette for the pill text on the light theme - the pastels are
    // tuned for a dark background and are hard to read on white (#15768).
    private static readonly Color[] ExtensionPaletteLight =
    {
        Color.FromRgb(0x00, 0x6b, 0x7d), // cyan
        Color.FromRgb(0x6a, 0x3d, 0xb8), // violet
        Color.FromRgb(0x8a, 0x5a, 0x00), // amber
        Color.FromRgb(0x1e, 0x7a, 0x34), // green
        Color.FromRgb(0xa8, 0x2e, 0x66), // pink
        Color.FromRgb(0x1a, 0x5f, 0xb4), // blue
    };

    private static int GetExtensionPaletteIndex(string extension)
    {
        var hash = 0;
        foreach (var ch in extension.ToLowerInvariant())
        {
            hash = hash * 31 + ch;
        }

        return System.Math.Abs(hash) % ExtensionPalette.Length;
    }
}
