using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

/// <summary>
/// One line in the review step: an OCR line, or a drawn rectangle (<see cref="IsRectangle"/>)
/// that is removed as a whole while shown. <see cref="Line"/> is what the timeline shows:
/// dragging a box edits its StartTime/EndTime in place, and the run uses those edited times.
/// </summary>
public partial class RemoveBurnedInLineItem : ObservableObject
{
    [ObservableProperty] private bool _isIncluded = true;
    [ObservableProperty] private int _rectX;
    [ObservableProperty] private int _rectY;
    [ObservableProperty] private int _rectWidth;
    [ObservableProperty] private int _rectHeight;

    public SubtitleLineViewModel Line { get; }

    public bool IsRectangle { get; }

    public string Text => Line.Text;

    public int Number => Line.Number;

    public RemoveBurnedInLineItem(int number, TimeSpan start, TimeSpan end, string text)
    {
        Line = new SubtitleLineViewModel
        {
            Id = Guid.NewGuid(),
            Number = number,
            StartTime = start,
            EndTime = end,
            Text = text,
        };
    }

    /// <summary>A drawn rectangle (video pixels).</summary>
    public RemoveBurnedInLineItem(int number, TimeSpan start, TimeSpan end, int x, int y, int width, int height)
        : this(number, start, end, string.Empty)
    {
        IsRectangle = true;
        _rectX = x;
        _rectY = y;
        _rectWidth = width;
        _rectHeight = height;
        UpdateRectangleText();
    }

    public void SetNumber(int number)
    {
        Line.Number = number;
        OnPropertyChanged(nameof(Number));
    }

    public BurnedInRectangle ToRectangle() => new(Line.StartTime, Line.EndTime, RectX, RectY, RectWidth, RectHeight);

    partial void OnRectWidthChanged(int value) => UpdateRectangleText();
    partial void OnRectHeightChanged(int value) => UpdateRectangleText();

    private void UpdateRectangleText()
    {
        if (IsRectangle)
        {
            Line.Text = string.Format(Se.Language.Video.RemoveBurnedIn.RectangleXY, RectWidth, RectHeight);
            OnPropertyChanged(nameof(Text));
        }
    }

    public override string ToString() => Text;
}
