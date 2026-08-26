using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;

namespace UITests.Features.Tools.FixCommonErrors;

/// <summary>
/// The paragraph ids of the subtitle the window was handed are how the main window finds the grid
/// row each line came from, so the fixed subtitle must carry them back - the lines that survive a
/// rule which removes lines above all, since those rows hold the original text of a translation and
/// would otherwise be rebuilt empty (#14053).
/// </summary>
public class FixCommonErrorsParagraphIdTests : IDisposable
{
    private readonly List<SeFixCommonErrorsProfile> _originalProfiles;

    public FixCommonErrorsParagraphIdTests()
    {
        // Se.Settings is static and starts out without profiles in tests; the scan does nothing
        // without one. Restored in Dispose so no other test sees this profile.
        _originalProfiles = Se.Settings.Tools.FixCommonErrors.Profiles;
        Se.Settings.Tools.FixCommonErrors.Profiles = new List<SeFixCommonErrorsProfile>
        {
            new()
            {
                ProfileName = "Default",
                SelectedRules = new List<string> { nameof(FixEmptyLines) },
            },
        };
    }

    public void Dispose()
    {
        Se.Settings.Tools.FixCommonErrors.Profiles = _originalProfiles;
    }

    [AvaloniaFact]
    public async Task RemovedEmptyLine_KeepsTheIdsOfTheLinesThatStay()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello there", 0, 2000));
        subtitle.Paragraphs.Add(new Paragraph(string.Empty, 2500, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Goodbye there", 4500, 6000));
        subtitle.Renumber();
        var ids = subtitle.Paragraphs.Select(p => p.Id).ToList();

        var vm = new FixCommonErrorsViewModel(new FakeNamesList(), null!, null!);
        vm.Initialize(subtitle, new SubRip());
        await vm.DoRefreshFixes();
        await vm.ApplySelectedFixes();

        Assert.Equal(
            new[] { "Hello there", "Goodbye there" },
            vm.FixedSubtitle.Paragraphs.Select(p => p.Text));
        Assert.Equal(new[] { ids[0], ids[2] }, vm.FixedSubtitle.Paragraphs.Select(p => p.Id));
    }

    private sealed class FakeNamesList : INamesList
    {
        public void Load(string dictionaryFolder, string languageCode)
        {
        }

        public bool IsName(string candidate) => false;

        public HashSet<string> GetAbbreviations() => new();
    }
}
