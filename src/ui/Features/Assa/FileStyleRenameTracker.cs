using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa;

/// <summary>
/// Keeps the subtitle's dialog lines pointing at the right style while file styles are
/// renamed in the ASSA/SSA styles dialog (#13101). The name text box binds per keystroke,
/// so each change re-points every line from the style's last known name to the new one -
/// without this, renamed styles keep their old name in the lines, and OK silently resets
/// those lines to the first style in the file.
/// </summary>
internal sealed class FileStyleRenameTracker
{
    private readonly ObservableCollection<StyleDisplay> _fileStyles;
    private readonly Func<Subtitle> _getSubtitle;
    private readonly Action _updateUsages;
    private readonly List<StyleDisplay> _tracked = new();

    public FileStyleRenameTracker(ObservableCollection<StyleDisplay> fileStyles, Func<Subtitle> getSubtitle, Action updateUsages)
    {
        _fileStyles = fileStyles;
        _getSubtitle = getSubtitle;
        _updateUsages = updateUsages;

        fileStyles.CollectionChanged += (_, _) => Resync();
        Resync();
    }

    private void Resync()
    {
        var previouslyTracked = new HashSet<StyleDisplay>(_tracked);
        foreach (var style in _tracked)
        {
            style.PropertyChanged -= StylePropertyChanged;
        }

        _tracked.Clear();

        foreach (var style in _fileStyles)
        {
            if (!previouslyTracked.Contains(style))
            {
                // The name a style enters the list with is the name its lines (if any) use -
                // for loaded styles the name from the header, for new/imported/duplicated
                // styles the freshly generated unique name.
                style.LastKnownName = style.Name;
            }

            style.PropertyChanged += StylePropertyChanged;
            _tracked.Add(style);
        }
    }

    private void StylePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StyleDisplay.Name) || sender is not StyleDisplay style)
        {
            return;
        }

        var oldName = style.LastKnownName;
        var newName = style.Name;
        if (string.IsNullOrWhiteSpace(newName) || newName == oldName)
        {
            return; // blank is a transient editing state; keep the chain and wait for a real name
        }

        if (_fileStyles.Any(other => !ReferenceEquals(other, style) && other.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            return; // duplicate name (possibly mid-typing); re-pointing now would merge two styles' lines
        }

        if (_fileStyles.Any(other => !ReferenceEquals(other, style) && other.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase)))
        {
            style.LastKnownName = newName; // the old name belongs to another style; never steal its lines
            return;
        }

        foreach (var paragraph in _getSubtitle().Paragraphs)
        {
            if (paragraph.Extra != null && paragraph.Extra.TrimStart('*').Equals(oldName, StringComparison.OrdinalIgnoreCase))
            {
                paragraph.Extra = newName;
            }
        }

        style.LastKnownName = newName;
        _updateUsages();
    }
}
