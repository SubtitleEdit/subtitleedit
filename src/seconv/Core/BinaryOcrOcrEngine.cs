using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// In-process OCR via Subtitle Edit's BinaryOCR matcher. Requires a <c>.db</c>
/// database file (typically shipped with SE under <c>%AppData%\Subtitle Edit\Ocr\</c>;
/// pass the path via <c>--ocrdb</c>). BinaryOCR uses fast bitmap-hash matching and
/// is a useful alternative to nOCR — different accuracy profile, similar speed.
/// </summary>
internal sealed class BinaryOcrOcrEngine : IOcrEngine
{
    public string Name => "binaryocr";

    private readonly BinaryOcrDb _db;
    private readonly BinaryOcrMatcher _matcher;
    private const int PixelsAreSpaceDefault = 12;
    private const double MaxErrorPercent = 0.5;

    public BinaryOcrOcrEngine(string dbPath)
    {
        if (!File.Exists(dbPath))
        {
            throw new FileNotFoundException(
                $"BinaryOCR database not found: {dbPath}. Use --ocrdb to point to a .db file " +
                "(typically %AppData%\\Subtitle Edit\\Ocr\\Latin.db or similar).", dbPath);
        }
        _db = new BinaryOcrDb(dbPath, loadCompareImages: true);
        if (_db.AllCompareImages.Count == 0)
        {
            throw new InvalidOperationException($"BinaryOCR database is empty: {dbPath}");
        }
        _matcher = new BinaryOcrMatcher
        {
            IsLatinDb = Path.GetFileNameWithoutExtension(dbPath).Contains("Latin", StringComparison.OrdinalIgnoreCase),
        };
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

        var matches = new List<BinaryOcrMatcher.CompareMatch>();
        var i = 0;
        while (i < letters.Count)
        {
            var item = letters[i];
            if (item.NikseBitmap == null)
            {
                if (item.SpecialCharacter != null)
                {
                    matches.Add(new BinaryOcrMatcher.CompareMatch(item.SpecialCharacter, false, 0, nameof(item.SpecialCharacter)));
                }
            }
            else
            {
                var match = _matcher.GetCompareMatch(item, out _, letters, i, _db, MaxErrorPercent);
                if (match is { ExpandCount: > 0 })
                {
                    i += match.ExpandCount - 1;
                }

                if (match == null)
                {
                    matches.Add(new BinaryOcrMatcher.CompareMatch("*", false, 0, null));
                }
                else
                {
                    matches.Add(new BinaryOcrMatcher.CompareMatch(match.Text, match.Italic, match.ExpandCount, match.Name));
                }
            }
            i++;
        }

        return ItalicTextMerger.MergeWithItalicTags(matches).Trim();
    }

    public void Dispose() { /* no-op */ }
}
