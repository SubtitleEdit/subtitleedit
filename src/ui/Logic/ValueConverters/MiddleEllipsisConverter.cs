using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic.ValueConverters;

/// <summary>
/// Shortens a long string (typically a file name or path) by replacing the middle
/// with an ellipsis, keeping the start and the end visible - e.g.
/// "C:\videos\...\my-movie.mp4". The max length can be set via the binding parameter.
/// </summary>
public class MiddleEllipsisConverter : IValueConverter
{
    private const string Ellipsis = "...";
    private const int DefaultMaxLength = 50;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s || string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }

        var maxLength = DefaultMaxLength;
        if (parameter is int i)
        {
            maxLength = i;
        }
        else if (parameter is string p && int.TryParse(p, out var len))
        {
            maxLength = len;
        }

        if (s.Length <= maxLength || maxLength <= Ellipsis.Length + 1)
        {
            return s;
        }

        var keep = maxLength - Ellipsis.Length;
        var front = (keep + 1) / 2;
        var back = keep - front;
        return s[..front] + Ellipsis + s[^back..];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
