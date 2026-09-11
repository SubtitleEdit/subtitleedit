using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.CheckArteErrors;

namespace UITests.Features.Tools.CheckArteErrors;

public class ArtePreviewTests
{
    private static CheckArteErrorsViewModel Create(Subtitle source, params string[] checks)
    {
        var vm = new CheckArteErrorsViewModel();
        foreach (var check in vm.Checks)
        {
            check.IsSelected = checks.Contains(check.Name);
        }
        // Avoid Initialize's persisted gap-setting side effect in tests.
        typeof(CheckArteErrorsViewModel).GetField("_sourceSnapshot", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(vm, new Subtitle(source, generateNewId: false));
        vm.AnalyzeCommand.Execute(null);
        return vm;
    }

    [AvaloniaTheory]
    [InlineData("08", false, "08")]
    [InlineData("08", true, "2D")]
    [InlineData("0F", false, "0F")]
    [InlineData("0F", true, "2F")]
    public void TargetHeader_PreservesProgrammeStartAndSupportsUndo(string language, bool sdh, string code)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var header = new Nikse.SubtitleEdit.Core.SubtitleFormats.Ebu.EbuGeneralSubtitleInformation
        {
            TimeCodeStartOfProgramme = "01000000",
            OriginalProgrammeTitle = "Programme title".PadRight(32),
        };
        var source = new Subtitle { Header = header.ToString() };
        source.Paragraphs.Add(new Paragraph("Hello", 3600000, 3602000));
        var vm = Create(source);
        vm.SelectedLanguage = vm.Languages.Single(item => item.Code == language);
        vm.IsSdh = sdh;
        vm.AnalyzeCommand.Execute(null);
        Assert.Single(vm.Fixes);
        Assert.Equal(25.0, vm.SelectedSourceFrameRate);
        vm.OkCommand.Execute(null);
        var result = Nikse.SubtitleEdit.Core.SubtitleFormats.Ebu.ReadHeader(System.Text.Encoding.GetEncoding(850).GetBytes(vm.FixedSubtitle!.Header));
        Assert.Equal("850", result.CodePageNumber);
        Assert.Equal("STL25.01", result.DiskFormatCode);
        Assert.Equal("2", result.DisplayStandardCode);
        Assert.Equal("00", result.CharacterCodeTableNumber);
        Assert.Equal(code, result.LanguageCode);
        Assert.Equal("40", result.MaximumNumberOfDisplayableCharactersInAnyTextRow);
        Assert.Equal("23", result.MaximumNumberOfDisplayableRows);
        Assert.Equal("01000000", result.TimeCodeStartOfProgramme);
        Assert.Equal(header.OriginalProgrammeTitle, result.OriginalProgrammeTitle);
        Assert.Equal(3600000, vm.FixedSubtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Empty(vm.Fixes);
        vm.UndoCommand.Execute(null);
        Assert.Equal(source.Header, vm.FixedSubtitle.Header);
    }

    [AvaloniaFact]
    public void CorrectAndUndo_KeepDialogOpenAndPublishSnapshots()
    {
        using var settings = new SettingsScope("General.SubtitleMinimumDisplayMilliseconds", "General.SubtitleMaximumDisplayMilliseconds", "General.SubtitleMaximumCharactersPerSeconds");
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.SubtitleMaximumDisplayMilliseconds = 10000;
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.SubtitleMaximumCharactersPerSeconds = 25;
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("<i>" + string.Join(" ", Enumerable.Repeat("Hello world", 12)) + "</i>", 1000, 13000));
        var vm = Create(source, "Teletext line length / control codes", "Italic formatting (not allowed)");
        var published = new List<Subtitle>();
        vm.ApplyToMainSubtitle = subtitle => published.Add(subtitle);
        var window = new CheckArteErrorsWindow(vm);
        window.Show();
        try
        {
            Assert.False(vm.UndoCommand.CanExecute(null));
            vm.OkCommand.Execute(null);
            Assert.True(window.IsVisible);
            Assert.True(vm.UndoCommand.CanExecute(null));
            Assert.Single(published);
            Assert.True(published[0].Paragraphs.Count > 1);
            Assert.Empty(vm.Fixes);
            vm.OkCommand.Execute(null); // no-op must not consume an undo step
            Assert.Single(published);
            vm.UndoCommand.Execute(null);
            Assert.True(window.IsVisible);
            Assert.Equal(2, published.Count);
            Assert.Single(published[1].Paragraphs);
            Assert.Equal(source.Paragraphs[0].Text, published[1].Paragraphs[0].Text);
            Assert.Equal(1000, published[1].Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(13000, published[1].Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.NotEmpty(vm.Fixes);
            Assert.False(vm.UndoCommand.CanExecute(null));
            vm.CancelCommand.Execute(null);
            Assert.False(window.IsVisible);
            Assert.Equal(2, published.Count); // close must not publish again
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTheory]
    [InlineData(2800)]
    [InlineData(3100)]
    public void GapAndOverlap_OfferOneFixAndApplyFiveFrames(double previousEnd)
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("First", 1000, previousEnd));
        source.Paragraphs.Add(new Paragraph("Next", 2960, 5000));
        var vm = Create(source, "Minimum gaps", "Overlapping display times");
        Assert.Single(vm.Fixes);
        Assert.True(vm.Fixes[0].CanBeFixed);
        vm.OkCommand.Execute(null);
        Assert.Equal(2760, vm.FixedSubtitle!.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(previousEnd, source.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void Groups_KeepSharedSelectionAndExpansionAcrossAnalysis()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph(" <i>Hello</i> ", 1000, 4000));
        var vm = Create(source, "Italic formatting (not allowed)", "Unneeded spaces");
        Assert.Equal(2, vm.FixGroups.Count);
        var italic = vm.FixGroups.Single(g => g.Name == "Italic formatting");
        Assert.Equal("Italic formatting (1)", italic.Header);
        Assert.False(italic.IsExpanded);
        Assert.Same(vm.Fixes.Single(f => f.GroupName == italic.Name), italic.Items[0]);
        italic.InvertSelectionCommand.Execute(null);
        Assert.False(italic.Items[0].Apply);
        Assert.True(vm.FixGroups.Single(g => g.Name == "Unneeded spaces").Items[0].Apply);
        italic.SelectAllCommand.Execute(null);
        Assert.True(italic.Items[0].Apply);
        italic.Items[0].Apply = false;
        italic.IsExpanded = true;
        italic.IsExpanded = false;
        Assert.False(italic.Items[0].Apply);
        vm.FixesSelectAllCommand.Execute(null);
        Assert.True(italic.Items[0].Apply);
        italic.IsExpanded = true;
        vm.Checks.Single(c => c.Name == "Unneeded spaces").IsSelected = false;
        Assert.Single(vm.FixGroups);
        Assert.True(vm.FixGroups[0].IsExpanded);
        Assert.Equal(vm.Fixes.Count, vm.FixGroups.Sum(g => g.Items.Count));
    }

    [AvaloniaFact]
    public void ItalicCheck_RemovesOnlyItalicAndUpdatesWhenUnchecked()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("<i>Hello</i>\n<font color=\"red\">world</font>", 1000, 4000));
        var vm = Create(source, "Italic formatting (not allowed)");
        Assert.Single(vm.Fixes);
        Assert.Equal("Hello\n<font color=\"red\">world</font>", vm.Fixes[0].After);
        vm.Checks.Single(c => c.Name == "Italic formatting (not allowed)").IsSelected = false;
        Assert.Empty(vm.Fixes);
        vm.Checks.Single(c => c.Name == "Italic formatting (not allowed)").IsSelected = true;
        vm.OkCommand.Execute(null);
        Assert.Equal("Hello\n<font color=\"red\">world</font>", vm.FixedSubtitle!.Paragraphs[0].Text);
        Assert.Contains("<i>", source.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void TeletextColors_MapHexColorsToTheNearestStandardColor()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("<font color=\"f02030\">Red</font> <font color=\"blue\">blue</font>", 1000, 4000));
        var vm = Create(source, "Teletext colors");
        vm.IsSdh = true;
        vm.AnalyzeCommand.Execute(null);

        var fix = Assert.Single(vm.Fixes.Where(item => item.Reason.Contains("nearest Teletext standard color")));
        Assert.True(fix.CanBeFixed);
        Assert.Equal("<font color=\"Red\">Red</font> <font color=\"Blue\">blue</font>", fix.After);

        vm.OkCommand.Execute(null);
        Assert.Equal(fix.After, vm.FixedSubtitle!.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void TeletextLinePosition_CorrectsInvalidBottomStartsForDoubleHeight()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph(string.Empty, 0, 200) { MarginV = "23" });
        source.Paragraphs.Add(new Paragraph("One line", 1000, 4000) { MarginV = "23" });
        source.Paragraphs.Add(new Paragraph("First line\nSecond line", 5000, 8000) { MarginV = "22" });
        var vm = Create(source, "Teletext line position");

        Assert.Collection(vm.Fixes,
            blank => Assert.Equal("22", blank.After),
            oneLine => Assert.Equal("22", oneLine.After),
            twoLines => Assert.Equal("20", twoLines.After));

        vm.OkCommand.Execute(null);
        Assert.Equal("22", vm.FixedSubtitle!.Paragraphs[0].MarginV);
        Assert.Equal("22", vm.FixedSubtitle.Paragraphs[1].MarginV);
        Assert.Equal("20", vm.FixedSubtitle.Paragraphs[2].MarginV);
    }

