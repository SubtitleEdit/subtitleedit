using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// The miniature of a main window layout in the layout picker, drawn in the theme's colors so the
/// video, lines, text and waveform panes can be told apart. Each layout is a list of panes, given
/// as kind, x, y, width and height, relative to the thumbnail. Keep in sync with <see cref="InitLayout"/>.
/// </summary>
internal static class LayoutThumbnail
{
    private const double ThumbnailWidth = 188;
    private const double ThumbnailHeight = 118;
    private const double PaneGap = 3;
    private const double LineRowHeight = 8;

    private static readonly IBrush VideoBrush = EditorChrome.Themed(Color.Parse("#20405f"), Color.Parse("#d4e6f8"));

    private static readonly string[] Layouts =
    [
        "L 0 0 .5 .55|T 0 .55 .5 .19|V .5 0 .5 .74|W 0 .74 1 .26",
        "V 0 0 .5 .74|L .5 0 .5 .55|T .5 .55 .5 .19|W 0 .74 1 .26",
        "L 0 0 .5 .55|T 0 .55 .5 .19|W 0 .74 .5 .26|V .5 0 .5 1",
        "V 0 0 .5 1|L .5 0 .5 .55|T .5 .55 .5 .19|W .5 .74 .5 .26",
        "V 0 0 1 .27|L 0 .27 1 .33|T 0 .6 1 .14|W 0 .74 1 .26",
        "L 0 0 1 .55|T 0 .55 1 .19|W 0 .74 1 .26",
        "L 0 0 .5 .77|T 0 .77 .5 .23|V .5 0 .5 1",
        "L 0 0 1 .55|T 0 .55 1 .17|V 0 .72 1 .28",
        "V 0 0 1 .27|L 0 .27 1 .58|T 0 .85 1 .15",
        "V 0 0 .5 .43|W .5 0 .5 .24|T .5 .24 .5 .19|L 0 .43 1 .57",
        "V 0 0 1 .27|W 0 .27 1 .25|L 0 .52 1 .33|T 0 .85 1 .15",
        "L 0 0 .5 .74|V .5 0 .5 .54|T .5 .54 .5 .2|W 0 .74 1 .26",
        "V 0 0 .5 .54|T 0 .54 .5 .2|L .5 0 .5 .74|W 0 .74 1 .26",
        "L 0 0 .5 .4|T 0 .4 .5 .15|V .5 0 .5 .55|C 0 .55 1 .14|S 0 .69 1 .07|W 0 .76 1 .24",
        "L 0 0 1 .77|T 0 .77 1 .23",
    ];

    internal static int Count => Layouts.Length;

    internal static Canvas Make(int layoutNumber)
    {
        var canvas = new Canvas { Width = ThumbnailWidth, Height = ThumbnailHeight };
        foreach (var pane in Layouts[layoutNumber - 1].Split('|'))
        {
            var parts = pane.Split(' ');
            AddPane(
                canvas,
                parts[0],
                Parse(parts[1]) * ThumbnailWidth + PaneGap / 2,
                Parse(parts[2]) * ThumbnailHeight + PaneGap / 2,
                Parse(parts[3]) * ThumbnailWidth - PaneGap,
                Parse(parts[4]) * ThumbnailHeight - PaneGap);
        }

        return canvas;
    }

    private static double Parse(string value) => double.Parse(value, CultureInfo.InvariantCulture);

    private static void AddPane(Canvas canvas, string kind, double x, double y, double width, double height)
    {
        Place(canvas, new Border
        {
            Width = width,
            Height = height,
            CornerRadius = new CornerRadius(3),
            Background = kind == "V" ? VideoBrush : EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
        }, x, y);

        switch (kind)
        {
            case "V":
                AddPlayGlyph(canvas, x, y, width, height);
                break;
            case "L":
                AddLines(canvas, x, y, width, height);
                break;
            case "T":
                AddText(canvas, x, y, width, height);
                break;
            case "W":
                AddWaveform(canvas, x, y, width, height);
                break;
            case "C":
                AddColumns(canvas, x, y, width, height);
                break;
            case "S":
                AddSegments(canvas, x, y, width, height);
                break;
        }
    }

