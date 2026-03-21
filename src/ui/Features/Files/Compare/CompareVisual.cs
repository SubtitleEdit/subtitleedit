using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public partial class CompareVisual 
{
    public string Name { get; set; }
    public CompareVisualType Type { get; set; }

    public CompareVisual(string name, CompareVisualType type)
    {
        Name = name;
        Type = type;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<CompareVisual> GetCompareVisuals()
    {
        return new List<CompareVisual>
        {
            new CompareVisual(Se.Language.General.All, CompareVisualType.All),
            new CompareVisual(Se.Language.File.ShowOnlyDifferences, CompareVisualType.ShowOnlyDifferences),
            new CompareVisual(Se.Language.File.ShowOnlyDifferencesInText, CompareVisualType.ShowOnlyDifferencesInText)
        };
    }
}