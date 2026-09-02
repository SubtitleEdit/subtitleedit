using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.FlowEditing;

public sealed class FlowEditingItem : INotifyPropertyChanged, IDisposable
{
    private bool _updatingFromSource;
    private string _text;
    private IBrush? _foreground;

    public FlowEditingItem(SubtitleLineViewModel source)
    {
        Source = source;
        var parsed = FlowTextParser.Parse(source.Text);
        _text = parsed.Text;
        _foreground = parsed.Foreground;
        Source.PropertyChanged += SourceOnPropertyChanged;
    }

    public SubtitleLineViewModel Source { get; }

    public int Number => Source.Number;

    public string TimeCode =>
        $"{FormatFrameTimeCode(Source.StartTime)}  →  {FormatFrameTimeCode(Source.EndTime)}";

    public IBrush? Foreground
    {
        get => _foreground;
        private set
        {
            if (ReferenceEquals(_foreground, value))
            {
                return;
            }

            _foreground = value;
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    nameof(Foreground)));
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value)
            {
                return;
            }

            var oldText = _text;
            _text = value;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    nameof(Text)));

            if (!_updatingFromSource)
            {
                var originalProjection =
                    FlowInlineColorProjection
                        .Parse(Source.Text);

                var editedProjection =
                    originalProjection
                        .ApplyVisibleEdit(value);

                if (TryGetPureInsertion(
                        oldText,
                        value,
                        out var insertionStart,
                        out var insertionLength))
                {
                    string? inheritedColor = null;

                    // A newly created EBU Flow subtitle can already carry its
                    // default colour as an empty font tag, e.g.
                    // <font color="yellow"></font>. Because there are no
                    // visible characters yet, the projection has no colour run
                    // from which the first typed character could inherit.
                    if (oldText.Length == 0 &&
                        insertionStart == 0)
                    {
                        inheritedColor =
                            FlowTextParser
                                .Parse(Source.Text)
                                .ColorToken;
                    }
                    else if (insertionStart > 0)
                    {
                        var leftOffset =
                            insertionStart - 1;

                        foreach (var run in originalProjection.ColorRuns)
                        {
                            if (leftOffset >= run.Start &&
                                leftOffset < run.End)
                            {
                                inheritedColor =
                                    run.Color;

                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(
                            inheritedColor))
                    {
                        editedProjection =
                            editedProjection.ApplyColor(
                                insertionStart,
                                insertionLength,
                                inheritedColor);
                    }
                }

                Source.Text =
                    editedProjection.Serialize();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static bool TryGetPureInsertion(
        string oldText,
        string newText,
        out int insertionStart,
        out int insertionLength)
    {
        insertionStart = 0;
        insertionLength = 0;

        if (newText.Length <= oldText.Length)
        {
            return false;
        }

        var prefixLength = 0;

        while (prefixLength < oldText.Length &&
               prefixLength < newText.Length &&
               oldText[prefixLength] == newText[prefixLength])
        {
            prefixLength++;
        }

        var oldSuffixIndex =
            oldText.Length - 1;

        var newSuffixIndex =
            newText.Length - 1;

        while (oldSuffixIndex >= prefixLength &&
               newSuffixIndex >= prefixLength &&
               oldText[oldSuffixIndex] == newText[newSuffixIndex])
        {
            oldSuffixIndex--;
            newSuffixIndex--;
        }

        if (oldSuffixIndex >= prefixLength)
        {
            return false;
        }

        insertionStart =
            prefixLength;

        insertionLength =
            newText.Length -
            oldText.Length;

        return insertionLength > 0;
    }

    private static string FormatFrameTimeCode(
        TimeSpan time)
    {
        var frameRate =
            Se.Settings.General.CurrentFrameRate;

        if (frameRate <= 0)
        {
            frameRate =
                Se.Settings.General.DefaultFrameRate;
        }

        if (frameRate <= 0)
        {
            return time.ToString(
                @"hh\:mm\:ss\.fff",
                CultureInfo.InvariantCulture);
        }

        var totalSeconds =
            Math.Max(
                0.0,
                time.TotalSeconds);

        var wholeSeconds =
            (long)Math.Floor(
                totalSeconds);

        var fraction =
            totalSeconds -
            wholeSeconds;

        var frame =
            (int)Math.Round(
                fraction * frameRate,
                MidpointRounding.AwayFromZero);

        var nominalFrameCount =
            Math.Max(
                1,
                (int)Math.Round(
                    frameRate));

        if (frame >= nominalFrameCount)
        {
            wholeSeconds++;
            frame = 0;
        }

        var hours =
            wholeSeconds / 3600;

        var minutes =
            (wholeSeconds % 3600) / 60;

        var seconds =
            wholeSeconds % 60;

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:00}:{1:00}:{2:00}:{3:00}",
            hours,
            minutes,
            seconds,
            frame);
    }

    private void SourceOnPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName ==
            nameof(SubtitleLineViewModel.Text))
        {
            var parsed =
                FlowTextParser.Parse(
                    Source.Text);

            _updatingFromSource = true;

            Text =
                parsed.Text;

            Foreground =
                parsed.Foreground;

            _updatingFromSource = false;
        }
        else if (e.PropertyName ==
                 nameof(SubtitleLineViewModel.Number))
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    nameof(Number)));
        }
        else if (e.PropertyName ==
                     nameof(SubtitleLineViewModel.StartTime) ||
                 e.PropertyName ==
                     nameof(SubtitleLineViewModel.EndTime))
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    nameof(TimeCode)));
        }
    }

    public void Dispose()
    {
        Source.PropertyChanged -=
            SourceOnPropertyChanged;
    }
}
