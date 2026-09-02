using System;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Builds the URL a "search via" slot opens: the slot's URL template with the searched text put
/// where "{0}" is (SE 4's placeholder). Returns null for anything that must not be launched.
/// </summary>
public static class CustomSearchUrlBuilder
{
    public static string? Build(string? urlTemplate, string? text)
    {
        if (string.IsNullOrWhiteSpace(urlTemplate) || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // A two-line subtitle is one phrase to search for - line breaks would otherwise travel as
        // %0A and split the query.
        var query = Uri.EscapeDataString(text.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ').Trim());

        // "{0}" marks where the searched text goes; a URL without it gets the text appended, which
        // is what a pasted search URL ending in "?q=" means. Replace rather than string.Format:
        // a stray brace in a URL must not throw.
        var url = urlTemplate.Trim();
        url = url.Contains("{0}", StringComparison.Ordinal)
            ? url.Replace("{0}", query, StringComparison.Ordinal)
            : url + query;

        // The URL ends up at the shell (UiUtil.OpenUrl), and it comes from a settings file that may
        // have been imported - so only http(s) is ever launched, never a local file or "file:"/
        // "javascript:" and friends.
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        return url;
    }
}
