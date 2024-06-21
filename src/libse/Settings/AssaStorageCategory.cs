using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class AssaStorageCategory
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public List<SsaStyle> Styles { get; set; }
    }
}