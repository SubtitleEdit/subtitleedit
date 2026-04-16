namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageRestoreAutoBackup
{
    public string Title { get; set; }
    public string OpenAutoBackupFolder { get; set; }
    public string RestoreAutoBackupFile { get; set; }
    public string RestoreXFromY { get; set; }
    public string DeleteAllSubtitleBackups { get; set; }
    public string DeleteAll { get; set; }

    public LanguageRestoreAutoBackup()
    {
        Title = "Restore auto-backup";
        OpenAutoBackupFolder = "Open auto-backup folder";
        RestoreAutoBackupFile = "Restore auto-backup file";
        RestoreXFromY = "Do you want to restore \"{0}\" from {0}?";
        DeleteAllSubtitleBackups = "Do you want to delete all subtitle backup files?";
        DeleteAll = "Delete all";
    }
}
