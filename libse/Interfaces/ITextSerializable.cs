using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface ITextSerializable
    {
        string ToText(Subtitle subtitle, string title);
    }
}
