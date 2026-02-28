using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportAlignmentDisplay
{
    public ExportAlignment Alignment { get; set; }
    public string DisplayName { get; set; }

    public ExportAlignmentDisplay(ExportAlignment exportAlignment, string displayName)
    {
        Alignment = exportAlignment;
        DisplayName = displayName;
    }

    override public string ToString()
    {
        return DisplayName;
    }   

    public static List<ExportAlignmentDisplay> GetAlignments()
    {
        return new List<ExportAlignmentDisplay>
        {
            new ExportAlignmentDisplay(ExportAlignment.TopLeft, "Top Left"),
            new ExportAlignmentDisplay(ExportAlignment.TopCenter, "Top Center"),
            new ExportAlignmentDisplay(ExportAlignment.TopRight, "Top Right"),
            new ExportAlignmentDisplay(ExportAlignment.MiddleLeft, "Middle Left"),
            new ExportAlignmentDisplay(ExportAlignment.MiddleCenter, "Middle Center"),
            new ExportAlignmentDisplay(ExportAlignment.MiddleRight, "Middle Right"),
            new ExportAlignmentDisplay(ExportAlignment.BottomLeft, "Bottom Left"),
            new ExportAlignmentDisplay(ExportAlignment.BottomCenter, "Bottom Center"),
            new ExportAlignmentDisplay(ExportAlignment.BottomRight, "Bottom Right")
        };
    }
}