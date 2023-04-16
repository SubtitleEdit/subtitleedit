using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic
{
    public static class ReplaceAllHelper
    {
        internal static int ReplaceAll(FindReplaceDialogHelper findHelper, Subtitle subtitle, Subtitle subtitleOriginal, bool allowEditOfOriginalSubtitle, int stopAtIndex)
        {
            if (findHelper.FindReplaceType.FindType == FindType.RegEx)
            {
                var count = 0;

                if (findHelper.FindReplaceType.SearchTranslation)
                {
                    count += ReplaceAllRegEx(findHelper, subtitle, stopAtIndex);
                }
                
                if (allowEditOfOriginalSubtitle && findHelper.FindReplaceType.SearchOriginal && subtitleOriginal?.Paragraphs.Count > 0)
                {
                    count += ReplaceAllRegEx(findHelper, subtitleOriginal, stopAtIndex);
                }

                return count;
            }

            var replaceCount = 0;
            if (findHelper.FindReplaceType.SearchTranslation)
            {
                replaceCount += ReplaceAllNonRegEx(findHelper, subtitle, stopAtIndex);
            }
            
            if (allowEditOfOriginalSubtitle && findHelper.FindReplaceType.SearchOriginal && subtitleOriginal?.Paragraphs.Count > 0)
            {
                replaceCount += ReplaceAllNonRegEx(findHelper, subtitleOriginal, stopAtIndex);
            }

            return replaceCount;
        }

        private static int ReplaceAllNonRegEx(FindReplaceDialogHelper findHelper, Subtitle subtitle, int stopAtIndex)
        {
            var replaceCount = 0;
            for (var i = Math.Max(0, findHelper.StartLineIndex); i < subtitle.Paragraphs.Count; i++)
            {
                if (i >= stopAtIndex)
                {
                    break;
                }

                var p = subtitle.Paragraphs[i];
                var start = -1;
                while (findHelper.FindNext(p.Text, start))
                {
                    p.Text = p.Text.Remove(findHelper.SelectedLineIndex, findHelper.FindTextLength).Insert(findHelper.SelectedLineIndex, findHelper.ReplaceText);
                    start = findHelper.SelectedLineIndex + findHelper.ReplaceText.Length -1;
                    replaceCount++;
                }
            }

            return replaceCount;
        }

        private static int ReplaceAllRegEx(FindReplaceDialogHelper findHelper, Subtitle subtitle, int stopAtIndex)
        {
            var replaceCount = 0;
            for (var i = Math.Max(0, findHelper.StartLineIndex); i < subtitle.Paragraphs.Count; i++)
            {
                if (i >= stopAtIndex)
                {
                    break;
                }

                var p = subtitle.Paragraphs[i];
                var before = p.Text;
                var r = new Regex(RegexUtils.FixNewLine(findHelper.FindText), RegexOptions.Multiline);
                p.Text = RegexUtils.ReplaceNewLineSafe(r, p.Text, findHelper.ReplaceText);
                if (before != p.Text)
                {
                    replaceCount += RegexUtils.CountNewLineSafe(r, before);
                }
            }

            return replaceCount;
        }
    }
}
