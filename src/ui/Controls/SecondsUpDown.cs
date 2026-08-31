using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Controls;

public class SecondsUpDown : TemplatedControl
{
    private TextBox? _textBox;
    private ButtonSpinner? _spinner;

    public static readonly StyledProperty<TimeSpan> ValueProperty =
    AvaloniaProperty.Register<SecondsUpDown, TimeSpan>(
       nameof(Value),
               defaultValue: TimeSpan.Zero,
  defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
         coerce: CoerceValue);

    public TimeSpan Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public event EventHandler<TimeSpan>? ValueChanged;

    public SecondsUpDown()
    {
        Template = CreateTemplate();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            UpdateText();

            if (change.OldValue is TimeSpan oldValue && change.NewValue is TimeSpan newValue && oldValue != newValue)
            {
                ValueChanged?.Invoke(this, newValue);
            }
        }
        else if (change.Property == BackgroundProperty)
        {
            ApplyBackgroundToTextBox(change.NewValue as IBrush);
        }
    }

    // The templated control's Background isn't drawn behind the inner TextBox —
    // propagate it explicitly so bindings (e.g. duration min/max warning color)
    // become visible. Transparent / null means "use default TextBox style".
    private void ApplyBackgroundToTextBox(IBrush? brush)
    {
        if (_textBox == null)
        {
            return;
        }

        if (brush is null || (brush is SolidColorBrush solid && solid.Color == Colors.Transparent))
        {
            _textBox.ClearValue(TextBox.BackgroundProperty);
        }
        else
        {
            _textBox.Background = brush;
        }
    }

    private static TimeSpan CoerceValue(AvaloniaObject sender, TimeSpan value)
    {
        return Clamp(value);
    }

    private static FuncControlTemplate<SecondsUpDown> CreateTemplate()
    {
        return new FuncControlTemplate<SecondsUpDown>((control, scope) =>
        {
            var textBox = new TextBox
            {
                Name = "PART_TextBox",
                IsReadOnly = false,
                Padding = new Thickness(9, 2, 2, 2),
                Margin = new Thickness(0),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = double.NaN,
                BorderBrush = Brushes.Transparent,
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = double.NaN,
            };
            grid.Children.Add(textBox);

            var spinner = new ButtonSpinner
            {
                Name = "PART_Spinner",
                ButtonSpinnerLocation = Location.Right,
                ShowButtonSpinner = true,
                // Keep the icon-only up/down repeat buttons out of the tab order: the text box is
                // the single (named) tab stop and Up/Down arrows already step the value, so a
                // screen-reader user is not stopped on two nameless buttons per field (#12087).
                IsTabStop = false,
                Content = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = double.NaN,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
            };

            scope.Register("PART_Spinner", spinner);
            scope.Register("PART_TextBox", textBox);

            return spinner;
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _spinner = e.NameScope.Find<ButtonSpinner>("PART_Spinner");

        if (_spinner != null)
        {
            _spinner.Spin += OnSpin;
        }

        if (_textBox != null)
        {
            _textBox.Text = FormatTime(Value);

            // The inner text box is the element that actually receives keyboard focus,
            // so the accessible name set on this control must be forwarded to it for
            // screen readers to announce it (e.g. "Duration") instead of just the value.
            _textBox.Bind(AutomationProperties.NameProperty, this.GetObservable(AutomationProperties.NameProperty));

            // Screen readers deliberately stay quiet when a plain edit control's value changes,
            // so stepping with Up/Down was inaudible; announced as a spinner, every value change
            // is spoken (#12087).
            AutomationProperties.SetControlTypeOverride(_textBox, Avalonia.Automation.Peers.AutomationControlType.Spinner);

            _textBox.KeyDown += OnTextBoxKeyDown;
            _textBox.LostFocus += (_, _) => ParseAndUpdate();
            _textBox.PointerWheelChanged += (_, args) =>
            {
                ChangeValue(args.Delta.Y > 0 ? +1 : -1);
                args.Handled = true;
            };

            ApplyBackgroundToTextBox(Background);
        }
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:
                ChangeValue(+1);
                e.Handled = true;
                break;
            case Key.Down:
                ChangeValue(-1);
                e.Handled = true;
                break;
            case Key.Enter:
                ParseAndUpdate();
                e.Handled = true;
                break;
        }
    }

    private void OnSpin(object? sender, SpinEventArgs e)
    {
        ChangeValue(e.Direction == SpinDirection.Increase ? +1 : -1);
    }

    private void ParseAndUpdate()
    {
        if (_textBox == null)
        {
            return;
        }

        var parsed = ParseTime(_textBox.Text ?? string.Empty);
        if (parsed != Value)
        {
            Value = parsed;
        }
        else
        {
            UpdateText();
        }
    }

    private void ChangeValue(int delta)
    {
        var val = Value;

        if (Se.Settings.General.UseFrameMode)
        {
            // Step by whole frames via the total frame count, so an unaligned value is
            // aligned to the nearest frame first - just adding one frame duration in ms
            // can otherwise round/cap back to the same displayed frame number.
            var totalFrames = SubtitleFormat.MillisecondsToFrames(val.TotalMilliseconds);
            val = TimeSpan.FromMilliseconds(SubtitleFormat.FramesToMilliseconds(totalFrames + delta));
        }
        else
        {
            val = val.Add(TimeSpan.FromMilliseconds(10 * delta));
        }

        Value = val;
    }

    /// <summary>
    /// Re-renders the unchanged value after the frame-mode display setting changed - the text
    /// otherwise only re-formats when the value itself changes.
    /// </summary>
    public void RefreshDisplayFormat()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (_textBox != null)
        {
            _textBox.Text = FormatTime(Value);
        }
    }

    private static TimeSpan Clamp(TimeSpan time)
        => time.TotalMilliseconds < 0 ? TimeSpan.Zero : time;

    private static TimeSpan ParseTime(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return TimeSpan.Zero;
        }

        if (Se.Settings.General.UseFrameMode)
        {
            // Expect "seconds:frames"
            var parts = text.Split(':');
            if (parts.Length == 2 &&
               double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) &&
               int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var frames))
            {
                var totalMs = seconds * 1000 + SubtitleFormat.FramesToMilliseconds(frames);
                return TimeSpanExtensions.FromMillisecondsWholeMilliseconds(totalMs);
            }

            if (TryParseSecondsAndFramesWithoutSeparator(text.Trim(), out var separatorLessMs))
            {
                return TimeSpanExtensions.FromMillisecondsWholeMilliseconds(separatorLessMs);
            }
        }
        else
        {
            // Expect "seconds.ms" or "seconds,ms"
            if (double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
            {
                // Snap to a whole millisecond: TimeSpan.FromSeconds(0.82) is 819.9999 ms, which
                // reads back as "0,820" here but as "0,819" in the grid, and ends the line a
                // millisecond early (#14056).
                return TimeSpanExtensions.FromSecondsWholeMilliseconds(seconds);
            }
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Accepts a frame-mode duration typed without the colon, like the masked start/end time
    /// fields do: "300" is three seconds and zero frames. One or two digits are read as whole
    /// seconds ("5" is five seconds, not five frames), longer input has its last two digits
    /// read as frames.
    /// </summary>
    private static bool TryParseSecondsAndFramesWithoutSeparator(string text, out double totalMilliseconds)
    {
        totalMilliseconds = 0;

        if (text.Length == 0 || text.Length > 8)
        {
            return false;
        }

        foreach (var c in text)
        {
            if (!char.IsAsciiDigit(c))
            {
                return false;
            }
        }

        var secondsText = text.Length <= 2 ? text : text.Substring(0, text.Length - 2);
        var framesText = text.Length <= 2 ? "0" : text.Substring(text.Length - 2);
        if (!int.TryParse(secondsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds) ||
            !int.TryParse(framesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frames))
        {
            return false;
        }

        // A frame number the frame rate cannot hold ("199" at 25 fps) would otherwise silently
        // add a second - keep it inside the last second instead.
        var maxFrames = (int)(Configuration.Settings.General.CurrentFrameRate - 0.01);
        if (frames > maxFrames)
        {
            frames = maxFrames;
        }

        totalMilliseconds = seconds * 1000.0 + SubtitleFormat.FramesToMilliseconds(frames);
        return true;
    }

    private static string FormatTime(TimeSpan ts)
    {
        if (Se.Settings.General.UseFrameMode)
        {
            var seconds = Math.Floor(ts.TotalSeconds);
            var frames = SubtitleFormat.MillisecondsToFramesMaxFrameRate(ts.Milliseconds);
            return $"{seconds:0}:{frames:00}";
        }

        return ts.TotalSeconds.ToString("0.000");
    }
}
