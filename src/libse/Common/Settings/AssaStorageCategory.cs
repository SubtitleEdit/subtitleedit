using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class AssaStorageCategory
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public List<SsaStyle> Styles { get; set; }
    }
}