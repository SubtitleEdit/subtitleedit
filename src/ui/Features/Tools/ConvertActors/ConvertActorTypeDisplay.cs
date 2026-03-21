using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.ConvertActors;

public class ConvertActorTypeDisplay
{
    public ConvertActorType Type { get; set; }
    public string DisplayName { get; set; }

    public ConvertActorTypeDisplay(ConvertActorType type, string displayName)
    {
        Type = type;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;

    public static List<ConvertActorTypeDisplay> GetTypes()
    {
        return
        [
            new ConvertActorTypeDisplay(ConvertActorType.InlineSquareBrackets, string.Format(Se.Language.Tools.ConvertActors.InlineActorViaX, "[]")),
            new ConvertActorTypeDisplay(ConvertActorType.InlineParentheses, string.Format(Se.Language.Tools.ConvertActors.InlineActorViaX, "()")),
            new ConvertActorTypeDisplay(ConvertActorType.InlineColon, string.Format(Se.Language.Tools.ConvertActors.InlineActorViaX, ":")),
            new ConvertActorTypeDisplay(ConvertActorType.Actor, Se.Language.General.Actor),
        ];
    }
}
