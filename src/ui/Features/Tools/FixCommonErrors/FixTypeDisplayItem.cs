using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

/// <summary>
/// One entry in the step 1 "Type" filter combo. FixType == null means "all types".
/// </summary>
public class FixTypeDisplayItem
{
    public FixType? FixType { get; }
    public string Name { get; }

    public FixTypeDisplayItem(FixType fixType)
    {
        FixType = fixType;
        Name = Se.Language.Tools.FixCommonErrors.GetFixTypeName(fixType);
    }

    public FixTypeDisplayItem() // "All" entry
    {
        FixType = null;
        Name = Se.Language.General.All;
    }

    // The combo uses the default item template, which shows ToString().
    public override string ToString()
    {
        return Name;
    }
}
