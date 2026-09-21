using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

public partial class ImproveTimeCodesRow : ObservableObject
{
    private static readonly IBrush Blue = new SolidColorBrush(Color.FromRgb(0x42, 0x8F, 0xDC)).ToImmutable();
    private static readonly IBrush Red = new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)).ToImmutable();

    [ObservableProperty] private bool _apply;
    [ObservableProperty] private bool _isChanged;
    [ObservableProperty] private string _startShift;
    [ObservableProperty] private string _endShift;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private IBrush? _statusBrush;

    public int Index { get; }
    public int Number { get; }
    public string Text { get; }
    public TimeSpan StartTime { get; }
    public double NewStartSeconds { get; private set; }
    public double NewEndSeconds { get; private set; }
    public double StartShiftMs { get; private set; }
    public double EndShiftMs { get; private set; }
    public SubtitleRetimer.LineStatus? Status { get; private set; }

    private readonly double _startSeconds;
    private readonly double _endSeconds;
    private readonly Action<ImproveTimeCodesRow> _applyChanged;
    private bool _settingResult;

    public ImproveTimeCodesRow(int index, SubtitleLineViewModel line, Action<ImproveTimeCodesRow> applyChanged)
    {
        Index = index;
        Number = line.Number > 0 ? line.Number : index + 1;
        Text = HtmlUtil.RemoveHtmlTags(line.Text ?? string.Empty, true).Replace("\r\n", "\n").Replace('\n', ' ').Trim();
        StartTime = line.StartTime;
        _startSeconds = line.StartTime.TotalSeconds;
        _endSeconds = line.EndTime.TotalSeconds;
        NewStartSeconds = _startSeconds;
        NewEndSeconds = _endSeconds;
        _applyChanged = applyChanged;
        _startShift = string.Empty;
        _endShift = string.Empty;
        _statusText = string.Empty;
    }

    public void SetResult(SubtitleRetimer.LineResult result)
    {
        var l = Se.Language.Tools.ImproveTimeCodes;
        Status = result.Status;
        NewStartSeconds = result.StartSeconds;
        NewEndSeconds = result.EndSeconds;
        StartShiftMs = (result.StartSeconds - _startSeconds) * 1000.0;
        EndShiftMs = (result.EndSeconds - _endSeconds) * 1000.0;

        _settingResult = true;
        IsChanged = result.Status is SubtitleRetimer.LineStatus.Retimed
            or SubtitleRetimer.LineStatus.MovedWithNeighbours
            or SubtitleRetimer.LineStatus.LargeMoveUnconfirmed;

        // An unconfirmed move is offered, not made: the user listens and ticks it.
        Apply = IsChanged && result.Status != SubtitleRetimer.LineStatus.LargeMoveUnconfirmed;
        _settingResult = false;

        StartShift = IsChanged ? FormatShift(StartShiftMs) : string.Empty;
        EndShift = IsChanged ? FormatShift(EndShiftMs) : string.Empty;

        (StatusText, StatusBrush) = result.Status switch
        {
            SubtitleRetimer.LineStatus.Retimed => (l.StatusRetimed, StatusDots.Green),
            SubtitleRetimer.LineStatus.MovedWithNeighbours => (l.StatusMovedWithNeighbours, Blue),
            SubtitleRetimer.LineStatus.LargeMoveUnconfirmed => (l.StatusLargeMoveUnconfirmed, StatusDots.Amber),
            SubtitleRetimer.LineStatus.Unchanged => (l.StatusUnchanged, StatusDots.Grey),
            SubtitleRetimer.LineStatus.NoSpeech => (l.StatusNoSpeech, StatusDots.Grey),
            SubtitleRetimer.LineStatus.ShiftTooLarge => (l.StatusShiftTooLarge, StatusDots.Amber),
            _ => (l.StatusFailed, Red),
        };
    }

    partial void OnApplyChanged(bool value)
    {
        if (!_settingResult)
        {
            _applyChanged(this);
        }
    }

    private static string FormatShift(double ms)
    {
        var rounded = Math.Round(ms);
        if (rounded == 0)
        {
            return "0 ms";
        }

        // A real minus sign, so earlier and later shifts are the same width in the column.
        return (rounded > 0 ? "+" : "−") + Math.Abs(rounded).ToString("0", CultureInfo.InvariantCulture) + " ms";
    }
}
