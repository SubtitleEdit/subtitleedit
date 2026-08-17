using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeSsa
{
    public List<SeAssaStyle> StoredStyles { get; set; }

    public SeSsa()
    {
        // SSA keeps its own storage - seed it too, so a fresh install (and a settings reset) has
        // a default style rather than an empty "Styles saved" list.
        StoredStyles = new List<SeAssaStyle> { SeAssaStyle.MakeStorageDefault() };
    }
}
