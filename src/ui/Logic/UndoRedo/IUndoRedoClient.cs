namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public interface IUndoRedoClient
{
    int GetFastHash();
    UndoRedoItem MakeUndoRedoObject(string description);
    bool IsTyping();
}
