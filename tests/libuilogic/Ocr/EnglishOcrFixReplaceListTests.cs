using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using System.IO;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// nOCR reads the same shape as "p" or "P" by its size and position; in a short bracketed sound
/// label ("[gasps]") the position is unreliable. The English replace list turns a capital P
/// between lowercase letters before an ending back into "p".
/// </summary>
public class EnglishOcrFixReplaceListTests
{
    private static OcrFixReplaceList2 LoadShippedList()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return new OcrFixReplaceList2(Path.Combine(dir!.FullName, "Dictionaries", "eng_OCRFixReplaceList.xml"));
    }

    private static string FixLine(string line)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(line, 0, 1000));
        return LoadShippedList().FixOcrErrorViaLineReplaceList(line, subtitle, 0, null!, new List<string>(), true);
    }

    [Theory]
    [InlineData("[gasPs]", "[gasps]")]
    [InlineData("(yelPs)", "(yelps)")]
    [InlineData("[gasPing]", "[gasping]")]
    [InlineData("[PeePs]", "[Peeps]")]
    [InlineData("He's sleePing.", "He's sleeping.")]
    [InlineData("McPherson and Peter", "McPherson and Peter")]
    [InlineData("GPS", "GPS")]
    public void CapitalPInsideLowercaseWord_IsLowercase(string input, string expected)
    {
        Assert.Equal(expected, FixLine(input));
    }

    [Theory]
    [InlineData("gasPs", "gasps")]
    [InlineData("yelPing", "yelping")]
    public void WholeWords(string input, string expected)
    {
        Assert.Equal(expected, LoadShippedList().FixCommonWordErrors(input));
    }
}
