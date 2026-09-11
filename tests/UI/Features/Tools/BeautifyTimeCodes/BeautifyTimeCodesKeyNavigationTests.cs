using System.Collections.Generic;
using System.Reflection;
using Avalonia.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

namespace UITests.Features.Tools.BeautifyTimeCodes;

/// <summary>
/// The change navigator's ▲/▼ buttons are mirrored on the keyboard at window level: arrows and
/// PageUp/PageDown step one change, Home/End jump to the ends. Only unmodified keys count, so
/// chords stay free for the platform. The change list is normally built by the preview timer;
/// here it is seeded directly.
/// </summary>
public class BeautifyTimeCodesKeyNavigationTests
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    private static BeautifyTimeCodesViewModel MakeViewModelWithChanges(int subtitleCount, int[] changedIndices, int current)
    {
        var vm = new BeautifyTimeCodesViewModel(new StubWindowService());
        var type = typeof(BeautifyTimeCodesViewModel);

        var original = (List<SubtitleLineViewModel>)type.GetField("_originalSubtitles", Private)!.GetValue(vm)!;
        var beautified = (List<SubtitleLineViewModel>)type.GetField("_beautifiedSubtitles", Private)!.GetValue(vm)!;
        var format = new SubRip();
        for (var i = 0; i < subtitleCount; i++)
        {
            var start = 1000 + i * 3000;
            original.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", start, start + 2000), format) { Number = i + 1 });
            beautified.Add(new SubtitleLineViewModel(new Paragraph($"Line {i + 1}", start + 40, start + 2040), format) { Number = i + 1 });
        }

        var changed = (List<int>)type.GetField("_changedIndices", Private)!.GetValue(vm)!;
        changed.AddRange(changedIndices);
        type.GetField("_currentChangeIndex", Private)!.SetValue(vm, current);
        return vm;
    }

    private static int CurrentChangeIndex(BeautifyTimeCodesViewModel vm)
        => (int)typeof(BeautifyTimeCodesViewModel).GetField("_currentChangeIndex", Private)!.GetValue(vm)!;

    private static KeyEventArgs Press(BeautifyTimeCodesViewModel vm, Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        var e = new KeyEventArgs { Key = key, KeyModifiers = modifiers, RoutedEvent = InputElement.KeyDownEvent };
        vm.OnKeyDown(e);
        return e;
    }

    [Theory]
    [InlineData(Key.Down)]
    [InlineData(Key.Right)]
    [InlineData(Key.PageDown)]
    public void NextKeysStepForward(Key key)
    {
        var vm = MakeViewModelWithChanges(5, new[] { 0, 2, 4 }, 0);

        var e = Press(vm, key);

        Assert.True(e.Handled);
        Assert.Equal(1, CurrentChangeIndex(vm));
        Assert.Contains("2", vm.ChangePositionLabel);
        Assert.True(vm.CanGoPrevious);
        Assert.True(vm.CanGoNext);
    }

    [Theory]
    [InlineData(Key.Up)]
    [InlineData(Key.Left)]
    [InlineData(Key.PageUp)]
    public void PreviousKeysStepBack(Key key)
    {
        var vm = MakeViewModelWithChanges(5, new[] { 0, 2, 4 }, 2);

        var e = Press(vm, key);

        Assert.True(e.Handled);
        Assert.Equal(1, CurrentChangeIndex(vm));
    }

    [Fact]
    public void HomeAndEndJumpToFirstAndLastChange()
    {
        var vm = MakeViewModelWithChanges(5, new[] { 0, 2, 4 }, 1);

        Press(vm, Key.End);
        Assert.Equal(2, CurrentChangeIndex(vm));
        Assert.False(vm.CanGoNext);
        Assert.True(vm.CanGoPrevious);

        Press(vm, Key.Home);
        Assert.Equal(0, CurrentChangeIndex(vm));
        Assert.True(vm.CanGoNext);
        Assert.False(vm.CanGoPrevious);
    }

    [Fact]
    public void SteppingStopsAtTheEnds()
    {
        var vm = MakeViewModelWithChanges(5, new[] { 0, 2, 4 }, 2);

        Press(vm, Key.Down);
        Assert.Equal(2, CurrentChangeIndex(vm));

        Press(vm, Key.Home);
        Press(vm, Key.Up);
        Assert.Equal(0, CurrentChangeIndex(vm));
    }

    [Theory]
    [InlineData(KeyModifiers.Control)]
    [InlineData(KeyModifiers.Alt)]
    [InlineData(KeyModifiers.Shift)]
    [InlineData(KeyModifiers.Meta)]
    public void ModifiedArrowKeysAreLeftAlone(KeyModifiers modifiers)
    {
        var vm = MakeViewModelWithChanges(5, new[] { 0, 2, 4 }, 1);

        var e = Press(vm, Key.Down, modifiers);

        Assert.False(e.Handled);
        Assert.Equal(1, CurrentChangeIndex(vm));
    }

    [Fact]
    public void KeysAreHarmlessWithoutChanges()
    {
        var vm = MakeViewModelWithChanges(3, new int[0], -1);

        Press(vm, Key.Down);
        Press(vm, Key.End);
        Press(vm, Key.Home);
        Press(vm, Key.Up);

        Assert.Equal(-1, CurrentChangeIndex(vm));
        Assert.False(vm.CanGoNext);
        Assert.False(vm.CanGoPrevious);
    }
}
