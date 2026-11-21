using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Ytt;

/// <summary>
/// A pen in the YouTube timed text styles window. Each optional attribute has its own "use" switch:
/// an attribute that is left out means the player default, which the viewer can change.
/// </summary>
public partial class YttPenDisplay : ObservableObject
{
    public int Id { get; }

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private string _name;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private bool _bold;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private bool _italic;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private bool _underline;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private bool _useForeColor;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private Color _foreColor;
    [ObservableProperty] private decimal _foreOpacity;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private bool _useBackColor;
    [ObservableProperty] private Color _backColor;
    [ObservableProperty] private decimal _backOpacity;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private int _edgeTypeIndex;
    [ObservableProperty] private bool _useEdgeColor;
    [ObservableProperty] private Color _edgeColor;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private int _fontStyleIndex;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(RealSizeText), nameof(Description))] private decimal _size;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private int _offsetIndex;
    [ObservableProperty] private int _usageCount;

    private readonly YttPen _original;

    public YttPenDisplay(YttPen pen)
    {
        _original = pen.Clone();
        Id = pen.Id;
        _name = pen.Name ?? string.Empty;
        _bold = pen.Bold == true;
        _italic = pen.Italic == true;
        _underline = pen.Underline == true;
        _useForeColor = pen.ForeColor != null || pen.ForeOpacity != null;
        _foreColor = ParseColor(pen.ForeColor, YttPen.DefaultForeColor);
        _foreOpacity = pen.ForeOpacity ?? YttPen.DefaultForeOpacity;
        _useBackColor = pen.BackColor != null || pen.BackOpacity != null;
        _backColor = ParseColor(pen.BackColor, YttPen.DefaultBackColor);
        _backOpacity = pen.BackOpacity ?? YttPen.DefaultBackOpacity;
        _edgeTypeIndex = pen.EdgeType is >= 1 and <= 4 ? pen.EdgeType.Value : 0;
        _useEdgeColor = pen.EdgeColor != null;
        _edgeColor = ParseColor(pen.EdgeColor, YttPen.DefaultEdgeColor);
        _fontStyleIndex = pen.FontStyle is >= 0 and <= 7 ? pen.FontStyle.Value : 0;
        _size = pen.Size ?? 100;
        _offsetIndex = pen.Offset is >= 0 and <= 2 ? pen.Offset.Value : 1;
    }

    public string IdText => Id.ToString(CultureInfo.InvariantCulture);

    public string RealSizeText => string.Format(Se.Language.File.Ytt.RealSizeX, Math.Round(YttPen.GetRealScale((int)Size) * 100));

    /// <summary>One-line summary for the list.</summary>
    public string Description
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                parts.Add(Name.Trim());
            }

            if (Bold)
            {
                parts.Add(Se.Language.General.Bold);
            }

            if (Italic)
            {
                parts.Add(Se.Language.General.Italic);
            }

            if (Underline)
            {
                parts.Add(Se.Language.General.Underline);
            }

            if (UseForeColor)
            {
                parts.Add(ToHtml(ForeColor));
            }

            if (UseBackColor)
            {
                parts.Add(Se.Language.File.Ytt.BackgroundColor.ToLowerInvariant());
            }

            if (EdgeTypeIndex > 0)
            {
                parts.Add(YttStylesViewModel.EdgeTypeNames[EdgeTypeIndex].ToLowerInvariant());
            }

            if (FontStyleIndex > 0)
            {
                parts.Add(YttPen.FontStyleNames[FontStyleIndex]);
            }

            if ((int)Size != 100)
            {
                parts.Add("sz " + ((int)Size).ToString(CultureInfo.InvariantCulture));
            }

            if (OffsetIndex != 1)
            {
                parts.Add(YttStylesViewModel.OffsetNames[OffsetIndex].ToLowerInvariant());
            }

            return parts.Count == 0 ? Se.Language.File.Ytt.Default : string.Join(", ", parts);
        }
    }

    public YttPen ToPen()
    {
        var pen = _original.Clone();
        pen.Name = string.IsNullOrWhiteSpace(Name) ? null : Name.Trim();
        pen.Bold = Bold ? true : _original.Bold == false ? false : null;
        pen.Italic = Italic ? true : _original.Italic == false ? false : null;
        pen.Underline = Underline ? true : _original.Underline == false ? false : null;
        pen.ForeColor = UseForeColor ? ToHtml(ForeColor) : null;
        pen.ForeOpacity = UseForeColor ? (int)ForeOpacity : null;
        pen.BackColor = UseBackColor ? ToHtml(BackColor) : null;
        pen.BackOpacity = UseBackColor ? (int)BackOpacity : null;
        pen.EdgeType = EdgeTypeIndex > 0 ? EdgeTypeIndex : null;
        pen.EdgeColor = EdgeTypeIndex > 0 && UseEdgeColor ? ToHtml(EdgeColor) : null;
        pen.FontStyle = FontStyleIndex > 0 ? FontStyleIndex : _original.FontStyle == 0 ? 0 : null;
        pen.Size = (int)Size != 100 || _original.Size != null ? (int)Size : null;
        pen.Offset = OffsetIndex != 1 || _original.Offset != null ? OffsetIndex : null;
        return pen;
    }

    /// <summary>
    /// Pen number and description for a menu item. Underscores are doubled, else Avalonia takes
    /// the first one in a pen name as an access key and hides it.
    /// </summary>
    public string MenuHeader => (string.Format(Se.Language.File.Ytt.PenX, Id) + " - " + Description).Replace("_", "__");

    public YttPenDisplay CloneWithId(int id) => CloneWithId(id, Array.Empty<string>());

    /// <summary>
    /// A copy with a new id. A named pen gets "(Copy)", "(Copy 2)", ... - the first one not in
    /// <paramref name="takenNames"/> - so copies of copies don't stack "(Copy) (Copy)".
    /// An unnamed pen stays unnamed.
    /// </summary>
    public YttPenDisplay CloneWithId(int id, IEnumerable<string?> takenNames)
    {
        var pen = ToPen();
        pen.Id = id;

        if (pen.Name != null)
        {
            pen.Name = MakeCopyName(pen.Name, takenNames);
        }

        return new YttPenDisplay(pen);
    }

    internal static string MakeCopyName(string name, IEnumerable<string?> takenNames)
    {
        var copy = Se.Language.General.Copy;
        var match = Regex.Match(name, "^(.*) \\(" + Regex.Escape(copy) + "(?: \\d+)?\\)$");
        var baseName = match.Success && match.Groups[1].Value.Length > 0 ? match.Groups[1].Value : name;
        var taken = new HashSet<string>(takenNames.OfType<string>().Select(n => n.Trim()), StringComparer.Ordinal);
        var candidate = baseName + " (" + copy + ")";
        for (var i = 2; taken.Contains(candidate); i++)
        {
            candidate = baseName + " (" + copy + " " + i.ToString(CultureInfo.InvariantCulture) + ")";
        }

        return candidate;
    }

    internal static Color ParseColor(string? html, string fallback)
    {
        return Color.TryParse(html ?? fallback, out var color) ? color : Color.Parse(fallback);
    }

    internal static string ToHtml(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}

