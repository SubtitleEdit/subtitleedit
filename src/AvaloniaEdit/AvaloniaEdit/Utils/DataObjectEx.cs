using Avalonia.Interactivity;
using AvaloniaEdit.Document;

namespace AvaloniaEdit.Utils;

public static class DataObjectEx
{
    /// <summary>
    /// Shim for WPF's DataObject.CopyingEvent which is not available in Avalonia.
    /// </summary>
    public static readonly RoutedEvent<DataObjectCopyingEventArgs> DataObjectCopyingEvent =
        RoutedEvent.Register<DataObjectCopyingEventArgs>(
            nameof(DataObjectCopyingEvent),
            RoutingStrategies.Bubble,
            typeof(DataObjectEx));
}
