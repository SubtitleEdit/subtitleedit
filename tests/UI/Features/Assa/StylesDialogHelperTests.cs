using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Features.Shared;

namespace UITests.Features.Assa;

/// <summary>
/// Logic shared by the ASSA and SSA styles windows - the SSA window used to always add a silent
/// "_2" copy on copy/import, without the overwrite / keep both choice the ASSA window has (#15312).
/// </summary>
public class StylesDialogHelperTests
{
    private const string AlreadyExists = "exists: {0}";

    private static StyleDisplay Style(string name, decimal fontSize = 20) => new(new SsaStyle { Name = name, FontSize = fontSize });

    private static StylesDialogHelper.AskOverwrite Answer(MessageBoxResult answer, bool forAll, List<string> asked) =>
        (message, _) =>
        {
            asked.Add(message);
            return Task.FromResult((answer, forAll));
        };

    [Fact]
    public void MakeUniqueName_AppendsFirstFreeNumber()
    {
        var styles = new[] { Style("Default"), Style("default_2") };

        Assert.Equal("Other", StylesDialogHelper.MakeUniqueName("Other", styles));
        Assert.Equal("Default_3", StylesDialogHelper.MakeUniqueName("Default", styles));
    }

    [Fact]
    public async Task CopyStyles_NoConflict_AddsWithoutAsking()
    {
        var target = new ObservableCollection<StyleDisplay> { Style("Default") };
        var asked = new List<string>();

        await StylesDialogHelper.CopyStyles(new List<StyleDisplay> { Style("Top") }, target, _ => true, AlreadyExists,
            s => new StyleDisplay(s), null, Answer(MessageBoxResult.Cancel, false, asked));

        Assert.Empty(asked);
        Assert.Equal(new[] { "Default", "Top" }, target.Select(p => p.Name));
    }

    [Fact]
    public async Task CopyStyles_Overwrite_UpdatesExistingInPlace()
    {
        var existing = Style("Default", 20);
        var target = new ObservableCollection<StyleDisplay> { existing };
        var asked = new List<string>();
        var overwritten = new List<StyleDisplay>();

        await StylesDialogHelper.CopyStyles(new List<StyleDisplay> { Style("default", 42) }, target, _ => true, AlreadyExists,
            s => new StyleDisplay(s), overwritten.Add, Answer(MessageBoxResult.Custom1, false, asked));

        Assert.Equal(new[] { "exists: default" }, asked);
        var only = Assert.Single(target);
        Assert.Same(existing, only);
        Assert.Equal("Default", only.Name);
        Assert.Equal(42, only.FontSize);
        Assert.Equal(new[] { existing }, overwritten);
    }

    [Fact]
    public async Task CopyStyles_KeepBoth_AddsRenamedCopy()
    {
        var target = new ObservableCollection<StyleDisplay> { Style("Default") };
        var asked = new List<string>();

        await StylesDialogHelper.CopyStyles(new List<StyleDisplay> { Style("Default") }, target, _ => true, AlreadyExists,
            s => new StyleDisplay(s), null, Answer(MessageBoxResult.Custom2, false, asked));

        Assert.Equal(new[] { "Default", "Default_2" }, target.Select(p => p.Name));
    }

    [Fact]
    public async Task CopyStyles_Cancel_StopsAtFirstConflict()
    {
        var target = new ObservableCollection<StyleDisplay> { Style("A") };
        var asked = new List<string>();

        await StylesDialogHelper.CopyStyles(new List<StyleDisplay> { Style("New1"), Style("A"), Style("New2") }, target, _ => true, AlreadyExists,
            s => new StyleDisplay(s), null, Answer(MessageBoxResult.Cancel, false, asked));

        Assert.Equal(new[] { "A", "New1" }, target.Select(p => p.Name));
    }

    [Fact]
    public async Task CopyStyles_DoThisForAll_AsksOnce()
    {
        var target = new ObservableCollection<StyleDisplay> { Style("A"), Style("B") };
        var asked = new List<string>();
        var hasMore = new List<bool>();
        StylesDialogHelper.AskOverwrite ask = (message, more) =>
        {
            asked.Add(message);
            hasMore.Add(more);
            return Task.FromResult((MessageBoxResult.Custom2, true));
        };

        await StylesDialogHelper.CopyStyles(new List<StyleDisplay> { Style("A"), Style("B") }, target, _ => true, AlreadyExists,
            s => new StyleDisplay(s), null, ask);

        Assert.Single(asked);
        Assert.Equal(new[] { true }, hasMore);
        Assert.Equal(new[] { "A", "B", "A_2", "B_2" }, target.Select(p => p.Name));
    }

    [Fact]
    public void ReplaceStylesWith_StorageTarget_AddsItRepointsLinesAndRemovesReplaced()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("a", 0, 1000) { Extra = "Old" });
        subtitle.Paragraphs.Add(new Paragraph("b", 1000, 2000) { Extra = "Keep" });
        var old = Style("Old");
        var fileStyles = new ObservableCollection<StyleDisplay> { old, Style("Keep") };
        var storageStyles = new[] { Style("Keep"), Style("FromStorage") };

        var candidates = StylesDialogHelper.GetReplaceWithCandidates(fileStyles, storageStyles, new HashSet<string> { "Old" });
        Assert.Equal(new[] { "Keep", "FromStorage" }, candidates.Select(p => p.Name));

        var made = new List<string>();
        var result = StylesDialogHelper.ReplaceStylesWith(subtitle, fileStyles, new[] { old }, storageStyles[1], s =>
        {
            made.Add(s.Name);
            return new StyleDisplay(s);
        });

        Assert.Equal("FromStorage", result.Name);
        Assert.Equal(new[] { "FromStorage" }, made);
        Assert.Equal(new[] { "Keep", "FromStorage" }, fileStyles.Select(p => p.Name));
        Assert.Equal("FromStorage", subtitle.Paragraphs[0].Extra);
        Assert.Equal("Keep", subtitle.Paragraphs[1].Extra);
    }
}
