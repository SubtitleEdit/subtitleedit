using Avalonia.Data.Converters;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

public class DoubleToDisplayShortConverter : IValueConverter
{
    public static readonly DoubleToDisplayShortConverter Instance = new();

    // Reused to avoid per-call TimeCode allocations (expected to be used from the UI thread only).
    private readonly TimeCode _formattingTimeCode = new();
    private const string ZeroFrameMode = "00.00";
    private const string ZeroTime = "00,000";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var useFrameMode = Se.Settings.General.UseFrameMode;
        if (value is double ms)
        {
            if (ms == double.MaxValue || double.IsNaN(ms))
            {
                // Sentinel for "no value" (e.g. the gap after the last line) - show nothing
                // instead of a clamped "0,000".
                return string.Empty;
            }

            _formattingTimeCode.TotalMilliseconds = ms;
            return useFrameMode
                ? _formattingTimeCode.ToShortStringHHMMSSFF()
                : _formattingTimeCode.ToShortString();
        }

        return useFrameMode ? ZeroFrameMode : ZeroTime;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
