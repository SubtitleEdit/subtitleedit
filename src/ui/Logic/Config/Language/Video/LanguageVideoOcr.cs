namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageVideoOcr
{
    public string Title { get; set; }
    public string ScanArea { get; set; }
    public string ScanAreaInfo { get; set; }
    public string BottomThird { get; set; }
    public string BottomHalf { get; set; }
    public string FullFrame { get; set; }
    public string Engine { get; set; }
    public string Language { get; set; }
    public string Url { get; set; }
    public string Model { get; set; }
    public string ApiKey { get; set; }
    public string Scan { get; set; }
    public string FramesPerSecond { get; set; }
    public string TextBrightnessMinimum { get; set; }
    public string PostProcessing { get; set; }
    public string TextSimilarityPercent { get; set; }
    public string MaxGapMs { get; set; }
    public string MinDurationMs { get; set; }
    public string AddAssaPositionTag { get; set; }
    public string StartOcr { get; set; }
    public string ExtractingFramesX { get; set; }
    public string AnalyzingFramesXY { get; set; }
    public string RunningOcrXY { get; set; }
    public string NoLinesFoundTitle { get; set; }
    public string NoLinesFoundMessage { get; set; }
    public string LinesFoundX { get; set; }
    public string PreviewPosition { get; set; }
    public string UnableToReadVideoTitle { get; set; }
    public string UnableToReadVideoMessage { get; set; }
    public string AbortOcrTitle { get; set; }
    public string AbortOcrMessage { get; set; }
    public string TestOcr { get; set; }
    public string TestOcrRunning { get; set; }
    public string TestOcrResultX { get; set; }
    public string TestOcrNoTextFound { get; set; }

    public LanguageVideoOcr()
    {
        Title = "OCR burned-in subtitle";
        ScanArea = "Scan area";
        ScanAreaInfo = "Drag on the video frame to select where the subtitles are";
        BottomThird = "Bottom third";
        BottomHalf = "Bottom half";
        FullFrame = "Full frame";
        Engine = "OCR engine";
        Language = "Language";
        Url = "URL";
        Model = "Model";
        ApiKey = "API key";
        Scan = "Scan";
        FramesPerSecond = "Frames per second";
        TextBrightnessMinimum = "Text brightness minimum (0=off)";
        PostProcessing = "Post-processing";
        TextSimilarityPercent = "Merge lines with similarity (%)";
        MaxGapMs = "Max gap between lines (ms)";
        MinDurationMs = "Minimum duration (ms)";
        AddAssaPositionTag = "Add ASSA position tag (e.g. {\\an8})";
        StartOcr = "Start OCR";
        ExtractingFramesX = "Extracting image frames... {0}%";
        AnalyzingFramesXY = "Analyzing frames... {0}/{1}";
        RunningOcrXY = "Running OCR... {0}/{1}";
        NoLinesFoundTitle = "No subtitles found";
        NoLinesFoundMessage = "No text was found in the scan area - try adjusting the scan area, engine, or brightness minimum.";
        LinesFoundX = "{0} lines found";
        PreviewPosition = "Preview position";
        UnableToReadVideoTitle = "Unable to read video";
        UnableToReadVideoMessage = "Could not read video info from: {0}";
        AbortOcrTitle = "Abort OCR?";
        AbortOcrMessage = "Do you want to abort the running OCR?";
        TestOcr = "Test current frame";
        TestOcrRunning = "Testing OCR on current frame...";
        TestOcrResultX = "Test result: {0}";
        TestOcrNoTextFound = "Test: no text found in the scan area";
    }
}
