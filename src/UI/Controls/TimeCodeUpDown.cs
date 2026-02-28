using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Controls
{
    public class TimeCodeUpDown : TemplatedControl
    {
        public bool UseVideoOffset { get; set; } = false;

        private TextBox? _textBox;
        private ButtonSpinner? _spinner;
        private string _textBuffer = "00:00:00:000";
        private bool _isUpdatingFromValue = false;

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

                _textBox.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
                _textBox.AddHandler(KeyDownEvent, OnTextBoxKeyDown, RoutingStrategies.Tunnel);
                _textBox.GotFocus += OnTextBoxGotFocus;
            }

            // Initial MinWidth calculation with text measurement
            UpdateMinWidth();
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
            // Measure the sample text
            var sampleText = "00:00:00:000";
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
                    CaretIndex = 0,
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


        private void OnTextBoxGotFocus(object? sender, GotFocusEventArgs e)
        {
            if (_textBox != null)
            {
                // Select the first digit position
                _textBox.CaretIndex = 0;
            }
        }

        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            if (_textBox == null || string.IsNullOrEmpty(e.Text))
            {
                return;
            }

            var c = e.Text[0];
            if (!char.IsDigit(c))
            {
                e.Handled = true;
                return;
            }

            var caret = _textBox.CaretIndex;

            // Skip colons
            while (caret < _textBuffer.Length && _textBuffer[caret] == ':')
            {
                caret++;
            }

            if (caret >= _textBuffer.Length)
            {
                e.Handled = true;
                return;
            }

            // Overwrite character at current position
            var chars = _textBuffer.ToCharArray();
            chars[caret] = c;
            _textBuffer = new string(chars);

            _textBox.Text = _textBuffer;

            // Move to next editable position
            var nextPos = caret + 1;
            while (nextPos < _textBuffer.Length && _textBuffer[nextPos] == ':')
            {
                nextPos++;
            }

            _textBox.CaretIndex = Math.Min(nextPos, _textBuffer.Length);

            // Update the bound value
            var newValue = ParseTime(_textBuffer);
            _isUpdatingFromValue = true;
            SetValue(ValueProperty, newValue);
            _isUpdatingFromValue = false;

            e.Handled = true;
        }

        private TimeSpan ParseTime(string text)
        {
            // Try parsing with milliseconds format (00:00:00:000 or 00:00:00.000)
            if (TimeSpan.TryParseExact(text, @"hh\:mm\:ss\:fff", null, out var result))
            {
                return RemoveVideoOffset(result);
            }

            // Try parsing with dot separator for milliseconds
            if (TimeSpan.TryParseExact(text, @"hh\:mm\:ss\.fff", null, out result))
            {
                return RemoveVideoOffset(result);
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
                    return RemoveVideoOffset(new TimeSpan(0, hours, minutes, seconds, milliseconds));
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
                while (newPos >= 0 && _textBuffer[newPos] == ':')
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
                while (newPos < _textBuffer.Length && _textBuffer[newPos] == ':')
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

            var caret = _textBox.CaretIndex;
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
                    //TODO: align to nearest frame before adjusting?
                    var ms = SubtitleFormat.FramesToMilliseconds(delta);
                    newVal = newVal.Add(TimeSpan.FromMilliseconds(ms));
                }
                else
                {
                    newVal = newVal.Add(TimeSpan.FromMilliseconds(delta));
                }
            }

            _isUpdatingFromValue = true;
            SetValue(ValueProperty, newVal);
            UpdateText();
            _isUpdatingFromValue = false;
        }

        private void UpdateText()
        {
            _textBuffer = FormatTime(Value);
            if (_textBox != null)
            {
                var oldCaret = _textBox.CaretIndex;
                _textBox.Text = _textBuffer;
                _textBox.CaretIndex = Math.Min(oldCaret, _textBuffer.Length);
            }
        }

        private string FormatTime(TimeSpan time)
        {
            if (UseVideoOffset && Se.Settings.General.CurrentVideoOffsetInMs != 0)
            {
                time = TimeSpan.FromMilliseconds(time.TotalMilliseconds + Se.Settings.General.CurrentVideoOffsetInMs);
            }

            TimeCode tc;
            if (time.TotalHours > 99)
            {
                tc = new TimeCode(99, time.Minutes, time.Seconds, time.Milliseconds);
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

        private TimeSpan Clamp(TimeSpan time)
        {
            return time.TotalMilliseconds < 0 ? TimeSpan.Zero : time;
        }
    }
}