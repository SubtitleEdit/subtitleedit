using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Grammar
{
    public interface IGrammarDictionary
    {
        string Url { get; set; }
        string LocalName { get; set; }
        string TwoLetterIsoCode { get; set; }
        void Load(string twoLetterIsoCode);
        List<IGrammarDictionary> List();
    }
}
