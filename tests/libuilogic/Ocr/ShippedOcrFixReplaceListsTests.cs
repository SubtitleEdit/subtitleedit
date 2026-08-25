using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Guards the shipped <c>Dictionaries/&lt;lang&gt;_OCRFixReplaceList.xml</c> files. The loaders
/// drop bad entries silently - an invalid regex, a "$1" without a capture group, a gated rule
/// missing its spellCheck attribute, a from==to pair, or an entry shadowed by an earlier key all
/// look like the fix "just doesn't work" at runtime. A 2026-08 audit found ~115 such entries,
/// some of them years old; this test turns every one of those defect classes into a CI failure.
/// </summary>
public class ShippedOcrFixReplaceListsTests
{
    // The section names OcrFixReplaceList2/SpellCheckRegex read. Anything else in a shipped
    // file is dead weight the loaders never look at.
    private static readonly string[] PairSections =
    {
        "WholeWords", "PartialWordsAlways", "PartialWords", "PartialLines",
        "PartialLinesAlways", "BeginLines", "EndLines", "WholeLines",
    };

    private static readonly string[] RegexSections =
    {
        "RegularExpressions", "RegularExpressionsIfSpelledCorrectly",
    };

    // PartialWords is loaded as a pair list where several readings of the same OCR artifact are
    // by design (deu ships i->t and i->l); every other pair section is a first-wins dictionary.
    private static readonly string[] MultiReadingSections = { "PartialWords" };

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    private static string DictionariesFolder() => Path.Combine(FindRepoRoot(), "Dictionaries");

    public static TheoryData<string> ReplaceListFiles()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.GetFiles(DictionariesFolder(), "*_OCRFixReplaceList.xml").OrderBy(f => f))
        {
            data.Add(Path.GetFileName(file));
        }

        return data;
    }

    [Fact]
    public void ThereAreReplaceListsToCheck()
    {
        Assert.NotEmpty(Directory.GetFiles(DictionariesFolder(), "*_OCRFixReplaceList.xml"));
    }

    [Theory]
    [MemberData(nameof(ReplaceListFiles))]
    public void EveryEntryIsLoadableAndReachable(string fileName)
    {
        var root = XDocument.Load(Path.Combine(DictionariesFolder(), fileName)).Root;
        Assert.NotNull(root);
        Assert.Equal("OCRFixReplaceList", root!.Name.LocalName);

        var knownSections = PairSections.Concat(RegexSections).ToHashSet();
        foreach (var section in root.Elements())
        {
            Assert.True(knownSections.Contains(section.Name.LocalName),
                $"{fileName}: unknown section <{section.Name.LocalName}> is never read by the loader");
        }

        // Duplicate same-name sections are legal (the loaders merge them), so all checks run on
        // the merged per-name view - a duplicate hiding in a second section block must still fail.
        foreach (var name in PairSections)
        {
            CheckPairSection(fileName, name, root.Elements(name).SelectMany(s => s.Elements()));
        }

        foreach (var name in RegexSections)
        {
            CheckRegexSection(fileName, name, root.Elements(name).SelectMany(s => s.Elements()));
        }
    }

    private static void CheckPairSection(string fileName, string sectionName, IEnumerable<XElement> items)
    {
        var seenPairs = new HashSet<(string From, string To)>();
        var seenKeys = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var where = $"{fileName} <{sectionName}>";
            var from = item.Attribute("from")?.Value;
            var to = item.Attribute("to")?.Value;
            Assert.True(from != null && to != null, $"{where}: entry without from/to is skipped: {item}");
            Assert.False(from!.Length == 0, $"{where}: empty from: {item}");
            Assert.False(from == to, $"{where}: from==to '{from}' is a no-op the loader skips");
            Assert.True(seenPairs.Add((from, to!)),
                $"{where}: exact duplicate '{from}' -> '{to}'");

            if (!MultiReadingSections.Contains(sectionName))
            {
                Assert.False(seenKeys.TryGetValue(from, out var firstTo),
                    $"{where}: '{from}' -> '{to}' is dead, first-wins already maps it to '{seenKeys.GetValueOrDefault(from)}'");
                seenKeys[from] = to!;
            }
        }
    }

    private static void CheckRegexSection(string fileName, string sectionName, IEnumerable<XElement> items)
    {
        var gated = sectionName == "RegularExpressionsIfSpelledCorrectly";
        var seenFinds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var where = $"{fileName} <{sectionName}>";
            var find = item.Attribute("find")?.Value;
            var replaceWith = item.Attribute("replaceWith")?.Value;
            var spellCheck = item.Attribute("spellCheck")?.Value;
            Assert.False(string.IsNullOrEmpty(find), $"{where}: entry without find is skipped: {item}");
            Assert.NotNull(replaceWith);
            if (gated)
            {
                // SpellCheckRegex.LoadRegExNodes requires all three attributes and skips the
                // rule silently otherwise - 11 shipped rules were dead this way for months.
                Assert.False(string.IsNullOrEmpty(spellCheck),
                    $"{where}: gated rule without spellCheck is silently skipped: {item}");
            }

            Regex regex;
            try
            {
                regex = new Regex(find!);
            }
            catch (ArgumentException e)
            {
                Assert.Fail($"{where}: find='{find}' does not compile and is silently dropped: {e.Message}");
                return;
            }

            // An unmatched $N in a .NET replacement is emitted literally, so a bad group
            // reference corrupts the subtitle text instead of fixing it.
            var groups = regex.GetGroupNumbers();
            foreach (var (attrName, value) in new[] { ("replaceWith", replaceWith), ("spellCheck", spellCheck) })
            {
                if (value == null)
                {
                    continue;
                }

                foreach (Match reference in Regex.Matches(value.Replace("$$", string.Empty), @"\$(\d+)"))
                {
                    var n = int.Parse(reference.Groups[1].Value);
                    Assert.True(groups.Contains(n),
                        $"{where}: {attrName}='{value}' references ${n} but find='{find}' has no group {n}");
                }
            }

            if (gated)
            {
                // Gated rules are a list, so several fixes for the same find are legal (each
                // candidate is dictionary-checked) - only an identical rule is a defect.
                var signature = find + "\n" + spellCheck + "\n" + replaceWith + "\n"
                    + item.Attribute("replaceAllFrom")?.Value + "\n" + item.Attribute("replaceAllTo")?.Value;
                Assert.True(seenFinds.Add(signature),
                    $"{where}: identical duplicate rule find='{find}' is applied twice");
            }
            else
            {
                // RegularExpressions is a first-wins dictionary - a repeated find is dead.
                Assert.True(seenFinds.Add(find!),
                    $"{where}: duplicate find='{find}' - the later entry is dead");
            }
        }
    }
}
