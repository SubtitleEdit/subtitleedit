using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// An original subtitle that does not line up 1:1 with the working subtitle is opened as a read-only
/// reference: it is shown side by side, but it is never counted as changed and never written back.
/// Before this, the mismatching lines were dropped and the truncated result was saved over the user's
/// file (issue #13449).
/// </summary>
public class MainReadOnlyOriginalTests
{
    [AvaloniaFact]
    public void ReadOnlyOriginal_ChangedText_IsNotReportedAsChanges()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Translated text", "Reference text");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = true;
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
            SetPrivateField(vm, "_changeSubtitleHashOriginal", vm.GetFastHashOriginal());

            line.OriginalText = "Changed reference text";

            Assert.False(vm.HasChanges());
            Assert.False(vm.CanEditOriginal);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void EditableOriginal_ChangedText_IsStillReportedAsChanges()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var line = AddLine(vm, "Translated text", "Original text");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = false;
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
            SetPrivateField(vm, "_changeSubtitleHashOriginal", vm.GetFastHashOriginal());

            line.OriginalText = "Changed original text";

            Assert.True(vm.HasChanges());
            Assert.True(vm.CanEditOriginal);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public async Task ReadOnlyOriginal_SaveSubtitleOriginal_LeavesTheFileUntouched()
    {
        var (window, vm) = CreateMainViewModel();
        var fileName = Path.Combine(Path.GetTempPath(), $"se-readonly-original-{Guid.NewGuid():N}.srt");
        const string onDisk = "1\r\n00:00:00,000 --> 00:00:02,000\r\nReference line one\r\n\r\n" +
                              "2\r\n00:00:02,000 --> 00:00:04,000\r\nReference line two\r\n\r\n";
        await File.WriteAllTextAsync(fileName, onDisk);

        try
        {
            AddLine(vm, "Translated text", "Reference line one");
            vm.ShowColumnOriginalText = true;
            vm.IsOriginalReadOnly = true;
            SetPrivateField(vm, "_subtitleFileNameOriginal", fileName);

            var saved = await InvokeSaveSubtitleOriginal(vm);

            Assert.False(saved);
            Assert.Equal(onDisk, await File.ReadAllTextAsync(fileName));
        }
        finally
        {
            CloseWindow(window, vm);
            File.Delete(fileName);
        }
    }

    [AvaloniaFact]
    public void ImportAsReadOnlyReference_KeepsEveryOriginalLineInMemory()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            var reference = ImportSampleReference(vm);

            Assert.True(vm.IsOriginalReadOnly);
            Assert.False(vm.CanEditOriginal);

