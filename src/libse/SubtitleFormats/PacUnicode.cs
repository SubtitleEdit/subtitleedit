using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// UniPac
    /// </summary>
    public class PacUnicode : Pac
    {
        public override string Extension => ".fpc";

        public override string Name => "PAC Unicode (UniPac)";

        public override bool IsFPC => true;
    }
}
