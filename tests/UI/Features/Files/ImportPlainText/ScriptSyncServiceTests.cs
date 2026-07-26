using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files.ImportPlainText;

public class ScriptSyncServiceTests
{
    public ScriptSyncServiceTests()
    {
        // SyncScript reads min/max duration and gap from settings; pin them so the
        // assertions below don't depend on whatever the running user has configured.
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
        Se.Settings.General.SubtitleMaximumDisplayMilliseconds = 8000;
        Se.Settings.General.UseFrameMode = false;
        Se.Settings.General.MinimumBetweenLines = new MsOrFramesValue { Milliseconds = 24 };
    }

    private static List<SubtitleLineViewModel> ScriptLines(params string[] texts)
        => texts.Select(t => new SubtitleLineViewModel { Text = t }).ToList();

    /// <summary>
    /// Builds a word-level transcription in the shape Whisper's highlight output uses:
    /// one paragraph per word, the timed word wrapped in &lt;u&gt;…&lt;/u&gt;.
    /// </summary>
    private static Subtitle WordLevel(params (string Word, double StartMs, double EndMs)[] words)
    {
        var subtitle = new Subtitle();
        foreach (var (word, startMs, endMs) in words)
        {
            subtitle.Paragraphs.Add(new Paragraph($"<u>{word}</u>", startMs, endMs));
        }

        return subtitle;
    }

    [Fact]
    public void RepeatedWordLaterInAudio_DoesNotDragTheCursorForward()
    {
        // The transcription mangles the first "celebration" but gets a later one exactly
        // right. The old greedy scan took the best score anywhere in its window, so the
        // exact match ~20 words ahead beat the near match at the correct position, moved
        // the cursor permanently, and desynced everything after it (#11746).
        var script = ScriptLines(
            "The celebration happened yesterday",
            "Nothing important occurred afterwards",
            "The celebration happened again");

        var transcription = WordLevel(
            ("The", 0, 500),
            ("selebration", 500, 1500),   // misrecognised
            ("happened", 1500, 2200),
            ("yesterday", 2200, 3000),
            ("Nothing", 4000, 4500),
            ("important", 4500, 5200),
            ("occurred", 5200, 5900),
            ("afterwards", 5900, 6800),
            ("The", 8000, 8500),
            ("celebration", 8500, 9500),  // exact match, but belongs to line 3
            ("happened", 9500, 10200),
            ("again", 10200, 11000));

        ScriptSyncService.SyncScript(script, transcription);

        // Line 1 must stay at the start of the audio, not jump to the 8.5s occurrence.
        Assert.InRange(script[0].StartTime.TotalMilliseconds, 0, 1000);
        Assert.InRange(script[1].StartTime.TotalMilliseconds, 3500, 4500);
        Assert.InRange(script[2].StartTime.TotalMilliseconds, 7500, 8500);
    }

