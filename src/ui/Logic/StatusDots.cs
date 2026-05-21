using System;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Download;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Install/update state shown as a small coloured dot next to an engine or model in a combo box.
/// </summary>
public enum DownloadDotStatus
{
    /// <summary>Nothing to install (e.g. a cloud/API engine) - no dot is shown.</summary>
    None,

    /// <summary>Downloadable but not on disk yet - grey dot.</summary>
    NotInstalled,

    /// <summary>Installed and ready to use - green dot.</summary>
    UpToDate,

    /// <summary>Installed, but a newer build is available - amber dot.</summary>
    UpdateAvailable,
}

/// <summary>
/// Shared helpers for the coloured install-status dot used by the text-to-speech, speech-to-text
/// and auto-translate engine/model combo boxes. The green/amber/grey palette matches the status
/// dot already shown in the engine settings dialogs.
/// </summary>
public static class StatusDots
{
    public static readonly IBrush Green = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    public static readonly IBrush Amber = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
    public static readonly IBrush Grey = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    /// <summary>The fill for a status dot, or null when no dot should be shown.</summary>
    public static IBrush? BrushFor(DownloadDotStatus status) => status switch
    {
        DownloadDotStatus.UpToDate => Green,
        DownloadDotStatus.UpdateAvailable => Amber,
        DownloadDotStatus.NotInstalled => Grey,
        _ => null,
    };

    /// <summary>
    /// Maps an installed flag plus a <see cref="DownloadHashManager.UpdateStatus"/> to a dot status.
    /// An installed engine whose version cannot be determined counts as up-to-date: in a combo the
    /// dot only flags "needs download" (grey) or "update waiting" (amber) - anything usable is green.
    /// </summary>
    public static DownloadDotStatus From(bool installed, DownloadHashManager.UpdateStatus updateStatus)
    {
        if (!installed)
        {
            return DownloadDotStatus.NotInstalled;
        }

        return updateStatus == DownloadHashManager.UpdateStatus.UpdateAvailable
            ? DownloadDotStatus.UpdateAvailable
            : DownloadDotStatus.UpToDate;
    }

    /// <summary>
    /// Builds a combo-box item template that renders "[dot] name (size)". The dot sits in a
    /// fixed-width slot so names line up whether or not a row has a dot; a
    /// <see cref="DownloadDotStatus.None"/> row leaves the slot empty. The size suffix is dropped
    /// when null/empty.
    /// </summary>
    /// <remarks>
    /// Recycling is left at the <see cref="FuncDataTemplate{T}"/> default (off): each row's visual
    /// is built from a one-off status snapshot, so a recycled visual would freeze on whichever item
    /// first populated it.
    /// </remarks>
    public static FuncDataTemplate<T> ComboItemTemplate<T>(
        Func<T, string> getName,
        Func<T, string?> getSize,
        Func<T, DownloadDotStatus> getStatus) where T : class
    {
        return new FuncDataTemplate<T>((item, _) =>
        {
            if (item == null)
            {
                return new TextBlock();
            }

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var dotSlot = new Panel
            {
                Width = 16,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var brush = BrushFor(getStatus(item));
            if (brush != null)
            {
                dotSlot.Children.Add(new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = brush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            panel.Children.Add(dotSlot);

            panel.Children.Add(new TextBlock
            {
                Text = getName(item),
                VerticalAlignment = VerticalAlignment.Center,
            });

            var size = getSize(item);
            if (!string.IsNullOrEmpty(size))
            {
                panel.Children.Add(new TextBlock
                {
                    Text = $"  ({size})",
                    Opacity = 0.6,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            return panel;
        });
    }
}
