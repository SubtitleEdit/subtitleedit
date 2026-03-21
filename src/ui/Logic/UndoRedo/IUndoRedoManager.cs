using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public interface IUndoRedoManager : IDisposable
{
    List<UndoRedoItem> UndoList { get; }
    List<UndoRedoItem> RedoList { get; }

    void Do(UndoRedoItem action);
    UndoRedoItem? Undo();
    UndoRedoItem? Redo();

    bool CanUndo { get; }
    bool CanRedo { get; }

    int UndoCount { get; }
    int RedoCount { get; }

    void StartChangeDetection();
    void StopChangeDetection();
    void SetupChangeDetection(IUndoRedoClient hashProvider, TimeSpan? interval = null);
    void CheckForChanges(object? state);
    void Reset();
}