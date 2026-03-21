using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeShortCut
{
    public string ActionName { get; set; }

    public List<string> Keys { get; set; }
    public string? ControlName { get; set; }

    public SeShortCut()
    {
        Keys = new List<string>();
        ActionName = string.Empty;
    }

    public SeShortCut(string action, List<string> keys)
    {
        ActionName = action;
        Keys = keys;
    }

    public SeShortCut(string action, List<string> keys, string controlName)
    {
        ActionName = action;
        Keys = keys;
        ControlName = controlName;
    }

    public SeShortCut(string action, List<string> keys, ShortcutCategory category)
    {
        ActionName = action;
        Keys = keys;
        ControlName = category.ToString();
    }

    public SeShortCut(ShortCut shortCut)
    {
        ActionName = shortCut.Name;
        Keys = shortCut.Keys;
        ControlName = shortCut.Category.ToString();
    }
}
