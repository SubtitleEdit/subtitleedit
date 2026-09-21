using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

namespace UITests.Features.Tools.RemoveTextForHearingImpaired;

public class RemoveTextForHearingImpairedViewModelTests
{
    private static RemoveTextForHearingImpairedViewModel Resolve()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        return services.BuildServiceProvider().GetRequiredService<RemoveTextForHearingImpairedViewModel>();
    }

    [AvaloniaFact]
    public void SettingsMode_ShowsOkCancel_WhenNoCallbackProvided()
    {
        var vm = Resolve();
        vm.Initialize(new Subtitle());
        Assert.False(vm.IsApplyVisible);
        Assert.True(vm.IsSettingsMode);
    }

    /// <summary>
    /// The 500 ms timer used to run the whole HI pass over every line on the UI thread on every
    /// tick. It now only does so when an input changed - so every input has to mark it dirty.
    /// </summary>
    [AvaloniaFact]
    public void Preview_IsOnlyDirty_AfterAnInputChanged()
    {
        var vm = Resolve();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("[door slams] Hello", 0, 1000));
        vm.Initialize(sub);
        Assert.True(vm.IsPreviewDirty); // first tick must build the list

        vm.GeneratePreview();
        Assert.False(vm.IsPreviewDirty);
        Assert.Single(vm.Fixes);

        vm.SelectedFix = vm.Fixes[0]; // browsing the fix list is not an input
        Assert.False(vm.IsPreviewDirty);

        vm.IsRemoveBracketsOn = !vm.IsRemoveBracketsOn;
        Assert.True(vm.IsPreviewDirty);
        vm.GeneratePreview();
        Assert.False(vm.IsPreviewDirty);

        vm.TextContains = "abc";
        Assert.True(vm.IsPreviewDirty);
        vm.GeneratePreview();

        vm.SelectedLanguage = vm.Languages.First(l => l.Code == "da");
        Assert.True(vm.IsPreviewDirty);
    }

    [AvaloniaFact]
    public void Preview_CarriesCheckboxStatesOverById_WithManyFixes()
    {
        var vm = Resolve();
        var sub = new Subtitle();
        for (var i = 0; i < 50; i++)
        {
            sub.Paragraphs.Add(new Paragraph(i % 2 == 0 ? $"[noise {i}] Hello {i}" : $"Plain {i}", i * 1000, i * 1000 + 900));
        }

        vm.Initialize(sub);
        vm.IsRemoveBracketsOn = true;
        vm.GeneratePreview();
        Assert.Equal(25, vm.Fixes.Count);
        vm.Fixes[3].Apply = false;
        vm.Fixes[7].Apply = false;
        var unticked = new[] { vm.Fixes[3].Paragraph.Id, vm.Fixes[7].Paragraph.Id };

        vm.IsRemoveTextUppercaseLineOn = !vm.IsRemoveTextUppercaseLineOn; // forces a rebuilt list...
        sub.Paragraphs[0].Text = "[other] Changed";                        // ...that differs from the old one
        vm.GeneratePreview();

        Assert.Equal(25, vm.Fixes.Count);
        Assert.Equal(unticked, vm.Fixes.Where(f => !f.Apply).Select(f => f.Paragraph.Id).ToArray());
    }

    [AvaloniaFact]
    public void Apply_PushesTickedFixesToCallback_WithoutNeedingOk()
    {
        var vm = Resolve();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("[door slams]", 0, 1000));
        sub.Paragraphs.Add(new Paragraph("Hello there", 1000, 2000));

        Subtitle? applied = null;
        vm.Initialize(sub, s => applied = s);

        Assert.True(vm.IsApplyVisible);

        // Simulate the preview: line 0 becomes empty (removed), line 1 unchanged.
        vm.Fixes.Add(new RemoveItem(true, 0, "[door slams]", string.Empty, sub.Paragraphs[0]));

        vm.ApplyCommand.Execute(null);

        Assert.NotNull(applied);
        Assert.Single(applied!.Paragraphs); // the emptied line was dropped
        Assert.Equal("Hello there", applied.Paragraphs[0].Text);
    }

    [AvaloniaFact]
    public void Preview_IgnoresTrailingEmptyLine_WhenNoHiRuleApplies()
    {
        var vm = Resolve();
        var sub = new Subtitle();
        // A trailing Enter in the edit text box leaves a trailing empty line; the lib rebuilds
        // the text without it, which must not be reported as a change (#13389).
        sub.Paragraphs.Add(new Paragraph("Hello there." + Environment.NewLine, 0, 1000));

        vm.Initialize(sub, _ => { });
        vm.IsRemoveBracketsOn = true;

        // Apply regenerates the preview synchronously (same path the timer takes).
        vm.ApplyCommand.Execute(null);

        Assert.Empty(vm.Fixes);
    }

    [AvaloniaFact]
    public void Preview_StillReportsRealFix_WhenTextHasTrailingEmptyLine()
    {
        var vm = Resolve();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("[door slams]" + Environment.NewLine + "Hello there." + Environment.NewLine, 0, 1000));

        vm.Initialize(sub, _ => { });
        vm.IsRemoveBracketsOn = true;

        vm.ApplyCommand.Execute(null);

        Assert.Single(vm.Fixes);
        Assert.Equal("Hello there.", vm.Fixes[0].After);
    }

    [AvaloniaFact]
    public void SelectionCommands_TickUntickAndInvertApplyColumn()
    {
        var vm = Resolve();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("[door slams]", 0, 1000));
        sub.Paragraphs.Add(new Paragraph("(sighs)", 1000, 2000));
        vm.Initialize(sub, _ => { });

        vm.Fixes.Add(new RemoveItem(true, 0, "[door slams]", string.Empty, sub.Paragraphs[0]));
        vm.Fixes.Add(new RemoveItem(false, 1, "(sighs)", string.Empty, sub.Paragraphs[1]));

        vm.SelectNoFixesCommand.Execute(null);
        Assert.All(vm.Fixes, f => Assert.False(f.Apply));

        vm.SelectAllFixesCommand.Execute(null);
        Assert.All(vm.Fixes, f => Assert.True(f.Apply));

        vm.Fixes[0].Apply = false;
        vm.InvertFixesSelectionCommand.Execute(null);
        Assert.True(vm.Fixes[0].Apply);
        Assert.False(vm.Fixes[1].Apply);
    }

    [AvaloniaTheory]
    [InlineData(Key.A, KeyModifiers.Control, true, true)]
    [InlineData(Key.D, KeyModifiers.Control, true, false)]
    [InlineData(Key.A, KeyModifiers.Meta, true, true)]
    [InlineData(Key.A, KeyModifiers.None, false, false)]
    [InlineData(Key.A, KeyModifiers.Control | KeyModifiers.Shift, false, false)]
    [InlineData(Key.D, KeyModifiers.None, false, false)]
    public void SelectionShortcuts_TickOrUntickAllFixes(Key key, KeyModifiers modifiers, bool expectedHandled, bool expectedApply)
    {
        var vm = Resolve();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("[door slams]", 0, 1000));
        vm.Initialize(sub, _ => { });
        vm.Fixes.Add(new RemoveItem(false, 0, "[door slams]", string.Empty, sub.Paragraphs[0]));

        var e = new KeyEventArgs { Key = key, KeyModifiers = modifiers, RoutedEvent = InputElement.KeyDownEvent };
        vm.OnKeyDown(e);

        Assert.Equal(expectedHandled, e.Handled);
        Assert.Equal(expectedApply, vm.Fixes[0].Apply);
    }

    [AvaloniaFact]
    public void InvertShortcut_NeedsShift()
    {
        var vm = Resolve();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("[door slams]", 0, 1000));
        vm.Initialize(sub, _ => { });
        vm.Fixes.Add(new RemoveItem(false, 0, "[door slams]", string.Empty, sub.Paragraphs[0]));

        var withoutShift = new KeyEventArgs { Key = Key.I, KeyModifiers = KeyModifiers.Control, RoutedEvent = InputElement.KeyDownEvent };
        vm.OnKeyDown(withoutShift);
        Assert.False(withoutShift.Handled);
        Assert.False(vm.Fixes[0].Apply);

        var withShift = new KeyEventArgs { Key = Key.I, KeyModifiers = KeyModifiers.Control | KeyModifiers.Shift, RoutedEvent = InputElement.KeyDownEvent };
        vm.OnKeyDown(withShift);
        Assert.True(withShift.Handled);
        Assert.True(vm.Fixes[0].Apply);
    }

    // #13591: the HI pass rebuilds the text with Environment.NewLine, so a paragraph holding a
    // foreign line break must not be offered as a fix that renders exactly like the original.
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public void LineBreakOnlyDifference_IsNotAFix(string lineBreak)
    {
        const string first = "I told him to get lost,";
        const string second = "and when he vanished with the light,";

        Assert.False(RemoveTextForHearingImpairedViewModel.IsVisibleChange(
            first + lineBreak + second,
            first + Environment.NewLine + second));
    }

    [AvaloniaTheory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void PlainTextWithAnyLineBreak_GivesNoFixes(string lineBreak)
    {
        var vm = Resolve();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph(
            "I told him to get lost," + lineBreak + "and when he vanished with the light,", 0, 3000));
        vm.Initialize(sub, _ => { });

        vm.GeneratePreview();

        Assert.Empty(vm.Fixes);
    }

    [Fact]
    public void RealChange_IsStillAFix()
    {
        Assert.True(RemoveTextForHearingImpairedViewModel.IsVisibleChange("[door slams]" + Environment.NewLine + "Hello", "Hello"));
    }
}
