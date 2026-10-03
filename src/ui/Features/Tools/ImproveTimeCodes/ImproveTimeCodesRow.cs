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
    [ObservableProperty] private string _heard;

    public int Index { get; }
    public int Number { get; }
    public string Text { get; }
    public TimeSpan StartTime { get; }
    public double NewStartSeconds { get; private set; }
    public double NewEndSeconds { get; private set; }
    public double StartShiftMs { get; private set; }
    public double EndShiftMs { get; private set; }
    public SubtitleRetimer.LineStatus? Status { get; private set; }

    /// <summary>Where the line stays when its new times are not applied.</summary>
    public double FallbackStartSeconds { get; private set; }
    public double FallbackEndSeconds { get; private set; }

    /// <summary>The user moved or resized the line in the waveform; <see cref="UndoAdjustment"/> brings back the result.</summary>
    public bool IsAdjustedByHand => Status == SubtitleRetimer.LineStatus.AdjustedByHand;

    private readonly double _startSeconds;
    private readonly double _endSeconds;
    private readonly Action<ImproveTimeCodesRow> _applyChanged;
    private bool _settingResult;
    private SubtitleRetimer.LineResult? _result;

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
        FallbackStartSeconds = _startSeconds;
        FallbackEndSeconds = _endSeconds;
        _applyChanged = applyChanged;
        _startShift = string.Empty;
        _endShift = string.Empty;
        _statusText = string.Empty;
        _heard = string.Empty;
    }

    public void SetResult(SubtitleRetimer.LineResult result)
    {
        _result = result;
        Status = result.Status;
        NewStartSeconds = result.StartSeconds;
        NewEndSeconds = result.EndSeconds;
        (FallbackStartSeconds, FallbackEndSeconds) = result.Fallback ?? (_startSeconds, _endSeconds);

        _settingResult = true;
        IsChanged = SubtitleRetimer.IsMove(result.Status);

        // An unconfirmed move is offered, not made: the user listens and ticks it.
        Apply = IsChanged && !SubtitleRetimer.IsUnconfirmed(result.Status);
        _settingResult = false;

        ShowApplied(NewStartSeconds, NewEndSeconds);
        Heard = result.HeardRatio is { } heard
            ? Math.Round(heard * 100).ToString("0", CultureInfo.InvariantCulture) + "%"
            : string.Empty;

        UpdateStatusText();
    }

    /// <summary>The user moved or resized the line in the aligned waveform: those times are applied.</summary>
    public void SetAdjustedByHand(double startSeconds, double endSeconds)
    {
        Status = SubtitleRetimer.LineStatus.AdjustedByHand;
        NewStartSeconds = startSeconds;
        NewEndSeconds = endSeconds;

        _settingResult = true;
        IsChanged = true;
        Apply = true;
        _settingResult = false;

        ShowApplied(startSeconds, endSeconds);
        UpdateStatusText();
    }

    /// <summary>Back to what the aligner made of the line - or to no change at all, before it ran.</summary>
    public void UndoAdjustment()
    {
        if (!IsAdjustedByHand)
        {
            return;
        }

        if (_result != null)
        {
            SetResult(_result);
            return;
        }

        Status = null;
        NewStartSeconds = _startSeconds;
        NewEndSeconds = _endSeconds;
        _settingResult = true;
        IsChanged = false;
        Apply = false;
        _settingResult = false;
        StartShift = string.Empty;
        EndShift = string.Empty;
        StatusText = string.Empty;
        StatusBrush = null;
    }

    /// <summary>
    /// Shows the shifts of the times the line actually gets - for an applied line that can be
    /// a little less than proposed, where it had to make room for a neighbour.
    /// </summary>
    public void ShowApplied(double startSeconds, double endSeconds)
    {
        StartShiftMs = (startSeconds - _startSeconds) * 1000.0;
        EndShiftMs = (endSeconds - _endSeconds) * 1000.0;
        StartShift = IsChanged ? FormatShift(StartShiftMs) : string.Empty;
        EndShift = IsChanged ? FormatShift(EndShiftMs) : string.Empty;
    }

    private void UpdateStatusText()
    {
        var l = Se.Language.Tools.ImproveTimeCodes;
        (StatusText, StatusBrush) = Status switch
        {
            SubtitleRetimer.LineStatus.Retimed => (l.StatusRetimed, StatusDots.Green),
            SubtitleRetimer.LineStatus.MovedWithNeighbours => (l.StatusMovedWithNeighbours, Blue),
            SubtitleRetimer.LineStatus.LargeMoveUnconfirmed => (l.StatusLargeMoveUnconfirmed, StatusDots.Amber),
            SubtitleRetimer.LineStatus.ConfirmedBySpeech => (l.StatusConfirmedBySpeech, StatusDots.Green),
            SubtitleRetimer.LineStatus.DisputedBySpeech => (l.StatusDisputedBySpeech, StatusDots.Amber),
            SubtitleRetimer.LineStatus.MovedWithSync => (l.StatusMovedWithSync, Blue),
            SubtitleRetimer.LineStatus.Unchanged => (l.StatusUnchanged, StatusDots.Grey),
            SubtitleRetimer.LineStatus.NoSpeech => (l.StatusNoSpeech, StatusDots.Grey),
            SubtitleRetimer.LineStatus.ShiftTooLarge => (l.StatusShiftTooLarge, StatusDots.Amber),
            SubtitleRetimer.LineStatus.AdjustedByHand => (l.StatusAdjustedByHand, Blue),
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
