using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public static class TextEncodingExtensions
    {
        // IANA registered EBCDIC character sets (https://www.iana.org/assignments/character-sets/character-sets.xml)
        private static readonly HashSet<string> EbcdicNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "IBM037", "IBM500", "IBM870", "cp875", "IBM1026", "IBM01140", "IBM01141", "IBM01142", "IBM01143", "IBM01144", "IBM01145", "IBM01146", "IBM01147", "IBM01148", "IBM01149", "IBM273", "IBM277",
            "IBM278", "IBM280", "IBM284", "IBM285", "IBM290", "IBM297", "IBM420", "IBM423", "IBM424", "x-EBCDIC-KoreanExtended", "IBM-Thai", "IBM871", "IBM880", "IBM905", "cp1025", "IBM01047", "OSD_EBCDIC_DF04_15",
            "OSD_EBCDIC_DF03_IRV", "OSD_EBCDIC_DF04_1", "IBM038", "IBM274", "IBM275", "IBM281", "IBM918", "EBCDIC-AT-DE", "EBCDIC-AT-DE-A", "EBCDIC-CA-FR", "EBCDIC-DK-NO", "EBCDIC-DK-NO-A", "EBCDIC-FI-SE",
            "EBCDIC-FI-SE-A", "EBCDIC-FR", "EBCDIC-IT", "EBCDIC-PT", "EBCDIC-ES", "EBCDIC-ES-A", "EBCDIC-ES-S", "EBCDIC-UK", "EBCDIC-US", "IBM00924", "IBM1047", "csOSDEBCDICDF0415", "csOSDEBCDICDF03IRV",
            "csOSDEBCDICDF041", "csIBMThai", "cp037", "ebcdic-cp-us", "ebcdic-cp-ca", "ebcdic-cp-wt", "ebcdic-cp-nl", "csIBM037", "EBCDIC-INT", "cp038", "csIBM038", "CP273", "csIBM273", "EBCDIC-BE", "CP274",
            "csIBM274", "EBCDIC-BR", "cp275", "csIBM275", "EBCDIC-CP-DK", "EBCDIC-CP-NO", "csIBM277", "CP278", "ebcdic-cp-fi", "ebcdic-cp-se", "csIBM278", "CP280", "ebcdic-cp-it", "csIBM280", "EBCDIC-JP-E",
            "cp281", "csIBM281", "CP284", "ebcdic-cp-es", "csIBM284", "CP285", "ebcdic-cp-gb", "csIBM285", "cp290", "EBCDIC-JP-kana", "csIBM290", "cp297", "ebcdic-cp-fr", "csIBM297", "cp420", "ebcdic-cp-ar1",
            "csIBM420", "cp423", "ebcdic-cp-gr", "csIBM423", "cp424", "ebcdic-cp-he", "csIBM424", "CP500", "ebcdic-cp-be", "ebcdic-cp-ch", "csIBM500", "CP870", "ebcdic-cp-roece", "ebcdic-cp-yu", "csIBM870",
            "CP871", "ebcdic-cp-is", "csIBM871", "cp880", "EBCDIC-Cyrillic", "csIBM880", "CP905", "ebcdic-cp-tr", "csIBM905", "CP918", "ebcdic-cp-ar2", "csIBM918", "CP1026", "csIBM1026", "csIBMEBCDICATDE",
            "csEBCDICATDEA", "csEBCDICCAFR", "csEBCDICDKNO", "csEBCDICDKNOA", "csEBCDICFISE", "csEBCDICFISEA", "csEBCDICFR", "csEBCDICIT", "csEBCDICPT", "csEBCDICES", "csEBCDICESA", "csEBCDICESS", "csEBCDICUK",
            "csEBCDICUS", "CCSID00924", "CP00924", "ebcdic-Latin9--euro", "csIBM00924", "CCSID01140", "CP01140", "ebcdic-us-37+euro", "csIBM01140", "CCSID01141", "CP01141", "ebcdic-de-273+euro", "csIBM01141",
            "CCSID01142", "CP01142", "ebcdic-dk-277+euro", "ebcdic-no-277+euro", "csIBM01142", "CCSID01143", "CP01143", "ebcdic-fi-278+euro", "ebcdic-se-278+euro", "csIBM01143", "CCSID01144", "CP01144",
            "ebcdic-it-280+euro", "csIBM01144", "CCSID01145", "CP01145", "ebcdic-es-284+euro", "csIBM01145", "CCSID01146", "CP01146", "ebcdic-gb-285+euro", "csIBM01146", "CCSID01147", "CP01147", "ebcdic-fr-297+euro",
            "csIBM01147", "CCSID01148", "CP01148", "ebcdic-international-500+euro", "csIBM01148", "CCSID01149", "CP01149", "ebcdic-is-871+euro", "csIBM01149", "IBM-1047", "csIBM1047"
        };

        public static bool IsEbcdic(this Encoding encoding)
        {
            return EbcdicNames.Contains(encoding.WebName);
        }

    }
}
