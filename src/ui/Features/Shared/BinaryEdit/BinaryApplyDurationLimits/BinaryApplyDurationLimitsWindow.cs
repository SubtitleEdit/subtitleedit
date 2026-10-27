using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryApplyDurationLimits;

/// <summary>
/// Apply duration limits to image-based subtitles, in the editor look (see <see cref="EditorChrome"/>):
/// the minimum and maximum duration in one section, Cancel/OK at the bottom.
/// </summary>
public class BinaryApplyDurationLimitsWindow : Window
{
    private const double LabelWidth = 220;

    public BinaryApplyDurationLimitsWindow(BinaryApplyDurationLimitsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ApplyDurationLimits.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        // Minimum duration
        var minimumNumeric = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 10000,
            Increment = 100,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.MinimumDurationMs))
            {
                Mode = BindingMode.TwoWay,
            }
        };
        EditorChrome.AsField(minimumNumeric, Se.Language.Tools.ApplyDurationLimits.MinimumDurationMilliseconds, 110, alignLeft: true, center: true);

        // Maximum duration
        var maximumNumeric = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 100000,
            Increment = 100,
            FormatString = "F0",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.MaximumDurationMs))
            {
                Mode = BindingMode.TwoWay,
            }
        };
        EditorChrome.AsField(maximumNumeric, Se.Language.Tools.ApplyDurationLimits.MaximumDurationMilliseconds, 110, alignLeft: true, center: true);

        var rows = new StackPanel { Spacing = 10 };
        rows.Children.Add(BinaryEditChrome.MakeRow(Se.Language.Tools.ApplyDurationLimits.MinimumDurationMilliseconds, minimumNumeric, LabelWidth));
        rows.Children.Add(BinaryEditChrome.MakeRow(Se.Language.Tools.ApplyDurationLimits.MaximumDurationMilliseconds, maximumNumeric, LabelWidth));
        var section = EditorChrome.MakeEditorSection(Se.Language.General.Duration, rows, first: true);
        section.MinWidth = 380;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, minimumNumeric); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
