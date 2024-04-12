using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.TextToSpeech
{
    public class PiperModels
    {
        public string Voice { get; set; }
        public string Language { get; set; }
        public string Quality { get; set; }
        public string Model { get; set; }
        public string ModelShort => Model.Split('/').Last();

        public string Config { get; set; }
        public string ConfigShort => Config.Split('/').Last();

        public override string ToString()
        {
            return $"{Language} - {Voice} ({Quality})";
        }

        public PiperModels(string voice, string language, string quality, string model, string config)
        {
            Voice = voice;
            Language = language;
            Quality = quality;
            Model = model;
            Config = config;
        }

        public static List<PiperModels> GetVoices()
        {
            var models = new List<PiperModels>
            {
                new PiperModels("kareem", "Arabic", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ar/ar_JO/kareem/medium/ar_JO-kareem-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ar/ar_JO/kareem/medium/ar_JO-kareem-medium.onnx.json"),
                new PiperModels("upc_ona", "Catalan", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ca/ca_ES/upc_ona/medium/ca_ES-upc_ona-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ca/ca_ES/upc_ona/medium/ca_ES-upc_ona-medium.onnx.json"),
                new PiperModels("jirka", "Czech", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/cs/cs_CZ/jirka/medium/cs_CZ-jirka-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/cs/cs_CZ/jirka/medium/cs_CZ-jirka-medium.onnx.json"),
                new PiperModels("talesyntese", "Danish", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/da/da_DK/talesyntese/medium/da_DK-talesyntese-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/da/da_DK/talesyntese/medium/da_DK-talesyntese-medium.onnx.json"),
                new PiperModels("eva_k", "German", "low", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/de/de_DE/eva_k/x_low/de_DE-eva_k-x_low.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/de/de_DE/eva_k/x_low/de_DE-eva_k-x_low.onnx.json"),
                new PiperModels("rapunzelina", "Greek", "low", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/el/el_GR/rapunzelina/low/el_GR-rapunzelina-low.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/el/el_GR/rapunzelina/low/el_GR-rapunzelina-low.onnx.json"),
                new PiperModels("alan", "English GB", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/alan/medium/en_GB-alan-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/alan/medium/en_GB-alan-medium.onnx.json"),
                new PiperModels("alba", "English GB", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/alba/medium/en_GB-alba-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/alba/medium/en_GB-alba-medium.onnx.json"),
                new PiperModels("cori", "English GB", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/cori/high/en_GB-cori-high.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/cori/medium/en_GB-cori-medium.onnx.json"),
                new PiperModels("jenny_dioco", "English GB", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/jenny_dioco/medium/en_GB-jenny_dioco-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/jenny_dioco/medium/en_GB-jenny_dioco-medium.onnx.json"),
                new PiperModels("northern_english_male", "English GB", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/northern_english_male/medium/en_GB-northern_english_male-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/northern_english_male/medium/en_GB-northern_english_male-medium.onnx.json"),
                new PiperModels("semaine", "English GB", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/semaine/medium/en_GB-semaine-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_GB/semaine/medium/en_GB-semaine-medium.onnx.json"),
                new PiperModels("amy", "English US", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/amy/medium/en_US-amy-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/amy/medium/en_US-amy-medium.onnx.json"),
                new PiperModels("arctic", "English US", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/arctic/medium/en_US-arctic-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/arctic/medium/en_US-arctic-medium.onnx.json"),
                new PiperModels("hfc_female", "English US", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/hfc_female/medium/en_US-hfc_female-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/hfc_female/medium/en_US-hfc_female-medium.onnx.json"),
                new PiperModels("hfc_male", "English US", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/hfc_male/medium/en_US-hfc_male-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/hfc_male/medium/en_US-hfc_male-medium.onnx.json"),
                new PiperModels("joe", "English US", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/joe/medium/en_US-joe-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/joe/medium/en_US-joe-medium.onnx.json"),
                new PiperModels("kristin", "English US", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/kristin/medium/en_US-kristin-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/kristin/medium/en_US-kristin-medium.onnx.json"),
                new PiperModels("kusal", "English US", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/kusal/medium/en_US-kusal-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/kusal/medium/en_US-kusal-medium.onnx.json"),
                new PiperModels("l2arctic", "English US", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/l2arctic/medium/en_US-l2arctic-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/l2arctic/medium/en_US-l2arctic-medium.onnx.json"),
                new PiperModels("lessac", "English US", "high", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/lessac/high/en_US-lessac-high.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/lessac/high/en_US-lessac-high.onnx.json"),
                new PiperModels("libritts", "English US", "high", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/libritts/high/en_US-libritts-high.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/libritts/high/en_US-libritts-high.onnx.json"),
                new PiperModels("ljspeech", "English US", "high", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/ljspeech/high/en_US-ljspeech-high.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/ljspeech/high/en_US-ljspeech-high.onnx.json"),
                new PiperModels("ryan", "English US", "high", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/ryan/high/en_US-ryan-high.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/en/en_US/ryan/high/en_US-ryan-high.onnx.json"),
                new PiperModels("davefx", "Spanish ES", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/es/es_ES/davefx/medium/es_ES-davefx-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/es/es_ES/davefx/medium/es_ES-davefx-medium.onnx.json"),
                new PiperModels("claude", "Spanish MX", "high", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/es/es_MX/claude/high/es_MX-claude-high.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/es/es_MX/claude/high/es_MX-claude-high.onnx.json"),
                new PiperModels("amir", "Farsi", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fa/fa_IR/amir/medium/fa_IR-amir-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fa/fa_IR/amir/medium/fa_IR-amir-medium.onnx.json"),
                new PiperModels("gyro", "Farsi", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fa/fa_IR/gyro/medium/fa_IR-gyro-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fa/fa_IR/gyro/medium/fa_IR-gyro-medium.onnx.json"),
                new PiperModels("harri", "Finnish", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fi/fi_FI/harri/medium/fi_FI-harri-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fi/fi_FI/harri/medium/fi_FI-harri-medium.onnx.json"),
                new PiperModels("mls", "French", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fr/fr_FR/mls/medium/fr_FR-mls-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fr/fr_FR/mls/medium/fr_FR-mls-medium.onnx.json"),
                new PiperModels("siwis", "French", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fr/fr_FR/siwis/medium/fr_FR-siwis-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fr/fr_FR/siwis/medium/fr_FR-siwis-medium.onnx.json"),
                new PiperModels("tom", "French", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fr/fr_FR/tom/medium/fr_FR-tom-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fr/fr_FR/tom/medium/fr_FR-tom-medium.onnx.json"),
                new PiperModels("upmc", "French", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fr/fr_FR/upmc/medium/fr_FR-upmc-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/fr/fr_FR/upmc/medium/fr_FR-upmc-medium.onnx.json?"),
                new PiperModels("berta", "Hungarian", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/hu/hu_HU/berta/medium/hu_HU-berta-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/hu/hu_HU/berta/medium/hu_HU-berta-medium.onnx.json"),
                new PiperModels("imre", "Hungarian", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/hu/hu_HU/imre/medium/hu_HU-imre-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/hu/hu_HU/imre/medium/hu_HU-imre-medium.onnx.json"),
                new PiperModels("bui", "Icelandic", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/is/is_IS/bui/medium/is_IS-bui-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/is/is_IS/bui/medium/is_IS-bui-medium.onnx.json"),
                new PiperModels("salka", "Icelandic", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/is/is_IS/salka/medium/is_IS-salka-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/is/is_IS/salka/medium/is_IS-salka-medium.onnx.json"),
                new PiperModels("steinn", "Icelandic", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/is/is_IS/steinn/medium/is_IS-steinn-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/is/is_IS/steinn/medium/is_IS-steinn-medium.onnx.json"),
                new PiperModels("ugla", "Icelandic", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/is/is_IS/ugla/medium/is_IS-ugla-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/is/is_IS/ugla/medium/is_IS-ugla-medium.onnx.json"),
                new PiperModels("riccardo", "Italian", "low", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/it/it_IT/riccardo/x_low/it_IT-riccardo-x_low.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/it/it_IT/riccardo/x_low/it_IT-riccardo-x_low.onnx.json"),
                new PiperModels("natia", "Georgian", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ka/ka_GE/natia/medium/ka_GE-natia-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ka/ka_GE/natia/medium/ka_GE-natia-medium.onnx.json"),
                new PiperModels("issai", "Kazakh", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/kk/kk_KZ/issai/high/kk_KZ-issai-high.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/kk/kk_KZ/issai/high/kk_KZ-issai-high.onnx.json"),
                new PiperModels("marylux", "Luxembourgish", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/lb/lb_LU/marylux/medium/lb_LU-marylux-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/lb/lb_LU/marylux/medium/lb_LU-marylux-medium.onnx.json"),
                new PiperModels("google", "Nepali", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ne/ne_NP/google/medium/ne_NP-google-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ne/ne_NP/google/medium/ne_NP-google-medium.onnx.json"),
                new PiperModels("nathalie", "Dutch BE", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/nl/nl_BE/nathalie/medium/nl_BE-nathalie-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/nl/nl_BE/nathalie/medium/nl_BE-nathalie-medium.onnx.json"),
                new PiperModels("rdh", "Dutch BE", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/nl/nl_BE/rdh/medium/nl_BE-rdh-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/nl/nl_BE/rdh/medium/nl_BE-rdh-medium.onnx.json"),
                new PiperModels("mls", "Dutch NL", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/nl/nl_NL/mls/medium/nl_NL-mls-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/nl/nl_NL/mls/medium/nl_NL-mls-medium.onnx.json"),
                new PiperModels("talesyntese", "Norwegian", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/no/no_NO/talesyntese/medium/no_NO-talesyntese-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/no/no_NO/talesyntese/medium/no_NO-talesyntese-medium.onnx.json"),
                new PiperModels("darkman", "Polish", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pl/pl_PL/darkman/medium/pl_PL-darkman-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pl/pl_PL/darkman/medium/pl_PL-darkman-medium.onnx.json"),
                new PiperModels("gosia", "Polish", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pl/pl_PL/gosia/medium/pl_PL-gosia-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pl/pl_PL/gosia/medium/pl_PL-gosia-medium.onnx.json"),
                new PiperModels("mc_speech", "Polish", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pl/pl_PL/mc_speech/medium/pl_PL-mc_speech-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pl/pl_PL/mc_speech/medium/pl_PL-mc_speech-medium.onnx.json"),
                new PiperModels("faber", "Portuguese BR", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pt/pt_BR/faber/medium/pt_BR-faber-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pt/pt_BR/faber/medium/pt_BR-faber-medium.onnx.json"),
                new PiperModels("tugão", "Portuguese PT", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pt/pt_PT/tug%C3%A3o/medium/pt_PT-tug%C3%A3o-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/pt/pt_PT/tug%C3%A3o/medium/pt_PT-tug%C3%A3o-medium.onnx.json"),
                new PiperModels("mihai", "Romanian", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ro/ro_RO/mihai/medium/ro_RO-mihai-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ro/ro_RO/mihai/medium/ro_RO-mihai-medium.onnx.json"),
                new PiperModels("dmitri", "Russian", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ru/ru_RU/dmitri/medium/ru_RU-dmitri-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ru/ru_RU/dmitri/medium/ru_RU-dmitri-medium.onnx.json"),
                new PiperModels("irina", "Serbian", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ru/ru_RU/irina/medium/ru_RU-irina-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/ru/ru_RU/irina/medium/ru_RU-irina-medium.onnx.json"),
                new PiperModels("lili", "Slovak ", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sr/sr_RS/serbski_institut/medium/sr_RS-serbski_institut-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sk/sk_SK/lili/medium/sk_SK-lili-medium.onnx.json"),
                new PiperModels("artur", "Slovenian ", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sl/sl_SI/artur/medium/sl_SI-artur-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sl/sl_SI/artur/medium/sl_SI-artur-medium.onnx.json"),
                new PiperModels("serbski_institut", "Serbian ", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sr/sr_RS/serbski_institut/medium/sr_RS-serbski_institut-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sr/sr_RS/serbski_institut/medium/sr_RS-serbski_institut-medium.onnx.json"),
                new PiperModels("nst", "Swedish", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sv/sv_SE/nst/medium/sv_SE-nst-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sv/sv_SE/nst/medium/sv_SE-nst-medium.onnx.json"),
                new PiperModels("lanfrica", "Swahili", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sw/sw_CD/lanfrica/medium/sw_CD-lanfrica-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/sw/sw_CD/lanfrica/medium/sw_CD-lanfrica-medium.onnx.json"),
                new PiperModels("fettah", "Turkish", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/tr/tr_TR/fettah/medium/tr_TR-fettah-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/tr/tr_TR/fettah/medium/tr_TR-fettah-medium.onnx.json"),
                new PiperModels("ukrainian_tts", "Ukrainian", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/uk/uk_UA/ukrainian_tts/medium/uk_UA-ukrainian_tts-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/uk/uk_UA/ukrainian_tts/medium/uk_UA-ukrainian_tts-medium.onnx.json"),
                new PiperModels("vais1000", "Vietnamese", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/vi/vi_VN/vais1000/medium/vi_VN-vais1000-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/vi/vi_VN/vais1000/medium/vi_VN-vais1000-medium.onnx.json"),
                new PiperModels("huayan", "Chinese", "medium", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/zh/zh_CN/huayan/medium/zh_CN-huayan-medium.onnx", "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/zh/zh_CN/huayan/medium/zh_CN-huayan-medium.onnx.json"),
            };

            return models;
        }
    }
}
