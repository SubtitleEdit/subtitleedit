using System.Text;
using Nikse.SubtitleEdit.Logic.Config.Language;

namespace UITests.Logic;

/// <summary>
/// <c>src/ui/Assets/Languages/English.json</c> is generated from the <c>Language*</c> classes in
/// code (the "Save language file" shortcut in the main window does it), and every translation is
/// generated from it - so a new or changed string in code is invisible to translators until the
/// file is regenerated. Running this test regenerates it, so no one has to start the app to do it:
///
/// <code>
/// dotnet test tests/UI/UITests.csproj --filter EnglishLanguageFileUpToDateTests
/// </code>
///
/// It deliberately does not fail on drift - a stale English.json is a chore, not a broken build,
/// and failing would turn every new UI string into a red CI run. It rewrites the file (harmless on
/// a CI agent, where the working copy is thrown away) and passes either way.
/// </summary>
public class EnglishLanguageFileUpToDateTests
{
    [Fact]
    public void EnglishJson_IsRegeneratedFromLanguageClassesInCode()
    {
        // Serializing the whole language graph is the part actually worth asserting - it runs on
        // every start-up for translated users, so a class that cannot be serialized must not pass.
        var expected = SeLanguage.ToJson(new SeLanguage());
        Assert.False(string.IsNullOrWhiteSpace(expected));

        var path = FindEnglishJson();
        if (path is null)
        {
            // Test binaries run from somewhere without the repository above them - nothing to update.
            return;
        }

        // File.ReadAllText strips the UTF-8 BOM the file is saved with.
        if (expected == File.ReadAllText(path))
        {
            return;
        }

        try
        {
            File.WriteAllText(path, expected, new UTF8Encoding(true));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Read-only checkout (some CI agents): regenerating is a convenience, not a test subject.
        }
    }

    /// <summary>
    /// Walks up from the test output directory to the repository copy of English.json,
    /// or null when the tests are not running from inside a checkout.
    /// </summary>
    private static string? FindEnglishJson()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ui", "Assets", "Languages", "English.json");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        return null;
    }
}
