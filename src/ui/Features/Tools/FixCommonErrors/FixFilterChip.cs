using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixFilterChip : ObservableObject
{
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private int _count;

    /// <summary>Action display name this chip filters on - null means "all".</summary>
    public string? Action { get; init; }
    public string Label { get; init; } = string.Empty;

    public string Display => Count > 0 ? $"{Label} ({Count})" : Label;

    partial void OnCountChanged(int value)
    {
        OnPropertyChanged(nameof(Display));
    }
}
