using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// In-process OCR via Subtitle Edit's nOCR matcher. Requires a <c>.nocr</c> database file
/// (typically shipped with SE under <c>%AppData%\Subtitle Edit\Ocr\</c>; pass the path
/// via <c>--ocrdb</c>).
/// </summary>
internal sealed class NOcrOcrEngine : IOcrEngine
{
    public string Name => "nocr";
    private readonly NOcrDb _db;
    private readonly NOcrCaseFixer _caseFixer = new();
    private const int MaxWrongPixels = 25;
    private const int PixelsAreSpaceDefault = 12;

    public NOcrOcrEngine(string nOcrDbPath)
    {
        if (!File.Exists(nOcrDbPath))
        {
            throw new FileNotFoundException(
                $"nOCR database not found: {nOcrDbPath}. Use --ocrdb to point to a .nocr file " +
                "(typically %AppData%\\Subtitle Edit\\Ocr\\Latin.nocr or similar).", nOcrDbPath);
        }
        _db = new NOcrDb(nOcrDbPath);
        if (_db.TotalCharacterCount == 0)
        {
            throw new InvalidOperationException($"nOCR database is empty: {nOcrDbPath}");
        }
    }

    public string Recognize(SKBitmap bitmap)
    {
        if (bitmap is null || bitmap.Width == 0 || bitmap.Height == 0)
        {
            return string.Empty;
        }

        var parent = new NikseBitmap2(bitmap);
        parent.MakeTwoColor(200);
        parent.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(
            parent, PixelsAreSpaceDefault, rightToLeft: false, topToBottom: true, minLineHeight: 20, autoHeight: true);

        var matches = new List<NOcrChar>();
        var i = 0;
        while (i < letters.Count)
        {
            var item = letters[i];
            if (item.NikseBitmap == null)
            {
                if (item.SpecialCharacter != null)
                {
                    matches.Add(new NOcrChar { Text = item.SpecialCharacter, ImageSplitterItem = item });
                }
            }
            else
            {
                var match = _db.GetMatch(parent, letters, item, item.Top, true, MaxWrongPixels);
                if (match is { ExpandCount: > 0 })
                {
                    i += match.ExpandCount - 1;
                }
                if (match == null)
                {
                    matches.Add(new NOcrChar { Text = "*", ImageSplitterItem = item });
                }
                else
                {
                    matches.Add(new NOcrChar
                    {
                        Text = _caseFixer.FixUppercaseLowercaseIssues(item, match),
                        Italic = match.Italic,
                        ExpandCount = match.ExpandCount,
                        ImageSplitterItem = item,
                    });
                }
            }
            i++;
        }

        return ItalicTextMerger.MergeWithItalicTags(matches).Trim();
    }

    public void Dispose() { /* no-op */ }
}
