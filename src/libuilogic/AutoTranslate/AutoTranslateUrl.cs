using System;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// Completes an API url that only names the service base ("https://api.deepseek.com") to the
    /// engine's real endpoint ("https://api.deepseek.com/chat/completions").
    /// <para>
    /// Vendors document the base url for OpenAI-SDK use, so that is what gets pasted into the url
    /// box - and every request then hits the origin root, which answers 404 with nothing pointing
    /// at the url as the culprit (#13044).
    /// </para>
    /// <para>
    /// A url with a path of its own is left alone: only a bare origin, or a leading part of the
    /// default endpoint path ("/v1", "/openai/v1"), is completed. That keeps custom endpoints
    /// (llama.cpp's native "/completion", proxies with their own routes) working as typed.
    /// </para>
    /// </summary>
    public static class AutoTranslateUrl
    {
        public static string Complete(string? url, string? defaultUrl)
        {
            var trimmed = (url ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                return (defaultUrl ?? string.Empty).Trim().TrimEnd('/');
            }

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
                !Uri.TryCreate((defaultUrl ?? string.Empty).Trim(), UriKind.Absolute, out var defaultUri))
            {
                return trimmed.TrimEnd('/');
            }

            if (uri.Query.Length > 0 || uri.Fragment.Length > 0)
            {
                return trimmed;
            }

            var path = uri.AbsolutePath.Trim('/');
            var defaultPath = defaultUri.AbsolutePath.Trim('/');
            if (defaultPath.Length == 0)
            {
                return trimmed.TrimEnd('/');
            }

            // Only complete an empty path or a proper prefix of the default path - anything else
            // is a deliberate endpoint choice.
            if (path.Length > 0 && !defaultPath.StartsWith(path + "/", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.TrimEnd('/');
            }

            return uri.GetLeftPart(UriPartial.Authority) + "/" + defaultPath;
        }
    }
}
