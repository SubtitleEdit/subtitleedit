namespace SeConv.Core;

/// <summary>
/// Progress for long-running work (OCR, translate), reported as a count of finished items
/// plus a percentage (issue #14267: seconv used to print one line at the start of an OCR run
/// and then stay silent until it finished). Callers suppress it in quiet/JSON mode.
///
/// On a terminal this rewrites a single line with <c>\r</c> - <c>"  OCR 12/345 (3%)..."</c> -
/// using <see cref="Console"/> rather than <c>AnsiConsole</c>, since Spectre's markup writer
/// does not rewrite a line on <c>\r</c>. When stdout is redirected (a pipe, a log file, a CI
/// job) a rewritten line would collapse into one enormous line, so progress falls back to
/// plain newline-terminated lines at every 10% instead - still parseable by a caller watching
/// the pipe, which is the whole point of the request.
/// </summary>
internal static class ProgressLine
{
    /// <summary>
    /// Percent complete as a whole number. Floored, so 100% shows only when the last item is
    /// done, and clamped so a bad count cannot print a negative or above-100 percentage.
    /// Returns 0 when there is nothing to do.
    /// </summary>
    public static int Percent(int done, int total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return (int)Math.Clamp(done * 100L / total, 0, 100);
    }

    /// <summary>
    /// Forces terminal (true) or redirected (false) rendering; null = ask
    /// <see cref="Console.IsOutputRedirected"/>. For tests only: a test runner redirects the
    /// process's stdout, and <see cref="Console.SetOut"/> does not change that, so without
    /// this seam only the redirected form would ever be testable.
    /// </summary>
    internal static bool? InteractiveOverride { get; set; }

    private static bool IsInteractive => InteractiveOverride ?? !Console.IsOutputRedirected;

    /// <summary>
    /// Reports <paramref name="done"/> of <paramref name="total"/> items finished.
    /// On a terminal every call rewrites the line; when redirected only calls that cross a
    /// 10% boundary print, so a 5000-image run leaves ten lines in the log rather than 5000.
    /// </summary>
    public static void Report(string label, int done, int total)
    {
        if (IsInteractive)
        {
            // The rendered text never shrinks between calls (the counts and the percent only
            // grow), so the rewrite cannot leave stale characters of a longer line behind.
            Console.Write($"\r  {label} {done}/{total} ({Percent(done, total)}%)...");
            return;
        }

        // Stateless milestone test: print only when this item pushed the percentage into a
        // new group of ten, which also makes the final item (100%) always print.
        if (Percent(done, total) / 10 != Percent(done - 1, total) / 10)
        {
            Console.WriteLine($"  {label} {done}/{total} ({Percent(done, total)}%)...");
        }
    }

    /// <summary>Ends a progress line so whatever prints next starts on a fresh line.</summary>
    public static void Finish()
    {
        if (IsInteractive)
        {
            Console.WriteLine();
        }
    }
}
