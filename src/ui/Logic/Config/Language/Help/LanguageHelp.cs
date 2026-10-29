namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageHelp
{
    public string AboutSubtitleEdit { get; set; }
    public string CheckForUpdates { get; set; }
    public string CheckForUpdatesChecking { get; set; }
    public string CheckForUpdatesUpToDate { get; set; }
    public string CheckForUpdatesNewVersionAvailable { get; set; }
    public string CheckForUpdatesUnableToCheck { get; set; }
    public string CheckForUpdatesDownloadNewVersion { get; set; }
    public string CheckForUpdatesInstallAndRestart { get; set; }
    public string CheckForUpdatesInstallFailed { get; set; }
    public string CheckForUpdatesInstallsWhenClosed { get; set; }

    public LanguageHelp()
    {
        AboutSubtitleEdit = "About Subtitle Edit";
        CheckForUpdates = "Check for updates";
        CheckForUpdatesChecking = "Checking for updates...";
        CheckForUpdatesUpToDate = "You are running the latest version.";
        CheckForUpdatesNewVersionAvailable = "New version available: {0}";
        CheckForUpdatesUnableToCheck = "Unable to check for updates.";
        CheckForUpdatesDownloadNewVersion = "Download new version";
        CheckForUpdatesInstallAndRestart = "Install and restart";
        CheckForUpdatesInstallFailed = "Automatic update failed - please use \"Download new version\" instead.";
        CheckForUpdatesInstallsWhenClosed = "The update will be installed when Subtitle Edit closes.";
    }

}