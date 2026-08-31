using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.ReviewSpeech;

/// <summary>
/// An exported session is only re-generatable if the recordings its cloned voices speak from
/// travel with it: a per-line clone's references are cut into the run folder, which is gone by the
/// next session, and an imported clone only exists in the voice list of the machine that imported
/// it (#14095).
/// </summary>
public class ExportVoiceReferenceTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "se-export-refs-" + Guid.NewGuid().ToString("N"));

    private string ReferenceFolder => Path.Combine(_folder, "refs");

    private string WriteClip(string name, string contents = "wav", string? transcript = "what the video says")
    {
        var sourceFolder = Path.Combine(_folder, "source", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(sourceFolder);
        var clipFileName = Path.Combine(sourceFolder, name);
        File.WriteAllText(clipFileName, contents);
        if (transcript != null)
        {
            File.WriteAllText(Path.ChangeExtension(clipFileName, ".txt"), transcript);
        }

        return clipFileName;
    }

    private static Voice CloneVoice(string clipFileName) =>
        new(new OmniVoice(Path.GetFileNameWithoutExtension(clipFileName), clipFileName));

    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, true);
        }
    }

    [Fact]
    public void TheRecordingAndItsTranscriptAreCopiedIntoTheExport()
    {
        var clipFileName = WriteClip("line-0007.wav");

        var relativeName = ReviewSpeechViewModel.ExportVoiceReference(CloneVoice(clipFileName), ReferenceFolder, new Dictionary<string, string>());

        // Forward slash so the same JSON opens on Windows, macOS and Linux.
        Assert.Equal("refs/line-0007.wav", relativeName);
        Assert.True(File.Exists(Path.Combine(ReferenceFolder, "line-0007.wav")));
        // Without the transcript omnivoice-tts refuses the reference, so a copy without it could
        // only fail at synthesis.
        Assert.Equal("what the video says", File.ReadAllText(Path.Combine(ReferenceFolder, "line-0007.txt")));
    }

    [Fact]
    public void AVoiceThatClonesFromNothingExportsNothing()
    {
        var exported = new Dictionary<string, string>();

        Assert.Equal(string.Empty, ReviewSpeechViewModel.ExportVoiceReference(new Voice(new OmniVoice("Default", string.Empty)), ReferenceFolder, exported));
        Assert.Equal(string.Empty, ReviewSpeechViewModel.ExportVoiceReference(null, ReferenceFolder, exported));
        // A voice whose recording has already been deleted has nothing to copy either.
        Assert.Equal(string.Empty, ReviewSpeechViewModel.ExportVoiceReference(CloneVoice(Path.Combine(_folder, "gone.wav")), ReferenceFolder, exported));
        Assert.False(Directory.Exists(ReferenceFolder));
    }

    [Fact]
    public void OneRecordingSharedByManyLinesIsCopiedOnce()
    {
        // A cast of a few imported clones would otherwise be copied once per line - hundreds of
        // copies of the same wav for a full-length subtitle.
        var clipFileName = WriteClip("Ada.wav");
        var exported = new Dictionary<string, string>();

        var first = ReviewSpeechViewModel.ExportVoiceReference(CloneVoice(clipFileName), ReferenceFolder, exported);
        var second = ReviewSpeechViewModel.ExportVoiceReference(CloneVoice(clipFileName), ReferenceFolder, exported);

        Assert.Equal(first, second);
        Assert.Single(Directory.GetFiles(ReferenceFolder, "*.wav"));
    }

    [Fact]
    public void TwoDifferentRecordingsWithOneNameBothSurvive()
    {
        var first = WriteClip("line-0001.wav", "first");
        var second = WriteClip("line-0001.wav", "second");
        var exported = new Dictionary<string, string>();

        var firstName = ReviewSpeechViewModel.ExportVoiceReference(CloneVoice(first), ReferenceFolder, exported);
        var secondName = ReviewSpeechViewModel.ExportVoiceReference(CloneVoice(second), ReferenceFolder, exported);

        Assert.NotEqual(firstName, secondName);
        Assert.Equal("first", File.ReadAllText(Path.Combine(_folder, firstName.Replace('/', Path.DirectorySeparatorChar))));
        Assert.Equal("second", File.ReadAllText(Path.Combine(_folder, secondName.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public void ReExportingIntoTheFolderTheSessionCameFromKeepsTheRecording()
    {
        // Import points the voice at <jsonFolder>/refs/line-0007.wav; exporting back to the same
        // folder makes source and target the same file, and copying it onto itself only throws.
        Directory.CreateDirectory(ReferenceFolder);
        var clipFileName = Path.Combine(ReferenceFolder, "line-0007.wav");
        File.WriteAllText(clipFileName, "wav");

        var relativeName = ReviewSpeechViewModel.ExportVoiceReference(CloneVoice(clipFileName), ReferenceFolder, new Dictionary<string, string>());

        Assert.Equal("refs/line-0007.wav", relativeName);
        Assert.Equal("wav", File.ReadAllText(clipFileName));
    }
}
