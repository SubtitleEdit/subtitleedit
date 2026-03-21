using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public static class PlainTextSplitter
{
    public static List<SubtitleLineViewModel> AutomaticSplit(string plainText, int maxNumberOfLines, int singleLineMaximumLength)
    {
        var subtitles = new List<SubtitleLineViewModel>();

        if (string.IsNullOrWhiteSpace(plainText))
        {
            return subtitles;
        }

        var paragraphs = plainText.SplitToLines();

        foreach (var paragraph in paragraphs)
        {
            var trimmedParagraph = paragraph.Trim();
            if (string.IsNullOrWhiteSpace(trimmedParagraph))
            {
                continue;
            }

            var lines = SplitParagraphIntoLines(trimmedParagraph, maxNumberOfLines, singleLineMaximumLength);
            subtitles.AddRange(lines);
        }

        return subtitles;
    }

    private static List<SubtitleLineViewModel> SplitParagraphIntoLines(string text, int maxNumberOfLines, int singleLineMaximumLength)
    {
        var result = new List<SubtitleLineViewModel>();
        var remainingText = text;

        while (!string.IsNullOrWhiteSpace(remainingText))
        {
            var subtitleText = ExtractNextSubtitle(remainingText, maxNumberOfLines, singleLineMaximumLength, out var usedLength);
            if (!string.IsNullOrWhiteSpace(subtitleText))
            {
                result.Add(new SubtitleLineViewModel { Text = subtitleText.Trim() });
            }

            remainingText = remainingText.Substring(usedLength).TrimStart();
        }

        return result;
    }

    private static string ExtractNextSubtitle(string text, int maxNumberOfLines, int singleLineMaximumLength, out int usedLength)
    {
        var lines = new List<string>();
        var currentPos = 0;

        for (int lineNumber = 0; lineNumber < maxNumberOfLines && currentPos < text.Length; lineNumber++)
        {
            var remainingText = text.Substring(currentPos);
            var line = ExtractNextLine(remainingText, singleLineMaximumLength, lineNumber == maxNumberOfLines - 1, out var lineLength);

            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            lines.Add(line.Trim());
            currentPos += lineLength;

            if (currentPos >= text.Length || IsNaturalBreakPoint(text, currentPos))
            {
                break;
            }
        }

        usedLength = currentPos;
        return string.Join(Environment.NewLine, lines);
    }

    private static string ExtractNextLine(string text, int maxLength, bool isLastLine, out int usedLength)
    {
        if (text.Length <= maxLength)
        {
            usedLength = text.Length;
            return text;
        }

        var bestBreakPoint = FindBestBreakPoint(text, maxLength, isLastLine);

        if (bestBreakPoint > 0)
        {
            usedLength = bestBreakPoint;
            return text.Substring(0, bestBreakPoint);
        }

        var spaceIndex = text.LastIndexOf(' ', Math.Min(maxLength, text.Length - 1));
        if (spaceIndex > maxLength * 0.6)
        {
            usedLength = spaceIndex + 1;
            return text.Substring(0, spaceIndex);
        }

        usedLength = Math.Min(maxLength, text.Length);
        return text.Substring(0, usedLength);
    }

    private static int FindBestBreakPoint(string text, int maxLength, bool isLastLine)
    {
        var searchLength = Math.Min(maxLength, text.Length);

        for (int i = searchLength; i > 0; i--)
        {
            if (i >= text.Length)
            {
                continue;
            }

            var ch = text[i];
            var prevCh = i > 0 ? text[i - 1] : ' ';

            if (prevCh == '.' || prevCh == '!' || prevCh == '?' || prevCh == '…')
            {
                if (char.IsWhiteSpace(ch) || i == text.Length - 1)
                {
                    return i;
                }
            }
        }

        if (!isLastLine && searchLength >= maxLength * 0.85)
        {
            for (int i = (int)(maxLength * 0.85); i < searchLength; i++)
            {
                if (i >= text.Length)
                {
                    break;
                }

                var ch = text[i];
                if (ch == ',' || ch == ';' || ch == ':')
                {
                    if (i + 1 < text.Length && char.IsWhiteSpace(text[i + 1]))
                    {
                        return i + 1;
                    }
                }
            }
        }

        for (int i = searchLength; i > maxLength * 0.6; i--)
        {
            if (i >= text.Length)
            {
                continue;
            }

            if (char.IsWhiteSpace(text[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsNaturalBreakPoint(string text, int position)
    {
        if (position <= 0 || position >= text.Length)
        {
            return true;
        }

        var prevCh = text[position - 1];
        return prevCh == '.' || prevCh == '!' || prevCh == '?' || prevCh == '…';
    }
}
