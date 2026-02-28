﻿using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public static class TextDiffHighlighter
{
    private static IBrush GetForegroundDifferenceColor()
    {
        if (UiTheme.IsDarkThemeEnabled())
        {
            // Bright red for dark theme - easier to read
            return new SolidColorBrush(Color.FromRgb(255, 100, 100));
        }
        // Dark red for light theme
        return new SolidColorBrush(Color.FromRgb(183, 28, 28));
    }

    private static IBrush GetBackDifferenceColor()
    {
        if (UiTheme.IsDarkThemeEnabled())
        {
            // Darker red background for dark theme
            return new SolidColorBrush(Color.FromRgb(80, 30, 30));
        }
        // Light pink background for light theme
        return new SolidColorBrush(Color.FromRgb(255, 235, 238));
    }

    private static IBrush GetDiffBackgroundColor()
    {
        if (UiTheme.IsDarkThemeEnabled())
        {
            // Darker green background for dark theme
            return new SolidColorBrush(Color.FromRgb(30, 60, 30));
        }
        // Light green background for light theme
        return new SolidColorBrush(Color.FromRgb(230, 255, 237));
    }

    public static (TextBlock left, TextBlock right) Compare(string text1, string text2)
    {
        var left = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var right = new TextBlock { TextWrapping = TextWrapping.Wrap };

        if (left.Inlines == null || right.Inlines == null)
        {
            return (left, right);
        }

        if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
        {
            return (left, right);
        }

        if (string.IsNullOrEmpty(text1))
        {
            right.Inlines.Add(new Run(text2)
            {
                Foreground = GetForegroundDifferenceColor(),
                Background = GetBackDifferenceColor()
            });
            return (left, right);
        }

        if (string.IsNullOrEmpty(text2))
        {
            left.Inlines.Add(new Run(text1)
            {
                Foreground = GetForegroundDifferenceColor(),
                Background = GetBackDifferenceColor()
            });
            return (left, right);
        }

        // Use longest common substring to find the best match
        var (commonStart, commonEnd, middleCommon1, middleCommon2) = FindCommonParts(text1, text2);

        bool hasDifferences = commonStart != text1.Length || commonEnd != 0 || middleCommon1.Count > 0;

        if (!hasDifferences)
        {
            // Texts are identical - don't set Foreground to allow theme color inheritance
            left.Inlines.Add(new Run(text1));
            right.Inlines.Add(new Run(text2));
            return (left, right);
        }

        // Build the visual representation
        BuildDiffRuns(left, text1, commonStart, commonEnd, middleCommon1, hasDifferences);
        BuildDiffRuns(right, text2, commonStart, commonEnd, middleCommon2, hasDifferences);

        return (left, right);
    }

    private static void BuildDiffRuns(TextBlock textBlock, string text, int commonStart, int commonEnd, 
        List<(int start, int length)> middleCommon, bool hasDifferences)
    {
        int currentPos = 0;

        // Add prefix if common
        if (commonStart > 0)
        {
            textBlock.Inlines!.Add(new Run(text.Substring(0, commonStart))
            {
                Background = hasDifferences ? GetDiffBackgroundColor() : null
            });
            currentPos = commonStart;
        }

        // Add middle parts (mix of common and different)
        int middleEnd = text.Length - commonEnd;
        if (middleCommon.Count > 0)
        {
            foreach (var (start, length) in middleCommon)
            {
                // Add different part before this common part
                if (start > currentPos)
                {
                    textBlock.Inlines!.Add(new Run(text.Substring(currentPos, start - currentPos))
                    {
                        Foreground = GetForegroundDifferenceColor(),
                        Background = GetBackDifferenceColor()
                    });
                }

                // Add common part
                textBlock.Inlines!.Add(new Run(text.Substring(start, length))
                {
                    Background = GetDiffBackgroundColor()
                });

                currentPos = start + length;
            }
        }

        // Add remaining different part in the middle
        if (currentPos < middleEnd)
        {
            textBlock.Inlines!.Add(new Run(text.Substring(currentPos, middleEnd - currentPos))
            {
                Foreground = GetForegroundDifferenceColor(),
                Background = GetBackDifferenceColor()
            });
            currentPos = middleEnd;
        }

        // Add suffix if common
        if (commonEnd > 0)
        {
            textBlock.Inlines!.Add(new Run(text.Substring(text.Length - commonEnd))
            {
                Background = hasDifferences ? GetDiffBackgroundColor() : null
            });
        }
    }

    private static (int commonStart, int commonEnd, List<(int start, int length)> middleCommon1, List<(int start, int length)> middleCommon2) FindCommonParts(string text1, string text2)
    {
        int commonStart = 0;
        int minLength = Math.Min(text1.Length, text2.Length);

        // Find common prefix
        while (commonStart < minLength && text1[commonStart] == text2[commonStart])
        {
            commonStart++;
        }

        // If everything matches
        if (commonStart == text1.Length && commonStart == text2.Length)
        {
            return (commonStart, 0, new List<(int, int)>(), new List<(int, int)>());
        }

        // Find common suffix
        int commonEnd = 0;
        while (commonEnd < minLength - commonStart &&
               text1[text1.Length - 1 - commonEnd] == text2[text2.Length - 1 - commonEnd])
        {
            commonEnd++;
        }

        // Find common parts in the middle using longest common substring
        var middleCommon1 = new List<(int start, int length)>();
        var middleCommon2 = new List<(int start, int length)>();

        if (commonStart < text1.Length && commonStart < text2.Length)
        {
            string middle1 = text1.Substring(commonStart, text1.Length - commonStart - commonEnd);
            string middle2 = text2.Substring(commonStart, text2.Length - commonStart - commonEnd);

            if (middle1.Length > 0 && middle2.Length > 0)
            {
                // Find longest common substrings in the middle
                var commonSubstrings = FindLongestCommonSubstrings(middle1, middle2, 3); // Minimum length of 3

                // Convert positions back to original text positions and store both
                foreach (var (pos1, pos2, length) in commonSubstrings)
                {
                    middleCommon1.Add((commonStart + pos1, length));
                    middleCommon2.Add((commonStart + pos2, length));
                }
            }
        }

        return (commonStart, commonEnd, middleCommon1, middleCommon2);
    }

    private static List<(int pos1, int pos2, int length)> FindLongestCommonSubstrings(string s1, string s2, int minLength)
    {
        var result = new List<(int pos1, int pos2, int length)>();
        var used1 = new bool[s1.Length];
        var used2 = new bool[s2.Length];

        // Find multiple common substrings
        while (true)
        {
            int maxLen = 0;
            int maxPos1 = 0;
            int maxPos2 = 0;

            // Build LCS table for unused parts
            for (int i = 0; i < s1.Length; i++)
            {
                if (used1[i]) continue;

                for (int j = 0; j < s2.Length; j++)
                {
                    if (used2[j]) continue;

                    int len = 0;
                    while (i + len < s1.Length && j + len < s2.Length &&
                           !used1[i + len] && !used2[j + len] &&
                           s1[i + len] == s2[j + len])
                    {
                        len++;
                    }

                    if (len > maxLen)
                    {
                        maxLen = len;
                        maxPos1 = i;
                        maxPos2 = j;
                    }
                }
            }

            // If no common substring found or too short, stop
            if (maxLen < minLength)
            {
                break;
            }

            // Mark as used
            for (int k = 0; k < maxLen; k++)
            {
                used1[maxPos1 + k] = true;
                used2[maxPos2 + k] = true;
            }

            result.Add((maxPos1, maxPos2, maxLen));
        }

        // Sort by position in first string
        result.Sort((a, b) => a.pos1.CompareTo(b.pos1));

        return result;
    }
}
