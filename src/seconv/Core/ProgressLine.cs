namespace SeConv.Core;

/// <summary>
/// A single carriage-return-rewritten console line for long-running work (OCR, translate):
/// <c>"  OCR 12/345 (3%)..."</c>. Written with <see cref="Console"/> rather than
/// <c>AnsiConsole</c> because Spectre's markup writer does not rewrite a line on <c>\r</c>.
/// Callers suppress it in quiet/JSON mode (issue #14267: seconv used to print one line at
/// the start of an OCR run and then stay silent until it finished).
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
    /// Rewrites the current console line as <c>"  {label} {done}/{total} ({pct}%)..."</c>.
    /// The rendered text never shrinks between calls (counts and percent only grow), so the
    /// rewrite cannot leave stale characters behind.
    /// </summary>
    public static void Report(string label, int done, int total)
    {
        Console.Write($"\r  {label} {done}/{total} ({Percent(done, total)}%)...");
    }

    /// <summary>Ends a progress line so whatever prints next starts on a fresh line.</summary>
    public static void Finish()
    {
        Console.WriteLine();
    }
}
