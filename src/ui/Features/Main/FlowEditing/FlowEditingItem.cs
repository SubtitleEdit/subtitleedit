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

            _text = value;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(
                    nameof(Text)));

            if (!_updatingFromSource)
            {
                Source.Text =
                    FlowInlineColorProjection
                        .Parse(Source.Text)
                        .ApplyVisibleEdit(value)
                        .Serialize();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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
