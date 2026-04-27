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
    public string ImportTimeCodes { get; set; }
    public string SetTextForSubtitle { get; set; }
    public string ScreenWidth { get; set; }
    public string ScreenHeight { get; set; }
    public string AlphaThresholdInfo { get; set; }
    public string ResetToDefaults { get; set; }
    public string AlphaAdjustmentInfo { get; set; }
    public string AdjustBrightness { get; set; }
    public string Brightness { get; set; }
    public string Contrast { get; set; }
    public string Gamma { get; set; }
    public string BrightnessAdjustmentInfo { get; set; }
    public string ResizeImages { get; set; }
    public string Percentage { get; set; }
    public string ResizeImagesInfo { get; set; }

    public LanguageImageBasedEdit()
    {
        EditImagedBaseSubtitle = "Edit imaged-based subtitle";
        EditImagedBaseSubtitleX = "Edit imaged-based subtitle: {0}";
        ResizeImagesDotDotDot = "Resize images...";
        AdjustBrightnessDotDotDot = "Adjust brightness...";
        AdjustAlphaDotDotDot = "Adjust alpha...";
        CenterHorizontally = "Center horizontally";
        CropImages = "Crop images";
        ImportTimeCodes = "Import time codes...";
        SetTextForSubtitle = "Set text for subtitle";
        ScreenWidth = "Screen width";
        ScreenHeight = "Screen height";
        AlphaThresholdInfo = "Pixels with alpha below threshold become fully transparent";
        ResetToDefaults = "Reset to defaults";
        AlphaAdjustmentInfo = "Alpha adjustment: Add/subtract from alpha channel.\nPreview updates automatically and shows with checkered background to visualize transparency.";
        AdjustBrightness = "Adjust brightness";
        Brightness = "Brightness";
        Contrast = "Contrast";
        Gamma = "Gamma";
        BrightnessAdjustmentInfo = "Adjust the sliders to modify brightness, contrast, and gamma.\nPreview updates automatically and shows the first selected subtitle.";
        ResizeImages = "Resize images";
        Percentage = "Percentage";
        ResizeImagesInfo = "Enter the percentage to resize images.\nPreview updates automatically.";
    }
}