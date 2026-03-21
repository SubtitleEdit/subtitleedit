using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public class ShotChangesWindow : Window
{
    private readonly ShotChangesViewModel _vm;

    public ShotChangesWindow(ShotChangesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.ShotChanges.TitleGenerateOrImport;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).BindIsEnabled(vm, nameof(vm.IsGenerating), new InverseBooleanConverter());
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
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var tabControl = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Items =
            {
                new TabItem
                {
                    Header = Se.Language.Video.ShotChanges.GenerateShotChanges,
                    Content = MakeGenerateView(vm)
                },
                new TabItem
                {
                    Header = Se.Language.Video.ShotChanges.ImportShotChanges,
                    Content = MakeImportView(vm)
                }
            }
        };

        grid.Add(tabControl, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private static Grid MakeGenerateView(ShotChangesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var dataGrid = new DataGrid
        {
            Height = double.NaN, // auto size inside scroll viewer
            Margin = new Thickness(2),
            ItemsSource = vm.FfmpegLines, // Use ItemsSource instead of Items
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            DataContext = vm,
        };
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(TimeItem.Number)),
            Width = new DataGridLength(50),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.Video.ShotChanges.ShotChangeTimeCode,
            Binding = new Binding(nameof(TimeItem.TimeText)),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
        });
        vm.DataGridFfmpegLines = dataGrid;

        var labelSensitivity = UiUtil.MakeLabel(Se.Language.General.Sensitivity);
        var sliderSensitivity = new Slider
        {
            Minimum = 0.1,
            Maximum = 0.9,
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,            
        };
        sliderSensitivity.Bind(Slider.ValueProperty, new Binding(nameof(ShotChangesViewModel.Sensitivity))
        {
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        sliderSensitivity.Bind(Slider.IsEnabledProperty, new Binding(nameof(vm.IsGenerating))
        {
            Converter = new InverseBooleanConverter(),
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        var labelSensitivityValue = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(ShotChangesViewModel.Sensitivity), new DoubleToTwoDecimalConverter());
        var buttonGenerate = UiUtil.MakeButton(Se.Language.Video.ShotChanges.GenerateShotChangesWithFfmpeg, vm.GenerateShotChangesFfmpegCommand).WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var panelSensitivity = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 5,
            Children =
            {
                labelSensitivity,
                sliderSensitivity,
                labelSensitivityValue,
                buttonGenerate,
            }
        };

        var sliderProgress = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            MinWidth = 400,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 2.0)
                    }
                },
            },
            [!Slider.ValueProperty] = new Binding(nameof(vm.ProgressValue)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            
        };
        var progressText = UiUtil.MakeLabel().WithBindText(vm, nameof(ShotChangesViewModel.ProgressText)).WithAlignmentCenter().WithMarginTop(14); 
        var gridProgress = new Grid
        {
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        gridProgress.Add(sliderProgress, 0);    
        gridProgress.Add(progressText, 0);

        grid.Add(UiUtil.MakeBorderForControlNoPadding(dataGrid), 0);
        grid.Add(panelSensitivity, 1);
        grid.Add(gridProgress, 2);

        return grid;
    }

    private static Grid MakeImportView(ShotChangesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var buttonImport = UiUtil.MakeButton(Se.Language.Video.ShotChanges.ImportShotChangesFromFile, vm.ImportFromTextFileCommand).WithLeftAlignment();

        var labelTimeCodeFormat = UiUtil.MakeLabel(Se.Language.Video.ShotChanges.TimeCodeFormatColon);
        var radioButtonFrames = UiUtil.MakeRadioButton(Se.Language.General.Frames, vm, nameof(ShotChangesViewModel.TimeCodeFrames), "TimeCode");
        var radioButtonSeconds = UiUtil.MakeRadioButton(Se.Language.General.Seconds, vm, nameof(ShotChangesViewModel.TimeCodeSeconds), "TimeCode");
        var radioButtonMilliseconds = UiUtil.MakeRadioButton(Se.Language.General.Milliseconds, vm, nameof(ShotChangesViewModel.TimeCodeMilliseconds), "TimeCode");
        var radioButtonTHhMmSsMs = UiUtil.MakeRadioButton(Se.Language.General.Milliseconds, vm, nameof(ShotChangesViewModel.TimeCodeHhMmSsMs), "TimeCode");

        var panelTimeCodeFormat = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children =
            {
                labelTimeCodeFormat,
                radioButtonFrames,
                radioButtonSeconds,
                radioButtonMilliseconds,
                radioButtonTHhMmSsMs,
            }
        };

        var textBoxImport = new TextBox
        {
            Width = double.NaN,
            Height = double.NaN,
            IsReadOnly = true,
        };
        textBoxImport.Bind(TextBox.TextProperty, new Binding(nameof(ShotChangesViewModel.ImportText)) { Source = vm, Mode = BindingMode.TwoWay });

        grid.Add(textBoxImport, 0);
        grid.Add(panelTimeCodeFormat, 1);
        grid.Add(buttonImport, 2);


        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
