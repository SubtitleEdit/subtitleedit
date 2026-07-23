using System.Text.Json;
using System.Text.Json.Serialization;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using SkiaSharp;

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

    [JsonPropertyName("exportImages")]
    public ExportImagesSection? ExportImages { get; set; }

    [JsonPropertyName("profiles")]
    public Dictionary<string, SeConvSettings>? Profiles { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? UnknownMembers { get; set; }

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
    /// Serializes a complete settings file populated with the current libse defaults —
    /// every key seconv understands, with its correct (camelCase) name and default value.
    /// Backs <c>seconv dump-settings</c>, which users redirect to a file as a starting point
    /// (issue #11037): it teaches the schema, so no one has to guess that the key is
    /// <c>removeIfAllUppercase</c> and not the GUI's <c>IsRemoveTextUppercaseLineOn</c>.
    /// <para>Built by mirroring <see cref="Configuration.Settings"/> (the same source
    /// <see cref="ApplyBaseSections"/> writes back into) and a fresh <see cref="ImageExportStyle"/>,
    /// so the output round-trips through <see cref="Load"/> with zero unknown keys — the dump
    /// can never drift from what the loader accepts.</para>
    /// </summary>
    public static string DumpDefaults()
    {
        var s = Configuration.Settings;
        var g = s.General;
        var hi = s.RemoveTextForHearingImpaired;
        var style = new ImageExportStyle();

        var defaults = new SeConvSettings
        {
            General = new GeneralSection
            {
                SubtitleLineMaximumLength = g.SubtitleLineMaximumLength,
                SubtitleMinimumDisplayMilliseconds = g.SubtitleMinimumDisplayMilliseconds,
                SubtitleMaximumDisplayMilliseconds = g.SubtitleMaximumDisplayMilliseconds,
                CurrentFrameRate = g.CurrentFrameRate,
                DefaultFrameRate = g.DefaultFrameRate,
                MinimumMillisecondsBetweenLines = g.MinimumMillisecondsBetweenLines,
                MaxNumberOfLines = g.MaxNumberOfLines,
                MergeLinesShorterThan = g.MergeLinesShorterThan,
                SubtitleMaximumCharactersPerSeconds = g.SubtitleMaximumCharactersPerSeconds,
                SubtitleOptimalCharactersPerSeconds = g.SubtitleOptimalCharactersPerSeconds,
                SubtitleMaximumWordsPerMinute = g.SubtitleMaximumWordsPerMinute,
                DialogStyle = g.DialogStyle.ToString(),
                ContinuationStyle = g.ContinuationStyle.ToString(),
            },
            Tools = new ToolsSection
            {
                MergeShortLinesMaxGap = s.Tools.MergeShortLinesMaxGap,
                MergeShortLinesOnlyContinuous = s.Tools.MergeShortLinesOnlyContinuous,
            },
            RemoveTextForHearingImpaired = new RemoveHiSection
            {
                RemoveTextBetweenBrackets = hi.RemoveTextBetweenBrackets,
                RemoveTextBetweenParentheses = hi.RemoveTextBetweenParentheses,
                RemoveTextBetweenCurlyBrackets = hi.RemoveTextBetweenCurlyBrackets,
                RemoveTextBetweenQuestionMarks = hi.RemoveTextBetweenQuestionMarks,
                RemoveTextBetweenCustom = hi.RemoveTextBetweenCustom,
                RemoveTextBetweenCustomBefore = hi.RemoveTextBetweenCustomBefore,
                RemoveTextBetweenCustomAfter = hi.RemoveTextBetweenCustomAfter,
                RemoveTextBetweenOnlySeparateLines = hi.RemoveTextBetweenOnlySeparateLines,
                RemoveTextBeforeColon = hi.RemoveTextBeforeColon,
                RemoveTextBeforeColonOnlyIfUppercase = hi.RemoveTextBeforeColonOnlyIfUppercase,
                RemoveTextBeforeColonOnlyOnSeparateLine = hi.RemoveTextBeforeColonOnlyOnSeparateLine,
                RemoveInterjections = hi.RemoveInterjections,
                RemoveInterjectionsOnlyOnSeparateLine = hi.RemoveInterjectionsOnlyOnSeparateLine,
                RemoveIfContains = hi.RemoveIfContains,
                RemoveIfAllUppercase = hi.RemoveIfAllUppercase,
                RemoveIfContainsText = hi.RemoveIfContainsText,
                RemoveIfOnlyMusicSymbols = hi.RemoveIfOnlyMusicSymbols,
            },
            ExportImages = new ExportImagesSection
            {
                FontName = style.FontName,
                FontSize = style.FontSize,
                FontColor = ToHex(style.FontColor),
                IsBold = style.IsBold,
                OutlineColor = ToHex(style.OutlineColor),
                OutlineWidth = style.OutlineWidth,
                ShadowColor = ToHex(style.ShadowColor),
                ShadowWidth = style.ShadowWidth,
                BackgroundColor = ToHex(style.BackgroundColor),
                BackgroundCornerRadius = style.BackgroundCornerRadius,
                // EffectiveBoxType resolves the null "auto" into the concrete default (none),
                // so the dumped file shows a real, editable value rather than a blank.
                BoxType = style.EffectiveBoxType.ToString(),
                BoxPaddingLeft = style.BoxPaddingLeft,
                BoxPaddingRight = style.BoxPaddingRight,
                BoxPaddingTop = style.BoxPaddingTop,
                BoxPaddingBottom = style.BoxPaddingBottom,
                LineSpacingPercent = style.LineSpacingPercent,
                Alignment = style.Alignment.ToString(),
                ContentAlignment = style.ContentAlignment.ToString(),
                // BottomTopMargin / LeftRightMargin left unset: their default is "5% of the
                // screen", not a fixed pixel count, so there is no honest number to emit.
            },
        };

        var options = new JsonSerializerOptions(JsonOptions)
        {
            WriteIndented = true,
            // Omit the null "auto" values (margins) rather than writing `null`, so every key
            // in the file is a concrete, copy-paste-ready default.
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // Print non-ASCII (e.g. the ¶ HI separator) literally instead of as ¶ — this
            // is a file for humans to read and edit. Safe here: the output is written to a file,
            // not embedded in HTML.
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        return JsonSerializer.Serialize(defaults, options);
    }

    // "#AARRGGBB" — the fully-qualified form ExportImagesSection.ApplyTo parses back exactly.
    private static string ToHex(SKColor c) => $"#{c.Alpha:X2}{c.Red:X2}{c.Green:X2}{c.Blue:X2}";

    /// <summary>
    /// Keys in the JSON that match no known section or property - typos, or keys from a newer
    /// seconv than the one running (issue #12437). System.Text.Json drops these silently, so a
    /// settings file that does nothing looks exactly like one that works. Callers report these
    /// as a warning rather than an error, so a file written for a newer seconv still applies
    /// everything else it understands.
    /// </summary>
    public IEnumerable<string> GetUnknownKeys() => CollectUnknownKeys(string.Empty);

    private IEnumerable<string> CollectUnknownKeys(string prefix)
    {
        foreach (var key in Prefixed(UnknownMembers, prefix))
        {
            yield return key;
        }

        foreach (var key in Prefixed(General?.UnknownMembers, prefix + "general."))
        {
            yield return key;
        }

        foreach (var key in Prefixed(Tools?.UnknownMembers, prefix + "tools."))
        {
            yield return key;
        }

        foreach (var key in Prefixed(RemoveTextForHearingImpaired?.UnknownMembers, prefix + "removeTextForHearingImpaired."))
        {
            yield return key;
        }

        foreach (var key in Prefixed(ExportImages?.UnknownMembers, prefix + "exportImages."))
        {
            yield return key;
        }

        if (Profiles is null)
        {
            yield break;
        }

        foreach (var (name, profile) in Profiles)
        {
            foreach (var key in profile.CollectUnknownKeys($"{prefix}profiles.{name}."))
            {
                yield return key;
            }
        }
    }

    private static IEnumerable<string> Prefixed(Dictionary<string, JsonElement>? unknown, string prefix)
    {
        return unknown is null
            ? Enumerable.Empty<string>()
            : unknown.Keys.Select(key => prefix + key);
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

    /// <summary>
    /// Overlays the <c>exportImages</c> section (base first, then the named profile's, if any)
    /// onto <paramref name="style"/>. Unlike the other sections this does not go through
    /// libse's Configuration — image rendering is driven by <see cref="ImageExportStyle"/>
    /// on <c>ConversionOptions</c>. Call after <see cref="ApplyToLibSe"/>, which validates
    /// the profile name.
    /// </summary>
    public void ApplyExportImages(ImageExportStyle style, string? profileName = null)
    {
        ExportImages?.ApplyTo(style);

        if (!string.IsNullOrWhiteSpace(profileName) &&
            Profiles is not null &&
            Profiles.TryGetValue(profileName, out var profile))
        {
            profile.ExportImages?.ApplyTo(style);
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

            if (g.MinimumMillisecondsBetweenLines.HasValue)
                s.General.MinimumMillisecondsBetweenLines = g.MinimumMillisecondsBetweenLines.Value;
            if (g.MaxNumberOfLines.HasValue)
                s.General.MaxNumberOfLines = g.MaxNumberOfLines.Value;
            if (g.MergeLinesShorterThan.HasValue)
                s.General.MergeLinesShorterThan = g.MergeLinesShorterThan.Value;
            if (g.SubtitleMaximumCharactersPerSeconds.HasValue)
                s.General.SubtitleMaximumCharactersPerSeconds = g.SubtitleMaximumCharactersPerSeconds.Value;
            if (g.SubtitleOptimalCharactersPerSeconds.HasValue)
                s.General.SubtitleOptimalCharactersPerSeconds = g.SubtitleOptimalCharactersPerSeconds.Value;
            if (g.SubtitleMaximumWordsPerMinute.HasValue)
                s.General.SubtitleMaximumWordsPerMinute = g.SubtitleMaximumWordsPerMinute.Value;

            // Enums are carried as strings; parse case-insensitively and ignore bad values
            // so a typo doesn't abort the whole settings load.
            if (!string.IsNullOrWhiteSpace(g.DialogStyle)
                && Enum.TryParse<DialogType>(g.DialogStyle, ignoreCase: true, out var dialogStyle))
                s.General.DialogStyle = dialogStyle;
            if (!string.IsNullOrWhiteSpace(g.ContinuationStyle)
                && Enum.TryParse<ContinuationStyle>(g.ContinuationStyle, ignoreCase: true, out var continuationStyle))
                s.General.ContinuationStyle = continuationStyle;
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

        // FixCommonErrors / split / merge profile values (issue #11874). These let a
        // --settings JSON reproduce an SE4 profile so batch output matches.
        public int? MinimumMillisecondsBetweenLines { get; set; }
        public int? MaxNumberOfLines { get; set; }
        public int? MergeLinesShorterThan { get; set; }
        public double? SubtitleMaximumCharactersPerSeconds { get; set; }
        public double? SubtitleOptimalCharactersPerSeconds { get; set; }
        public double? SubtitleMaximumWordsPerMinute { get; set; }

        // Enum-valued; carried as strings and parsed case-insensitively (see ApplyBaseSections).
        // DialogStyle: DialogType, e.g. "DashBothLinesWithSpace".
        // ContinuationStyle: ContinuationStyle, e.g. "NoneTrailingDots".
        public string? DialogStyle { get; set; }
        public string? ContinuationStyle { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? UnknownMembers { get; set; }
    }

    internal sealed class ToolsSection
    {
        public int? MergeShortLinesMaxGap { get; set; }
        public bool? MergeShortLinesOnlyContinuous { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? UnknownMembers { get; set; }
    }

    /// <summary>
    /// Styling for text → image output (Blu-Ray sup, VobSub, BDN-XML, ...). Colours are
    /// hex strings (<c>#AARRGGBB</c> / <c>#RRGGBB</c>) or SkiaSharp colour names; enums are
    /// carried as strings (<c>boxType</c>: none | one-box | box-per-line; <c>alignment</c>:
    /// e.g. bottom-center; <c>contentAlignment</c>: left | center | right). Following the
    /// convention above, unparsable values are ignored rather than aborting the load.
    /// </summary>
    internal sealed class ExportImagesSection
    {
        public string? FontName { get; set; }
        public float? FontSize { get; set; }
        public string? FontColor { get; set; }
        public bool? IsBold { get; set; }
        public string? OutlineColor { get; set; }
        public double? OutlineWidth { get; set; }
        public string? ShadowColor { get; set; }
        public double? ShadowWidth { get; set; }
        public string? BackgroundColor { get; set; }
        public double? BackgroundCornerRadius { get; set; }
        public string? BoxType { get; set; }
        public int? BoxPaddingLeft { get; set; }
        public int? BoxPaddingRight { get; set; }
        public int? BoxPaddingTop { get; set; }
        public int? BoxPaddingBottom { get; set; }
        public int? LineSpacingPercent { get; set; }
        public string? Alignment { get; set; }
        public string? ContentAlignment { get; set; }
        public int? BottomTopMargin { get; set; }
        public int? LeftRightMargin { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? UnknownMembers { get; set; }

        public void ApplyTo(ImageExportStyle style)
        {
            if (!string.IsNullOrWhiteSpace(FontName))
                style.FontName = FontName;
            if (FontSize.HasValue)
                style.FontSize = FontSize.Value;
            if (!string.IsNullOrWhiteSpace(FontColor) && ImageExportStyle.TryParseColor(FontColor, out var fontColor))
                style.FontColor = fontColor;
            if (IsBold.HasValue)
                style.IsBold = IsBold.Value;
            if (!string.IsNullOrWhiteSpace(OutlineColor) && ImageExportStyle.TryParseColor(OutlineColor, out var outlineColor))
                style.OutlineColor = outlineColor;
            if (OutlineWidth.HasValue)
                style.OutlineWidth = OutlineWidth.Value;
            if (!string.IsNullOrWhiteSpace(ShadowColor) && ImageExportStyle.TryParseColor(ShadowColor, out var shadowColor))
                style.ShadowColor = shadowColor;
            if (ShadowWidth.HasValue)
                style.ShadowWidth = ShadowWidth.Value;
            if (!string.IsNullOrWhiteSpace(BackgroundColor) && ImageExportStyle.TryParseColor(BackgroundColor, out var backgroundColor))
                style.BackgroundColor = backgroundColor;
            if (BackgroundCornerRadius.HasValue)
                style.BackgroundCornerRadius = BackgroundCornerRadius.Value;
            if (!string.IsNullOrWhiteSpace(BoxType) && ImageExportStyle.TryParseBoxType(BoxType, out var boxType))
                style.BoxType = boxType;
            if (BoxPaddingLeft.HasValue)
                style.BoxPaddingLeft = BoxPaddingLeft.Value;
            if (BoxPaddingRight.HasValue)
                style.BoxPaddingRight = BoxPaddingRight.Value;
            if (BoxPaddingTop.HasValue)
                style.BoxPaddingTop = BoxPaddingTop.Value;
            if (BoxPaddingBottom.HasValue)
                style.BoxPaddingBottom = BoxPaddingBottom.Value;
            if (LineSpacingPercent.HasValue)
                style.LineSpacingPercent = LineSpacingPercent.Value;
            if (!string.IsNullOrWhiteSpace(Alignment) && ImageExportStyle.TryParseAlignment(Alignment, out var alignment))
                style.Alignment = alignment;
            if (!string.IsNullOrWhiteSpace(ContentAlignment) && ImageExportStyle.TryParseContentAlignment(ContentAlignment, out var contentAlignment))
                style.ContentAlignment = contentAlignment;
            if (BottomTopMargin.HasValue)
                style.BottomTopMargin = BottomTopMargin.Value;
            if (LeftRightMargin.HasValue)
                style.LeftRightMargin = LeftRightMargin.Value;
        }
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

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? UnknownMembers { get; set; }
    }
}
