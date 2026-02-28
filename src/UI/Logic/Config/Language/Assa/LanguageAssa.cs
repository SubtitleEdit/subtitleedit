namespace Nikse.SubtitleEdit.Logic.Config.Language.Assa;

public class LanguageAssa
{

    // ASSA Draw
    public string AssaDraw { get; set; }
    public string DrawSelectTool { get; set; }
    public string DrawLineTool { get; set; }
    public string DrawBezierTool { get; set; }
    public string DrawRectangleTool { get; set; }
    public string DrawCircleTool { get; set; }
    public string DrawCloseShape { get; set; }
    public string DrawDeleteShape { get; set; }
    public string DrawChangeLayer { get; set; }
    public string DrawClearAll { get; set; }
    public string DrawZoomIn { get; set; }
    public string DrawZoomOut { get; set; }
    public string DrawResetView { get; set; }
    public string DrawToggleGrid { get; set; }
    public string DrawCopyToClipboard { get; set; }
    public string DrawShapes { get; set; }
    public string DrawSelectedPoint { get; set; }
    public string DrawSelectedShape { get; set; }
    public string DrawSelectedLayer { get; set; }
    public string DrawToolX { get; set; }
    public string DrawHelpText { get; set; }

    // Progress Bar Generator
    public string ProgressBarTitle { get; set; }
    public string ProgressBarSettings { get; set; }
    public string ProgressBarPosition { get; set; }
    public string ProgressBarBottom { get; set; }
    public string ProgressBarTop { get; set; }
    public string ProgressBarForeColor { get; set; }
    public string ProgressBarBackColor { get; set; }
    public string ProgressBarStyle { get; set; }
    public string ProgressBarSquareCorners { get; set; }
    public string ProgressBarRoundedCorners { get; set; }
    public string ProgressBarChapters { get; set; }
    public string ProgressBarSplitterWidth { get; set; }
    public string ProgressBarSplitterHeight { get; set; }
    public string ProgressBarXAdjustment { get; set; }
    public string ProgressBarYAdjustment { get; set; }
    public string ProgressBarTextAlignment { get; set; }
    public string ProgressBarTakePosFromVideo { get; set; }
    public string ProgressBarPreview { get; set; }

    // Resolution Resampler
    public string ResolutionResamplerTitle { get; set; }
    public string ResolutionResamplerSourceRes { get; set; }
    public string ResolutionResamplerTargetRes { get; set; }
    public string ResolutionResamplerChangeMargins { get; set; }
    public string ResolutionResamplerChangeFontSize { get; set; }
    public string ResolutionResamplerChangePositions { get; set; }
    public string ResolutionResamplerChangeDrawing { get; set; }
    public string ResolutionResamplerFromVideo { get; set; }
    public string ResolutionResamplerSourceAndTargetEqual { get; set; }
    public string ResolutionResamplerNothingSelected { get; set; }

    // Background Box Generator
    public string BackgroundBoxGenerator { get; set; }
    public string BackgroundBoxPadding { get; set; }
    public string BackgroundBoxFillWidth { get; set; }
    public string BackgroundBoxBoxColor { get; set; }
    public string BackgroundBoxRadius { get; set; }
    public string BackgroundBoxCircle { get; set; }
    public string BackgroundBoxSpikes { get; set; }
    public string BackgroundBoxBubbles { get; set; }

    public string StylesTitle { get; set; }
    public string StylesInFile { get; set; }
    public string StylesSaved { get; set; }
    public string StylesTitleX { get; set; }
    public string PropertiesTitleX { get; set; }
    public string AttachmentsTitleX { get; set; }
    public string SmartWrappingTopWide { get; set; }
    public string EndOfLineWrapping { get; set; }
    public string NoWrapping { get; set; }
    public string SmartWrappingBottomWide { get; set; }
    public string FontsAndGraphics { get; set; }
    public string WrapStyle { get; set; }
    public string BorderAndShadowScaling { get; set; }
    public string OriginalScript { get; set; }
    public string Graphics { get; set; }
    public string CopyToStorageStyles { get; set; }
    public string CopyToFileStyles { get; set; }
    public string SetStyleAsDefault { get; set; }
    public string TakeUsagesFromDotDotDot { get; set; }
    public string NoAttachmentsFound { get; set; }
    public string DeleteStyleQuestion { get; set; }
    public string DeleteStylesQuestion { get; set; }
    public string OpenStyleImportFile { get; set; }
    public string Primary { get; set; }
    public string Secondary { get; set; }
    public string ApplyOverrideTags { get; set; }
    public string ChooseOverrideTagToAdd { get; set; }
    public string FontSizeChange { get; set; }
    public string MoveTextFromLeftToRight { get; set; }
    public string ColorFromWhiteToRed { get; set; }
    public string RotateXSlow { get; set; }
    public string RotateX { get; set; }
    public string RotateY { get; set; }
    public string RotateTilt { get; set; }
    public string SpaceIncrease { get; set; }
    public string PlayCurrent { get; set; }
    public string SetPosition { get; set; }
    public string ImageColorPicker { get; set; }
    public string CopyColorAsHextoClipboard { get; set; }
    public string GeneratingBackgroundBoxXOfY { get; set; }

