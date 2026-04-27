namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageShotChanges
{
    public string TitleGenerateOrImport { get; set; }
    public string GenerateShotChanges { get; set; }
    public string ImportShotChanges { get; set; }
    public string GenerateShotChangesWithFfmpeg { get; set; }
    public string ShotChangeTimeCode { get; set; }
    public string OpenShotChangesFile { get; set; }
    public string ImportShotChangesFromFile { get; set; }
    public string TimeCodeFormatColon { get; set; }
    public string ShotChangesList { get; set; }
    public string ShotChangesClearQuestion { get; set; }
    public string DeleteSelectedShotChangeQuestion { get; set; }

    public LanguageShotChanges()
    {
        TitleGenerateOrImport = "Generate/import shot changes";
        GenerateShotChanges = "Generate shot changes";
        ImportShotChanges = "Import shot changes";
        GenerateShotChangesWithFfmpeg = "Generate shot changes with ffmpeg";
        ShotChangeTimeCode = "Shot change time code";
        OpenShotChangesFile = "Open shot changes file";
        ImportShotChangesFromFile = "Import shot changes from file";
        TimeCodeFormatColon = "Time code format:";
        ShotChangesList = "Shot changes list";
        ShotChangesClearQuestion = "Are you sure you want to clear all shot changes?";
        DeleteSelectedShotChangeQuestion = "Are you sure you want to delete the selected shot change?";
    }
}