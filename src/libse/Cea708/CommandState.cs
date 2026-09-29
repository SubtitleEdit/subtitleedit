using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Cea708.Commands;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CommandState
    {
        public List<ICea708Command> Commands { get; set; }
        public int StartLineIndex { get; set; }

        /// <summary>
        /// Captions flushed by the last <see cref="Cea708.Decode"/> call: the line index (packet
        /// number) where each started, and its text.
        /// </summary>
        public List<KeyValuePair<int, string>> FlushedTexts { get; } = new List<KeyValuePair<int, string>>();

        /// <summary>
        /// Indexes into <see cref="FlushedTexts"/> of lines ended by a carriage return in a visible
        /// window (roll-up/paint-on) - the line is complete, but stays on screen until a later line
        /// or a command that erases it (see <see cref="ErasedAtFlushCounts"/>).
        /// </summary>
        public HashSet<int> StillVisibleFlushes { get; } = new HashSet<int>();

        /// <summary>
        /// Positions (number of <see cref="FlushedTexts"/> so far) at which the last
        /// <see cref="Cea708.Decode"/> call cleared, hid, deleted or reset windows - text on screen
        /// ends there.
        /// </summary>
        public List<int> ErasedAtFlushCounts { get; } = new List<int>();

        /// <summary>
        /// Window the pen is in (0-7), -1 when undefined - set by SetCurrentWindow and DefineWindow.
        /// </summary>
        public int CurrentWindow { get; set; } = -1;

        /// <summary>
        /// Visibility per window (0-7). Roll-up and paint-on captions are written straight into a
        /// visible window, pop-on captions into a hidden one that is displayed when complete.
        /// </summary>
        public bool[] VisibleWindows { get; } = new bool[8];

        public CommandState()
        {
            Commands = new List<ICea708Command>();
        }
    }
}
