using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

/// <summary>
/// Remove burned-in subtitles (opened from Video OCR): mask and output settings on the left,
/// a before/after preview of one OCR line on the right, progress and Start at the bottom.
/// </summary>
public class RemoveBurnedInWindow : Window
{
    private readonly RemoveBurnedInViewModel _vm;
    private const double LabelWidth = 170;

    public RemoveBurnedInWindow(RemoveBurnedInViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.RemoveBurnedIn.Title;
        Width = 1100;
        Height = 640;
        MinWidth = 800;
        MinHeight = 520;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.RemoveBurnedIn;
        var info = new TextBlock
        {
            Text = l.Info,
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 380,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var brightness = EditorChrome.MakeField(nameof(vm.BrightnessMinimum), 1, 255, 90);
        ToolTip.SetTip(brightness, Se.Language.Video.VideoOcr.TextBrightnessMinimumHint);
        var grow = EditorChrome.MakeField(nameof(vm.Grow), 0, 40, 90);
        ToolTip.SetTip(grow, l.GrowMaskHint);
        var outline = EditorChrome.MakeSwitchRow(nameof(vm.RequireOutline), l.RequireOutline);
        ToolTip.SetTip(outline, l.RequireOutlineHint);
        var temporal = EditorChrome.MakeSwitchRow(nameof(vm.Temporal), l.TemporalConsistency);
        ToolTip.SetTip(temporal, l.TemporalConsistencyHint);

        var modelCombo = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel));
        modelCombo.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Models)));
        modelCombo.MinWidth = 220;
        AutomationProperties.SetName(modelCombo, l.Model);

        var maskRows = new StackPanel { Spacing = 8 };
        maskRows.Children.Add(info);
        maskRows.Children.Add(EditorChrome.MakeLabeledRow(l.Model, modelCombo, LabelWidth));
        maskRows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.Video.VideoOcr.TextBrightnessMinimum, brightness, LabelWidth));
        maskRows.Children.Add(EditorChrome.MakeLabeledRow(l.GrowMask, grow, LabelWidth));
        var extraWidth = EditorChrome.MakeField(nameof(vm.ExtraWidth), 0, 100, 90);
        ToolTip.SetTip(extraWidth, l.ExtraWidthHint);
        maskRows.Children.Add(EditorChrome.MakeLabeledRow(l.ExtraWidth, extraWidth, LabelWidth));
        maskRows.Children.Add(outline);
        maskRows.Children.Add(temporal);

        var outputBox = EditorChrome.MakeTextField(nameof(vm.OutputFileName), l.OutputFile);
        outputBox.MinWidth = 220;
        var outputPanel = new DockPanel { LastChildFill = true };
        var browse = UiUtil.MakeButtonBrowse(vm.BrowseOutputCommand);
        DockPanel.SetDock(browse, Dock.Right);
        browse.Margin = new Thickness(6, 0, 0, 0);
        outputPanel.Children.Add(browse);
        outputPanel.Children.Add(outputBox);
        var crf = EditorChrome.MakeField(nameof(vm.Crf), 0, 51, 90);
        ToolTip.SetTip(crf, l.QualityHint);
        var outputRows = new StackPanel { Spacing = 8 };
        outputRows.Children.Add(EditorChrome.MakeLabeledRow(l.OutputFile, outputPanel, LabelWidth));
        outputRows.Children.Add(EditorChrome.MakeLabeledRow(l.Quality, crf, LabelWidth));

        var settings = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(l.TextMask, maskRows, first: true),
                EditorChrome.MakeEditorSection(Se.Language.Video.BurnIn.VideoOutput, outputRows),
            },
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var progressView = EditorChrome.MakeProgressStrip(progressBar, nameof(vm.ProgressText), nameof(vm.IsRunning));

        var buttonStart = UiUtil.MakeButton(l.Start, vm.StartCommand)
            .WithBindEnabled(nameof(vm.IsRunning), InverseBooleanConverter.Instance).AsPrimary();
        var buttonPanel = EditorChrome.MakeButtonBar(null, UiUtil.MakeButtonCancel(vm.CancelCommand), buttonStart);

        var columns = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        columns.Add(new ScrollViewer { Content = settings, MaxWidth = 470 }, 0, 0);
        columns.Add(MakePreviewView(vm), 0, 1);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(EditorChrome.MakePanel(columns), 0);
        grid.Add(progressView, 1);
        grid.Add(buttonPanel, 2);
        Content = grid;

        Loaded += (_, _) => vm.OnLoaded();
        UiUtil.FocusOnFirstActivation(this, () => brightness.Focus());
    }

    private static Control MakePreviewView(RemoveBurnedInViewModel vm)
    {
        var l = Se.Language.Video.RemoveBurnedIn;
        var lineCombo = UiUtil.MakeComboBox(vm.Lines, vm, nameof(vm.SelectedLine));
        lineCombo.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Lines)));
        lineCombo.MinWidth = 260;
        lineCombo.MaxWidth = 420;
        AutomationProperties.SetName(lineCombo, l.PreviewLine);
        var previewButton = UiUtil.MakeButton(l.Preview, vm.PreviewCommand)
            .WithBindEnabled(nameof(vm.IsPreviewing), InverseBooleanConverter.Instance);
        var status = EditorChrome.MakeBoundHint(nameof(vm.PreviewStatus));
        status.VerticalAlignment = VerticalAlignment.Center;
        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { lineCombo, previewButton, status },
        };

        var images = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(0, 8, 0, 0),
        };
        images.Add(EditorChrome.MakeHint(l.Before), 0);
        images.Add(MakeImage(nameof(vm.PreviewBefore)), 1);
        images.Add(EditorChrome.MakeHint(l.After), 2);
        images.Add(MakeImage(nameof(vm.PreviewAfter)), 3);

        var panel = new DockPanel { LastChildFill = true, Margin = new Thickness(12, 0, 0, 0) };
        DockPanel.SetDock(header, Dock.Top);
        panel.Children.Add(header);
        panel.Children.Add(images);
        return EditorChrome.MakeFillSection(l.Preview, panel, first: true);
    }

    private static Image MakeImage(string property) => new()
    {
        Stretch = Stretch.Uniform,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
        [!Image.SourceProperty] = new Binding(property),
    };

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
