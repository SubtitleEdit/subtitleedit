using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Controls.TrackHeaderControl;

public class TrackHeaderPanel : Control
{
    public static readonly StyledProperty<List<SubtitleTrack>> TracksProperty =
        AvaloniaProperty.Register<TrackHeaderPanel, List<SubtitleTrack>>(nameof(Tracks), new List<SubtitleTrack>());

    public static readonly StyledProperty<int> ActiveTrackIndexProperty =
        AvaloniaProperty.Register<TrackHeaderPanel, int>(nameof(ActiveTrackIndex), 0);

    public List<SubtitleTrack> Tracks
    {
        get => GetValue(TracksProperty);
        set => SetValue(TracksProperty, value);
    }

    public int ActiveTrackIndex
    {
        get => GetValue(ActiveTrackIndexProperty);
        set => SetValue(ActiveTrackIndexProperty, value);
    }

    public event EventHandler<int>? ActiveTrackChanged;
    public event EventHandler<int>? RenameRequested;

    public TrackHeaderPanel()
    {
        AffectsRender<TrackHeaderPanel>(TracksProperty, ActiveTrackIndexProperty);
        PointerPressed += OnPointerPressed;
        DoubleTapped += OnDoubleTapped;
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        var height = Bounds.Height;
        if (height <= 0) return;
        var pos = e.GetPosition(this);
        var trackCount = Math.Max(1, Tracks.Count);
        var trackRowHeight = height / trackCount;
        if (trackRowHeight <= 0) return;
        var clickedTrack = Math.Clamp((int)(pos.Y / trackRowHeight), 0, trackCount - 1);
        RenameRequested?.Invoke(this, clickedTrack);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var height = Bounds.Height;
        if (height <= 0) { e.Handled = true; return; }
        var pos = e.GetPosition(this);
        var trackCount = Math.Max(1, Tracks.Count);
        var trackRowHeight = height / trackCount;
        if (trackRowHeight <= 0) { e.Handled = true; return; }
        var clickedTrack = (int)(pos.Y / trackRowHeight);
        clickedTrack = Math.Clamp(clickedTrack, 0, trackCount - 1);
        ActiveTrackIndex = clickedTrack;
        ActiveTrackChanged?.Invoke(this, clickedTrack);
    }

    public override void Render(DrawingContext context)
    {
        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width <= 0 || height <= 0) return;

        context.DrawRectangle(new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)), null, new Rect(0, 0, width, height));

        var tracks = Tracks;
        var trackCount = Math.Max(1, tracks.Count);
        var trackRowHeight = height / trackCount;

        for (var i = 0; i < trackCount; i++)
        {
            var trackY = i * trackRowHeight;
            var track = i < tracks.Count ? tracks[i] : new SubtitleTrack(i, $"Track {i + 1}", "#4B6EAF");

            // Active track background
            if (i == ActiveTrackIndex)
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), null,
                    new Rect(0, trackY, width, trackRowHeight));
            }

            // Color swatch (left 6px)
            if (Color.TryParse(track.Color, out var trackColor))
            {
                context.DrawRectangle(new SolidColorBrush(trackColor), null,
                    new Rect(0, trackY + 4, 6, trackRowHeight - 8));
            }

            // Track name
            var nameText = new FormattedText(
                track.Name,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                11,
                new SolidColorBrush(Colors.White));
            context.DrawText(nameText, new Point(10, trackY + (trackRowHeight - nameText.Height) / 2));

            // Divider line
            if (i < trackCount - 1)
            {
                context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)), 1),
                    new Point(0, trackY + trackRowHeight),
                    new Point(width, trackY + trackRowHeight));
            }
        }
    }
}
