using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayerFilter;
public partial class LayerItem : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    public int Layer { get; set; }
    public int UsageCount { get; set; }

    public LayerItem(int layer, bool isSelected, int usageCount)
    {
        Layer = layer;
        IsSelected = isSelected;
        UsageCount = usageCount;
    }
}
