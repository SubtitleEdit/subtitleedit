using Nikse.SubtitleEdit.Core.Dictionaries;
using System;
using System.IO;

namespace UITests.Features.SpellCheck;

// Former countries were missing from the names lists, so name casing never fixed them (#12465).
// Portuguese ships its lists per region (pt_PT_names.xml / pt_BR_names.xml) and has no neutral
// pt_names.xml, so NameList - which reduced the language to its two letter code - never loaded them.
public class FormerCountryNamesTests
{
    private static string DictionariesFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Dictionaries")))
        {
            dir = dir.Parent;
        }

        return Path.Combine(dir!.FullName, "Dictionaries");
    }

    private static bool HasName(string language, string name)
    {
        var nameList = new NameList(DictionariesFolder(), language, false, string.Empty);
        return nameList.GetNames().Contains(name) || nameList.GetMultiNames().Contains(name);
    }

    [Theory]
    [InlineData("en_US", "Yugoslavia")]
    [InlineData("en_US", "Czechoslovakia")]
    [InlineData("en_US", "Soviet Union")]
    [InlineData("nl_NL", "Joegoslavië")]
    [InlineData("nl_NL", "Tsjecho-Slowakije")]
    [InlineData("de_DE", "Jugoslawien")]
    [InlineData("de_DE", "Tschechoslowakei")]
    [InlineData("fr_FR", "Yougoslavie")]
    [InlineData("fr_FR", "Tchécoslovaquie")]
    [InlineData("es_ES", "Checoslovaquia")]
    [InlineData("es_ES", "Unión Soviética")]
    [InlineData("pt_PT", "Jugoslávia")]
    [InlineData("pt_PT", "Checoslováquia")]
    [InlineData("pt_BR", "Iugoslávia")]
    [InlineData("pt_BR", "Tchecoslováquia")]
    public void FormerCountryIsInTheNamesList(string language, string name)
    {
        Assert.True(HasName(language, name), $"{name} is not in the {language} names list");
    }

    [Theory]
    [InlineData("pt_PT")]
    [InlineData("pt_BR")]
    public void RegionSpecificNamesFileIsLoaded(string language)
    {
        // Guards the region specific load. "Abelardo" is in pt_PT_names.xml and pt_BR_names.xml but
        // not in the shared names.xml, so it can only be found when the region file itself is read.
        Assert.True(HasName(language, "Abelardo"), $"the {language} names file was not loaded");
    }
}