/// <summary>A window position (wp) in the YouTube timed text styles window.</summary>
public partial class YttPositionDisplay : ObservableObject
{
    public int Id { get; }

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description), nameof(RealPositionText))] private int _anchorPointIndex;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description), nameof(RealPositionText))] private decimal _horizontal;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description), nameof(RealPositionText))] private decimal _vertical;
    [ObservableProperty] private int _usageCount;

    private readonly YttWindowPosition _original;

    public YttPositionDisplay(YttWindowPosition wp)
    {
        _original = wp.Clone();
        Id = wp.Id;
        _anchorPointIndex = wp.AnchorPoint is >= 0 and <= 8 ? wp.AnchorPoint.Value : YttWindowPosition.DefaultAnchorPoint;
        _horizontal = wp.Horizontal ?? YttWindowPosition.DefaultHorizontal;
        _vertical = wp.Vertical ?? YttWindowPosition.DefaultVertical;
    }

    public string IdText => Id.ToString(CultureInfo.InvariantCulture);

    public string Description => $"{YttStylesViewModel.AnchorPointNames[AnchorPointIndex]}, {Horizontal:0}% / {Vertical:0}%";

    public string RealPositionText => string.Format(CultureInfo.CurrentCulture, Se.Language.File.Ytt.RealPositionXY,
        YttWindowPosition.GetRealPercent((int)Horizontal), YttWindowPosition.GetRealPercent((int)Vertical));

    public YttWindowPosition ToWindowPosition()
    {
        var wp = _original.Clone();
        wp.AnchorPoint = AnchorPointIndex;
        wp.Horizontal = (int)Horizontal;
        wp.Vertical = (int)Vertical;
        return wp;
    }

    public YttPositionDisplay CloneWithId(int id)
    {
        var wp = ToWindowPosition();
        wp.Id = id;
        return new YttPositionDisplay(wp);
    }
}

/// <summary>A window style (ws) in the YouTube timed text styles window.</summary>
public partial class YttWindowStyleDisplay : ObservableObject
{
    public int Id { get; }

    /// <summary>0 player default, then ju 0 (left), 1 (right), 2 (center).</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private int _justifyIndex;

    /// <summary>0 player default, then pd 0-3.</summary>
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Description))] private int _printDirectionIndex;

    [ObservableProperty] private bool _reverseColumns;
    [ObservableProperty] private bool _useWindowOpacity;
    [ObservableProperty] private decimal _windowOpacity;
    [ObservableProperty] private int _usageCount;

    private readonly YttWindowStyle _original;

    public YttWindowStyleDisplay(YttWindowStyle ws)
    {
        _original = ws.Clone();
        Id = ws.Id;
        _justifyIndex = ws.Justify is >= 0 and <= 2 ? ws.Justify.Value + 1 : 0;
        _printDirectionIndex = ws.PrintDirection is >= 0 and <= 3 ? ws.PrintDirection.Value + 1 : 0;
        _reverseColumns = ws.ScrollDirection == 1;
        _useWindowOpacity = ws.WindowOpacity != null;
        _windowOpacity = ws.WindowOpacity ?? 0;
    }

    public string IdText => Id.ToString(CultureInfo.InvariantCulture);

    public string Description => $"{YttStylesViewModel.JustifyNames[JustifyIndex]}, {YttStylesViewModel.PrintDirectionNames[PrintDirectionIndex]}";

    public YttWindowStyle ToWindowStyle()
    {
        var ws = _original.Clone();
        ws.Justify = JustifyIndex > 0 ? JustifyIndex - 1 : null;
        ws.PrintDirection = PrintDirectionIndex > 0 ? PrintDirectionIndex - 1 : null;
        ws.ScrollDirection = ReverseColumns ? 1 : _original.ScrollDirection == null ? null : 0;
        ws.WindowOpacity = UseWindowOpacity ? (int)WindowOpacity : null;
        return ws;
    }

    public YttWindowStyleDisplay CloneWithId(int id)
    {
        var ws = ToWindowStyle();
        ws.Id = id;
        return new YttWindowStyleDisplay(ws);
    }
}
