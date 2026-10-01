using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public enum CompareRowKind
{
    Same,
    Changed,
    NumberOnly,
    OnlyLeft,
    OnlyRight,
}

/// <summary>
/// One aligned pair of the merge view: the current line on the left, its reference twin on the
/// right, drawn as a single list row. Keeping both halves in one row is what keeps them aligned -
/// the two used to be separate grids that had to be scrolled in step and still drifted apart as
/// soon as one side wrapped to two lines and the other did not (#13504).
/// </summary>
public partial class CompareRow : ObservableObject
{
    private static readonly IBrush Transparent = new ImmutableSolidColorBrush(Colors.Transparent);

    public CompareItem Left { get; }
    public CompareItem Right { get; }
    public CompareRowKind Kind { get; }

    /// <summary>The current line was changed inside Compare (and not undone).</summary>
    public bool IsEdited { get; }

    /// <summary>The current side is the editor's own subtitle, so it can be changed here.</summary>
    public bool IsLeftEditable { get; }

    /// <summary>The pair was forced together by the user's sync point (#15394).</summary>
    public bool IsSyncPoint { get; init; }

    /// <summary>The line on that side was picked as half of a sync point, waiting for the other half.</summary>
    [ObservableProperty] private bool _isLeftSyncPending;
    [ObservableProperty] private bool _isRightSyncPending;

    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private string _editText = string.Empty;
    [ObservableProperty] private TimeSpan _editStart;
    [ObservableProperty] private TimeSpan _editEnd;

    public CompareRow(CompareItem left, CompareItem right, CompareRowKind kind, bool isEdited, bool isLeftEditable)
    {
        Left = left;
        Right = right;
        Kind = kind;
        IsEdited = isEdited;
        IsLeftEditable = isLeftEditable;
    }

    public bool HasLeft => !Left.IsDefault;
    public bool HasRight => !Right.IsDefault;
    public bool IsMissingLeft => !HasLeft && HasRight;
    public bool IsMissingRight => HasLeft && !HasRight;

    public bool CanTakeReference => IsLeftEditable && (Kind == CompareRowKind.Changed || Kind == CompareRowKind.OnlyRight);
    public bool CanDeleteCurrent => IsLeftEditable && Kind == CompareRowKind.OnlyLeft;
    public bool HasGutterAction => CanTakeReference || CanDeleteCurrent;
    public bool CanEdit => IsLeftEditable && HasLeft && Left.Line != null;
    public bool CanTakeFromPair => CanEdit && HasRight && Right.Line != null;

    public string TakeReferenceHint => Kind == CompareRowKind.OnlyRight
        ? Se.Language.File.CompareInsertFromReference
        : Se.Language.File.CompareTakeFromReference;

    /// <summary>Background of a line card: the pale variant of the difference color, so the full-strength cells stand out on it.</summary>
    public IBrush LeftCardBrush => GetCardBrush(HasLeft);
    public IBrush RightCardBrush => GetCardBrush(HasRight);

    /// <summary>The band between the two cards that ties a differing pair together.</summary>
    public IBrush GutterBrush => Kind switch
    {
        CompareRowKind.Changed => CompareColors.TextOrTimeDifferenceRow,
        CompareRowKind.NumberOnly => CompareColors.NumberDifferenceRow,
        CompareRowKind.OnlyLeft or CompareRowKind.OnlyRight => CompareColors.OnlyInOneFileRow,
        _ => Transparent,
    };

    public IBrush EditedBrush => IsEdited ? CompareColors.Edited : Transparent;

    public string LeftDurationDisplay => FormatDuration(Left);
    public string RightDurationDisplay => FormatDuration(Right);

    private IBrush GetCardBrush(bool hasLine)
    {
        return Kind switch
        {
            CompareRowKind.Changed => CompareColors.TextOrTimeDifferenceSoft,
            CompareRowKind.NumberOnly => CompareColors.NumberDifferenceSoft,
            CompareRowKind.OnlyLeft or CompareRowKind.OnlyRight => hasLine ? CompareColors.OnlyInOneFileSoft : Transparent,
            _ => Transparent,
        };
    }

    internal static string FormatDuration(CompareItem item)
    {
        if (item.IsDefault)
        {
            return string.Empty;
        }

        return (item.EndTime - item.StartTime).TotalSeconds.ToString("0.00", CultureInfo.CurrentCulture) + "s";
    }

    internal void BeginEdit()
    {
        if (Left.Line == null)
        {
            return;
        }

        EditText = Left.Line.Text;
        EditStart = Left.Line.StartTime;
        EditEnd = Left.Line.EndTime;
        IsEditing = true;
    }
}
