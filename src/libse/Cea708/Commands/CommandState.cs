using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class CommandState
    {
        public List<CommandBase> Commands { get; set; }
        public int StartLineIndex { get; set; }

        public CommandState()
        {
            Commands = new List<CommandBase>();
        }
    }
}
