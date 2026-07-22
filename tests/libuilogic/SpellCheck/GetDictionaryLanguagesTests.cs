using System;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.SpellCheck;

public class GetDictionaryLanguagesTests
{
    private static string CreateFolderWith(params string[] baseNames)
    {
        var dir = Path.Combine(Path.GetTempPath(), "se_dicts_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        foreach (var name in baseNames)
        {
            File.WriteAllText(Path.Combine(dir, name + ".dic"), "1\nword\n");
            File.WriteAllText(Path.Combine(dir, name + ".aff"), "SET UTF-8\n");
        }

        return dir;
    }

    [Fact]
    public void VariantSuffixes_AreStripped_ButFileStillIdentified()
    {
        var dir = CreateFolderWith("de_DE_frami", "de_AT_frami", "de_CH_frami", "be-official", "sl_SI", "en_US");
        try
        {
            var list = new SpellChecker().GetDictionaryLanguages(dir);

            // No ugly upper-case variant tokens leak into the display name.
            Assert.DoesNotContain(list, d => d.Name.Contains("FRAMI"));
            Assert.DoesNotContain(list, d => d.Name.Contains("OFFICIAL"));

            // The original file name is still shown in brackets so the file is identifiable.
            Assert.Contains(list, d => d.Name.Contains("[de_DE_frami]"));
            Assert.Contains(list, d => d.Name.Contains("[be-official]"));

            // Every entry resolved to a real language name, not the "[name]" fallback.
            Assert.All(list, d => Assert.False(d.Name.StartsWith("[", StringComparison.Ordinal)));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Script_IsPreserved()
    {
        var dir = CreateFolderWith("sr-Latn");
        try
        {
            var list = new SpellChecker().GetDictionaryLanguages(dir);
            var entry = Assert.Single(list);
            // Resolves (no "[name]" fallback) and keeps the script tag rather than dropping it.
            Assert.False(entry.Name.StartsWith("[", StringComparison.Ordinal));
            Assert.Contains("[sr-Latn]", entry.Name);
            // ICU renders the kept script as a qualifier in parentheses (locale-independent structure);
            // had "Latn" been stripped, plain "sr" would have no qualifier.
            Assert.Contains("(", entry.Name);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