    [Fact]
    public void LongScriptWithTranscriptionErrors_StaysInSync()
    {
        // Every 7th word is dropped and every 11th mangled - roughly what a real
        // transcription of difficult audio looks like. The old algorithm desynced within
        // the first couple of dozen words and never recovered.
        // Two distinct content words per line, none repeated anywhere in the script - a
        // real script behaves this way, and it is what lets rare-word anchoring work.
        var distinctive = new[]
        {
            "harbour", "lantern", "compass", "meadow", "thunder", "cavern", "orchard",
            "granite", "willow", "beacon", "furnace", "marigold", "quarry", "sparrow",
            "trellis", "anvil", "pelican", "cobbler", "juniper", "tunnel", "rafter",
            "mantle", "brisket", "pewter", "hollow", "cinder", "plover", "satchel",
            "ferment", "gallop", "kestrel", "lattice", "mortar", "nectar", "obelisk",
            "parsnip", "quiver", "rhubarb", "saffron", "tundra", "urchin", "vellum",
            "walnut", "yarrow", "zephyr", "abbey", "bramble", "citrus", "dovecote",
            "ember", "fathom", "gable", "harrow", "inkwell", "jetty", "kindling",
            "loft", "mallet", "nutmeg", "otter", "pantry", "quill", "ridge", "sledge",
            "thicket", "umber", "vessel", "wharf", "yeoman", "zinnia", "arbour",
            "burrow", "chisel", "damask", "eaves", "flint", "gorse", "hearth",
            "ivory", "jackdaw",
        };

        var script = new List<SubtitleLineViewModel>();
        var words = new List<(string, double, double)>();
        var expectedStartMs = new List<double>();

        var t = 0.0;
        var slot = 0;
        for (var line = 0; line < 40; line++)
        {
            var a = distinctive[line * 2];
            var b = distinctive[(line * 2) + 1];
            script.Add(new SubtitleLineViewModel { Text = $"The {a} and the {b}" });
            expectedStartMs.Add(t);

            foreach (var w in new[] { "The", a, "and", "the", b })
            {
                // Counted over spoken slots, not over emitted words - otherwise a drop
                // never advances the counter and every later word is dropped too.
                if (slot % 7 == 6)
                {
                    slot++;
                    t += 400; // dropped by the transcription, but time still passes
                    continue;
                }

                var spoken = slot % 11 == 10 && w.Length > 4 ? w.Substring(0, w.Length - 1) + "x" : w;
                words.Add((spoken, t, t + 400));
                slot++;
                t += 400;
            }

            t += 600; // pause between lines
        }

        var result = ScriptSyncService.SyncScript(script, WordLevel(words.ToArray()));

        // Time codes must be strictly increasing and non-overlapping.
        for (var i = 1; i < script.Count; i++)
        {
            Assert.True(
                script[i].StartTime > script[i - 1].StartTime,
                $"line {i} starts at {script[i].StartTime} which is not after line {i - 1} at {script[i - 1].StartTime}");
            Assert.True(
                script[i].StartTime >= script[i - 1].EndTime,
                $"line {i} overlaps line {i - 1}");
        }

        // And they must actually land on the audio, not merely be ordered. A line whose
        // distinctive words were dropped by the transcription can only be placed by a
        // later word in the same line, so allow up to one line-pitch (2600 ms) of
        // lateness - but never the multi-line drift the greedy scan produced.
        var errors = new List<double>();
        for (var i = 0; i < script.Count; i++)
        {
            var error = script[i].StartTime.TotalMilliseconds - expectedStartMs[i];
            errors.Add(Math.Abs(error));
            Assert.InRange(error, -2600, 2600);
        }

        Assert.True(errors.Average() < 900, $"mean start error was {errors.Average():F0} ms");
        Assert.True(result.MatchedLines >= 38, $"only {result.MatchedLines} of {result.TotalLines} lines matched directly");
    }

    [Fact]
    public void TrailingLinesWithNoMatch_GetMinimumDurationNotMinimumGap()
    {
        // These used to be laid out with the minimum *gap* as their duration, producing
        // ~24 ms cues piled on top of each other.
        var script = ScriptLines(
            "The harbour lantern",
            "Completely absent dialogue",
            "Also entirely missing");

        var transcription = WordLevel(
            ("The", 0, 500),
            ("harbour", 500, 1200),
            ("lantern", 1200, 2000));

        ScriptSyncService.SyncScript(script, transcription);

        for (var i = 1; i < script.Count; i++)
        {
            Assert.True(
                script[i].Duration.TotalMilliseconds >= 1000,
                $"line {i} lasts only {script[i].Duration.TotalMilliseconds} ms");
            Assert.True(script[i].StartTime >= script[i - 1].EndTime, $"line {i} overlaps line {i - 1}");
        }
    }

    [Fact]
    public void LeadingLinesWithNoMatch_DoNotPushTheFirstMatchedLineLater()
    {
        // The first two lines aren't in the audio at all. Laying them out backwards used
        // to reserve only the minimum gap for them, so after the duration clamp they
        // collided with line 3 and the overlap pass moved line 3 - the one line whose
        // timing was actually known - later.
        var script = ScriptLines(
            "Missing opening narration",
            "Also not spoken aloud",
            "The harbour lantern glows");

        var transcription = WordLevel(
            ("The", 30000, 30400),
            ("harbour", 30400, 31100),
            ("lantern", 31100, 31800),
            ("glows", 31800, 32400));

        ScriptSyncService.SyncScript(script, transcription);

        Assert.Equal(30400, script[2].StartTime.TotalMilliseconds, 1);
        Assert.True(script[1].EndTime <= script[2].StartTime, "leading filler overlaps the matched line");
        Assert.True(script[0].EndTime <= script[1].StartTime, "leading filler lines overlap each other");
    }

    [Fact]
    public void EmptyTranscription_ReportsEveryLineUnmatched()
    {
        var script = ScriptLines("One", "Two");

        var result = ScriptSyncService.SyncScript(script, new Subtitle());

        Assert.Equal(2, result.TotalLines);
        Assert.Equal(2, result.UnmatchedLines);
        Assert.Equal(0, result.MatchedLines);
    }
}
