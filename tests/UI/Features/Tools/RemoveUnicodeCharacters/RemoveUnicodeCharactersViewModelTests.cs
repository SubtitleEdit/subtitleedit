using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.RemoveUnicodeCharacters;

namespace UITests.Features.Tools.RemoveUnicodeCharacters;

public class RemoveUnicodeCharactersViewModelTests
{
    private static RemoveUnicodeCharactersViewModel Resolve()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        return services.BuildServiceProvider().GetRequiredService<RemoveUnicodeCharactersViewModel>();
    }

    private static List<SubtitleLineViewModel> MakeLines(params string[] texts)
    {
        return texts.Select((t, i) => new SubtitleLineViewModel { Text = t, Number = i + 1 }).ToList();
    }

    [AvaloniaFact]
    public void FindsOnlyCharactersAboveLatin1AndGroupsThem()
    {
        var vm = Resolve();
        vm.Initialize(MakeLines("♪ Hello ♪", "Café", "OK 👍"));

        // é is 0xE9 (Latin-1) and must not be listed; ♪ is grouped with count 2;
        // 👍 is a surrogate pair and must appear as one character.
        Assert.Equal(2, vm.Characters.Count);

        var note = vm.Characters.Single(c => c.Character == "♪");
        Assert.Equal(2, note.Count);
        Assert.Equal("1", note.LinesDisplay);
        Assert.Equal("U+266A", note.CodeDisplay);
        Assert.Equal("#", note.ReplaceWith); // prefilled from the default replace list

        var emoji = vm.Characters.Single(c => c.Character == "👍");
        Assert.Equal(1, emoji.Count);
        Assert.Equal("3", emoji.LinesDisplay);
        Assert.Equal("U+01F44D", emoji.CodeDisplay);
        Assert.Equal(string.Empty, emoji.ReplaceWith);
    }

    [AvaloniaFact]
    public void OkReplacesCheckedAndKeepsUncheckedCharacters()
    {
        var vm = Resolve();
        vm.Initialize(MakeLines("♪ La la ♪", "你好 world", "Café"));

        // ♪ replaced via the default replace list ("#"), 你 removed, 好 left unchecked.
        vm.Characters.Single(c => c.Character == "好").IsChecked = false;
        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.Equal("# La la #", vm.FixedSubtitle[0].Text);
        Assert.Equal("好 world", vm.FixedSubtitle[1].Text);
        Assert.Equal("Café", vm.FixedSubtitle[2].Text);
    }

    [AvaloniaFact]
    public void NoUnicodeCharactersGivesEmptyListAndStatus()
    {
        var vm = Resolve();
        vm.Initialize(MakeLines("Hello", "Café"));

        Assert.Empty(vm.Characters);
        Assert.False(string.IsNullOrEmpty(vm.StatusText));
    }

    [AvaloniaFact]
    public void InvertSelectionFlipsAllItems()
    {
        var vm = Resolve();
        vm.Initialize(MakeLines("♪你"));

        Assert.All(vm.Characters, c => Assert.True(c.IsChecked));
        vm.InvertSelectionCommand.Execute(null);
        Assert.All(vm.Characters, c => Assert.False(c.IsChecked));
        vm.SelectAllCommand.Execute(null);
        Assert.All(vm.Characters, c => Assert.True(c.IsChecked));
    }
}
