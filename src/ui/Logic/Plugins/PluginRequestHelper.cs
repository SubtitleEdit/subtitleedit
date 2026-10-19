using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Nikse.SubtitleEdit.Logic.Plugins;

/// <summary>
/// Request/response plumbing shared by the main window (Plugins menu) and Batch convert.
/// </summary>
public static class PluginRequestHelper
{
    /// <summary>
    /// A request with everything that does not depend on a loaded subtitle or video: UI,
    /// theme, rules, data folder and the plugin's persisted settings.
    /// </summary>
    public static PluginRequest CreateRequest(InstalledPlugin plugin, string requestType)
    {
        var (settings, settingsVersion) = LoadSettings(plugin);
        return new PluginRequest
        {
            RequestType = requestType,
            PluginDataDirectory = GetPluginDataDirectory(plugin),
            FrameRate = Se.Settings.General.CurrentFrameRate,
            UiLanguage = Se.Settings.General.Language ?? string.Empty,
            Theme = UiTheme.ThemeName,
            ThemeColors = PluginThemeColorsFactory.Build(),
            SeVersion = Se.Version,
            Rules = BuildRules(),
            Settings = settings,
            SettingsVersion = settingsVersion,
        };
    }

    /// <summary>The subtitle as sent to a plugin, written in <paramref name="format"/>.</summary>
    public static PluginSubtitle ToPluginSubtitle(Subtitle subtitle, SubtitleFormat format, string fileName)
    {
        return new PluginSubtitle
        {
            Format = format.Name,
            FileName = fileName,
            // A binary format (EBU STL, PAC, ...) has no text form - Ebu.ToText returns
            // "Not supported!" - so plugins get its header and paragraphs only.
            Native = format.IsTextBased ? subtitle.ToText(format) : string.Empty,
            SubRip = subtitle.ToText(new SubRip()),
            Header = subtitle.Header,
            Paragraphs = subtitle.Paragraphs.Select(ToPluginParagraph).ToList(),
        };
    }

    public static (JsonElement? Settings, int? SettingsVersion) LoadSettings(InstalledPlugin plugin)
    {
        JsonElement? settings = null;
        if (Se.Settings.Plugins.Settings.TryGetValue(plugin.Manifest.Name, out var settingsJson) &&
            !string.IsNullOrWhiteSpace(settingsJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(settingsJson);
                settings = doc.RootElement.Clone();
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Invalid stored plugin settings for " + plugin.Manifest.Name);
            }
        }

        int? settingsVersion = null;
        if (Se.Settings.Plugins.SettingsVersions.TryGetValue(plugin.Manifest.Name, out var storedVersion))
        {
            settingsVersion = storedVersion;
        }

        return (settings, settingsVersion);
    }

    public static void SaveSettings(InstalledPlugin plugin, PluginResponse response)
    {
        if (!response.Settings.HasValue)
        {
            return;
        }

        try
        {
            Se.Settings.Plugins.Settings[plugin.Manifest.Name] = response.Settings.Value.GetRawText();
            if (response.SettingsVersion.HasValue)
            {
                Se.Settings.Plugins.SettingsVersions[plugin.Manifest.Name] = response.SettingsVersion.Value;
            }
            else
            {
                Se.Settings.Plugins.SettingsVersions.Remove(plugin.Manifest.Name);
            }
            Se.SaveSettings();
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to persist plugin settings for " + plugin.Manifest.Name);
        }
    }

    // Persistent, writable folder private to a plugin, keyed by its (sanitized) manifest name -
    // the same identity used to persist plugin settings. Lives under Plugins/Data so it survives
    // plugin updates (which replace the install folder). Created here so the plugin can rely on it.
    public static string GetPluginDataDirectory(InstalledPlugin plugin)
    {
        var name = plugin.Manifest.Name;
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        var path = Path.Combine(Se.PluginsDataFolder, name);
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Could not create plugin data directory: " + path);
        }

