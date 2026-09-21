using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using System.IO;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Romanian is written with s/t comma below; the shipped "ron" replace list turns the cedilla
/// look-alikes (old code pages, OCR engines) into them, so the words match the spell check
/// dictionary.
/// </summary>
public class RomanianOcrFixReplaceListTests
{
    private static OcrFixReplaceList2 LoadShippedList()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return new OcrFixReplaceList2(Path.Combine(dir!.FullName, "Dictionaries", "ron_OCRFixReplaceList.xml"));
    }

    [Theory]
    [InlineData("şi", "și")]
    [InlineData("viaţă", "viață")]
    [InlineData("Ştiu", "Știu")]
    [InlineData("Ţară", "Țară")]
    [InlineData("ŞTIINŢĂ", "ȘTIINȚĂ")]
    [InlineData("mașină", "mașină")]
    public void CedillaLetters_BecomeCommaBelow(string input, string expected)
    {
        Assert.Equal(expected, LoadShippedList().FixCommonWordErrors(input));
    }
}
