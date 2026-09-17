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

    /// <summary>
    /// Called by change detection when it found an edit, right before it snapshots the state, so
    /// the client can normalize the edit (frame mode snaps re-timed lines to frames) and the undo
    /// step records the normalized state. <paramref name="lastRecorded"/> is the newest undo entry.
    /// </summary>
    void OnChangeDetected(UndoRedoItem? lastRecorded)
    {
    }
}
