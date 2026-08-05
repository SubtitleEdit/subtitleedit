namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public interface IUndoRedoClient
{
    int GetFastHash();
    UndoRedoItem MakeUndoRedoObject(string description);
    /// <summary>
    /// True while a continuous edit is in progress (typing, dragging time codes in the
    /// waveform). Change detection skips those ticks and captures the settled state instead.
    /// </summary>
    bool IsUserEditing();
}
