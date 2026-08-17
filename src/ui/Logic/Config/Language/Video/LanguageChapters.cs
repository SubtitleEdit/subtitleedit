namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageChapters
{
    public string Title { get; set; }
    public string ChaptersDotDotDot { get; set; }
    public string Chapters { get; set; }
    public string SelectedChapter { get; set; }
    public string ChapterTitle { get; set; }
    public string StartTime { get; set; }
    public string AddChapter { get; set; }
    public string AddChapterAtVideoPosition { get; set; }
    public string GoToPreviousChapter { get; set; }
    public string GoToNextChapter { get; set; }
    public string ToggleChapterAtVideoPosition { get; set; }
    public string NewChapterTitle { get; set; }
    public string DeleteSelectedChapterQuestion { get; set; }
    public string ClearChaptersQuestion { get; set; }
    public string ImportFromVideo { get; set; }
    public string ImportFromFile { get; set; }
    public string ExportToFile { get; set; }
    public string ChapterFilesFilter { get; set; }
    public string NoChaptersFoundInVideo { get; set; }
    public string XChaptersImported { get; set; }
    public string AdjustTimes { get; set; }
    public string ShiftAllTimes { get; set; }
    public string ShiftAllTimesDescription { get; set; }
    public string ScaleTimes { get; set; }
    public string ScaleTimesDescription { get; set; }
    public string FromFrameRate { get; set; }
    public string ToFrameRate { get; set; }
    public string Apply { get; set; }
    public string WriteToVideo { get; set; }
    public string WriteToVideoTitle { get; set; }
    public string WriteToVideoDescription { get; set; }
    public string WriteToVideoUnsupportedContainer { get; set; }
    public string OutputFileName { get; set; }
    public string Writing { get; set; }
    public string WrittenToX { get; set; }
    public string UnableToWriteChapters { get; set; }
    public string ChaptersSavedToX { get; set; }
    public string NoVideoLoaded { get; set; }
    public string EmptyListCallToAction { get; set; }

    public LanguageChapters()
    {
        Title = "Chapters";
        ChaptersDotDotDot = "Chapters...";
        Chapters = "Chapters";
        SelectedChapter = "Selected chapter";
        ChapterTitle = "Chapter title";
        StartTime = "Start time";
        AddChapter = "Add chapter";
        AddChapterAtVideoPosition = "Add chapter at video position";
        GoToPreviousChapter = "Go to previous chapter";
        GoToNextChapter = "Go to next chapter";
        ToggleChapterAtVideoPosition = "Toggle chapter at video position";
        NewChapterTitle = "Chapter {0}";
        DeleteSelectedChapterQuestion = "Are you sure you want to delete the selected chapter?";
        ClearChaptersQuestion = "Are you sure you want to remove all chapters?";
        ImportFromVideo = "Import from video";
        ImportFromFile = "Import from file...";
        ExportToFile = "Export to file...";
        ChapterFilesFilter = "Chapter files";
        NoChaptersFoundInVideo = "No chapters found in the video file.";
        XChaptersImported = "{0} chapters imported";
        AdjustTimes = "Adjust times";
        ShiftAllTimes = "Shift all times";
        ShiftAllTimesDescription = "Move every chapter forward or backward by the same amount.";
        ScaleTimes = "Change frame rate";
        ScaleTimesDescription = "Scale every chapter time from one frame rate to another.";
        FromFrameRate = "From";
        ToFrameRate = "To";
        Apply = "Apply";
        WriteToVideo = "Write to video...";
        WriteToVideoTitle = "Write chapters to video";
        WriteToVideoDescription = "The chapters are written into a copy of the video file. Nothing is re-encoded, so this is quick and lossless.";
        WriteToVideoUnsupportedContainer = "Chapters can only be written to MP4 and Matroska files.";
        OutputFileName = "Output file name";
        Writing = "Writing chapters...";
        WrittenToX = "Chapters written to {0}";
        UnableToWriteChapters = "Unable to write chapters to the video file.";
        ChaptersSavedToX = "Chapters saved to {0}";
        NoVideoLoaded = "No video loaded";
        EmptyListCallToAction = "No chapters yet - add one at the video position, or import them from a video or file.";
    }
}
