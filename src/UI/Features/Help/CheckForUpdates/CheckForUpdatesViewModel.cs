using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

public partial class CheckForUpdatesViewModel : ObservableObject
{
    private const string ChangeLogUrl = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/refs/heads/main/ChangeLog.txt";
    private const string ReleasesUrl = "https://github.com/SubtitleEdit/subtitleedit/releases";
    private static readonly Regex UnreleasedChangeLogRegex = new(@"(x(th|st) \w+ \d+)", RegexOptions.Compiled);

    public Window? Window { get; set; }

    [ObservableProperty] private string _statusText = string.Empty;

    [ObservableProperty] private string _changeLogText = string.Empty;

    [ObservableProperty] private bool _isDownloadLinkVisible;

    public async Task CheckForUpdates()
    {
        StatusText = Se.Language.Help.CheckForUpdatesChecking;
        IsDownloadLinkVisible = false;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            var content = await httpClient.GetStringAsync(ChangeLogUrl);

            ChangeLogText = ParseLatestChangeLog(content);
            var latestVersion = ParseLatestVersion(ChangeLogText);

            if (string.IsNullOrEmpty(latestVersion))
            {
                StatusText = Se.Language.Help.CheckForUpdatesUnableToCheck;
                return;
            }

            if (string.Equals(latestVersion, Se.Version, StringComparison.OrdinalIgnoreCase))
            {
                StatusText = Se.Language.Help.CheckForUpdatesUpToDate;
            }
            else
            {
                StatusText = string.Format(Se.Language.Help.CheckForUpdatesNewVersionAvailable, latestVersion);
                IsDownloadLinkVisible = true;
            }
        }
        catch
        {
            StatusText = Se.Language.Help.CheckForUpdatesUnableToCheck;
        }
    }

    private static string ParseLatestVersion(string changeLogContent)
    {
        var match = Regex.Match(changeLogContent, @"^(v[\d.]+(?:-\w+)?)\s*\(", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string ParseLatestChangeLog(string changeLogContent)
    {
        const string releaseSeparator = "-----------------------------------------------------------------------------------------------------";
        var separatorIndex = changeLogContent.IndexOf(releaseSeparator, StringComparison.Ordinal);
        if (separatorIndex > 0)
        {
            var changeLog = changeLogContent[..separatorIndex].Trim();
            if (!UnreleasedChangeLogRegex.IsMatch(changeLog))
            {
                return changeLog;
            }

            // get next change log which is current released version
            var releaseChangeLogIndex = changeLogContent.IndexOf(releaseSeparator, separatorIndex + releaseSeparator.Length, StringComparison.Ordinal);
            if (releaseChangeLogIndex > 0)
            {
                return changeLogContent[(separatorIndex + releaseSeparator.Length) ..releaseChangeLogIndex].Trim();
            }
        }

        return changeLogContent.Trim();
    }

    [RelayCommand]
    private async Task OpenDownloadPage()
    {
        if (Window == null)
        {
            return;
        }

        UiUtil.OpenUrl(ReleasesUrl);
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}