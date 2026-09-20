using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Export;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class Export3DModeDisplay
{
    public Export3DMode Mode { get; }
    public string Name { get; }

    public Export3DModeDisplay(Export3DMode mode, string name)
    {
        Mode = mode;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<Export3DModeDisplay> GetItems()
    {
        return
        [
            new(Export3DMode.None, Se.Language.General.None),
            new(Export3DMode.HalfSideBySide, Se.Language.File.Export.HalfSideBySide),
            new(Export3DMode.HalfTopBottom, Se.Language.File.Export.HalfTopBottom),
        ];
    }
}
