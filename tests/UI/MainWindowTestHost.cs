using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests;

/// <summary>
/// Shared setup for tests that host <see cref="MainView"/> in a window.
/// </summary>
internal static class MainWindowTestHost
{
    /// <summary>
    /// Takes <see cref="MainViewModel.OnClosing"/> off the window so closing it cannot strand a
    /// modal message box.
    ///
    /// OnClosing cancels the close and opens the "Save changes?" box whenever the subtitle differs
    /// from the last saved state - which is every test that puts lines in the grid. Nothing answers
    /// that box, so it and the window it belongs to stay open for the rest of the run, and
    /// WindowService.ShowModalAsync goes on enforcing the stranded box's foreground: each time a
    /// later test focuses a control of its own, the enforcement pulls keyboard focus back to the
    /// box's default button. Avalonia raises key events on a single app-wide focused element
    /// (KeyboardDevice.FocusedElement) and the whole headless run shares one application, so one
    /// unanswered prompt makes key presses land nowhere in an arbitrary later test - the CI flake
    /// that picked a different victim on every run.
    ///
    /// A test that wants the prompt (see MainWindowClosingTranslationTests) keeps the handler and
    /// closes the message box itself instead.
    /// </summary>
    internal static void SuppressSaveChangesPromptOnClose(this Window window, MainViewModel vm)
    {
        window.Closing -= vm.OnClosing;
    }
}
