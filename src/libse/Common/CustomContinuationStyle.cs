using Nikse.SubtitleEdit.Core.Settings;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// The user-defined ("Custom") continuation style: the prefix/suffix added to a subtitle that
    /// continues into the next one, plus an optional different style after a long gap. Stored per
    /// rule profile and mirrored onto the active <see cref="GeneralSettings"/> for the libse engine.
    /// </summary>
    public class CustomContinuationStyle
    {
        public int Pause { get; set; }
        public string Suffix { get; set; }
        public bool SuffixApplyIfComma { get; set; }
        public bool SuffixAddSpace { get; set; }
        public bool SuffixReplaceComma { get; set; }
        public string Prefix { get; set; }
        public bool PrefixAddSpace { get; set; }
        public bool UseDifferentStyleGap { get; set; }
        public string GapSuffix { get; set; }
        public bool GapSuffixApplyIfComma { get; set; }
        public bool GapSuffixAddSpace { get; set; }
        public bool GapSuffixReplaceComma { get; set; }
        public string GapPrefix { get; set; }
        public bool GapPrefixAddSpace { get; set; }

        public CustomContinuationStyle()
        {
            Pause = 300;
            Suffix = string.Empty;
            SuffixApplyIfComma = false;
            SuffixAddSpace = false;
            SuffixReplaceComma = false;
            Prefix = string.Empty;
            PrefixAddSpace = false;
            UseDifferentStyleGap = true;
            GapSuffix = "...";
            GapSuffixApplyIfComma = true;
            GapSuffixAddSpace = false;
            GapSuffixReplaceComma = true;
            GapPrefix = "...";
            GapPrefixAddSpace = false;
        }

        // Chains the default constructor so a null argument yields a valid defaulted instance
        // rather than throwing (old settings files may omit the object entirely).
        public CustomContinuationStyle(CustomContinuationStyle other) : this()
        {
            if (other == null)
            {
                return;
            }

            Pause = other.Pause;
            Suffix = other.Suffix;
            SuffixApplyIfComma = other.SuffixApplyIfComma;
            SuffixAddSpace = other.SuffixAddSpace;
            SuffixReplaceComma = other.SuffixReplaceComma;
            Prefix = other.Prefix;
            PrefixAddSpace = other.PrefixAddSpace;
            UseDifferentStyleGap = other.UseDifferentStyleGap;
            GapSuffix = other.GapSuffix;
            GapSuffixApplyIfComma = other.GapSuffixApplyIfComma;
            GapSuffixAddSpace = other.GapSuffixAddSpace;
            GapSuffixReplaceComma = other.GapSuffixReplaceComma;
            GapPrefix = other.GapPrefix;
            GapPrefixAddSpace = other.GapPrefixAddSpace;
        }

        /// <summary>
        /// Reads the custom continuation style out of the flat fields on a <see cref="GeneralSettings"/>.
        /// </summary>
        public static CustomContinuationStyle FromGeneralSettings(GeneralSettings general)
        {
            return new CustomContinuationStyle
            {
                Pause = general.ContinuationPause,
                Suffix = general.CustomContinuationStyleSuffix,
                SuffixApplyIfComma = general.CustomContinuationStyleSuffixApplyIfComma,
                SuffixAddSpace = general.CustomContinuationStyleSuffixAddSpace,
                SuffixReplaceComma = general.CustomContinuationStyleSuffixReplaceComma,
                Prefix = general.CustomContinuationStylePrefix,
                PrefixAddSpace = general.CustomContinuationStylePrefixAddSpace,
                UseDifferentStyleGap = general.CustomContinuationStyleUseDifferentStyleGap,
                GapSuffix = general.CustomContinuationStyleGapSuffix,
                GapSuffixApplyIfComma = general.CustomContinuationStyleGapSuffixApplyIfComma,
                GapSuffixAddSpace = general.CustomContinuationStyleGapSuffixAddSpace,
                GapSuffixReplaceComma = general.CustomContinuationStyleGapSuffixReplaceComma,
                GapPrefix = general.CustomContinuationStyleGapPrefix,
                GapPrefixAddSpace = general.CustomContinuationStyleGapPrefixAddSpace,
            };
        }

        /// <summary>
        /// Writes this custom continuation style into the flat fields on a <see cref="GeneralSettings"/>
        /// (the shape the libse fix/merge engine reads).
        /// </summary>
        public void ApplyToGeneralSettings(GeneralSettings general)
        {
            general.ContinuationPause = Pause;
            general.CustomContinuationStyleSuffix = Suffix;
            general.CustomContinuationStyleSuffixApplyIfComma = SuffixApplyIfComma;
            general.CustomContinuationStyleSuffixAddSpace = SuffixAddSpace;
            general.CustomContinuationStyleSuffixReplaceComma = SuffixReplaceComma;
            general.CustomContinuationStylePrefix = Prefix;
            general.CustomContinuationStylePrefixAddSpace = PrefixAddSpace;
            general.CustomContinuationStyleUseDifferentStyleGap = UseDifferentStyleGap;
            general.CustomContinuationStyleGapSuffix = GapSuffix;
            general.CustomContinuationStyleGapSuffixApplyIfComma = GapSuffixApplyIfComma;
            general.CustomContinuationStyleGapSuffixAddSpace = GapSuffixAddSpace;
            general.CustomContinuationStyleGapSuffixReplaceComma = GapSuffixReplaceComma;
            general.CustomContinuationStyleGapPrefix = GapPrefix;
            general.CustomContinuationStyleGapPrefixAddSpace = GapPrefixAddSpace;
        }
    }
}