    private static void AddPlayGlyph(Canvas canvas, double x, double y, double width, double height)
    {
        var size = Math.Clamp(Math.Min(width, height) * 0.26, 9, 20);
        var left = x + width / 2 - size * 0.4;
        var middle = y + height / 2;
        canvas.Children.Add(new Polygon
        {
            Points = [new Point(left, middle - size / 2), new Point(left + size * 0.9, middle), new Point(left, middle + size / 2)],
            Fill = EditorChrome.AccentBrush,
        });
    }

    /// <summary>
    /// Subtitle grid rows, the second one selected.
    /// </summary>
    private static void AddLines(Canvas canvas, double x, double y, double width, double height)
    {
        const int selectedRow = 1;
        var rows = Math.Max(2, (int)((height - 6) / LineRowHeight));
        for (var row = 0; row < rows; row++)
        {
            var rowY = y + 5 + row * LineRowHeight;
            var selected = row == selectedRow;
            if (selected)
            {
                Place(canvas, new Border { Width = width - 6, Height = 7, CornerRadius = new CornerRadius(2), Background = EditorChrome.AccentSoftBrush }, x + 3, rowY - 2);
            }

            var textWidth = (width - 26) * (0.55 + 0.1 * (row * 7 % 5));
            Place(canvas, Bar(10, 2, EditorChrome.FaintTextBrush), x + 6, rowY);
            Place(canvas, Bar(textWidth, 2, selected ? EditorChrome.AccentBrush : EditorChrome.FaintTextBrush), x + 20, rowY);
        }
    }

    /// <summary>
    /// The text box, as one or two centered lines of a subtitle.
    /// </summary>
    private static void AddText(Canvas canvas, double x, double y, double width, double height)
    {
        var lines = height > 18 ? 2 : 1;
        for (var line = 0; line < lines; line++)
        {
            var lineWidth = width * (line == 0 ? 0.5 : 0.34);
            var top = y + height / 2 - (lines == 2 ? 5 : 1) + line * 7;
            Place(canvas, Bar(lineWidth, 2.5, EditorChrome.DimTextBrush), x + (width - lineWidth) / 2, top);
        }
    }

    private static void AddWaveform(Canvas canvas, double x, double y, double width, double height)
    {
        const double step = 3;
        var bars = (int)((width - 8) / step);
        for (var i = 0; i < bars; i++)
        {
            var amplitude = 0.18 + 0.72 * Math.Abs(Math.Sin(i * 0.37) * Math.Cos(i * 0.11 + 1));
            var barHeight = Math.Max(1.5, (height - 8) * amplitude);
            Place(canvas, Bar(1.6, barHeight, EditorChrome.AccentBrush, 0.75), x + 4 + i * step, y + (height - barHeight) / 2);
        }
    }

    /// <summary>
    /// The horizontal subtitle grid of layout 14.
    /// </summary>
    private static void AddColumns(Canvas canvas, double x, double y, double width, double height)
    {
        const int columns = 5;
        for (var i = 1; i < columns; i++)
        {
            Place(canvas, Bar(1, height - 2, EditorChrome.FieldBorderBrush), x + width * i / columns, y + 1);
        }
    }

    /// <summary>
    /// The subtitle track of layout 14.
    /// </summary>
    private static void AddSegments(Canvas canvas, double x, double y, double width, double height)
    {
        double[] segments = [0.03, 0.17, 0.27, 0.5, 0.58, 0.72, 0.8, 0.96];
        for (var i = 0; i < segments.Length; i += 2)
        {
            Place(canvas, Bar((segments[i + 1] - segments[i]) * width, height - 3, EditorChrome.AccentBrush, 0.7), x + segments[i] * width, y + 1.5);
        }
    }

    private static Rectangle Bar(double width, double height, IBrush fill, double opacity = 1)
    {
        return new Rectangle { Width = width, Height = height, RadiusX = 1, RadiusY = 1, Fill = fill, Opacity = opacity };
    }

    private static void Place(Canvas canvas, Control child, double x, double y)
    {
        Canvas.SetLeft(child, x);
        Canvas.SetTop(child, y);
        canvas.Children.Add(child);
    }
}
