using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class MultipleSearchAndReplaceGroup
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public List<MultipleSearchAndReplaceSetting> Rules { get; set; }
    }
}