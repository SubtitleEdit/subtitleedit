using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Models
{
    public class DictionaryInfo
    {
        public CultureInfo Culture { get; set; }
        public string EnglishName { get; set; }
        public string NativeName { get; set; }
        public string Description { get; set; }
        public List<Uri> DownloadLinks { get; set; }

        public override string ToString()
        {
            if (Culture != null)
            {
                return Culture.EnglishName;
            }
            return EnglishName;
        }
    }
}
