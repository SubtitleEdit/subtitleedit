using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public class UndoRedoItem
{
    public string Description { get; set; }
    public SubtitleLineViewModel[] Subtitles { get; set; }
    public string SubtitleFileName { get; set; }
    public string? SelectedEncodingDisplayName { get; set; }
    public string? SubtitleHeader { get; set; }
    public string? SubtitleFooter { get; set; }

    // The original-subtitle file-level state must be part of the snapshot: the undo
    // hash covers it (GetFastHashOriginal), so if restore leaves it untouched the
    // restored state can never hash-match its own entry, and the next Undo() treats
    // the mismatch as unrecorded changes and clears the redo timeline (#12952).
    public string? SubtitleFileNameOriginal { get; set; }
    public string? SubtitleHeaderOriginal { get; set; }
    public string? SubtitleFooterOriginal { get; set; }
    public int[] SelectedLines { get; set; }
    public int CaretIndex { get; set; }
    public int SelectionLength { get; set; }
    public DateTime Created { get; set; }
    public int Hash { get; set; }

    public UndoRedoItem(
        string description,
        SubtitleLineViewModel[] subtitles,
        int hash,
        string? subtitleFileName,
        int[] selectedLines,
        int caretIndex,
        int selectionLength)
    {
        Description = description;
        Subtitles = subtitles;
        SubtitleFileName = subtitleFileName ?? string.Empty;
        SelectedLines = selectedLines;
        CaretIndex = caretIndex;
        SelectionLength = selectionLength;
        Created = DateTime.Now;
        Hash = hash;
    }

    public static UndoRedoItem? Clone(UndoRedoItem? item)
    {
        if (item == null)
        {
            return null;
        }

        return new UndoRedoItem(
            item.Description,
            item.Subtitles.Select(p => new SubtitleLineViewModel(p, false)).ToArray(),
            item.Hash,
            item.SubtitleFileName,
            item.SelectedLines,
            item.CaretIndex,
            item.SelectionLength)
        {
            SelectedEncodingDisplayName = item.SelectedEncodingDisplayName,
            SubtitleHeader = item.SubtitleHeader,
            SubtitleFooter = item.SubtitleFooter,
            SubtitleFileNameOriginal = item.SubtitleFileNameOriginal,
            SubtitleHeaderOriginal = item.SubtitleHeaderOriginal,
            SubtitleFooterOriginal = item.SubtitleFooterOriginal,
            // Preserve the original timestamp — every Clone() used to overwrite
            // Created with DateTime.Now via the constructor, so any UI that
            // displays Created (or any logic that relies on the chronological
            // order) would see undo history dates marching forward on each
            // Undo/Redo round-trip.
            Created = item.Created,
        };
    }
}
