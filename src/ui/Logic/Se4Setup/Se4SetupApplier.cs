using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Se4Setup;

// One-shot "make this feel like Subtitle Edit 4" setup. It:
//   * imports SE 4 replace rules and keyboard shortcuts from the classic
//     Settings.xml (or falls back to SE 4 default values when no file is found),
//   * switches the theme + toolbar icons to "Classic", and
//   * applies the SE 4 green-on-black waveform palette.
//
// Importing is a MERGE: the user's existing rules/shortcuts are kept and only
// direct conflicts (same action / same rule) are overwritten. The caller is
// responsible for saving settings and calling MainViewModel.ApplySettings() so
// the changes take effect live.
public static class Se4SetupApplier
{
    public sealed class Se4SetupResult
    {
        public bool SettingsXmlFound { get; set; }
        public string? SettingsXmlPath { get; set; }
        public int ShortcutsImported { get; set; }
        public int ShortcutsSkipped { get; set; }
        public int ReplaceCategoriesAdded { get; set; }
        public int ReplaceRulesAdded { get; set; }
    }

    public static Se4SetupResult Apply(MainViewModel vm)
    {
        var result = new Se4SetupResult();

        var settingsXmlPath = Se4ShortcutsImporter.FindDefaultSettingsFile();
        result.SettingsXmlFound = settingsXmlPath != null;
        result.SettingsXmlPath = settingsXmlPath;

        ApplyShortcuts(vm, settingsXmlPath, result);
        ApplyReplaceRules(settingsXmlPath, result);
        ApplyClassicTheme();
        ApplyToolbarAndEditBox();
        ApplyWaveformColors();

        Se.SaveSettings();
        return result;
    }

    private static void ApplyShortcuts(MainViewModel vm, string? settingsXmlPath, Se4SetupResult result)
    {
        if (settingsXmlPath != null)
        {
            var imported = Se4ShortcutsImporter.ImportFromFile(settingsXmlPath);
            foreach (var shortcut in imported.Shortcuts)
            {
                var existing = Se.Settings.Shortcuts.FirstOrDefault(s => s.ActionName == shortcut.ActionName);
                if (existing != null)
                {
                    Se.Settings.Shortcuts.Remove(existing);
                }

                Se.Settings.Shortcuts.Add(shortcut);
            }

            result.ShortcutsImported = imported.Shortcuts.Count;
            result.ShortcutsSkipped = imported.SkippedNoMapping;
            return;
        }

        // No SE 4 Settings.xml on this machine: fall back to SE 4 default
        // shortcuts. Merge mode means we only add bindings the user doesn't
        // already have an entry for (keeping any customizations).
        foreach (var def in ShortcutsMain.GetDefaultShortcuts(vm))
        {
            if (Se.Settings.Shortcuts.All(s => s.ActionName != def.ActionName))
            {
                Se.Settings.Shortcuts.Add(def);
                result.ShortcutsImported++;
            }
        }
    }

    private static void ApplyReplaceRules(string? settingsXmlPath, Se4SetupResult result)
    {
        if (settingsXmlPath == null || !File.Exists(settingsXmlPath))
        {
            return;
        }

        string xml;
        try
        {
            xml = File.ReadAllText(settingsXmlPath);
        }
        catch
        {
            return;
        }

        var importedCategories = Se4SettingsXmlReplaceImporter.ImportFromXml(xml);
        var categories = Se.Settings.Edit.MultipleReplace.Categories;

        foreach (var imported in importedCategories)
        {
            var existing = categories.FirstOrDefault(c =>
                string.Equals(c.Name, imported.Name, System.StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                categories.Add(imported);
                result.ReplaceCategoriesAdded++;
                result.ReplaceRulesAdded += imported.Rules.Count;
                continue;
            }

            foreach (var rule in imported.Rules)
            {
                if (existing.Rules.Any(r => r.Find == rule.Find && r.ReplaceWith == rule.ReplaceWith && r.Type == rule.Type))
                {
                    continue;
                }

                existing.Rules.Add(rule);
                result.ReplaceRulesAdded++;
            }
        }
    }

    private static void ApplyClassicTheme()
    {
        Se.Settings.Appearance.Theme = UiTheme.ThemeNameClassic;
        Se.Settings.Appearance.IconTheme = UiTheme.ThemeNameClassic;
    }

    // SE 4 toolbar/edit-box differences: SE 4 showed the encoding selector in the
    // toolbar and had no dedicated "Italic" button above the text box (formatting
    // was applied via the context menu / shortcut instead).
    private static void ApplyToolbarAndEditBox()
    {
        Se.Settings.Appearance.ToolbarShowEncoding = true;
        Se.Settings.Appearance.TextBoxShowButtonItalic = false;
    }

    // The SE 4 waveform look, taken from the classic AudioVisualizer defaults
    // (se4-legacy branch): a green-yellow waveform on black that turns red for
    // the selected subtitle, a green start marker and red end marker on each
    // paragraph, grey text, grid lines on and the "Classic" (non-fancy) draw style.
    private static void ApplyWaveformColors()
    {
        var w = Se.Settings.Waveform;

        // SE 4: Color = GreenYellow, SelectedColor = Red (the selected subtitle's
        // waveform line is drawn in this colour), BackgroundColor = Black,
        // TextColor = Gray.
        w.WaveformColor = Colors.GreenYellow.FromColorToHex();
        w.WaveformSelectedColor = Colors.Red.FromColorToHex();
        w.WaveformBackgroundColor = Colors.Black.FromColorToHex();
        w.WaveformTextColor = Colors.Gray.FromColorToHex();
        w.WaveformCursorColor = Colors.Cyan.FromColorToHex();
        w.WaveformShotChangeColor = Colors.AntiqueWhite.FromColorToHex();

        // SE 4 drew the paragraph start marker (left) green and the end marker
        // (right) red.
        w.WaveformParagraphLeftColor = Color.FromArgb(180, 50, 205, 50).FromColorToHex();
        w.WaveformParagraphRightColor = Color.FromArgb(180, 255, 0, 0).FromColorToHex();
        w.WaveformFancyHighColor = Colors.Orange.FromColorToHex();

        // SE 4 used a dark gray paragraph background; make the selected paragraph a
        // little lighter so the active subtitle stands out.
        w.ParagraphBackground = Color.FromArgb(90, 70, 70, 70).FromColorToHex();
        w.ParagraphSelectedBackground = Color.FromArgb(90, 100, 100, 100).FromColorToHex();

        // SE 4 drew classic (non-fancy) waveforms with vertical grid lines.
        w.DrawGridLines = true;
        w.WaveformDrawStyle = Controls.AudioVisualizerControl.WaveformDrawStyle.Classic.ToString();
    }
}
