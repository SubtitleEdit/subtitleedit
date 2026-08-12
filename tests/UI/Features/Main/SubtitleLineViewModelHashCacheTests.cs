using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

/// <summary>
/// <see cref="SubtitleLineViewModel.TextHash"/> / <see cref="SubtitleLineViewModel.OriginalTextHash"/>
/// memoize <c>string.GetHashCode()</c> per string instance for the change-detection hashes (dirty
/// star, auto-save, undo). These pin down that they stay equal to the call they replace - a stale
/// hash would make an edited file look unchanged, so auto-save and undo would silently skip it.
/// </summary>
public class SubtitleLineViewModelHashCacheTests
{
    [AvaloniaFact]
    public void TextHash_EqualsGetHashCode()
    {
        var vm = new SubtitleLineViewModel { Text = "Hello there, world." };

        Assert.Equal(vm.Text.GetHashCode(), vm.TextHash);
        Assert.Equal(vm.TextHash, vm.TextHash); // memoized read returns the same value
    }

    [AvaloniaFact]
    public void TextHash_FollowsTextChange()
    {
        var vm = new SubtitleLineViewModel { Text = "Hello there, world." };
        var before = vm.TextHash;

        vm.Text = "Something else entirely.";

        Assert.Equal(vm.Text.GetHashCode(), vm.TextHash);
        Assert.NotEqual(before, vm.TextHash);
    }

    [AvaloniaFact]
    public void TextHash_FollowsChangeBackToAnEqualValue()
    {
        // A different instance with the same content must still hash the same - the memo is
        // keyed on the instance, so this is the case where it recomputes and must agree.
        var vm = new SubtitleLineViewModel { Text = "Line one" };
        var before = vm.TextHash;

        vm.Text = "Line two";
        vm.Text = string.Concat("Line", " one");

        Assert.Equal(before, vm.TextHash);
    }

    [AvaloniaFact]
    public void TextHash_IsZeroForEmptyAndNull()
    {
        var vm = new SubtitleLineViewModel { Text = string.Empty };
        Assert.Equal(string.Empty.GetHashCode(), vm.TextHash);

        vm.Text = null!;
        Assert.Equal(0, vm.TextHash);
    }

    [AvaloniaFact]
    public void OriginalTextHash_EqualsGetHashCodeAndFollowsChanges()
    {
        var vm = new SubtitleLineViewModel { OriginalText = "Den originale tekst." };
        Assert.Equal(vm.OriginalText.GetHashCode(), vm.OriginalTextHash);

        var before = vm.OriginalTextHash;
        vm.OriginalText = "En anden tekst.";

        Assert.Equal(vm.OriginalText.GetHashCode(), vm.OriginalTextHash);
        Assert.NotEqual(before, vm.OriginalTextHash);

        vm.OriginalText = null!;
        Assert.Equal(0, vm.OriginalTextHash);
    }

    [AvaloniaFact]
    public void TextAndOriginalTextHashes_AreIndependent()
    {
        var vm = new SubtitleLineViewModel { Text = "Working", OriginalText = "Original" };
        var textHash = vm.TextHash;

        vm.OriginalText = "Changed original";

        Assert.Equal(textHash, vm.TextHash);
        Assert.Equal(vm.OriginalText.GetHashCode(), vm.OriginalTextHash);
    }
}
