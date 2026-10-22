using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.BlankVideo;

/// <summary>
/// Generate blank video, in the editor look (see <see cref="EditorChrome"/>): the video settings
/// and the background side by side, progress and Generate at the bottom.
/// </summary>
public class BlankVideoWindow : Window
{
    private readonly BlankVideoViewModel _vm;
    private NumericUpDown? _numericUpDownDuration;

    private const double LabelWidth = 84;

    /// <summary>
    /// The height of the progress strip, reserved below the settings while idle so the window
    /// does not have to grow (or clip) when generating starts.
    /// </summary>
    private const double ProgressHeight = 38;

    public BlankVideoWindow(BlankVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.GenerateBlankVideoTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);
        AddStyles();

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settings = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), // video settings
                new ColumnDefinition(GridLength.Auto), // background
            },
        };
        settings.Add(MakeVideoSettingsView(vm), 0, 0);
        settings.Add(new Border
        {
            Child = MakeBackgroundSettingsView(vm),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var settingsPanel = new Border
        {
            Child = settings,
            VerticalAlignment = VerticalAlignment.Top,
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // settings
                new RowDefinition(GridLength.Auto), // progress, while generating
                new RowDefinition(GridLength.Auto), // buttons
            },
        };
        // The panel color fills the whole row, so the room kept for the progress strip does not
        // show as an empty band under the settings
        root.Add(new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = new ScrollViewer
            {
                Content = settingsPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            },
        }, 0, 0);
        root.Add(MakeProgressView(vm), 1, 0);
        root.Add(MakeButtonBar(vm), 2, 0);

        Content = root;

        UiUtil.FocusOnFirstActivation(this, () => { _numericUpDownDuration?.Focus(); }); // initial focus on an input, not an action button - a focused button clicks on bare Space

        // Fit to content once, then freeze: the progress text that appears while generating is
        // wider than the settings, and with SizeToContent still active the window re-fit itself
        // on every progress update. The height of the progress strip is added on top, so it has
        // room when it shows up.
        Opened += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            var width = ClientSize.Width;
            var height = ClientSize.Height;
            SizeToContent = SizeToContent.Manual;
            if (width > 0 && height > 0)
            {
                Width = width;
                Height = height + (vm.IsGenerating ? 0 : ProgressHeight);
            }
        }, DispatcherPriority.Loaded);
    }

    /// <summary>
    /// Compact combo boxes.
    /// </summary>
    private void AddStyles()
    {
        Styles.Add(new Style(x => x.OfType<ComboBox>())
        {
            Setters =
            {
                new Setter(TemplatedControl.FontSizeProperty, 12.5),
                new Setter(TemplatedControl.MinHeightProperty, 28.0),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(8, 2, 4, 2)),
                new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)),
            },
        });

        // A disabled Generate (while generating) must not keep the accent - it looked clickable
        Styles.Add(new Style(x => x.OfType<Button>().Class("primary").Class(":disabled").Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>().Name("PART_ContentPresenter"))
        {
            Setters =
            {
                new Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, EditorChrome.HoverBrush),
                new Setter(Avalonia.Controls.Presenters.ContentPresenter.ForegroundProperty, EditorChrome.FaintTextBrush),
            },
        });
    }

    /// <summary>
    /// A label of fixed width and the control(s) after it, so the rows line up.
    /// </summary>
    private static Grid Row(string? label, Control content)
    {
        var text = EditorChrome.MakeDimLabel(label ?? string.Empty, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    private Border MakeVideoSettingsView(BlankVideoViewModel vm)
    {
        // Minimum 1: a duration of 0 gives "-t 0", which newer ffmpeg reads as "no limit".
        var numericUpDownDuration = EditorChrome.MakeField(nameof(vm.DurationMinutes), 1, 10000, 100, "min");
        numericUpDownDuration.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(numericUpDownDuration, Se.Language.General.DurationMinutes);
        _numericUpDownDuration = numericUpDownDuration;

        // Resolution: W x H, or the source's own size; the picker sets either
        var width = EditorChrome.MakeField(nameof(vm.VideoWidth), 0, 10_000, 76, "W");
        AutomationProperties.SetName(width, Se.Language.General.Width);
        var height = EditorChrome.MakeField(nameof(vm.VideoHeight), 0, 10_000, 76, "H");
        AutomationProperties.SetName(height, Se.Language.General.Height);
        var size = EditorChrome.MakeInlineRow(width, new TextBlock { Text = Se.Language.Video.ResolutionSeparator, Foreground = EditorChrome.DimTextBrush, VerticalAlignment = VerticalAlignment.Center }, height);
        size.Bind(IsVisibleProperty, new Binding(nameof(vm.UseSourceResolution)) { Converter = InverseBooleanConverter.Instance });
        var source = new TextBlock
        {
            Text = Se.Language.General.UseSourceResolution,
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.IsVisibleProperty] = new Binding(nameof(vm.UseSourceResolution)),
        };
        var pick = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.Resolution, vm.BrowseResolutionCommand, 28);
        var sizeOrSource = new Panel();
        sizeOrSource.Children.Add(size);
        sizeOrSource.Children.Add(source);
        var resolution = new Grid { ColumnSpacing = 4, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) } };
        resolution.Add(sizeOrSource, 0, 0);
        resolution.Add(pick, 0, 1);

        var comboBoxFrameRate = UiUtil.MakeComboBox(vm.FrameRates, vm, nameof(vm.SelectedFrameRate)).WithFrameRateDisplay();
        comboBoxFrameRate.MinWidth = 120;
        AutomationProperties.SetName(comboBoxFrameRate, Se.Language.General.FrameRate);

        var rows = new StackPanel { Spacing = 7 };
        rows.Children.Add(Row(Se.Language.General.Duration, numericUpDownDuration));
        rows.Children.Add(Row(Se.Language.General.Resolution, resolution));
        rows.Children.Add(Row(Se.Language.General.FrameRate, comboBoxFrameRate));
        rows.Children.Add(EditorChrome.MakeSwitchRow(EditorChrome.MakeSwitch(nameof(vm.GenerateTimeCodes), Se.Language.Video.GenerateTimeCodes), Se.Language.Video.GenerateTimeCodes));

        var section = EditorChrome.MakeEditorSection(Se.Language.Video.BurnIn.VideoOutput, rows, first: true);
        section.MinWidth = 320;
        return section;
    }

    private static Border MakeBackgroundSettingsView(BlankVideoViewModel vm)
    {
        var radioButtonCheckeredImage = UiUtil.MakeRadioButton(Se.Language.Video.CheckeredImage, vm, nameof(vm.UseCheckedImage), "background");

        var radioButtonUseSolidColor = UiUtil.MakeRadioButton(Se.Language.General.SolidColor, vm, nameof(vm.UseSolidColor), "background");
        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.SolidColor), false);
        AutomationProperties.SetName(colorPicker, Se.Language.General.SolidColor);
        var panelSolidColor = EditorChrome.MakeInlineRow(radioButtonUseSolidColor, colorPicker);

        var radioButtonImage = UiUtil.MakeRadioButton(Se.Language.General.Image, vm, nameof(vm.UseBackgroundImage), "background");
        var buttonBrowseImage = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.General.Image, vm.BrowseImageCommand, 28);
        var labelImage = EditorChrome.MakeBoundHint(nameof(vm.BackgroundImageFileName));
        labelImage.MaxWidth = 200;
        var panelImage = EditorChrome.MakeInlineRow(radioButtonImage, buttonBrowseImage, labelImage);

        foreach (var radioButton in new[] { radioButtonCheckeredImage, radioButtonUseSolidColor, radioButtonImage })
        {
            radioButton.FontSize = EditorChrome.FieldFontSize;
        }

        var rows = new StackPanel { Spacing = 4 };
        rows.Children.Add(radioButtonCheckeredImage);
        rows.Children.Add(panelSolidColor);
        rows.Children.Add(panelImage);

        var section = EditorChrome.MakeEditorSection(Se.Language.General.Background, rows, first: true);
        section.MinWidth = 260;
        return section;
    }

    private Border MakeProgressView(BlankVideoViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.VerticalAlignment = VerticalAlignment.Center;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var statusText = EditorChrome.MakeBoundHint(nameof(vm.ProgressText));
        statusText.FontSize = EditorChrome.FieldFontSize;
        statusText.MaxWidth = 320;

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto) },
        };
        grid.Add(progressBar, 0, 0);
        grid.Add(statusText, 0, 1);

        return new Border
        {
            Child = grid,
            Background = EditorChrome.StatusBrush,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(16, 8),
            Height = ProgressHeight,
            [!Border.IsVisibleProperty] = new Binding(nameof(vm.IsGenerating)),
        };
    }

    private static Border MakeButtonBar(BlankVideoViewModel vm)
    {
        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand)
            .WithBindEnabled(nameof(vm.IsGenerating), InverseBooleanConverter.Instance);
        buttonGenerate.Classes.Add("primary");
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsGenerating), InverseBooleanConverter.Instance);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttons = EditorChrome.MakeInlineRow(buttonCancel, buttonOk, buttonGenerate);
        buttons.Spacing = 10;
        buttons.HorizontalAlignment = HorizontalAlignment.Right;

        return EditorChrome.WrapButtonBar(buttons);
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
