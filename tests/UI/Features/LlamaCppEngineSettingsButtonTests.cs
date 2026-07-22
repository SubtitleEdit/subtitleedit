using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Main.AiAssistant;
using Nikse.SubtitleEdit.Features.Tools.AiReview;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features;

/// <summary>
/// The llama.cpp engine settings button is added in code-built toolbars and is only shown when the
/// llama.cpp engine is selected, so a wrong grid index, a dropped child or a stale visibility binding
/// is invisible until the window is actually constructed. These tests assert the button reaches the
/// logical tree and is effectively visible, which a build alone cannot prove.
/// </summary>
public class LlamaCppEngineSettingsButtonTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static IEnumerable<Button> AllButtons(Control root)
    {
        return root.GetLogicalDescendants().OfType<Button>();
    }

    private static Button? FindEngineSettingsButton(Control root)
    {
        return AllButtons(root).FirstOrDefault(b =>
            AutomationProperties.GetName(b) == Se.Language.General.LlamaCppEngineSettings);
    }

    [AvaloniaFact]
    public void AiReviewWindow_HasLlamaCppEngineSettingsButton()
    {
        var vm = new AiReviewViewModel(new WindowService(new NullServiceProvider()));
        var window = new AiReviewWindow(vm);

        var button = FindEngineSettingsButton(window);

        Assert.NotNull(button);
        Assert.True(vm.IsLlamaCppVisible, "llama.cpp is the default engine, so its controls must be visible.");
        Assert.True(button!.IsVisible);
    }

    [AvaloniaFact]
    public void AiAssistantWindow_HasLlamaCppEngineSettingsButton()
    {
        var vm = new AiAssistantViewModel(new WindowService(new NullServiceProvider()));
        var window = new AiAssistantWindow(vm);

        var button = FindEngineSettingsButton(window);

        Assert.NotNull(button);
        Assert.True(vm.IsLlamaCppVisible, "llama.cpp is the default engine, so its controls must be visible.");
        Assert.True(button!.IsVisible);
    }

    /// <summary>
    /// The engine combos in both windows list plain strings and shipped with no ItemTemplate at all,
    /// so the llama.cpp row had no install dot. Assert the template is applied and that it produces a
    /// dot for llama.cpp but none for the external services.
    /// </summary>
    [AvaloniaFact]
    public void AiEngineCombo_ShowsDotForLlamaCppOnly()
    {
        var template = AiEngineCombo.ItemTemplate();

        var llamaCpp = template.Build(SeAiReview.EngineLlamaCpp);
        var ollama = template.Build(SeAiReview.EngineOllama);

        Assert.NotNull(llamaCpp);
        Assert.NotNull(ollama);
        Assert.Single(llamaCpp!.GetLogicalDescendants().OfType<Ellipse>());
        Assert.Empty(ollama!.GetLogicalDescendants().OfType<Ellipse>());
    }

    [AvaloniaFact]
    public void AiReviewWindow_EngineComboHasDotTemplate()
    {
        var vm = new AiReviewViewModel(new WindowService(new NullServiceProvider()));
        var window = new AiReviewWindow(vm);

        var engineCombo = window.GetLogicalDescendants().OfType<ComboBox>()
            .First(c => AutomationProperties.GetName(c) == Se.Language.General.Engine);

        Assert.NotNull(engineCombo.ItemTemplate);
    }

    [AvaloniaFact]
    public void AiAssistantWindow_EngineComboHasDotTemplate()
    {
        var vm = new AiAssistantViewModel(new WindowService(new NullServiceProvider()));
        var window = new AiAssistantWindow(vm);

        var engineCombo = window.GetLogicalDescendants().OfType<ComboBox>()
            .First(c => AutomationProperties.GetName(c) == Se.Language.General.Engine);

        Assert.NotNull(engineCombo.ItemTemplate);
    }

    /// <summary>
    /// Auto-translate builds its llama.cpp controls into a WrapPanel that is hidden unless the
    /// llama.cpp engine is selected and the "use external server" toggle is off, so assert the
    /// button is constructed rather than that it is visible on a default (non-llama.cpp) engine.
    /// </summary>
    [AvaloniaFact]
    public void AutoTranslateWindow_HasLlamaCppEngineSettingsButton()
    {
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var window = new AutoTranslateWindow(vm);

        Assert.NotNull(FindEngineSettingsButton(window));
    }

    /// <summary>
    /// Selecting a non-llama.cpp engine must hide the button along with the model combo - the two
    /// live in one panel, so this also guards against the button being re-parented out of it.
    /// </summary>
    [AvaloniaFact]
    public void AiReviewWindow_HidesEngineSettingsButtonForOtherEngines()
    {
        var vm = new AiReviewViewModel(new WindowService(new NullServiceProvider()));
        var window = new AiReviewWindow(vm);

        vm.SelectedEngine = SeAiReview.EngineOllama;

        Assert.False(vm.IsLlamaCppVisible);
        var button = FindEngineSettingsButton(window);
        Assert.NotNull(button);
        Assert.False(button!.IsEffectivelyVisible);
    }
}
