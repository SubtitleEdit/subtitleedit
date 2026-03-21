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

namespace Nikse.SubtitleEdit.Features.Video.ReEncodeVideo;

public class ReEncodeVideoWindow : Window
{
    private readonly ReEncodeVideoViewModel _vm;

    public ReEncodeVideoWindow(ReEncodeVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.ReEncodeVideoForBetterSubtitlingTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelInfoText = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.InfoText)).WithMarginBottom(8);

        var videoSettingsView = MakeVideoSettingsView(vm);
        var progressView = MakeProgressView(vm);
        var buttonGenerate = new SplitButton
        {
            Content = Se.Language.General.Generate,
            Command = vm.GenerateCommand,
            Flyout =  new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.Video.PromptForFfmpegParamsAndGenerate,
                        Command = vm.PromptFfmpegParametersAndGeenrateCommand,
                    },
                }
            }
        };
        buttonGenerate.Bind(SplitButton.IsEnabledProperty, new Binding(nameof(vm.IsGenerating)) { Converter = new InverseBooleanConverter() });

        var buttonDone = UiUtil.MakeButtonDone(vm.OkCommand).WithBindEnabled(nameof(vm.IsGenerating), new InverseBooleanConverter());
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonGenerate,
            buttonDone,
            UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindIsVisible(nameof(vm.IsGenerating))
        );

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // info
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // settings
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress bar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelInfoText, 0);
        grid.Add(videoSettingsView, 1);
        grid.Add(progressView, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        Activated += delegate { buttonGenerate.Focus(); }; // hack to make OnKeyDown work
    }

    private static Border MakeVideoSettingsView(ReEncodeVideoViewModel vm)
    {
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

        var labelVideoExtension = UiUtil.MakeLabel(Se.Language.General.VideoExtension);
        var comboBoxVideoExtensions = UiUtil.MakeComboBox(vm.VideoExtensions, vm, nameof(vm.SelectedVideoExtension));

        var grid = new Grid
        {
            RowDefinitions =
            {
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

        //grid.Add(labelResolution, 0, 0);
        //grid.Add(panelResolution, 0, 1);
        //grid.Add(panelResolutionSource, 0, 1);

        grid.Add(labelFrameRate, 1, 0);
        grid.Add(comboBoxFrameRate, 1, 1);

        grid.Add(labelVideoExtension, 2, 0);
        grid.Add(comboBoxVideoExtensions, 2, 1);

        return UiUtil.MakeBorderForControl(grid).WithMarginBottom(5).WithMarginRight(5);
    }

    private static Grid MakeProgressView(ReEncodeVideoViewModel vm)
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
