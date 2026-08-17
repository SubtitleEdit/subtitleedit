using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace UITests.Features.Edit;

/// <summary>
/// The "Replace/search in" picker (#13471). It is only meaningful with an editable original text
/// column on screen, so it must stay out of the window entirely the rest of the time.
/// </summary>
public class ReplaceWindowScopeTests
{
    // Shown and closed inside the test, with the dispatcher flushed while the window is still
    // alive: InitializeWindow posts a clamp-to-working-area job from Opened, which throws against
    // the disposed platform implementation if it is left to run during session teardown.
    private static bool IsScopePickerVisible(bool canEditOriginal)
    {
        var vm = new ReplaceViewModel();
        vm.RefreshSubtitles(["Hello world"], ["Hallo Welt"], canEditOriginal);
        var window = new ReplaceWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var comboBox = window.GetLogicalDescendants()
                .OfType<ComboBox>()
                .FirstOrDefault(c => c.ItemsSource is IEnumerable<ReplaceScopeDisplay>);

            Assert.NotNull(comboBox);
            return comboBox!.IsEffectivelyVisible;
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ScopePicker_IsShown_WhenOriginalIsEditable()
    {
        Assert.True(IsScopePickerVisible(canEditOriginal: true));
    }

    [AvaloniaFact]
    public void ScopePicker_IsHidden_WhenThereIsNoEditableOriginal()
    {
        Assert.False(IsScopePickerVisible(canEditOriginal: false));
    }

    // A scope the user could not see must not be saved over their remembered choice, and must not
    // reach the find service either - both columns is the only meaningful scope without an original.
    [AvaloniaFact]
    public void EffectiveScope_IsBothColumns_WhenPickerIsHidden()
    {
        var vm = new ReplaceViewModel();
        vm.RefreshSubtitles(["Hello world"], null, canEditOriginal: false);
        vm.SelectedScope = vm.Scopes.First(p => p.Scope == FindService.FindScope.OriginalOnly);

        Assert.Equal(FindService.FindScope.TextAndOriginal, vm.EffectiveScope);

        var before = Se.Settings.Edit.Find.ReplaceIn;
        vm.SaveSettings();
        Assert.Equal(before, Se.Settings.Edit.Find.ReplaceIn);
    }

    [AvaloniaFact]
    public void EffectiveScope_FollowsSelection_WhenPickerIsShown()
    {
        var vm = new ReplaceViewModel();
        vm.RefreshSubtitles(["Hello world"], ["Hallo Welt"], canEditOriginal: true);
        vm.SelectedScope = vm.Scopes.First(p => p.Scope == FindService.FindScope.TextOnly);

        Assert.Equal(FindService.FindScope.TextOnly, vm.EffectiveScope);
    }
}
