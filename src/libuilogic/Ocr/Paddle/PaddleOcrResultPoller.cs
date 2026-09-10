namespace Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;

/// <summary>
/// Watches the <c>--save_path</c> folder of a running PaddleOCR batch and hands each result
/// over as soon as its file is complete, in line order.
/// <para>
/// Polling rather than waiting for the process: the pip "paddleocr" launcher spawns a separate
/// worker process and can exit - or block - long before that worker finishes, so a run gated on
/// the launcher exiting reports the first couple of lines and abandons the rest. It also means
/// progress for every line instead of one update at the very end.
/// </para>
/// </summary>
public sealed class PaddleOcrResultPoller
{
    /// <summary>How often the folder is listed while the batch runs.</summary>
    public static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(400);

    /// <summary>Give up this long after the launcher exits with nothing new arriving.</summary>
    public static readonly TimeSpan IdleGraceAfterExit = TimeSpan.FromSeconds(60);

    /// <summary>Safety net while the launcher is still alive but has stopped producing.</summary>
    public static readonly TimeSpan IdleGraceWhileRunning = TimeSpan.FromMinutes(5);

    private readonly string _saveFolder;
    private readonly IReadOnlyList<string> _stemsInOrder;
    private readonly HashSet<string> _reported = new(StringComparer.Ordinal);

    /// <param name="saveFolder">The folder passed to PaddleOCR as <c>--save_path</c>.</param>
    /// <param name="stemsInOrder">
    /// Input file stems ("0000", "0001", ...) in the order results should be delivered. Results
    /// are handed over by position in this list, so a caller can index straight into its own
    /// per-line state.
    /// </param>
    public PaddleOcrResultPoller(string saveFolder, IReadOnlyList<string> stemsInOrder)
    {
        _saveFolder = saveFolder;
        _stemsInOrder = stemsInOrder;
    }

    /// <summary>Results handed over so far.</summary>
    public int ReportedCount => _reported.Count;

    /// <summary>True once every input has been reported.</summary>
    public bool IsComplete => _reported.Count >= _stemsInOrder.Count;

    /// <summary>
    /// Reports every result that has appeared since the last call, lowest line first. Files
    /// still being written are left for the next round.
    /// </summary>
    /// <param name="onResult">Called with the position in <c>stemsInOrder</c> and its regions.</param>
    /// <param name="onParseError">Called with (stem, message) when a result file cannot be parsed.</param>
    /// <returns>How many results this call handed over.</returns>
    public int ReportNew(
        Action<int, List<PaddleOcrTextRegion>> onResult, Action<string, string>? onParseError = null)
    {
        // One directory listing per poll instead of a File.Exists per outstanding image: at a
        // 400 ms poll over a feature-length subtitle the per-image form is thousands of stat
        // calls a second, nearly all of them for files that are not there yet.
        string[] resultFiles;
        try
        {
            resultFiles = Directory.GetFiles(_saveFolder, "*" + PaddleOcrResultJson.FileSuffix);
        }
        catch
        {
            return 0; // folder gone or not created yet - try again on the next poll
        }

        if (resultFiles.Length <= _reported.Count)
        {
            return 0; // nothing new on disk since the last poll
        }

        var writtenStems = new HashSet<string>(resultFiles.Length, StringComparer.Ordinal);
        foreach (var resultFile in resultFiles)
        {
            var name = Path.GetFileName(resultFile);
            writtenStems.Add(name.Substring(0, name.Length - PaddleOcrResultJson.FileSuffix.Length));
        }

        var handedOver = 0;
        for (var position = 0; position < _stemsInOrder.Count; position++)
        {
            var stem = _stemsInOrder[position];
            if (_reported.Contains(stem) || !writtenStems.Contains(stem))
            {
                continue;
            }

            string json;
            var jsonPath = Path.Combine(_saveFolder, stem + PaddleOcrResultJson.FileSuffix);
            try
            {
                json = File.ReadAllText(jsonPath);
            }
            catch
            {
                continue; // locked / mid-write - try again on the next poll
            }

            if (!PaddleOcrResultJson.IsComplete(json))
            {
                continue;
            }

            _reported.Add(stem);
            handedOver++;

            if (!PaddleOcrResultJson.TryParse(json, out var regions, out var error))
            {
                onParseError?.Invoke(stem, error ?? "unknown error");
            }

            onResult(position, regions);
        }

        return handedOver;
    }

    /// <summary>
    /// Polls until every result has been reported, results clearly stop arriving, or the run is
    /// cancelled. Returns when there is nothing left to wait for; the caller still has to wait
    /// for the process and decide what a short count means.
    /// </summary>
    /// <param name="hasProcessExited">
    /// Checked only when a round brings nothing new: once the launcher is gone the wait is cut
    /// from the long safety net to a short grace.
    /// </param>
    public async Task PollUntilDoneAsync(
        Func<bool> hasProcessExited,
        Action<int, List<PaddleOcrTextRegion>> onResult,
        Action<string, string>? onParseError,
        CancellationToken cancellationToken)
    {
        var idleRounds = 0;
        while (!IsComplete && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(PollInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (ReportNew(onResult, onParseError) > 0)
            {
                idleRounds = 0;
                continue;
            }

            idleRounds++;
            var grace = hasProcessExited() ? IdleGraceAfterExit : IdleGraceWhileRunning;
            if (idleRounds >= grace.TotalMilliseconds / PollInterval.TotalMilliseconds)
            {
                return;
            }
        }
    }
}
