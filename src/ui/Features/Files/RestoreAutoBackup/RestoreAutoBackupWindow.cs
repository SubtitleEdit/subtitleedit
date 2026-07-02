using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Data;
using Avalonia.Layout;
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

        var dataGrid = new DataGrid
        {
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            SelectionMode = DataGridSelectionMode.Single,
            IsReadOnly = true,
            DataContext = vm,
            AutoGenerateColumns = false,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.DateAndTime,
                    Binding = new Binding(nameof(DisplayFile.DateAndTime)),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileName,
                    Binding = new Binding(nameof(DisplayFile.FileName)),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTemplateColumn
                {
                    Header = Se.Language.General.FileExtension,
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
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
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Size,
                    Binding = new Binding(nameof(DisplayFile.Size)),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                }
            }
        };

        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Files)));
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedFile)));
        dataGrid.SelectionChanged += vm.DataGridSelectionChanged;

        var linkOpenFolder = UiUtil.MakeLink(Se.Language.File.RestoreAutoBackup.OpenAutoBackupFolder, vm.OpenFolderCommand);

        var summaryText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0.8,
            Margin = new Thickness(10, 0, 0, 0),
        };
        summaryText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.FilesSummaryText)));

        var buttonDeleteAllSubtitles = UiUtil.MakeButton(Se.Language.File.RestoreAutoBackup.DeleteAll, vm.DeleteAllFilesCommand)
            .WithBindIsVisible(nameof(vm.IsEmptyFilesVisible))
            .WithIconLeft("fa-solid fa-trash");
        var buttonRestore = UiUtil.MakeButton(Se.Language.File.RestoreAutoBackup.RestoreAutoBackupFile, vm.RestoreFileCommand)
            .WithIconLeft("fa-solid fa-clock-rotate-left");
        buttonRestore.BindIsEnabled(vm, nameof(vm.IsOkButtonEnabled));
        var buttonOk = UiUtil.MakeButtonOk(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonDeleteAllSubtitles, buttonRestore, buttonOk);

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
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Children.Add(dataGrid);
        Grid.SetRow(dataGrid, 0);
        Grid.SetColumn(dataGrid, 0);
        Grid.SetColumnSpan(dataGrid, 2);

        var panelBottomLeft = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Children = { linkOpenFolder, summaryText },
        };
        grid.Children.Add(panelBottomLeft);
        Grid.SetRow(panelBottomLeft, 1);
        Grid.SetColumn(panelBottomLeft, 0);

        grid.Children.Add(panelButtons);
        Grid.SetRow(panelButtons, 1);
        Grid.SetColumn(panelButtons, 0);
        Grid.SetColumnSpan(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

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
