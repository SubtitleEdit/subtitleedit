using Avalonia.Controls;
using Avalonia.Layout;

namespace Nikse.SubtitleEdit.Features.Main;

public static class ViewContent
{
    public static Grid Make(MainViewModel vm)
    {
        return new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children = { new StackPanel() }
        };
    }
}