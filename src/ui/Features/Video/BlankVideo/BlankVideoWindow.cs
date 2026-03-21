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

namespace Nikse.SubtitleEdit.Features.Video.BlankVideo;

public class BlankVideoWindow : Window
{
    private readonly BlankVideoViewModel _vm;

    public BlankVideoWindow(BlankVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.GenerateBlankVideoTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var videoSettingsView = MakeVideoSettingsView(vm);
        var backgroundSettingsView = MakeBackgroundSettingsView(vm);
        var progressView = MakeProgressView(vm);

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand)
            .WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonGenerate,
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // settings
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress bar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // subtitle/video settings
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }, // cut/preview/video info
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(videoSettingsView, 0, 0);
        grid.Add(backgroundSettingsView, 0, 1);
        grid.Add(progressView, 1, 0, 1, 2);
        grid.Add(buttonPanel, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private static Border MakeVideoSettingsView(BlankVideoViewModel vm)
    {
        var labelDuration = UiUtil.MakeLabel(Se.Language.General.DurationMinutes);
        var numericUpDownDuration = UiUtil.MakeNumericUpDownInt(0, 10000, 0, 120, vm, nameof(vm.DurationMinutes));

        var labelResolution = UiUtil.MakeLabel(Se.Language.General.Resolution);
        var textBoxWidth = UiUtil.MakeTextBox(100, vm, nameof(vm.VideoWidth));
        var labelX = UiUtil.MakeLabel("x");
        var textBoxHeight = UiUtil.MakeTextBox(100, vm, nameof(vm.VideoHeight));
        var buttonResolution = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand);
        var panelResolution = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                textBoxWidth,
                labelX,
                textBoxHeight,
                buttonResolution,
            }
        }.WithBindVisible(vm, nameof(vm.UseSourceResolution), new InverseBooleanConverter());

        var labelSourceResolution = UiUtil.MakeLabel(Se.Language.General.UseSourceResolution).WithBindVisible(vm, nameof(vm.UseSourceResolution));
        var buttonResolutionSource = UiUtil.MakeButtonBrowse(vm.BrowseResolutionCommand);
        var panelResolutionSource = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelSourceResolution,
                buttonResolutionSource,
            }
        }.WithBindVisible(vm, nameof(vm.UseSourceResolution));

        var labelFrameRate = UiUtil.MakeLabel(Se.Language.General.FrameRate);
        var comboBoxFrameRate = UiUtil.MakeComboBox(vm.FrameRates, vm, nameof(vm.SelectedFrameRate));

        var checkBoxGenerateTimeCodes = UiUtil.MakeCheckBox(Se.Language.Video.GenerateTimeCodes, vm, nameof(vm.GenerateTimeCodes));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelDuration, 0, 0);
        grid.Add(numericUpDownDuration, 0, 1);

        grid.Add(labelResolution, 1, 0);
        grid.Add(panelResolution, 1, 1);
        grid.Add(panelResolutionSource, 1, 1);

        grid.Add(labelFrameRate, 2, 0);
        grid.Add(comboBoxFrameRate, 2, 1);

        grid.Add(checkBoxGenerateTimeCodes, 3, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Border MakeBackgroundSettingsView(BlankVideoViewModel vm)
    {
        var labelBackground = UiUtil.MakeLabel(Se.Language.General.Background).WithBold().WithMarginBottom(8);

        var radioButtonCheckeredImage = UiUtil.MakeRadioButton(Se.Language.Video.CheckeredImage, vm, nameof(vm.UseCheckedImage), "background");

        var radioButtonUseSolidColor = UiUtil.MakeRadioButton(Se.Language.General.SolidColor, vm, nameof(vm.UseSolidColor), "background");
        var colorPicker = UiUtil.MakeColorPicker(vm, nameof(_vm.SolidColor));
        var panelSolidColor = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                radioButtonUseSolidColor,
                colorPicker,
            }
        };  

        var radioButtonImage = UiUtil.MakeRadioButton(Se.Language.General.Image, vm, nameof(vm.UseBackgroundImage), "background");
        var buttonBrowseImage = UiUtil.MakeButtonBrowse(vm.BrowseImageCommand);
        var labelImage = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.BackgroundImageFileName));
        var panelImage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                radioButtonImage,
                buttonBrowseImage,
                labelImage,
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelBackground, 0, 0);
        grid.Add(radioButtonCheckeredImage, 1, 0);
        grid.Add(panelSolidColor, 2, 0);
        grid.Add(panelImage, 3, 0);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Grid MakeProgressView(BlankVideoViewModel vm)
    {
        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
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
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressSlider.Bind(Slider.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var statusText = new TextBlock
        {
            Margin = new Thickness(5, 20, 0, 0),
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(progressSlider, 0, 0);
        grid.Add(statusText, 0, 0);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
