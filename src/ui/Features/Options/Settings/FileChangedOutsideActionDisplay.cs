using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class FileChangedOutsideActionDisplay
{
    public string Name { get; set; } = string.Empty;
    public FileChangedOutsideActionType Action { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public static List<FileChangedOutsideActionDisplay> List()
    {
        return
        [
            new() { Name = Se.Language.Options.Settings.FileChangedOutsideReloadWhenUnmodified, Action = FileChangedOutsideActionType.ReloadWhenUnmodified },
            new() { Name = Se.Language.Options.Settings.FileChangedOutsideAlwaysAsk, Action = FileChangedOutsideActionType.AlwaysAsk },
            new() { Name = Se.Language.Options.Settings.FileChangedOutsideNever, Action = FileChangedOutsideActionType.Never },
        ];
    }
}
