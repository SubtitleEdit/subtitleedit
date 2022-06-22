namespace Nikse.SubtitleEdit.Core.Enums
{
    public enum ContinuationStyle
    {
        None,                           // None
        NoneTrailingDots,               // None, dots for pauses (trailing only)
        NoneLeadingTrailingDots,        // None, dots for pauses
        NoneTrailingEllipsis,           // None, ellipsis for pauses (trailing only)
        NoneLeadingTrailingEllipsis,    // None, ellipsis for pauses
        OnlyTrailingDots,               // Dots (trailing only)
        LeadingTrailingDots,            // Dots
        OnlyTrailingEllipsis,           // Ellipsis (trailing only)
        LeadingTrailingEllipsis,        // Ellipsis
        LeadingTrailingDash,            // Dash
        LeadingTrailingDashDots         // Dash, but dots for pauses
    }
}