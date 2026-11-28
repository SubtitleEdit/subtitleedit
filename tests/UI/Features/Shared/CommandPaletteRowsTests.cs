using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared.CommandPalette;

namespace UITests.Features.Shared;

/// <summary>
/// Palette rows show the shortcut as key caps, and stay right when a search reuses the rows.
/// </summary>
public class CommandPaletteRowsTests
{
    private static CommandPaletteItem MakeItem(string name, string keys = "") => new(name, name, "Tools", keys, new RelayCommand(() => { }));

    [Theory]
    [InlineData("", new string[0])]
    [InlineData("F5", new[] { "F5" })]
    [InlineData("Ctrl+Shift+S", new[] { "Ctrl", "Shift", "S" })]
    [InlineData("Ctrl++", new[] { "Ctrl", "+" })]
    public void ShortcutKeys_AreOnePerKey(string shortcut, string[] expected)
    {
        Assert.Equal(expected, MakeItem("Name", shortcut).ShortcutKeys);
    }

    [Theory]
    [InlineData("transcribe", "Speech to text")]
    [InlineData("transcirbe", "Speech to text")]
    [InlineData("captions burn", "Generate video with burned-in subtitles")]
    [InlineData("text\tspeech", "Speech to text")]
    [InlineData("transcribe speech", "Speech to text")]
    [InlineData("hardcode video", "Generate video with burned-in subtitles")]
    public void Search_MatchesAliasesTyposAndReorderedWords(string query, string expected)
    {
        var command = new RelayCommand(() => { });
        var vm = new CommandPaletteViewModel();
        vm.Initialize([
            new("speech", "Speech to text", "Video", "", command, "transcribe"),
            new("burn", "Generate video with burned-in subtitles", "Video", "", command, "burn captions hardcode"),
        ]);

        vm.SearchText = query;

        Assert.Equal(expected, vm.SelectedItem?.DisplayName);
    }

    [Fact]
    public void Search_PutsExactCommandsBeforeTyposAndSettings()
    {
        var vm = new CommandPaletteViewModel();
        var exact = MakeItem("Save");
        var typo = MakeItem("Wave");
        var settings = MakeItem("Search settings");
        vm.Initialize([typo, exact], settings);

        vm.SearchText = "save";
        Assert.Equal(new[] { exact, typo, settings }, vm.Items.ToArray());

        Assert.False(vm.IsNothingFound);

        vm.SearchText = "zoom anchor";
        Assert.Same(settings, vm.SelectedItem);
        Assert.Single(vm.Items);
        Assert.True(vm.IsNothingFound); // no command matched, even with the settings row shown

        vm.SearchText = "";
        Assert.DoesNotContain(settings, vm.Items);
    }

    [AvaloniaFact]
    public void Rows_ShowTheItemsLeftAfterASearch()
    {
        var vm = new CommandPaletteViewModel();
        vm.Initialize([MakeItem("Apple"), MakeItem("Banana"), MakeItem("Cherry")]);
        var window = new CommandPaletteWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        vm.SearchText = "cherry";
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var shown = window.GetVisualDescendants().OfType<ListBoxItem>().SelectMany(i => i.GetVisualDescendants().OfType<TextBlock>()).Select(t => t.Text).ToList();
        Assert.Contains("Cherry", shown);
        Assert.DoesNotContain("Apple", shown);
        window.Close();
    }
}
