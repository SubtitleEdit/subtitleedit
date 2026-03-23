namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageRestoreAutoBackup
{
    public string Title { get; set; }
    public string OpenAutoBackupFolder { get; set; }
    public string RestoreAutoBackupFile { get; set; }
    public string DeleteAllSubtitleBackups { get; set; }
    public string DeleteAll { get; set; }

    public LanguageRestoreAutoBackup()
    {
        Title = "Restore auto-backup";
        OpenAutoBackupFolder = "Open auto-backup folder";
        RestoreAutoBackupFile = "Restore auto-backup file";
        DeleteAllSubtitleBackups = "Do you want to delete all subtitle backup files?";
        DeleteAll = "Delete all";
    }
}
