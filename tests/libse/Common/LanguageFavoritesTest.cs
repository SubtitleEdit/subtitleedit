using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class LanguageFavoritesTest
{
    private class Lang
    {
        public string Code { get; init; } = string.Empty;
        public override string ToString() => Code;
    }

    private static List<Lang> Make(params string[] codes) => codes.Select(c => new Lang { Code = c }).ToList();

    [Fact]
    public void ParseCodes_NormalizesAndDedupes()
    {
        var codes = LanguageFavorites.ParseCodes("en; eng ;pt-BR;EN");
        Assert.Equal(new[] { "en", "pt" }, codes.ToArray());
    }

    [Fact]
    public void ParseCodes_EmptyReturnsEmpty()
    {
        Assert.Empty(LanguageFavorites.ParseCodes(null));
        Assert.Empty(LanguageFavorites.ParseCodes(" "));
    }

    [Fact]
    public void NormalizeTwoLetter_HandlesForms()
    {
        Assert.Equal("en", LanguageFavorites.NormalizeTwoLetter("EN"));
        Assert.Equal("pt", LanguageFavorites.NormalizeTwoLetter("pt-BR"));
        Assert.Equal("zh", LanguageFavorites.NormalizeTwoLetter("zh_CN"));
        Assert.Equal("en", LanguageFavorites.NormalizeTwoLetter("eng"));
        Assert.Equal(string.Empty, LanguageFavorites.NormalizeTwoLetter(""));
    }

    [Fact]
    public void OrderWithFavoritesFirst_PutsFavoritesFirstInUserOrder()
    {
        var items = Make("de", "en", "fr", "es");
        var ordered = LanguageFavorites.OrderWithFavoritesFirst(items, x => x.Code, new[] { "es", "en" });
        Assert.Equal(new[] { "es", "en", "de", "fr" }, ordered.Select(x => x.Code).ToArray());
    }

    [Fact]
    public void OrderWithFavoritesFirst_NoFavoritesKeepsOrder()
    {
        var items = Make("de", "en", "fr");
        var ordered = LanguageFavorites.OrderWithFavoritesFirst(items, x => x.Code, new List<string>());
        Assert.Equal(new[] { "de", "en", "fr" }, ordered.Select(x => x.Code).ToArray());
    }

    [Fact]
    public void OrderWithFavoritesFirst_TolerantCodeMatching()
    {
        // item codes are region-qualified / three-letter; favorites are two-letter
        var items = Make("pt-BR", "deu", "en-US");
        var ordered = LanguageFavorites.OrderWithFavoritesFirst(items, x => x.Code, new[] { "de", "pt" });
        Assert.Equal(new[] { "deu", "pt-BR", "en-US" }, ordered.Select(x => x.Code).ToArray());
    }

    [Fact]
    public void OrderWithFavoritesFirst_PinTopStaysAboveFavorites()
    {
        var items = Make("auto", "de", "en");
        var ordered = LanguageFavorites.OrderWithFavoritesFirst(
            items, x => x.Code, new[] { "en" }, x => x.Code == "auto");
        Assert.Equal(new[] { "auto", "en", "de" }, ordered.Select(x => x.Code).ToArray());
    }
}
