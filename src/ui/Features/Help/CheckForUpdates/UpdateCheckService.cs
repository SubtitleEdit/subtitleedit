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

    private static readonly string[] ChangeLogUrls =
    {
        "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/refs/heads/main/change-log.txt",
        // SE 4 changelog file names, kept as fallbacks for compatibility.
        "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/refs/heads/main/Changelog.txt",
        "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/refs/heads/main/ChangeLog.txt",
    };

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

    /// <summary>Pause between retry rounds; tests set this to zero.</summary>
    internal TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

    public async Task<UpdateCheckResult> CheckForUpdates()
    {
        var content = await DownloadChangeLogAsync();

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

    /// <summary>
    /// Downloads the changelog, retrying once more after a short pause. File downloads
    /// (DownloadHelper) retry transient failures, which is what keeps them working behind
    /// flaky corporate proxies - the update check gets the same second chance instead of
    /// giving up on the first dropped connection. The definitive failure is logged so
    /// proxy users find the actual exception in error-log.txt.
    /// </summary>
    private async Task<string> DownloadChangeLogAsync()
    {
        const int maxAttempts = 2;
        Exception lastException = new HttpRequestException("Update check failed");

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            foreach (var url in ChangeLogUrls)
            {
                try
                {
                    return await _httpClient.GetStringAsync(url);
                }
                catch (Exception exception)
                {
                    lastException = exception;
                }
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(RetryDelay);
            }
        }

        Se.LogError(lastException, "Update check could not download the changelog");
        throw lastException;
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