    public LanguageAssa()
    {
        // ASSA Draw
        AssaDraw = "ASSA Draw";
        DrawSelectTool = "Select (move points)";
        DrawLineTool = "Line Tool (F4)";
        DrawBezierTool = "Bezier Curve (F5)";
        DrawRectangleTool = "Rectangle (F6)";
        DrawCircleTool = "Circle (F7)";
        DrawCloseShape = "Close Shape (F8/Enter)";
        DrawDeleteShape = "Delete Shape (Del)";
        DrawChangeLayer = "Change Layer";
        DrawClearAll = "Clear All (Ctrl+N)";
        DrawZoomIn = "Zoom In (Ctrl++)";
        DrawZoomOut = "Zoom Out (Ctrl+-)";
        DrawResetView = "Reset View (Ctrl+0)";
        DrawToggleGrid = "Toggle Grid (Ctrl+G)";
        DrawCopyToClipboard = "Copy to Clipboard (Ctrl+C)";
        DrawShapes = "Shapes";
        DrawSelectedPoint = "Selected point";
        DrawSelectedShape = "Selected shape";
        DrawSelectedLayer = "Selected layer";
        DrawToolX = "Tool: {0}";
        DrawHelpText = "Click to add points • Enter/F8 to close shape • Shift+Drag to pan • Ctrl+Scroll to zoom";


        // Progress Bar Generator
        ProgressBarTitle = "ASSA progress bar";
        ProgressBarSettings = "Progress bar";
        ProgressBarPosition = "Position";
        ProgressBarBottom = "Bottom";
        ProgressBarTop = "Top";
        ProgressBarForeColor = "Foreground color";
        ProgressBarBackColor = "Background color";
        ProgressBarStyle = "Style";
        ProgressBarSquareCorners = "Square corners";
        ProgressBarRoundedCorners = "Rounded corners";
        ProgressBarChapters = "Chapters";
        ProgressBarSplitterWidth = "Splitter width";
        ProgressBarSplitterHeight = "Splitter height";
        ProgressBarXAdjustment = "X adjustment";
        ProgressBarYAdjustment = "Y adjustment";
        ProgressBarTextAlignment = "Text alignment";
        ProgressBarTakePosFromVideo = "Take position from video";
        ProgressBarPreview = "Preview";

        // Resolution Resampler
        ResolutionResamplerTitle = "Change resolution";
        ResolutionResamplerSourceRes = "Source resolution";
        ResolutionResamplerTargetRes = "Target resolution";
        ResolutionResamplerChangeMargins = "Change margins";
        ResolutionResamplerChangeFontSize = "Change font size";
        ResolutionResamplerChangePositions = "Change positions";
        ResolutionResamplerChangeDrawing = "Change drawing";
        ResolutionResamplerFromVideo = "From video...";
        ResolutionResamplerSourceAndTargetEqual = "Source and target resolution are the same - nothing to do.";
        ResolutionResamplerNothingSelected = "Please select at least one option to change.";

        // Background Box Generator
        BackgroundBoxGenerator = "ASSA background box generator";
        BackgroundBoxGenerator = "Generate background box";
        BackgroundBoxPadding = "Padding";
        BackgroundBoxFillWidth = "Fill width";
        BackgroundBoxBoxColor = "Box color";
        BackgroundBoxRadius = "Radius";
        BackgroundBoxCircle = "Circle";
        BackgroundBoxSpikes = "Spikes";
        BackgroundBoxBubbles = "Bubbles";

        StylesTitle = "Advanced Sub Station Alpha styles";
        StylesInFile = "Styles in file";
        StylesSaved = "Styles saved";
        StylesTitleX = "Styles - {0}";
        PropertiesTitleX = "Properties - {0}";
        AttachmentsTitleX = "Attachments - {0}";
        SmartWrappingTopWide = "0: Smart wrapping (top wide)";
        EndOfLineWrapping = "1: End-of-line word wrapping, only \\N breaks";
        NoWrapping = "2: No wrapping, both \\N an \\n breaks";
        SmartWrappingBottomWide = "3: Smart wrapping (bottom wide)";
        FontsAndGraphics = "Fonts and graphics";
        WrapStyle = "Wrap style";
        BorderAndShadowScaling = "Border and shadow scaling";
        OriginalScript = "Original script";
        Graphics = "Graphics";
        CopyToStorageStyles = "Copy to storage styles";
        CopyToFileStyles = "Copy to file styles";
        SetStyleAsDefault = "Set style as default";
        TakeUsagesFromDotDotDot = "Take usages from...";
        NoAttachmentsFound = "No attachments found in selected ASSA file.";
        DeleteStyleQuestion = "Delete style?";
        DeleteStylesQuestion = "Delete styles?";
        OpenStyleImportFile = "Open subtitle file to import styles from";
        Primary = "Primary";
        Secondary = "Secondary";
        ApplyOverrideTags = "Apply override tags";
        ChooseOverrideTagToAdd = "Choose override tag to add";
        FontSizeChange = "Font size change";
        MoveTextFromLeftToRight = "Move text from left to right";
        ColorFromWhiteToRed = "Color from white to red";
        RotateXSlow = "Rotate X (slow)";
        RotateX = "Rotate X";
        RotateY = "Rotate Y";
        RotateTilt = "Rotate tilt";
        SpaceIncrease = "Space increase";
        PlayCurrent = "Play current";
        SetPosition = "Set position";
        ImageColorPicker = "Image color picker";
        CopyColorAsHextoClipboard = "Copy color as hex to clipboard";
        GeneratingBackgroundBoxXOfY = "Generating background box {0} of {1}...";
    }
}