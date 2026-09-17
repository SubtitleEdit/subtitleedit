using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Actors;

// One row in the actor picker.
public partial class ActorDisplayItem : ObservableObject
{
    public string Name { get; }

    // Null for rows past the 10th (no shortcut slot, badge hidden); empty for a slot with no
    // shortcut assigned yet (badge shown empty); otherwise the shortcut's display text.
    public string? ShortcutText { get; }

    [ObservableProperty] private bool _isHighlighted;

    public ActorDisplayItem(string name, string? shortcutText = null)
    {
        Name = name;
        ShortcutText = shortcutText;
    }
}
