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
