using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public class UndoRedoItem
{
    public string Description { get; set; }
    public SubtitleLineViewModel[] Subtitles { get; set; }
    public string SubtitleFileName { get; set; }
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
            item.SelectionLength);
    }
}
