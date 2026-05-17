namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguagePlugins
{
    public string Title { get; set; }
    public string ManagePlugins { get; set; }
    public string NoPluginsInstalled { get; set; }
    public string GetPluginsOnline { get; set; }
    public string GetPluginsTitle { get; set; }
    public string Enabled { get; set; }
    public string Disabled { get; set; }
    public string NotSupportedOnThisOs { get; set; }
    public string RemovePluginXConfirm { get; set; }
    public string CouldNotRemovePluginX { get; set; }
    public string PluginXHasNoExecutable { get; set; }
    public string PluginXFailed { get; set; }
    public string PluginXReportedError { get; set; }
    public string PluginXDone { get; set; }
    public string PluginXReturnedUnparsableSubtitle { get; set; }
    public string LoadingPluginList { get; set; }
    public string NoPluginsAvailable { get; set; }
    public string XPluginsAvailable { get; set; }
    public string CouldNotLoadPluginList { get; set; }
    public string DownloadingXPercent { get; set; }
    public string PluginXInstalled { get; set; }
    public string CouldNotInstallPluginX { get; set; }
    public string NotInstalled { get; set; }
    public string Install { get; set; }
    public string Reinstall { get; set; }
    public string Update { get; set; }
    public string InstalledX { get; set; }
    public string UpdateAvailableXToY { get; set; }
    public string ApplyPluginToWhichLinesX { get; set; }
    public string ApplyToSelectedLinesX { get; set; }
    public string ApplyToAllLinesX { get; set; }

    public LanguagePlugins()
    {
        Title = "Plugins";
        ManagePlugins = "Manage plugins...";
        NoPluginsInstalled = "No plugins installed";
        GetPluginsOnline = "Get plugins online...";
        GetPluginsTitle = "Get plugins";
        Enabled = "Enabled";
        Disabled = "Disabled";
        NotSupportedOnThisOs = "Not supported on this operating system";
        RemovePluginXConfirm = "Remove plugin '{0}'? This deletes the plugin folder:{1}";
        CouldNotRemovePluginX = "Could not remove plugin '{0}': {1}";
        PluginXHasNoExecutable = "Plugin '{0}' has no executable for this operating system.";
        PluginXFailed = "Plugin '{0}' failed.";
        PluginXReportedError = "Plugin '{0}' reported an error.";
        PluginXDone = "{0} done";
        PluginXReturnedUnparsableSubtitle = "Plugin '{0}' returned a subtitle that could not be parsed.";
        LoadingPluginList = "Loading plugin list...";
        NoPluginsAvailable = "No plugins available.";
        XPluginsAvailable = "{0} plugins available.";
        CouldNotLoadPluginList = "Could not load the plugin list: {0}";
        DownloadingXPercent = "Downloading {0}%...";
        PluginXInstalled = "'{0}' installed.";
        CouldNotInstallPluginX = "Could not install '{0}': {1}";
        NotInstalled = "Not installed";
        Install = "Install";
        Reinstall = "Reinstall";
        Update = "Update";
        InstalledX = "Installed ({0})";
        UpdateAvailableXToY = "Update available ({0} → {1})";
        ApplyPluginToWhichLinesX = "Apply '{0}' to:";
        ApplyToSelectedLinesX = "Selected lines ({0})";
        ApplyToAllLinesX = "All lines ({0})";
    }
}
