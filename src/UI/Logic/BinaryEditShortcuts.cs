using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Provides keyboard shortcuts for BinaryEditViewModel.
/// </summary>
public static class BinaryEditShortcuts
{
    /// <summary>
    /// Gets video control shortcuts from BinaryEditViewModel.
    /// </summary>
    public static List<ShortCut> GetVideoShortcuts(BinaryEditViewModel vm)
    {
        var shortcuts = new List<ShortCut>();

        var keys = Se.Settings.Shortcuts
            .Where(p => !p.ActionName.Contains(' '))
            .GroupBy(p => p.ActionName)
            .Select(g => g.First())
            .ToDictionary(p => p.ActionName, p => p);

        // Map video movement shortcuts
        AddShortcutIfExists(shortcuts, keys, nameof(vm.VideoOneFrameBackCommand), vm.VideoOneFrameBackCommand, Se.Language.General.VideoOneFrameBack);
        AddShortcutIfExists(shortcuts, keys, nameof(vm.VideoOneFrameForwardCommand), vm.VideoOneFrameForwardCommand, Se.Language.General.VideoOneFrameForward);
        AddShortcutIfExists(shortcuts, keys, nameof(vm.Video100MsBackCommand), vm.Video100MsBackCommand, Se.Language.General.Video100MsBack);
        AddShortcutIfExists(shortcuts, keys, nameof(vm.Video100MsForwardCommand), vm.Video100MsForwardCommand, Se.Language.General.Video100MsForward);
        AddShortcutIfExists(shortcuts, keys, nameof(vm.Video500MsBackCommand), vm.Video500MsBackCommand, Se.Language.General.Video500MsBack);
        AddShortcutIfExists(shortcuts, keys, nameof(vm.Video500MsForwardCommand), vm.Video500MsForwardCommand, Se.Language.General.Video500MsForward);
        AddShortcutIfExists(shortcuts, keys, nameof(vm.VideoOneSecondBackCommand), vm.VideoOneSecondBackCommand, Se.Language.General.VideoOneSecondBack);
        AddShortcutIfExists(shortcuts, keys, nameof(vm.VideoOneSecondForwardCommand), vm.VideoOneSecondForwardCommand, Se.Language.General.VideoOneSecondForward);
        AddShortcutIfExists(shortcuts, keys, nameof(vm.VideoMoveCustom1BackCommand), vm.VideoMoveCustom1BackCommand, 
            string.Format(Se.Language.General.VideoCustom1BackX, Se.Settings.Video.MoveVideoPositionCustom1Back));
        AddShortcutIfExists(shortcuts, keys, nameof(vm.VideoMoveCustom1ForwardCommand), vm.VideoMoveCustom1ForwardCommand,
            string.Format(Se.Language.General.VideoCustom1ForwardX, Se.Settings.Video.MoveVideoPositionCustom1Forward));
        AddShortcutIfExists(shortcuts, keys, nameof(vm.VideoMoveCustom2BackCommand), vm.VideoMoveCustom2BackCommand,
            string.Format(Se.Language.General.VideoCustom2BackX, Se.Settings.Video.MoveVideoPositionCustom2Back));
        AddShortcutIfExists(shortcuts, keys, nameof(vm.VideoMoveCustom2ForwardCommand), vm.VideoMoveCustom2ForwardCommand,
            string.Format(Se.Language.General.VideoCustom2ForwardX, Se.Settings.Video.MoveVideoPositionCustom2Forward));

        return shortcuts;
    }

    private static void AddShortcutIfExists(List<ShortCut> shortcuts, Dictionary<string, SeShortCut> keys, string commandName, IRelayCommand command, string displayName)
    {
        if (keys.TryGetValue(commandName, out var match))
        {
            shortcuts.Add(new ShortCut(displayName, match.Keys, ShortcutCategory.General, command));
        }
    }
}
