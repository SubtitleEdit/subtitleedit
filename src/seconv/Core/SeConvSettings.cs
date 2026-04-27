using System.Text.Json;
using System.Text.Json.Serialization;
using Nikse.SubtitleEdit.Core.Common;

namespace SeConv.Core;

/// <summary>
/// Optional JSON file passed via <c>--settings:path.json</c>. Each section overlays
/// the corresponding part of libse's <c>Configuration.Settings</c>. All properties
/// are nullable — missing keys leave the libse defaults untouched.
/// </summary>
internal sealed class SeConvSettings
{
    [JsonPropertyName("general")]
    public GeneralSection? General { get; set; }

    [JsonPropertyName("tools")]
    public ToolsSection? Tools { get; set; }

    [JsonPropertyName("removeTextForHearingImpaired")]
    public RemoveHiSection? RemoveTextForHearingImpaired { get; set; }

    [JsonPropertyName("profiles")]
    public Dictionary<string, SeConvSettings>? Profiles { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static SeConvSettings Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Settings file not found: {path}", path);
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SeConvSettings>(json, JsonOptions)
            ?? throw new InvalidDataException($"Settings file is empty or null: {path}");
    }

    /// <summary>
    /// Writes any non-null values from this object into libse's global Configuration.Settings.
    /// When <paramref name="profileName"/> is provided, the named profile under <c>profiles.&lt;name&gt;</c>
    /// is overlaid on top of the base sections. Must be called before any conversion runs.
    /// </summary>
    public void ApplyToLibSe(string? profileName = null)
    {
        ApplyBaseSections();

        if (!string.IsNullOrWhiteSpace(profileName))
        {
            if (Profiles is null || !Profiles.TryGetValue(profileName, out var profile))
            {
                var available = Profiles is null ? "(none defined)" : string.Join(", ", Profiles.Keys);
                throw new InvalidOperationException($"Profile '{profileName}' not found in settings. Available: {available}");
            }
            profile.ApplyBaseSections();
        }
    }

