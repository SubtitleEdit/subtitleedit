using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public interface IUndoRedoManager : IDisposable
{
    IReadOnlyList<UndoRedoItem> UndoList { get; }
    IReadOnlyList<UndoRedoItem> RedoList { get; }

    void Do(UndoRedoItem action);
    UndoRedoItem? Undo();
    UndoRedoItem? Redo();

    bool CanUndo { get; }
    bool CanRedo { get; }

    int UndoCount { get; }
    int RedoCount { get; }

    void StartChangeDetection();
    void StopChangeDetection();

    /// <summary>
    /// Holds change detection off until <see cref="ResumeChangeDetection"/>, even if something calls
    /// <see cref="StartChangeDetection"/> in between - so a run of several commands, each of which
    /// stops and restarts detection itself, still becomes a single undo step.
    /// </summary>
    void SuspendChangeDetection();
    void ResumeChangeDetection();
    void SetupChangeDetection(IUndoRedoClient hashProvider, TimeSpan? interval = null);
    void CheckForChanges(object? state);
    void Reset();
}