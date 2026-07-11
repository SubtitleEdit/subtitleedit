using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

/// <summary>
/// Thematic grouping of shortcuts, used only for browsing/filtering in the shortcuts
/// window. Unlike <see cref="ShortcutCategory"/> it has no effect on dispatch - the
/// category still decides which focused control a shortcut is active in.
/// </summary>
public enum ShortcutGroup
{
    General,
    File,
    Video,
    Waveform,
    SubtitleGrid,
    TextBox,
    SubtitleGridAndTextBox,
    Sync,
    Translate,
    Search,
    Tools,
    Ai,
}

public static class ShortcutGroupUi
{
    public static ShortcutGroup FromCategory(ShortcutCategory category)
    {
        return category switch
        {
            ShortcutCategory.Waveform => ShortcutGroup.Waveform,
            ShortcutCategory.SubtitleGrid => ShortcutGroup.SubtitleGrid,
            ShortcutCategory.TextBox => ShortcutGroup.TextBox,
            ShortcutCategory.SubtitleGridAndTextBox => ShortcutGroup.SubtitleGridAndTextBox,
            _ => ShortcutGroup.General,
        };
    }

    public static string GetName(ShortcutGroup group)
    {
        var language = Se.Language.Options.Shortcuts;
        return group switch
        {
            ShortcutGroup.File => language.CategoryFile,
            ShortcutGroup.Video => language.CategoryVideo,
            ShortcutGroup.Waveform => language.CategoryWaveform,
            ShortcutGroup.SubtitleGrid => language.CategorySubtitleGrid,
            ShortcutGroup.TextBox => language.CategoryTextBox,
            ShortcutGroup.SubtitleGridAndTextBox => language.CategorySubtitleGridAndTextBox,
            ShortcutGroup.Sync => language.CategorySync,
            ShortcutGroup.Translate => language.CategoryTranslate,
            ShortcutGroup.Search => language.CategorySearch,
            ShortcutGroup.Tools => language.CategoryTools,
            ShortcutGroup.Ai => language.CategoryAi,
            _ => language.CategoryGeneral,
        };
    }

    public static string GetIconName(ShortcutGroup group)
    {
        return group switch
        {
            ShortcutGroup.File => IconNames.FileOutline,
            ShortcutGroup.Video => IconNames.MovieOpenOutline,
            ShortcutGroup.Waveform => IconNames.Waveform,
            ShortcutGroup.SubtitleGrid => IconNames.ViewList,
            ShortcutGroup.TextBox => IconNames.FormTextBox,
            ShortcutGroup.SubtitleGridAndTextBox => IconNames.ViewSplitVertical,
            ShortcutGroup.Sync => IconNames.Sync,
            ShortcutGroup.Translate => IconNames.Translate,
            ShortcutGroup.Search => IconNames.Find,
            ShortcutGroup.Tools => IconNames.Tools,
            ShortcutGroup.Ai => IconNames.Creation,
            _ => IconNames.Settings,
        };
    }

    // Mid-saturation tones picked to stay readable on both the dark and light theme.
    public static IBrush GetBrush(ShortcutGroup group)
    {
        return group switch
        {
            ShortcutGroup.File => new SolidColorBrush(Color.Parse("#5fa8e0")),
            ShortcutGroup.Video => new SolidColorBrush(Color.Parse("#a78bfa")),
            ShortcutGroup.Waveform => new SolidColorBrush(Color.Parse("#f0b429")),
            ShortcutGroup.SubtitleGrid => new SolidColorBrush(Color.Parse("#4ec98a")),
            ShortcutGroup.TextBox => new SolidColorBrush(Color.Parse("#4fc3e8")),
            ShortcutGroup.SubtitleGridAndTextBox => new SolidColorBrush(Color.Parse("#7fa8f0")),
            ShortcutGroup.Sync => new SolidColorBrush(Color.Parse("#58c9b4")),
            ShortcutGroup.Translate => new SolidColorBrush(Color.Parse("#6bb84e")),
            ShortcutGroup.Search => new SolidColorBrush(Color.Parse("#d9a03f")),
            ShortcutGroup.Tools => new SolidColorBrush(Color.Parse("#f0885a")),
            ShortcutGroup.Ai => new SolidColorBrush(Color.Parse("#e668c4")),
            _ => new SolidColorBrush(Color.Parse("#8494a4")),
        };
    }
}
