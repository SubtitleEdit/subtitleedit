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

    /// <summary>
    /// Selectable presets for the text effect settings window. "None" is not in the list -
    /// the enable checkbox in the export dialog covers that.
    /// </summary>
    public static List<TextEffectDisplayItem> GetItems()
    {
        return new List<TextEffectDisplayItem>
        {
            new(TextEffectPreset.SoftShadow, Se.Language.File.Export.TextEffectSoftShadow),
            new(TextEffectPreset.GradientGold, Se.Language.File.Export.TextEffectGradientGold),
            new(TextEffectPreset.DoubleOutline, Se.Language.File.Export.TextEffectDoubleOutline),
            new(TextEffectPreset.NeonGlow, Se.Language.File.Export.TextEffectNeonGlow),
            new(TextEffectPreset.Extrude3D, Se.Language.File.Export.TextEffectExtrude3D),
            new(TextEffectPreset.Chrome, Se.Language.File.Export.TextEffectChrome),
            new(TextEffectPreset.Fire, Se.Language.File.Export.TextEffectFire),
            new(TextEffectPreset.Comic, Se.Language.File.Export.TextEffectComic),
            new(TextEffectPreset.Retro80s, Se.Language.File.Export.TextEffectRetro80s),
            new(TextEffectPreset.Anaglyph3D, Se.Language.File.Export.TextEffectAnaglyph3D),
            new(TextEffectPreset.Ice, Se.Language.File.Export.TextEffectIce),
            new(TextEffectPreset.Emboss, Se.Language.File.Export.TextEffectEmboss),
            new(TextEffectPreset.Hollow, Se.Language.File.Export.TextEffectHollow),
            new(TextEffectPreset.Marble, Se.Language.File.Export.TextEffectMarble),
            new(TextEffectPreset.Wood, Se.Language.File.Export.TextEffectWood),
            new(TextEffectPreset.Lava, Se.Language.File.Export.TextEffectLava),
            new(TextEffectPreset.BrushedSteel, Se.Language.File.Export.TextEffectBrushedSteel),
            new(TextEffectPreset.CandyCane, Se.Language.File.Export.TextEffectCandyCane),
            new(TextEffectPreset.Rainbow, Se.Language.File.Export.TextEffectRainbow),
            new(TextEffectPreset.PolkaDots, Se.Language.File.Export.TextEffectPolkaDots),
        };
    }
}
