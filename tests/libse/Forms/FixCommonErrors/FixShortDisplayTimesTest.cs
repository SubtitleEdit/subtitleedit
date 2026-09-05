using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Forms.FixCommonErrors;

public class FixShortDisplayTimesTest
{
    private sealed class RecordingCallback : IFixCallbacks
    {
        public List<(int number, string before, string after)> FixesAdded { get; } = new();
        public bool AllowFix(Paragraph p, string action) => true;
        public void AddFixToListView(Paragraph p, string action, string before, string after)
            => FixesAdded.Add((p.Number, before, after));
        public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked)
            => FixesAdded.Add((p.Number, before, after));
        public void LogStatus(string sender, string message) { }
        public void LogStatus(string sender, string message, bool isImportant) { }
        public void UpdateFixStatus(int fixes, string message) { }
        public bool IsName(string candidate) => false;
        public HashSet<string> GetAbbreviations() => new();
        public void AddToTotalErrors(int count) { }
        public void AddToDeleteIndices(int index) { }
        public SubtitleFormat Format => new SubRip();
        public Encoding Encoding => Encoding.UTF8;
        public string Language => "en";
    }

    // Issue #13617: a cue whose chars-per-second is only marginally above the maximum used to
    // get a sub-millisecond extension. Paragraph.ToString() renders whole milliseconds, so the
    // fix appeared as a no-op (identical before/after) and a whole-ms save discarded the gain,
    // leaving the error to reappear. The fix must extend by a real whole millisecond and push
    // cps strictly below the maximum.
    [Fact]
    public void CpsMarginallyOverMax_ProducesRealWholeMillisecondFix_NotNoOp()
    {
        var oldMax = Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds;
        var oldMove = Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime;
        try
        {
            Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds = 15.0;
            Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime = false;

            // 56 chars over 3733 ms => 15.0013 cps, just over the 15.0 limit.
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(
                "Teremos muito tempo para isso." + Environment.NewLine + "Vamos fazer-nos à estrada.",
                TimeCode.ParseToMilliseconds("00:00:50,144"),
                TimeCode.ParseToMilliseconds("00:00:53,877")) { Number = 1 });
            // A far-away next line so there is room to extend.
            subtitle.Paragraphs.Add(new Paragraph("next",
                TimeCode.ParseToMilliseconds("00:01:00,000"),
                TimeCode.ParseToMilliseconds("00:01:02,000")) { Number = 2 });

            Assert.True(subtitle.Paragraphs[0].GetCharactersPerSecond() > 15.0);

            var cb = new RecordingCallback();
            new FixShortDisplayTimes().Fix(subtitle, cb);

            var fix = Assert.Single(cb.FixesAdded);
            Assert.Equal(1, fix.number);

            // The fix is not a phantom no-op.
            Assert.NotEqual(fix.before, fix.after);

            var fixed0 = subtitle.Paragraphs[0];

            // The end time moved by a whole millisecond (53,877 -> 53,878).
            Assert.Equal(53878, (int)fixed0.EndTime.TotalMilliseconds);

            // cps is at or below the maximum and stays there after whole-ms rounding (SRT save).
            Assert.True(fixed0.GetCharactersPerSecond() <= 15.0);
            var wholeMsDuration = (int)fixed0.EndTime.TotalMilliseconds - (int)fixed0.StartTime.TotalMilliseconds;
            var cpsAfterSave = (double)fixed0.Text.CountCharacters(true) / (wholeMsDuration / 1000.0);
            Assert.True(cpsAfterSave <= 15.0, $"cps after whole-ms save was {cpsAfterSave}");
        }
        finally
        {
            Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds = oldMax;
            Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime = oldMove;
        }
    }

    // Extending a line can only lower its chars/sec, so a cps that is still over the
    // maximum at the minimum duration must not stop the extension (discussion #12929).
    [Fact]
    public void ShortLine_StillTooFastAtMinimum_IsExtendedAnyway()
    {
        var oldMax = Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds;
        var oldMin = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
        try
        {
            Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds = 25.0;
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;

            // 40 chars in 200 ms; at 1000 ms it is still 40 cps.
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("This line has exactly forty characters!!", 0, 200) { Number = 1 });
            subtitle.Paragraphs.Add(new Paragraph("next", 10000, 12000) { Number = 2 });

            var cb = new RecordingCallback();
            new FixShortDisplayTimes().Fix(subtitle, cb);

            Assert.NotEmpty(cb.FixesAdded);
            var p = subtitle.Paragraphs[0];
            Assert.True(p.DurationTotalMilliseconds >= 1000, $"duration was {p.DurationTotalMilliseconds}");
            Assert.True(p.GetCharactersPerSecond() <= 25.0, $"cps was {p.GetCharactersPerSecond()}");
        }
        finally
        {
            Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds = oldMax;
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = oldMin;
        }
    }

    // Back-to-back lines: the full minimum does not fit, but the gap that is there
    // should still be used instead of leaving the line untouched (discussion #12929).
    [Fact]
    public void ShortLine_WithoutRoomForMinimum_IsExtendedToTheGap()
    {
        var oldMin = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var oldGap = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
        var oldMove = Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime;
        try
        {
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = 24;
            Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime = false;

            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hi", 0, 300) { Number = 1 });
            subtitle.Paragraphs.Add(new Paragraph("next", 800, 3000) { Number = 2 });

            var cb = new RecordingCallback();
            new FixShortDisplayTimes().Fix(subtitle, cb);

            Assert.Equal(776, (int)subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal(800, (int)subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = oldMin;
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = oldGap;
            Configuration.Settings.Tools.FixShortDisplayTimesAllowMoveStartTime = oldMove;
        }
    }
}