    private void ApplyBaseSections()
    {
        var s = Configuration.Settings;

        if (General is { } g)
        {
            if (g.SubtitleLineMaximumLength.HasValue)
                s.General.SubtitleLineMaximumLength = g.SubtitleLineMaximumLength.Value;
            if (g.SubtitleMinimumDisplayMilliseconds.HasValue)
                s.General.SubtitleMinimumDisplayMilliseconds = g.SubtitleMinimumDisplayMilliseconds.Value;
            if (g.SubtitleMaximumDisplayMilliseconds.HasValue)
                s.General.SubtitleMaximumDisplayMilliseconds = g.SubtitleMaximumDisplayMilliseconds.Value;
            if (g.CurrentFrameRate.HasValue)
                s.General.CurrentFrameRate = g.CurrentFrameRate.Value;
            if (g.DefaultFrameRate.HasValue)
                s.General.DefaultFrameRate = g.DefaultFrameRate.Value;
        }

        if (Tools is { } t)
        {
            if (t.MergeShortLinesMaxGap.HasValue)
                s.Tools.MergeShortLinesMaxGap = t.MergeShortLinesMaxGap.Value;
            if (t.MergeShortLinesOnlyContinuous.HasValue)
                s.Tools.MergeShortLinesOnlyContinuous = t.MergeShortLinesOnlyContinuous.Value;
        }

        if (RemoveTextForHearingImpaired is { } r)
        {
            var dst = s.RemoveTextForHearingImpaired;
            if (r.RemoveTextBetweenBrackets.HasValue) dst.RemoveTextBetweenBrackets = r.RemoveTextBetweenBrackets.Value;
            if (r.RemoveTextBetweenParentheses.HasValue) dst.RemoveTextBetweenParentheses = r.RemoveTextBetweenParentheses.Value;
            if (r.RemoveTextBetweenCurlyBrackets.HasValue) dst.RemoveTextBetweenCurlyBrackets = r.RemoveTextBetweenCurlyBrackets.Value;
            if (r.RemoveTextBetweenQuestionMarks.HasValue) dst.RemoveTextBetweenQuestionMarks = r.RemoveTextBetweenQuestionMarks.Value;
            if (r.RemoveTextBetweenCustom.HasValue) dst.RemoveTextBetweenCustom = r.RemoveTextBetweenCustom.Value;
            if (r.RemoveTextBetweenCustomBefore is not null) dst.RemoveTextBetweenCustomBefore = r.RemoveTextBetweenCustomBefore;
            if (r.RemoveTextBetweenCustomAfter is not null) dst.RemoveTextBetweenCustomAfter = r.RemoveTextBetweenCustomAfter;
            if (r.RemoveTextBetweenOnlySeparateLines.HasValue) dst.RemoveTextBetweenOnlySeparateLines = r.RemoveTextBetweenOnlySeparateLines.Value;
            if (r.RemoveTextBeforeColon.HasValue) dst.RemoveTextBeforeColon = r.RemoveTextBeforeColon.Value;
            if (r.RemoveTextBeforeColonOnlyIfUppercase.HasValue) dst.RemoveTextBeforeColonOnlyIfUppercase = r.RemoveTextBeforeColonOnlyIfUppercase.Value;
            if (r.RemoveTextBeforeColonOnlyOnSeparateLine.HasValue) dst.RemoveTextBeforeColonOnlyOnSeparateLine = r.RemoveTextBeforeColonOnlyOnSeparateLine.Value;
            if (r.RemoveInterjections.HasValue) dst.RemoveInterjections = r.RemoveInterjections.Value;
            if (r.RemoveInterjectionsOnlyOnSeparateLine.HasValue) dst.RemoveInterjectionsOnlyOnSeparateLine = r.RemoveInterjectionsOnlyOnSeparateLine.Value;
            if (r.RemoveIfContains.HasValue) dst.RemoveIfContains = r.RemoveIfContains.Value;
            if (r.RemoveIfAllUppercase.HasValue) dst.RemoveIfAllUppercase = r.RemoveIfAllUppercase.Value;
            if (r.RemoveIfContainsText is not null) dst.RemoveIfContainsText = r.RemoveIfContainsText;
            if (r.RemoveIfOnlyMusicSymbols.HasValue) dst.RemoveIfOnlyMusicSymbols = r.RemoveIfOnlyMusicSymbols.Value;
        }
    }

    internal sealed class GeneralSection
    {
        public int? SubtitleLineMaximumLength { get; set; }
        public int? SubtitleMinimumDisplayMilliseconds { get; set; }
        public int? SubtitleMaximumDisplayMilliseconds { get; set; }
        public double? CurrentFrameRate { get; set; }
        public double? DefaultFrameRate { get; set; }
    }

    internal sealed class ToolsSection
    {
        public int? MergeShortLinesMaxGap { get; set; }
        public bool? MergeShortLinesOnlyContinuous { get; set; }
    }

    internal sealed class RemoveHiSection
    {
        public bool? RemoveTextBetweenBrackets { get; set; }
        public bool? RemoveTextBetweenParentheses { get; set; }
        public bool? RemoveTextBetweenCurlyBrackets { get; set; }
        public bool? RemoveTextBetweenQuestionMarks { get; set; }
        public bool? RemoveTextBetweenCustom { get; set; }
        public string? RemoveTextBetweenCustomBefore { get; set; }
        public string? RemoveTextBetweenCustomAfter { get; set; }
        public bool? RemoveTextBetweenOnlySeparateLines { get; set; }
        public bool? RemoveTextBeforeColon { get; set; }
        public bool? RemoveTextBeforeColonOnlyIfUppercase { get; set; }
        public bool? RemoveTextBeforeColonOnlyOnSeparateLine { get; set; }
        public bool? RemoveInterjections { get; set; }
        public bool? RemoveInterjectionsOnlyOnSeparateLine { get; set; }
        public bool? RemoveIfContains { get; set; }
        public bool? RemoveIfAllUppercase { get; set; }
        public string? RemoveIfContainsText { get; set; }
        public bool? RemoveIfOnlyMusicSymbols { get; set; }
    }
}
