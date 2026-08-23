using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

/// <summary>One row in "List errors": one error on one line. A line with several errors has several rows.</summary>
public class ErrorListItem
{
    public int Number { get; }
    public string Text { get; }
    public string Show { get; }
    public string Hide { get; }
    public LineErrorType Type { get; }
    public string Category { get; }
    public string Detail { get; }
    public IBrush Brush { get; }
    public SubtitleLineViewModel Subtitle { get; }

    /// <summary>Old single-string form ("Reading speed: 27.3 > 25"), kept for the Error column binding.</summary>
    public string Error => $"{Category}: {Detail}";

    public ErrorListItem(SubtitleLineViewModel subtitle, LineError error)
    {
        Subtitle = subtitle;
        Text = subtitle.Text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
        Number = subtitle.Number;
        Show = (string)TimeSpanToDisplayShortConverter.Instance.Convert(subtitle.StartTime, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture);
        Hide = (string)TimeSpanToDisplayShortConverter.Instance.Convert(subtitle.EndTime, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture);
        Type = error.Type;
        Category = error.Label;
        Detail = error.Detail;
        Brush = LineError.GetBrush(error.Type);
    }

    /// <summary>One item per error on the line - the caller knows the real neighbours, which the gap/overlap wording needs.</summary>
    public static IEnumerable<ErrorListItem> Make(SubtitleLineViewModel subtitle, SubtitleLineViewModel? prev, SubtitleLineViewModel? next)
    {
        return subtitle.GetErrorList(prev, next).Select(e => new ErrorListItem(subtitle, e));
    }
}