            // The reference itself keeps all three lines, so nothing is lost.
            var subtitleOriginal = (Subtitle)GetField("_subtitleOriginal").GetValue(vm)!;
            Assert.Equal(3, subtitleOriginal.Paragraphs.Count);
            Assert.Equal(reference.Paragraphs[1].Text, subtitleOriginal.Paragraphs[1].Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ImportAsReadOnlyReference_ShowsUnmatchedLinesAsReferenceOnlyRows()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            // The reference-only line sits between the two translated rows, in time order.
            Assert.Equal(3, vm.Subtitles.Count);

            Assert.False(vm.Subtitles[0].IsReferenceOnly);
            Assert.Equal("Translated one", vm.Subtitles[0].Text);
            Assert.Equal("Reference one", vm.Subtitles[0].OriginalText);

            Assert.True(vm.Subtitles[1].IsReferenceOnly);
            Assert.Equal(string.Empty, vm.Subtitles[1].Text);
            Assert.Equal("Reference only - no translation", vm.Subtitles[1].OriginalText);

            Assert.False(vm.Subtitles[2].IsReferenceOnly);
            Assert.Equal("Translated two", vm.Subtitles[2].Text);
            Assert.Equal("Reference two", vm.Subtitles[2].OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReferenceOnlyRows_AreNotPartOfTheSavedSubtitle()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            var saved = vm.GetUpdateSubtitle();

            Assert.Equal(2, saved.Paragraphs.Count);
            Assert.Equal("Translated one", saved.Paragraphs[0].Text);
            Assert.Equal("Translated two", saved.Paragraphs[1].Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReferenceOnlyRows_DoNotMakeTheWorkingSubtitleLookChanged()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);
            var hashBefore = vm.GetFastHash();

            ImportReference(vm, BuildSampleReference());

            Assert.Equal(hashBefore, vm.GetFastHash());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReferenceOnlyRows_TakeNoNumber()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            Assert.Equal(1, vm.Subtitles[0].Number);
            Assert.Equal(2, vm.Subtitles[2].Number);

            // The display-only row shows no number at all.
            Assert.Equal(string.Empty, vm.Subtitles[1].NumberDisplay);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReapplyReadOnlyReference_RealignsAfterTheWorkingRowsChange()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            // Delete the second translated row: its reference line now has no counterpart either,
            // so after re-matching there are two reference-only rows and one translated row.
            vm.Subtitles.Remove(vm.Subtitles.Single(p => p.Text == "Translated two"));

            InvokeReapplyReadOnlyReference(vm);

            Assert.Equal(3, vm.Subtitles.Count);
            Assert.Equal(new[] { false, true, true }, vm.Subtitles.Select(p => p.IsReferenceOnly));
            Assert.Equal(
                new[] { "Reference one", "Reference only - no translation", "Reference two" },
                vm.Subtitles.Select(p => p.OriginalText));

            // Still only the one real line is saved.
            Assert.Single(vm.GetUpdateSubtitle().Paragraphs);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The old SE mode with "Allow edit of original subtitle" left off: the matching lines are shown,
    /// there are no reference-only rows, and the original still cannot be written back.
    /// </summary>
    [AvaloniaFact]
    public void ImportMatchingLinesOnly_ReadOnly_HasNoReferenceRowsAndIsNotSaveable()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);

            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, BuildSampleReference());
            InvokeImportOriginalSubtitle(vm, "reference.srt", match.Projection, match: null, isReadOnly: true);

