namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Italic glyphs lean over the word gaps, so the letter splitter's straight-column
/// space detection undercounts them. This re-measures the gaps between matched
/// glyphs along the italic slant and inserts the spaces that were missed.
/// Callers should keep the result only if it scores better against the dictionary.
/// </summary>
public static class ItalicSpaceFixer
{
    public static string GetTextWithMoreSpacesInItalic(
        List<BinaryOcrMatcher.CompareMatch> matches,
        List<ImageSplitterItem2> letters,
        NikseBitmap2 parentBitmap,
        double unItalicFactor,
        int pixelsIsSpace)
    {
        // Clear all CouldBeSpaceBefore flags
        foreach (var letter in letters)
        {
            letter.CouldBeSpaceBefore = false;
        }

        // Check for potential spaces in italic text
        for (var i = 0; i < matches.Count - 1; i++)
        {
            var match = matches[i];
            var matchNext = matches[i + 1];
            if (!match.Italic || matchNext.Text == "," ||
                string.IsNullOrWhiteSpace(match.Text) || string.IsNullOrWhiteSpace(matchNext.Text) ||
                match.ImageSplitterItem == null || matchNext.ImageSplitterItem == null)
            {
                continue;
            }

            var blankVerticalLines = IsVerticalAngledLineTransparent(parentBitmap, match.ImageSplitterItem, matchNext.ImageSplitterItem, unItalicFactor);
            if (match.Text == "f" || match.Text == "," || matchNext.Text.StartsWith('y') || matchNext.Text.StartsWith('j'))
            {
                blankVerticalLines++;
            }

            if (blankVerticalLines >= pixelsIsSpace)
            {
                matchNext.ImageSplitterItem.CouldBeSpaceBefore = true;
            }
        }

        // Insert spaces where CouldBeSpaceBefore is true and previous match is italic
        var j = 1;
        while (j < matches.Count)
        {
            var match = matches[j];
            var prevMatch = matches[j - 1];
            if (match.ImageSplitterItem?.CouldBeSpaceBefore == true)
            {
                match.ImageSplitterItem.CouldBeSpaceBefore = false;
                if (prevMatch.Italic)
                {
                    matches.Insert(j, new BinaryOcrMatcher.CompareMatch(" ", false, 0, null));
                    j++; // Skip the inserted space
                }
            }

            j++;
        }

        return ItalicTextMerger.MergeWithItalicTags(matches).Trim();
    }

    public static string GetTextWithMoreSpacesInItalic(
        List<NOcrChar> matches,
        List<ImageSplitterItem2> letters,
        NikseBitmap2 parentBitmap,
        double unItalicFactor,
        int pixelsIsSpace)
    {
        // Clear all CouldBeSpaceBefore flags
        foreach (var letter in letters)
        {
            letter.CouldBeSpaceBefore = false;
        }

        pixelsIsSpace = GetItalicSpaceThreshold(letters, pixelsIsSpace);

        // Check for potential spaces in italic text
        for (var i = 0; i < matches.Count - 1; i++)
        {
            var match = matches[i];
            var matchNext = matches[i + 1];
            if (!match.Italic || matchNext.Text == "," ||
                string.IsNullOrWhiteSpace(match.Text) || string.IsNullOrWhiteSpace(matchNext.Text) ||
                match.ImageSplitterItem == null || matchNext.ImageSplitterItem == null)
            {
                continue;
            }

            var blankVerticalLines = IsVerticalAngledLineTransparent(parentBitmap, match.ImageSplitterItem, matchNext.ImageSplitterItem, unItalicFactor);
            if (match.Text == "f" || match.Text == "," || matchNext.Text.StartsWith('y') || matchNext.Text.StartsWith('j'))
            {
                blankVerticalLines++;
            }

            if (blankVerticalLines >= pixelsIsSpace)
            {
                matchNext.ImageSplitterItem.CouldBeSpaceBefore = true;
            }
        }

        // Insert spaces where CouldBeSpaceBefore is true and previous match is italic
        var j = 1;
        while (j < matches.Count)
        {
            var match = matches[j];
            var prevMatch = matches[j - 1];
            if (match.ImageSplitterItem?.CouldBeSpaceBefore == true)
            {
                match.ImageSplitterItem.CouldBeSpaceBefore = false;
                if (prevMatch.Italic)
                {
                    matches.Insert(j, new NOcrChar(" "));
                    j++; // Skip the inserted space
                }
            }

            j++;
        }

        return ItalicTextMerger.MergeWithItalicTags(matches).Trim();
    }

    /// <summary>
    /// The slanted glyph boxes of an italic line overlap, so the space detector sees few gaps
    /// in it and its threshold can come out as a double space (a 24 px threshold on a 35 px
    /// line: no word gap ever reaches it, and the line stays one word). The angled blank
    /// columns between two italic words measure 0.3-0.5 of the letter height (0.25 of a line
    /// with descenders); between letters at most 0.15. So the threshold is capped at 0.22 of
    /// the tallest glyph.
    /// </summary>
    private static int GetItalicSpaceThreshold(List<ImageSplitterItem2> letters, int pixelsIsSpace)
    {
        var maxHeight = 0;
        foreach (var letter in letters)
        {
            if (letter.NikseBitmap != null && letter.NikseBitmap.Height > maxHeight)
            {
                maxHeight = letter.NikseBitmap.Height;
            }
        }

        if (maxHeight == 0)
        {
            return pixelsIsSpace;
        }

        var fromHeight = Math.Max(2, (int)Math.Round(maxHeight * 0.22, MidpointRounding.AwayFromZero));
        return Math.Min(pixelsIsSpace, fromHeight);
    }

    private static int IsVerticalAngledLineTransparent(NikseBitmap2 parentBitmap, ImageSplitterItem2 match, ImageSplitterItem2 next, double unItalicFactor)
    {
        if (match.NikseBitmap == null || next.NikseBitmap == null)
        {
            return 0;
        }

        var blanks = 0;
        var min = match.X + match.NikseBitmap.Width;
        var max = next.X + next.NikseBitmap.Width / 2;
        for (var startX = min; startX < max; startX++)
        {
            var lineBlank = true;
            for (var y = match.Y; y < match.Y + match.NikseBitmap.Height; y++)
            {
                var x = startX - (y - match.Y) * unItalicFactor;
                if (x >= 0 && x < parentBitmap.Width && y < parentBitmap.Height)
                {
                    var color = parentBitmap.GetPixel((int)Math.Round(x), y);
                    if (color.Alpha != 0)
                    {
                        lineBlank = false;
                        if (blanks > 0)
                        {
                            return blanks;
                        }
                    }
                }
            }

            if (lineBlank)
            {
                blanks++;
            }
        }

        return blanks;
    }
}
