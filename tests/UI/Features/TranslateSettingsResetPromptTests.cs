using System.Collections.Generic;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace UITests.Features;

/// <summary>
/// The reset button in auto-translate advanced settings is an icon-only button built in code and only
/// shown for engines that have a prompt, so a dropped child or a stale visibility binding would not
/// show up in a build. These tests assert the button reaches the logical tree and that the command
/// behind it puts the built-in prompt back in the text box.
/// </summary>
public class TranslateSettingsResetPromptTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: if a test stops early, an
    // unclosed window would outlive the test and race with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private static Button? FindResetPromptButton(Control root)
    {
        return root.GetLogicalDescendants().OfType<Button>().FirstOrDefault(b =>
            AutomationProperties.GetName(b) == Se.Language.Translate.ResetPromptToDefault);
    }

    [AvaloniaFact]
    public void TranslateSettingsWindow_HasVisibleResetPromptButtonForPromptEngine()
    {
        var savedPrompt = Se.Settings.AutoTranslate.ChatGptPrompt;
        try
        {
            var vm = new TranslateSettingsViewModel();
            vm.LoadValues(new ChatGptTranslate());
            var window = new TranslateSettingsWindow(vm);
            _windows.Add(window);

            var button = FindResetPromptButton(window);

            Assert.True(vm.PromptIsVisible, "ChatGPT has a prompt, so the prompt row must be visible.");
            Assert.NotNull(button);
            Assert.True(button!.IsVisible);
        }
        finally
        {
            Se.Settings.AutoTranslate.ChatGptPrompt = savedPrompt;
        }
    }

    [AvaloniaFact]
    public void ResetPrompt_PutsBackTheBuiltInPrompt()
    {
        var savedPrompt = Se.Settings.AutoTranslate.ChatGptPrompt;
        try
        {
            Se.Settings.AutoTranslate.ChatGptPrompt = "My own prompt from {0} to {1}";
            var vm = new TranslateSettingsViewModel();
            vm.LoadValues(new ChatGptTranslate());
            Assert.Equal("My own prompt from {0} to {1}", vm.PromptText);

            vm.ResetPromptCommand.Execute(null);

            Assert.Equal(new SeAutoTranslate().ChatGptPrompt, vm.PromptText);
        }
        finally
        {
            Se.Settings.AutoTranslate.ChatGptPrompt = savedPrompt;
        }
    }

    /// <summary>
    /// Engines without a prompt hide the whole prompt row - the reset button must go with it, not
    /// linger as a button that resets nothing.
    /// </summary>
    [AvaloniaFact]
    public void TranslateSettingsWindow_HidesResetPromptButtonForEngineWithoutPrompt()
    {
        var vm = new TranslateSettingsViewModel();
        vm.LoadValues(new GoogleTranslateV1());
        var window = new TranslateSettingsWindow(vm);
        _windows.Add(window);

        Assert.False(vm.PromptIsVisible);
        var button = FindResetPromptButton(window);
        Assert.NotNull(button);
        Assert.False(button!.IsEffectivelyVisible);
    }
}
