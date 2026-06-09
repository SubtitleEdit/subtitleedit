namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Separates the user-visible fix action label from an internal variant key so the UI can
    /// group/sort equivalent actions while the apply/selection pipeline still distinguishes them.
    /// </summary>
    public static class FixActionKey
    {
        private const char Separator = '\u001F';

        public static string Create(string actionDisplay, string variant)
        {
            if (string.IsNullOrEmpty(variant))
            {
                return actionDisplay ?? string.Empty;
            }

            return (actionDisplay ?? string.Empty) + Separator + variant;
        }

        public static string GetDisplay(string actionKey)
        {
            if (string.IsNullOrEmpty(actionKey))
            {
                return string.Empty;
            }

            var separatorIndex = actionKey.IndexOf(Separator);
            if (separatorIndex >= 0)
            {
                return actionKey.Substring(0, separatorIndex);
            }

            return actionKey;
        }
    }
}
