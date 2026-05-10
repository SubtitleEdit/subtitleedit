using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ImportCsvXlsxCustomColumns;

public class ImportCsvXlsxCustomColumnsWindow : Window
{
    private readonly DataGrid _dataGrid;
    private readonly DataGrid _previewGrid;

    public ImportCsvXlsxCustomColumnsWindow(ImportCsvXlsxCustomColumnsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Import.TitleImportCsvXlsxCustomColumns;
        CanResize = true;
        Width = 1200;
        Height = 800;
        MinWidth = 800;
        MinHeight = 500;

        vm.Window = this;
        DataContext = vm;

        var buttonPickFile = UiUtil.MakeButton(Se.Language.General.OpenFile, vm.PickFileCommand).WithMinWidth(140);
        var labelFile = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.FilePath)).WithAlignmentTop();
        var labelSeparator = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.SeparatorDisplay)).WithAlignmentTop();
        var topPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children = { buttonPickFile, labelFile, labelSeparator },
            Spacing = 10,
        };

        _dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            DataContext = vm,
            ItemsSource = vm.Rows,
        };

        _previewGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.PreviewSubtitles,
        };
        BuildPreviewColumns(_previewGrid);

        var labelPreview = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.PreviewCount)).WithAlignmentTop();

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsOkEnabled)) { Source = vm });
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var splitter = new GridSplitter
        {
            ResizeDirection = GridResizeDirection.Rows,
            Height = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(topPanel, 0);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(_dataGrid), 1);
        grid.Add(splitter, 2);
        grid.Add(labelPreview, 3);
        grid.Add(UiUtil.MakeBorderForControlNoPadding(_previewGrid), 4);
        grid.Add(panelButtons, 5);

        Content = grid;

        vm.ColumnsRebuilt += (_, _) => RebuildSourceGridColumns(vm);
        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private void RebuildSourceGridColumns(ImportCsvXlsxCustomColumnsViewModel vm)
    {
        _dataGrid.Columns.Clear();

        for (var i = 0; i < vm.Columns.Count; i++)
        {
            var def = vm.Columns[i];
            var column = new DataGridTextColumn
            {
                Header = MakeColumnHeader(def),
                CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                Binding = new Binding($"Cells[{i}]"),
                IsReadOnly = true,
                Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
            };
            _dataGrid.Columns.Add(column);
        }
    }

    private static Control MakeColumnHeader(CsvColumnDefinition def)
    {
        var label = new TextBlock
        {
            Text = def.HeaderName,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis,
        };

        var combo = new ComboBox
        {
            ItemsSource = MakeRoleOptions(),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 110,
            Margin = new Thickness(0, 4, 0, 0),
            DataContext = def,
        };
        combo.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(CsvColumnDefinition.Role))
        {
            Mode = BindingMode.TwoWay,
            Converter = new RoleToOptionConverter(),
        });

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children = { label, combo },
        };
    }

    private static IEnumerable<RoleOption> MakeRoleOptions()
    {
        return new[]
        {
            new RoleOption(CsvColumnRole.None, Se.Language.General.None),
            new RoleOption(CsvColumnRole.Start, Se.Language.General.Show),
            new RoleOption(CsvColumnRole.End, Se.Language.General.Hide),
            new RoleOption(CsvColumnRole.Duration, Se.Language.General.Duration),
            new RoleOption(CsvColumnRole.Text, Se.Language.General.Text),
            new RoleOption(CsvColumnRole.Character, Se.Language.General.Character),
        };
    }

    private static void BuildPreviewColumns(DataGrid grid)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Show,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Hide,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Duration,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter, Mode = BindingMode.OneWay },
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Actor,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Actor)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Text,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
        });
    }

    public sealed class RoleOption
    {
        public CsvColumnRole Role { get; }
        public string DisplayName { get; }

        public RoleOption(CsvColumnRole role, string displayName)
        {
            Role = role;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;

        public override bool Equals(object? obj) => obj is RoleOption o && o.Role == Role;
        public override int GetHashCode() => Role.GetHashCode();
    }

    private sealed class RoleToOptionConverter : Avalonia.Data.Converters.IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is CsvColumnRole role)
            {
                return new RoleOption(role, RoleDisplay(role));
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is RoleOption opt)
            {
                return opt.Role;
            }
            return CsvColumnRole.None;
        }

        private static string RoleDisplay(CsvColumnRole role) => role switch
        {
            CsvColumnRole.Start => Se.Language.General.Show,
            CsvColumnRole.End => Se.Language.General.Hide,
            CsvColumnRole.Duration => Se.Language.General.Duration,
            CsvColumnRole.Text => Se.Language.General.Text,
            CsvColumnRole.Character => Se.Language.General.Character,
            _ => Se.Language.General.None,
        };
    }
}
