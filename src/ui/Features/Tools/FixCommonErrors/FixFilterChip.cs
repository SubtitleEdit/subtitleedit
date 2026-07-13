using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixFilterChip : ObservableObject
{
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private int _count;
    [ObservableProperty] private int _selectedCount;

    /// <summary>Action display name this chip filters on - null means "all".</summary>
    public string? Action { get; init; }
    public string Label { get; init; } = string.Empty;

    // Show "selected / total" so each tab reflects how many of its fixes are ticked (#12377).
    public string Display => Count > 0 ? $"{Label} ({SelectedCount}/{Count})" : Label;

    partial void OnCountChanged(int value)
    {
        OnPropertyChanged(nameof(Display));
    }

    partial void OnSelectedCountChanged(int value)
    {
        OnPropertyChanged(nameof(Display));
    }
}
