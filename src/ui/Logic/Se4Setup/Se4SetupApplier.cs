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
        ApplyWaveformToolbar();
        ApplySelectCurrentLineWhilePlaying(vm);

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

        // SE 4 showed each subtitle on a single grid row with line breaks rendered as
        // "<br />" (Settings.General.ListViewLineSeparatorString default). Match that.
        Se.Settings.Appearance.SubtitleGridTextSingleLine = true;
        Se.Settings.Appearance.SubtitleGridTextSingleLineSeparator = "<br />";
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

    // SE 4 had two control areas around the waveform:
    //   * the waveform's own little toolbar below it (zoom out / zoom / zoom in,
    //     play + pause, lock center, playback rate), and
    //   * the "Translate/Create/Adjust" box to the LEFT of the waveform, whose
    //     Translate tab held the "<< Previous", "Play current", "Pause" and
    //     "Next >>" buttons and whose Create/Adjust tabs held insert-new,
    //     set start, set end and set-start-and-offset-the-rest.
    // SE 5 has a single waveform toolbar, so lay it out in that order: the SE 4
    // play/pause toggle, then the four Translate-tab buttons, then the
    // Create/Adjust actions, then the waveform toolbar's own zoom/position/rate/
    // lock-center controls. Everything with no SE 4 counterpart is hidden.
    private static readonly SeWaveformToolbarItemType[] Se4ToolbarOrder =
    [
        SeWaveformToolbarItemType.Play,                     // SE 4 waveform toolbar play/pause
        SeWaveformToolbarItemType.TextPrevious,             // "<< Previous"
        SeWaveformToolbarItemType.TextPlay,                 // "Play current"
        SeWaveformToolbarItemType.TextPause,                // "Pause"
        SeWaveformToolbarItemType.TextNext,                 // "Next >>"
        SeWaveformToolbarItemType.New,                      // Create tab: insert new at video position
        SeWaveformToolbarItemType.SetStart,                 // Create tab: set start
        SeWaveformToolbarItemType.SetEnd,                   // Create tab: set end
        SeWaveformToolbarItemType.SetStartAndOffsetTheRest, // Adjust tab: set start and offset the rest
        SeWaveformToolbarItemType.HorizontalZoom,           // SE 4 waveform toolbar zoom combo
        SeWaveformToolbarItemType.VideoPositionSlider,      // SE 4 trackBarWaveformPosition
        SeWaveformToolbarItemType.AudioTrackPicker,         // only rendered for multi-track videos
        SeWaveformToolbarItemType.PlaybackSpeed,            // SE 4 play-rate split button
        SeWaveformToolbarItemType.AutoSelectOnPlay,         // SE 4 checkBoxSyncListViewWithVideoWhilePlaying
        SeWaveformToolbarItemType.Center,                   // SE 4 "lock center" toggle
        SeWaveformToolbarItemType.More,                     // kept so the toolbar can be reconfigured
    ];

    // "Select current subtitle while playing" - SE 4 had it as a checkbox sitting right
    // above the waveform (checkBoxSyncListViewWithVideoWhilePlaying); in SE 5 it is the
    // AutoSelectOnPlay toggle in the waveform toolbar, turned on here so the SE 4 setup
    // arrives with it enabled. MainViewModel keeps its own observable copy of the setting
    // and writes that back on exit, so both have to be set or this value is lost again.
    private static void ApplySelectCurrentLineWhilePlaying(MainViewModel vm)
    {
        Se.Settings.General.SelectCurrentSubtitleWhilePlaying = true;
        vm.SelectCurrentSubtitleWhilePlaying = true;
    }

    internal static void ApplyWaveformToolbar()
    {
        var w = Se.Settings.Waveform;
        w.ShowToolbar = true;

        // Settings written by an older version can be missing item types entirely.
        w.EnsureAllToolbarItems();

        foreach (var item in w.ToolbarItems)
        {
            var index = System.Array.IndexOf(Se4ToolbarOrder, item.Type);
            item.IsVisible = index >= 0;
            if (index >= 0)
            {
                // Same 10, 20, 30, ... spacing the "Configure toolbar items" dialog writes.
                item.SortOrder = (index + 1) * 10;
            }
        }
    }
}