    [AvaloniaFact]
    public void NormalArteSubtitle_ConvertsTeletextColorsToYellow()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("<font color=\"Blue\">Hello</font>", 1000, 4000));
        var vm = Create(source, "Teletext colors");

        var fix = Assert.Single(vm.Fixes);
        Assert.Equal("<font color=\"Yellow\">Hello</font>", fix.After);
        vm.OkCommand.Execute(null);
        Assert.Equal(fix.After, vm.FixedSubtitle!.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void NormalArteSubtitle_UsesYellowConsistentlyWhenTheFileUsesColor()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("<font color=\"Red\">Red</font>", 1000, 4000));
        source.Paragraphs.Add(new Paragraph("No color", 5000, 8000));
        var vm = Create(source, "Teletext colors");

        Assert.Equal("<font color=\"Yellow\">Red</font>", vm.Fixes.Single(item => item.Index == 1).After);
        Assert.Equal("<font color=\"Yellow\">No color</font>", vm.Fixes.Single(item => item.Index == 2).After);
    }

    [AvaloniaFact]
    public void NormalArteSubtitle_RemovesSdhBoxingBeforeNormalizingToYellow()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("<font color=\"Red\">Colored cue</font>", 1000, 4000));
        source.Paragraphs.Add(new Paragraph("<box color=\"White\">Previously boxed SDH cue</box>", 5000, 8000));
        var vm = Create(source, "Teletext colors");

        var boxingFix = vm.Fixes.Single(item => item.Index == 2);
        Assert.Equal("<font color=\"Yellow\">Previously boxed SDH cue</font>", boxingFix.After);
        Assert.Contains("boxing is removed", boxingFix.Reason);

        vm.OkCommand.Execute(null);
        Assert.Equal(boxingFix.After, vm.FixedSubtitle!.Paragraphs[1].Text);
    }

    [AvaloniaFact]
    public void NonStlSource_CreatesAnArteEbuStlTargetHeader()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("Hello", 1000, 4000));
        var vm = Create(source);

        var headerFix = Assert.Single(vm.Fixes.Where(item => item.Reason.Contains("Create an ARTE EBU STL target header")));
        Assert.True(headerFix.CanBeFixed);
        vm.OkCommand.Execute(null);

        Assert.True(Nikse.SubtitleEdit.Core.SubtitleFormats.Ebu.IsStlHeader(vm.FixedSubtitle!.Header));
        Assert.Contains("STL25.01", vm.FixedSubtitle.Header);
        Assert.Equal("Hello", vm.FixedSubtitle.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void MissingBlankSubtitle_IsCreatedAtThePreviousFullHour()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("First subtitle", (10 * 60 * 60 + 82) * 1000, (10 * 60 * 60 + 85) * 1000));
        var vm = Create(source, "ARTE blank subtitle");

        var blank = Assert.Single(vm.Fixes.Where(item => item.Reason.Contains("five-frame blank/control")));
        Assert.Contains("10:00:00:00", blank.After);
        vm.OkCommand.Execute(null);

        Assert.Equal(string.Empty, vm.FixedSubtitle!.Paragraphs[0].Text);
        Assert.Equal("22", vm.FixedSubtitle.Paragraphs[0].MarginV);
        Assert.Equal(10 * 60 * 60 * 1000, vm.FixedSubtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(10 * 60 * 60 * 1000 + 200, vm.FixedSubtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [AvaloniaFact]
    public void AutomaticSplit_KeepsTextTimingAndLaterEdits()
    {
        var source = new Subtitle();
        source.Paragraphs.Add(new Paragraph("<i>" + string.Join(" ", Enumerable.Repeat("Hello world", 12)) + "</i>", 1000, 13000));
        source.Paragraphs.Add(new Paragraph("<i>Later subtitle</i>", 14000, 16000));
        using var settings = new SettingsScope("General.SubtitleMinimumDisplayMilliseconds", "General.SubtitleMaximumDisplayMilliseconds", "General.SubtitleMaximumCharactersPerSeconds");
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.SubtitleMaximumDisplayMilliseconds = 10000;
        Nikse.SubtitleEdit.Logic.Config.Se.Settings.General.SubtitleMaximumCharactersPerSeconds = 25;
        var vm = Create(source, "Teletext line length / control codes", "Italic formatting (not allowed)");
        var split = Assert.Single(vm.Fixes, f => f.Reason.Contains("method 1"));
        Assert.True(split.Apply);
        Assert.Contains("00:00:01:00 → 00:00:13:00 | 12 s 00 fr", split.BeforePreview);
        Assert.Contains("fr", split.AfterPreview);
        vm.OkCommand.Execute(null);
        var result = vm.FixedSubtitle!;
        Assert.True(result.Paragraphs.Count > 2);
        Assert.Equal("Later subtitle", result.Paragraphs[^1].Text);
        Assert.All(result.Paragraphs, p => Assert.DoesNotContain("<i>", p.Text));
        var parts = result.Paragraphs.Take(result.Paragraphs.Count - 1).ToList();
        static string Words(string text) => Regex.Replace(HtmlUtil.RemoveHtmlTags(text, true), @"\s+", " ").Trim();
        Assert.Equal(Words(source.Paragraphs[0].Text), Words(string.Join(" ", parts.Select(p => p.Text))));
        Assert.Equal(1000, parts[0].StartTime.TotalMilliseconds);
        Assert.Equal(13000, parts[^1].EndTime.TotalMilliseconds);
        Assert.All(parts, p => Assert.True(p.EndTime.TotalMilliseconds > p.StartTime.TotalMilliseconds));
        for (var i = 1; i < parts.Count; i++)
        {
            Assert.True(parts[i].StartTime.TotalMilliseconds - parts[i - 1].EndTime.TotalMilliseconds >= 200);
        }
    }
}
