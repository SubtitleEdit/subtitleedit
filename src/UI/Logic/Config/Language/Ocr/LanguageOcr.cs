using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Translate;

public class LanguageOcr
{
    public string LinesToDraw { get; set; }
    public string CurrentImage { get; set; }
    public string AutoDrawAgain { get; set; }
    public string StartOcr { get; set; }
    public string PauseOcr { get; set; }
    public string InspectLine { get; set; }
    public string OcrEngine { get; set; }
    public string Database { get; set; }
    public string MaxWrongPixels { get; set; }
    public string NumberOfPixelsIsSpace { get; set; }
    public string InspectImageMatches { get; set; }
    public string ResolutionXYAndTopmarginZ { get; set; }
    public string RunningOcrDotDotDotXY { get; set; }
    public string RunningOcrDotDotDot { get; set; }
    public string AutoSubmitFirstCharacter { get; set; }
    public string EditNOcrDatabase { get; set; }
    public string ZoomFactorX { get; set; }
    public string ExpandInfoX { get; set; }
    public string EditNOcrDatabaseXWithYItems { get; set; }
    public string NewNOcrDatabase { get; set; }
    public string RenameNOcrDatabase { get; set; }
    public string NOcrDatabase { get; set; }
    public string DrawMode { get; set; }
    public string AddNewCharcter { get; set; }
    public string LineIndexX { get; set; }
    public string InspectNOcrAdditions { get; set; }
    public string OcrSelectedLines { get; set; }
    public string ShowImage { get; set; }
    public string FixOcrErrors { get; set; }
    public string PromptForUknownWords { get; set; }
    public string TryToGuessUnknownWords { get; set; }
    public string AutoBreakIfMoreThanXLines { get; set; }
    public string UnknownWords { get; set; }
    public string AllFixes { get; set; }
    public string GuessesUsed { get; set; }
    public string Ocr { get; set; }
    public string OcrX { get; set; }
    public string AddBetterMatch { get; set; }
    public string NOcrInspectImageMatches { get; set; }
    public string AddToOcrPair { get; set; }
    public string AddNameToOcrReplaceList { get; set; }
    public string WordToAdd { get; set; }
    public string NameToAdd { get; set; }
    public string ChangeWordFromTo { get; set; }
    public string ClearBackground { get; set; }
    public string ClearForeground { get; set; }
    public string NOcrDrawHelp { get; set; }
    public string EditWholeText { get; set; }
    public string EditWordOnly { get; set; }
    public string ImagePreProcessing { get; set; }
    public string PreProcessingTitle { get; set; }
    public string CropTransparent { get; set; }
    public string InverseColors { get; set; }
    public string RemoveBorders { get; set; }
    public string Binarize { get; set; }
    public string BorderSize { get; set; }
    public string CaptureTopAlign { get; set; }
    public string OcrImage { get; set; }
    public string OneColor { get; set; }
    public string DarknessThreshold { get; set; }
    public string EditExportDotDotDot { get; set; }
    public string EditBinaryOcrDatabase { get; set; }
    public string BinaryImageCompareDatabase { get; set; }
    public string RemoveXFromUnknownWordsList { get; set; }
    public string DownloadingPaddleOcrEngineDotDotDot { get; set; }
    public string DownloadingPaddleOcrModelsDotDotDot { get; set; }
    public string PaddleOcr { get; set; }

    public LanguageOcr()
    {
        LinesToDraw = "Lines to draw";
        CurrentImage = "Current image";
        AutoDrawAgain = "Auto draw again";
        StartOcr = "Start OCR";
        PauseOcr = "Pause OCR";
        InspectLine = "Inspect line...";
        OcrEngine = "OCR Engine";
        Database = "Database";
        MaxWrongPixels = "Max wrong pixels";
        NumberOfPixelsIsSpace = "Number of pixels is space";
        InspectImageMatches = "Inspect image matches";
        ResolutionXYAndTopmarginZ = "Resolution {0}x{1}, top margin {2}";
        RunningOcrDotDotDotXY = "Running OCR... {0}/{1}";
        RunningOcrDotDotDot = "Running OCR...";
        AutoSubmitFirstCharacter = "Auto submit first character";
        EditNOcrDatabase = "Edit nOCR database";
        ZoomFactorX = "Zoom factor: {0}x";
        ExpandInfoX = "Expand count: {0}";
        EditNOcrDatabaseXWithYItems = "Edit nOCR database {0} with {1:#,###,##0} items";
        NewNOcrDatabase = "New nOCR database";
        RenameNOcrDatabase = "Rename nOCR database";
        NOcrDatabase = "nOCR database";
        DrawMode = "Draw mode:";
        AddNewCharcter = "Add new character";
        LineIndexX = "Line {0}";
        InspectNOcrAdditions = "Inspect new nOCR additions";
        OcrSelectedLines = "OCR selected lines";
        ShowImage = "Show image";
        FixOcrErrors = "Fix OCR errors";
        PromptForUknownWords = "Prompt for unknown words";
        TryToGuessUnknownWords = "Try to guess unknown words";
        AutoBreakIfMoreThanXLines = "Auto break if more than {0} lines";
        UnknownWords = "Unknown words";
        AllFixes = "All fixes";
        GuessesUsed = "Guesses used";
        Ocr = "OCR";
        OcrX = "OCR - {0}";
        AddBetterMatch = "Add better match";
        NOcrInspectImageMatches = "nOCR - Inspect image matches";
        AddToOcrPair = "Add to OCR replace pairs";
        AddNameToOcrReplaceList = "Add name to OCR replace list";
        WordToAdd = "Word to add";
        NameToAdd = "Name to add";
        ChangeWordFromTo = "Change word from/to";
        ClearBackground = "Clear background";
        ClearForeground = "Clear foreground";
        NOcrDrawHelp = "Tips for drawing\r\n────────────────────\r\n• Hold Ctrl down to continue line\r\n• Ctrl+z=undo, Ctrl+y=redo";
        EditWholeText = "Edit whole text";
        EditWordOnly = "Edit word only";
        ImagePreProcessing = "Image pre-processing";
        PreProcessingTitle = "Pre-processing";
        CropTransparent = "Crop transparent colors";
        InverseColors = "Inverse colors";
        RemoveBorders = "Remove borders";
        Binarize = "Binarize";
        BorderSize = "Border size";
        CaptureTopAlign = "Capture top align";
        OcrImage = "OCR image";
        OneColor = "One color (white)";
        DarknessThreshold = "Darkness threshold";
        EditExportDotDotDot = "Edit/export...";
        EditBinaryOcrDatabase = "Edit \"Binary image compare\" database";
        BinaryImageCompareDatabase = "\"Binary image compare\" database";
        RemoveXFromUnknownWordsList = "Remove \"{0}\" from unknown words list";
        DownloadingPaddleOcrEngineDotDotDot = "Downloading Paddle OCR engine...";
        DownloadingPaddleOcrModelsDotDotDot = "Downloading Paddle OCR models...";
        PaddleOcr = "Paddle OCR";
    }
}