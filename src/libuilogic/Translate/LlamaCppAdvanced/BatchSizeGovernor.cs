namespace Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

/// <summary>
/// Remembers the batch size that last worked in a translation run. Without it every batch starts
/// at the configured size again, so a model that cannot handle that size fails - and pays for a
/// retry and a cut - on every single batch. After a cut, batches use the smaller size; after
/// <see cref="SuccessesBeforeGrowing"/> clean batches in a row the size grows by half again,
/// up to the configured size, where the governor steps aside.
/// </summary>
internal sealed class BatchSizeGovernor
{
    internal const int SuccessesBeforeGrowing = 2;

    private int _workingSize; // 0 = no cut yet in this run: use the configured size
    private int _successes;

    public void Reset()
    {
        _workingSize = 0;
        _successes = 0;
    }

    public int Limit(int configuredSize)
    {
        return _workingSize > 0 ? Math.Min(_workingSize, configuredSize) : configuredSize;
    }

    public void OnCut(int nextSize)
    {
        _workingSize = _workingSize > 0 ? Math.Min(_workingSize, nextSize) : nextSize;
        _successes = 0;
    }

    public void OnSuccess()
    {
        if (_workingSize <= 0 || ++_successes < SuccessesBeforeGrowing)
        {
            return;
        }

        _workingSize += Math.Max(1, _workingSize / 2);
        _successes = 0;
    }
}
