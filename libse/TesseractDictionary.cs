using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Core
{
    public class TesseractDictionary
    {

        private const string DownloadUrlTemplate = "https://raw.githubusercontent.com/tesseract-ocr/tessdata/master/{0}.traineddata";

        /// <summary>
        /// Dictionaries containing both 3.5 + 4.0 data - see https://github.com/tesseract-ocr/tessdata
        /// </summary>
        private static readonly string[] Dictionaries =
        {
            "afr",
            "amh",
            "ara",
            "asm",
            "aze",
            "aze_cyrl",
            "bel",
            "ben",
            "bod",
            "bos",
            "bre",
            "bul",
            "cat",
            "ceb",
            "ces",
            "chi_sim",
            "chi_sim_vert",
            "chi_tra",
            "chi_tra_vert",
            "chr",
            "cos",
            "cym",
            "dan",
            "deu",
            "div",
            "dzo",
            "ell",
            "eng",
            "enm",
            "epo",
            "equ",
            "est",
            "eus",
            "fao",
            "fas",
            "fil",
            "fin",
            "fra",
            "frk",
            "frm",
            "fry",
            "gla",
            "gle",
            "glg",
            "grc",
            "guj",
            "hat",
            "heb",
            "hin",
            "hrv",
            "hun",
            "hye",
            "iku",
            "ind",
            "isl",
            "ita",
            "jav",
            "jpn",
            "jpn_vert",
            "kan",
            "kat",
            "kaz",
            "khm",
            "kir",
            "kor",
            "kor_vert",
            "kur",
            "kur_ara",
            "lao",
            "lat",
            "lav",
            "lit",
            "ltz",
            "mal",
            "mar",
            "mkd",
            "mlt",
            "mon",
            "mri",
            "msa",
            "mya",
            "nep",
            "nld",
            "nor",
            "oci",
            "ori",
            "osd",
            "pan",
            "pol",
            "por",
            "pus",
            "que",
            "ron",
            "rus",
            "san",
            "sin",
            "slk",
            "slv",
            "snd",
            "spa",
            "sqi",
            "srp",
            "srp_latn",
            "sun",
            "swa",
            "swe",
            "syr",
            "tam",
            "tat",
            "tel",
            "tgk",
            "tgl",
            "tha",
            "tir",
            "ton",
            "tur",
            "uig",
            "ukr",
            "urd",
            "uzb",
            "uzb_cyrl",
            "vie",
            "yid",
            "yor"
        };

        public string Code { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public static List<TesseractDictionary> List()
        {
            var list = new List<TesseractDictionary>();
            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            foreach (var dictionary in Dictionaries)
            {
                list.Add(new TesseractDictionary
                {
                    Name = MakeName(dictionary, cultures),
                    Code = dictionary,
                    Url = string.Format(DownloadUrlTemplate, dictionary)
                });
            }
            return list;
        }

        private static string MakeName(string dictionary, CultureInfo[] cultures)
        {
            string code = dictionary;
            string post = string.Empty;
            var idx = code.IndexOf('_');
            if (idx > 0)
            {
                post = $" ({code.Substring(idx).Trim('_')})";
                code = code.Substring(0, idx).Trim('_');
            }

            try
            {
                var cultureInfo = cultures.FirstOrDefault(ci => string.Equals(ci.ThreeLetterISOLanguageName, code, StringComparison.OrdinalIgnoreCase));
                if (cultureInfo != null)
                {
                    code = cultureInfo.EnglishName;
                }
            }
            catch
            {
                // ignore
            }

            return code + post;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
