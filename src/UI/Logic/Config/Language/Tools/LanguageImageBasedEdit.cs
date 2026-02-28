namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageImageBasedEdit
{
    public string EditImagedBaseSubtitle { get; set; }
    public string EditImagedBaseSubtitleX { get; set; }
    public string ResizeImagesDotDotDot { get;  set; }
    public string AdjustBrightnessDotDotDot { get; set; }
    public string AdjustAlphaDotDotDot { get; set; }
    public string CenterHorizontally { get; set; }
    public string CropImages { get; set; }

    public LanguageImageBasedEdit()
    {
        EditImagedBaseSubtitle = "Edit imaged-based subtitle";
        EditImagedBaseSubtitleX = "Edit imaged-based subtitle: {0}";
        ResizeImagesDotDotDot = "Resize images...";
        AdjustBrightnessDotDotDot = "Adjust brightness...";
        AdjustAlphaDotDotDot = "Adjust alpha...";
        CenterHorizontally = "Center horizontally";
        CropImages = "Crop images";
    }
}