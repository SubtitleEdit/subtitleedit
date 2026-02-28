using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Logic.Ocr;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrCaseFixer : INOcrCaseFixer
{
    private static readonly HashSet<string> UppercaseLikeLowercase = new() { "V", "W", "U", "S", "Z", "O", "X", "Ø", "C" };
    private static readonly HashSet<string> LowercaseLikeUppercase = new() { "v", "w", "u", "s", "z", "o", "x", "ø", "c" };
    private static readonly HashSet<string> UppercaseWithAccent = new() { "Č", "Š", "Ž", "Ś", "Ż", "Ś", "Ö", "Ü", "Ú", "Ï", "Í", "Ç", "Ì", "Ò", "Ù", "Ó", "Í" };
    private static readonly HashSet<string> LowercaseWithAccent = new() { "č", "š", "ž", "ś", "ż", "ś", "ö", "ü", "ú", "ï", "í", "ç", "ì", "ò", "ù", "ó", "í" };

    private long _ocrLowercaseHeightsTotal;
    private int _ocrLowercaseHeightsTotalCount;
    private long _ocrUppercaseHeightsTotal;
    private int _ocrUppercaseHeightsTotalCount;

    /// <summary>
    /// Fix uppercase/lowercase issues (not I/l)
    /// </summary>
    public string FixUppercaseLowercaseIssues(ImageSplitterItem2 targetItem, NOcrChar result)
    {
        if (targetItem.NikseBitmap == null)
        {
            return result.Text;
        }

        if (result.Text is "e" or "a" or "d" or "t")
        {
            _ocrLowercaseHeightsTotalCount++;
            _ocrLowercaseHeightsTotal += targetItem.NikseBitmap.Height;
            if (_ocrUppercaseHeightsTotalCount < 3)
            {
                _ocrUppercaseHeightsTotalCount++;
                _ocrUppercaseHeightsTotal += targetItem.NikseBitmap.Height + 10;
            }
        }

        if (result.Text is "E" or "H" or "R" or "D" or "T" or "M")
        {
            _ocrUppercaseHeightsTotalCount++;
            _ocrUppercaseHeightsTotal += targetItem.NikseBitmap.Height;
            if (_ocrLowercaseHeightsTotalCount < 3 && targetItem.NikseBitmap.Height > 20)
            {
                _ocrLowercaseHeightsTotalCount++;
                _ocrLowercaseHeightsTotal += targetItem.NikseBitmap.Height - 10;
            }
        }

        if (_ocrLowercaseHeightsTotalCount <= 2 || _ocrUppercaseHeightsTotalCount <= 2)
        {
            return result.Text;
        }

        // Latin letters where lowercase versions look like uppercase version 
        if (UppercaseLikeLowercase.Contains(result.Text))
        {
            var averageLowercase = _ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount;
            var averageUppercase = _ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount;
            if (Math.Abs(averageLowercase - targetItem.NikseBitmap.Height) < Math.Abs(averageUppercase - targetItem.NikseBitmap.Height))
            {
                return result.Text.ToLowerInvariant();
            }

            return result.Text;
        }

        if (LowercaseLikeUppercase.Contains(result.Text))
        {
            var averageLowercase = _ocrLowercaseHeightsTotal / _ocrLowercaseHeightsTotalCount;
            var averageUppercase = _ocrUppercaseHeightsTotal / _ocrUppercaseHeightsTotalCount;
            if (Math.Abs(averageLowercase - targetItem.NikseBitmap.Height) > Math.Abs(averageUppercase - targetItem.NikseBitmap.Height))
            {
                return result.Text.ToUpperInvariant();
            }

            return result.Text;
        }

        if (UppercaseWithAccent.Contains(result.Text))
        {
            var averageUppercase = _ocrUppercaseHeightsTotal / (double)_ocrUppercaseHeightsTotalCount;
            if (targetItem.NikseBitmap.Height < averageUppercase + 3)
            {
                return result.Text.ToLowerInvariant();
            }

            return result.Text;
        }

        if (LowercaseWithAccent.Contains(result.Text))
        {
            var averageUppercase = _ocrUppercaseHeightsTotal / (double)_ocrUppercaseHeightsTotalCount;
            if (targetItem.NikseBitmap.Height > averageUppercase + 4)
            {
                return result.Text.ToUpperInvariant();
            }
        }

        return result.Text;
    }
}