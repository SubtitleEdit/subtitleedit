using System.Text.RegularExpressions;
using System.Xml.Linq;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// OcrFixReplaceList2 skips a "RegularExpressions" entry when the line cannot contain a literal
/// every match needs. A literal that is not really required would silently turn a fix off, so
/// these tests pin the extractor on hand-picked shapes and check it against every shipped list.
/// </summary>
public class OcrFixRegexRequiredLiteralTests
{
    [Theory]
    [InlineData("aćemo", "aćemo")]
    [InlineData(@"\badvokat(?![si])", "advokat")]
    [InlineData("([aA])kcion(?!ar)", "kcion")]
    [InlineData("lj?ezd", "ezd")]
    [InlineData("ab*cd", "cd")]
    [InlineData("ab+cd", "ab")]
    [InlineData("a.bc", "bc")]
    [InlineData(@"x\.yz", "yz")]
    [InlineData(@"\sno\s", "no")]
    [InlineData("[žŽ]ude([lt])", "ude")]
    [InlineData("(ab|cd)ef", "ef")]
    [InlineData("[]a]bc", "bc")]
    public void FindsRequiredLiteral(string pattern, string expected)
    {
        Assert.Equal(expected, OcrFixReplaceList2.GetRequiredLiteral(pattern));
    }

    [Theory]
    [InlineData("ab|cd")] // top-level alternation - neither side is required
    [InlineData("abc|x(yz)")]
    [InlineData("(?i)abc")] // inline options change case sensitivity
    [InlineData("x(?i:y)abc")]
    [InlineData("ab{0,1}c")]
    [InlineData(@"\p{Lu}abc")]
    [InlineData(@"(a)\1bc")]
    [InlineData(@"\x41bc")]
    [InlineData("a?b?c?")]
    [InlineData("[a-z-[aeiou]]bc")]
    [InlineData("abc)")]
    [InlineData("(abc")]
    [InlineData("a")]
    public void GivesUpWhenNothingIsSurelyRequired(string pattern)
    {
        Assert.Null(OcrFixReplaceList2.GetRequiredLiteral(pattern));
    }

    public static TheoryData<string> ReplaceListFiles()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.GetFiles(DictionariesFolder(), "*_OCRFixReplaceList.xml").OrderBy(f => f))
        {
            data.Add(Path.GetFileName(file));
        }

        return data;
    }

    /// <summary>
    /// Every match of every shipped expression, over a corpus made of all the text in the same
    /// file (from/to values, replacements), must contain the extracted literal.
    /// </summary>
    [Theory]
    [MemberData(nameof(ReplaceListFiles))]
    public void ShippedExpressionsNeverMatchWithoutTheirLiteral(string fileName)
    {
        var doc = XDocument.Load(Path.Combine(DictionariesFolder(), fileName));
        var patterns = doc.Descendants("RegularExpressions").Elements("RegEx")
            .Select(e => (string?)e.Attribute("find"))
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p!)
            .ToList();

        var corpus = doc.Descendants()
            .SelectMany(e => e.Attributes())
            .Select(a => Regex.Replace(a.Value, @"\$\d", string.Empty))
            .Where(v => v.Length > 0)
            .Distinct()
            .ToList();
        var lines = new List<string>(corpus);
        for (var i = 0; i + 3 < corpus.Count; i += 3)
        {
            lines.Add(corpus[i] + " " + corpus[i + 1] + Environment.NewLine + corpus[i + 2] + " " + corpus[i + 3]);
        }

        foreach (var pattern in patterns)
        {
            Regex regex;
            try
            {
                regex = new Regex(pattern, RegexOptions.Multiline);
            }
            catch (ArgumentException)
            {
                continue; // the loader drops invalid expressions
            }

            var literal = OcrFixReplaceList2.GetRequiredLiteral(pattern);
            if (literal == null)
            {
                continue;
            }

            foreach (var line in lines)
            {
                foreach (Match match in regex.Matches(line))
                {
                    Assert.True(match.Value.Contains(literal, StringComparison.Ordinal),
                        $"{fileName}: \"{pattern}\" matched \"{match.Value}\" without \"{literal}\"");
                }
            }
        }
    }

    private static string DictionariesFolder()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "Dictionaries");
    }
}
