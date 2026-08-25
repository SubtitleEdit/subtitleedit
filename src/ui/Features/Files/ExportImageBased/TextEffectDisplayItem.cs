using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Export;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class TextEffectDisplayItem
{
    public TextEffectPreset Preset { get; }
    public string Name { get; }

    public TextEffectDisplayItem(TextEffectPreset preset, string name)
    {
        Preset = preset;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<TextEffectDisplayItem> GetItems()
    {
        return new List<TextEffectDisplayItem>
        {
            new(TextEffectPreset.None, Se.Language.General.None),
            new(TextEffectPreset.SoftShadow, Se.Language.File.Export.TextEffectSoftShadow),
            new(TextEffectPreset.GradientGold, Se.Language.File.Export.TextEffectGradientGold),
            new(TextEffectPreset.DoubleOutline, Se.Language.File.Export.TextEffectDoubleOutline),
            new(TextEffectPreset.NeonGlow, Se.Language.File.Export.TextEffectNeonGlow),
            new(TextEffectPreset.Extrude3D, Se.Language.File.Export.TextEffectExtrude3D),
            new(TextEffectPreset.Chrome, Se.Language.File.Export.TextEffectChrome),
            new(TextEffectPreset.Fire, Se.Language.File.Export.TextEffectFire),
        };
    }
}
