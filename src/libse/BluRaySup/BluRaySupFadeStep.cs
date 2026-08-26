using System;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    /// <summary>
    /// One alpha level of a Blu-ray fade. The object is sent once, at the start of the epoch, and
    /// each step after that is written as a "palette update display set" - a PCS with
    /// palette_update_flag set plus a PDS holding the same palette with every entry's alpha
    /// scaled by <see cref="AlphaPercent"/>. That is how retail discs fade: re-sending the whole
    /// object per step instead would cost kilobytes each and run into the decoder's pixel
    /// transfer budget, while a palette is ~1.3 KB and needs no decoding at all.
    /// </summary>
    public class BluRaySupFadeStep
    {
        /// <summary>
        /// When the alpha level takes effect, in milliseconds - the same time base as
        /// <see cref="BluRaySupPicture.StartTime"/>.
        /// </summary>
        public long TimeMs { get; set; }

        /// <summary>
        /// Alpha of the palette at this step, 0 (invisible) to 100 (fully opaque).
        /// </summary>
        public int AlphaPercent { get; set; }

        public long TimeForWrite => (long)Math.Round(TimeMs * 90.0, MidpointRounding.AwayFromZero);

        public BluRaySupFadeStep()
        {
        }

        public BluRaySupFadeStep(long timeMs, int alphaPercent)
        {
            TimeMs = timeMs;
            AlphaPercent = alphaPercent;
        }
    }
}
