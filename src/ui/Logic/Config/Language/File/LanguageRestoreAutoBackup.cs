namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageRestoreAutoBackup
{
    public string Title { get; set; }
    public string XBackups { get; set; }
    public string OpenAutoBackupFolder { get; set; }
    public string RestoreAutoBackupFile { get; set; }
    public string RestoreXFromY { get; set; }
    public string DeleteAllSubtitleBackups { get; set; }
    public string DeleteAll { get; set; }
    public string Subtitles { get; set; }
    public string Settings { get; set; }
    public string SettingsBackupInfo { get; set; }
    public string SettingsBackupOff { get; set; }
    public string LastSettingsBackupX { get; set; }
    public string NoSettingsBackupsYet { get; set; }
    public string OpenSettingsBackupFolder { get; set; }
    public string BackUpSettingsNow { get; set; }
    public string SettingsBackedUp { get; set; }
    public string RestoreSettings { get; set; }
    public string RestoreSettingsFromX { get; set; }
    public string SettingsRestored { get; set; }
    public string SettingsRestoreFailed { get; set; }
    public string DeleteAllSettingsBackups { get; set; }

    public LanguageRestoreAutoBackup()
    {
        Title = "Restore auto-backup";
        XBackups = "{0} backups";
        OpenAutoBackupFolder = "Open auto-backup folder";
        RestoreAutoBackupFile = "Restore auto-backup file";
        RestoreXFromY = "Do you want to restore \"{0}\" from {1}?";
        DeleteAllSubtitleBackups = "Do you want to delete all subtitle backup files?";
        DeleteAll = "Delete all";
        Subtitles = "Subtitles";
        Settings = "Settings";
        SettingsBackupInfo = "Settings.json is backed up when Subtitle Edit starts, at most once every {0} day(s). The newest {1} backups are kept.";
        SettingsBackupOff = "Automatic settings backup is turned off (Options → Settings → File).";
        LastSettingsBackupX = "Last backup: {0}";
        NoSettingsBackupsYet = "No settings backups yet";
        OpenSettingsBackupFolder = "Open settings backup folder";
        BackUpSettingsNow = "Back up now";
        SettingsBackedUp = "Current settings were backed up.";
        RestoreSettings = "Restore settings";
        RestoreSettingsFromX = "Do you want to replace your current settings with the backup from {0}?\n\nYour current settings will be backed up first. Some changes only take effect after restarting Subtitle Edit.";
        SettingsRestored = "Settings were restored. Please restart Subtitle Edit for all changes to take effect.";
        SettingsRestoreFailed = "Could not restore settings: {0}";
        DeleteAllSettingsBackups = "Do you want to delete all settings backup files?";
    }
}
