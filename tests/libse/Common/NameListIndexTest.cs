using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class NameListIndexTest
{
    private static readonly List<string> Names = new()
    {
        "John Smith", "Mr. Smith", "Jean-Luc", "O'Brien", "New York", "'Tis", "-Dash", "Don", "John",
        "Smith", "US", "Ève", "Straße", "Dr. Jones", "Bob", "bob", "Mary Jane", "Jane", "Ann", "Anna",
        "J.R.R.", "St. Louis", "Louis", "York", "Brien", "Luc", string.Empty,
    };

    private static readonly string[] Lines =
    {
        "john smith met mr. smith in new york.",
        "- don't go, don!\r\n- jean-luc and o'brien are here.",
        "'tis the season, said -dash to bob and BOB.",
        "ève went to the straße with dr. jones",
        "<i>mary jane</i> and jane saw ann and anna",
        "j.r.r. tolkien lived in st. louis, not louis or york",
        "us and the u.s. and US-made",
        "johnjohn smithsmith brien luc jean luc",
        "\"john\" [smith] (bob) “jane” ann's anna?",
        "the end.",
        "{\\an8}john smith",
        "",
        "john",
        "a b c d e f g bob john john john",
    };

    [Fact]
    public void FixCasing_IndexMatchesNameList()
    {
        var index = new NameListIndex(Names);
        foreach (var line in Lines)
        {
            foreach (var changeNameCases in new[] { true, false })
            {
                var expected = new StrippableText(line);
                expected.FixCasing(Names, changeNameCases, true, true, "previous line.");
                var actual = new StrippableText(line);
                actual.FixCasing(index, changeNameCases, true, true, "previous line.");
                Assert.Equal(expected.MergedString, actual.MergedString);
            }
        }
    }

    [Fact]
    public void FixCasing_IndexMatchesNameList_RealNames()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Dictionaries", "en_names.xml")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        var dictionaries = Path.Combine(dir!.FullName, "Dictionaries") + Path.DirectorySeparatorChar;
        var names = new Nikse.SubtitleEdit.Core.Dictionaries.NameList(dictionaries, "en", false, string.Empty).GetAllNames();
        Assert.True(names.Count > 1000, "names: " + names.Count);
        names.Sort((s1, s2) => s2.Length.CompareTo(s1.Length));
        var index = new NameListIndex(names);
        var lines = Lines.Concat(Lines.Select(p => p.ToUpperInvariant())).Concat(new[]
        {
            "i saw tom hanks and julia roberts in los angeles last tuesday.",
            "- did you call mike?\r\n- no, i called sarah and peter jackson.",
            "welcome to new york city, mr. anderson.",
        });
        foreach (var line in lines)
        {
            var expected = new StrippableText(line);
            expected.FixCasing(names, true, false, false, string.Empty);
            var actual = new StrippableText(line);
            actual.FixCasing(index, true, false, false, string.Empty);
            Assert.Equal(expected.MergedString, actual.MergedString);
        }
    }
}
