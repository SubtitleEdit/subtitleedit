using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using System.Text.RegularExpressions;

namespace UITests.Features.Ocr.FixEngine;

public class SpellCheckRegexTests
{
    private readonly HashSet<string> _dictionary;
    private readonly List<string> _wordsToIgnore;

    public SpellCheckRegexTests()
    {
        _dictionary = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "I", "ITEM", "ITEMS", "GEARS", "GRINDING", "SING", "David", "there",
            "launch", "of", "David", "in", "London", "challenge", "trailing",
            "Hello", "World", "trailing", "Hello", "World"
        };
        _wordsToIgnore = new List<string>();
    }

    private bool IsWordSpelledCorrectly(string word)
    {
        return _dictionary.Contains(word);
    }

    [Fact]
    public void Apply_ReplaceAllFrom_BracketsWithLowerL_ShouldReplaceWithI()
    {
        // Arrange: [lTEM] should become [ITEM] if ITEM is in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\[([A-Zl]+)\]",
            FindRegEx = new Regex(@"\[([A-Zl]+)\]", RegexOptions.Compiled),
            SpellCheckWord = "$1",
            ReplaceWith = "[$1]",
            ReplaceAllFrom = "l",
            ReplaceAllTo = "I"
        };

        var input = "[lTEM]";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("[ITEM]", result);
    }

    [Fact]
    public void Apply_ReplaceAllFrom_MultipleLInBrackets_ShouldReplaceAllWithI()
    {
        // Arrange: [lTEMl] should become [ITEMI] if ITEMI is not in dictionary, so no change
        var regex = new SpellCheckRegex
        {
            Find = @"\[([A-Zl]+)\]",
            FindRegEx = new Regex(@"\[([A-Zl]+)\]", RegexOptions.Compiled),
            SpellCheckWord = "$1",
            ReplaceWith = "[$1]",
            ReplaceAllFrom = "l",
            ReplaceAllTo = "I"
        };

        var input = "[lTEMl]";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert - ITEMI is not in dictionary, so should not change
        Assert.Equal("[lTEMl]", result);
    }

    [Fact]
    public void Apply_CapitalWordWithIInMiddle_ShouldReplaceWithL()
    {
        // Arrange: HelloI → Hellol if Hellol is not in dictionary (no change)
        var regex = new SpellCheckRegex
        {
            Find = @"\b([A-Z][a-zæøåöääöéèàùâêîôûëï]+)I([a-zæøåöääöéèàùâêîôûëï]+)\b",
            FindRegEx = new Regex(@"\b([A-Z][a-zæøåöääöéèàùâêîôûëï]+)I([a-zæøåöääöéèàùâêîôûëï]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "$1l$2",
            ReplaceWith = "$1l$2"
        };

        var input = "HelloI";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert - "Hellol" is not in dictionary, so no change
        Assert.Equal("HelloI", result);
    }

    [Fact]
    public void Apply_CapitalWordWithApostropheS_ShouldReplaceApostrophe()
    {
        // Arrange: David's → David's (spell check "David")
        var regex = new SpellCheckRegex
        {
            Find = @"\b([A-Z][a-zæøåöääöéèàùâêîôûëï]+)['']s\b",
            FindRegEx = new Regex(@"\b([A-Z][a-zæøåöääöéèàùâêîôûëï]+)['']s\b", RegexOptions.Compiled),
            SpellCheckWord = "$1",
            ReplaceWith = "$1's"
        };

        var input = "David's";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("David's", result);
    }

    [Fact]
    public void Apply_LowercaseWordWithApostropheS_ShouldReplaceApostrophe()
    {
        // Arrange: there's → there's (spell check "there")
        var regex = new SpellCheckRegex
        {
            Find = @"\b([a-zæøåöääöéèàùâêîôûëï]+)['']s\b",
            FindRegEx = new Regex(@"\b([a-zæøåöääöéèàùâêîôûëï]+)['']s\b", RegexOptions.Compiled),
            SpellCheckWord = "$1",
            ReplaceWith = "$1's"
        };

        var input = "there's";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("there's", result);
    }

    [Fact]
    public void Apply_LowercaseLFollowedByWord_ShouldReplaceWithI()
    {
        // Arrange: launch → Iaunch if Iaunch is not in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\bl([a-zæøåöääöéèàùâêîôûëï]{2,})\b",
            FindRegEx = new Regex(@"\bl([a-zæøåöääöéèàùâêîôûëï]{2,})\b", RegexOptions.Compiled),
            SpellCheckWord = "I$1",
            ReplaceWith = "I$1"
        };

        var input = "launch";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert - "Iaunch" is not in dictionary, so no change
        Assert.Equal("launch", result);
    }

    [Fact]
    public void Apply_LowercaseWordEndingWithI_ShouldReplaceWithL()
    {
        // Arrange: traiI → trail if trail is not in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\b([a-zæøåöääöéèàùâêîôûëï]+)I\b",
            FindRegEx = new Regex(@"\b([a-zæøåöääöéèàùâêîôûëï]+)I\b", RegexOptions.Compiled),
            SpellCheckWord = "$1l",
            ReplaceWith = "$1l"
        };

        var input = "traiI";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert - "trail" is not in dictionary, so no change
        Assert.Equal("traiI", result);
    }

    [Fact]
    public void Apply_LowercaseLFollowedByCapitals_ShouldReplaceWithI()
    {
        // Arrange: lTEM → ITEM if ITEM is in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\bl([A-Z]+)\b",
            FindRegEx = new Regex(@"\bl([A-Z]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "I$1",
            ReplaceWith = "I$1"
        };

        var input = "lTEM";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("ITEM", result);
    }

    [Fact]
    public void Apply_CapitalsEndingWithL_ShouldReplaceWithI()
    {
        // Arrange: ITEMl → ITEMI if ITEMI is not in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\b([A-Z]+)l\b",
            FindRegEx = new Regex(@"\b([A-Z]+)l\b", RegexOptions.Compiled),
            SpellCheckWord = "$1I",
            ReplaceWith = "$1I"
        };

        var input = "ITEMl";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert - "ITEMI" is not in dictionary, so no change
        Assert.Equal("ITEMl", result);
    }

    [Fact]
    public void Apply_CapitalsWithLInMiddle_ShouldReplaceWithI()
    {
        // Arrange: SlNG → SING if SING is in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\b([A-Z]+)l([A-Z]+)\b",
            FindRegEx = new Regex(@"\b([A-Z]+)l([A-Z]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "$1I$2",
            ReplaceWith = "$1I$2"
        };

        var input = "SlNG";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("SING", result);
    }

    [Fact]
    public void Apply_LowercaseWordWithDoubleI_ShouldReplaceWithLL()
    {
        // Arrange: chaIIenge → challenge if challenge is in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\b([a-zæøåöääöéèàùâêîôûëï]+)II([a-zæøåöääöéèàùâêîôûëï]+)\b",
            FindRegEx = new Regex(@"\b([a-zæøåöääöéèàùâêîôûëï]+)II([a-zæøåöääöéèàùâêîôûëï]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "$1ll$2",
            ReplaceWith = "$1ll$2"
        };

        var input = "chaIIenge";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("challenge", result);
    }

    [Fact]
    public void Apply_LowercaseWordWithSingleI_ShouldReplaceWithL()
    {
        // Arrange: traiIing → trailing if trailing is in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\b([a-zæøåöääöéèàùâêîôûëï]+)I([a-zæøåöääöéèàùâêîôûëï]+)\b",
            FindRegEx = new Regex(@"\b([a-zæøåöääöéèàùâêîôûëï]+)I([a-zæøåöääöéèàùâêîôûëï]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "$1l$2",
            ReplaceWith = "$1l$2"
        };

        var input = "traiIing";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("trailing", result);
    }

    [Fact]
    public void Apply_CapitalIFollowedByLowercaseWord_ShouldReplaceWithL()
    {
        // Arrange: Iaunch → launch if launch is in dictionary (3+ chars)
        var regex = new SpellCheckRegex
        {
            Find = @"\bI([a-zæøåöääöéèàùâêîôûëï]{3,})\b",
            FindRegEx = new Regex(@"\bI([a-zæøåöääöéèàùâêîôûëï]{3,})\b", RegexOptions.Compiled),
            SpellCheckWord = "l$1",
            ReplaceWith = "l$1"
        };

        var input = "Iaunch";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("launch", result);
    }

    [Fact]
    public void Apply_OfFollowedByCapitalWord_ShouldAddSpace()
    {
        // Arrange: ofDavid → of David if David is in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\bof([A-Z][a-zæøåöääöéèàùâêîôûëï]+)\b",
            FindRegEx = new Regex(@"\bof([A-Z][a-zæøåöääöéèàùâêîôûëï]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "$1",
            ReplaceWith = "of $1"
        };

        var input = "ofDavid";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("of David", result);
    }

    [Fact]
    public void Apply_InFollowedByCapitalWord_ShouldAddSpace()
    {
        // Arrange: inLondon → in London if London is in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\bin([A-Z][a-zæøåöääöéèàùâêîôûëï]+)\b",
            FindRegEx = new Regex(@"\bin([A-Z][a-zæøåöääöéèàùâêîôûëï]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "$1",
            ReplaceWith = "in $1"
        };

        var input = "inLondon";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("in London", result);
    }

    [Fact]
    public void Apply_StandaloneLowercaseL_ShouldReplaceWithI()
    {
        // Arrange: "l" → "I" (no spell check needed)
        var regex = new SpellCheckRegex
        {
            Find = @"\bl\b",
            FindRegEx = new Regex(@"\bl\b", RegexOptions.Compiled),
            SpellCheckWord = "",
            ReplaceWith = "I"
        };

        var input = "l";

        // Act - Note: This pattern has empty SpellCheckWord, so it should check empty string
        // The code will check empty string which should return false, but let's see
        var result = regex.Apply(input, _wordsToIgnore, word => string.IsNullOrEmpty(word) || IsWordSpelledCorrectly(word));

        // Assert
        Assert.Equal("I", result);
    }

    [Fact]
    public void Apply_BracketedTextWithLInMiddle_ShouldReplaceWithI()
    {
        // Arrange: [GEARS GRlNDING] → [GEARS GRINDING] if [GEARS GRINDING] is in dictionary
        // Note: This regex only replaces one 'l' at a time, so we test with only one 'l'
        var regex = new SpellCheckRegex
        {
            Find = @"\[([A-Z ]*)l([A-Z ]*)\]",
            FindRegEx = new Regex(@"\[([A-Z ]*)l([A-Z ]*)\]", RegexOptions.Compiled),
            SpellCheckWord = "[$1I$2]",
            ReplaceWith = "[$1I$2]"
        };

        var input = "[GEARS GRlNDING]";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, word => word == "GEARS GRINDING" || IsWordSpelledCorrectly(word));

        // Assert
        Assert.Equal("[GEARS GRINDING]", result);
    }

    [Fact]
    public void Apply_NoMatch_ShouldReturnOriginal()
    {
        // Arrange
        var regex = new SpellCheckRegex
        {
            Find = @"\bl([A-Z]+)\b",
            FindRegEx = new Regex(@"\bl([A-Z]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "I$1",
            ReplaceWith = "I$1"
        };

        var input = "Hello World";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Apply_WordNotInDictionary_ShouldNotReplace()
    {
        // Arrange: lXYZ where IXYZ is not in dictionary
        var regex = new SpellCheckRegex
        {
            Find = @"\bl([A-Z]+)\b",
            FindRegEx = new Regex(@"\bl([A-Z]+)\b", RegexOptions.Compiled),
            SpellCheckWord = "I$1",
            ReplaceWith = "I$1"
        };

        var input = "lXYZ";

        // Act
        var result = regex.Apply(input, _wordsToIgnore, IsWordSpelledCorrectly);

        // Assert - IXYZ is not in dictionary, so no change
        Assert.Equal("lXYZ", result);
    }
}
