using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.ColorPicker;

/// <summary>
/// Lets the color dialog pick a color from the video of the editor window it was opened from,
/// without knowing about the editor: each editor window registers how to do it, and the dialog
/// looks it up through its owner chain (style/settings dialogs in between, second editor windows).
/// </summary>
internal sealed class ImageColorSource
{
    private static readonly ConditionalWeakTable<Window, ImageColorSource> Sources = new();

    private ImageColorSource(Func<bool> canPick, Func<Window, Task<Color?>> pickAsync)
    {
        CanPick = canPick;
        PickAsync = pickAsync;
    }

    public Func<bool> CanPick { get; }
    public Func<Window, Task<Color?>> PickAsync { get; }

    public static void Register(Window window, Func<bool> canPick, Func<Window, Task<Color?>> pickAsync)
    {
        Sources.AddOrUpdate(window, new ImageColorSource(canPick, pickAsync));
    }

    /// <summary>
    /// The source registered on the nearest window in the owner chain of <paramref name="window"/>, if any.
    /// </summary>
    public static ImageColorSource? Find(Window window)
    {
        for (var owner = window.Owner as Window; owner != null; owner = owner.Owner as Window)
        {
            if (Sources.TryGetValue(owner, out var source))
            {
                return source;
            }
        }

        return null;
    }
}
