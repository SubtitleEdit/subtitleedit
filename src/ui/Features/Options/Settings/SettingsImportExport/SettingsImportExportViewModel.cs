using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.Se4Setup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public partial class SettingsImportExportViewModel : ObservableObject
{
    public string TitleText { get; set; }
    [ObservableProperty] private bool _exportImportAll;
    [ObservableProperty] private bool _exportImportRecentFiles;
    [ObservableProperty] private bool _exportImportRules;
    [ObservableProperty] private bool _exportImportAppearance;
    [ObservableProperty] private bool _exportImportAutoTranslate;
    [ObservableProperty] private bool _exportImportWaveform;
    [ObservableProperty] private bool _exportImportSyntaxColoring;
    [ObservableProperty] private bool _exportImportShortcuts;

    private bool _isRulesEnabled = true;
    public bool IsRulesEnabled
    {
        get => _isRulesEnabled;
        set
        {
            if (SetProperty(ref _isRulesEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditRules));
            }
        }
    }

    private bool _isAppearanceEnabled = true;
    public bool IsAppearanceEnabled
    {
        get => _isAppearanceEnabled;
        set
        {
            if (SetProperty(ref _isAppearanceEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditAppearance));
            }
        }
    }

    private bool _isAutoTranslateEnabled = true;
    public bool IsAutoTranslateEnabled
    {
        get => _isAutoTranslateEnabled;
        set
        {
            if (SetProperty(ref _isAutoTranslateEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditAutoTranslate));
            }
        }
    }

    private bool _isWaveformEnabled = true;
    public bool IsWaveformEnabled
    {
        get => _isWaveformEnabled;
        set
        {
            if (SetProperty(ref _isWaveformEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditWaveform));
            }
        }
    }

    private bool _isShortcutsEnabled = true;
    public bool IsShortcutsEnabled
    {
        get => _isShortcutsEnabled;
        set
        {
            if (SetProperty(ref _isShortcutsEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditShortcuts));
            }
        }
    }

    public bool CanEditRules => !ExportImportAll && _isRulesEnabled;
    public bool CanEditAppearance => !ExportImportAll && _isAppearanceEnabled;
    public bool CanEditAutoTranslate => !ExportImportAll && _isAutoTranslateEnabled;
    public bool CanEditWaveform => !ExportImportAll && _isWaveformEnabled;
    public bool CanEditShortcuts => !ExportImportAll && _isShortcutsEnabled;

    private bool _isExport;
    private string _importFilePath = string.Empty;
    private Se? _importData;
    private string? _importSourceOs;
    private bool _importHasShortcutSlots;
    private bool _importHasCustomSearchSlots;

    // Set instead of _importData when the picked file is an SE 4 Settings.xml (#14309): SE 4 has
    // no Settings.json, so the only file a user migrating from 4.x can point at is the classic
    // XML. Field-by-field mapping, see Se4SettingsXmlImporter.
    private Se4SettingsXmlImporter.Se4SettingsFile? _se4ImportData;

    // Marker property name written at the top level of the export JSON so the
    // importer can tell which OS the file came from (Se has no such field, so
    // System.Text.Json silently ignores it when deserializing into Se).
    private const string ExportSourceOsProperty = "exportSourceOs";

    // Second marker: set when the file carries the shortcut *slot* values (colors,
    // actors, "surround with" pairs). Files written before #14232 always held the
    // defaults for those, so without the marker the importer must leave them alone
    // rather than reset the user's own to factory values.
    private const string ExportShortcutSlotsProperty = "exportIncludesShortcutSlots";
    public bool OkPressed { get; set; }
    public Window? Window { get; set; }
    private readonly IFileHelper _fileHelper;

    public SettingsImportExportViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        TitleText = Se.Language.General.ImportDotDotDot;
        ExportImportAll = true;
    }

    partial void OnExportImportAllChanged(bool value)
    {
        OnPropertyChanged(nameof(CanEditRules));
        OnPropertyChanged(nameof(CanEditAppearance));
        OnPropertyChanged(nameof(CanEditAutoTranslate));
        OnPropertyChanged(nameof(CanEditWaveform));
        OnPropertyChanged(nameof(CanEditShortcuts));
    }

    public void SetIsExport(bool isExport)
    {
        _isExport = isExport;
        TitleText = isExport ? Se.Language.General.ExportDotDotDot : Se.Language.General.ImportDotDotDot;
    }


    public async Task<bool> PromptAndLoadImportFile()
    {
        if (Window == null)
        {
            return false;
        }

        var fileName = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.ImportDotDotDot,
            "JSON files",
            ".json",
            "Subtitle Edit 4 settings",
            ".xml");

        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        _importFilePath = fileName;

        try
        {
            var json = File.ReadAllText(_importFilePath);

            if (Se4SettingsXmlImporter.LooksLikeXml(json))
            {
                return LoadSe4ImportFile(json);
            }

            _importData = JsonSerializer.Deserialize<Se>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (_importData == null)
            {
                return false;
            }

            _importSourceOs = TryReadExportSourceOs(json);
            _importHasShortcutSlots = TryReadExportIncludesShortcutSlots(json);
            _importHasCustomSearchSlots = TryReadHasCustomSearchSlots(json);

            IsRulesEnabled = _importData.General != null;
            IsAppearanceEnabled = _importData.Appearance != null;
            IsAutoTranslateEnabled = _importData.AutoTranslate != null;
            IsWaveformEnabled = _importData.Waveform != null;
            IsShortcutsEnabled = _importData.Shortcuts != null;

            if (!IsRulesEnabled)
            {
                ExportImportRules = false;
                ExportImportSyntaxColoring = false; // the coloring fields live in General
            }

            if (!IsAppearanceEnabled)
            {
                ExportImportAppearance = false;
            }

            if (!IsAutoTranslateEnabled)
            {
                ExportImportAutoTranslate = false;
            }

            if (!IsWaveformEnabled)
            {
                ExportImportWaveform = false;
            }

            if (!IsShortcutsEnabled)
            {
                ExportImportShortcuts = false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    // An SE 4 Settings.xml carries only some of what the dialog offers - the categories its
    // sections cover - so the checkboxes for the rest are greyed out just like they are for a
    // partial JSON export.
    private bool LoadSe4ImportFile(string xml)
    {
        var se4 = Se4SettingsXmlImporter.Parse(xml);
        if (se4 == null)
        {
            return false;
        }

        _importData = null;
        _se4ImportData = se4;
        _importSourceOs = null;
        _importHasShortcutSlots = false;
        _importHasCustomSearchSlots = false;

        IsRulesEnabled = se4.HasRules;
        IsAppearanceEnabled = se4.HasAppearance;
        IsAutoTranslateEnabled = se4.HasAutoTranslate;
        IsWaveformEnabled = se4.HasWaveform;
        IsShortcutsEnabled = se4.HasShortcuts;

        if (!IsRulesEnabled)
        {
            ExportImportRules = false;
        }

        if (!se4.HasSyntaxColoring)
        {
            ExportImportSyntaxColoring = false;
        }

        if (!IsAppearanceEnabled)
        {
            ExportImportAppearance = false;
        }

        if (!IsAutoTranslateEnabled)
        {
            ExportImportAutoTranslate = false;
        }

        if (!IsWaveformEnabled)
        {
            ExportImportWaveform = false;
        }

        if (!IsShortcutsEnabled)
        {
            ExportImportShortcuts = false;
        }

        return true;
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (_isExport)
        {
            await ExportSettings();
        }
        else
        {
            ImportSettings();
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private async Task ExportSettings()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            ".json",
            "Settings.json",
            Se.Language.General.ExportDotDotDot);

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var exportData = new Se();
        var currentSettings = Se.Settings;

        // Every Se section is self-initializing, so a section left untouched here would still
        // serialize as a full block of *defaults* - and the importer, which only checks for
        // null, would happily install those defaults over the user's settings. Assign null
        // for anything not being exported so "what's in the file" is unambiguous.

        // Syntax coloring lives inside General, so General has to travel for it - the import
        // side then applies either the whole section or just the coloring fields.
        exportData.General = ExportImportAll || ExportImportRules || ExportImportSyntaxColoring
            ? currentSettings.General
            : null!;

        exportData.Waveform = ExportImportAll || ExportImportWaveform ? currentSettings.Waveform : null!;
        exportData.Tools = ExportImportAll ? currentSettings.Tools : null!;
        exportData.Appearance = ExportImportAll || ExportImportAppearance ? currentSettings.Appearance : null!;
        exportData.Options = ExportImportAll ? currentSettings.Options : null!;
        // The shortcut slots the Shortcuts window configures (colors 1-8, actors 1-10 and the
        // "surround with" pairs) live as top-level values on Se, so they were left at the
        // defaults of `new Se()` above and the import side never looked at them - every one of
        // those customizations was silently dropped on export/import (#14232).
        var exportShortcuts = ExportImportAll || ExportImportShortcuts;
        exportData.Shortcuts = exportShortcuts ? currentSettings.Shortcuts : null!;
        CopyShortcutSlots(exportShortcuts ? currentSettings : null, exportData);
        exportData.AutoTranslate = ExportImportAll || ExportImportAutoTranslate ? currentSettings.AutoTranslate : null!;
        exportData.SpellCheck = ExportImportAll ? currentSettings.SpellCheck : null!;

        // Video was never assigned, so an "all settings" file carried a default Video block
        // that the importer applied - silently resetting the player choice, the mpv preview
        // style and the custom seek amounts. Exclude RecentFiles so local recent video paths are not exported.
        if (ExportImportAll && currentSettings.Video != null)
        {
            var videoJson = JsonSerializer.Serialize(currentSettings.Video);
            var videoExport = JsonSerializer.Deserialize<SeVideo>(videoJson);
            if (videoExport != null)
            {
                videoExport.RecentFiles.Clear();
                exportData.Video = videoExport;
            }
        }
        else
        {
            exportData.Video = null!;
        }

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        var jsonWithSource = InjectExportMarkers(json, GetCurrentOsName(), exportShortcuts);
        File.WriteAllText(fileName, jsonWithSource);
    }

    private void ImportSettings()
    {
        if (_se4ImportData != null)
        {
            ImportSe4Settings(_se4ImportData);
            return;
        }

        if (_importData == null)
        {
            return;
        }

        var importData = _importData;

        if (ExportImportAll || ExportImportRules)
        {
            if (importData.General != null)
            {
                Se.Settings.General = importData.General;
            }
        }
        else if (ExportImportSyntaxColoring && importData.General != null)
        {
            // Only the syntax-coloring fields of General - the rest of the section is the
            // "Rules" import, which the user did not ask for. (This checkbox used to be
            // wired to nothing at all.)
            var from = importData.General;
            var to = Se.Settings.General;
            to.ColorDurationTooShort = from.ColorDurationTooShort;
            to.ColorDurationTooLong = from.ColorDurationTooLong;
            to.ColorTextTooLong = from.ColorTextTooLong;
            to.ColorTextTooWide = from.ColorTextTooWide;
            to.ColorTextTooWidePixels = from.ColorTextTooWidePixels;
            to.ColorTextTooWideFontName = from.ColorTextTooWideFontName;
            to.ColorTextTooWideFontSize = from.ColorTextTooWideFontSize;
            to.ColorTextTooManyLines = from.ColorTextTooManyLines;
            to.ColorCharactersPerSecond = from.ColorCharactersPerSecond;
            to.ColorWordsPerMinute = from.ColorWordsPerMinute;
            to.ColorTimeCodeOverlap = from.ColorTimeCodeOverlap;
            to.ColorGapTooShort = from.ColorGapTooShort;
            to.ErrorColor = from.ErrorColor;
        }

        if (ExportImportAll || ExportImportWaveform)
        {
            if (importData.Waveform != null)
            {
                Se.Settings.Waveform = importData.Waveform;
            }
        }

        if (ExportImportAll)
        {
            if (importData.Video != null)
            {
                var existingRecentFiles = Se.Settings.Video.RecentFiles;
                Se.Settings.Video = importData.Video;
                Se.Settings.Video.RecentFiles = existingRecentFiles;
            }

            if (importData.Tools != null)
            {
                Se.Settings.Tools = importData.Tools;
            }

            if (importData.Options != null)
            {
                Se.Settings.Options = importData.Options;
            }

            if (importData.SpellCheck != null)
            {
                Se.Settings.SpellCheck = importData.SpellCheck;
            }
        }

        if (ExportImportAll || ExportImportAppearance)
        {
            if (importData.Appearance != null)
            {
                Se.Settings.Appearance = importData.Appearance;
                Se.MigrateMacOsFontSettings(Se.Settings.Appearance, OperatingSystem.IsMacOS(), true);
            }
        }

        if (ExportImportAll || ExportImportShortcuts)
        {
            if (importData.Shortcuts != null)
            {
                if (_importSourceOs != null &&
                    !string.Equals(_importSourceOs, GetCurrentOsName(), StringComparison.Ordinal))
                {
                    NormalizeShortcutModifiersForCurrentOs(importData.Shortcuts);
                }

                Se.Settings.Shortcuts = importData.Shortcuts;
            }

            if (_importHasShortcutSlots)
            {
                CopyShortcutSlots(importData, Se.Settings, _importHasCustomSearchSlots);
            }
        }

        if (ExportImportAll || ExportImportAutoTranslate)
        {
            if (importData.AutoTranslate != null)
            {
                Se.Settings.AutoTranslate = importData.AutoTranslate;
            }
        }

        Se.SaveSettings();
    }

    // The SE 4 side of the import. Same checkboxes, but every category is copied field by field
    // into the current settings instead of replacing a whole section: SE 4 has no counterpart for
    // most of what an SE 5 section holds, and a section-level assignment would reset all of it.
    internal void ImportSe4Settings(Se4SettingsXmlImporter.Se4SettingsFile se4)
    {
        if (ExportImportAll || ExportImportRules)
        {
            Se4SettingsXmlImporter.ApplyRules(se4);
        }

        if (ExportImportAll || ExportImportRules || ExportImportSyntaxColoring)
        {
            // The coloring values live in General, same as the JSON path - "Rules" brings them
            // along and the syntax-coloring checkbox brings them on their own.
            Se4SettingsXmlImporter.ApplySyntaxColoring(se4);
        }

        if (ExportImportAll || ExportImportWaveform)
        {
            Se4SettingsXmlImporter.ApplyWaveform(se4);
        }

        if (ExportImportAll || ExportImportAppearance)
        {
            Se4SettingsXmlImporter.ApplyAppearance(se4);
        }

        if (ExportImportAll || ExportImportAutoTranslate)
        {
            Se4SettingsXmlImporter.ApplyAutoTranslate(se4);
        }

        if (ExportImportAll || ExportImportShortcuts)
        {
            // SE 4 is Windows-only, so every binding in the file is Ctrl-based - on macOS they
            // get the same Ctrl -> Cmd swap a Windows JSON export gets.
            Se4SettingsXmlImporter.ApplyShortcuts(se4, shortcuts =>
            {
                if (OperatingSystem.IsMacOS())
                {
                    NormalizeShortcutModifiersForCurrentOs(shortcuts);
                }
            });
        }

        Se.SaveSettings();
    }

    // Default shortcuts use "Win" as the modifier on macOS (the Cmd/⌘ key) and
    // "Ctrl" on Windows/Linux — see ShortcutsMain.GetCommandOrWin. Only called
    // when the import file is known to have come from a different OS, so we
    // don't disturb user-customized modifiers (e.g. a real Ctrl shortcut on
    // macOS) during a same-OS round-trip.
    //
    // Shortcuts coming from the SE 4 importer use "Control" instead of "Ctrl"
    // (Se4ShortcutsImporter normalises to the Avalonia token), and historical
    // SE 5 settings may contain either spelling — ShortcutManager treats them
    // as the same modifier. Map both spellings so the cross-OS rewrite catches
    // every case.
    private static void NormalizeShortcutModifiersForCurrentOs(List<SeShortCut> shortcuts)
    {
        var isMac = OperatingSystem.IsMacOS();
        // From-set is matched as a group; we always emit the OS-default token.
        var fromTokens = isMac
            ? new[] { "Ctrl", "Control" }
            : new[] { "Win" };
        var to = isMac ? "Win" : "Ctrl";

        foreach (var shortcut in shortcuts)
        {
            if (shortcut.Keys == null)
            {
                continue;
            }

            for (var i = 0; i < shortcut.Keys.Count; i++)
            {
                foreach (var from in fromTokens)
                {
                    if (string.Equals(shortcut.Keys[i], from, StringComparison.Ordinal))
                    {
                        shortcut.Keys[i] = to;
                        break;
                    }
                }
            }
        }
    }

    private static string GetCurrentOsName()
    {
        if (OperatingSystem.IsMacOS())
        {
            return "MacOS";
        }

        if (OperatingSystem.IsWindows())
        {
            return "Windows";
        }

        return "Linux";
    }

    /// <summary>
    /// Copies the shortcut slot values the Shortcuts window owns - colors 1-8, actors 1-10 and the
    /// "surround with" pairs and the "search via" slots - which sit as top-level values on <see cref="Se"/> rather than in one
    /// of its sections. A null <paramref name="from"/> clears them, so an export that leaves
    /// shortcuts out says so instead of shipping a block of defaults.
    /// </summary>
    private static void CopyShortcutSlots(Se? from, Se to, bool includeCustomSearch = true)
    {
        to.Color1 = from?.Color1!;
        to.Color2 = from?.Color2!;
        to.Color3 = from?.Color3!;
        to.Color4 = from?.Color4!;
        to.Color5 = from?.Color5!;
        to.Color6 = from?.Color6!;
        to.Color7 = from?.Color7!;
        to.Color8 = from?.Color8!;

        for (var slot = 1; slot <= Se.SurroundWithSlotCount; slot++)
        {
            to.SetSurround(slot, from?.GetSurroundLeft(slot)!, from?.GetSurroundRight(slot)!);
        }

        // The "search via" slots were added after the slot marker, so a file carrying the marker
        // may still predate them - deserializing then hands back the factory defaults, and
        // copying those would silently reset the user's own slots. Only copy what the file says.
        if (includeCustomSearch)
        {
            for (var slot = 1; slot <= Se.CustomSearchSlotCount; slot++)
            {
                to.SetCustomSearch(slot, from?.GetCustomSearchName(slot)!, from?.GetCustomSearchUrl(slot)!);
            }
        }

        to.Actor1 = from?.Actor1!;
        to.Actor2 = from?.Actor2!;
        to.Actor3 = from?.Actor3!;
        to.Actor4 = from?.Actor4!;
        to.Actor5 = from?.Actor5!;
        to.Actor6 = from?.Actor6!;
        to.Actor7 = from?.Actor7!;
        to.Actor8 = from?.Actor8!;
        to.Actor9 = from?.Actor9!;
        to.Actor10 = from?.Actor10!;
    }

    // Adds the top-level marker properties to the serialized JSON without touching the Se
    // type. Se has no such properties, so System.Text.Json silently ignores them on import.
    private static string InjectExportMarkers(string json, string osName, bool includesShortcutSlots)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return json;
            }

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                writer.WriteString(ExportSourceOsProperty, osName);
                if (includesShortcutSlots)
                {
                    writer.WriteBoolean(ExportShortcutSlotsProperty, true);
                }

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    prop.WriteTo(writer);
                }
                writer.WriteEndObject();
            }

            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        catch
        {
            return json;
        }
    }

    private static string? TryReadExportSourceOs(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty(ExportSourceOsProperty, out var element) &&
                element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
        }
        catch
        {
            // Fall through — missing marker is treated as unknown source OS.
        }

        return null;
    }

    private static bool TryReadExportIncludesShortcutSlots(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object &&
                   doc.RootElement.TryGetProperty(ExportShortcutSlotsProperty, out var element) &&
                   element.ValueKind == JsonValueKind.True;
        }
        catch
        {
            // Missing marker: a file from before the slots travelled - leave them alone.
            return false;
        }
    }

    /// <summary>
    /// Whether the export file carries the "search via" slot values at all. They joined the
    /// existing shortcut-slot marker later, so this is detected off the serialized property
    /// itself: a file from a build without them lacks the key entirely.
    /// </summary>
    private static bool TryReadHasCustomSearchSlots(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, nameof(Se.CustomSearch1Name), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);

        if (!_isExport)
        {
            var loaded = await PromptAndLoadImportFile();
            if (!loaded)
            {
                Window?.Close();
            }
        }
    }
}
