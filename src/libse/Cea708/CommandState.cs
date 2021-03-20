using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Cea708.Commands;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CommandState
    {
        public List<ICommand> Commands { get; set; }
        public int StartLineIndex { get; set; }

        public CommandState()
        {
            Commands = new List<ICommand>();
        }
    }
}
