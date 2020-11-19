using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Grammar
{
    public class LanguageToolsDictionary : IGrammarDictionary
    {
        public string Url { get; set; }
        public string LocalName { get; set; }

        public string TwoLetterIsoCode { get; set; }

        public void Load(string twoLetterIsoCode)
        {
            TwoLetterIsoCode = twoLetterIsoCode;
            //TODO: Load xml
        }

        public List<IGrammarDictionary> List()
        {
            return new List<IGrammarDictionary>
            {
                new LanguageToolsDictionary
                {
                    TwoLetterIsoCode = "da",
                    Url = "https://raw.githubusercontent.com/languagetool-org/languagetool/master/languagetool-language-modules/da/src/main/resources/org/languagetool/rules/da/grammar.xml",
                    LocalName = "da.xml"
                },
                new LanguageToolsDictionary
                {
                    TwoLetterIsoCode = "en",
                    Url = "https://raw.githubusercontent.com/languagetool-org/languagetool/master/languagetool-language-modules/en/src/main/resources/org/languagetool/rules/en/grammar.xml",
                    LocalName = "en.xml"
                },
                new LanguageToolsDictionary
                {
                    TwoLetterIsoCode = "nl",
                    Url = "https://github.com/languagetool-org/languagetool/raw/master/languagetool-language-modules/nl/src/main/resources/org/languagetool/rules/nl/grammar.xml",
                    LocalName = "nl.xml"
                },
                new LanguageToolsDictionary
                {
                    TwoLetterIsoCode = "pl",
                    Url = "https://github.com/languagetool-org/languagetool/raw/master/languagetool-language-modules/pl/src/main/resources/org/languagetool/rules/pl/grammar.xml",
                    LocalName = "pl.xml"
                },
                new LanguageToolsDictionary
                {
                    TwoLetterIsoCode = "de",
                    Url = "https://github.com/languagetool-org/languagetool/raw/master/languagetool-language-modules/de/src/main/resources/org/languagetool/rules/de/grammar.xml",
                    LocalName = "de.xml"
                },
                new LanguageToolsDictionary
                {
                    TwoLetterIsoCode = "es",
                    Url = "https://github.com/languagetool-org/languagetool/raw/master/languagetool-language-modules/es/src/main/resources/org/languagetool/rules/es/grammar.xml",
                    LocalName = "es.xml"
                },
                new LanguageToolsDictionary
                {
                    TwoLetterIsoCode = "it",
                    Url = "https://github.com/languagetool-org/languagetool/raw/master/languagetool-language-modules/lt/src/main/resources/org/languagetool/rules/lt/grammar.xml",
                    LocalName = "it.xml"
                }
            };
        }
    }
}
