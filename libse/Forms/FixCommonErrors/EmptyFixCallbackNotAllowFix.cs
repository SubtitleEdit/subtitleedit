using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    class EmptyFixCallbackNotAllowFix : EmptyFixCallback, IFixCallbacks
    {
        private readonly HashSet<string> _notAllowedFixes = new HashSet<string>();

        public HashSet<string> NotAllowedFix
        {
            get
            {
                return _notAllowedFixes;
            }
        }

        public new bool AllowFix(Paragraph p, string action)
        {
            return !_notAllowedFixes.Contains(p.Number.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" + action);
        }
    }
}
