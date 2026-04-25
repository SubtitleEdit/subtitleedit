using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

public class CrispAsrOmni : CrispAsrEngineBase
{
    public static string StaticName => "Crisp ASR Omni";
    public override string Name => StaticName;
    public override string Choice => WhisperChoice.CrispAsrOmni;
    public override string BackendName => "omniasr";
    public override string DefaultLanguage => "eng_Latn";
    public override bool IncludeLanguage => true;
    public override string Url => "https://github.com/CrispStrobe/CrispASR";

    public override List<WhisperLanguage> Languages =>
       new()
       {
            // Germanic
            new WhisperLanguage("eng_Latn", "english"),
            new WhisperLanguage("deu_Latn", "german"),
            new WhisperLanguage("nld_Latn", "dutch"),
            new WhisperLanguage("afr_Latn", "afrikaans"),
            new WhisperLanguage("swe_Latn", "swedish"),
            new WhisperLanguage("dan_Latn", "danish"),
            new WhisperLanguage("nob_Latn", "norwegian bokmål"),
            new WhisperLanguage("nno_Latn", "norwegian nynorsk"),
            new WhisperLanguage("isl_Latn", "icelandic"),
            new WhisperLanguage("fao_Latn", "faroese"),
            new WhisperLanguage("ltz_Latn", "luxembourgish"),
            new WhisperLanguage("fry_Latn", "frisian"),
            new WhisperLanguage("lim_Latn", "limburgish"),
            new WhisperLanguage("yid_Hebr", "yiddish"),

            // Romance
            new WhisperLanguage("fra_Latn", "french"),
            new WhisperLanguage("spa_Latn", "spanish"),
            new WhisperLanguage("por_Latn", "portuguese"),
            new WhisperLanguage("ita_Latn", "italian"),
            new WhisperLanguage("ron_Latn", "romanian"),
            new WhisperLanguage("cat_Latn", "catalan"),
            new WhisperLanguage("glg_Latn", "galician"),
            new WhisperLanguage("oci_Latn", "occitan"),
            new WhisperLanguage("ast_Latn", "asturian"),
            new WhisperLanguage("srd_Latn", "sardinian"),
            new WhisperLanguage("hat_Latn", "haitian creole"),
            new WhisperLanguage("pap_Latn", "papiamento"),

            // Slavic
            new WhisperLanguage("rus_Cyrl", "russian"),
            new WhisperLanguage("ukr_Cyrl", "ukrainian"),
            new WhisperLanguage("bel_Cyrl", "belarusian"),
            new WhisperLanguage("pol_Latn", "polish"),
            new WhisperLanguage("ces_Latn", "czech"),
            new WhisperLanguage("slk_Latn", "slovak"),
            new WhisperLanguage("slv_Latn", "slovenian"),
            new WhisperLanguage("hrv_Latn", "croatian"),
            new WhisperLanguage("srp_Cyrl", "serbian"),
            new WhisperLanguage("bos_Latn", "bosnian"),
            new WhisperLanguage("mkd_Cyrl", "macedonian"),
            new WhisperLanguage("bul_Cyrl", "bulgarian"),

            // Baltic / Finno-Ugric
            new WhisperLanguage("lit_Latn", "lithuanian"),
            new WhisperLanguage("lvs_Latn", "latvian"),
            new WhisperLanguage("est_Latn", "estonian"),
            new WhisperLanguage("fin_Latn", "finnish"),
            new WhisperLanguage("hun_Latn", "hungarian"),

            // Celtic
            new WhisperLanguage("gle_Latn", "irish"),
            new WhisperLanguage("cym_Latn", "welsh"),
            new WhisperLanguage("gla_Latn", "scottish gaelic"),

            // Greek / Albanian / Maltese / Basque
            new WhisperLanguage("ell_Grek", "greek"),
            new WhisperLanguage("als_Latn", "albanian"),
            new WhisperLanguage("mlt_Latn", "maltese"),
            new WhisperLanguage("eus_Latn", "basque"),

            // Turkic
            new WhisperLanguage("tur_Latn", "turkish"),
            new WhisperLanguage("azj_Latn", "azerbaijani"),
            new WhisperLanguage("kaz_Cyrl", "kazakh"),
            new WhisperLanguage("uzn_Latn", "uzbek"),
            new WhisperLanguage("kir_Cyrl", "kyrgyz"),
            new WhisperLanguage("tuk_Latn", "turkmen"),
            new WhisperLanguage("tat_Cyrl", "tatar"),
            new WhisperLanguage("uig_Arab", "uyghur"),

            // Semitic / Iranian
            new WhisperLanguage("arb_Arab", "arabic"),
            new WhisperLanguage("arz_Arab", "arabic (egyptian)"),
            new WhisperLanguage("ary_Arab", "arabic (moroccan)"),
            new WhisperLanguage("apc_Arab", "arabic (levantine)"),
            new WhisperLanguage("heb_Hebr", "hebrew"),
            new WhisperLanguage("amh_Ethi", "amharic"),
            new WhisperLanguage("tir_Ethi", "tigrinya"),
            new WhisperLanguage("pes_Arab", "persian"),
            new WhisperLanguage("pbt_Arab", "pashto"),
            new WhisperLanguage("ckb_Arab", "central kurdish"),
            new WhisperLanguage("kmr_Latn", "northern kurdish"),
            new WhisperLanguage("tgk_Cyrl", "tajik"),

            // Caucasian
            new WhisperLanguage("kat_Geor", "georgian"),
            new WhisperLanguage("hye_Armn", "armenian"),

            // Indic / Dravidian
            new WhisperLanguage("hin_Deva", "hindi"),
            new WhisperLanguage("urd_Arab", "urdu"),
            new WhisperLanguage("ben_Beng", "bengali"),
            new WhisperLanguage("guj_Gujr", "gujarati"),
            new WhisperLanguage("pan_Guru", "punjabi"),
            new WhisperLanguage("mar_Deva", "marathi"),
            new WhisperLanguage("npi_Deva", "nepali"),
            new WhisperLanguage("ory_Orya", "odia"),
            new WhisperLanguage("asm_Beng", "assamese"),
            new WhisperLanguage("snd_Arab", "sindhi"),
            new WhisperLanguage("sin_Sinh", "sinhala"),
            new WhisperLanguage("mai_Deva", "maithili"),
            new WhisperLanguage("bho_Deva", "bhojpuri"),
            new WhisperLanguage("awa_Deva", "awadhi"),
            new WhisperLanguage("san_Deva", "sanskrit"),
            new WhisperLanguage("kas_Arab", "kashmiri"),
            new WhisperLanguage("tam_Taml", "tamil"),
            new WhisperLanguage("tel_Telu", "telugu"),
            new WhisperLanguage("kan_Knda", "kannada"),
            new WhisperLanguage("mal_Mlym", "malayalam"),

            // South-East / East Asian
            new WhisperLanguage("tha_Thai", "thai"),
            new WhisperLanguage("lao_Laoo", "lao"),
            new WhisperLanguage("khm_Khmr", "khmer"),
            new WhisperLanguage("mya_Mymr", "burmese"),
            new WhisperLanguage("vie_Latn", "vietnamese"),
            new WhisperLanguage("ind_Latn", "indonesian"),
            new WhisperLanguage("zsm_Latn", "malay"),
            new WhisperLanguage("jav_Latn", "javanese"),
            new WhisperLanguage("sun_Latn", "sundanese"),
            new WhisperLanguage("tgl_Latn", "tagalog"),
            new WhisperLanguage("ceb_Latn", "cebuano"),
            new WhisperLanguage("ilo_Latn", "ilocano"),
            new WhisperLanguage("war_Latn", "waray"),
            new WhisperLanguage("hil_Latn", "hiligaynon"),
            new WhisperLanguage("min_Latn", "minangkabau"),
            new WhisperLanguage("ban_Latn", "balinese"),
            new WhisperLanguage("cmn_Hans", "chinese (simplified)"),
            new WhisperLanguage("cmn_Hant", "chinese (traditional)"),
            new WhisperLanguage("yue_Hant", "cantonese"),
            new WhisperLanguage("jpn_Jpan", "japanese"),
            new WhisperLanguage("kor_Hang", "korean"),
            new WhisperLanguage("bod_Tibt", "tibetan"),
            new WhisperLanguage("mon_Cyrl", "mongolian"),

            // Pacific / Americas
            new WhisperLanguage("mri_Latn", "maori"),
            new WhisperLanguage("smo_Latn", "samoan"),
            new WhisperLanguage("fij_Latn", "fijian"),
            new WhisperLanguage("tpi_Latn", "tok pisin"),
            new WhisperLanguage("mlg_Latn", "malagasy"),
            new WhisperLanguage("quy_Latn", "quechua"),
            new WhisperLanguage("grn_Latn", "guarani"),
            new WhisperLanguage("ayr_Latn", "aymara"),

            // African
            new WhisperLanguage("swh_Latn", "swahili"),
            new WhisperLanguage("hau_Latn", "hausa"),
            new WhisperLanguage("yor_Latn", "yoruba"),
            new WhisperLanguage("ibo_Latn", "igbo"),
            new WhisperLanguage("som_Latn", "somali"),
            new WhisperLanguage("gaz_Latn", "oromo"),
            new WhisperLanguage("zul_Latn", "zulu"),
            new WhisperLanguage("xho_Latn", "xhosa"),
            new WhisperLanguage("sna_Latn", "shona"),
            new WhisperLanguage("nya_Latn", "chichewa"),
            new WhisperLanguage("lug_Latn", "ganda"),
            new WhisperLanguage("kik_Latn", "kikuyu"),
            new WhisperLanguage("kin_Latn", "kinyarwanda"),
            new WhisperLanguage("run_Latn", "rundi"),
            new WhisperLanguage("sot_Latn", "southern sotho"),
            new WhisperLanguage("nso_Latn", "northern sotho"),
            new WhisperLanguage("tsn_Latn", "tswana"),
            new WhisperLanguage("tso_Latn", "tsonga"),
            new WhisperLanguage("ssw_Latn", "swati"),
            new WhisperLanguage("lin_Latn", "lingala"),
            new WhisperLanguage("fon_Latn", "fon"),
            new WhisperLanguage("ewe_Latn", "ewe"),
            new WhisperLanguage("twi_Latn", "twi"),
            new WhisperLanguage("wol_Latn", "wolof"),
            new WhisperLanguage("bam_Latn", "bambara"),
            new WhisperLanguage("fuv_Latn", "fulah"),
       };

