using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class FontFamilyHelperTests
{
    // "Default" is what the font drop-downs show for "whatever the system uses" - it is stored as
    // that literal string, and it is not a font family that exists anywhere. Asking Avalonia for it
    // put an unresolvable name into its glyph-fallback lookups, which is how Arabic came out as
    // empty boxes in the subtitle grid on Windows (issue #14150).

    [Theory]
    [InlineData("Default")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void PlaceholderNamesResolveToThePlatformDefault(string? fontName)
    {
        Assert.Equal(FontFamily.Default, FontFamilyHelper.Make(fontName));
    }

    [Fact]
    public void ARealFontNameIsKept()
    {
        Assert.Equal("Arial", FontFamilyHelper.Make("Arial").Name);
    }

    [Fact]
    public void TheMacOsSystemFontFamilyIsKept()
    {
        // ".AppleSystemUIFont" is a real family name on macOS - the one stored when the user picks
        // "System Font" - and the platform default there is Helvetica Neue, not the system font.
        Assert.Equal(".AppleSystemUIFont", FontFamilyHelper.Make(".AppleSystemUIFont").Name);
    }
}
