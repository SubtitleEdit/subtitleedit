using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportContentAlignmentDisplay
{
    public ExportContentAlignment ContentAlignment { get; set; }
    public string DisplayName { get; set; }

    public ExportContentAlignmentDisplay(ExportContentAlignment exportAlignment, string displayName)
    {
        ContentAlignment = exportAlignment;
        DisplayName = displayName;
    }

    override public string ToString()
    {
        return DisplayName;
    }   

    public static List<ExportContentAlignmentDisplay> GetAlignments()
    {
        return new List<ExportContentAlignmentDisplay>
        {
            new ExportContentAlignmentDisplay(ExportContentAlignment.Left, "Left"),
            new ExportContentAlignmentDisplay(ExportContentAlignment.Center, "Center"),
            new ExportContentAlignmentDisplay(ExportContentAlignment.Right, "Right"),
        };
    }
}