            Assert.True(vm.IsOriginalReadOnly);
            Assert.False(vm.CanEditOriginal);
            Assert.Equal(2, vm.Subtitles.Count);
            Assert.DoesNotContain(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.Equal("Reference one", vm.Subtitles[0].OriginalText);
            Assert.Equal("Reference two", vm.Subtitles[1].OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReferenceOnlyRows_AreNotReturnedAsSelectedItems()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            vm.SubtitleGrid.SelectedItems!.Clear();
            foreach (var line in vm.Subtitles)
            {
                vm.SubtitleGrid.SelectedItems!.Add(line);
            }

            // Commands see only the working lines; the selection machinery still sees all three.
            Assert.Equal(2, vm.SubtitleGridSelectedItems.Count);
            Assert.DoesNotContain(vm.SubtitleGridSelectedItems, p => p.IsReferenceOnly);
            Assert.Equal(3, vm.SubtitleGridSelectedItemsWithReference.Count);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void ReferenceOnlyRow_SelectedAlone_IsNotReturnedAsASelectedItem()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            vm.SubtitleGrid.SelectedItems!.Clear();
            vm.SubtitleGrid.SelectedItems!.Add(vm.Subtitles[1]);

            Assert.Empty(vm.SubtitleGridSelectedItems);
            Assert.Single(vm.SubtitleGridSelectedItemsWithReference);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Showing the original's non-matching lines no longer locks time codes: each row keeps a
    /// sticky link to the original line it displays, so retiming cannot shuffle the reference
    /// around under the user (#13594). Only a selected display-only row keeps its editors off -
    /// its timings belong to the original file.
    /// </summary>
    [AvaloniaFact]
    public void ShowingNonMatchingOriginalLines_DoesNotLockTimeCodes()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            Assert.False(vm.AreTimeCodesLocked);

            ImportSampleReference(vm);

            Assert.True(vm.IsShowingOriginalNonMatchingLines);
            Assert.False(vm.AreTimeCodesLocked);
            Assert.False(vm.LockTimeCodes);

            // The show/hide/duration editors still refuse a display-only row.
            vm.SelectedSubtitle = vm.Subtitles.Single(p => p.IsReferenceOnly);
            Assert.False(vm.AreTimeCodesEditable);

            vm.SelectedSubtitle = vm.Subtitles.First(p => !p.IsReferenceOnly);
            Assert.True(vm.AreTimeCodesEditable);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The whole point of the sticky link (#13594): retiming a working line keeps the original
    /// text it displays, even when the new timing overlaps a different original line - the old
    /// time-based re-match would have swapped the texts around.
    /// </summary>
    [AvaloniaFact]
    public void RetimedWorkingRow_KeepsItsOriginalLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            // Retime "Translated one" (displays "Reference one", 0-2000) into the middle of the
            // reference-only line's span (2000-4000).
            var retimed = vm.Subtitles.Single(p => p.Text == "Translated one");
            retimed.SetTimes(TimeSpan.FromMilliseconds(2200), TimeSpan.FromMilliseconds(3800));

            InvokeReapplyReadOnlyReference(vm);

            Assert.Equal("Reference one", retimed.OriginalText);
            var referenceRow = Assert.Single(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.Equal("Reference only - no translation", referenceRow.OriginalText);
            Assert.Equal(3, vm.Subtitles.Count);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// After a retime the display-only rows glide back into start-time order - and only they move;
    /// the working rows keep their order and identity.
    /// </summary>
    [AvaloniaFact]
    public void RetimedWorkingRow_MakesReferenceRowsGlideIntoPlace()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            // "Translated two" (4000-6000) moves before the reference-only row (2000-4000).
            var retimed = vm.Subtitles.Single(p => p.Text == "Translated two");
            retimed.SetTimes(TimeSpan.FromMilliseconds(900), TimeSpan.FromMilliseconds(1900));

            InvokeReapplyReadOnlyReference(vm);

            Assert.Equal(
                new[] { "Translated one", "Translated two", string.Empty },
                vm.Subtitles.Select(p => p.Text));
            Assert.True(vm.Subtitles[2].IsReferenceOnly);
            Assert.Equal("Reference only - no translation", vm.Subtitles[2].OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Typing into a display-only row adopts the missing line: the row becomes an ordinary,
    /// numbered, saveable working line at the reference line's timings (#13594).
    /// </summary>
    [AvaloniaFact]
    public void TypingIntoReferenceOnlyRow_PromotesItToAWorkingLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);
            var referenceRow = vm.Subtitles.Single(p => p.IsReferenceOnly);

            vm.SelectedSubtitle = referenceRow;
            referenceRow.Text = "Freshly typed translation";
            vm.SubtitleTextChanged(null, null!);

            Assert.False(referenceRow.IsReferenceOnly);
            Assert.Equal(2, referenceRow.Number);
            Assert.Equal(TimeSpan.FromMilliseconds(2000), referenceRow.StartTime);

            var saved = vm.GetUpdateSubtitle();
            Assert.Equal(3, saved.Paragraphs.Count);
            Assert.Equal("Freshly typed translation", saved.Paragraphs[1].Text);

            // The reference itself is untouched, and the refresh does not bring a duplicate row:
            // the promoted row still displays that original line.
            InvokeReapplyReadOnlyReference(vm);
            Assert.Equal(3, vm.Subtitles.Count);
            Assert.DoesNotContain(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.Equal("Reference only - no translation", referenceRow.OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Deleting a promoted row hands its original line back to the reference: the line reappears
    /// as a display-only row on the next refresh, so nothing of the reference is ever lost.
    /// </summary>
    [AvaloniaFact]
    public void DeletingAPromotedRow_BringsTheReferenceLineBack()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);
            var referenceRow = vm.Subtitles.Single(p => p.IsReferenceOnly);

            vm.SelectedSubtitle = referenceRow;
            referenceRow.Text = "Typed then deleted";
            vm.SubtitleTextChanged(null, null!);
            Assert.False(referenceRow.IsReferenceOnly);

            vm.Subtitles.Remove(referenceRow);
            InvokeReapplyReadOnlyReference(vm);

            var restored = Assert.Single(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.Equal("Reference only - no translation", restored.OriginalText);
            Assert.Equal(3, vm.Subtitles.Count);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// With the non-matching lines on screen the grid holds every original line exactly once, so an
    /// editable original round-trips - which is what makes "allow edit" safe in this mode.
    /// </summary>
    [AvaloniaFact]
    public void EditableOriginal_WithNonMatchingLinesShown_SavesEveryOriginalLine()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);

            var reference = BuildSampleReference();
            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
            InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: false);

            Assert.True(vm.CanEditOriginal);
            Assert.True(vm.IsShowingOriginalNonMatchingLines);

            // Edit the line that only exists in the original.
            vm.Subtitles[1].OriginalText = "Edited reference-only line";

            var saved = vm.GetUpdateSubtitleOriginal();

            Assert.Equal(3, saved.Paragraphs.Count);
            Assert.Equal("Reference one", saved.Paragraphs[0].Text);
            Assert.Equal("Edited reference-only line", saved.Paragraphs[1].Text);
            Assert.Equal("Reference two", saved.Paragraphs[2].Text);

            // ...and the working subtitle is still just the two translated lines.
            Assert.Equal(2, vm.GetUpdateSubtitle().Paragraphs.Count);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>An edit to a display-only row must survive the re-match that follows a row change.</summary>
    [AvaloniaFact]
    public void EditableOriginal_ReapplyAfterRowChange_KeepsEditsToNonMatchingLines()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);

            var reference = BuildSampleReference();
            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
            InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: false);

            vm.Subtitles[1].OriginalText = "Edited reference-only line";

            InvokeReapplyReadOnlyReference(vm);

            var referenceRow = Assert.Single(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.Equal("Edited reference-only line", referenceRow.OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>The read-only original is authoritative: the file's text wins over any stray edit.</summary>
    [AvaloniaFact]
    public void ReadOnlyOriginal_ReapplyAfterRowChange_RestoresTheFilesText()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            vm.Subtitles[1].OriginalText = "Tampered";

            InvokeReapplyReadOnlyReference(vm);

            var referenceRow = Assert.Single(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.Equal("Reference only - no translation", referenceRow.OriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    [AvaloniaFact]
    public void CloseOriginal_RemovesTheReferenceOnlyRows()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);
            Assert.Equal(3, vm.Subtitles.Count);

            InvokeFileCloseOriginal(vm);

            Assert.Equal(2, vm.Subtitles.Count);
            Assert.DoesNotContain(vm.Subtitles, p => p.IsReferenceOnly);
            Assert.False(vm.IsOriginalReadOnly);
            Assert.False(vm.ShowColumnOriginalText);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The re-match that follows a row change may only rebaseline a clean original - an original
    /// with unsaved edits must still count as changed afterwards, or the edits would be lost on
    /// exit without a save prompt.
    /// </summary>
    [AvaloniaFact]
    public void EditableOriginal_ReapplyAfterRowChange_KeepsUnsavedEditsCountingAsChanges()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);

            var reference = BuildSampleReference();
            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
            InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: false);
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());
            Assert.False(vm.HasChanges());

            vm.Subtitles[0].OriginalText = "Edited original line";
            Assert.True(vm.HasChanges());

            InvokeReapplyReadOnlyReference(vm);

            Assert.True(vm.HasChanges());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>...while a clean original stays clean across the re-match's row shuffling.</summary>
    [AvaloniaFact]
    public void EditableOriginal_ReapplyAfterRowChange_KeepsACleanOriginalClean()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);

            var reference = BuildSampleReference();
            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
            InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: false);
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());

            InvokeReapplyReadOnlyReference(vm);

            Assert.False(vm.HasChanges());
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// "Remove translation" promotes the original to the working subtitle. With the non-matching
    /// lines on screen, the display-only rows carry original lines too - so they must become
    /// ordinary, numbered, saveable lines, and the mode (with its time-code lock) must end.
    /// </summary>
    [AvaloniaFact]
    public async Task EditableOriginal_RemoveTranslation_PromotesTheReferenceOnlyRows()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            AddLine(vm, "Translated one", string.Empty, 0, 2000);
            AddLine(vm, "Translated two", string.Empty, 4000, 6000);

            var reference = BuildSampleReference();
            var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
            InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: false);
            SetPrivateField(vm, "_changeSubtitleHash", vm.GetFastHash());

            await InvokeFileCloseTranslation(vm);

            Assert.False(vm.IsShowingOriginalNonMatchingLines);
            Assert.False(vm.AreTimeCodesLocked);
            Assert.DoesNotContain(vm.Subtitles, p => p.IsReferenceOnly);

            var saved = vm.GetUpdateSubtitle();
            Assert.Equal(3, saved.Paragraphs.Count);
            Assert.Equal("Reference one", saved.Paragraphs[0].Text);
            Assert.Equal("Reference only - no translation", saved.Paragraphs[1].Text);
            Assert.Equal("Reference two", saved.Paragraphs[2].Text);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The Delete command must skip reference-only rows in the selection: they belong to the
    /// original, and deleting one from an editable original would drop the line for good on the
    /// next capture.
    /// </summary>
    [AvaloniaFact]
    public async Task DeleteSelectedItems_SkipsReferenceOnlyRows()
    {
        var (window, vm) = CreateMainViewModel();
        var promptBeforeDelete = Se.Settings.General.PromptBeforeDelete;
        Se.Settings.General.PromptBeforeDelete = false;
        try
        {
            ImportSampleReference(vm);
            var referenceRow = Assert.Single(vm.Subtitles, p => p.IsReferenceOnly);

            SetPrivateField(vm, "_selectedSubtitles",
                new List<SubtitleLineViewModel> { referenceRow, vm.Subtitles[0] });

            await InvokeDeleteSelectedItems(vm);

            Assert.Contains(referenceRow, vm.Subtitles);
            Assert.DoesNotContain(vm.Subtitles, p => p.Text == "Translated one");
        }
        finally
        {
            Se.Settings.General.PromptBeforeDelete = promptBeforeDelete;
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// The waveform stays editable while the non-matching lines are shown - the sticky links mean
    /// dragging a paragraph edge cannot shuffle the reference rows onto other lines (#13594).
    /// </summary>
    [AvaloniaFact]
    public void ShowingNonMatchingOriginalLines_KeepsTheWaveformEditable()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            if (vm.AudioVisualizer == null)
            {
                return; // no waveform in this headless setup - nothing to verify
            }

            Assert.False(vm.AudioVisualizer.IsReadOnly);

            ImportSampleReference(vm);
            Assert.False(vm.AudioVisualizer.IsReadOnly);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// A remembered "original text only" find scope must not survive the original going away, or
    /// every find afterwards searches nothing and comes up empty.
    /// </summary>
    [AvaloniaFact]
    public void CloseOriginal_ResetsAnOriginalOnlyFindScope()
    {
        var (window, vm) = CreateMainViewModel();
        try
        {
            ImportSampleReference(vm);

            var findService = (IFindService)GetField("_findService").GetValue(vm)!;
            findService.CurrentScope = FindService.FindScope.OriginalOnly;

            InvokeFileCloseOriginal(vm);

            Assert.Equal(FindService.FindScope.TextAndOriginal, findService.CurrentScope);
        }
        finally
        {
            CloseWindow(window, vm);
        }
    }

    /// <summary>
    /// Working subtitle: two translated lines with a gap. Reference: the same two plus a line in the
    /// gap that the translation never got - the case from issue #13449.
    /// </summary>
    private static Subtitle BuildSampleReference()
    {
        var reference = new Subtitle();
        reference.Paragraphs.Add(new Paragraph("Reference one", 0, 2000));
        reference.Paragraphs.Add(new Paragraph("Reference only - no translation", 2000, 4000));
        reference.Paragraphs.Add(new Paragraph("Reference two", 4000, 6000));
        return reference;
    }

    private static Subtitle ImportSampleReference(MainViewModel vm)
    {
        AddLine(vm, "Translated one", string.Empty, 0, 2000);
        AddLine(vm, "Translated two", string.Empty, 4000, 6000);

        var reference = BuildSampleReference();
        ImportReference(vm, reference);
        return reference;
    }

    private static void ImportReference(MainViewModel vm, Subtitle reference)
    {
        var match = ImportOriginalHelper.MatchOriginalLines(vm.Subtitles, reference);
        InvokeImportOriginalSubtitle(vm, "reference.srt", reference, match, isReadOnly: true);
    }

    private static (Window Window, MainViewModel Vm) CreateMainViewModel()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();

        var window = new Window { Width = 1200, Height = 800 };
        MainView.NextHostWindow = window;
        var view = new MainView();
        window.Content = view;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return (window, (MainViewModel)view.DataContext!);
    }

    private static SubtitleLineViewModel AddLine(
        MainViewModel vm, string text, string originalText, int startMs = 0, int endMs = 2000)
    {
        var line = new SubtitleLineViewModel(new Paragraph(text, startMs, endMs), null!)
        {
            OriginalText = originalText,
            // The grid is always renumbered on load, so start from a numbered state like the app does.
            Number = vm.Subtitles.Count + 1,
        };

        vm.Subtitles.Add(line);
        return line;
    }

    private static void CloseWindow(Window window, MainViewModel vm)
    {
        foreach (var ownedWindow in window.OwnedWindows.ToArray())
        {
            ownedWindow.Close();
        }

        window.Closing -= vm.OnClosing;
        if (window.IsVisible)
        {
            window.Close();
        }
    }

    private static async Task<bool> InvokeSaveSubtitleOriginal(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "SaveSubtitleOriginal", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("SaveSubtitleOriginal not found");

        return await (Task<bool>)method.Invoke(vm, new object[] { true })!;
    }

    private static void InvokeImportOriginalSubtitle(
        MainViewModel vm, string fileName, Subtitle subtitle, ImportOriginalHelper.OriginalMatch? match, bool isReadOnly)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ImportOriginalSubtitle", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ImportOriginalSubtitle not found");

        method.Invoke(vm, new object?[] { 0, fileName, subtitle, match, isReadOnly });
    }

    private static void InvokeReapplyReadOnlyReference(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "ReapplyOriginalReference", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("ReapplyOriginalReference not found");

        method.Invoke(vm, new object?[] { true });
    }

    private static void InvokeFileCloseOriginal(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "FileCloseOriginal", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("FileCloseOriginal not found");

        method.Invoke(vm, null);
    }

    private static async Task InvokeFileCloseTranslation(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "FileCloseTranslation", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("FileCloseTranslation not found");

        await (Task)method.Invoke(vm, null)!;
    }

    private static async Task InvokeDeleteSelectedItems(MainViewModel vm)
    {
        var method = typeof(MainViewModel).GetMethod(
                         "DeleteSelectedItems", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new InvalidOperationException("DeleteSelectedItems not found");

        await (Task)method.Invoke(vm, null)!;
    }

    private static void SetPrivateField(MainViewModel vm, string name, object value)
    {
        GetField(name).SetValue(vm, value);
    }

    private static FieldInfo GetField(string name)
    {
        return typeof(MainViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
               ?? throw new InvalidOperationException($"Field not found: {name}");
    }
}
