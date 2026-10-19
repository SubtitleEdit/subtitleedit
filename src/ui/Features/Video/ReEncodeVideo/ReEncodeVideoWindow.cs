using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.ReEncodeVideo;

/// <summary>
/// Re-encode video for better subtitling, in the editor look (see <see cref="EditorChrome"/>):
/// the info and the video settings in a section, progress and Generate at the bottom.
/// </summary>
public class ReEncodeVideoWindow : Window
{
    private readonly ReEncodeVideoViewModel _vm;
    private ComboBox? _comboBoxFrameRate;

    private const double LabelWidth = 130;

    public ReEncodeVideoWindow(ReEncodeVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.ReEncodeVideoForBetterSubtitlingTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 600;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Video);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelInfoText = new TextBlock
        {
            FontSize = 12.5,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MaxWidth = 560,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.InfoText)),
        };

        var videoSettingsView = MakeVideoSettingsView(vm, labelInfoText);
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var progressView = EditorChrome.MakeProgressStrip(progressBar, nameof(vm.ProgressText), nameof(vm.IsGenerating));
        var buttonGenerate = new SeSplitButton
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
        buttonGenerate.Classes.Add("primary");
        buttonGenerate.Bind(SplitButton.IsEnabledProperty, new Binding(nameof(vm.IsGenerating)) { Converter = InverseBooleanConverter.Instance });
        AutomationProperties.SetName(buttonGenerate, Se.Language.General.Generate);

        var buttonDone = UiUtil.MakeButtonDone(vm.OkCommand).WithBindEnabled(nameof(vm.IsGenerating), InverseBooleanConverter.Instance);
        var buttonPanel = EditorChrome.MakeButtonBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindIsVisible(nameof(vm.IsGenerating)), buttonDone, buttonGenerate);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // info + settings
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // progress bar
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
        };

        grid.Add(EditorChrome.MakePanel(videoSettingsView), 0);
        grid.Add(progressView, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, () => { _comboBoxFrameRate?.Focus(); }); // initial focus on an input, not an action button - a focused button clicks on bare Space
    }

    private Border MakeVideoSettingsView(ReEncodeVideoViewModel vm, TextBlock labelInfoText)
    {
        var textBoxWidth = EditorChrome.MakeTextField(nameof(vm.VideoWidth), Se.Language.General.Width, 76);
        var labelX = EditorChrome.MakeDimLabel(Se.Language.Video.ResolutionSeparator, new Thickness(0));
        var textBoxHeight = EditorChrome.MakeTextField(nameof(vm.VideoHeight), Se.Language.General.Height, 76);
        var buttonResolution = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.Resolution, vm.BrowseResolutionCommand, 28);
        var panelResolution = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                textBoxWidth,
                labelX,
                textBoxHeight,
                buttonResolution,
            }
        }.WithBindVisible(vm, nameof(vm.UseSourceResolution), InverseBooleanConverter.Instance);

        var labelSourceResolution = new TextBlock { Text = Se.Language.General.UseSourceResolution, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center };
        var buttonResolutionSource = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.Resolution, vm.BrowseResolutionCommand, 28);
        var panelResolutionSource = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                labelSourceResolution,
                buttonResolutionSource,
            }
        }.WithBindVisible(vm, nameof(vm.UseSourceResolution));

        var comboBoxFrameRate = UiUtil.MakeComboBox(vm.FrameRates, vm, nameof(vm.SelectedFrameRate)).WithFrameRateDisplay();
        comboBoxFrameRate.MinWidth = 140;
        AutomationProperties.SetName(comboBoxFrameRate, Se.Language.General.FrameRate);
        _comboBoxFrameRate = comboBoxFrameRate;

        var comboBoxVideoExtensions = UiUtil.MakeComboBox(vm.VideoExtensions, vm, nameof(vm.SelectedVideoExtension));
        comboBoxVideoExtensions.MinWidth = 140;
        AutomationProperties.SetName(comboBoxVideoExtensions, Se.Language.General.VideoExtension);

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(labelInfoText);

        // The resolution rows are not shown (yet)
        //rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.General.Resolution, panelResolution, LabelWidth, spacing: 6));
        //rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.General.Resolution, panelResolutionSource, LabelWidth, spacing: 6));

        rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.General.FrameRate, WrapLeft(comboBoxFrameRate), LabelWidth, spacing: 6));
        rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.General.VideoExtension, WrapLeft(comboBoxVideoExtensions), LabelWidth, spacing: 6));

        return EditorChrome.MakeEditorSection(Se.Language.Video.BurnIn.VideoOutput, rows, first: true);
    }

    private static Control WrapLeft(Control control)
    {
        control.HorizontalAlignment = HorizontalAlignment.Left;
        return control;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
