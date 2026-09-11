using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace UITests.Controls;

// Negative time codes (#13695). "Adjust all times" with a negative offset moves lines before zero,
// and the editor used to clamp any negative value to zero and write that back through its two-way
// binding - so every line the user selected had its time code destroyed. The control now shows,
// edits and steps negative time codes the way SE 4.x did.
public partial class TimeCodeUpDownNegativeTests : IDisposable
{
    // A window left open outlives the test: it keeps the application-wide activation and focused
    // element, so a later test's click or key press is delivered to it instead. Closing here rather
    // than at the end of each test also covers the tests that stop early on a failed assertion.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    public partial class Line : ObservableObject
    {
        [ObservableProperty] private TimeSpan _start = TimeSpan.Zero;
    }

    public partial class Vm : ObservableObject
    {
        [ObservableProperty] private Line? _selected;
    }

    private sealed class TimeCodeSettings : IDisposable
    {
        private readonly SettingsScope _scope;
        private readonly long _videoOffset;

        internal TimeCodeSettings(bool frameMode, int stepMs)
        {
            // CurrentVideoOffsetInMs is a field, which SettingsScope (properties only) cannot hold.
            _scope = new SettingsScope("General.UseFrameMode", "General.TimeCodeUpDownStepMs");
            _videoOffset = Se.Settings.General.CurrentVideoOffsetInMs;

            Se.Settings.General.UseFrameMode = frameMode;
            Se.Settings.General.TimeCodeUpDownStepMs = stepMs;
            Se.Settings.General.CurrentVideoOffsetInMs = 0;
        }

        public void Dispose()
        {
            Se.Settings.General.CurrentVideoOffsetInMs = _videoOffset;
            _scope.Dispose();
        }
    }

    private static TimeCodeSettings MillisecondMode(int stepMs = 100) => new(frameMode: false, stepMs);

    private static TimeCodeSettings FrameMode() => new(frameMode: true, stepMs: 100);

    // Focus() alone can leave the caret placement (and with it the part the arrow keys step)
    // a dispatcher frame behind on a loaded runner; the first key then lands before the control
    // has settled on the millisecond part (CI flake: Up from -100 ms left the value unchanged).
    private static void FocusAndSettle(TextBox textBox)
    {
        textBox.Focus();
        Dispatcher.UIThread.RunJobs();
    }

    private (Window window, TimeCodeUpDown control, TextBox textBox) Show(TimeCodeUpDown control)
    {
        var window = new Window { Content = control };
        _windows.Add(window);
        window.Show();
        var textBox = control.GetVisualDescendants().OfType<TextBox>().Single();
        return (window, control, textBox);
    }

    // The reported bug: walking a list of lines that start before zero zeroed them one by one.
    [AvaloniaFact]
    public void SelectingLinesWithNegativeTimesLeavesThemUnchanged()
    {
        using var _ = MillisecondMode();

        var beforeZero = new Line { Start = TimeSpan.FromMilliseconds(-3000) };
        var alsoBeforeZero = new Line { Start = TimeSpan.FromMilliseconds(-1500) };
        var afterZero = new Line { Start = TimeSpan.FromMilliseconds(2000) };

        var vm = new Vm();
        var control = new TimeCodeUpDown { DataContext = vm };
        control[!TimeCodeUpDown.ValueProperty] =
            new Binding($"{nameof(Vm.Selected)}.{nameof(Line.Start)}") { Mode = BindingMode.TwoWay };
        var (_, _, textBox) = Show(control);

        vm.Selected = beforeZero;
        Assert.Equal(-3000, beforeZero.Start.TotalMilliseconds);
        Assert.Equal(-3000, control.Value.TotalMilliseconds);
        Assert.Equal("-00:00:03,000", textBox.Text);

        vm.Selected = alsoBeforeZero;
        vm.Selected = afterZero;
        vm.Selected = beforeZero;

        Assert.Equal(-3000, beforeZero.Start.TotalMilliseconds);
        Assert.Equal(-1500, alsoBeforeZero.Start.TotalMilliseconds);
        Assert.Equal(2000, afterZero.Start.TotalMilliseconds);
    }

    // Same write-back, but through the very first binding push when the control is attached.
    [AvaloniaFact]
    public void AttachingToANegativeValueDoesNotZeroTheSource()
    {
        using var _ = MillisecondMode();

        var line = new Line { Start = TimeSpan.FromMilliseconds(-1500) };
        var control = new TimeCodeUpDown { DataContext = line };
        control[!TimeCodeUpDown.ValueProperty] = new Binding(nameof(Line.Start)) { Mode = BindingMode.TwoWay };
        Show(control);

        Assert.Equal(-1500, line.Start.TotalMilliseconds);
    }

    // Typing must not eat the sign, and must not flip the value positive ("-00" parses as 0).
    [AvaloniaFact]
    public void TypingKeepsTheValueNegative()
    {
        using var _ = MillisecondMode();

        var control = new TimeCodeUpDown { Value = TimeSpan.FromMilliseconds(-1500) };
        var (window, _, textBox) = Show(control);

        FocusAndSettle(textBox);
        textBox.CaretIndex = 8; // the seconds part of "-00:00:01,500"
        window.KeyTextInput("7");

        Assert.Equal("-00:00:07,500", textBox.Text);
        Assert.Equal(-7500, control.Value.TotalMilliseconds);
    }

