using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public interface IText
    {
        string ToText(Subtitle subtitle, string title);
    }
}
