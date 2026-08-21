using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Controls
{
    public class TimeCodeUpDown : TemplatedControl
    {
        public bool UseVideoOffset { get; set; } = false;

        private TextBox? _textBox;
        private ButtonSpinner? _spinner;
        private string _textBuffer = "00:00:00:000";
        private bool _isUpdatingFromValue = false;
        private bool _minWidthIncludesSign;

        // Every step (spinner, wheel and Up/Down) applies to the part the caret is on. The caret
        // starts on the last part, the milliseconds or frames, so a plain spinner click makes the
        // small adjustment that is wanted almost every time; it used to start on the hours and
        // jump a whole hour (#12506). The millisecond step size is configurable in Settings.
        // Computed rather than a constant because a negative time code carries a leading minus
        // (#13695), which shifts every part one to the right, and frame mode has a shorter last part.
        private int LastPartCaretIndex => LastPartStartIndex(_textBuffer);

        // A negative time code is shown as "-00:00:01,500". Like the separators, the sign is a mask
        // literal: it is never typed over and the caret skips past it.
        private int SignOffset => _textBuffer.Length > 0 && _textBuffer[0] == '-' ? 1 : 0;

        public static readonly StyledProperty<TimeSpan> ValueProperty =
            AvaloniaProperty.Register<TimeCodeUpDown, TimeSpan>(
                nameof(Value),
                defaultValue: TimeSpan.Zero,
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public TimeSpan Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public event EventHandler<TimeSpan>? ValueChanged;

        public TimeCodeUpDown()
        {
            Template = CreateTemplate();
            _textBuffer = FormatTime(Value);
        }

        private void MakeMouseWheelHandler(TextBox? control)
        {
            if (control == null)
            {
                return;
            }

            control.AddHandler(InputElement.PointerWheelChangedEvent, (s, e) =>
            {
                ChangeValue(e.Delta.Y > 0 ? +1 : -1);
                e.Handled = true;
            });
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            // Unsubscribe from old events
            if (_spinner != null)
            {
                _spinner.Spin -= OnSpin;
                _spinner.LayoutUpdated -= OnSpinnerLayoutUpdated;
            }

            if (_textBox != null)
            {
                _textBox.RemoveHandler(TextInputEvent, OnTextInput);
                _textBox.RemoveHandler(KeyDownEvent, OnTextBoxKeyDown);
                _textBox.GotFocus -= OnTextBoxGotFocus;
                _textBox.PastingFromClipboard -= OnPastingFromClipboard;
            }

            _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            _spinner = e.NameScope.Find<ButtonSpinner>("PART_Spinner");

            if (_spinner != null)
            {
                _spinner.Spin += OnSpin;
                _spinner.LayoutUpdated += OnSpinnerLayoutUpdated;
            }

            if (_textBox != null)
            {
                _textBuffer = FormatTime(Value);
                _textBox.Text = _textBuffer;

                // The inner text box is the element that actually receives keyboard focus,
                // so the accessible name set on this control must be forwarded to it for
                // screen readers to announce it (e.g. "Start time") instead of just the value.
                _textBox.Bind(AutomationProperties.NameProperty, this.GetObservable(AutomationProperties.NameProperty));

                // Screen readers deliberately stay quiet when a plain edit control's value changes,
                // so stepping with Up/Down was inaudible; announced as a spinner, every value change
                // is spoken (#12087).
                AutomationProperties.SetControlTypeOverride(_textBox, Avalonia.Automation.Peers.AutomationControlType.Spinner);

                _textBox.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
                _textBox.AddHandler(KeyDownEvent, OnTextBoxKeyDown, RoutingStrategies.Tunnel);
                _textBox.GotFocus += OnTextBoxGotFocus;
                _textBox.PastingFromClipboard += OnPastingFromClipboard;
            }

            // Initial MinWidth calculation with text measurement
            UpdateMinWidth();

            MakeMouseWheelHandler(_textBox);
        }

        private void OnSpinnerLayoutUpdated(object? sender, EventArgs e)
        {
            // After first layout, recalculate with actual spinner button width
            if (_spinner != null)
            {
                _spinner.LayoutUpdated -= OnSpinnerLayoutUpdated;
                UpdateMinWidth();
            }
        }

        private void UpdateMinWidth()
        {
            // Measure the sample text - with room for the minus sign only while the value actually is
            // negative, so a negative time code is not clipped (#13695) but every other time box keeps
            // the width it had.
            _minWidthIncludesSign = SignOffset > 0;
            var sampleText = _minWidthIncludesSign ? "-00:00:00:000" : "00:00:00:000";
            var formattedText = new FormattedText(
                sampleText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily),
                FontSize,
                Brushes.Black);

            double spinnerButtonsWidth = 0;

            // Try to get actual spinner button width if available after layout
            if (_spinner?.Bounds.Width > 0 && _textBox?.Bounds.Width > 0)
            {
                // Spinner contains textbox + buttons, so difference gives us button width
                spinnerButtonsWidth = _spinner.Bounds.Width - _textBox.Bounds.Width;
            }

            // Fallback to reasonable estimate if layout hasn't completed yet
            if (spinnerButtonsWidth <= 0)
            {
                // Conservative estimate for spinner buttons (varies by platform)
                // macOS: ~84px, Windows/Linux: ~50-60px
                spinnerButtonsWidth = 84;
            }

            // Account for textbox internal padding (9 left, 2 right from template)
            var textBoxInternalPadding = 11;

            MinWidth = formattedText.Width + Padding.Left + Padding.Right + textBoxInternalPadding + spinnerButtonsWidth + 3; // +3 for safety margin
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty)
            {
                var newValue = (TimeSpan)change.NewValue!;
                var clampedValue = Clamp(newValue);

                // Only an out-of-range value is rewritten. Negative time codes are legal - they come
                // from "adjust all times" with a negative offset - and the value is written straight
                // back through the two-way binding, so clamping them to zero here destroyed the time
                // code of every line the user selected (#13695).
                if (newValue != clampedValue)
                {
                    SetValue(ValueProperty, clampedValue);
                    return;
                }

                if (!_isUpdatingFromValue)
                {
                    _isUpdatingFromValue = true;
                    UpdateText();
                    _isUpdatingFromValue = false;
                }

                ValueChanged?.Invoke(this, clampedValue);
            }
        }

        private static FuncControlTemplate<TimeCodeUpDown> CreateTemplate()
        {
            return new FuncControlTemplate<TimeCodeUpDown>((control, scope) =>
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
                    CaretIndex = control.LastPartCaretIndex,
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


        private void OnTextBoxGotFocus(object? sender, FocusChangedEventArgs e)
        {
            if (_textBox != null)
            {
                _textBox.CaretIndex = LastPartCaretIndex;
            }
        }

        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            if (_textBox == null || string.IsNullOrEmpty(e.Text))
            {
                return;
            }

            var caret = _textBox.CaretIndex;
            var chars = _textBuffer.ToCharArray();
            var changed = false;

            // An IME commit (or an X11 compose sequence) can deliver several characters in one
            // event, so consume the whole string instead of only the first character.
            foreach (var c in e.Text)
            {
                // The mask holds ASCII digits only. char.IsDigit() also accepts full-width and
                // Arabic-Indic digits, which every ParseTime() branch then fails to parse - that
                // silently reset the time code to zero - so match the ASCII range explicitly.
                if (c is < '0' or > '9')
                {
                    continue;
                }

                // Skip mask literals (colons, commas, dots and the leading minus)
                while (caret < chars.Length && IsMaskLiteral(chars[caret], caret))
                {
                    caret++;
                }

                if (caret >= chars.Length)
                {
                    break;
                }

                // Overwrite character at current position
                chars[caret] = c;
                changed = true;

                // Move to next editable position
                caret++;
                while (caret < chars.Length && IsMaskLiteral(chars[caret], caret))
                {
                    caret++;
                }
            }

            e.Handled = true;

            if (!changed)
            {
                return;
            }

            _textBuffer = new string(chars);
            _textBox.Text = _textBuffer;
            _textBox.CaretIndex = Math.Min(caret, _textBuffer.Length);

            // Update the bound value
            var newValue = ParseTime(_textBuffer);
            _isUpdatingFromValue = true;
            SetValue(ValueProperty, newValue);
            _isUpdatingFromValue = false;
        }

        // The text box is masked and edits character-by-character via OnTextInput, but paste bypasses
        // that path, so without this the pasted text would corrupt the mask and leave Value out of sync.
        // We take over paste entirely: parse the clipboard as a time code (or a bare number of
        // milliseconds) and replace the whole value.
        private async void OnPastingFromClipboard(object? sender, RoutedEventArgs e)
        {
            e.Handled = true; // suppress the default paste (must be set before the first await)

            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard == null)
            {
                return;
            }

            var text = await clipboard.TryGetTextAsync();
            if (TryParsePastedValue(text, out var value))
            {
                SetValue(ValueProperty, value); // OnPropertyChanged clamps, reformats the text and raises ValueChanged
                if (_textBox != null)
                {
                    _textBox.CaretIndex = LastPartCaretIndex;
                }
            }
        }

        // Symmetric with paste: with no selection, copy grabs the whole time code (as shown, so it
        // round-trips back through paste). Handled from the key gesture rather than the
        // CopyingToClipboard event, because the text box's built-in copy does nothing when there is no
        // selection, so that event never fires in this case. An explicit selection copies natively.
        private bool TryHandleCopyWholeValue(KeyEventArgs e)
        {
            if (_textBox == null || _textBox.SelectionStart != _textBox.SelectionEnd)
            {
                return false; // let the text box copy an explicit selection itself
            }

            // Copy is Cmd+C on macOS, Ctrl+C elsewhere.
            var commandModifier = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
            if (e.Key != Key.C || e.KeyModifiers != commandModifier)
            {
                return false;
            }

            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                _ = clipboard.SetTextAsync(_textBuffer);
            }

            return true;
        }

        // A bare number is taken as milliseconds and replaces the whole time code (e.g. "231" ->
        // 00:00:00,231), which is the common paste intent (#12056). Anything with separators is parsed
        // as a full or partial time code ("00:00:05,500", "01:02,300"; frames when in frame mode).
        // A leading minus makes the value negative, matching what the control now shows (#13695).
        private bool TryParsePastedValue(string? clipboardText, out TimeSpan value)
        {
            value = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                return false;
            }

            var text = clipboardText.Trim();
            var newlineIndex = text.IndexOfAny(new[] { '\r', '\n' });
            if (newlineIndex >= 0)
            {
                text = text.Substring(0, newlineIndex).Trim();
            }

            // The sign is taken off here and re-applied at the end: the parts of a time code are
            // unsigned, and the bare-milliseconds form is parsed with NumberStyles.None.
            var isNegative = text.StartsWith('-');
            if (isNegative)
            {
                text = text.Substring(1).TrimStart();
            }

            if (text.Length == 0)
            {
                return false;
            }

            if (long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var milliseconds))
            {
                if (milliseconds > TimeCode.MaxTimeTotalMilliseconds)
                {
                    return false; // beyond the control's range (max 99:59:59,999) - reject rather than overflow
                }

                value = RemoveVideoOffset(TimeSpan.FromMilliseconds(isNegative ? -milliseconds : milliseconds));
                return true;
            }

            var useFrameMode = Se.Settings.General.UseFrameMode;
            var parts = text.Split(TimeCode.TimeSplitChars, StringSplitOptions.RemoveEmptyEntries);
            var validCount = useFrameMode ? parts.Length == 4 : parts.Length is 3 or 4;
            if (!validCount || !parts.All(p => int.TryParse(p, out _)))
            {
                return false; // ParseXxx returns 0 for junk, so reject anything that isn't clearly a time code
            }

            double ms;
            try
            {
                ms = useFrameMode
                    ? TimeCode.ParseHHMMSSFFToMilliseconds(text)
                    : TimeCode.ParseToMilliseconds(text);
            }
            catch (Exception exception) when (exception is OverflowException or ArgumentOutOfRangeException)
            {
                return false; // int-parseable but out-of-range parts (e.g. "999999999:0:0,0") overflow the TimeSpan ctor
            }

            if (ms > TimeCode.MaxTimeTotalMilliseconds)
            {
                return false;
            }

            value = RemoveVideoOffset(TimeSpan.FromMilliseconds(isNegative ? -ms : ms));
            return true;
        }

        private TimeSpan ParseTime(string text)
        {
            // The mask carries the sign as a leading minus; the parts themselves are unsigned, so take
            // the sign off first and re-apply it to the parsed magnitude (#13695). Without this, editing
            // a negative time code silently flipped it positive ("-00" parses as 0).
            var isNegative = text.StartsWith('-');
            if (isNegative)
            {
                text = text.Substring(1);
            }

            var magnitude = ParseUnsignedTime(text);
            return RemoveVideoOffset(isNegative ? magnitude.Negate() : magnitude);
        }

        private static TimeSpan ParseUnsignedTime(string text)
        {
            if (Se.Settings.General.UseFrameMode)
            {
                // In frame mode the last section is a frame number, not milliseconds
                var frameParts = text.Split(':', ',', '.');
                if (frameParts.Length == 4 &&
                    int.TryParse(frameParts[0], out var frameHours) &&
                    int.TryParse(frameParts[1], out var frameMinutes) &&
                    int.TryParse(frameParts[2], out var frameSeconds) &&
                    int.TryParse(frameParts[3], out var frames))
                {
                    var frameMs = SubtitleFormat.FramesToMillisecondsMax999(frames);
                    return new TimeSpan(0, frameHours, frameMinutes, frameSeconds, frameMs);
                }

                return TimeSpan.Zero;
            }

            // Try parsing with milliseconds format (00:00:00:000 or 00:00:00.000)
            if (TimeSpan.TryParseExact(text, @"hh\:mm\:ss\:fff", null, out var result))
            {
                return result;
            }

            // Try parsing with dot separator for milliseconds
            if (TimeSpan.TryParseExact(text, @"hh\:mm\:ss\.fff", null, out result))
            {
                return result;
            }

            // Manual parsing as fallback
            var parts = text.Split(':', ',', '.');
            if (parts.Length == 4)
            {
                if (int.TryParse(parts[0], out var hours) &&
                    int.TryParse(parts[1], out var minutes) &&
                    int.TryParse(parts[2], out var seconds) &&
                    int.TryParse(parts[3], out var milliseconds))
                {
                    return new TimeSpan(0, hours, minutes, seconds, milliseconds);
                }
            }

            return TimeSpan.Zero;
        }

        private TimeSpan RemoveVideoOffset(TimeSpan result)
        {
            if (UseVideoOffset && Se.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                result = TimeSpan.FromMilliseconds(result.TotalMilliseconds - Se.Settings.General.CurrentVideoOffsetInMs);
            }

            return result;
        }

        private void OnSpin(object? sender, SpinEventArgs e)
        {
            ChangeValue(e.Direction == SpinDirection.Increase ? +1 : -1);
        }

        private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (_textBox == null)
            {
                return;
            }

            if (TryHandleCopyWholeValue(e))
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up)
            {
                ChangeValue(+1);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ChangeValue(-1);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                var newPos = _textBox.CaretIndex - 1;
                while (newPos >= 0 && IsMaskLiteral(_textBuffer[newPos], newPos))
                {
                    newPos--;
                }

                if (newPos >= 0)
                {
                    _textBox.CaretIndex = newPos;
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                var newPos = _textBox.CaretIndex + 1;
                while (newPos < _textBuffer.Length && IsMaskLiteral(_textBuffer[newPos], newPos))
                {
                    newPos++;
                }

                if (newPos < _textBuffer.Length)
                {
                    _textBox.CaretIndex = newPos;
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                // Prevent deletion
                e.Handled = true;
            }
        }

        private void ChangeValue(int delta)
        {
            if (_textBox == null)
            {
                return;
            }

            // Measured from the first digit, so the leading minus of a negative time code does not
            // shift every part one place to the right (#13695).
            var caret = _textBox.CaretIndex - SignOffset;
            TimeSpan newVal = Value;

            if (caret <= 2)
            {
                newVal = newVal.Add(TimeSpan.FromHours(delta));
            }
            else if (caret <= 5)
            {
                newVal = newVal.Add(TimeSpan.FromMinutes(delta));
            }
            else if (caret <= 8)
            {
                newVal = newVal.Add(TimeSpan.FromSeconds(delta));
            }
            else
            {
                if (Se.Settings.General.UseFrameMode)
                {
                    // Step by whole frames via the total frame count, so an unaligned value is
                    // aligned to the nearest frame first - just adding one frame duration in ms
                    // can otherwise round/cap back to the same displayed frame number.
                    var totalFrames = SubtitleFormat.MillisecondsToFrames(newVal.TotalMilliseconds);
                    newVal = TimeSpan.FromMilliseconds(SubtitleFormat.FramesToMilliseconds(totalFrames + delta));
                }
                else
                {
                    var step = Math.Max(1, Se.Settings.General.TimeCodeUpDownStepMs);
                    newVal = newVal.Add(TimeSpan.FromMilliseconds(delta * step));
                }
            }

            _isUpdatingFromValue = true;
            SetValue(ValueProperty, newVal);
            UpdateText();
            _isUpdatingFromValue = false;
        }

        private void UpdateText()
        {
            var oldSignOffset = SignOffset;
            _textBuffer = FormatTime(Value);
            if (_textBox != null)
            {
                // Stepping across zero adds or removes the leading minus; move the caret with it so it
                // stays on the same part of the time code (#13695).
                var oldCaret = _textBox.CaretIndex + (SignOffset - oldSignOffset);
                _textBox.Text = _textBuffer;
                _textBox.CaretIndex = Math.Clamp(oldCaret, 0, _textBuffer.Length);
            }

            if (_minWidthIncludesSign != SignOffset > 0)
            {
                UpdateMinWidth();
            }
        }

        private string FormatTime(TimeSpan time)
        {
            if (UseVideoOffset && Se.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                time = TimeSpan.FromMilliseconds(time.TotalMilliseconds + Se.Settings.General.CurrentVideoOffsetInMs);
            }

            // A negative time span has negative parts throughout, which TimeCode renders as a single
            // leading minus.
            TimeCode tc;
            if (time.TotalHours > 99)
            {
                tc = new TimeCode(99, time.Minutes, time.Seconds, time.Milliseconds);
            }
            else if (time.TotalHours < -99)
            {
                tc = new TimeCode(-99, time.Minutes, time.Seconds, time.Milliseconds);
            }
            else
            {
                tc = new TimeCode(time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            }


            if (Se.Settings.General.UseFrameMode)
            {
                return tc.ToHHMMSSFF();
            }

            return tc.ToString();
        }

        private static bool IsSeparator(char c) => c == ':' || c == ',' || c == '.';

        // Positions the caret cannot edit or land on: the separators and the sign of a negative value.
        private static bool IsMaskLiteral(char c, int index) => IsSeparator(c) || (index == 0 && c == '-');

        // First character of the last part (milliseconds, or frames in frame mode).
        private static int LastPartStartIndex(string text)
        {
            for (var i = text.Length - 1; i >= 0; i--)
            {
                if (IsSeparator(text[i]))
                {
                    return i + 1;
                }
            }

            return 0;
        }

        // Negative time codes are allowed (#13695); only values the mask cannot render are pulled back
        // into range.
        private TimeSpan Clamp(TimeSpan time)
        {
            return time.TotalMilliseconds < -TimeCode.MaxTimeTotalMilliseconds
                ? TimeSpan.FromMilliseconds(-TimeCode.MaxTimeTotalMilliseconds)
                : time;
        }
    }
}
