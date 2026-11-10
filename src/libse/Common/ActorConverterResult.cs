using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class ActorConverterResult
    {
        public Paragraph Paragraph { get; set; }
        public Paragraph NextParagraph { get; set; }
        public bool Selected { get; set; }
        public bool Skip { get; internal set; }

        /// <summary>
        /// The speaker names found in the source paragraph, as written before casing, brackets or
        /// color were applied - lets a caller tick or untick every line of one actor.
        /// </summary>
        public List<string> Actors { get; set; } = new List<string>();
    }
}
