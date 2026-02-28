using Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

public class GoogleLensOcrSharp
{
    private IProgress<PaddleOcrBatchProgress>? _batchProgress;
    private ILens _lens;

    public GoogleLensOcrSharp(ILens lens)
    {
        _lens = lens;
    }

    private Lock _lockObject = new Lock();

    public async Task OcrBatch(List<PaddleOcrBatchInput> input, string language, IProgress<PaddleOcrBatchProgress> progress, CancellationToken cancellationToken)
    {
        _batchProgress = progress;

        foreach (var bmpInput in input)
        {
            if (bmpInput.Bitmap == null)
            {
                continue;
            }

            var result = await _lens.ScanByBitmap(bmpInput.Bitmap, language);
            bmpInput.Text = string.Join(Environment.NewLine, result.Segments.Where(s => !string.IsNullOrWhiteSpace(s.Text)).Select(p => p.Text).ToList()).Trim();
            lock (_lockObject)
            {
                var progressReport = new PaddleOcrBatchProgress
                {
                    Index = bmpInput.Index,
                    Text = bmpInput.Text,
                    Item = bmpInput.Item,
                };
                _batchProgress?.Report(progressReport);
            }
        }
    }

    public static List<OcrLanguage2> GetLanguages()
    {
        return new List<OcrLanguage2>
            {
                new OcrLanguage2("ab", "Abkhaz"),
                new OcrLanguage2("ace", "Acehnese"),
                new OcrLanguage2("ach", "Acholi"),
                new OcrLanguage2("af", "Afrikaans"),
                new OcrLanguage2("ak", "Twi (Akan)"),
                new OcrLanguage2("sq", "Albanian"),
                new OcrLanguage2("alz", "Alur"),
                new OcrLanguage2("am", "Amharic"),
                new OcrLanguage2("ar", "Arabic"),
                new OcrLanguage2("hy", "Armenian"),
                new OcrLanguage2("as", "Assamese"),
                new OcrLanguage2("awa", "Awadhi"),
                new OcrLanguage2("ay", "Aymara"),
                new OcrLanguage2("az", "Azerbaijani"),
                new OcrLanguage2("ban", "Balinese"),
                new OcrLanguage2("bm", "Bambara"),
                new OcrLanguage2("ba", "Bashkir"),
                new OcrLanguage2("eu", "Basque"),
                new OcrLanguage2("btx", "Batak Karo"),
                new OcrLanguage2("bts", "Batak Simalungun"),
                new OcrLanguage2("bbc", "Batak Toba"),
                new OcrLanguage2("be", "Belarusian"),
                new OcrLanguage2("bem", "Bemba"),
                new OcrLanguage2("bn", "Bengali"),
                new OcrLanguage2("bew", "Betawi"),
                new OcrLanguage2("bho", "Bhojpuri"),
                new OcrLanguage2("bik", "Bikol"),
                new OcrLanguage2("bs", "Bosnian"),
                new OcrLanguage2("br", "Breton"),
                new OcrLanguage2("bg", "Bulgarian"),
                new OcrLanguage2("bua", "Buryat"),
                new OcrLanguage2("yue", "Cantonese"),
                new OcrLanguage2("ca", "Catalan"),
                new OcrLanguage2("ceb", "Cebuano"),
                new OcrLanguage2("ny", "Chichewa (Nyanja)"),
                new OcrLanguage2("zh-CN", "Chinese (Simplified)"),
                new OcrLanguage2("zh-TW", "Chinese (Traditional)"),
                new OcrLanguage2("cv", "Chuvash"),
                new OcrLanguage2("co", "Corsican"),
                new OcrLanguage2("crh", "Crimean Tatar"),
                new OcrLanguage2("hr", "Croatian"),
                new OcrLanguage2("cs", "Czech"),
                new OcrLanguage2("da", "Danish"),
                new OcrLanguage2("din", "Dinka"),
                new OcrLanguage2("dv", "Divehi"),
                new OcrLanguage2("doi", "Dogri"),
                new OcrLanguage2("dov", "Dombe"),
                new OcrLanguage2("nl", "Dutch"),
                new OcrLanguage2("dz", "Dzongkha"),
                new OcrLanguage2("en", "English"),
                new OcrLanguage2("eo", "Esperanto"),
                new OcrLanguage2("et", "Estonian"),
                new OcrLanguage2("ee", "Ewe"),
                new OcrLanguage2("fj", "Fijian"),
                new OcrLanguage2("fil", "Filipino (Tagalog)"),
                new OcrLanguage2("fi", "Finnish"),
                new OcrLanguage2("fr", "French"),
                new OcrLanguage2("fr-CA", "French (Canadian)"),
                new OcrLanguage2("fr-FR", "French (French)"),
                new OcrLanguage2("fy", "Frisian"),
                new OcrLanguage2("ff", "Fulfulde"),
                new OcrLanguage2("gaa", "Ga"),
                new OcrLanguage2("gl", "Galician"),
                new OcrLanguage2("lg", "Ganda (Luganda)"),
                new OcrLanguage2("ka", "Georgian"),
                new OcrLanguage2("de", "German"),
                new OcrLanguage2("el", "Greek"),
                new OcrLanguage2("gn", "Guarani"),
                new OcrLanguage2("gu", "Gujarati"),
                new OcrLanguage2("ht", "Haitian Creole"),
                new OcrLanguage2("cnh", "Hakha Chin"),
                new OcrLanguage2("ha", "Hausa"),
                new OcrLanguage2("haw", "Hawaiian"),
                new OcrLanguage2("iw", "Hebrew"),
                new OcrLanguage2("hil", "Hiligaynon"),
                new OcrLanguage2("hi", "Hindi"),
                new OcrLanguage2("hmn", "Hmong"),
                new OcrLanguage2("hu", "Hungarian"),
                new OcrLanguage2("hrx", "Hunsrik"),
                new OcrLanguage2("is", "Icelandic"),
                new OcrLanguage2("ig", "Igbo"),
                new OcrLanguage2("ilo", "Iloko"),
                new OcrLanguage2("id", "Indonesian"),
                new OcrLanguage2("ga", "Irish"),
                new OcrLanguage2("it", "Italian"),
                new OcrLanguage2("ja", "Japanese"),
                new OcrLanguage2("jw", "Javanese"),
                new OcrLanguage2("kn", "Kannada"),
                new OcrLanguage2("pam", "Kapampangan"),
                new OcrLanguage2("kk", "Kazakh"),
                new OcrLanguage2("km", "Khmer"),
                new OcrLanguage2("cgg", "Kiga"),
                new OcrLanguage2("rw", "Kinyarwanda"),
                new OcrLanguage2("ktu", "Kituba"),
                new OcrLanguage2("gom", "Konkani"),
                new OcrLanguage2("ko", "Korean"),
                new OcrLanguage2("kri", "Krio"),
                new OcrLanguage2("ku", "Kurdish (Kurmanji)"),
                new OcrLanguage2("ckb", "Kurdish (Sorani)"),
                new OcrLanguage2("ky", "Kyrgyz"),
                new OcrLanguage2("lo", "Lao"),
                new OcrLanguage2("ltg", "Latgalian"),
                new OcrLanguage2("la", "Latin"),
                new OcrLanguage2("lv", "Latvian"),
                new OcrLanguage2("lij", "Ligurian"),
                new OcrLanguage2("li", "Limburgan"),
                new OcrLanguage2("ln", "Lingala"),
                new OcrLanguage2("lt", "Lithuanian"),
                new OcrLanguage2("lmo", "Lombard"),
                new OcrLanguage2("luo", "Luo"),
                new OcrLanguage2("lb", "Luxembourgish"),
                new OcrLanguage2("mk", "Macedonian"),
                new OcrLanguage2("mai", "Maithili"),
                new OcrLanguage2("mak", "Makassar"),
                new OcrLanguage2("mg", "Malagasy"),
                new OcrLanguage2("ms", "Malay"),
                new OcrLanguage2("ms-Arab", "Malay (Jawi)"),
                new OcrLanguage2("ml", "Malayalam"),
                new OcrLanguage2("mt", "Maltese"),
                new OcrLanguage2("mi", "Maori"),
                new OcrLanguage2("mr", "Marathi"),
                new OcrLanguage2("chm", "Meadow Mari"),
                new OcrLanguage2("mni-Mtei", "Meiteilon (Manipuri)"),
                new OcrLanguage2("min", "Minang"),
                new OcrLanguage2("lus", "Mizo"),
                new OcrLanguage2("mn", "Mongolian"),
                new OcrLanguage2("my", "Myanmar (Burmese)"),
                new OcrLanguage2("nr", "Ndebele (South)"),
                new OcrLanguage2("new", "Nepalbhasa (Newari)"),
                new OcrLanguage2("ne", "Nepali"),
                new OcrLanguage2("nso", "Northern Sotho (Sepedi)"),
                new OcrLanguage2("no", "Norwegian"),
                new OcrLanguage2("nus", "Nuer"),
                new OcrLanguage2("oc", "Occitan"),
                new OcrLanguage2("or", "Odia (Oriya)"),
                new OcrLanguage2("om", "Oromo"),
                new OcrLanguage2("pag", "Pangasinan"),
                new OcrLanguage2("pap", "Papiamento"),
                new OcrLanguage2("ps", "Pashto"),
                new OcrLanguage2("fa", "Persian"),
                new OcrLanguage2("pl", "Polish"),
                new OcrLanguage2("pt", "Portuguese"),
                new OcrLanguage2("pt-BR", "Portuguese (Brazil)"),
                new OcrLanguage2("pt-PT", "Portuguese (Portugal)"),
                new OcrLanguage2("pa", "Punjabi"),
                new OcrLanguage2("pa-Arab", "Punjabi (Shahmukhi)"),
                new OcrLanguage2("qu", "Quechua"),
                new OcrLanguage2("rom", "Romani"),
                new OcrLanguage2("ro", "Romanian"),
                new OcrLanguage2("rn", "Rundi"),
                new OcrLanguage2("ru", "Russian"),
                new OcrLanguage2("sm", "Samoan"),
                new OcrLanguage2("sg", "Sango"),
                new OcrLanguage2("sa", "Sanskrit"),
                new OcrLanguage2("gd", "Scots Gaelic"),
                new OcrLanguage2("sr", "Serbian"),
                new OcrLanguage2("st", "Sesotho"),
                new OcrLanguage2("crs", "Seychellois Creole"),
                new OcrLanguage2("shn", "Shan"),
                new OcrLanguage2("sn", "Shona"),
                new OcrLanguage2("scn", "Sicilian"),
                new OcrLanguage2("szl", "Silesian"),
                new OcrLanguage2("sd", "Sindhi"),
                new OcrLanguage2("si", "Sinhala (Sinhalese)"),
                new OcrLanguage2("sk", "Slovak"),
                new OcrLanguage2("sl", "Slovenian"),
                new OcrLanguage2("so", "Somali"),
                new OcrLanguage2("es", "Spanish"),
                new OcrLanguage2("su", "Sundanese"),
                new OcrLanguage2("sw", "Swahili"),
                new OcrLanguage2("ss", "Swati"),
                new OcrLanguage2("sv", "Swedish"),
                new OcrLanguage2("tg", "Tajik"),
                new OcrLanguage2("ta", "Tamil"),
                new OcrLanguage2("tt", "Tatar"),
                new OcrLanguage2("te", "Telugu"),
                new OcrLanguage2("tet", "Tetum"),
                new OcrLanguage2("th", "Thai"),
                new OcrLanguage2("ti", "Tigrinya"),
                new OcrLanguage2("ts", "Tsonga"),
                new OcrLanguage2("tn", "Tswana"),
                new OcrLanguage2("tr", "Turkish"),
                new OcrLanguage2("tk", "Turkmen"),
                new OcrLanguage2("uk", "Ukrainian"),
                new OcrLanguage2("ur", "Urdu"),
                new OcrLanguage2("ug", "Uyghur"),
                new OcrLanguage2("uz", "Uzbek"),
                new OcrLanguage2("vi", "Vietnamese"),
                new OcrLanguage2("cy", "Welsh"),
                new OcrLanguage2("xh", "Xhosa"),
                new OcrLanguage2("yi", "Yiddish"),
                new OcrLanguage2("yo", "Yoruba"),
                new OcrLanguage2("yua", "Yucatec Maya"),
                new OcrLanguage2("zu", "Zulu"),
            }.OrderBy(p => p.Name).ToList();
    }
}