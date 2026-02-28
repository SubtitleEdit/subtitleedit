using Avalonia.Controls;
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
                new DataGridTextColumn
                {
                    Header = Se.Language.General.FileExtension,
                    Binding = new Binding(nameof(DisplayFile.Extension)),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
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

        var buttonDeleteAllSubtitles = UiUtil.MakeButton(Se.Language.File.RestoreAutoBackup.DeleteAll, vm.DeleteAllFilesCommand)
            .WithBindIsVisible(nameof(vm.IsEmptyFilesVisible));
        var buttonRestore = UiUtil.MakeButton(Se.Language.File.RestoreAutoBackup.RestoreAutoBackupFile, vm.RestoreFileCommand);
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

        grid.Children.Add(linkOpenFolder);
        Grid.SetRow(linkOpenFolder, 1);
        Grid.SetColumn(linkOpenFolder, 0);

        grid.Children.Add(panelButtons);
        Grid.SetRow(panelButtons, 1);
        Grid.SetColumn(panelButtons, 0);
        Grid.SetColumnSpan(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
