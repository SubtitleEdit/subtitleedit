using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class TranslationPair
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
