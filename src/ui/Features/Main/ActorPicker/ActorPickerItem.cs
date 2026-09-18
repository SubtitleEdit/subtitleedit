using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Main.ActorPicker;

public partial class ActorPickerItem : ObservableObject
{
    /// <summary>Actor name - for the "new actor" row, the name that will be created.</summary>
    public string Name { get; }

    public string DisplayName { get; }
    public string LineCountText { get; }
    public bool IsCurrent { get; }
    public bool IsNew { get; }

    // Number key and direct "Set actor N" shortcut follow the position, so both are
    // refreshed when the order changes.
    [ObservableProperty] private string _numberText;
    [ObservableProperty] private string _shortcutText;

    // The number keys type into the filter box once it has text, so the numbers are dimmed then.
    [ObservableProperty] private bool _isNumberKeyActive;

    public ActorPickerItem(string name, string displayName, int lineCount, bool isCurrent, bool isNew)
    {
        Name = name;
        DisplayName = displayName;
        LineCountText = lineCount > 0 ? "(" + lineCount + ")" : string.Empty;
        IsCurrent = isCurrent;
        IsNew = isNew;
        _numberText = string.Empty;
        _shortcutText = string.Empty;
        _isNumberKeyActive = true;
    }
}
