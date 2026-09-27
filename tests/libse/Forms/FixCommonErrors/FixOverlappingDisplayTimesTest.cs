using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Forms.FixCommonErrors;

public class FixOverlappingDisplayTimesTest
{
    // Mirrors the UI: a preview pass lists fixes per paragraph, then the apply pass only
    // allows a fix when a listed row for that same paragraph and action exists.
    private sealed class PreviewApplyCallback : IFixCallbacks
    {
        public bool PreviewMode { get; set; } = true;
        public HashSet<(Guid? id, string action)> Listed { get; } = new();
        public bool AllowFix(Paragraph p, string action) => PreviewMode || Listed.Contains((p.Id, action));
        public void AddFixToListView(Paragraph p, string action, string before, string after)
        {
            if (PreviewMode)
            {
                Listed.Add((p.Id, action));
            }
        }
        public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked)
            => AddFixToListView(p, action, before, after);
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

    private static Subtitle MakeEqualEndStart(double prevStart)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hi.", prevStart, 10000) { Number = 1 });
        subtitle.Paragraphs.Add(new Paragraph("How are you doing today?", 10000, 13000) { Number = 2 });
        return subtitle;
    }

    // A short previous line (not above minimum display time) that ends exactly where the next
    // line starts is fixed by moving the next line's start. The row was listed under the next
    // line but the apply pass asked about the previous one, so "Apply selected fixes" never
    // changed anything.
    [Fact]
    public void EqualEndStart_ShortPrevious_MovesCurrentStart_AndIsAppliedAfterPreview()
    {
        var oldAllowEqual = Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart;
        var oldMin = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
        try
        {
            Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart = false;
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;

            var subtitle = MakeEqualEndStart(9500);
            var preview = new Subtitle();
            foreach (var p in subtitle.Paragraphs)
            {
                preview.Paragraphs.Add(new Paragraph(p, generateNewId: false));
            }

            var cb = new PreviewApplyCallback();
            new FixOverlappingDisplayTimes().Fix(preview, cb);
            Assert.Single(cb.Listed);

            cb.PreviewMode = false;
            new FixOverlappingDisplayTimes().Fix(subtitle, cb);
            var apply = subtitle;

            Assert.Equal(10000, apply.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal(10001, apply.Paragraphs[1].StartTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart = oldAllowEqual;
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = oldMin;
        }
    }

    // Both lines at or below minimum display time (sample file lines 66/67: 793 ms and 333 ms)
    // used to be skipped with no fix row and no error, leaving the overlap in place.
    [Fact]
    public void EqualEndStart_BothShort_ShortensPrevious()
    {
        var oldAllowEqual = Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart;
        var oldMin = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
        try
        {
            Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart = false;
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;

            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Hi.", 168585, 169378) { Number = 1 });
            subtitle.Paragraphs.Add(new Paragraph("Yes.", 169378, 169711) { Number = 2 });
            var preview = new Subtitle();
            foreach (var p in subtitle.Paragraphs)
            {
                preview.Paragraphs.Add(new Paragraph(p, generateNewId: false));
            }

            var cb = new PreviewApplyCallback();
            new FixOverlappingDisplayTimes().Fix(preview, cb);
            Assert.Single(cb.Listed);

            cb.PreviewMode = false;
            new FixOverlappingDisplayTimes().Fix(subtitle, cb);

            Assert.Equal(169377, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.Equal(169378, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        }
        finally
        {
            Configuration.Settings.Tools.FixCommonErrorsFixOverlapAllowEqualEndStart = oldAllowEqual;
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = oldMin;
        }
    }
}
