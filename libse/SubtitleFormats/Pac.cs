using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// The PAC format was developed by Screen Electronics
    /// The PAC format save the contents, time code, position, justification, and italicization of each subtitle. The choice of font is not saved.
    /// </summary>
    public class Pac : SubtitleFormat
    {
        public interface IGetPacEncoding
        {
            int GetPacEncoding(byte[] previewBuffer, string fileName);
        }

        public static IGetPacEncoding GetPacEncodingImplementation;

        public static readonly TimeCode PacNullTime = new TimeCode(655, 35, 00, 0);

        public static bool ThrowOnError = false;

        public static bool IsValidCodePage(int codePage)
        {
            return 0 <= codePage && codePage <= 11;
        }
        public const int CodePageLatin = 0;
        public const int CodePageGreek = 1;
        public const int CodePageLatinCzech = 2;
        public const int CodePageArabic = 3;
        public const int CodePageHebrew = 4;
        public const int CodePageThai = 5;
        public const int CodePageCyrillic = 6;
        public const int CodePageChineseTraditional = 7;
        public const int CodePageChineseSimplified = 8;
        public const int CodePageKorean = 9;
        public const int CodePageJapanese = 10;
        public const int CodePageLatinTurkish = 11;

        public const int EncodingChineseSimplified = 936;
        public const int EncodingChineseTraditional = 950;
        public const int EncodingKorean = 949;
        public const int EncodingJapanese = 932;

        /// <summary>
        /// Contains Swedish, Danish, German, Spanish, and French letters
        /// </summary>
        private static readonly Dictionary<int, SpecialCharacter> LatinCodes = new Dictionary<int, SpecialCharacter>
        {
            { 0xe041, new SpecialCharacter("Ã")},
            { 0xe04e, new SpecialCharacter("Ñ")},
            { 0xe04f, new SpecialCharacter("Õ")},
            { 0xe061, new SpecialCharacter("ã")},
            { 0xe06e, new SpecialCharacter("ñ")},
            { 0xe06f, new SpecialCharacter("õ")},
            { 0xe161, new SpecialCharacter("å")},
            { 0xe141, new SpecialCharacter("Å")},

            { 0x618a, new SpecialCharacter("ā")},
            { 0x418a, new SpecialCharacter("Ā")},
            { 0x458a, new SpecialCharacter("Ē")},
            { 0x658a, new SpecialCharacter("ē")},
            { 0x498a, new SpecialCharacter("Ī")},
            { 0x698a, new SpecialCharacter("ī")},
            { 0x4f8a, new SpecialCharacter("Ō")},
            { 0x6f8a, new SpecialCharacter("ō")},
            { 0x558a, new SpecialCharacter("Ū")},
            { 0x758a, new SpecialCharacter("ū")},

            { 0x23, new SpecialCharacter("£")},
            { 0x7c, new SpecialCharacter("æ")},
            { 0x7d, new SpecialCharacter("ø")},
            { 0x7e, new SpecialCharacter("§")},
            { 0x80, new SpecialCharacter("#")},
            { 0x5c, new SpecialCharacter("Æ")},
            { 0x5d, new SpecialCharacter("Ø")},
            { 0x5e, new SpecialCharacter("÷")},
            { 0x2d, new SpecialCharacter("-")},
            { 0x5f, new SpecialCharacter("–")},
            { 0xE54f, new SpecialCharacter("Ö")},
            { 0xE56f, new SpecialCharacter("ö")},
            { 0xe541, new SpecialCharacter("Ä")},
            { 0xe561, new SpecialCharacter("ä")},
            { 0xe555, new SpecialCharacter("Ü")},
            { 0xe575, new SpecialCharacter("ü")},
            { 0x81,   new SpecialCharacter("ß")},
            { 0x82,   new SpecialCharacter("²")},
            { 0xe241, new SpecialCharacter("Á")},
            { 0xe249, new SpecialCharacter("Í")},
            { 0xe255, new SpecialCharacter("Ú")},
            { 0xe259, new SpecialCharacter("Ý")},
            { 0xe261, new SpecialCharacter("á")},
            { 0xe265, new SpecialCharacter("é")},
            { 0xe269, new SpecialCharacter("í")},
            { 0xe245, new SpecialCharacter("É")},
            { 0xe275, new SpecialCharacter("ú")}, // or "ѓ" !?
            { 0xe279, new SpecialCharacter("ý")},
            { 0xe361, new SpecialCharacter("à")},
            { 0xe365, new SpecialCharacter("è")},
            { 0xe36f, new SpecialCharacter("ò")},
            { 0xe345, new SpecialCharacter("È")},
            { 0xe349, new SpecialCharacter("Ì")},
            { 0xe34f, new SpecialCharacter("Ò")},
            { 0xe369, new SpecialCharacter("ì")},
            { 0xe443, new SpecialCharacter("Ĉ")},
            { 0xe447, new SpecialCharacter("Ĝ")},
            { 0xe448, new SpecialCharacter("Ĥ")},
            { 0xe44A, new SpecialCharacter("Ĵ")},
            { 0xe453, new SpecialCharacter("Ŝ")},
            { 0xeA55, new SpecialCharacter("Ǔ")},
            { 0xe463, new SpecialCharacter("ĉ")},
            { 0xe467, new SpecialCharacter("ĝ")},
            { 0xe468, new SpecialCharacter("ĥ")},
            { 0xe46A, new SpecialCharacter("ĵ")},
            { 0xe473, new SpecialCharacter("ŝ")},
            { 0xeA75, new SpecialCharacter("ǔ")},
            { 0xe341, new SpecialCharacter("À")},
            { 0xe441, new SpecialCharacter("Â")},
            { 0xe461, new SpecialCharacter("â")},
            { 0xe643, new SpecialCharacter("Ç")},
            { 0xe663, new SpecialCharacter("ç")},
            { 0xe445, new SpecialCharacter("Ê")},
            { 0xe465, new SpecialCharacter("ê")},
            { 0xe545, new SpecialCharacter("Ë")},
            { 0xe565, new SpecialCharacter("ë")},
            { 0xe449, new SpecialCharacter("Î")},
            { 0xe469, new SpecialCharacter("î")},
            { 0xe549, new SpecialCharacter("Ï")},
            { 0xe569, new SpecialCharacter("ï")},
            { 0xe44F, new SpecialCharacter("Ô")},
            { 0xe46F, new SpecialCharacter("ô")},
            { 0xe355, new SpecialCharacter("Ù")},
            { 0xe375, new SpecialCharacter("ù")},
            { 0xe455, new SpecialCharacter("Û")},
            { 0xe475, new SpecialCharacter("û")},
            { 0xe559, new SpecialCharacter("Ÿ")},
            { 0xe579, new SpecialCharacter("ÿ")},
            { 0xeb41, new SpecialCharacter("Ą")},
            { 0xeb61, new SpecialCharacter("ą")},
            { 0xe243, new SpecialCharacter("Ć")},
            { 0xe263, new SpecialCharacter("ć")},
            { 0xeB45, new SpecialCharacter("Ę")},
            { 0xeB65, new SpecialCharacter("ę")},
            { 0x9c,   new SpecialCharacter("Ł")},
            { 0xbc,   new SpecialCharacter("ł")},
            { 0xe24e, new SpecialCharacter("Ń")},
            { 0xe26e, new SpecialCharacter("ń")},
            { 0xe24f, new SpecialCharacter("Ó")},
            { 0xe26f, new SpecialCharacter("ó")},
            { 0xe253, new SpecialCharacter("Ś")},
            { 0xe273, new SpecialCharacter("ś")},
            { 0xe25a, new SpecialCharacter("Ź")},
            { 0xe27a, new SpecialCharacter("ź")},
            { 0xe85a, new SpecialCharacter("Ż")},
            { 0xe87a, new SpecialCharacter("ż")},
            { 0x87, new SpecialCharacter("þ")},
            { 0x89, new SpecialCharacter("ð")},
            { 0x88, new SpecialCharacter("Þ")},
            { 0x8c, new SpecialCharacter("Đ")},

            { 0xe653, new SpecialCharacter("Ş")},
            { 0xe673, new SpecialCharacter("ş")},
            { 0x7b,   new SpecialCharacter("ı")},
            { 0xeA67, new SpecialCharacter("ǧ")},
            { 0xeA47, new SpecialCharacter("Ǧ")},
            { 0xe849, new SpecialCharacter("İ")},

            { 0xE75A, new SpecialCharacter("Ž")},
            { 0xE753, new SpecialCharacter("Š")},
            { 0xE743, new SpecialCharacter("Č")},

            { 0xE77A, new SpecialCharacter("ž")},
            { 0xE773, new SpecialCharacter("š")},
            { 0xE763, new SpecialCharacter("č")},
            { 0xAE, new SpecialCharacter("đ")},

            { 0xA8,  new SpecialCharacter("¿")},
            { 0xAD,  new SpecialCharacter("¡")},
            { 0xA6,  new SpecialCharacter("ª")},
            { 0xA7,  new SpecialCharacter("º")},

            { 0xAB, new SpecialCharacter("½")},
            { 0xBB, new SpecialCharacter("»")},
            { 0xB3, new SpecialCharacter("³")},
            { 0x1C, new SpecialCharacter("“")},
            { 0x1D, new SpecialCharacter("”")},
            { 0x18, new SpecialCharacter("‘")},
            { 0x19, new SpecialCharacter("’")},
            { 0x13, new SpecialCharacter("–")},
            { 0x14, new SpecialCharacter("—")},
            { 0x83, new SpecialCharacter("³")},
            { 0x5B, new SpecialCharacter("¤")},
            { 0x85, new SpecialCharacter("ŧ")},
            { 0x86, new SpecialCharacter("Ŧ")},
            { 0x8A, new SpecialCharacter("«")},
            { 0x8B, new SpecialCharacter("»")},
            { 0x8E, new SpecialCharacter("Ŋ")},
            { 0x8D, new SpecialCharacter("ŋ")},
            { 0x95, new SpecialCharacter("Ħ")},
            { 0x96, new SpecialCharacter("ħ")},
            { 0x9A, new SpecialCharacter("Œ")},
            { 0x9B, new SpecialCharacter("¢")},
            { 0x9D, new SpecialCharacter("¥")},
            { 0x9E, new SpecialCharacter("€")},
            { 0xA9, new SpecialCharacter("°")},
            { 0xAA, new SpecialCharacter("¾")},
            { 0xAC, new SpecialCharacter("¼")},
            { 0xB1, new SpecialCharacter("±")},
            { 0xB5, new SpecialCharacter("µ")},
            { 0xBA, new SpecialCharacter("œ")},
            { 0xBF, new SpecialCharacter("ǧ")},
            { 0xD2, new SpecialCharacter("®")},
            { 0xD4, new SpecialCharacter("©")},
            { 0x4CE2, new SpecialCharacter("Ľ", priority: 1)},
            { 0x74E2, new SpecialCharacter("ť", priority: 1)},
            { 0x64E2, new SpecialCharacter("ď", priority: 1)},
            { 0x6CE2, new SpecialCharacter("ľ", priority: 1)},
            { 0xE020, new SpecialCharacter("˜", priority: 0)},
            { 0xE045, new SpecialCharacter("Ẽ")},
            { 0xE049, new SpecialCharacter("Ĩ")},
            { 0xE055, new SpecialCharacter("Ũ")},
            { 0xE056, new SpecialCharacter("Ṽ")},
            { 0xE059, new SpecialCharacter("Ỹ")},
            { 0xE065, new SpecialCharacter("ẽ")},
            { 0xE069, new SpecialCharacter("ĩ")},
            { 0xE075, new SpecialCharacter("ũ")},
            { 0xE076, new SpecialCharacter("ṽ")},
            { 0xE079, new SpecialCharacter("ỹ")},
            { 0xE120, new SpecialCharacter("˚", priority: 0)},
            { 0xE155, new SpecialCharacter("Ů")},
            { 0xE175, new SpecialCharacter("ů")},
            { 0xE177, new SpecialCharacter("ẘ")},
            { 0xE179, new SpecialCharacter("ẙ")},
            { 0xE220, new SpecialCharacter("´", priority: 0)},
            { 0xE247, new SpecialCharacter("Ǵ")},
            { 0xE24B, new SpecialCharacter("Ḱ")},
            { 0xE24C, new SpecialCharacter("Ĺ")},
            { 0xE24D, new SpecialCharacter("Ḿ")},
            { 0xE250, new SpecialCharacter("Ṕ")},
            { 0xE252, new SpecialCharacter("Ŕ")},
            { 0xE257, new SpecialCharacter("Ẃ")},
            { 0xE25C, new SpecialCharacter("Ǽ")},
            { 0xE25D, new SpecialCharacter("Ǿ")},
            { 0xE267, new SpecialCharacter("ǵ")},
            { 0xE26B, new SpecialCharacter("ḱ")},
            { 0xE26C, new SpecialCharacter("ĺ")},
            { 0xE26D, new SpecialCharacter("ḿ")},
            { 0xE270, new SpecialCharacter("ṕ")},
            { 0xE272, new SpecialCharacter("ŕ")},
            { 0xE277, new SpecialCharacter("ẃ")},
            { 0xE27C, new SpecialCharacter("ǽ")},
            { 0xE27D, new SpecialCharacter("ǿ")},
            { 0xE320, new SpecialCharacter("`", priority: 0)},
            { 0xE34E, new SpecialCharacter("Ǹ")},
            { 0xE357, new SpecialCharacter("Ẁ")},
            { 0xE359, new SpecialCharacter("Ỳ")},
            { 0xE36E, new SpecialCharacter("ǹ")},
            { 0xE377, new SpecialCharacter("ẁ")},
            { 0xE379, new SpecialCharacter("ỳ")},
            { 0xE420, new SpecialCharacter("^", priority: 0)},
            { 0xE457, new SpecialCharacter("Ŵ")},
            { 0xE459, new SpecialCharacter("Ŷ")},
            { 0xE45A, new SpecialCharacter("Ẑ")},
            { 0xE477, new SpecialCharacter("ŵ")},
            { 0xE479, new SpecialCharacter("ŷ")},
            { 0xE47A, new SpecialCharacter("ẑ")},
            { 0xE520, new SpecialCharacter("¨", priority: 0)},
            { 0xE548, new SpecialCharacter("Ḧ")},
            { 0xE557, new SpecialCharacter("Ẅ")},
            { 0xE558, new SpecialCharacter("Ẍ")},
            { 0xE568, new SpecialCharacter("ḧ")},
            { 0xE574, new SpecialCharacter("ẗ")},
            { 0xE577, new SpecialCharacter("ẅ")},
            { 0xE578, new SpecialCharacter("ẍ")},
            { 0xE620, new SpecialCharacter("¸", priority: 0)},
            { 0xE644, new SpecialCharacter("Ḑ")},
            { 0xE645, new SpecialCharacter("Ȩ")},
            { 0xE647, new SpecialCharacter("Ģ")},
            { 0xE648, new SpecialCharacter("Ḩ")},
            { 0xE64B, new SpecialCharacter("Ķ")},
            { 0xE64C, new SpecialCharacter("Ļ")},
            { 0xE64E, new SpecialCharacter("Ņ")},
            { 0xE652, new SpecialCharacter("Ŗ")},
            { 0xE654, new SpecialCharacter("Ţ")},
            { 0xE664, new SpecialCharacter("ḑ")},
            { 0xE665, new SpecialCharacter("ȩ")},
            { 0xE667, new SpecialCharacter("ģ")},
            { 0xE668, new SpecialCharacter("ḩ")},
            { 0xE66B, new SpecialCharacter("ķ")},
            { 0xE66C, new SpecialCharacter("ļ")},
            { 0xE66E, new SpecialCharacter("ņ")},
            { 0xE672, new SpecialCharacter("ŗ")},
            { 0xE674, new SpecialCharacter("ţ")},
            { 0xE720, new SpecialCharacter("ˇ", priority: 0)},
            { 0xE741, new SpecialCharacter("Ă")},
            { 0xE744, new SpecialCharacter("Ď")},
            { 0xE745, new SpecialCharacter("Ě")},
            { 0xE747, new SpecialCharacter("Ǧ")},
            { 0xE748, new SpecialCharacter("Ȟ")},
            { 0xE749, new SpecialCharacter("Ǐ")},
            { 0xE74B, new SpecialCharacter("Ǩ")},
            { 0xE74E, new SpecialCharacter("Ň")},
            { 0xE74F, new SpecialCharacter("Ǒ")},
            { 0xE752, new SpecialCharacter("Ř")},
            { 0xE754, new SpecialCharacter("Ť")},
            { 0xE755, new SpecialCharacter("Ǔ")},
            { 0xE761, new SpecialCharacter("ă")},
            { 0xE765, new SpecialCharacter("ě")},
            { 0xE768, new SpecialCharacter("ȟ")},
            { 0xE769, new SpecialCharacter("ǐ")},
            { 0xE76A, new SpecialCharacter("ǰ")},
            { 0xE76B, new SpecialCharacter("ǩ")},
            { 0xE76E, new SpecialCharacter("ň")},
            { 0xE76F, new SpecialCharacter("ǒ")},
            { 0xE772, new SpecialCharacter("ř")},
            { 0xE775, new SpecialCharacter("ǔ")},
            { 0xE820, new SpecialCharacter("˙", priority: 0)},
            { 0xE841, new SpecialCharacter("Ȧ")},
            { 0xE842, new SpecialCharacter("Ḃ")},
            { 0xE843, new SpecialCharacter("Ċ")},
            { 0xE844, new SpecialCharacter("Ḋ")},
            { 0xE845, new SpecialCharacter("Ė")},
            { 0xE846, new SpecialCharacter("Ḟ")},
            { 0xE847, new SpecialCharacter("Ġ")},
            { 0xE848, new SpecialCharacter("Ḣ")},
            { 0xE84D, new SpecialCharacter("Ṁ")},
            { 0xE84E, new SpecialCharacter("Ṅ")},
            { 0xE84F, new SpecialCharacter("Ȯ")},
            { 0xE850, new SpecialCharacter("Ṗ")},
            { 0xE852, new SpecialCharacter("Ṙ")},
            { 0xE853, new SpecialCharacter("Ṡ")},
            { 0xE854, new SpecialCharacter("Ṫ")},
            { 0xE857, new SpecialCharacter("Ẇ")},
            { 0xE858, new SpecialCharacter("Ẋ")},
            { 0xE859, new SpecialCharacter("Ẏ")},
            { 0xE861, new SpecialCharacter("ȧ")},
            { 0xE862, new SpecialCharacter("ḃ")},
            { 0xE863, new SpecialCharacter("ċ")},
            { 0xE864, new SpecialCharacter("ḋ")},
            { 0xE865, new SpecialCharacter("ė")},
            { 0xE866, new SpecialCharacter("ḟ")},
            { 0xE867, new SpecialCharacter("ġ")},
            { 0xE868, new SpecialCharacter("ḣ")},
            { 0xE86D, new SpecialCharacter("ṁ")},
            { 0xE86E, new SpecialCharacter("ṅ")},
            { 0xE86F, new SpecialCharacter("ȯ")},
            { 0xE870, new SpecialCharacter("ṗ")},
            { 0xE872, new SpecialCharacter("ṙ")},
            { 0xE873, new SpecialCharacter("ṡ")},
            { 0xE874, new SpecialCharacter("ṫ")},
            { 0xE877, new SpecialCharacter("ẇ")},
            { 0xE878, new SpecialCharacter("ẋ")},
            { 0xE879, new SpecialCharacter("ẏ")},
            { 0xE920, new SpecialCharacter("¯", priority: 0)},
            { 0xE941, new SpecialCharacter("Ā")},
            { 0xE945, new SpecialCharacter("Ē")},
            { 0xE947, new SpecialCharacter("Ḡ")},
            { 0xE949, new SpecialCharacter("Ī")},
            { 0xE94F, new SpecialCharacter("Ō")},
            { 0xE955, new SpecialCharacter("Ū")},
            { 0xE959, new SpecialCharacter("Ȳ")},
            { 0xE95C, new SpecialCharacter("Ǣ")},
            { 0xE961, new SpecialCharacter("ā")},
            { 0xE965, new SpecialCharacter("ē")},
            { 0xE967, new SpecialCharacter("ḡ")},
            { 0xE969, new SpecialCharacter("ī")},
            { 0xE96F, new SpecialCharacter("ō")},
            { 0xE975, new SpecialCharacter("ū")},
            { 0xE979, new SpecialCharacter("ȳ")},
            { 0xE97C, new SpecialCharacter("ǣ")},
            { 0xEA20, new SpecialCharacter("ˇ", priority: 0)},
            { 0xEA41, new SpecialCharacter("Ă")},
            { 0xEA45, new SpecialCharacter("Ě")},
            { 0xEA49, new SpecialCharacter("Ǐ")},
            { 0xEA4F, new SpecialCharacter("Ǒ")},
            { 0xEA61, new SpecialCharacter("ă")},
            { 0xEA65, new SpecialCharacter("ě")},
            { 0xEA69, new SpecialCharacter("ǐ")},
            { 0xEA6F, new SpecialCharacter("ǒ")},
            { 0xEB20, new SpecialCharacter("˛", priority: 0)},
            { 0xEB49, new SpecialCharacter("Į")},
            { 0xEB4F, new SpecialCharacter("Ǫ")},
            { 0xEB55, new SpecialCharacter("Ų")},
            { 0xEB69, new SpecialCharacter("į")},
            { 0xEB6F, new SpecialCharacter("ǫ")},
            { 0xEB75, new SpecialCharacter("ų")},
            { 0xEC20, new SpecialCharacter("˝", priority: 0)},
            { 0xEC4F, new SpecialCharacter("Ő")},
            { 0xEC55, new SpecialCharacter("Ű")},
            { 0xEC6F, new SpecialCharacter("ő")},
            { 0xEC75, new SpecialCharacter("ű")},
        };

        private static readonly Dictionary<int, SpecialCharacter> LatinTurkishOverrides = new Dictionary<int, SpecialCharacter>
        {
            { 0xE720, new SpecialCharacter("˘", priority: 0) },
            { 0xE741, new SpecialCharacter("Ă")},
            { 0xE745, new SpecialCharacter("Ĕ")},
            { 0xE747, new SpecialCharacter("Ğ")},
            { 0xE749, new SpecialCharacter("Ĭ")},
            { 0xE74F, new SpecialCharacter("Ŏ")},
            { 0xE755, new SpecialCharacter("Ŭ")},
            { 0xE761, new SpecialCharacter("ă")},
            { 0xE765, new SpecialCharacter("ĕ")},
            { 0xE769, new SpecialCharacter("ĭ")},
            { 0xE76F, new SpecialCharacter("ŏ")},
            { 0xE775, new SpecialCharacter("ŭ")},

            { 0xEA20, new SpecialCharacter("˘", priority: 0) },
            { 0xEA45, new SpecialCharacter("Ĕ")},
            { 0xEA47, new SpecialCharacter("Ğ")},
            { 0xEA49, new SpecialCharacter("Ĭ")},
            { 0xEA4F, new SpecialCharacter("Ŏ")},
            { 0xEA55, new SpecialCharacter("Ŭ")},
            { 0xEA65, new SpecialCharacter("ĕ")},
            { 0xEA67, new SpecialCharacter("ğ")},
            { 0xEA69, new SpecialCharacter("ĭ")},
            { 0xEA6F, new SpecialCharacter("ŏ")},
            { 0xEA75, new SpecialCharacter("ŭ")},
        };

        private static readonly Dictionary<int, SpecialCharacter> HebrewCodes = new Dictionary<int, SpecialCharacter>
        {
            { 0x80, new SpecialCharacter("ְ")},
            { 0x81, new SpecialCharacter("ֱ")},
            { 0x82, new SpecialCharacter("ֲ")},
            { 0x83, new SpecialCharacter("ֳ")},
            { 0x84, new SpecialCharacter("ִ")},
            { 0x85, new SpecialCharacter("ֵ")},
            { 0x86, new SpecialCharacter("ֶ")},
            { 0x87, new SpecialCharacter("ַ")},
            { 0x88, new SpecialCharacter("ָ")},
            { 0x89, new SpecialCharacter("ֹ")},
            { 0x8b, new SpecialCharacter("ֻ")},
            { 0x8c, new SpecialCharacter("ּ")},
            { 0x8d, new SpecialCharacter("ֽ")},
            { 0x92, new SpecialCharacter("֗")},
            { 0xa0, new SpecialCharacter("א")},
            { 0xa1, new SpecialCharacter("ב")},
            { 0xa2, new SpecialCharacter("ג")},
            { 0xa3, new SpecialCharacter("ד")},
            { 0xa4, new SpecialCharacter("ה")},
            { 0xa5, new SpecialCharacter("ו")},
            { 0xa6, new SpecialCharacter("ז")},
            { 0xa7, new SpecialCharacter("ח")},
            { 0xa8, new SpecialCharacter("ט")},
            { 0xa9, new SpecialCharacter("י")},
            { 0xaa, new SpecialCharacter("ך")},
            { 0xab, new SpecialCharacter("כ")},
            { 0xac, new SpecialCharacter("ל")},
            { 0xad, new SpecialCharacter("ם")},
            { 0xae, new SpecialCharacter("מ")},
            { 0xaf, new SpecialCharacter("ן")},
            { 0xb0, new SpecialCharacter("נ")},
            { 0xb1, new SpecialCharacter("ס")},
            { 0xb2, new SpecialCharacter("ע")},
            { 0xb3, new SpecialCharacter("ף")},
            { 0xb4, new SpecialCharacter("פ")},
            { 0xb5, new SpecialCharacter("ץ")},
            { 0xb6, new SpecialCharacter("צ")},
            { 0xb7, new SpecialCharacter("ק")},
            { 0xb8, new SpecialCharacter("ר")},
            { 0xb9, new SpecialCharacter("ש")},
            { 0xba, new SpecialCharacter("ת")},
            { 0xbb, new SpecialCharacter("װ")},
            { 0xbc, new SpecialCharacter("ױ")},
            { 0xbd, new SpecialCharacter("ײ")},
            { 0xcc, new SpecialCharacter("ףּ")},
            { 0xcd, new SpecialCharacter("רּ")},
            { 0xce, new SpecialCharacter("אַ")},
            { 0xcf, new SpecialCharacter("אּ")},
            { 0xd0, new SpecialCharacter("אָ")},
            { 0xd1, new SpecialCharacter("שּׂ")},
            { 0xd2, new SpecialCharacter("שּׁ")},
            { 0xd3, new SpecialCharacter("שׂ")},
            { 0xd4, new SpecialCharacter("שׁ")},
            { 0xd5, new SpecialCharacter("ﬥ")},
            { 0xd6, new SpecialCharacter("ﬠ")},
            { 0xd7, new SpecialCharacter("לּ")},
            { 0xd8, new SpecialCharacter("יּ")},
            { 0xd9, new SpecialCharacter("טּ")},
            { 0xda, new SpecialCharacter("זּ")},
            { 0xdb, new SpecialCharacter("וּ")},
            { 0xdc, new SpecialCharacter("הּ")},
            { 0xdd, new SpecialCharacter("דּ")},
            { 0xde, new SpecialCharacter("גּ")},
            { 0xdf, new SpecialCharacter("בּ")},
            { 0x2b, new SpecialCharacter(".")},
            { 0x2c, new SpecialCharacter(",")}
        };

        private static readonly Dictionary<int, SpecialCharacter> ArabicCodes = new Dictionary<int, SpecialCharacter>
        {
            { 0xe081, new SpecialCharacter("أ")},
            { 0xe086, new SpecialCharacter("ځ")},
            { 0xe09b, new SpecialCharacter("ؤ")},
            { 0xe09c, new SpecialCharacter("ئ")},
            { 0xe181, new SpecialCharacter("إ")},
            { 0xe281, new SpecialCharacter("آ")},
            { 0xe781, new SpecialCharacter("اً")},
            { 0x80, new SpecialCharacter("ـ")},
            { 0x81, new SpecialCharacter("ا")},
            { 0x82, new SpecialCharacter("ب")},
            { 0x83, new SpecialCharacter("ت")},
            { 0x84, new SpecialCharacter("ث")},
            { 0x85, new SpecialCharacter("ج")},
            { 0x86, new SpecialCharacter("ح")},
            { 0x87, new SpecialCharacter("خ")},
            { 0x88, new SpecialCharacter("د")},
            { 0x89, new SpecialCharacter("ذ")},
            { 0x8A, new SpecialCharacter("ر")},
            { 0x8b, new SpecialCharacter("ز")},
            { 0x8c, new SpecialCharacter("س")},
            { 0x8d, new SpecialCharacter("ش")},
            { 0x8e, new SpecialCharacter("ص")},
            { 0x8f, new SpecialCharacter("ض")},
            { 0x90, new SpecialCharacter("ظ")},
            { 0x91, new SpecialCharacter("ط")},
            { 0x92, new SpecialCharacter("ع")},
            { 0x93, new SpecialCharacter("غ")},
            { 0x94, new SpecialCharacter("ف")},
            { 0x95, new SpecialCharacter("ق")},
            { 0x96, new SpecialCharacter("ك")},
            { 0x97, new SpecialCharacter("ل")},
            { 0x98, new SpecialCharacter("م")},
            { 0x99, new SpecialCharacter("ن")},
            { 0x9A, new SpecialCharacter("ه")},
            { 0x9b, new SpecialCharacter("و")},
            { 0x9c, new SpecialCharacter("ى")},
            { 0x9d, new SpecialCharacter("ة")},
            { 0x9e, new SpecialCharacter("لا")},
            { 0x9f, new SpecialCharacter("ي")},
            { 0xe09f, new SpecialCharacter("ي")},
            { 0xa0, new SpecialCharacter("ء")},
            { 0xad, new SpecialCharacter("ڪ")},
            { 0xae, new SpecialCharacter("ﺭ")},
            { 0xb0, new SpecialCharacter("ﺩ")},
            { 0xb1, new SpecialCharacter("ﯼ")},
            { 0xb3, new SpecialCharacter("۔")},
            { 0x3f, new SpecialCharacter("؟")},
            { 0x25, new SpecialCharacter("٪")},
            { 0x2c, new SpecialCharacter("،")},
            { 0x3b, new SpecialCharacter("؛")},
            { 0xe7, new SpecialCharacter("\u064B", true)},
            { 0xea, new SpecialCharacter("\u064C", true)},
            { 0xe8, new SpecialCharacter("\u064D", true)},
            { 0xe5, new SpecialCharacter("\u064E", true)},
            { 0xe4, new SpecialCharacter("\u064F", true)},
            { 0xe6, new SpecialCharacter("\u0650", true)},
            { 0xe3, new SpecialCharacter("\u0651", true)},
            { 0xe9, new SpecialCharacter("\u0652", true)},
            { 0xe2, new SpecialCharacter("\u0653", true)},
            { 0xe0, new SpecialCharacter("\u0654", true)},
            { 0xe1, new SpecialCharacter("\u0655", true)},
        };

        private static readonly Dictionary<int, SpecialCharacter> CyrillicCodes = new Dictionary<int, SpecialCharacter>
        {
            { 0x20, new SpecialCharacter(" ")},
            { 0x21, new SpecialCharacter("!")},
            { 0x22, new SpecialCharacter("Э")},
            { 0x23, new SpecialCharacter("/")},
            { 0x24, new SpecialCharacter("?")},
            { 0x25, new SpecialCharacter(":")},
            { 0x26, new SpecialCharacter(".")},
            { 0x27, new SpecialCharacter("э")},
            { 0x28, new SpecialCharacter("(")},
            { 0x29, new SpecialCharacter(")")},
            { 0x2a, new SpecialCharacter(";")},
            { 0x2b, new SpecialCharacter("+")},
            { 0x2c, new SpecialCharacter("б")},
            { 0x2d, new SpecialCharacter("-")},
            { 0x2e, new SpecialCharacter("ю")},
            { 0x3a, new SpecialCharacter("Ж")},
            { 0x3b, new SpecialCharacter("ж")},
            { 0x3c, new SpecialCharacter("<")},
            { 0x3d, new SpecialCharacter("=")},
            { 0x3e, new SpecialCharacter(">")},
            { 0x41, new SpecialCharacter("Ф")},
            { 0x42, new SpecialCharacter("И")},
            { 0x43, new SpecialCharacter("С")},
            { 0x44, new SpecialCharacter("В")},
            { 0x45, new SpecialCharacter("У")},
            { 0x46, new SpecialCharacter("А")},
            { 0x47, new SpecialCharacter("П")},
            { 0x48, new SpecialCharacter("Р")},
            { 0x49, new SpecialCharacter("Ш")},
            { 0x4a, new SpecialCharacter("О")},
            { 0x4b, new SpecialCharacter("Л")},
            { 0x4c, new SpecialCharacter("Д")},
            { 0x4d, new SpecialCharacter("Ь")},
            { 0x4e, new SpecialCharacter("Т")},
            { 0x4f, new SpecialCharacter("Щ")},
            { 0x50, new SpecialCharacter("З")},
            { 0x51, new SpecialCharacter("Q")},
            { 0x52, new SpecialCharacter("К")},
            { 0x53, new SpecialCharacter("Ы")},
            { 0x54, new SpecialCharacter("Е")},
            { 0x55, new SpecialCharacter("Г")},
            { 0x56, new SpecialCharacter("М")},
            { 0x57, new SpecialCharacter("Ц")},
            { 0x58, new SpecialCharacter("Ч")},
            { 0x59, new SpecialCharacter("Н")},
            { 0x5a, new SpecialCharacter("Я")},
            { 0x5b, new SpecialCharacter("х")},
            { 0x5d, new SpecialCharacter("ъ")},
            { 0x5e, new SpecialCharacter(",")},
            { 0x5f, new SpecialCharacter("-")},
            { 0x61, new SpecialCharacter("ф")},
            { 0x62, new SpecialCharacter("и")},
            { 0x63, new SpecialCharacter("с")},
            { 0x64, new SpecialCharacter("в")},
            { 0x65, new SpecialCharacter("у")},
            { 0x66, new SpecialCharacter("а")},
            { 0x67, new SpecialCharacter("п")},
            { 0x68, new SpecialCharacter("р")},
            { 0x69, new SpecialCharacter("ш")},
            { 0x6a, new SpecialCharacter("о")},
            { 0x6b, new SpecialCharacter("л")},
            { 0x6c, new SpecialCharacter("д")},
            { 0x6d, new SpecialCharacter("ь")},
            { 0x6e, new SpecialCharacter("т")},
            { 0x6f, new SpecialCharacter("щ")},
            { 0x70, new SpecialCharacter("з")},
            { 0x72, new SpecialCharacter("к")},
            { 0x73, new SpecialCharacter("ы")},
            { 0x74, new SpecialCharacter("е")},
            { 0x75, new SpecialCharacter("г")},
            { 0x76, new SpecialCharacter("м")},
            { 0x77, new SpecialCharacter("ц")},
            { 0x78, new SpecialCharacter("ч")},
            { 0x79, new SpecialCharacter("н")},
            { 0x7a, new SpecialCharacter("я")},
            { 0x7b, new SpecialCharacter("Х")},
            { 0x7d, new SpecialCharacter("Ъ")},
            { 0x80, new SpecialCharacter("Б")},
            { 0x81, new SpecialCharacter("Ю")},
            { 0x82, new SpecialCharacter("Ђ")},
            { 0x84, new SpecialCharacter("Є")},
            { 0x85, new SpecialCharacter("Ѕ")},
            { 0x86, new SpecialCharacter("І")},
            { 0x88, new SpecialCharacter("Ј")},
            { 0x89, new SpecialCharacter("Љ")},
            { 0x8a, new SpecialCharacter("Њ")},
            { 0x8b, new SpecialCharacter("Ћ")},
            { 0x8c, new SpecialCharacter("Ґ")},
            { 0x8d, new SpecialCharacter("ґ")},
            { 0x8f, new SpecialCharacter("Џ")},
            { 0x90, new SpecialCharacter("№")},
            { 0x92, new SpecialCharacter("ђ")},
            { 0x94, new SpecialCharacter("є")},
            { 0x95, new SpecialCharacter("ѕ")},
            { 0x96, new SpecialCharacter("і")},
            { 0x98, new SpecialCharacter("ј")},
            { 0x99, new SpecialCharacter("љ")},
            { 0x9a, new SpecialCharacter("њ")},
            { 0x9b, new SpecialCharacter("ћ")},
            { 0x9d, new SpecialCharacter("§")},
            { 0x9f, new SpecialCharacter("џ")},
            { 0xa2, new SpecialCharacter("%")},
            { 0xa4, new SpecialCharacter("&")},
            { 0xac, new SpecialCharacter("C")},
            { 0xad, new SpecialCharacter("D")},
            { 0xae, new SpecialCharacter("E")},
            { 0xaf, new SpecialCharacter("F")},
            { 0xb0, new SpecialCharacter("G")},
            { 0xb1, new SpecialCharacter("H")},
            { 0xb2, new SpecialCharacter("'")},
            { 0xb3, new SpecialCharacter("\"")},
            { 0xb4, new SpecialCharacter("I")},
            { 0xb5, new SpecialCharacter("J")},
            { 0xb6, new SpecialCharacter("K")},
            { 0xb7, new SpecialCharacter("L")},
            { 0xb8, new SpecialCharacter("M")},
            { 0xb9, new SpecialCharacter("N")},
            { 0xba, new SpecialCharacter("P")},
            { 0xbb, new SpecialCharacter("Q")},
            { 0xbc, new SpecialCharacter("R")},
            { 0xbd, new SpecialCharacter("S")},
            { 0xbe, new SpecialCharacter("T")},
            { 0xbf, new SpecialCharacter("U")},
            { 0xc0, new SpecialCharacter("V")},
            { 0xc1, new SpecialCharacter("*")},
            { 0xc2, new SpecialCharacter("W")},
            { 0xc3, new SpecialCharacter("X")},
            { 0xc4, new SpecialCharacter("Y")},
            { 0xc5, new SpecialCharacter("Z")},
            { 0xc6, new SpecialCharacter("b")},
            { 0xc7, new SpecialCharacter("c")},
            { 0xc8, new SpecialCharacter("d")},
            { 0xc9, new SpecialCharacter("e")},
            { 0xca, new SpecialCharacter("f")},
            { 0xcb, new SpecialCharacter("g")},
            { 0xcc, new SpecialCharacter("h")},
            { 0xcd, new SpecialCharacter("i")},
            { 0xce, new SpecialCharacter("j")},
            { 0xcf, new SpecialCharacter("k")},
            { 0xd0, new SpecialCharacter("—")},
            { 0xd1, new SpecialCharacter("l")},
            { 0xd2, new SpecialCharacter("m")},
            { 0xd3, new SpecialCharacter("n")},
            { 0xd4, new SpecialCharacter("o")},
            { 0xd5, new SpecialCharacter("p")},
            { 0xd6, new SpecialCharacter("q")},
            { 0xd7, new SpecialCharacter("r")},
            { 0xd8, new SpecialCharacter("s")},
            { 0xd9, new SpecialCharacter("t")},
            { 0xda, new SpecialCharacter("u")},
            { 0xdb, new SpecialCharacter("v")},
            { 0xdc, new SpecialCharacter("w")},
            { 0xdd, new SpecialCharacter("э")},
            { 0xde, new SpecialCharacter("ю")},
            { 0xdf, new SpecialCharacter("z")},
            { 0xe3, new SpecialCharacter("`")},
            { 0xe5, new SpecialCharacter("¨")},
            { 0xe020, new SpecialCharacter("˘")},
            { 0xe03a, new SpecialCharacter("Ӂ")},
            { 0xe03b, new SpecialCharacter("ӂ")},
            { 0xe042, new SpecialCharacter("Й")},
            { 0xe045, new SpecialCharacter("Ў")},
            { 0xe046, new SpecialCharacter("Ӑ")},
            { 0xe054, new SpecialCharacter("Ӗ")},
            { 0xe062, new SpecialCharacter("й")},
            { 0xe065, new SpecialCharacter("ў")},
            { 0xe066, new SpecialCharacter("ӑ")},
            { 0xe074, new SpecialCharacter("ӗ")},
            { 0xe220, new SpecialCharacter("´")},
            { 0xe252, new SpecialCharacter("Ќ")},
            { 0xe255, new SpecialCharacter("Ѓ")},
            { 0xe272, new SpecialCharacter("ќ")},
            { 0xe275, new SpecialCharacter("ѓ")},
            { 0xe342, new SpecialCharacter("Ѝ")},
            { 0xe354, new SpecialCharacter("Ѐ")},
            { 0xe362, new SpecialCharacter("ѝ")},
            { 0xe374, new SpecialCharacter("ѐ")},
            { 0xe522, new SpecialCharacter("Ӭ")},
            { 0xe527, new SpecialCharacter("ӭ")},
            { 0xe53a, new SpecialCharacter("Ӝ")},
            { 0xe53b, new SpecialCharacter("ӝ")},
            { 0xe542, new SpecialCharacter("Ӥ")},
            { 0xe545, new SpecialCharacter("Ӱ")},
            { 0xe546, new SpecialCharacter("Ӓ")},
            { 0xe54a, new SpecialCharacter("Ӧ")},
            { 0xe550, new SpecialCharacter("Ӟ")},
            { 0xe553, new SpecialCharacter("Ӹ")},
            { 0xe554, new SpecialCharacter("Ё")},
            { 0xe558, new SpecialCharacter("Ӵ")},
            { 0xe562, new SpecialCharacter("ӥ")},
            { 0xe565, new SpecialCharacter("ӱ")},
            { 0xe566, new SpecialCharacter("ӓ")},
            { 0xe56a, new SpecialCharacter("ӧ")},
            { 0xe570, new SpecialCharacter("ӟ")},
            { 0xe573, new SpecialCharacter("ӹ")},
            { 0xe574, new SpecialCharacter("ё")},
            { 0xe578, new SpecialCharacter("ӵ")},
            { 0xe586, new SpecialCharacter("Ї")},
            { 0xe596, new SpecialCharacter("ї")},
            { 0xe5b4, new SpecialCharacter("Ї")},
        };

        private static readonly Dictionary<int, SpecialCharacter> KoreanCodes = new Dictionary<int, SpecialCharacter>
        {
            { 0x20, new SpecialCharacter(" ")}
        };

        private static readonly Dictionary<int, SpecialCharacter> GreekCodes = new Dictionary<int, SpecialCharacter>
        {
            { 0x20, new SpecialCharacter(" ") },
            { 0x21, new SpecialCharacter("!") },
            { 0x22, new SpecialCharacter("\"") },
            { 0x23, new SpecialCharacter("£") },
            { 0x24, new SpecialCharacter("$") },
            { 0x25, new SpecialCharacter("%") },
            { 0x26, new SpecialCharacter("&") },
            { 0x27, new SpecialCharacter("'") },
            { 0x28, new SpecialCharacter("(") },
            { 0x29, new SpecialCharacter(")") },
            { 0x2A, new SpecialCharacter("*") },
            { 0x2B, new SpecialCharacter("+") },
            { 0x2C, new SpecialCharacter(",") },
            { 0x2D, new SpecialCharacter("-") },
            { 0x2E, new SpecialCharacter(".") },
            { 0x2F, new SpecialCharacter("/") },
            { 0x3A, new SpecialCharacter(":") },
            { 0x3B, new SpecialCharacter(";") },
            { 0x3C, new SpecialCharacter("<") },
            { 0x3D, new SpecialCharacter("=") },
            { 0x3E, new SpecialCharacter(">") },
            { 0x3F, new SpecialCharacter("?") },
            { 0x40, new SpecialCharacter("@") },
            { 0x41, new SpecialCharacter("Α") },
            { 0x42, new SpecialCharacter("Β") },
            { 0x43, new SpecialCharacter("Γ") },
            { 0x44, new SpecialCharacter("Δ") },
            { 0x45, new SpecialCharacter("Ε") },
            { 0x46, new SpecialCharacter("Ζ") },
            { 0x47, new SpecialCharacter("Η") },
            { 0x48, new SpecialCharacter("Θ") },
            { 0x49, new SpecialCharacter("Ι") },
            { 0x4A, new SpecialCharacter("Κ") },
            { 0x4B, new SpecialCharacter("Λ") },
            { 0x4C, new SpecialCharacter("Μ") },
            { 0x4D, new SpecialCharacter("Ν") },
            { 0x4E, new SpecialCharacter("Ξ") },
            { 0x4F, new SpecialCharacter("Ο") },
            { 0x50, new SpecialCharacter("Π") },
            { 0x51, new SpecialCharacter("Ρ") },
            { 0x52, new SpecialCharacter("R") },
            { 0x53, new SpecialCharacter("Σ") },
            { 0x54, new SpecialCharacter("Τ") },
            { 0x55, new SpecialCharacter("Υ") },
            { 0x56, new SpecialCharacter("Φ") },
            { 0x57, new SpecialCharacter("Χ") },
            { 0x58, new SpecialCharacter("Ψ") },
            { 0x59, new SpecialCharacter("Ω") },
            { 0x5A, new SpecialCharacter("Z") },

            { 0x5F, new SpecialCharacter("-") },

            { 0x61, new SpecialCharacter("α") },
            { 0x62, new SpecialCharacter("β") },
            { 0x63, new SpecialCharacter("γ") },
            { 0x64, new SpecialCharacter("δ") },
            { 0x65, new SpecialCharacter("ε") },
            { 0x66, new SpecialCharacter("ζ") },
            { 0x67, new SpecialCharacter("η") },
            { 0x68, new SpecialCharacter("θ") },
            { 0x69, new SpecialCharacter("ι") },
            { 0x6A, new SpecialCharacter("κ") },
            { 0x6B, new SpecialCharacter("λ") },
            { 0x6C, new SpecialCharacter("μ") },
            { 0x6D, new SpecialCharacter("ν") },
            { 0x6E, new SpecialCharacter("ξ") },
            { 0x6F, new SpecialCharacter("ο") },
            { 0x70, new SpecialCharacter("π") },
            { 0x71, new SpecialCharacter("ρ") },
            { 0x72, new SpecialCharacter("ς") },
            { 0x73, new SpecialCharacter("σ") },
            { 0x74, new SpecialCharacter("τ") },
            { 0x75, new SpecialCharacter("υ") },
            { 0x76, new SpecialCharacter("φ") },
            { 0x77, new SpecialCharacter("χ") },
            { 0x78, new SpecialCharacter("ψ") },
            { 0x79, new SpecialCharacter("ω") },
            { 0x7A, new SpecialCharacter("z") },
            { 0x7E, new SpecialCharacter("§") },
            { 0x80, new SpecialCharacter("#") },
            { 0x81, new SpecialCharacter("ß") },
            { 0x82, new SpecialCharacter("²") },
            { 0x83, new SpecialCharacter("³") },
            { 0x84, new SpecialCharacter("«") },
            { 0x85, new SpecialCharacter("»") },
            { 0x86, new SpecialCharacter("±") },
            { 0x87, new SpecialCharacter("þ") },
            { 0x88, new SpecialCharacter("Þ") },
            { 0x89, new SpecialCharacter("ð") },
            { 0x8C, new SpecialCharacter("A") },
            { 0x8D, new SpecialCharacter("B") },
            { 0x8E, new SpecialCharacter("C") },
            { 0x8F, new SpecialCharacter("D") },
            { 0x90, new SpecialCharacter("E") },
            { 0x91, new SpecialCharacter("F") },
            { 0x92, new SpecialCharacter("G") },
            { 0x93, new SpecialCharacter("H") },
            { 0x94, new SpecialCharacter("I") },
            { 0x95, new SpecialCharacter("J") },
            { 0x96, new SpecialCharacter("K") },
            { 0x97, new SpecialCharacter("L") },
            { 0x98, new SpecialCharacter("M") },
            { 0x99, new SpecialCharacter("N") },
            { 0x9A, new SpecialCharacter("O") },
            { 0x9B, new SpecialCharacter("P") },
            { 0x9C, new SpecialCharacter("Q") },
            { 0x9D, new SpecialCharacter("R") },
            { 0x9E, new SpecialCharacter("S") },
            { 0x9F, new SpecialCharacter("T") },
            { 0xA0, new SpecialCharacter("U") },
            { 0xA1, new SpecialCharacter("V") },
            { 0xA2, new SpecialCharacter("W") },
            { 0xA3, new SpecialCharacter("X") },
            { 0xA4, new SpecialCharacter("Y") },
            { 0xA5, new SpecialCharacter("Z") },
            { 0xA9, new SpecialCharacter("°") },
            { 0xAB, new SpecialCharacter("½") },
            { 0xAC, new SpecialCharacter("a") },
            { 0xAD, new SpecialCharacter("b") },
            { 0xAE, new SpecialCharacter("c") },
            { 0xAF, new SpecialCharacter("d") },
            { 0xB0, new SpecialCharacter("e") },
            { 0xB1, new SpecialCharacter("f") },
            { 0xB2, new SpecialCharacter("g") },
            { 0xB3, new SpecialCharacter("h") },
            { 0xB4, new SpecialCharacter("i") },
            { 0xB5, new SpecialCharacter("j") },
            { 0xB6, new SpecialCharacter("k") },
            { 0xB7, new SpecialCharacter("l") },
            { 0xB8, new SpecialCharacter("m") },
            { 0xB9, new SpecialCharacter("n") },
            { 0xBA, new SpecialCharacter("o") },
            { 0xBB, new SpecialCharacter("p") },
            { 0xBC, new SpecialCharacter("q") },
            { 0xBD, new SpecialCharacter("r") },
            { 0xBE, new SpecialCharacter("s") },
            { 0xBF, new SpecialCharacter("t") },
            { 0xC0, new SpecialCharacter("u") },
            { 0xC1, new SpecialCharacter("v") },
            { 0xC2, new SpecialCharacter("w") },
            { 0xC3, new SpecialCharacter("x") },
            { 0xC4, new SpecialCharacter("y") },
            { 0xC5, new SpecialCharacter("z") },

            { 0x202A, new SpecialCharacter("®") },
            { 0xE241, new SpecialCharacter("Ά") },
            { 0xE242, new SpecialCharacter("Β́") },
            { 0xE243, new SpecialCharacter("Γ́") },
            { 0xE244, new SpecialCharacter("Δ́") },
            { 0xE245, new SpecialCharacter("Έ") },
            { 0xE246, new SpecialCharacter("Ζ́") },
            { 0xE247, new SpecialCharacter("Ή") },
            { 0xE248, new SpecialCharacter("Θ́") },
            { 0xE249, new SpecialCharacter("Ί") },
            { 0xE24A, new SpecialCharacter("Κ́") },
            { 0xE24B, new SpecialCharacter("Λ́") },
            { 0xE24C, new SpecialCharacter("Μ́") },
            { 0xE24D, new SpecialCharacter("Ν́") },
            { 0xE24E, new SpecialCharacter("Ξ́") },
            { 0xE24F, new SpecialCharacter("Ό") },
            { 0xE255, new SpecialCharacter("Ύ") },
            { 0xE258, new SpecialCharacter("Ψ́") },
            { 0xE259, new SpecialCharacter("Ώ") },
            { 0xE261, new SpecialCharacter("ά") },
            { 0xE262, new SpecialCharacter("β́") },
            { 0xE263, new SpecialCharacter("γ́") },
            { 0xE264, new SpecialCharacter("δ́") },
            { 0xE265, new SpecialCharacter("έ") },
            { 0xE266, new SpecialCharacter("ζ") },
            { 0xE267, new SpecialCharacter("ή") },
            { 0xE268, new SpecialCharacter("θ́") },
            { 0xE269, new SpecialCharacter("ί") },
            { 0xE26A, new SpecialCharacter("κ́") },
            { 0xE26B, new SpecialCharacter("λ́") },
            { 0xE26C, new SpecialCharacter("μ́") },
            { 0xE26D, new SpecialCharacter("ν́") },
            { 0xE26E, new SpecialCharacter("ξ") },
            { 0xE26F, new SpecialCharacter("ό") },
            { 0xE270, new SpecialCharacter("π") },
            { 0xE271, new SpecialCharacter("ρ́") },
            { 0xE272, new SpecialCharacter("ς́") },
            { 0xE273, new SpecialCharacter("σ́") },
            { 0xE274, new SpecialCharacter("τ́") },
            { 0xE275, new SpecialCharacter("ύ") },
            { 0xE276, new SpecialCharacter("φ́") },
            { 0xE277, new SpecialCharacter("χ́") },
            { 0xE278, new SpecialCharacter("ψ́") },
            { 0xE279, new SpecialCharacter("ώ") },
            { 0xE27B, new SpecialCharacter("ί") },
            { 0xE320, new SpecialCharacter("`") },
            { 0xE399, new SpecialCharacter("Ǹ") },
            { 0xE39A, new SpecialCharacter("Ò") },
            { 0xE3A0, new SpecialCharacter("Ù") },
            { 0xE3A2, new SpecialCharacter("Ẁ") },
            { 0xE3A4, new SpecialCharacter("Ỳ") },
            { 0xE3B9, new SpecialCharacter("ǹ") },
            { 0xE3C2, new SpecialCharacter("ẁ") },
            { 0xE3C4, new SpecialCharacter("ỳ") },
            { 0xE549, new SpecialCharacter("Ϊ") },
            { 0xE555, new SpecialCharacter("Ϋ") },
            { 0xE561, new SpecialCharacter("α̈") },
            { 0xE562, new SpecialCharacter("β̈") },
            { 0xE563, new SpecialCharacter("γ̈") },
            { 0xE564, new SpecialCharacter("δ̈") },
            { 0xE565, new SpecialCharacter("ε̈") },
            { 0xE566, new SpecialCharacter("ζ̈") },
            { 0xE567, new SpecialCharacter("η̈") },
            { 0xE568, new SpecialCharacter("θ̈") },
            { 0xE569, new SpecialCharacter("ϊ") },
            { 0xE56A, new SpecialCharacter("κ̈") },
            { 0xE56B, new SpecialCharacter("λ̈") },
            { 0xE56C, new SpecialCharacter("μ̈") },
            { 0xE56D, new SpecialCharacter("ν̈") },
            { 0xE56E, new SpecialCharacter("ξ̈") },
            { 0xE56F, new SpecialCharacter("ο̈") },
            { 0xE570, new SpecialCharacter("π̈") },
            { 0xE571, new SpecialCharacter("ρ") },
            { 0xE572, new SpecialCharacter("ς̈") },
            { 0xE573, new SpecialCharacter("σ̈") },
            { 0xE574, new SpecialCharacter("τ̈") },
            { 0xE575, new SpecialCharacter("ϋ") },
            { 0xE576, new SpecialCharacter("φ̈") },
            { 0xE577, new SpecialCharacter("χ̈") },
            { 0xE578, new SpecialCharacter("ψ̈") },
            { 0xE579, new SpecialCharacter("ω̈") },
            { 0xE57B, new SpecialCharacter("ϊ") },
            { 0xE5E269, new SpecialCharacter("ΐ") },
            { 0xE5E275, new SpecialCharacter("ΰ") }
        };

        private string _fileName = string.Empty;

        public int CodePage { get; set; } = -1;

        public override string Extension => ".pac";

        public const string NameOfFormat = "PAC (Screen Electronics)";

        public override string Name => NameOfFormat;

        public bool Save(string fileName, Subtitle subtitle)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                return Save(fileName, fs, subtitle);
            }
        }

        public bool Save(string fileName, Stream stream, Subtitle subtitle)
        {
            _fileName = fileName;

            // header
            stream.WriteByte(1);
            for (int i = 1; i < 23; i++)
            {
                stream.WriteByte(0);
            }

            stream.WriteByte(0x60);

            // paragraphs
            int number = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                WriteParagraph(stream, p, number, number + 1 == subtitle.Paragraphs.Count);
                number++;
            }

            // footer
            stream.WriteByte(0xff);
            for (int i = 0; i < 11; i++)
            {
                stream.WriteByte(0);
            }

            stream.WriteByte(0x11);
            stream.WriteByte(0);
            byte[] footerBuffer = Encoding.ASCII.GetBytes("dummy end of file");
            stream.Write(footerBuffer, 0, footerBuffer.Length);
            return true;
        }

        private void WriteParagraph(Stream fs, Paragraph p, int number, bool isLast)
        {
            WriteTimeCode(fs, p.StartTime);
            WriteTimeCode(fs, p.EndTime);

            if (CodePage == -1)
            {
                GetCodePage(null, 0, 0);
            }

            byte alignment = 2; // center
            byte verticalAlignment = 0x0a; // bottom
            if (!p.Text.Contains(Environment.NewLine))
            {
                verticalAlignment = 0x0b;
            }

            string text = p.Text;
            if (text.StartsWith("{\\an1}", StringComparison.Ordinal) || text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an7}", StringComparison.Ordinal))
            {
                alignment = 1; // left
            }
            else if (text.StartsWith("{\\an3}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
            {
                alignment = 0; // right
            }
            if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
            {
                verticalAlignment = 0; // top
            }
            else if (text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an5}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal))
            {
                verticalAlignment = 5; // center
            }
            if (text.Length >= 6 && text[0] == '{' && text[5] == '}')
            {
                text = text.Remove(0, 6);
            }

            text = MakePacItalicsAndRemoveOtherTags(text);

            Encoding encoding = GetEncoding(CodePage);
            byte[] textBuffer;

            if (CodePage == CodePageArabic)
            {
                textBuffer = GetArabicBytes(Utilities.FixEnglishTextInRightToLeftLanguage(text, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"), alignment);
            }
            else if (CodePage == CodePageHebrew)
            {
                textBuffer = GetHebrewBytes(Utilities.FixEnglishTextInRightToLeftLanguage(text, "0123456789abcdefghijklmnopqrstuvwxyz"), alignment);
            }
            else if (CodePage == CodePageLatin || CodePage == CodePageLatinCzech)
            {
                textBuffer = GetLatinBytes(encoding, text, alignment, null);
            }
            else if (CodePage == CodePageCyrillic)
            {
                textBuffer = GetCyrillicBytes(text, alignment);
            }
            else if (CodePage == CodePageGreek)
            {
                textBuffer = GetGreekBytes(text, alignment);
            }
            else if (CodePage == CodePageLatinTurkish)
            {
                textBuffer = GetLatinBytes(encoding, text, alignment, LatinTurkishOverrides);
            }
            else if (CodePage == CodePageChineseTraditional)
            {
                textBuffer = GetW16Bytes(text, alignment, EncodingChineseTraditional);
            }
            else if (CodePage == CodePageChineseSimplified)
            {
                textBuffer = GetW16Bytes(text, alignment, EncodingChineseSimplified);
            }
            else if (CodePage == CodePageKorean)
            {
                textBuffer = GetW16Bytes(text, alignment, EncodingKorean);
            }
            else if (CodePage == CodePageJapanese)
            {
                textBuffer = GetW16Bytes(text, alignment, EncodingJapanese);
            }
            else if (CodePage == CodePageThai)
            {
                textBuffer = encoding.GetBytes(text.Replace('ต', '€'));
            }
            else
            {
                textBuffer = encoding.GetBytes(text);
            }

            // write text length
            var length = (UInt16)(textBuffer.Length + 4);
            fs.Write(BitConverter.GetBytes(length), 0, 2);

            fs.WriteByte(verticalAlignment); // fs.WriteByte(0x0a); // sometimes 0x0b? - this seems to be vertical alignment - 0 to 11
            fs.WriteByte(0xfe);
            fs.WriteByte(alignment); //2=centered, 1=left aligned, 0=right aligned, 09=Fount2 (large font),
            //55=safe area override (too long line), 0A=Fount2 + centered, 06=centered + safe area override
            fs.WriteByte(0x03);

            fs.Write(textBuffer, 0, textBuffer.Length);

            if (!isLast)
            {
                fs.WriteByte(0);
                fs.WriteByte((byte)((number + 1) % 256));
                fs.WriteByte((byte)((number + 1) / 256));
                fs.WriteByte(0x60);
            }
        }

        internal static string MakePacItalicsAndRemoveOtherTags(string text)
        {
            text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagFont, HtmlUtil.TagUnderline).Trim();
            if (!text.Contains("<i>", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            text = text.Replace("<I>", "<i>");
            text = text.Replace("</I>", "</i>");

            if (Utilities.CountTagInText(text, "<i>") == 1 && text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal))
            {
                return "<" + HtmlUtil.RemoveHtmlTags(text).Replace(Environment.NewLine, Environment.NewLine + "<");
            }

            var sb = new StringBuilder();
            var parts = text.SplitToLines();
            foreach (string line in parts)
            {
                string s = line.Trim();
                if (Utilities.CountTagInText(s, "<i>") == 1 && s.StartsWith("<i>", StringComparison.Ordinal) && s.EndsWith("</i>", StringComparison.Ordinal))
                {
                    sb.AppendLine("<" + HtmlUtil.RemoveHtmlTags(s));
                }
                else
                {
                    s = s.Replace("</i>", "___@___");
                    s = s.Replace("<i>", "<");
                    s = s.Replace("___@___", ">");
                    s = s.Replace(" <", "<");
                    s = s.Replace("> ", ">");
                    sb.AppendLine(s);
                }
            }
            return sb.ToString().Trim();
        }

        internal static void WriteTimeCode(Stream fs, TimeCode timeCode)
        {
            // write four bytes time code
            string highPart = $"{timeCode.Hours:00}{timeCode.Minutes:00}";
            byte frames = (byte)MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds);
            string lowPart = $"{timeCode.Seconds:00}{frames:00}";

            int high = int.Parse(highPart);
            if (high < 256)
            {
                fs.WriteByte((byte)high);
                fs.WriteByte(0);
            }
            else
            {
                fs.WriteByte((byte)(high % 256));
                fs.WriteByte((byte)(high / 256));
            }

            int low = int.Parse(lowPart);
            if (low < 256)
            {
                fs.WriteByte((byte)low);
                fs.WriteByte(0);
            }
            else
            {
                fs.WriteByte((byte)(low % 256));
                fs.WriteByte((byte)(low / 256));
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    var fi = new FileInfo(fileName);
                    if (fi.Length > 65 && fi.Length < 1024000) // not too small or too big
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

                        if (buffer[00] == 1 && // These bytes seems to be PAC files... TODO: Verify!
                            buffer[01] == 0 &&
                            buffer[02] == 0 &&
                            buffer[03] == 0 &&
                            buffer[04] == 0 &&
                            buffer[05] == 0 &&
                            buffer[06] == 0 &&
                            buffer[07] == 0 &&
                            buffer[08] == 0 &&
                            buffer[09] == 0 &&
                            buffer[10] == 0 &&
                            buffer[11] == 0 &&
                            buffer[12] == 0 &&
                            buffer[13] == 0 &&
                            buffer[14] == 0 &&
                            buffer[15] == 0 &&
                            buffer[16] == 0 &&
                            buffer[17] == 0 &&
                            buffer[18] == 0 &&
                            buffer[19] == 0 &&
                            buffer[20] == 0 &&
                            //buffer[21] < 10 && // start from number
                            //buffer[22] == 0 &&
                            (buffer[23] >= 0x60 && buffer[23] <= 0x70) &&
                            fileName.EndsWith(".pac", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _fileName = fileName;
            LoadSubtitle(subtitle, FileUtil.ReadAllBytesShared(fileName));
        }

        public void LoadSubtitle(Subtitle subtitle, byte[] buffer)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;

            bool usesSecondaryCodePage = UsesSecondaryCodePage(buffer);
            int index = 0;
            while (index < buffer.Length)
            {
                Paragraph p = GetPacParagraph(ref index, buffer, usesSecondaryCodePage);
                if (p != null)
                {
                    subtitle.Paragraphs.Add(p);
                }
            }
            subtitle.Renumber();
        }

        private bool UsesSecondaryCodePage(byte[] buffer)
        {
            bool? firstIsSecondary = null;
            int secondaryUse = 0;
            for (int i = 15; i < buffer.Length - 1; i++)
            {
                if (buffer[i] == 0xFE && (buffer[i - 15] == 0x60 || buffer[i - 15] == 0x61 || buffer[i - 12] == 0x60 || buffer[i - 12] == 0x61))
                {
                    bool secondary = (buffer[i + 1] & 0x08) != 0;
                    firstIsSecondary = firstIsSecondary ?? secondary;
                    if (secondary)
                    {
                        secondaryUse++;
                    }
                }
            }

            return secondaryUse > (firstIsSecondary.GetValueOrDefault() ? 1 : 0);
        }

        private double _lastStartTotalSeconds;
        private double _lastEndTotalSeconds;

        private Paragraph GetPacParagraph(ref int index, byte[] buffer, bool usesSecondaryCodePage)
        {
            bool isStory = index < 15;
            if (isStory)
            {
                index = 15;
            }
            while (true)
            {
                index++;
                if (index + 20 >= buffer.Length)
                {
                    return null;
                }

                if (buffer[index] == 0xFE && (buffer[index - 15] == 0x60 || buffer[index - 15] == 0x61))
                {
                    break;
                }

                if (buffer[index] == 0xFE && (buffer[index - 12] == 0x60 || buffer[index - 12] == 0x61))
                {
                    break;
                }
            }

            int feIndex = index;
            const int endDelimiter = 0x00;
            byte alignment = buffer[feIndex + 1];
            bool isSecondaryCodePage = (alignment & 0x08) != 0;
            alignment &= 0x07;

            var p = new Paragraph();

            int timeStartIndex = feIndex - 15;
            if (buffer[timeStartIndex] == 0x60)
            {
                p.StartTime = GetTimeCode(timeStartIndex + 1, buffer);
                p.EndTime = GetTimeCode(timeStartIndex + 5, buffer);
            }
            else if (buffer[timeStartIndex + 3] == 0x60)
            {
                timeStartIndex += 3;
                p.StartTime = GetTimeCode(timeStartIndex + 1, buffer);
                p.EndTime = GetTimeCode(timeStartIndex + 5, buffer);
            }
            else if (buffer[timeStartIndex] == 0x61)
            {
                p.StartTime = GetTimeCode(timeStartIndex + 1, buffer);
                p.EndTime = GetTimeCode(timeStartIndex + 5, buffer);
                int length = buffer[timeStartIndex + 9] + buffer[timeStartIndex + 10] * 256;
                if (length < 1 || length > 200 ||
                    p.StartTime.TotalSeconds - _lastStartTotalSeconds < 1 || p.StartTime.TotalSeconds - _lastStartTotalSeconds > 1500 ||
                    p.EndTime.TotalSeconds - _lastEndTotalSeconds < 1 || p.EndTime.TotalSeconds - _lastEndTotalSeconds > 1500)
                {
                    return null;
                }
            }
            else if (buffer[timeStartIndex + 3] == 0x61)
            {
                timeStartIndex += 3;
                p.StartTime = GetTimeCode(timeStartIndex + 1, buffer);
                p.EndTime = GetTimeCode(timeStartIndex + 5, buffer);
                int length = buffer[timeStartIndex + 9] + buffer[timeStartIndex + 10] * 256;
                if (length < 1 || length > 200 ||
                    p.StartTime.TotalSeconds - _lastStartTotalSeconds < 1 || p.StartTime.TotalSeconds - _lastStartTotalSeconds > 1500 ||
                    p.EndTime.TotalSeconds - _lastEndTotalSeconds < 1 || p.EndTime.TotalSeconds - _lastEndTotalSeconds > 1500)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            int textLength = buffer[timeStartIndex + 9] + buffer[timeStartIndex + 10] * 256;
            if (textLength > 500)
            {
                return null; // probably not correct index
            }

            _lastStartTotalSeconds = p.StartTime.TotalSeconds;
            _lastEndTotalSeconds = p.EndTime.TotalSeconds;

            int maxIndex = timeStartIndex + 10 + textLength;

            byte verticalAlignment = buffer[timeStartIndex + 11];

            if (CodePage == -1)
            {
                GetCodePage(buffer, index, endDelimiter);
            }

            var overrides = (CodePage == CodePageLatinTurkish) ? LatinTurkishOverrides : null;
            var sb = new StringBuilder();
            index = feIndex + 3;
            bool w16 = buffer[index] == 0x1f && Encoding.ASCII.GetString(buffer, index + 1, 3) == "W16";
            if (w16)
            {
                index += 5;
            }

            while (index < buffer.Length && index <= maxIndex) // buffer[index] != endDelimiter)
            {
                if (buffer.Length > index + 3 && buffer[index] == 0x1f && Encoding.ASCII.GetString(buffer, index + 1, 3) == "W16")
                {
                    w16 = true;
                    index += 5;
                }
                else if (buffer.Length > index + 2 && buffer[index] == 0x1f && buffer[index + 1] == 'C' && char.IsDigit((char)buffer[index + 2]))
                {
                    index += 3;
                    continue;
                }

                if (w16)
                {
                    if (buffer[index] == 0xFE)
                    {
                        alignment = buffer[index + 1];
                        isSecondaryCodePage = (alignment & 0x08) != 0;
                        alignment &= 0x07;
                        sb.AppendLine();
                        w16 = buffer[index + 3] == 0x1f && Encoding.ASCII.GetString(buffer, index + 4, 3) == "W16";
                        if (w16)
                        {
                            index += 5;
                        }

                        index += 2;
                    }
                    else
                    {
                        if (buffer[index] == 0)
                        {
                            sb.Append(Encoding.ASCII.GetString(buffer, index + 1, 1));
                        }
                        else if (buffer.Length > index + 1)
                        {
                            if (CodePage == CodePageChineseSimplified)
                            {
                                sb.Append(Encoding.GetEncoding(EncodingChineseSimplified).GetString(buffer, index, 2));
                            }
                            else if (CodePage == CodePageKorean)
                            {
                                sb.Append(Encoding.GetEncoding(EncodingKorean).GetString(buffer, index, 2));
                            }
                            else if (CodePage == CodePageJapanese)
                            {
                                sb.Append(Encoding.GetEncoding(EncodingJapanese).GetString(buffer, index, 2));
                            }
                            else
                            {
                                sb.Append(Encoding.GetEncoding(EncodingChineseTraditional).GetString(buffer, index, 2));
                            }
                        }

                        index++;
                    }
                }
                else if (buffer[index] == 0xFF)
                {
                    sb.Append(' ');
                }
                else if (buffer[index] == 0xFE)
                {
                    alignment = buffer[index + 1];
                    isSecondaryCodePage = (alignment & 0x08) != 0;
                    alignment &= 0x07;
                    sb.AppendLine();
                    index += 2;
                }
                else if (CodePage == CodePageLatin || CodePage == CodePageLatinTurkish || CodePage == CodePageLatinCzech
                         || (usesSecondaryCodePage && !isSecondaryCodePage))
                {
                    sb.Append(GetLatinString(GetEncoding(CodePage), buffer, ref index, overrides));
                }
                else if (CodePage == CodePageArabic)
                {
                    sb.Append(GetArabicString(buffer, ref index));
                }
                else if (CodePage == CodePageHebrew)
                {
                    sb.Append(GetHebrewString(buffer, ref index));
                }
                else if (CodePage == CodePageCyrillic)
                {
                    sb.Append(GetCyrillicString(buffer, ref index));
                }
                else if (CodePage == CodePageGreek)
                {
                    sb.Append(GetGreekString(buffer, ref index));
                }
                else if (CodePage == CodePageThai)
                {
                    sb.Append(GetEncoding(CodePage).GetString(buffer, index, 1).Replace("€", "ต"));
                }
                else
                {
                    sb.Append(GetEncoding(CodePage).GetString(buffer, index, 1));
                }

                index++;
            }
            if (index + 20 >= buffer.Length)
            {
                return null;
            }

            p.Text = sb.ToString();
            p.Text = p.Text.Replace("\0", string.Empty);
            p.Text = FixItalics(p.Text);
            if (CodePage == CodePageArabic)
            {
                p.Text = Utilities.FixEnglishTextInRightToLeftLanguage(p.Text, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }
            else if (CodePage == CodePageHebrew)
            {
                p.Text = Utilities.FixEnglishTextInRightToLeftLanguage(p.Text, "0123456789abcdefghijklmnopqrstuvwxyz");
            }

            if (verticalAlignment < 5)
            {
                if (alignment == 1) // left
                {
                    p.Text = "{\\an7}" + p.Text;
                }
                else if (alignment == 0) // right
                {
                    p.Text = "{\\an9}" + p.Text;
                }
                else
                {
                    p.Text = "{\\an8}" + p.Text;
                }
            }
            else if (verticalAlignment < 9)
            {
                if (alignment == 1) // left
                {
                    p.Text = "{\\an4}" + p.Text;
                }
                else if (alignment == 0) // right
                {
                    p.Text = "{\\an6}" + p.Text;
                }
                else
                {
                    p.Text = "{\\an5}" + p.Text;
                }
            }
            else
            {
                if (alignment == 1) // left
                {
                    p.Text = "{\\an1}" + p.Text;
                }
                else if (alignment == 0) // right
                {
                    p.Text = "{\\an3}" + p.Text;
                }
            }
            p.Text = p.Text.RemoveControlCharactersButWhiteSpace();
            return p;
        }

        /// <summary>
        /// Fix italic tags, lines starting with ">" - whole line is italic, words between &lt;&gt; is italic
        /// </summary>
        private static string FixItalics(string input)
        {
            var pre = string.Empty;
            var text = input;
            if (text.StartsWith("{\\", StringComparison.Ordinal))
            {
                var endIdx = text.IndexOf('}', 2);
                if (endIdx > 2)
                {
                    pre = text.Substring(0, endIdx + 1);
                    text = text.Remove(0, endIdx + 1);
                }
            }
            int index = text.IndexOf('<');
            if (index < 0)
            {
                return input;
            }

            while (index >= 0 && index < text.Length - 1)
            {
                text = text.Insert(index + 1, "i>");
                int indexOfNewLine = text.IndexOf(Environment.NewLine, index + 3, StringComparison.Ordinal);
                int indexOfEnd = text.IndexOf('>', index + 3);
                if (indexOfNewLine < 0 && indexOfEnd < 0)
                {
                    index = -1;
                    text += "</i>";
                }
                else
                {
                    if (indexOfNewLine < 0 || (indexOfEnd > 0 && indexOfEnd < indexOfNewLine))
                    {
                        text = text.Insert(indexOfEnd, "</i");
                        index = text.IndexOf('<', indexOfEnd + 3);
                    }
                    else
                    {
                        text = text.Insert(indexOfNewLine, "</i>");
                        index = text.IndexOf('<', indexOfNewLine + 4);
                    }
                }
            }
            text = text.Replace("</i>", "</i> ");
            text = text.Replace("  ", " ");
            return pre + text.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
        }

        public static Encoding GetEncoding(int codePage)
        {
            switch (codePage)
            {
                case CodePageLatin:
                    return Encoding.GetEncoding("iso-8859-1");
                case CodePageGreek:
                    return Encoding.GetEncoding("iso-8859-7");
                case CodePageLatinCzech:
                    return Encoding.GetEncoding("iso-8859-2");
                case CodePageArabic:
                    return Encoding.GetEncoding("iso-8859-6");
                case CodePageHebrew:
                    return Encoding.GetEncoding("iso-8859-8");
                case CodePageLatinTurkish:
                    return Encoding.GetEncoding("iso-8859-9");
                case CodePageThai:
                    return Encoding.GetEncoding("windows-874");
                case CodePageCyrillic:
                    return Encoding.GetEncoding("iso-8859-5");
                default:
                    return Encoding.Default;
            }
        }

        public static int AutoDetectEncoding(string fileName)
        {
            var pac = new Pac();
            try
            {
                var dictionary = new Dictionary<int, string>
                {
                    { CodePageLatin, "en-da-no-sv-es-it-fr-pt-de-nl-pl-bg-sq-hr" },
                    { CodePageGreek, "el" },
                    { CodePageLatinCzech, "cz" },
                    { CodePageLatinTurkish, "tr" },
                    { CodePageArabic, "ar" },
                    { CodePageHebrew, "he" },
                    { CodePageThai, "th" },
                    { CodePageCyrillic, "ru-uk-mk" },
                    { CodePageChineseTraditional, "zh" },
                    { CodePageChineseSimplified, "zh" },
                    { CodePageKorean, "ko" },
                    { CodePageJapanese, "ja" }
                };
                foreach (var kvp in dictionary)
                {
                    var sub = new Subtitle();
                    pac.CodePage = kvp.Key;
                    pac.LoadSubtitle(sub, null, fileName);
                    var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(sub);
                    if (languageCode != null && kvp.Value.Contains(languageCode))
                    {
                        return kvp.Key;
                    }
                }
            }
            catch
            {
                // ignored
            }
            return CodePageLatin;
        }

        private void GetCodePage(byte[] buffer, int index, int endDelimiter)
        {
            if (BatchMode)
            {
                if (CodePage == -1)
                {
                    CodePage = AutoDetectEncoding(_fileName);
                }

                return;
            }

            byte[] previewBuffer = null;
            if (buffer != null)
            {
                var textSample = new byte[200];
                int textIndex = 0;
                while (index < buffer.Length && buffer[index] != endDelimiter)
                {
                    if (buffer[index] == 0xFF)
                    {
                        if (textIndex < textSample.Length - 1)
                        {
                            textSample[textIndex++] = 32; // ASCII 32 SP (Space)
                        }

                        index++;
                    }
                    else if (buffer[index] == 0xFE)
                    {
                        if (textIndex < textSample.Length - 3)
                        {
                            textSample[textIndex++] = buffer[index];
                            textSample[textIndex++] = buffer[index + 1];
                            textSample[textIndex++] = buffer[index + 2];
                        }
                        index += 3;
                    }
                    else
                    {
                        if (textIndex < textSample.Length - 1)
                        {
                            textSample[textIndex++] = buffer[index];
                        }

                        index++;
                    }
                }
                previewBuffer = new byte[textIndex];
                for (int i = 0; i < textIndex; i++)
                {
                    previewBuffer[i] = textSample[i];
                }
            }

            CodePage = GetPacEncodingImplementation?.GetPacEncoding(previewBuffer, _fileName) ?? 2;
        }

        private static byte[] GetLatinBytes(Encoding encoding, string text, byte alignment, Dictionary<int, SpecialCharacter> extraCharacters)
        {
            int i = 0;
            var buffer = new byte[text.Length * 2];
            int extra = 0;
            while (i < text.Length)
            {
                if (text.Substring(i).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                {
                    buffer[i + extra] = 0xfe;
                    i++;
                    buffer[i + extra] = alignment;
                    extra++;
                    buffer[i + extra] = 3;
                }
                else
                {
                    string letter = text.Substring(i, 1);
                    var code = Find(extraCharacters, letter) ?? Find(LatinCodes, letter);
                    if (code != null)
                    {
                        int byteValue = code.Value.Key;
                        if (byteValue < 256)
                        {
                            buffer[i + extra] = (byte)byteValue;
                        }
                        else
                        {
                            int high = byteValue / 256;
                            int low = byteValue % 256;

                            buffer[i + extra] = (byte)high;
                            extra++;
                            buffer[i + extra] = (byte)low;
                        }
                    }
                    else
                    {
                        var values = encoding.GetBytes(letter);
                        for (int k = 0; k < values.Length; k++)
                        {
                            byte v = values[k];
                            if (k > 0)
                            {
                                extra++;
                            }

                            buffer[i + extra] = v;
                        }
                    }
                }
                i++;
            }

            var result = new byte[i + extra];
            for (int j = 0; j < i + extra; j++)
            {
                result[j] = buffer[j];
            }

            return result;
        }

        private static byte[] GetArabicBytes(string text, byte alignment)
        {
            return GetBytesViaLists(text, ArabicCodes, alignment);
        }

        private static byte[] GetHebrewBytes(string text, byte alignment)
        {
            return GetBytesViaLists(text, HebrewCodes, alignment);
        }

        private static byte[] GetCyrillicBytes(string text, byte alignment)
        {
            return GetBytesViaLists(text, CyrillicCodes, alignment);
        }

        private static byte[] GetGreekBytes(string text, byte alignment)
        {
            return GetBytesViaLists(text, GreekCodes, alignment);
        }

        private static byte[] GetBytesViaLists(string text, Dictionary<int, SpecialCharacter> codes, byte alignment)
        {
            text = text.Replace("’", "'");
            int i = 0;
            var buffer = new byte[text.Length * 2];
            int extra = 0;
            while (i < text.Length)
            {
                if (text.Substring(i).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                {
                    buffer[i + extra] = 0xfe;
                    i++;
                    buffer[i + extra] = alignment;
                    extra++;
                    buffer[i + extra] = 3;
                }
                else
                {
                    bool doubleCharacter = false;
                    string letter = string.Empty;
                    KeyValuePair<int, SpecialCharacter>? character = null;
                    if (i + 1 < text.Length)
                    {
                        letter = text.Substring(i, 2);
                        character = Find(codes, letter);
                        if (character != null)
                        {
                            doubleCharacter = true;
                        }
                    }
                    if (character == null)
                    {
                        letter = text.Substring(i, 1);
                        character = Find(codes, letter);
                    }
                    if (character.HasValue)
                    {
                        int byteValue = character.Value.Key;
                        if (byteValue < 256)
                        {
                            buffer[i + extra] = (byte)byteValue;
                        }
                        else if (byteValue < 65536)
                        {
                            int high = byteValue / 256;
                            int low = byteValue % 256;
                            buffer[i + extra] = (byte)high;
                            if (doubleCharacter)
                            {
                                i++;
                                doubleCharacter = false;
                            }
                            else
                            {
                                extra++;
                            }
                            buffer[i + extra] = (byte)low;
                        }
                        else
                        {
                            int highest = byteValue / 65536;
                            int high = (byteValue / 256) % 256;
                            int low = byteValue % 256;

                            buffer[i + extra] = (byte)highest;
                            extra++;
                            buffer[i + extra] = (byte)high;
                            extra++;
                            buffer[i + extra] = (byte)low;
                        }
                    }
                    else
                    {
                        var values = Encoding.Default.GetBytes(letter);
                        for (int k = 0; k < values.Length; k++)
                        {
                            byte v = values[k];
                            if (k > 0)
                            {
                                extra++;
                            }

                            buffer[i + extra] = v;
                        }
                    }
                    if (doubleCharacter)
                    {
                        i++;
                    }
                }
                i++;
            }

            var result = new byte[i + extra];
            for (int j = 0; j < i + extra; j++)
            {
                result[j] = buffer[j];
            }

            return result;
        }

        private static byte[] GetW16Bytes(string text, byte alignment, int encoding)
        {
            var result = new List<byte>();
            bool firstLine = true;
            foreach (var line in text.SplitToLines())
            {
                if (!firstLine)
                {
                    result.Add(0xfe);
                    result.Add(alignment);
                    result.Add(3);
                }

                if (OnlyAnsi(line))
                {
                    foreach (var b in GetLatinBytes(GetEncoding(CodePageLatin), line, alignment, null))
                    {
                        result.Add(b);
                    }
                }
                else
                {
                    result.Add(0x1f); // ?
                    result.Add(0x57); // W
                    result.Add(0x31); // 1
                    result.Add(0x36); // 6
                    result.Add(0x2e); // ?

                    foreach (var b in Encoding.GetEncoding(encoding).GetBytes(line))
                    {
                        result.Add(b);
                    }
                }
                firstLine = false;
            }
            return result.ToArray();
        }

        private static bool OnlyAnsi(string line)
        {
            string latin = Utilities.AllLettersAndNumbers + " .!?/%:;=()#$'&\"";
            foreach (char ch in line)
            {
                if (!latin.Contains(ch))
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetArabicString(byte[] buffer, ref int index)
        {
            var arabicCharacter = GetNextArabicCharacter(buffer, ref index);

            if (arabicCharacter.HasValue && arabicCharacter.Value.SwitchOrder)
            {
                // if we have a special character we must fetch the next one and move it before the current special one
                var tempIndex = index + 1;
                var nextArabicCharacter = GetNextArabicCharacter(buffer, ref tempIndex);
                if (buffer[tempIndex] >= 0x80 && nextArabicCharacter != null)
                {
                    index = tempIndex;
                    var combined = $"{nextArabicCharacter.Value.Character}{arabicCharacter.Value.Character}";
                    return combined;
                }
            }

            if (ThrowOnError && !arabicCharacter.HasValue)
            {
                throw new InvalidOperationException($"Unknown byte ({buffer[index]}) in subtitle file @ binary offset {index}.");
            }

            return arabicCharacter.HasValue
                ? arabicCharacter.Value.Character
                : string.Empty;
        }

        private static SpecialCharacter? GetNextArabicCharacter(byte[] buffer, ref int index)
        {
            if (index >= buffer.Length)
            {
                return null;
            }

            byte b = buffer[index];
            SpecialCharacter? arabicCharacter = null;
            if (ArabicCodes.ContainsKey(b))
            {
                arabicCharacter = ArabicCodes[b];
            }

            if (arabicCharacter != null && buffer.Length > index + 1)
            {
                var code = b * 256 + buffer[index + 1];
                if (ArabicCodes.ContainsKey(code))
                {
                    index++;
                    arabicCharacter = ArabicCodes[code];
                }
            }

            if (arabicCharacter == null && b >= 0x20 && b < 0x70)
            {
                return new SpecialCharacter(Encoding.ASCII.GetString(buffer, index, 1));
            }

            return arabicCharacter;
        }

        public static string GetHebrewString(byte[] buffer, ref int index)
        {
            byte b = buffer[index];
            if (b >= 0x20 && b < 0x70 && b != 44)
            {
                return Encoding.ASCII.GetString(buffer, index, 1);
            }

            if (HebrewCodes.ContainsKey(b))
            {
                return HebrewCodes[b].Character;
            }

            if (ThrowOnError)
            {
                throw new InvalidOperationException($"Unknown byte ({b}) in subtitle file @ binary offset {index}.");
            }

            return string.Empty;
        }

        private static bool TryGetMappedCharacter(Dictionary<int, SpecialCharacter> map, byte[] buffer, ref int index, out string result)
        {
            byte b = buffer[index];

            result = string.Empty;
            if (map.ContainsKey(b))
            {
                result = map[b].Character;
            }

            if (buffer.Length > index + 2)
            {
                var code = b * 256 + buffer[index + 1];
                var next = buffer[index + 1] * 256 + buffer[index + 2];

                if (map.ContainsKey(code) && (!map.ContainsKey(next) || map[code].Priority > map[next].Priority))
                {
                    index++;
                    result = map[code].Character;
                }
            }

            return !string.IsNullOrEmpty(result);
        }

        public static string GetLatinString(Encoding encoding, byte[] buffer, ref int index)
        {
            return GetLatinString(encoding, buffer, ref index, null);
        }

        private static string GetLatinString(Encoding encoding, byte[] buffer, ref int index, Dictionary<int, SpecialCharacter> overrides)
        {
            if (overrides != null && TryGetMappedCharacter(overrides, buffer, ref index, out var result))
            {
                return result;
            }

            if (TryGetMappedCharacter(LatinCodes, buffer, ref index, out result))
            {
                return result;
            }

            if (buffer[index] > 13)
            {
                return encoding.GetString(buffer, index, 1);
            }

            if (ThrowOnError)
            {
                throw new InvalidOperationException($"Unknown byte ({buffer[index]}) in subtitle file @ binary offset {index}.");
            }

            return string.Empty;
        }

        public static string GetCyrillicString(byte[] buffer, ref int index)
        {
            byte b = buffer[index];

            if (b >= 0x30 && b <= 0x39) // decimal digits
            {
                return Encoding.ASCII.GetString(buffer, index, 1);
            }

            if (buffer.Length > index + 1)
            {
                var code = b * 256 + buffer[index + 1];
                if (CyrillicCodes.ContainsKey(code))
                {
                    index++;
                    return CyrillicCodes[code].Character;
                }
            }

            if (CyrillicCodes.ContainsKey(b))
            {
                return CyrillicCodes[b].Character;
            }

            if (ThrowOnError)
            {
                throw new InvalidOperationException($"Unknown byte (0x{b:X2}) in subtitle file @ binary offset {index}.");
            }

            return string.Empty;
        }

        public static string GetGreekString(byte[] buffer, ref int index)
        {
            byte b = buffer[index];

            if (b >= 0x30 && b <= 0x39) // decimal digits
            {
                return Encoding.ASCII.GetString(buffer, index, 1);
            }

            if (GreekCodes.ContainsKey(b))
            {
                return GreekCodes[b].Character;
            }

            if (buffer.Length > index + 2)
            {
                int code = b * 65536 + buffer[index + 1] * 256 + buffer[index + 2];
                if (GreekCodes.ContainsKey(code))
                {
                    index += 2;
                    return GreekCodes[code].Character;
                }

                code = b * 256 + buffer[index + 1];
                if (GreekCodes.ContainsKey(code))
                {
                    index++;
                    return GreekCodes[code].Character;
                }
            }

            if (ThrowOnError)
            {
                throw new InvalidOperationException($"Unknown byte ({b}) in subtitle file @ binary offset {index}.");
            }
            return string.Empty;
        }

        public static string GetKoreanString(byte[] buffer, ref int index)
        {
            byte b = buffer[index];

            if (b >= 0x30 && b <= 0x39) // decimal digits
            {
                return Encoding.ASCII.GetString(buffer, index, 1);
            }

            if (KoreanCodes.ContainsKey(b))
            {
                return KoreanCodes[b].Character;
            }

            if (buffer.Length > index + 1)
            {
                var code = b * 256 + buffer[index + 1];
                if (KoreanCodes.ContainsKey(code))
                {
                    index++;
                    return KoreanCodes[code].Character;
                }
            }

            if (ThrowOnError)
            {
                throw new InvalidOperationException($"Unknown byte ({b}) in subtitle file @ binary offset {index}.");
            }

            return string.Empty;
        }

        internal static TimeCode GetTimeCode(int timeCodeIndex, byte[] buffer)
        {
            if (timeCodeIndex > 0)
            {
                string highPart = $"{buffer[timeCodeIndex] + buffer[timeCodeIndex + 1] * 256:000000}";
                string lowPart = $"{buffer[timeCodeIndex + 2] + buffer[timeCodeIndex + 3] * 256:000000}";

                int hours = int.Parse(highPart.Substring(0, 4));
                int minutes = int.Parse(highPart.Substring(4, 2));
                int seconds = int.Parse(lowPart.Substring(2, 2));
                int frames = int.Parse(lowPart.Substring(4, 2));

                return new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));
            }
            return new TimeCode();
        }

        private static KeyValuePair<int, SpecialCharacter>? Find(Dictionary<int, SpecialCharacter> list, string letter)
        {
            return list?.Where(c => c.Value.Character == letter).Cast<KeyValuePair<int, SpecialCharacter>?>().FirstOrDefault();
        }

        internal struct SpecialCharacter
        {
            internal SpecialCharacter(string character, bool switchOrder = false, int priority = 2)
            {
                Character = character;
                SwitchOrder = switchOrder;
                Priority = priority;
            }

            internal string Character { get; set; }
            internal bool SwitchOrder { get; set; }
            internal int Priority { get; set; }
        }
    }
}
