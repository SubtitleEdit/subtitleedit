using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomShortcuts;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Options.Shortcuts;

// The custom shortcut dialogs must build - a throwing view model made "+" silently do nothing.
public class CustomShortcutWindowsTests
{
    [AvaloniaFact]
    public void Windows_CanBeCreated()
    {
        var vm2 = new CustomShortcutEditViewModel(null!);
        vm2.Initialize(new SeCustomShortcut(), new List<CustomShortcutCommandItem>(), "Custom shortcut #1");
        _ = new CustomShortcutEditWindow(vm2);
        var vm3 = new CustomShortcutStepViewModel();
        vm3.Initialize(null, new List<CustomShortcutCommandItem>());
        _ = new CustomShortcutStepWindow(vm3);
    }

    [AvaloniaFact]
    public void StepViewModel_FiltersCommandsAndKeepsExistingStep()
    {
        var commands = new List<CustomShortcutCommandItem>
        {
            new("GoToNextLineCommand", "Go to next line", "General"),
            new("ItalicCommand", "Italic", "Text box"),
        };
        var vm = new CustomShortcutStepViewModel();
        vm.Initialize(new SeCustomShortcutStep { ActionName = "ItalicCommand" }, commands);

        Assert.Equal(2, vm.FilteredCommands.Count);
        Assert.Equal("ItalicCommand", vm.SelectedCommand?.ActionName);

        vm.CommandSearchText = "next";
        Assert.Equal("GoToNextLineCommand", Assert.Single(vm.FilteredCommands).ActionName);
    }

    [AvaloniaFact]
    public void EditViewModel_KeepsActiveInAndShowsTextBoxHint()
    {
        var vm = new CustomShortcutEditViewModel(null!);
        vm.Initialize(new SeCustomShortcut { ActiveIn = nameof(ShortcutCategory.TextBox) }, new List<CustomShortcutCommandItem>(), "Custom shortcut #1");

        Assert.Equal(ShortcutCategory.TextBox, vm.SelectedActiveIn.Value);
        Assert.True(vm.IsTextBoxKeyHintVisible);

        vm.SelectedActiveIn = vm.ActiveInChoices.First(p => p.Value == ShortcutCategory.SubtitleGrid);
        Assert.False(vm.IsTextBoxKeyHintVisible);
        vm.OkCommand.Execute(null);

        Assert.Equal(ShortcutCategory.SubtitleGrid, vm.CustomShortcut.GetActiveIn());
    }
}
