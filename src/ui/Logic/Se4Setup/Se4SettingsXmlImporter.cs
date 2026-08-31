using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Logic.Se4Setup;

/// <summary>
/// Reads an SE 4 <c>Settings.xml</c> for the Settings import dialog, which otherwise only accepts
/// an SE 5 <c>Settings.json</c> export (#14309 - users coming from 4.0.12 have nothing else).
///
/// Only the categories the dialog offers are mapped: rules, syntax coloring, waveform, shortcuts,
/// appearance and auto-translate. Everything is applied field by field onto the *current* settings
/// rather than by replacing a whole section: SE 4's tree is not SE 5's, so an unmapped SE 5 field
/// must keep its current value instead of being reset to a default (which is what a section-level
/// assignment - the JSON import path - would do).
/// </summary>
public static class Se4SettingsXmlImporter
{
    public sealed class Se4SettingsFile
    {
        internal XElement? General { get; init; }
        internal XElement? Tools { get; init; }
        internal XElement? VideoControls { get; init; }
        internal XElement? Shortcuts { get; init; }
        internal string Xml { get; init; } = string.Empty;

        public bool HasRules => General != null;
        public bool HasSyntaxColoring => Tools != null || General != null;
        public bool HasWaveform => VideoControls != null;
        public bool HasAppearance => General != null;
        public bool HasAutoTranslate => Tools != null;
        public bool HasShortcuts => Shortcuts != null;
    }

    /// <summary>
    /// Cheap check on the file *content* - the import dialog takes both .json and .xml, and a
    /// user pointing at an SE 4 file that has been renamed should still get the XML path.
    /// </summary>
    public static bool LooksLikeXml(string text)
    {
        return text.TrimStart('﻿', ' ', '\t', '\r', '\n').StartsWith('<');
    }

    /// <summary>
    /// Returns null when the text is not an SE 4 <c>Settings.xml</c> (unparseable, or a different
    /// SE 4 XML such as an exported replace list or shortcut file).
    /// </summary>
    public static Se4SettingsFile? Parse(string xml)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch
        {
            return null;
        }

        var root = doc.Root;
        if (root == null || root.Name.LocalName != "Settings")
        {
            return null;
        }

        var file = new Se4SettingsFile
        {
            Xml = xml,
            General = Section(root, "General"),
            Tools = Section(root, "Tools"),
            VideoControls = Section(root, "VideoControls"),
            Shortcuts = Section(root, "Shortcuts"),
        };

