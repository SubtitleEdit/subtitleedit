using System.Collections.Generic;

namespace Nikse.SubtitleEdit.UiLogic.AutoTranslate
{
    /// <summary>
    /// The last-used auto-translate engine is persisted by its display name, so renaming an
    /// engine silently drops every user who had it selected back to the default. This maps the
    /// names older settings files may hold to the names the engines carry now.
    /// </summary>
    public static class AutoTranslateEngineNames
    {
        private static readonly Dictionary<string, string> LegacyNames = new()
        {
            { "API-Route", ApiRouteTranslate.StaticName },
        };

        /// <summary>
        /// The current display name for a stored engine name - the stored value itself when it
        /// was never renamed.
        /// </summary>
        public static string FromStored(string? storedName)
        {
            if (string.IsNullOrEmpty(storedName))
            {
                return string.Empty;
            }

            return LegacyNames.TryGetValue(storedName, out var current) ? current : storedName;
        }
    }
}
