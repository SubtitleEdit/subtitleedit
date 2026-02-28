using Google.Api.Gax.ResourceNames;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

public static class ImportOriginalHelper
{
    internal static Subtitle GetMatchingOriginalLines(ObservableCollection<SubtitleLineViewModel> current, Subtitle original)
    {
        var newOriginal = new Subtitle();
        foreach (var line in current)
        {
            var originalLine = FindOriginalLine(line, original);
            if (originalLine != null)
            {
                newOriginal.Paragraphs.Add(originalLine);
            }
            else
            {
                var emptyLine = new Paragraph
                {
                    StartTime = TimeCode.FromSeconds(line.StartTime.TotalSeconds),
                    EndTime = TimeCode.FromSeconds(line.EndTime.TotalSeconds),
                    Text = string.Empty
                };
                newOriginal.Paragraphs.Add(emptyLine);
            }
        }

        return newOriginal;
    }

    private static Paragraph? FindOriginalLine(SubtitleLineViewModel line, Subtitle original)
    {
        foreach (var originalLine in original.Paragraphs)
        {
            if (line.StartTime.TotalMilliseconds == originalLine.StartTime.TotalMilliseconds &&
                line.EndTime.TotalMilliseconds == originalLine.EndTime.TotalMilliseconds)
            {
                return originalLine;
            }
        }

        // try with some tolerance
        foreach (var originalLine in original.Paragraphs)
        {
            if (Math.Abs(line.StartTime.TotalMilliseconds - originalLine.StartTime.TotalMilliseconds) < 250 &&
                Math.Abs(line.EndTime.TotalMilliseconds - originalLine.EndTime.TotalMilliseconds) < 500)
            {
                return originalLine;
            }
        }

        // try with middle time only
        var lineMiddle = (line.StartTime.TotalMilliseconds + line.EndTime.TotalMilliseconds) / 2.0;
        foreach (var originalLine in original.Paragraphs)
        {
            if (originalLine.StartTime.TotalMilliseconds <= lineMiddle && originalLine.EndTime.TotalMilliseconds >= lineMiddle)
            {
                return originalLine;
            }

            var originalMiddle = (originalLine.StartTime.TotalMilliseconds + originalLine.EndTime.TotalMilliseconds) / 2.0;
            if (line.StartTime.TotalMilliseconds <= originalMiddle && line.EndTime.TotalMilliseconds >= originalMiddle)
            {
                return originalLine;
            }
        }

        return null;
    }
}
