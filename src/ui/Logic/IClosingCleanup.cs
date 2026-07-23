namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Implemented by view models that own background <see cref="System.Timers.Timer"/>s (preview,
/// debounce, download-progress, ...). <see cref="OnClosingCleanup"/> is invoked once from the
/// window's <c>Closed</c> event (wired centrally in <see cref="UiUtil.InitializeWindow"/>), so the
/// timers are stopped and disposed on every close path - OK/Cancel buttons, Escape, the title-bar
/// close and Alt+F4 alike - without each view model having to hook them individually.
///
/// Implementations must be idempotent: the method may run after the view model has already torn its
/// timers down through some other path.
/// </summary>
public interface IClosingCleanup
{
    void OnClosingCleanup();
}
