using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Platform.Progress;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public class ShotChangesWindow : Window
{
    private readonly ShotChangesViewModel _vm;

    public ShotChangesWindow(ShotChangesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.GenerateImportShotChanges;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).BindIsEnabled(vm, nameof(vm.IsGenerating), InverseBooleanConverter.Instance).AsPrimary();
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress, while generating
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
        };

        var tabControl = new TabControl
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(0),
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

        grid.Add(new Border
        {
            Background = EditorChrome.PanelBrush,
            Padding = new Thickness(8, 4, 8, 0),
            Child = tabControl,
        }, 0);
        grid.Add(MakeProgressView(vm), 1);
        grid.Add(EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk), 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () =>
        {
            // initial focus on an input, not an action button - a focused button clicks on bare Space
            if (vm.FfmpegLinesGrid != null)
            {
                TableViewExtras.FocusRow(vm.FfmpegLinesGrid);
            }
        });
    }

    private static Grid MakeGenerateView(ShotChangesViewModel vm)
    {
        var tableView = TableViewExtras.MakeTableView();
        tableView.Height = double.NaN; // auto size inside scroll viewer
        tableView.Margin = new Thickness(2);
        tableView.ItemsSource = vm.FfmpegLines;
        tableView.DataContext = vm;
        tableView.WithAccessibleName(Se.Language.Video.ShotChanges.ShotChangesList);
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(TimeItem.Number)),
            Width = new GridLength(50),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        tableView.Columns.Add(new SeTableViewColumn
        {
            Header = Se.Language.Video.ShotChanges.ShotChangeTimeCode,
            Binding = new Binding(nameof(TimeItem.TimeText)),
            Width = new GridLength(1, GridUnitType.Star),
            CellTheme = UiUtil.TableViewCellTheme,
            HeaderTheme = UiUtil.TableViewColumnHeaderTheme,
        });
        vm.FfmpegLinesGrid = tableView;

        // Same range and step as the SE 4 numeric box (0.05 - 0.95 in 0.01 steps). A floor of 0.1
        // silently dropped about half the cuts for users who relied on 0.05 (issue #15190).
        var sliderSensitivity = new Slider
        {
            Minimum = 0.05,
            Maximum = 0.95,
            TickFrequency = 0.01,
            IsSnapToTickEnabled = true,
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
            Converter = InverseBooleanConverter.Instance,
            Source = vm,
            Mode = BindingMode.OneWay,
        });
        AutomationProperties.SetName(sliderSensitivity, Se.Language.General.Sensitivity);
        var labelSensitivityValue = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(ShotChangesViewModel.Sensitivity), new DoubleToTwoDecimalConverter());
        labelSensitivityValue.MinWidth = 36;
        labelSensitivityValue.FontSize = 12.5;
        var buttonGenerate = UiUtil.MakeButton(Se.Language.Video.ShotChanges.GenerateShotChangesWithFfmpeg, vm.GenerateShotChangesFfmpegCommand)
            .WithBindEnabled(nameof(vm.IsGenerating), InverseBooleanConverter.Instance);
        buttonGenerate.Margin = new Thickness(12, 0, 0, 0);
        var panelSensitivity = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.General.Sensitivity, new Thickness(0)),
                sliderSensitivity,
                labelSensitivityValue,
                buttonGenerate,
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(EditorChrome.MakeListPane(Se.Language.General.ShotChanges, tableView, margin: new Thickness(6, 12, 6, 12)), 0);
        var settings = EditorChrome.MakeEditorSection(Se.Language.General.Settings, panelSensitivity);
        settings.Padding = new Thickness(6, 12, 6, 14);
        grid.Add(settings, 1);

        return grid;
    }

    /// <summary>
    /// Progress bar and text in a strip above the buttons, while ffmpeg looks for shot changes.
    /// </summary>
    private Border MakeProgressView(ShotChangesViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.WithPlatformProgress(this, nameof(vm.IsGenerating));
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar[!ProgressBar.ValueProperty] = new Binding(nameof(vm.ProgressValue)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
        var progressText = EditorChrome.MakeBoundHint(nameof(ShotChangesViewModel.ProgressText), 12);
        progressText.MinWidth = 60;
        var gridProgress = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        gridProgress.Add(progressBar, 0, 0);
        gridProgress.Add(progressText, 0, 1);

        var strip = EditorChrome.MakeProgressStrip(gridProgress);
        strip.Bind(IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));
        return strip;
    }

    private static Grid MakeImportView(ShotChangesViewModel vm)
    {
        var buttonImport = EditorChrome.MakeActionButton("fa-solid fa-file-import", Se.Language.Video.ShotChanges.ImportShotChangesFromFile, vm.ImportFromTextFileCommand);

        var radioButtonFrames = UiUtil.MakeRadioButton(Se.Language.General.Frames, vm, nameof(ShotChangesViewModel.TimeCodeFrames), "TimeCode");
        var radioButtonSeconds = UiUtil.MakeRadioButton(Se.Language.General.Seconds, vm, nameof(ShotChangesViewModel.TimeCodeSeconds), "TimeCode");
        var radioButtonMilliseconds = UiUtil.MakeRadioButton(Se.Language.General.Milliseconds, vm, nameof(ShotChangesViewModel.TimeCodeMilliseconds), "TimeCode");
        var radioButtonTHhMmSsMs = UiUtil.MakeRadioButton(Se.Language.General.HoursMinutesSecondsMilliseconds, vm, nameof(ShotChangesViewModel.TimeCodeHhMmSsMs), "TimeCode");

        var panelTimeCodeFormat = new WrapPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            ItemSpacing = 14,
            Children =
            {
                EditorChrome.MakeDimLabel(Se.Language.Video.ShotChanges.TimeCodeFormatColon, new Thickness(0)),
                radioButtonFrames,
                radioButtonSeconds,
                radioButtonMilliseconds,
                radioButtonTHhMmSsMs,
            }
        };
        foreach (var radioButton in new[] { radioButtonFrames, radioButtonSeconds, radioButtonMilliseconds, radioButtonTHhMmSsMs })
        {
            radioButton.FontSize = 12.5;
        }

        var textBoxImport = new TextBox
        {
            Width = double.NaN,
            Height = double.NaN,
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
        };
        textBoxImport.Bind(TextBox.TextProperty, new Binding(nameof(ShotChangesViewModel.ImportText)) { Source = vm, Mode = BindingMode.TwoWay });
        textBoxImport.WithAccessibleName(Se.Language.Video.ShotChanges.ImportShotChanges); // the tab header; the box has no label (#12087)

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(EditorChrome.MakeListPane(Se.Language.General.ShotChanges, EditorChrome.MakeListFrame(textBoxImport), buttonImport, new Thickness(6, 12, 6, 12)), 0);
        var settings = EditorChrome.MakeEditorSection(Se.Language.General.Settings, panelTimeCodeFormat);
        settings.Padding = new Thickness(6, 12, 6, 14);
        grid.Add(settings, 1);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