        return file.HasRules || file.HasWaveform || file.HasShortcuts || file.HasAutoTranslate
            ? file
            : null;
    }

    private static XElement? Section(XElement root, string name)
    {
        var section = root.Elements().FirstOrDefault(e => e.Name.LocalName == name);
        return section != null && section.HasElements ? section : null;
    }

    /// <summary>
    /// The "Rules" half of SE 4's General settings - the same values SE 5 keeps in
    /// <see cref="SeGeneral"/> and the Settings window's Rules page shows.
    /// </summary>
    public static void ApplyRules(Se4SettingsFile file)
    {
        var general = file.General;
        if (general == null)
        {
            return;
        }

        var g = Se.Settings.General;

        SetInt(general, "SubtitleLineMaximumLength", 1, 500, v => g.SubtitleLineMaximumLength = v);
        SetInt(general, "MaxNumberOfLines", 1, 20, v => g.MaxNumberOfLines = v);
        SetInt(general, "MaxNumberOfLinesPlusAbort", 0, 100, v => g.MaxNumberOfLinesPlusAbort = v);
        SetInt(general, "MergeLinesShorterThan", 0, 500, v => g.UnbreakLinesShorterThan = v);
        SetInt(general, "SubtitleMinimumDisplayMilliseconds", 0, 100000, v => g.SubtitleMinimumDisplayMilliseconds = v);
        SetInt(general, "SubtitleMaximumDisplayMilliseconds", 0, 500000, v => g.SubtitleMaximumDisplayMilliseconds = v);
        SetInt(general, "SubtitleLineMaximumPixelWidth", 0, 10000, v => g.SubtitleLineMaximumPixelWidth = v);

        // SE 5 keeps the minimum gap as ms *and* frames; SE 4 only has the ms value, so leave the
        // frame count alone rather than deriving one from a frame rate the file may not carry.
        SetInt(general, "MinimumMillisecondsBetweenLines", 0, 100000, v => g.MinimumBetweenLines.Milliseconds = v);

        SetDouble(general, "SubtitleMaximumCharactersPerSeconds", 0, 1000, v => g.SubtitleMaximumCharactersPerSeconds = v);
        SetDouble(general, "SubtitleOptimalCharactersPerSeconds", 0, 1000, v => g.SubtitleOptimalCharactersPerSeconds = v);
        SetDouble(general, "SubtitleMaximumWordsPerMinute", 0, 10000, v => g.SubtitleMaximumWordsPerMinute = v);

        // Same guard as the SE 4 setup importer: SE 4 wrote the frame rate with the invariant
        // culture but could read back a locale-mangled one ("23,976" -> 23976).
        SetDouble(general, "DefaultFrameRate", 10, 200, v => g.DefaultFrameRate = v);
        SetDouble(general, "CurrentFrameRate", 10, 200, v => g.CurrentFrameRate = v);

        // Stored as strings in SE 5 but parsed back into the libse enums (SeGeneral.ToProfile
        // throws on an unknown name), so only accept values that still exist.
        SetEnumName<DialogType>(general, "DialogStyle", v => g.DialogStyle = v);
        SetEnumName<ContinuationStyle>(general, "ContinuationStyle", v => g.ContinuationStyle = v);

        var cpsStrategy = Value(general, "CpsLineLengthStrategy");
        if (!string.IsNullOrWhiteSpace(cpsStrategy))
        {
            g.CpsLineLengthStrategy = cpsStrategy;
        }
    }

    /// <summary>
    /// SE 4 keeps the syntax-coloring toggles in Tools and the "too wide" measuring font in
    /// General; SE 5 has all of it on <see cref="SeGeneral"/> next to the rules it colors.
    /// </summary>
    public static void ApplySyntaxColoring(Se4SettingsFile file)
    {
        var g = Se.Settings.General;
        var tools = file.Tools;

        if (tools != null)
        {
            SetBool(tools, "ListViewSyntaxColorDurationSmall", v => g.ColorDurationTooShort = v);
            SetBool(tools, "ListViewSyntaxColorDurationBig", v => g.ColorDurationTooLong = v);
            SetBool(tools, "ListViewSyntaxColorLongLines", v => g.ColorTextTooLong = v);
            SetBool(tools, "ListViewSyntaxColorWideLines", v => g.ColorTextTooWide = v);
            SetBool(tools, "ListViewSyntaxMoreThanXLines", v => g.ColorTextTooManyLines = v);
            SetBool(tools, "ListViewSyntaxColorOverlap", v => g.ColorTimeCodeOverlap = v);
            SetBool(tools, "ListViewSyntaxColorGap", v => g.ColorGapTooShort = v);
            SetColor(tools, "ListViewSyntaxErrorColor", v => g.ErrorColor = v);
        }

        var general = file.General;
        if (general != null)
        {
            SetInt(general, "SubtitleLineMaximumPixelWidth", 0, 10000, v => g.ColorTextTooWidePixels = v);
            SetInt(general, "MeasureFontSize", 1, 500, v => g.ColorTextTooWideFontSize = v);

            var measureFontName = Value(general, "MeasureFontName");
            if (!string.IsNullOrWhiteSpace(measureFontName))
            {
                g.ColorTextTooWideFontName = measureFontName;
            }
        }
    }

    /// <summary>
    /// SE 4's waveform lives in VideoControls. Only the values with a real SE 5 counterpart are
    /// carried - SE 5's paragraph/shot-change colors and draw style have no SE 4 equivalent and
    /// keep whatever the user has now.
    /// </summary>
    public static void ApplyWaveform(Se4SettingsFile file)
    {
        var videoControls = file.VideoControls;
        if (videoControls == null)
        {
            return;
        }

        var w = Se.Settings.Waveform;

        SetColor(videoControls, "WaveformColor", v => w.WaveformColor = v);
        SetColor(videoControls, "WaveformSelectedColor", v => w.WaveformSelectedColor = v);
        SetColor(videoControls, "WaveformBackgroundColor", v => w.WaveformBackgroundColor = v);
        SetColor(videoControls, "WaveformTextColor", v => w.WaveformTextColor = v);
        SetColor(videoControls, "WaveformCursorColor", v => w.WaveformCursorColor = v);

        SetBool(videoControls, "WaveformDrawGrid", v => w.DrawGridLines = v);
        SetBool(videoControls, "WaveformDrawCps", v => w.WaveformShowCps = v);
        SetBool(videoControls, "WaveformFocusOnMouseEnter", v => w.FocusOnMouseOver = v);
        SetBool(videoControls, "WaveformSnapToShotChanges", v => w.SnapToShotChanges = v);
        SetBool(videoControls, "WaveformTextBold", v => w.WaveformTextFontBold = v);
        SetInt(videoControls, "WaveformTextSize", 1, 100, v => w.WaveformTextFontSize = v);
    }

    /// <summary>
    /// Theme, fonts and toolbar buttons. SE 4's icon theme names do not line up with SE 5's, so
    /// the icon set is left alone - "Set up like Subtitle Edit 4" is the way to get those.
    /// </summary>
    public static void ApplyAppearance(Se4SettingsFile file)
    {
        var general = file.General;
        if (general == null)
        {
            return;
        }

        var a = Se.Settings.Appearance;

        SetBool(general, "UseDarkTheme", v => a.Theme = v ? UiTheme.ThemeNameDark : UiTheme.ThemeNameLight);
        SetColor(general, "DarkThemeBackColor", v => a.DarkModeBackgroundColor = v);
        SetColor(general, "DarkThemeForeColor", v => a.DarkModeForegroundColor = v);

        var fontName = Value(general, "SubtitleFontName");
        if (!string.IsNullOrWhiteSpace(fontName))
        {
            a.SubtitleTextBoxAndGridFontName = fontName;
        }

        SetInt(general, "SubtitleTextBoxFontSize", 1, 500, v => a.SubtitleTextBoxFontSize = v);
        SetInt(general, "SubtitleListViewFontSize", 1, 500, v => a.SubtitleGridFontSize = v);
        SetBool(general, "SubtitleTextBoxFontBold", v => a.SubtitleTextBoxFontBold = v);
        SetBool(general, "CenterSubtitleInTextBox", v => a.SubtitleTextBoxCenterText = v);
        SetBool(general, "SubtitleTextBoxSyntaxColor", v => a.SubtitleTextBoxColorTags = v);

        foreach (var (se4Name, apply) in ToolbarMap(a))
        {
            SetBool(general, se4Name, apply);
        }
    }

    // SE 4 setting name -> the SE 5 toolbar toggle it turns on. SE 5 toolbar buttons with no SE 4
    // counterpart (multiple replace, point sync, auto-translate, speech to text, ...) are left as
    // they are - SE 4 could not have had an opinion about them.
    private static IEnumerable<(string Name, Action<bool> Apply)> ToolbarMap(SeAppearance a)
    {
        yield return ("ShowToolbarNew", v => a.ToolbarShowFileNew = v);
        yield return ("ShowToolbarOpen", v => a.ToolbarShowFileOpen = v);
        yield return ("ShowToolbarOpenVideo", v => a.ToolbarShowVideoFileOpen = v);
        yield return ("ShowToolbarSave", v => a.ToolbarShowSave = v);
        yield return ("ShowToolbarSaveAs", v => a.ToolbarShowSaveAs = v);
        yield return ("ShowToolbarFind", v => a.ToolbarShowFind = v);
        yield return ("ShowToolbarReplace", v => a.ToolbarShowReplace = v);
        yield return ("ShowToolbarFixCommonErrors", v => a.ToolbarShowFixCommonErrors = v);
        yield return ("ShowToolbarRemoveTextForHi", v => a.ToolbarShowRemoveTextForHi = v);
        yield return ("ShowToolbarToggleSourceView", v => a.ToolbarShowSourceView = v);
        yield return ("ShowToolbarVisualSync", v => a.ToolbarShowVisualSync = v);
        yield return ("ShowToolbarBurnIn", v => a.ToolbarShowBurnIn = v);
        yield return ("ShowToolbarSpellCheck", v => a.ToolbarShowSpellCheck = v);
        yield return ("ShowToolbarBeautifyTimeCodes", v => a.ToolbarShowBeautifyTimeCodes = v);
        yield return ("ShowToolbarSettings", v => a.ToolbarShowSettings = v);
        yield return ("ShowToolbarHelp", v => a.ToolbarShowHelp = v);
        yield return ("ShowFrameRate", v => a.ToolbarShowFrameRate = v);
    }

    /// <summary>
    /// Engine URLs, models, prompts and API keys. SE 4 prefixed several of these with
    /// "AutoTranslate" and SE 5 does not, so the pairs are spelled out.
    /// </summary>
    public static void ApplyAutoTranslate(Se4SettingsFile file)
    {
        var tools = file.Tools;
        if (tools == null)
        {
            return;
        }

        var t = Se.Settings.AutoTranslate;

        foreach (var (se4Name, apply) in AutoTranslateMap(t))
        {
            var value = Value(tools, se4Name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                apply(value);
            }
        }

        SetInt(tools, "AutoTranslateMaxBytes", 1, int.MaxValue, v => t.RequestMaxBytes = v);
        SetInt(tools, "AutoTranslateDelaySeconds", 0, 3600, v => t.RequestDelaySeconds = v);
        SetInt(tools, "TranslateViaCopyPasteMaxSize", 1, int.MaxValue, v => t.CopyPasteMaxBlockSize = v);
    }

    private static IEnumerable<(string Name, Action<string> Apply)> AutoTranslateMap(SeAutoTranslate t)
    {
        yield return ("GoogleApiV2Key", v => t.GoogleApiV2Key = v);
        yield return ("MicrosoftBingApiId", v => t.MicrosoftBingApiId = v);
        yield return ("MicrosoftTranslatorApiKey", v => t.MicrosoftTranslatorApiKey = v);
        yield return ("MicrosoftTranslatorTokenEndpoint", v => t.MicrosoftTranslatorTokenEndpoint = v);
        yield return ("MicrosoftTranslatorCategory", v => t.MicrosoftTranslatorCategory = v);

        yield return ("AutoTranslateDeepLApiKey", v => t.DeepLApiKey = v);
        yield return ("AutoTranslateDeepLUrl", v => t.DeepLUrl = v);
        yield return ("AutoTranslateDeepLFormality", v => t.DeepLFormality = v);
        yield return ("AutoTranslateDeepLXUrl", v => t.DeepLXUrl = v);
        yield return ("AutoTranslateLibreUrl", v => t.LibreTranslateUrl = v);
        yield return ("AutoTranslateLibreApiKey", v => t.LibreTranslateApiKey = v);
        yield return ("AutoTranslateMyMemoryApiKey", v => t.MyMemoryApiKey = v);
        yield return ("AutoTranslateNllbApiUrl", v => t.NllbApiUrl = v);
        yield return ("AutoTranslateNllbServeUrl", v => t.NllbServeUrl = v);
        yield return ("AutoTranslateNllbServeModel", v => t.NllbServeModel = v);
        yield return ("AutoTranslateSeamlessM4TUrl", v => t.SeamlessM4TUrl = v);
        yield return ("AutoTranslatePapagoApiKeyId", v => t.PapagoApiKeyId = v);
        yield return ("AutoTranslatePapagoApiKey", v => t.PapagoApiKey = v);
        yield return ("AutoTranslateMistralApiKey", v => t.MistralApiKey = v);
        yield return ("AutoTranslateMistralUrl", v => t.MistralUrl = v);
        yield return ("AutoTranslateMistralModel", v => t.MistralModel = v);
        yield return ("AutoTranslateMistralPrompt", v => t.MistralPrompt = v);

        yield return ("ChatGptUrl", v => t.ChatGptUrl = v);
        yield return ("ChatGptPrompt", v => t.ChatGptPrompt = v);
        yield return ("ChatGptApiKey", v => t.ChatGptApiKey = v);
        yield return ("ChatGptModel", v => t.ChatGptModel = v);
        yield return ("GroqUrl", v => t.GroqUrl = v);
        yield return ("GroqPrompt", v => t.GroqPrompt = v);
        yield return ("GroqApiKey", v => t.GroqApiKey = v);
        yield return ("GroqModel", v => t.GroqModel = v);
        yield return ("DeepSeekUrl", v => t.DeepSeekUrl = v);
        yield return ("DeepSeekPrompt", v => t.DeepSeekPrompt = v);
        yield return ("DeepSeekApiKey", v => t.DeepSeekApiKey = v);
        yield return ("DeepSeekModel", v => t.DeepSeekModel = v);
        yield return ("AvalAiUrl", v => t.AvalAiUrl = v);
        yield return ("AvalAiPrompt", v => t.AvalAiPrompt = v);
        yield return ("AvalAiApiKey", v => t.AvalAiApiKey = v);
        yield return ("AvalAiModel", v => t.AvalAiModel = v);
        yield return ("OpenRouterUrl", v => t.OpenRouterUrl = v);
        yield return ("OpenRouterPrompt", v => t.OpenRouterPrompt = v);
        yield return ("OpenRouterApiKey", v => t.OpenRouterApiKey = v);
        yield return ("OpenRouterModel", v => t.OpenRouterModel = v);
        yield return ("LmStudioApiUrl", v => t.LmStudioApiUrl = v);
        yield return ("LmStudioModel", v => t.LmStudioModel = v);
        yield return ("LmStudioPrompt", v => t.LmStudioPrompt = v);
        yield return ("OllamaApiUrl", v => t.OllamaUrl = v);
        yield return ("OllamaModels", v => t.OllamaModels = v);
        yield return ("OllamaModel", v => t.OllamaModel = v);
        yield return ("OllamaPrompt", v => t.OllamaPrompt = v);
        yield return ("KoboldCppUrl", v => t.KoboldCppUrl = v);
        yield return ("KoboldCppPrompt", v => t.KoboldCppPrompt = v);
        yield return ("AnthropicApiUrl", v => t.AnthropicApiUrl = v);
        yield return ("AnthropicPrompt", v => t.AnthropicPrompt = v);
        yield return ("AnthropicApiKey", v => t.AnthropicApiKey = v);
        yield return ("AnthropicApiModel", v => t.AnthropicApiModel = v);
        yield return ("BaiduUrl", v => t.BaiduUrl = v);
        yield return ("BaiduApiKey", v => t.BaiduApiKey = v);
        yield return ("GeminiProApiKey", v => t.GeminiProApiKey = v);
        yield return ("GeminiModel", v => t.GeminiModel = v);
        yield return ("TranslateViaCopyPasteSeparator", v => t.CopyPasteLineSeparator = v);
    }

    /// <summary>
    /// Shortcuts go through the existing SE 4 importer (it also carries the custom-search slots
    /// and the toggle-custom-tags pair the keys point at). A merge, like every other SE 4 import:
    /// only actions the file actually binds are replaced.
    /// </summary>
    /// <param name="normalizeShortcuts">
    /// Called with the imported bindings before they are merged in. SE 4 runs on Windows, so its
    /// modifiers are the Ctrl-based ones; importing on macOS hands them here to be swapped for the
    /// Cmd key the SE 5 defaults use.
    /// </param>
    public static Se4ShortcutsImporter.ImportResult ApplyShortcuts(
        Se4SettingsFile file,
        Action<List<SeShortCut>>? normalizeShortcuts = null)
    {
        var result = Se4ShortcutsImporter.ImportFromXml(file.Xml);
        normalizeShortcuts?.Invoke(result.Shortcuts);

        foreach (var shortcut in result.Shortcuts)
        {
            var existing = Se.Settings.Shortcuts.FirstOrDefault(s => s.ActionName == shortcut.ActionName);
            if (existing != null)
            {
                Se.Settings.Shortcuts.Remove(existing);
            }

            Se.Settings.Shortcuts.Add(shortcut);
        }

        ApplyCustomSearches(result);
        ApplyCustomTags(result);

        return result;
    }

    // Slot N of SE 4 is slot N of SE 5, and the key that fires it travels in the same import -
    // a key pointing at a slot that searches a different site is worse than no key at all.
    private static void ApplyCustomSearches(Se4ShortcutsImporter.ImportResult result)
    {
        foreach (var (slotNumber, search) in result.CustomSearches)
        {
            if (slotNumber < 1 || slotNumber > Se.CustomSearchSlotCount)
            {
                continue;
            }

            Se.Settings.SetCustomSearch(slotNumber, search.Name, search.Url);
            ShortcutsMain.CommandTranslationLookup[$"CustomSearch{slotNumber}Command"] =
                ShortcutsMain.GetSearchViaTitle(slotNumber, search.Name, search.Url);
        }
    }

    // SE 4 has one "toggle custom tags" pair; SE 5 has eight "surround with" slots. Park the pair
    // on the slot already holding it, or on the first free one, and move the imported key onto
    // that slot - see ShortcutsViewModel.ApplySe4CustomTags, which does the same for the Shortcuts
    // window. When every slot is taken the key stays on slot 1: overwriting a pair the user is
    // using would cost more than the import gains.
    private static void ApplyCustomTags(Se4ShortcutsImporter.ImportResult result)
    {
        var start = result.CustomTagsStart;
        var end = result.CustomTagsEnd;
        if (start == null || end == null)
        {
            return;
        }

        var importedShortcut = result.Shortcuts
            .FirstOrDefault(s => s.ActionName == nameof(Features.Main.MainViewModel.SurroundWith1Command));
        if (importedShortcut == null)
        {
            return;
        }

        var slotNumber = -1;
        for (var i = 1; i <= Se.SurroundWithSlotCount; i++)
        {
            if (Se.Settings.GetSurroundLeft(i) == start && Se.Settings.GetSurroundRight(i) == end)
            {
                slotNumber = i;
                break;
            }
        }

        for (var i = 1; slotNumber < 0 && i <= Se.SurroundWithSlotCount; i++)
        {
            if (string.IsNullOrEmpty(Se.Settings.GetSurroundLeft(i)) &&
                string.IsNullOrEmpty(Se.Settings.GetSurroundRight(i)))
            {
                slotNumber = i;
            }
        }

        if (slotNumber < 0)
        {
            return;
        }

        Se.Settings.SetSurround(slotNumber, start, end);

        var commandName = $"SurroundWith{slotNumber}Command";
        var existing = Se.Settings.Shortcuts.FirstOrDefault(s => s.ActionName == commandName);
        if (existing != null)
        {
            Se.Settings.Shortcuts.Remove(existing);
        }

        // The shortcut was added as SurroundWith1Command above; move it to the slot the pair
        // landed on (it is the same object, so the list entry moves with it).
        importedShortcut.ActionName = commandName;
        if (!Se.Settings.Shortcuts.Contains(importedShortcut))
        {
            Se.Settings.Shortcuts.Add(importedShortcut);
        }

        ShortcutsMain.CommandTranslationLookup[commandName] =
            ShortcutsMain.GetSurroundWithTitle(slotNumber, start, end);
    }

    private static string? Value(XElement parent, string name)
    {
        return parent.Elements().FirstOrDefault(e => e.Name.LocalName == name)?.Value;
    }

    private static void SetInt(XElement parent, string name, int min, int max, Action<int> apply)
    {
        var value = Value(parent, name);
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) &&
            number >= min &&
            number <= max)
        {
            apply(number);
        }
    }

    private static void SetDouble(XElement parent, string name, double min, double max, Action<double> apply)
    {
        var value = Value(parent, name);
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
            number >= min &&
            number <= max)
        {
            apply(number);
        }
    }

    private static void SetBool(XElement parent, string name, Action<bool> apply)
    {
        if (bool.TryParse(Value(parent, name), out var value))
        {
            apply(value);
        }
    }

    private static void SetEnumName<T>(XElement parent, string name, Action<string> apply) where T : struct, Enum
    {
        var value = Value(parent, name);
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<T>(value, out var parsed))
        {
            apply(parsed.ToString());
        }
    }

    // SE 4 writes colors as System.Drawing's signed 32-bit ARGB (Color.ToArgb); SE 5 stores hex.
    private static void SetColor(XElement parent, string name, Action<string> apply)
    {
        var value = Value(parent, name);
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var argb))
        {
            return;
        }

        var bytes = unchecked((uint)argb);
        var color = Color.FromArgb(
            (byte)((bytes >> 24) & 0xFF),
            (byte)((bytes >> 16) & 0xFF),
            (byte)((bytes >> 8) & 0xFF),
            (byte)(bytes & 0xFF));

        apply(color.FromColorToHex());
    }
}