        return path;
    }

    public static PluginRules BuildRules()
    {
        var general = Se.Settings.General;
        return new PluginRules
        {
            SubtitleMinimumDisplayMilliseconds = general.SubtitleMinimumDisplayMilliseconds,
            SubtitleMaximumDisplayMilliseconds = general.SubtitleMaximumDisplayMilliseconds,
            SubtitleMaximumCharactersPerSeconds = general.SubtitleMaximumCharactersPerSeconds,
            SubtitleOptimalCharactersPerSeconds = general.SubtitleOptimalCharactersPerSeconds,
            SubtitleLineMaximumLength = general.SubtitleLineMaximumLength,
            MaxNumberOfLines = general.MaxNumberOfLines,
            MinimumMillisecondsBetweenLines = general.MinimumBetweenLines.GetMilliseconds(),
            MinimumFramesBetweenLines = general.MinimumBetweenLines.Frames,
            UseFrameMode = general.UseFrameMode,
            EbuStlTeletextUseBox = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox,
            EbuStlTeletextUseDoubleHeight = Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight,
            VideoOffsetMs = general.CurrentVideoOffsetInMs,
        };
    }

    public static PluginParagraph ToPluginParagraph(Paragraph p)
    {
        return new PluginParagraph
        {
            StartMs = p.StartTime.TotalMilliseconds,
            EndMs = p.EndTime.TotalMilliseconds,
            Text = p.Text ?? string.Empty,
            Style = p.Style,
            Actor = p.Actor,
            Language = p.Language,
            Region = p.Region,
            Effect = p.Effect,
            Extra = p.Extra,
            MarginL = p.MarginL,
            MarginR = p.MarginR,
            MarginV = p.MarginV,
            Layer = p.Layer,
            IsComment = p.IsComment,
            Forced = p.Forced,
            NewSection = p.NewSection,
            Bookmark = p.Bookmark,
        };
    }

    public static Paragraph FromPluginParagraph(PluginParagraph p)
    {
        return new Paragraph(new TimeCode(p.StartMs), new TimeCode(p.EndMs), p.Text ?? string.Empty)
        {
            Style = p.Style,
            Actor = p.Actor,
            Language = p.Language,
            Region = p.Region,
            Effect = p.Effect,
            Extra = p.Extra,
            MarginL = p.MarginL,
            MarginR = p.MarginR,
            MarginV = p.MarginV,
            Layer = p.Layer,
            IsComment = p.IsComment,
            Forced = p.Forced,
            NewSection = p.NewSection,
            Bookmark = p.Bookmark,
        };
    }

    public static bool HasSubtitle(PluginSubtitle? subtitle)
    {
        return subtitle != null && (subtitle.Paragraphs is { Count: > 0 } || !string.IsNullOrWhiteSpace(subtitle.Native));
    }

    /// <summary>
    /// The subtitle a plugin returned - from its paragraphs when present (lossless), else parsed
    /// from <see cref="PluginSubtitle.Native"/>. Empty when nothing could be read. The header is
    /// not applied; callers decide what to do with <see cref="PluginSubtitle.Header"/>.
    /// </summary>
    public static Subtitle ReadSubtitle(PluginSubtitle pluginSubtitle)
    {
        var subtitle = new Subtitle();
        if (pluginSubtitle.Paragraphs is { Count: > 0 })
        {
            // Lossless path: the lines come back exactly as SE holds them, so the current format
            // (and for EBU STL the teletext rows and GSI header) survive - no re-parse involved.
            subtitle.Paragraphs.AddRange(pluginSubtitle.Paragraphs.Select(FromPluginParagraph));
            subtitle.Renumber();
            return subtitle;
        }

        var lines = pluginSubtitle.Native.SplitToLines().ToList();
        var format = SubtitleFormat.AllSubtitleFormats
            .FirstOrDefault(f => f.Name.Equals(pluginSubtitle.Format, StringComparison.OrdinalIgnoreCase));

        var loadedFormat = subtitle.ReloadLoadSubtitle(lines, string.Empty, format);
        if (loadedFormat == null)
        {
            subtitle.Paragraphs.Clear();
        }

        return subtitle;
    }
}
