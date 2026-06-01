using System.Text;

namespace SeConv.Core;

/// <summary>
/// Renders an <see cref="Exception"/> as a user-facing error message. Wrapper
/// exceptions like <see cref="TypeInitializationException"/> have <c>.Message</c>
/// values that hide the real cause ("The type initializer for 'X' threw an exception."),
/// so by default we walk the <see cref="Exception.InnerException"/> chain and join
/// the messages. With <c>--verbose</c> the full <see cref="Exception.ToString()"/>
/// — including stack traces — is emitted.
///
/// Issue #11314 was the trigger: a static field initializer in libse threw and
/// users only saw the outer message, hiding the actual SkiaSharp font-discovery
/// failure that was the root cause. This formatter makes those failures actionable.
/// </summary>
internal static class ErrorMessageFormatter
{
    /// <summary>
    /// Formats <paramref name="ex"/> for surfacing to the user. When
    /// <paramref name="verbose"/> is true returns the full <c>ToString()</c>
    /// (stack traces and all); otherwise returns the message chain joined with
    /// " → ".
    /// </summary>
    public static string FormatForUser(Exception ex, bool verbose)
    {
        if (verbose)
        {
            return ex.ToString();
        }

        var sb = new StringBuilder();
        var current = ex;
        var depth = 0;
        // Hard cap on chain depth — pathological exception chains (cyclical
        // wrappers etc.) shouldn't make us spin or produce kilobyte-long messages.
        while (current != null && depth < 8)
        {
            if (sb.Length > 0)
            {
                sb.Append(" → ");
            }
            // Trim trailing periods so the joined chain reads cleanly when each
            // wrapper's Message ends in one ("The type initializer for X threw an
            // exception." → "Unable to load font.").
            sb.Append(current.Message.TrimEnd('.'));
            current = current.InnerException;
            depth++;
        }
        return sb.ToString();
    }
}
