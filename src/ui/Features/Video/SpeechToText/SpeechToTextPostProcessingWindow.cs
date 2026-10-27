using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

/// <summary>
/// Speech-to-text post-processing options in the editor look: a switch per fix, grouped into
/// timing, lines, text and report sections, with Cancel/OK in the button bar.
/// </summary>
public class SpeechToTextPostProcessingWindow : Window
{
    private readonly SpeechToTextPostProcessingViewModel _vm;

    public SpeechToTextPostProcessingWindow(SpeechToTextPostProcessingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.WhisperPostProcessingTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.SpeechToText);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var l = Se.Language.Video.AudioToText;

        var colorPickerUnderlineToColor = UiUtil.MakeColorPickerButton(_vm, nameof(_vm.ChangeUnderlineToColorColor));
        AutomationProperties.SetName(colorPickerUnderlineToColor, l.ChangeUnderlineToColor);

        var timing = new StackPanel { Spacing = 8 };
        timing.Children.Add(MakeSwitchRow(vm, nameof(vm.AdjustTimings), l.AdjustTimings));
        timing.Children.Add(MakeSwitchRow(vm, nameof(vm.FixShortDuration), l.FixShortDuration));

        var lines = new StackPanel { Spacing = 8 };
        lines.Children.Add(MakeSwitchRow(vm, nameof(vm.MergeShortLines), l.MergeShortLines));
        lines.Children.Add(MakeSwitchRow(vm, nameof(vm.BreakSplitLongLines), l.BreakSplitLongLines));

        var text = new StackPanel { Spacing = 8 };
        text.Children.Add(MakeSwitchRow(vm, nameof(vm.FixCasing), l.FixCasing));
        text.Children.Add(MakeSwitchRow(vm, nameof(vm.AddPeriods), l.AddPeriods));
        text.Children.Add(MakeSwitchRow(vm, nameof(vm.RemoveNonSpeechLines), l.RemoveNonSpeechLines, l.RemoveNonSpeechLinesHint));
        text.Children.Add(MakeSwitchRow(vm, nameof(vm.RemoveRepeatedLines), l.RemoveRepeatedLines, l.RemoveRepeatedLinesHint));
        text.Children.Add(MakeSwitchRow(vm, nameof(vm.ChangeUnderlineToColor), l.ChangeUnderlineToColor, null, colorPickerUnderlineToColor));

        var report = MakeSwitchRow(vm, nameof(vm.ShowQualityReport), l.ShowQualityReport);

        var sections = new StackPanel
        {
            Background = EditorChrome.PanelBrush,
            MaxWidth = 520,
        };
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Timing, timing, first: true));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Lines, lines));
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Text, text));
        sections.Children.Add(EditorChrome.MakeEditorSection(l.QualityReportTitle, report));

        // The settings scroll while the OK/Cancel bar stays pinned, so the dialog
        // remains usable when the screen is too small for the full form (e.g. high
        // DPI) and the window gets height-clamped to the working area.
        var scrollViewer = new ScrollViewer
        {
            Content = sections,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var buttonBar = EditorChrome.MakeFlushButtonBar(null, UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand), EditorChrome.AsPrimary(UiUtil.MakeButton(Se.Language.General.Ok, vm.OKCommand)));

        var outerGrid = new Grid
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
        };
        outerGrid.Add(scrollViewer, 0, 0);
        outerGrid.Add(buttonBar, 1, 0);

        Content = outerGrid;

        UiUtil.FocusOnFirstActivation(this, () => { Focus(); }); // hack to make OnKeyDown work
    }

    private static Grid MakeSwitchRow(SpeechToTextPostProcessingViewModel vm, string property, string text, string? hint = null, params Control[] trailing)
    {
        var toggle = EditorChrome.MakeSwitch(property, text, vm);
        var label = UiUtil.MakeTextBlock(text);
        if (hint != null && Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(label, hint);
            ToolTip.SetTip(toggle, hint);
        }

        foreach (var control in trailing)
        {
            control.VerticalAlignment = VerticalAlignment.Center;
        }

        return EditorChrome.MakeSwitchRow(toggle, label, trailing);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
