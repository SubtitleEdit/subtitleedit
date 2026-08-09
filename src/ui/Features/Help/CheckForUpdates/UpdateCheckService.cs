using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

public interface IUpdateCheckService
{
    Task<UpdateCheckResult> CheckForUpdates();
}

public class UpdateCheckResult
{
    public string ChangeLogText { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public bool IsNewVersionAvailable { get; set; }
}

public class UpdateCheckService : IUpdateCheckService
{
    public const string ChannelStable = "Stable";
    public const string ChannelBeta = "Beta";

    private const string ChangeLogUrl1 = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/refs/heads/main/change-log.txt";
    private const string ChangeLogUrl2 = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/refs/heads/main/Changelog.txt";
    private const string ChangeLogUrl3 = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/refs/heads/main/ChangeLog.txt";
    private static readonly Regex UnreleasedChangeLogRegex = new(@"(x(th|st) \w+ \d+|TBD)", RegexOptions.Compiled);

    private readonly HttpClient _httpClient;

    public UpdateCheckService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    /// <summary>
    /// Flatpak (and other store-managed) installs get their updates through the store,
    /// so the startup check and its setting are suppressed there.
    /// </summary>
    public static bool IsStoreManagedInstall =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FLATPAK_ID"));

    public static bool IncludePrereleases()
    {
        return ResolveIncludePrereleases(Se.Settings.General.CheckForUpdatesChannel, Se.Version);
    }

    public static bool ResolveIncludePrereleases(string channel, string currentVersion)
    {
        if (channel == ChannelStable)
        {
            return false;
        }

        if (channel == ChannelBeta)
        {
            return true;
        }

        // Empty = the user never picked a channel: people already running a
        // beta/rc build get pre-release updates, stable users stable only.
        return IsPrerelease(currentVersion);
    }

    public static bool IsPrerelease(string version)
    {
        try
        {
            return !string.IsNullOrEmpty(new SemanticVersion(version).PreRelease);
        }
        catch (ArgumentException)
        {
            return version.Contains('-');
        }
    }

    public async Task<UpdateCheckResult> CheckForUpdates()
    {
        string content;
        try
        {
            content = await _httpClient.GetStringAsync(ChangeLogUrl1);
        }
        catch
        {
            try
            {
                content = await _httpClient.GetStringAsync(ChangeLogUrl2);
            }
            catch
            {
                content = await _httpClient.GetStringAsync(ChangeLogUrl3);
            }
        }

        var includePrereleases = IncludePrereleases();
        var changeLogText = ParseLatestChangeLog(content, includePrereleases);
        var latestVersion = ParseLatestVersion(changeLogText);

        return new UpdateCheckResult
        {
            ChangeLogText = changeLogText,
            LatestVersion = latestVersion,
            IsNewVersionAvailable = !string.IsNullOrEmpty(latestVersion) &&
                                    IsNewerThanCurrent(latestVersion) &&
                                    (includePrereleases || !IsPrerelease(latestVersion)),
        };
    }

    public static bool IsNewerThanCurrent(string latestVersion)
    {
        return IsNewerThan(latestVersion, Se.Version);
    }

    public static bool IsNewerThan(string latestVersion, string currentVersion)
    {
        try
        {
            return new SemanticVersion(latestVersion).IsGreaterThan(new SemanticVersion(currentVersion));
        }
        catch (ArgumentException)
        {
            // unparsable version - only offer the download when it differs from what we run
            return !string.Equals(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static string ParseLatestVersion(string changeLogContent)
    {
        var match = Regex.Match(changeLogContent, @"^(v[\d.]+(?:-\w+)?)\s*\(", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    public static string ParseLatestChangeLog(string changeLogContent, bool includePrereleases)
    {
        const string releaseSeparator = "-----------------------------------------------------------------------------------------------------";
        foreach (var block in changeLogContent.Split(releaseSeparator))
        {
            var changeLog = block.Trim();
            if (changeLog.Length == 0)
            {
                continue;
            }

            var version = ParseLatestVersion(changeLog);
            if (string.IsNullOrEmpty(version) || // file title or other block without a version header
                UnreleasedChangeLogRegex.IsMatch(changeLog)) // unreleased draft entry
            {
                continue;
            }

            if (!includePrereleases && IsPrerelease(version))
            {
                continue;
            }

            return changeLog;
        }

        return changeLogContent.Trim();
    }
}
