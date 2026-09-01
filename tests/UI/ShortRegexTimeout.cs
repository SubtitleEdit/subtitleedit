using Nikse.SubtitleEdit.Core.Common;
using System;

namespace UITests;

/// <summary>
/// Shortens <see cref="RegexUtils.UserPatternMatchTimeout"/> for the tests that feed find/replace a
/// catastrophically backtracking pattern and check it gives up.
/// </summary>
/// <remarks>
/// Those tests do not care how long the give-up takes - only that the call comes back at all, with
/// "no match" - but they were paying the shipped five seconds each. Fourteen of them added up to
/// roughly half the wall clock of the whole UI suite. A quarter of a second still needs orders of
/// magnitude more backtracking than the pattern can do, so the tests prove the same thing.
/// </remarks>
public sealed class ShortRegexTimeout : IDisposable
{
    private readonly TimeSpan _previous = RegexUtils.UserPatternMatchTimeout;

    public ShortRegexTimeout()
    {
        RegexUtils.UserPatternMatchTimeout = TimeSpan.FromMilliseconds(250);
    }

    public void Dispose()
    {
        RegexUtils.UserPatternMatchTimeout = _previous;
    }
}