    // The caret lands on the last part, and every part is measured from the first digit - not from
    // the sign, which used to shift hours/minutes/seconds one place to the right.
    [AvaloniaFact]
    public void SteppingAppliesToThePartTheCaretIsOn()
    {
        using var _ = MillisecondMode(stepMs: 100);

        var control = new TimeCodeUpDown { Value = TimeSpan.FromMilliseconds(-1500) };
        var (window, _, textBox) = Show(control);

        FocusAndSettle(textBox);
        window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
        Assert.Equal(-1600, control.Value.TotalMilliseconds); // milliseconds, not hours

        textBox.CaretIndex = 8; // seconds
        window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, null);
        Assert.Equal(-600, control.Value.TotalMilliseconds);
    }

    // Stepping across zero adds/removes the sign; the caret must stay on the same part.
    [AvaloniaFact]
    public void SteppingAcrossZeroUpdatesSignAndCaret()
    {
        using var _ = MillisecondMode(stepMs: 100);

        var control = new TimeCodeUpDown { Value = TimeSpan.FromMilliseconds(-100) };
        var (window, _, textBox) = Show(control);

        FocusAndSettle(textBox);
        var caretOnMilliseconds = textBox.CaretIndex;
        Assert.Equal(10, caretOnMilliseconds); // "-00:00:00,100"

        window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, null);
        Assert.Equal(TimeSpan.Zero, control.Value);
        Assert.Equal("00:00:00,000", textBox.Text);
        Assert.Equal(9, textBox.CaretIndex); // still the first millisecond digit

        window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
        Assert.Equal(-100, control.Value.TotalMilliseconds);
        Assert.Equal("-00:00:00,100", textBox.Text);
        Assert.Equal(10, textBox.CaretIndex);
    }

    // Left/Right skip the sign like they skip the separators.
    [AvaloniaFact]
    public void CaretDoesNotLandOnTheSign()
    {
        using var _ = MillisecondMode();

        var control = new TimeCodeUpDown { Value = TimeSpan.FromMilliseconds(-1500) };
        var (window, _, textBox) = Show(control);

        FocusAndSettle(textBox);
        textBox.CaretIndex = 1; // first hour digit of "-00:00:01,500"
        window.KeyPress(Key.Left, RawInputModifiers.None, PhysicalKey.ArrowLeft, null);

        Assert.Equal(1, textBox.CaretIndex);
    }

    [AvaloniaFact]
    public void ValueBelowTheMaskRangeIsClamped()
    {
        using var _ = MillisecondMode();

        var control = new TimeCodeUpDown
        {
            Value = TimeSpan.FromMilliseconds(-TimeCode.MaxTimeTotalMilliseconds - 5000)
        };
        Show(control);

        Assert.Equal(-TimeCode.MaxTimeTotalMilliseconds, control.Value.TotalMilliseconds);
    }

    [AvaloniaTheory]
    [InlineData("-00:00:01,500", -1500)]
    [InlineData("-01:02:03,004", -3723004)]
    [InlineData("00:00:01,500", 1500)]
    public void ParsesTheSignOfTheMaskedText(string text, int expectedMs)
    {
        using var _ = MillisecondMode();

        var parse = typeof(TimeCodeUpDown).GetMethod("ParseTime", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var value = (TimeSpan)parse.Invoke(new TimeCodeUpDown(), new object?[] { text })!;

        Assert.Equal(expectedMs, (int)value.TotalMilliseconds);
    }

    // Frame mode has a shorter last part ("-00:00:01:12"), so the sign handling must not assume the
    // millisecond layout.
    [AvaloniaFact]
    public void FrameModeShowsAndParsesNegativeValues()
    {
        using var _ = FrameMode();

        var control = new TimeCodeUpDown { Value = TimeSpan.FromMilliseconds(-1500) };
        var (_, _, textBox) = Show(control);

        Assert.StartsWith("-00:00:01", textBox.Text);

        var parse = typeof(TimeCodeUpDown).GetMethod("ParseTime", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var value = (TimeSpan)parse.Invoke(control, new object?[] { textBox.Text })!;
        Assert.True(value.TotalMilliseconds < 0, $"expected '{textBox.Text}' to parse as negative");
    }

    [AvaloniaTheory]
    [InlineData("-1500", -1500)]
    [InlineData("-00:00:01,500", -1500)]
    [InlineData("1500", 1500)]
    public void PasteAcceptsNegativeValues(string clipboardText, int expectedMs)
    {
        using var _ = MillisecondMode();

        var tryParse = typeof(TimeCodeUpDown).GetMethod("TryParsePastedValue", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var args = new object?[] { clipboardText, null };
        var ok = (bool)tryParse.Invoke(new TimeCodeUpDown(), args)!;

        Assert.True(ok, $"expected '{clipboardText}' to parse");
        Assert.Equal(expectedMs, (int)((TimeSpan)args[1]!).TotalMilliseconds);
    }
}
