namespace AvaloniaEdit.CodeCompletion;

/// <summary>
/// Defines the pointer action used to request the insertion of a completion item.
/// </summary>
public enum CompletionAcceptAction
{
    /// <summary>
    /// Insert the completion item when the pointer is pressed. (This option makes the completion
    /// list behave similar to the completion list in Visual Studio Code.)
    /// </summary>
    PointerPressed,

    /// <summary>
    /// Insert the completion item when the pointer is pressed. (This option makes the completion
    /// list behave similar to a context menu.)
    /// </summary>
    PointerReleased,

    /// <summary>
    /// Insert the code completion item when the item is double-tapped. (This option makes the
    /// completion list behave similar to the completion list in Visual Studio.)
    /// </summary>
    DoubleTapped
}
