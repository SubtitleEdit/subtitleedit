using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Cea708.Commands;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CommandState
    {
        public List<ICea708Command> Commands { get; set; }
        public int StartLineIndex { get; set; }

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
