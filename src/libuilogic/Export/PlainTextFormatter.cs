using Nikse.SubtitleEdit.Core.Common;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Export;

public enum PlainTextProcessing
{
    None,
    Merge,
    Unbreak,
}

public class PlainTextOptions
{
    public PlainTextProcessing TextProcessing { get; set; }
    public bool RemoveStyling { get; set; } = true;
    public bool ShowLineNumbers { get; set; }
    public bool AddNewLineAfterLineNumber { get; set; } = true;
    public bool ShowTimeCodes { get; set; }
    public bool AddNewLineAfterTimeCode { get; set; } = true;
    public string TimeCodeFormat { get; set; } = PlainTextFormatter.TimeCodeFormatSrt;
    public string TimeCodeSeparator { get; set; } = " --> ";
    public bool AddLineAfterText { get; set; } = true;
    public bool AddLineBetweenSubtitles { get; set; } = true;
}

/// <summary>
/// Plain text export, shared by File → Export → Plain text and batch convert so both write the same text.
/// </summary>
public static class PlainTextFormatter
{
    public const string TimeCodeFormatSrt = "hh:mm:ss,zzz";
    public const string TimeCodeFormatDot = "hh:mm:ss.zzz";
    public const string TimeCodeFormatFrames = "hh:mm:ss:ff";
    public const string TimeCodeFormatSeconds = "seconds";

    public static string Generate(IReadOnlyList<Paragraph> paragraphs, PlainTextOptions options)
    {
        if (options.TextProcessing == PlainTextProcessing.Merge)
        {
            var parts = new List<string>();
            foreach (var p in paragraphs)
            {
                var text = p.Text ?? string.Empty;
                if (options.RemoveStyling)
                {
                    text = HtmlUtil.RemoveHtmlTags(text, true);
                }

                text = text.Replace(Environment.NewLine, " ").Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    parts.Add(text);
                }
            }

            return string.Join(" ", parts);
        }

        var sb = new StringBuilder();
        for (var i = 0; i < paragraphs.Count; i++)
        {
            var p = paragraphs[i];
            var text = p.Text ?? string.Empty;

            if (options.RemoveStyling)
            {
                text = HtmlUtil.RemoveHtmlTags(text, true);
            }

            if (options.TextProcessing == PlainTextProcessing.Unbreak)
            {
                text = text.Replace(Environment.NewLine, " ");
                text = Utilities.UnbreakLine(text);
            }

            if (options.ShowLineNumbers)
            {
                sb.Append(i + 1);
                if (options.AddNewLineAfterLineNumber)
                {
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(' ');
                }
            }

            if (options.ShowTimeCodes)
            {
                sb.Append(FormatTimeCode(p.StartTime.TotalMilliseconds, options.TimeCodeFormat));
                sb.Append(options.TimeCodeSeparator);
                sb.Append(FormatTimeCode(p.EndTime.TotalMilliseconds, options.TimeCodeFormat));
                if (options.AddNewLineAfterTimeCode)
                {
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(' ');
                }
            }

            sb.AppendLine(text.Trim());

            if (options.AddLineAfterText)
            {
                sb.AppendLine();
            }

            if (options.AddLineBetweenSubtitles)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatTimeCode(double totalMilliseconds, string timeCodeFormat)
    {
        var tc = new TimeCode(totalMilliseconds);
        return timeCodeFormat switch
        {
            TimeCodeFormatSeconds => tc.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture),
            TimeCodeFormatSrt => tc.ToString(false),
            TimeCodeFormatDot => tc.ToString().Replace(',', '.'),
            TimeCodeFormatFrames => tc.ToHHMMSSFF(),
            _ => tc.ToString()
        };
    }
}
