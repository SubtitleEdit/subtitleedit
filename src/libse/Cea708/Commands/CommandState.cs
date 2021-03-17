using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class CommandState
    {
        public List<CommandText> Texts { get; set; }
        public Window[] Windows { get; set; }
        public int CurrentWindow { get; set; }

        public const int WindowCount = 8;
        public CommandState()
        {
            Texts = new List<CommandText>();
            Windows = new Window[WindowCount];
            for (var i = 0; i < WindowCount; i++)
            {
                Windows[i] = new Window();
            }
        }
    }
}
