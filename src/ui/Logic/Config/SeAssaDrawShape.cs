using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// A shape saved to the ASSA draw shape library.
/// </summary>
public class SeAssaDrawShape
{
    public string Name { get; set; } = string.Empty;
    public List<SeAssaDrawShapePart> Parts { get; set; } = [];
}

/// <summary>
/// One color of a saved library shape: ASSA drawing commands (may hold several "m" shapes).
/// </summary>
public class SeAssaDrawShapePart
{
    public string Drawing { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool IsEraser { get; set; }
}
