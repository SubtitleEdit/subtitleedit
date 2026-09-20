using SkiaSharp;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    /// <summary>
    /// One caption of a Blu-ray display set. A display set composes up to
    /// <see cref="BluRaySupPicture.MaxCompositionObjects"/> of these, each in a window of its
    /// own, which is how a Blu-ray shows two subtitles at the same time - a line at the bottom
    /// of the frame and one at the top, say. The windows may not overlap each other.
    /// </summary>
    public class BluRaySupCompositionObject
    {
        public SKBitmap Bitmap { get; set; }

        /// <summary>
        /// First entry of the object's palette range, see <see cref="BluRaySupPicture.CreateSupFrame(BluRaySupPicture, IList{BluRaySupCompositionObject}, double, bool)"/>.
        /// </summary>
        public SKColor FontColor { get; set; }

        /// <summary>
        /// Left edge of the bitmap in the frame.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Top edge of the bitmap in the frame.
        /// </summary>
        public int Y { get; set; }

        public bool IsForced { get; set; }

        /// <summary>
        /// Alpha levels of this object's fade, in the time base of
        /// <see cref="BluRaySupPicture.StartTime"/>. Objects fade independently of each other.
        /// </summary>
        public List<BluRaySupFadeStep> FadeSteps { get; set; } = new List<BluRaySupFadeStep>();
    }
}