    public override List<WhisperModel> Models =>
       new()
       {
            new WhisperModel
            {
                Name = "omniasr-llm-300m-v2-q4_k.gguf",
                Size = "1.08 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/omniasr-llm-300m-v2-GGUF/resolve/main/omniasr-llm-300m-v2-q4_k.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "omniasr-llm-300m-v2-q8_0.gguf",
                Size = "1.84 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/omniasr-llm-300m-v2-GGUF/resolve/main/omniasr-llm-300m-v2-q8_0.gguf"
                ],
            },
            new WhisperModel
            {
                Name = "omniasr-llm-300m-v2-f16.gguf",
                Size = "3.26 GB",
                Urls =
                [
                    "https://huggingface.co/cstr/omniasr-llm-300m-v2-GGUF/resolve/main/omniasr-llm-300m-v2-f16.gguf"
                ],
            },
       };

    public override string Extension => string.Empty;
    public override string UnpackSkipFolder => string.Empty;

    public override bool IsEngineInstalled()
    {
        var executableFile = GetExecutable();
        return File.Exists(executableFile);
    }

    public override string ToString()
    {
        return Name;
    }

    public override string GetAndCreateWhisperFolder()
    {
        var baseFolder = Se.SpeechToTextFolder;
        if (!Directory.Exists(baseFolder))
        {
            Directory.CreateDirectory(baseFolder);
        }

        var folder = Path.Combine(baseFolder, "CrispASR");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public override string GetAndCreateWhisperModelFolder(WhisperModel? whisperModel)
    {
        var folder = GetAndCreateWhisperFolder();
        var modelsFolder = Path.Combine(folder, "models");
        if (!Directory.Exists(modelsFolder))
        {
            Directory.CreateDirectory(modelsFolder);
        }

        return modelsFolder;
    }

    public override string GetExecutable()
    {
        string fullPath = Path.Combine(GetAndCreateWhisperFolder(), GetExecutableFileName());
        return fullPath;
    }

    public override bool IsModelInstalled(WhisperModel model)
    {
        var modelFile = GetModelForCmdLine(model.Name);
        if (!File.Exists(modelFile))
        {
            return false;
        }

        return new FileInfo(modelFile).Length > 10_000_000;
    }

    public override string GetModelForCmdLine(string modelName)
    {
        var modelFileName = Path.Combine(GetAndCreateWhisperModelFolder(null), modelName);
        return modelFileName;
    }


    public override string GetWhisperModelDownloadFileName(WhisperModel whisperModel, string url)
    {
        var folder = GetAndCreateWhisperModelFolder(whisperModel);
        var fileNameOnly = Path.GetFileName(url);
        var fileName = Path.Combine(folder, fileNameOnly);
        return fileName;
    }

    internal static string GetExecutableFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "crispasr.exe";
        }

        return "crispasr";
    }

    public override bool CanBeDownloaded()
    {
        return true;
    }

    public override string CommandLineParameter
    {
        get => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrOmni;
        set => Se.Settings.Tools.AudioToText.CommandLineParameterCrispAsrOmni = value;
    }
}
