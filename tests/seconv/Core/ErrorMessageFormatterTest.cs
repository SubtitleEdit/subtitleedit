using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Tests for ErrorMessageFormatter — added alongside the seconv catch-site enrichment
/// so the InnerException-chain unwrapping (issue #11314 follow-up) is locked in.
/// </summary>
public class ErrorMessageFormatterTest
{
    [Fact]
    public void FormatForUser_NoInnerException_ReturnsBareMessage()
    {
        var ex = new InvalidOperationException("Boom");

        var formatted = ErrorMessageFormatter.FormatForUser(ex, verbose: false);

        // Trailing period would have been stripped if present; raw message has none, so
        // the output is unchanged.
        Assert.Equal("Boom", formatted);
    }

    [Fact]
    public void FormatForUser_TypeInitializerWithInner_SurfacesRootCause()
    {
        // Mirrors the issue #11314 shape: outer TypeInitializationException whose
        // .Message is opaque ("The type initializer for 'X' threw an exception."), with
        // the actual root cause one level deeper. The formatter must surface that root.
        var inner = new InvalidOperationException("Unable to load default typeface.");
        var outer = new TypeInitializationException(
            "Nikse.SubtitleEdit.Core.Common.TextSplitResult",
            inner);

        var formatted = ErrorMessageFormatter.FormatForUser(outer, verbose: false);

        Assert.Contains("The type initializer", formatted);
        Assert.Contains("Unable to load default typeface", formatted);
        Assert.Contains(" → ", formatted);
    }

    [Fact]
    public void FormatForUser_NestedChain_IncludesAllLayers()
    {
        var innermost = new ArgumentNullException("path");
        var middle = new InvalidOperationException("Wrapper", innermost);
        var outer = new TypeInitializationException("Some.Type", middle);

        var formatted = ErrorMessageFormatter.FormatForUser(outer, verbose: false);

        // All three layers' messages should appear, separated by the arrow.
        Assert.Contains("The type initializer", formatted);
        Assert.Contains("Wrapper", formatted);
        Assert.Contains("path", formatted);
        // Exactly two separators for a three-level chain.
        Assert.Equal(2, CountOccurrences(formatted, " → "));
    }

    [Fact]
    public void FormatForUser_Verbose_IncludesStackTrace()
    {
        Exception caught;
        try
        {
            throw new InvalidOperationException("Verbose test");
        }
        catch (Exception ex)
        {
            caught = ex;
        }

        var formatted = ErrorMessageFormatter.FormatForUser(caught, verbose: true);

        // Verbose mode delegates to ex.ToString() which prefixes the type and includes
        // a stack frame from this test method.
        Assert.Contains("InvalidOperationException", formatted);
        Assert.Contains("Verbose test", formatted);
        Assert.Contains(nameof(FormatForUser_Verbose_IncludesStackTrace), formatted);
    }

    [Fact]
    public void FormatForUser_TrimsTrailingPeriodsInChain()
    {
        // Real-world wrapper exceptions (TypeInitializationException's own message ends
        // with ".") would otherwise produce "...threw an exception. → cause", which is
        // visually clunky. The formatter trims the dot before joining.
        var inner = new InvalidOperationException("cause text");
        var outer = new InvalidOperationException("first layer.", inner);

        var formatted = ErrorMessageFormatter.FormatForUser(outer, verbose: false);

        Assert.Equal("first layer → cause text", formatted);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        if (string.IsNullOrEmpty(needle))
        {
            return 0;
        }
        var count = 0;
        var index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }
}
