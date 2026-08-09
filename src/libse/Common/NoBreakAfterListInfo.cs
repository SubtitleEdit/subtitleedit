using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// A language's "no break after" list plus the two facts <see cref="Utilities.CanBreak"/> needs
    /// about it. CanBreak runs once per candidate break point in the line, so deriving these per
    /// call - as the multi-word phrase check for issue #9631 first did - means re-walking the whole
    /// list (117 entries for Greek, the largest shipped list) for every space in the line.
    /// </summary>
    internal sealed class NoBreakAfterListInfo
    {
        /// <summary>
        /// CanBreak asks for the list once per candidate break point, and AutoBreakLine(text)
        /// passes no language at all, so the "no language" case must not allocate every time.
        /// </summary>
        public static readonly NoBreakAfterListInfo Empty = new NoBreakAfterListInfo(new List<NoBreakAfterItem>());

        public NoBreakAfterListInfo(List<NoBreakAfterItem> items)
        {
            Items = items;
            foreach (var item in items)
            {
                if (item.Regex != null)
                {
                    HasRegex = true;
                }
                else if (item.Text != null && item.Text.IndexOf(' ') >= 0)
                {
                    MultiWordItems = MultiWordItems ?? new List<NoBreakAfterItem>();
                    MultiWordItems.Add(item);
                }
            }
        }

        public List<NoBreakAfterItem> Items { get; }

        /// <summary>True when at least one entry matches with a regex, which needs a string.</summary>
        public bool HasRegex { get; }

        /// <summary>
        /// The multi-word phrase entries, or null when the list has none - which is the case for
        /// all but 2 of the 33 shipped lists.
        /// </summary>
        public List<NoBreakAfterItem> MultiWordItems { get; }
    }
}
