using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.ErrorList;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.BatchErrorList;

/// <summary>One row in the batch error list: one error on one line of one file.</summary>
public class BatchErrorListItem
{
    public string FileName { get; }
    public int Number { get; }
    public string Text { get; }
    public string Show { get; }
    public string Hide { get; }
    public LineErrorType Type { get; }
    public string Category { get; }
    public string Detail { get; }
    public IBrush Brush { get; }
    public SubtitleLineViewModel Subtitle { get; }

    /// <summary>"Reading speed: 27.3 > 25" - the single-string form used by the CSV export.</summary>
    public string Error => $"{Category}: {Detail}";

    public BatchErrorListItem(string fileName, SubtitleLineViewModel subtitle, LineError error)
    {
        FileName = fileName;
        Subtitle = subtitle;
        Text = subtitle.Text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ');
        Number = subtitle.Number;
        Show = (string)TimeSpanToDisplayShortConverter.Instance.Convert(subtitle.StartTime, typeof(string), null, CultureInfo.CurrentCulture);
        Hide = (string)TimeSpanToDisplayShortConverter.Instance.Convert(subtitle.EndTime, typeof(string), null, CultureInfo.CurrentCulture);
        Type = error.Type;
        Category = error.Label;
        Detail = error.Detail;
        Brush = LineError.GetBrush(error.Type);
    }

    public static IEnumerable<BatchErrorListItem> Make(string fileName, SubtitleLineViewModel subtitle, SubtitleLineViewModel? prev, SubtitleLineViewModel? next)
    {
        return subtitle.GetErrorList(prev, next).Select(e => new BatchErrorListItem(fileName, subtitle, e));
    }
}
