using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core
{
    public class TextSplit
    {
        private readonly List<TextSplitResult> _splits;
        private readonly List<TextSplitResult> _allSplits;
        private readonly string _language;
        private readonly int _singleLineMaxLength;
        private const string EndLineChars = ".!?)]…؟";
        private const string Commas = ",،";

        public TextSplit(string text, int singleLineMaxLength, string language)
        {
            _singleLineMaxLength = singleLineMaxLength;
            _language = language;

            // create list with all split possibilities
            _splits = new List<TextSplitResult>();
            _allSplits = new List<TextSplitResult>();
            for (int i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == ' ')
                {
                    var l1 = text.Substring(0, i).Trim();
                    var l2 = text.Substring(i + 1).Trim();
                    _allSplits.Add(new TextSplitResult(new List<string> { l1, l2 }));
                    if (Utilities.CanBreak(text, i, language))
                    {
                        _splits.Add(new TextSplitResult(new List<string> { l1, l2 }));
                    }
                }
            }
        }

        public string AutoBreak(bool breakEarlyDialog, bool breakEarlyLineEnding, bool breakEarlyComma, bool usePixelWidth)
        {
            if (breakEarlyDialog)
            {
                var s = GetBestDialog();
                if (s != null)
                {
                    return s;
                }
            }
            if (breakEarlyLineEnding)
            {
                var s = GetBestEnding(EndLineChars);
                if (s != null)
                {
                    return s;
                }
            }
            if (breakEarlyComma)
            {
                var s = GetBestEnding(Commas);
                if (s != null)
                {
                    return s;
                }
            }
            if (usePixelWidth)
            {
                var s = GetBestPixelSplit();
                if (s != null)
                {
                    return s;
                }
            }

            return GetBestLengthSplit();
        }

        private string GetBestDialog()
        {
            var orderedArray = _splits.Where(IsDialog).OrderBy(p => p.DiffFromAveragePixel());
            var best = orderedArray.FirstOrDefault();
            return best != null ? string.Join(Environment.NewLine, best.Lines) : null;
        }

        private string GetBestEnding(string chars)
        {
            var orderedArray = _splits.Where(p => p.IsLineLengthOkay(_singleLineMaxLength) && EndsWith(p, chars)).OrderBy(p => p.DiffFromAveragePixel());
            var best = orderedArray.FirstOrDefault();
            return best != null ? string.Join(Environment.NewLine, best.Lines) : null;
        }

        private string GetBestPixelSplit()
        {
            var orderedArray = _splits.Where(p => p.IsLineLengthOkay(_singleLineMaxLength)).OrderBy(p => p.DiffFromAveragePixel());
            var best = orderedArray.FirstOrDefault();
            if (best != null && !best.IsBottomHeavy && Configuration.Settings.Tools.AutoBreakUsePixelWidth && Configuration.Settings.Tools.AutoBreakPreferBottomHeavy)
            {
                orderedArray = _splits.Where(p => p.IsLineLengthOkay(_singleLineMaxLength)).OrderBy(p => p.DiffFromAveragePixelBottomHeavy());
                best = orderedArray.FirstOrDefault() ?? best;
            }
            return best != null ? string.Join(Environment.NewLine, best.Lines) : null;
        }

        private string GetBestLengthSplit()
        {
            var orderedArray = _splits.OrderBy(p => p.DiffFromAverage());
            var best = orderedArray.FirstOrDefault();
            if (best != null)
            {
                return string.Join(Environment.NewLine, best.Lines);
            }

            orderedArray = _allSplits.OrderBy(p => p.DiffFromAverage());
            best = orderedArray.FirstOrDefault();
            return best != null ? string.Join(Environment.NewLine, best.Lines) : null;
        }

        private bool EndsWith(TextSplitResult textSplitResult, string chars)
        {
            if (textSplitResult.Lines.Count == 0)
            {
                return false;
            }

            string l = textSplitResult.Lines[0];

            // arabic 
            if (_language.Equals("ar", StringComparison.OrdinalIgnoreCase))
            {
                l = l.TrimStart('"', '\'');
                return l.Length > 0 && chars.Contains(l[0]);
            }

            l = l.TrimEnd('"', '\'');
            return l.Length > 0 && chars.Contains(l[l.Length - 1]);
        }

        private bool IsDialog(TextSplitResult textSplitResult)
        {
            if (textSplitResult.Lines.Count == 0)
            {
                return false;
            }

            string l1 = string.Empty;
            string l2 = string.Empty;

            // arabic
            if (_language.Equals("ar", StringComparison.OrdinalIgnoreCase))
            {
                l1 = textSplitResult.Lines[0].TrimStart('"', '\'');
                l2 = textSplitResult.Lines[1].TrimEnd('"', '\'');
                return l1.Length > 0 && l2.EndsWith('-') && EndLineChars.Contains(l1[0]);
            }

            l1 = textSplitResult.Lines[0].TrimEnd('"', '\'');
            l2 = textSplitResult.Lines[1].TrimStart('"', '\'');
            return l1.Length > 0 && l2.StartsWith('-') && EndLineChars.Contains(l1[l1.Length - 1]);
        }
    }
}
