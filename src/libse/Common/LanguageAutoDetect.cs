﻿using Nikse.SubtitleEdit.Core.DetectEncoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class LanguageAutoDetect
    {

        private static int GetCount(string text, params string[] words)
        {
            var options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
            var pattern = "\\b(" + string.Join("|", words) + ")\\b";
            return Regex.Matches(text, pattern, options).Count;
        }

        private static int GetCountContains(string text, params string[] words)
        {
            int count = 0;
            foreach (var w in words)
            {
                var regEx = new Regex(w);
                count += regEx.Matches(text).Count;
            }
            return count;
        }

        public static string AutoDetectGoogleLanguage(Encoding encoding)
        {
            switch (encoding.CodePage)
            {
                case 860:
                    return "pt"; // Portuguese
                case 28599:
                case 1254:
                    return "tr"; // Turkish
                case 28598:
                case 1255:
                    return "he"; // Hebrew
                case 28596:
                case 1256:
                    return "ar"; // Arabic
                case 1258:
                    return "vi"; // Vietnamese
                case 949:
                case 1361:
                case 20949:
                case 51949:
                case 50225:
                    return "ko"; // Korean
                case 1253:
                case 28597:
                    return "el"; // Greek
                case 50220:
                case 50221:
                case 50222:
                case 51932:
                case 20932:
                case 10001:
                    return "ja"; // Japanese
                case 20000:
                case 20002:
                case 20936:
                case 950:
                case 52936:
                case 54936:
                case 51936:
                    return "zh"; // Chinese
                default:
                    return null;
            }
        }

        private static readonly string[] AutoDetectWordsEnglish =
        {
            "we", "are", "and", "your", "what", "[TW]hat's", "You're", "(any|some|every)thing", "money", "because", "human", "abandon",
            "welcome", "really", "something", "confusing", "about", "know", "people", "other", "which", "would", "these", "could",
            "tutorial", "I'll", "show", "you", "about", "absolutely", "accept", "wrong", "thought", "training", "treatment", "successful",
            "strong", "simply", "situation","soldier", "somebody", "someone","something", "sometimes", "speak","statement", "perhaps",
            "piece", "picture", "police", "popular", "population", "suggest", "weapon", "water", "window", "woman", "talk", "security",
            "season", "remain", "relationship", "report" ,"user" ,"result", "this",
        };

        private static readonly string[] AutoDetectWordsDanish =
        {
            "vi", "han", "og", "jeg", "var", "men", "gider", "bliver", "virkelig", "kommer", "tilbage", "Hej", "længere", "gjorde", "dig", "havde", "[Uu]ndskyld", "arbejder", "vidste", "troede", "stadigvæk", "[Mm]åske", "første", "gik",
            "fortælle", "begyndt", "spørgsmål", "pludselig", "være", "fordi", "været", "sidste","også", "løb", "sagde", "hjem",
        };

        private static readonly string[] AutoDetectWordsNorwegian =
        {
            "vi", "og", "jeg", "var", "men", "igjen", "Nei", "Hei", "noen", "gjøre", "kanskje", "[Tt]renger", "tenker", "skjer", "møte", "veldig", "takk", "penger", "konsept", "hjelp", "forsvunnet",
            "skutt", "sterkt", "minste", "første", "fortsette", "inneholder", "gikk", "fortelle", "begynt", "spørsmål", "plutselig"
        };

        private static readonly string[] AutoDetectWordsSwedish =
        {
            "vi", "är", "och", "Jag", "inte", "för", "måste", "Öppna", "Förlåt", "nånting", "ingenting", "jävla", "Varför", "[Ss]nälla", "fattar", "själv", "säger", "öppna", "jävligt", "dörren", "göra", "behöver", "tillbaka", "Varför",
            "träffa", "kanske", "säga", "hände", "honom", "hennes", "veckor", "tänker", "själv", "pratar", "mycket", "mamma", "dödade", "Ursäkta", "säger", "senaste", "håller", "förstår", "veckan", "varför", "tycker", "första", "gick",
            "berätta", "börjat", "människor", "frågor", "fråga", "plötsligt", "skämt"
        };

        private static readonly string[] AutoDetectWordsSpanish =
        {
            "qué", "eso", "muy", "estoy?", "ahora", "hay", "tú", "así", "cuando", "cómo", "él", "sólo", "quiero", "gracias", "puedo", "bueno", "soy", "hacer", "fue", "eres", "usted", "tienes", "puede", "[Ss]eñor", "ese", "voy", "quién",
            "creo", "hola", "dónde", "sus", "verdad", "quieres", "mucho", "entonces", "estaba", "tiempo", "esa", "mejor", "hombre", "hace", "dios", "también", "están", "siempre", "hasta", "ahí", "siento",
            "puedes", "Dónde", "está", "estoy", "Vámonos", "vistiendo", "Caliente", "chimenea"
        };

        private static readonly string[] AutoDetectWordsItalian =
        {
            "Buongiorno", "Grazie", "Cosa", "quest[ao]", "quell[ao]", "tutt[io]", "[st]uo", "qualcosa", "ancora", "sono", "bene", "più", "andare", "essere", "venire", "abbiamo", "andiamo", "ragazzi", "signore", "numero", "giorno",
            "propriamente", "sensitivo", "negativo", "davvero", "faccio", "voglio", "vuole", "perché", "allora", "niente", "anche", "stai", "detto", "fatto", "hanno", "molto", "stato", "siamo", "così", "vuoi", "noi", "vero", "loro",
            "fare", "gli", "due"
        };

        private static readonly string[] AutoDetectWordsFrench =
        {
            "pas", "[vn]ous", "ça", "une", "pour", "[mt]oi", "dans", "elle", "tout", "plus", "[bmt]on", "suis", "avec", "oui", "fait", "ils", "être", "faire", "comme", "était", "quoi", "ici", "veux",
            "vouloir", "quelque", "pouvoir", "rien", "dit", "où", "votre", "pourquoi", "sont", "cette", "peux", "alors", "comment", "avez", "très", "même", "merci", "ont", "aussi", "chose", "voir",
            "allez", "tous", "ces", "deux", "avoir", " pouvoir", "même", "œil", "sœur",
        };

        private static readonly string[] AutoDetectWordsPortuguese =
        {
            "[Nn]ão", "[Ee]ntão", "uma", "ele", "bem", "isso", "você", "sim", "meu", "muito", "estou", "ela", "fazer", "tem", "já", "minha", "tudo", "só", "tenho", "agora", "vou", "seu", "quem", "há",
            "lhe", "quero", "nós", "são", "ter", "coisa", "dizer", "eles", "pode", "bom", "mesmo", "mim", "estava", "assim", "estão", "até", "quer", "temos", "acho", "obrigado", "também", "tens",
            "deus", "quê", "ainda", "noite", "conversar", "Desculpe", "verdade"
        };

        private static readonly string[] AutoDetectWordsGerman =
        {
            "und", "auch", "sich", "bin", "hast", "möchte", "müssen", "weiß", "[Vv]ielleicht", "Warum", "jetzt", "Verdammt", "bist", "Darum", "sitzt", "Setz", "Das ist", "Du bist", "nicht", "Scheiße",
            "Nein", "für", "gesagt", "zwei", "richtig"
        };

        private static readonly string[] AutoDetectWordsDutch =
        {
            "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n", "hebben", "alleen", "Waarom", "stripboeken", "Goed", "geweest"
        };

        private static readonly string[] AutoDetectWordsPolish =
        {
            "Czy", "ale", "ty", "siê", "się", "jest", "mnie", "Proszę", "życie", "statku", "życia", "Czyli", "Wszystko", 
            "Wiem", "Przepraszam", "dobrze", "chciałam", "Dziękuję", "Żołnierzyk", "Łowca", "został", "stało", "dolarów",
            "wiadomości", "Dobrze", "będzie", "Dzień", "przyszłość", "Uratowałaś", "Cześć", "Trzeba", "zginąć", "walczyć", 
            "ludzkość", "maszyny", "Jeszcze", "okrążenie", "wyścigu", "porządku", "detektywie", "przebieralni", "który", 
            "śmierci", "zabić", "wiedźminie", "przeznaczenie",
        };

        private static readonly string[] AutoDetectWordsGreek =
        {
            "μου", "[Εε]ίναι", "αυτό", "Τόμπυ", "καλά", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις"
        };

        private static readonly string[] AutoDetectWordsRussian =
        {
            "[Ээч]?то", "[Нн]е", "[ТтМмбв]ы", "Да", "[Нн]ет", "Он", "его", "тебя", "как", "меня", "Но", "всё", "мне", "вас", "знаю", "ещё", "за", "нас", "чтобы", "был"
        };

        private static readonly string[] AutoDetectWordsBulgarian =
        {
            "беше", "[Бб]лагодаря", "бързо", "вас", "[Вв]аше", "[Вв]ече", "[Вв]иждам", "време", "[Вв]сичк[ио]", "години",
            "Да", "[Дд]обре", "дяволите", "за", "Защо", "защото", "[Зз]начи", "иска[мнш]", "[Кк]ак", "[Кк]акво", "като",
            "ко[еий]то", "малко", "много", "[Мм]оже[хш]?", "място", "нас", "[Нн]е", "н[еи]що", "н[ия]кой", "[Нн]яма",
            "преди", "повече", "става", "така", "[Тт]ова", "[Тт]олкова", "[Тт]рябва", "тук",
            "Хайде", "човек"
        };

        private static readonly string[] AutoDetectWordsUkrainian =
        {
            "Вона", "мене", "[Нн]і", "[Пп]ривіт", "[Цц]е", "[Щщ]о", "[Йй]ого", "[Вв]ін", "[Яя]к", "[Гг]аразд", "[Яя]кщо", "[Мм]ені", "[Тт]вій", "[Її]х", "[Вв]ітаю", "[Дд]якую", "вже", "було", "був", "цього",
            "нічого", "немає", "може", "знову", "бо", "щось", "щоб", "цим", "тобі", "хотів", "твоїх", "мої", "мій", "має", "їм", "йому", "дуже",
            "Привіт", "побачення", "тобою", "побачення", "Гаразд", "повинні",
        };

        private static readonly string[] AutoDetectWordsAlbanian =
        {
            "është", "këtë", "Unë", "mirë", "shumë", "Çfarë", "çfarë", "duhet", "Është", "mbrapa", "Faleminderit", "vërtet", "diçka", "gjithashtu", "gjithe", "eshte", "shume", "vetem", "tënde",
            "çmendur", "vullnetin", "vdekur"
        };

        private static readonly string[] AutoDetectWordsArabic =
        {
            "من", "هل", "لا", "في", "لقد", "ما", "ماذا", "يا", "هذا", "إلى", "على", "أنا", "أنت", "حسناً", "أيها", "كان", "كيف", "يكون", "هذه", "هذان", "الذي", "التي", "الذين", "هناك", "هنالك"
        };

        private static readonly string[] AutoDetectWordsFarsi =
        {
            "این", "برای", "اون", "سیب", "کال", "رو", "خيلي", "آره", "بود", "اون", "نيست", "اينجا", "باشه", "سلام", "ميکني", "داري", "چيزي", "چرا", "خوبه"
        };

        private static readonly string[] AutoDetectWordsHebrew =
        {
            "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב", "אֶת", "שֶׁל", "עַל", "הוּא",
            "אֲשֶׁר", "הִיא", "הַמְלֶט", "נָסִיךְ", "הֲלֹא", "עוֹד", "אֵין", "אֲנִי", "זֹאת", "וְלֹא",
            "כָּךְ", "הִנֵּה", "מְאֹד", "אֲדוֹנִי",
        };

        private static readonly string[] AutoDetectWordsVietnamese =
        {
            "không", "[Tt]ôi", "anh", "đó", "ông", "tôi", "phải", "người", "được", "Cậu", "chúng", "chuyện", "muốn", "những", "nhiều"
        };

        private static readonly string[] AutoDetectWordsHungarian =
        {
            "hogy", "lesz", "tudom", "vagy", "mondtam", "még", "vagyok", "csak", "Hát", "felesége", "Csak", "utána", "jött",
            "Miért", "Akkor", "magát", "holnap", "Tudja", "Köszönöm", "élet", "Örvendek", "vissza", "hogy", "tudom", "Rendben",
            "Istenem", "Gyerünk", "értem", "vagyok", "hiszem", "történt", "rendben", "olyan", "őket", "vannak", "mindig", "Kérlek",
            "Gyere", "kicsim", "vagyunk"
        };

        private static readonly string[] AutoDetectWordsTurkish =
        {
            "için", "Tamam", "Hayır", "benim", "daha", "deðil", "önce", "lazým", "çalýþýyor", "Aldırma",
            "burada", "efendim", "şey", "çok", "Çok", "için", "Merhaba", "Evet", "kötü", "musun",
            "güzel", "çünkü", "büyük", "Bebeğim", "olduğunu", "istiyorum", "değilsin", "bilmiyorum",
            "otursana", "Selam", "Tabii","konuda","istiyor","Tetekkürler", "istemiyorum", "Gerçekte"
        };

        private static readonly string[] AutoDetectWordsCroatianAndSerbian =
        {
            "sam", "ali", "nije", "Nije", "samo", "ovo", "kako", "dobro", "Dobro", "sve", "tako", "će", "mogu", "ću", "zašto", "nešto", "za", "misliš", "možeš", "možemo", "ništa", "znaš", "ćemo", "znam"
        };

        private static readonly string[] AutoDetectWordsCroatian =
        {
            "što", "ovdje", "gdje", "kamo", "tko", "prije", "uvijek", "vrijeme", "vidjeti", "netko", "vidio", "nitko", "bok", "lijepo", "oprosti", "htio", "mjesto", "oprostite", "čovjek", "dolje", "čovječe", "dvije", "dijete", "dio",
            "poslije", "događa", "vjerovati", "vjerojatno", "vjerujem", "točno", "razumijem", "vidjela", "cijeli", "svijet", "obitelj", "volio", "sretan", "dovraga", "svijetu", "htjela", "vidjeli", "negdje", "želio", "ponovno",
            "djevojka", "umrijeti", "čovjeka", "mjesta", "djeca", "osjećam", "uopće", "djecu", "naprijed", "obitelji", "doista", "mjestu", "lijepa", "također", "riječ", "tijelo"
        };

        private static readonly string[] AutoDetectWordsSerbian =
        {
            "šta", "ovde", "gde", "ko", "pre", "uvek", "vreme", "videti", "neko", "video", "niko", "ćao", "lepo", "izvini", "hteo", "mesto", "izvinite", "čovek", "dole", "čoveče", "dve", "dete", "deo", "posle", "dešava", "verovati",
            "verovatno", "verujem", "tačno", "razumem", "videla", "ceo", "svet", "porodica", "voleo", "srećan", "dođavola", "svetu", "htela", "videli", "negde", "želeo", "ponovo", "devojka", "umreti", "čoveka", "mesta", "deca", "osećam",
            "uopšte", "decu", "napred", "porodicu", "zaista", "mestu", "lepa", "takođe", "reč", "telo"
        };

        private static readonly string[] AutoDetectWordsSerbianCyrillic =
        {
            "ће", "ћемо", "Хоћу", "сам", "али", "није", "само", "ово", "како", "добро", "све", "тако", "ће", "могу", "ћу", "зашто", "нешто", "за", "шта", "овде", "бити", "чини", "учениче", "побегне", "остати", "Један", "Назад", "Молим"
        };

        private static readonly string[] AutoDetectWordsSerbianCyrillicOnly =
        {
            "дођавола", "Дођи", "пронађемо", "ће", "ћемо", "Хоћу", "чекић", "неће",
        };

        private static readonly string[] AutoDetectWordsIndonesian =
        {
            "yang", "tahu", "bisa", "akan", "tahun", "tapi", "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu"
        };

        private static readonly string[] AutoDetectWordsThai =
        {
            "ผู้กอง", "โรเบิร์ต", "วิตตอเรีย", "เข้าใจมั้ย", "คุณตำรวจ", "ราเชล", "เพื่อน", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์", "ขอโทษครับ", "ขอบคุณ", "วาร์กัส", "ทุกคน",
            "ไม่เอาน่า", "อิซานะ", "มจริง", "รับทราบ", "พอคะ", "ครับ", "อยาตขาป", "ยินดีทีดรูจักคะ", "ปลอดภัยดีนะ", "ทุกคน", "ตอนที", "ขอบคุณครับ", "ขอทษนะคะ", "ขอทษคะ"
        };

        private static readonly string[] AutoDetectWordsKorean =
        {
            "사루", "거야", "엄마", "그리고", "아니야", "하지만", "말이야", "그들은", "우리가", "엄마가", "괜찮아", "일어나", "잘했어", "뭐라고", "있어요",
            "있어요", "보여줘", "괜찮아요", "목소릴", "기억이", "저주를", "좋아요"
        };

        private static readonly string[] AutoDetectWordsMacedonian =
        {
            "господине", "Нема", "господине", "работа", "вселената", "Може", "треба", "Треба", "слетување", "капсулата", "време", "Френдшип", "Прием", "Добро", "пресметки", "Благодарам", "нешто", "Благодарам", "орбитата", "инженер",
            "Харисон", "Фала", "тоалет", "орбита", "знаеме", "Супервизор", "жени", "Добра", "требаат", "што", "дeкa", "eшe", "кучe", "Руиз", "кучeто", "кучињa", "Бјути", "имa", "многу", "кучињaтa", "AДЗЖ", "Животни", "моЖe", "мaчe",
            "мecто", "имaмe", "мaчињa", "пpвото", "пpaвaт", "нeшто", "колку",
            "што", "тоа", "дека", "Што", "Дали", "треба", "беше", "таму"
        };

        private static readonly string[] AutoDetectWordsFinnish =
        {
            "että", "kuin", "minä", "mitään", "Mutta", "siitä", "täällä", "poika", "Kiitos", "enää", "vielä", "tässä", "sulkeutuu", "Soitetaan", "Soita", "Onnistui", "Mitä", "Etuovi", "tippiä", "antaa", "Onko", "Hidasta", "tuntia",
            "tilata", "päästä", "palolaitokselle", "hätänumeroon", "aikaa", "Tämä", "Sinulla", "Palauttaa", "Kiitos", "Arvostele", "Älä", "toimi", "televisiota", "takaisin", "reitin", "pitäisi", "palauttaa", "nopeamman", "mitään",
            "meidät", "maksaa", "kuullut", "kaikki", "jälkeen", "ihmiset", "hätäkeskukseen", "hiljaa", "haluat", "enää", "enemmän", "auttaa", "Tunkeilijahälytys", "Pysähdy", "Princen", "Käänny", "Kyllä", "Kiva", "Haluatko", "Haluan"
        };

        private static readonly string[] AutoDetectWordsRomanian =
        {
            "pentru", "oamenii", "decât", "[Vv]reau", "[Ss]înt", "Asteaptã", "Fãrã", "aici", "domnule", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "fãcut", "spune", "făcut", "când", "aici", "Asta", "văzut", "dacă", "câteva",
            "amândoi", "Când", "totuși", "mașină", "aceeași", "întâmplat", "niște", "ziua", "noastră", "cunoscut", "decat", "[Tt]rebuie", "[Aa]cum", "Poate", "vrea", "soare", "nevoie", "daca", "echilibrul", "vorbesti", "zeului",
            "atunci", "memoria", "soarele"
        };

        // Czech and Slovak languages have many common words (especially when non flexed)
        private static readonly string[] AutoDetectWordsCzechAndSlovak =
        {
            "[Oo]n[ao]?", "[Jj]?si", "[Aa]le", "[Tt]en(to)?", "[Rr]ok", "[Tt]ak", "[Aa]by", "[Tt]am", "[Jj]ed(en|na|no)", "[Nn]ež", "[Aa]ni", "[Bb]ez", "[Dd]obr[ýáé]", "[Vv]šak", "[Cc]el[ýáé]", "[Nn]ov[ýáé]", "[Dd]ruh[ýáé]", "jsem",
            "poøádku", "Pojïme", "háje", "není", "Jdeme", "všecko", "jsme", "Prosím", "Vezmi", "když", "Takže", "Dìkuji", "prechádzku", "všetko", "Poïme", "potom", "Takže", "Neviem", "budúcnosti", "trochu"
        };
        // differences between Czech and Slovak languages / Czech words / please keep the words aligned between these languages for better comparison
        private static readonly string[] AutoDetectWordsCzech =
        {
            ".*[Řř].*", ".*[ůě].*", "[Bb]ýt", "[Jj]sem", "[Jj]si", "[Jj]á", "[Mm]ít", "[Aa]no", "[Nn]e",  "[Nn]ic", "[Dd]en", "[Jj]en", "[Cc]o", "[Jj]ak[o]?", "[Nn]ebo",  "[Pp]ři", "[Pp]ro", "[Pp]řed.*", "[Jj](ít|du|de|deme|dou)",
            "[Mm]ezi",  "[Jj]eště", "[Čč]lověk", "[Pp]odle", "[Dd]alší", "přejděte", "používat", "obrázky", "obrázek", "vašem", "vygenerovat"
        };
        // differences between Czech and Slovak languages / Slovak words / please keep the words aligned between these languages for better comparison
        private static readonly string[] AutoDetectWordsSlovak =
        {
            ".*[Ôô].*", ".*[ä].*",  "[Bb]yť", "[Ss]om",  "[Ss]i",  "[Jj]a", "[Mm]ať", "[Áá]no", "[Nn]ie", "[Nn]ič", "[Dd]eň", "[Ll]en", "[Čč]o", "[Aa]ko", "[Aa]?[Ll]ebo", "[Pp]ri", "[Pp]re", "[Pp]red.*", "([Íí]sť|[Ii](dem|de|deme|dú))",
            "[Mm]edzi", "[Ee]šte",  "[Čč]lovek", "[Pp]odľa", "[Ďď]alš(í|ia|ie)"
        };

        private static readonly string[] AutoDetectWordsSlovenian =
        {
            "sem", "bom", "Zakaj", "lahko", "Kje", "Lahko", "Hvala", "Ampak", "nekaj", "Prosim", "prišla", "nisem", "denar", "ampak",
            "všeč", "videti", "stanovanje", "službo", "prosim", "naredila", "moram", "domov", "Vrzi", "Povej", "številko", "zaporu",
            "ugrabila", "ubila", "tvoja", "tudi", "tečna", "stvari", "rusko", "povedala", "obraz", "nalogo", "mislim", "govoriš",
            "Seveda", "Razmišljam", "Potem", "Nič", "Nisem", "Mogoče", "žensko", "žalostna", "človek", "Čisto", "znaš",
            "zlomi", "zgodilo", "zdaj", "zajtrk", "utrujena","ustrelila", "srečna", "čarovnik", "zaživeti",
            "Torej", "plasti","kanjon", "več", "prišlo", "presenečeni", "največje"
        };

        private static readonly string[] AutoDetectWordsIcelandic =
        {
            "við",
            "Það",
            "með",
            "Hvað",
            "fyrir",
            "hvað",
            "verður",
            "segja",
            "þessu",
            "hérna",
            "maður",
            "Mér",
            "Guð",
            "þið",
            "svona",
            "okkur",
            "þau",
            "sé",
            "núna",
            "hefur",
            "hafa",
            "eða",
            "þegar",
            "þarna",
            "verð",
            "veist",
            "ætla",
            "okkar",
            "barnið",
            "eitthvað",
            "ólétt",
            "væri",
            "sagði",
            "Því",
            "ykkur",
            "annað",
            "Frábært",
            "Þakka",
            "góður",
            "Gerðu",
            "Fyrirgefðu",
            "verða",
            "kvöld",
            "alvöru",
            "Veistu",
            "Kannski",
            "Einmitt",
            "verðum",
            "líður",
            "höfum",
            "hugmynd",
            "Heyrðu",
            "skrýtið",
            "stefnumót",
            "skeið",
            "síðan",
            "hingað",
            "ótrúlegt",
            "Þekkirðu",
            "spýtustrákur",
            "miðskóla",
        };

        private static readonly string[] AutoDetectWordsLatvian =
        {
            "Paldies", "neesmu ", "nezinu", "viòð", "viņš", "viņu", "kungs", "esmu", "Viņš", "Velns", "viņa", "dievs", "Pagaidi", "varonis", "agrāk", "varbūt"
        };

        private static readonly string[] AutoDetectWordsLithuanian =
        {
            "tavęs", "veidai", "apie", "jums", "Veidai", "Kaip", "kaip", "reikia", "Šūdas", "frensis", "Ačiū", "vilsonai", "Palauk", "Veidas", "viskas", "Tikrai", "manęs", "Tačiau", "žmogau", "Flagai", "Prašau", "Džiune", "Nakties",
            "šviesybe", "Supratau", "komanda", "reikia", "apie", "Kodėl", "mūsų", "Ačiū", "vyksta"
        };

        private static readonly string[] AutoDetectWordsHindi =
        {
            "एक", "और", "को", "का", "यह", "सकते", "लिए", "करने", "भारतीय", "सकता", "भारत", "तकनीक", "कंप्यूटिंग", "उपकरण", "भाषाओं", "भाषा", "कंप्यूटर", "आप", "आपको", "अपने", "लेकिन", "करना", "सकता", "बहुत", "चाहते", "अच्छा", "वास्तव",
            "लगता", "इसलिए", "शेल्डन", "धन्यवाद।", "तरह", "करता", "चाहता", "कोशिश", "करते", "किया", "अजीब", "सिर्फ", "शुरू"
        };

        private static readonly string[] AutoDetectWordsUrdu =
        {
            "نہیں", "ایک", "ہیں", "کیا", "اور", "لئے", "ٹھیک", "ہوں", "مجھے", "تھا", "کرنے", "صرف", "ارے", "یہاں", "بہت", "لیکن", "ساتھ", "اپنے", "اچھا", "میرے", "چاہتا", "انہوں", "تمہیں"
        };

        private static readonly string[] AutoDetectWordsSinhalese =
        {
            "කරන්න", "නැහැ", "කියල", "මගේ", "එයා", "ඔයාට", "කැප්ටන්", "ඔයාගේ", "පුළුවන්", "හැම", "වගේ", "වෙන්න", "ඕනා", "නෙමේ", "තියෙන්නේ", "වගේම", "දන්නවා", "වෙනවා", "එහෙම", "හිතන්නේ", "කියලා", "කරනවා", "යන්න", "දෙයක්", "තියනවා",
            "හොදයි", "දෙන්න", "එකක්", "මොනාද", "කියන්න", "කරන්නේ", "වෙන්නේ", "රොජර්ස්", "දාන්න", "කරලා", "ඔයාව", "වෙලා", "කොහොමද", "කිවුවා", "ඔබට", "රාවුල්"
        };

        private static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            var count = GetCount(text, AutoDetectWordsEnglish);
            if (count > bestCount)
            {
                var dutchCount = GetCount(text, AutoDetectWordsDutch);
                if (dutchCount < count)
                {
                    return "en";
                }
            }

            count = GetCount(text, AutoDetectWordsDanish);
            if (count > bestCount)
            {
                var norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                var dutchCount = GetCount(text, AutoDetectWordsDutch);
                var swedishCount = GetCount(text, AutoDetectWordsSwedish);
                var icelandicCount = GetCount(text, AutoDetectWordsIcelandic);
                if (norwegianCount < 2 && dutchCount < count && swedishCount < count)
                {
                    if (icelandicCount > count * 1.5)
                    {
                        return "is";
                    }

                    return "da";
                }
            }

            count = GetCount(text, AutoDetectWordsNorwegian);
            if (count > bestCount)
            {
                var danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                var dutchCount = GetCount(text, AutoDetectWordsDutch);
                var swedishCount = GetCount(text, AutoDetectWordsSwedish);
                if (danishCount < 2 && dutchCount < count && swedishCount < count)
                {
                    return "no";
                }
            }

            count = GetCount(text, AutoDetectWordsSwedish);
            if (count > bestCount)
            {
                return "sv";
            }

            count = GetCount(text, AutoDetectWordsSpanish);
            if (count > bestCount)
            {
                var frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                var portugueseCount = GetCount(text, "[NnCc]ão", "Então", "h?ouve", "pessoal", "rapariga", "tivesse", "fizeste",
                                                     "jantar", "conheço", "atenção", "foste", "milhões", "devias", "ganhar", "raios"); // not spanish words
                if (frenchCount < 2 && portugueseCount < 2)
                {
                    return "es";
                }
            }

            count = GetCount(text, AutoDetectWordsItalian);
            if (count > bestCount)
            {
                var frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not italian words
                if (frenchCount < 2)
                {
                    return "it";
                }
            }

            count = GetCount(text, AutoDetectWordsFrench);
            if (count > bestCount)
            {
                var romanianCount = GetCount(text, "[Vv]reau", "[Ss]înt", "[Aa]cum", "pentru", "domnule", "aici");
                if (romanianCount < 5)
                {
                    return "fr";
                }
            }

            count = GetCount(text, AutoDetectWordsPortuguese);
            if (count > bestCount)
            {
                var slovenianCount = GetCount(text, AutoDetectWordsSlovenian);
                if (slovenianCount > count)
                {
                    return "sl";
                }

                return "pt"; // Portuguese
            }

            count = GetCount(text, AutoDetectWordsGerman);
            if (count > bestCount)
            {
                return "de";
            }

            count = GetCount(text, AutoDetectWordsDutch);
            if (count > bestCount)
            {
                return "nl";
            }

            count = GetCount(text, AutoDetectWordsPolish);
            if (count > bestCount)
            {
                var czechWordsCount = GetCount(text, AutoDetectWordsCzech);
                if (czechWordsCount > count)
                {
                    return "cs";
                }

                return "pl"; 
            }

            count = GetCount(text, AutoDetectWordsGreek);
            if (count > bestCount)
            {
                return "el"; // Greek
            }

            count = GetCount(text, AutoDetectWordsRussian);
            if (count > bestCount)
            {
                var bulgarianCount = GetCount(text, AutoDetectWordsBulgarian);
                var ukrainianCount = GetCount(text, AutoDetectWordsUkrainian);
                var macedonianCount = GetCount(text, AutoDetectWordsMacedonian);
                if (bulgarianCount > count)
                {
                    if (ukrainianCount > bulgarianCount && ukrainianCount > macedonianCount)
                    {
                        return "uk"; // Ukrainian
                    }

                    if (macedonianCount > bulgarianCount && macedonianCount > ukrainianCount)
                    {
                        return "mk"; // Macedonian
                    }

                    return "bg"; // Bulgarian
                }

                if (ukrainianCount > count)
                {
                    return "uk"; // Ukrainian
                }

                var serbianCount = GetCount(text, AutoDetectWordsSerbianCyrillic);
                if (serbianCount > count)
                {
                    return "sr"; // Serbian
                }

                var serbianWordsOnlyCount = GetCount(text, AutoDetectWordsSerbianCyrillicOnly);
                if (serbianWordsOnlyCount > 1)
                {
                    return "sr"; // Serbian
                }

                return "ru"; // Russian
            }

            count = GetCount(text, AutoDetectWordsUkrainian);
            if (count > bestCount)
            {
                return "uk"; // Ukrainian
            }

            count = GetCount(text, AutoDetectWordsBulgarian);
            if (count > bestCount)
            {
                var macedonianCount = GetCount(text, AutoDetectWordsMacedonian);
                if (macedonianCount > count)
                {
                    return "mk";
                }

                return "bg"; // Bulgarian
            }

            count = GetCount(text, AutoDetectWordsAlbanian);
            if (count > bestCount)
            {
                return "sq"; // Albanian
            }

            count = GetCount(text, AutoDetectWordsArabic);
            if (count > bestCount)
            {
                var hebrewCount = GetCount(text, AutoDetectWordsHebrew);
                var farsiCount = GetCount(text, AutoDetectWordsFarsi);
                if (hebrewCount < count && farsiCount < count)
                {
                    return "ar"; // Arabic
                }
            }

            count = GetCount(text, AutoDetectWordsHebrew);
            if (count > bestCount)
            {
                return "he"; // Hebrew
            }

            count = GetCount(text, AutoDetectWordsFarsi);
            if (count > bestCount)
            {
                return "fa"; // Farsi (Persian)
            }

            count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
            if (count > bestCount)
            {
                var croatianCount = GetCount(text, AutoDetectWordsCroatian);
                var serbianCount = GetCount(text, AutoDetectWordsSerbian);
                var slovenianCount = GetCount(text, AutoDetectWordsSlovenian);
                if (croatianCount > serbianCount)
                {
                    if (slovenianCount > croatianCount)
                    {
                        return "sl";
                    }

                    return "hr"; // Croatian
                }

                if (slovenianCount > count)
                {
                    return "sl";
                }

                return "sr"; // Serbian
            }

            count = GetCount(text, AutoDetectWordsVietnamese);
            if (count > bestCount)
            {
                return "vi"; // Vietnamese
            }

            count = GetCount(text, AutoDetectWordsHungarian);
            if (count > bestCount)
            {
                return "hu"; // Hungarian
            }

            count = GetCount(text, AutoDetectWordsTurkish);
            if (count > bestCount)
            {
                return "tr"; // Turkish
            }

            count = GetCount(text, AutoDetectWordsIndonesian);
            if (count > bestCount)
            {
                return "id"; // Indonesian
            }

            count = GetCount(text, AutoDetectWordsThai);
            if (count > 10 || count > bestCount)
            {
                return "th"; // Thai
            }

            count = GetCount(text, AutoDetectWordsKorean);
            if (count > 10 || count > bestCount)
            {
                return "ko"; // Korean
            }

            count = GetCount(text, AutoDetectWordsFinnish);
            if (count > bestCount)
            {
                return "fi"; // Finnish
            }

            count = GetCount(text, AutoDetectWordsRomanian);
            if (count > bestCount)
            {
                return "ro"; // Romanian
            }

            count = GetCountContains(text, "シ", "ュ", "シン", "シ", "ン", "ユ");
            count += GetCountContains(text, "イ", "ン", "チ", "ェ", "ク", "ハ");
            count += GetCountContains(text, "シ", "ュ", "う", "シ", "ン", "サ");
            count += GetCountContains(text, "シ", "ュ", "シ", "ン", "だ", "う");
            if (count > bestCount * 2)
            {
                return "ja"; // Japanese - not tested...
            }

            count = GetCountContains(text, "是", "是早", "吧", "的", "爱", "上好");
            count += GetCountContains(text, "的", "啊", "好", "好", "亲", "的");
            count += GetCountContains(text, "谢", "走", "吧", "晚", "上", "好");
            count += GetCountContains(text, "来", "卡", "拉", "吐", "滚", "他");
            if (count > bestCount * 2)
            {
                return "zh"; // Chinese (simplified) - not tested...
            }

            count = GetCount(text, AutoDetectWordsCzechAndSlovak);
            if (count > bestCount)
            {
                var lithuanianCount = GetCount(text, AutoDetectWordsLithuanian);
                int finnishCount = GetCount(text, AutoDetectWordsFinnish);
                if (lithuanianCount <= count && finnishCount < count)
                {
                    int czechWordsCount = GetCount(text, AutoDetectWordsCzech);
                    int slovakWordsCount = GetCount(text, AutoDetectWordsSlovak);
                    if (czechWordsCount >= slovakWordsCount)
                    {
                        return "cs"; // Czech
                    }

                    return "sk"; // Slovak
                }
            }

            count = GetCount(text, AutoDetectWordsSlovenian);
            if (count > bestCount)
            {
                return "sl";
            }

            count = GetCount(text, AutoDetectWordsLatvian);
            if (count > bestCount * 1.2)
            {
                return "lv";
            }

            count = GetCount(text, AutoDetectWordsLithuanian);
            if (count > bestCount)
            {
                return "lt";
            }

            count = GetCount(text, AutoDetectWordsHindi);
            if (count > bestCount)
            {
                return "hi";
            }

            count = GetCount(text, AutoDetectWordsUrdu);
            if (count > bestCount)
            {
                return "ur";
            }

            count = GetCount(text, AutoDetectWordsSinhalese);
            if (count > bestCount)
            {
                return "si";
            }

            count = GetCount(text, AutoDetectWordsMacedonian);
            if (count > bestCount)
            {
                return "mk";
            }

            count = GetCount(text, AutoDetectWordsIcelandic);
            if (count > bestCount)
            {
                return "is";
            }

            return string.Empty;
        }

        public static string AutoDetectGoogleLanguage(Subtitle subtitle)
        {
            return AutoDetectGoogleLanguageOrNull(subtitle) ?? "en";
        }

        public static string AutoDetectGoogleLanguage(Subtitle subtitle, int maxNumberOfSubtitlesToCheck)
        {
            const int averageLength = 40;
            var max = subtitle.Paragraphs.Count;
            var sb = new StringBuilder(max * averageLength);
            var index = 0;
            var emptyLines = 0;
            while (index < max && index < maxNumberOfSubtitlesToCheck + emptyLines)
            {
                var text = subtitle.Paragraphs[index].Text;
                if (string.IsNullOrEmpty(text))
                {
                    emptyLines++;
                }
                else
                {
                    sb.AppendLine(text);
                }

                index++;
            }

            var allText = sb.ToString();
            string languageId = AutoDetectGoogleLanguage(allText, maxNumberOfSubtitlesToCheck / 14);
            if (string.IsNullOrEmpty(languageId))
            {
                languageId = GetEncodingViaLetter(allText);
            }

            if (string.IsNullOrEmpty(languageId))
            {
                return "en";
            }

            return languageId;
        }

        public class LanguageForAutoDetect
        {
            public string LanguageCode { get; set; }
            public string[] Words { get; set; }
            public int WordCount { get; set; }
        }

        public static LanguageForAutoDetect[] GetLanguagesWithCount(string text)
        {
            var list = new LanguageForAutoDetect[]
            {
                new LanguageForAutoDetect
                {
                    LanguageCode = "en",
                    Words = AutoDetectWordsEnglish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "da",
                    Words = AutoDetectWordsDanish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "no",
                    Words = AutoDetectWordsNorwegian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sv",
                    Words = AutoDetectWordsSwedish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "es",
                    Words = AutoDetectWordsSpanish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "it",
                    Words = AutoDetectWordsItalian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "fr",
                    Words = AutoDetectWordsFrench,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "pt",
                    Words = AutoDetectWordsPortuguese,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "de",
                    Words = AutoDetectWordsGerman,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "nl",
                    Words = AutoDetectWordsDutch,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "pl",
                    Words = AutoDetectWordsPolish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "el",
                    Words = AutoDetectWordsGreek,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ru",
                    Words = AutoDetectWordsRussian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "bg",
                    Words = AutoDetectWordsBulgarian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "uk",
                    Words = AutoDetectWordsUkrainian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sq",
                    Words = AutoDetectWordsAlbanian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ar",
                    Words = AutoDetectWordsArabic,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "fa",
                    Words = AutoDetectWordsFarsi,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "he",
                    Words = AutoDetectWordsHebrew,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "vi",
                    Words = AutoDetectWordsVietnamese,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "hu",
                    Words = AutoDetectWordsHungarian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "tr",
                    Words = AutoDetectWordsTurkish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "-",
                    Words = AutoDetectWordsCroatianAndSerbian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "hr",
                    Words = AutoDetectWordsCroatian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sr",
                    Words = AutoDetectWordsSerbian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sr",
                    Words = AutoDetectWordsSerbianCyrillic,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sr-cy",
                    Words = AutoDetectWordsSerbianCyrillicOnly,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "id",
                    Words = AutoDetectWordsIndonesian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "th",
                    Words = AutoDetectWordsThai,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ko",
                    Words = AutoDetectWordsKorean,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "mk",
                    Words = AutoDetectWordsMacedonian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "fi",
                    Words = AutoDetectWordsFinnish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ro",
                    Words = AutoDetectWordsRomanian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "-",
                    Words = AutoDetectWordsCzechAndSlovak,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "cs",
                    Words = AutoDetectWordsCzech,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sk",
                    Words = AutoDetectWordsSlovak,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sl",
                    Words = AutoDetectWordsSlovenian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "is",
                    Words = AutoDetectWordsIcelandic,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "lv",
                    Words = AutoDetectWordsLatvian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "lt",
                    Words = AutoDetectWordsLithuanian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "hi",
                    Words = AutoDetectWordsHindi,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ur",
                    Words = AutoDetectWordsUrdu,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "si",
                    Words = AutoDetectWordsSinhalese,
                },
            };

            foreach (var item in list)
            {
                item.WordCount = GetCount(text, item.Words);
            }

            return list;
        }

        //TODO: improve... a lot ;)
        public static string AutoDetectGoogleLanguageOrNull2(Subtitle subtitle)
        {
            var s = new Subtitle(subtitle);
            s.RemoveEmptyLines();
            var allText = s.GetAllTexts(500000).TrimEnd();
            var languagesAndWords = GetLanguagesWithCount(allText);
            var languageAndWordHitsOrdered = languagesAndWords
                .Where(p => p.WordCount > 0)
                .OrderByDescending(p => p.WordCount);
            var languageIdFromWordCount = languageAndWordHitsOrdered
                .FirstOrDefault(p => p.LanguageCode != "-")?.LanguageCode;

            var languageIdViaLetters = GetEncodingViaLetter(allText);

            if (languageIdFromWordCount != null)
            {
                return languageIdFromWordCount;
            }

            if (languageIdViaLetters != null)
            {
                return languageIdViaLetters;
            }

            return null;
        }

        public static string AutoDetectGoogleLanguageOrNull(Subtitle subtitle)
        {
            var s = new Subtitle(subtitle);
            s.RemoveEmptyLines();
            var allText = s.GetAllTexts(500000).TrimEnd();
            var languageId = AutoDetectGoogleLanguage(allText, s.Paragraphs.Count / 14);
            if (string.IsNullOrEmpty(languageId))
            {
                languageId = GetEncodingViaLetter(allText);
            }

            if (string.IsNullOrEmpty(languageId))
            {
                languageId = null;
            }

            return languageId;
        }

        public static string AutoDetectLanguageName(string languageName, Subtitle subtitle)
        {
            if (string.IsNullOrEmpty(languageName))
            {
                languageName = "en_US";
            }

            var bestCount = subtitle.Paragraphs.Count / 14;

            var text = subtitle.GetAllTexts();
            var dictionaryNames = Utilities.GetDictionaryLanguages();

            var containsEnGb = false;
            var containsEnUs = false;
            var containsHrHr = false;
            var containsSrLatn = false;
            foreach (var name in dictionaryNames)
            {
                if (name.Contains("[en_GB]"))
                {
                    containsEnGb = true;
                }

                if (name.Contains("[en_US]"))
                {
                    containsEnUs = true;
                }

                if (name.Contains("[hr_HR]"))
                {
                    containsHrHr = true;
                }

                if (name.Contains("[sr_Latn]"))
                {
                    containsSrLatn = true;
                }
            }

            foreach (var name in dictionaryNames)
            {
                var shortName = string.Empty;
                var start = name.IndexOf('[');
                var end = name.IndexOf(']');
                if (start >= 0 && end > start)
                {
                    start++;
                    shortName = name.Substring(start, end - start);
                }

                int count;
                switch (shortName.Replace("-", "_").ToLowerInvariant())
                {
                    case "da_dk":
                        count = GetCount(text, AutoDetectWordsDanish);
                        if (count > bestCount)
                        {
                            int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                            int dutchCount = GetCount(text, AutoDetectWordsDutch);
                            if (norwegianCount < 2 && dutchCount < count)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "nb_no":
                        count = GetCount(text, AutoDetectWordsNorwegian);
                        if (count > bestCount)
                        {
                            int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                            int dutchCount = GetCount(text, AutoDetectWordsDutch);
                            if (danishCount < 2 && dutchCount < count)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "sv_se":
                        count = GetCount(text, AutoDetectWordsSwedish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "en_us":
                        count = GetCount(text, AutoDetectWordsEnglish);
                        if (count > bestCount)
                        {
                            int dutchCount = GetCount(text, AutoDetectWordsDutch);
                            if (dutchCount < count)
                            {
                                languageName = shortName;
                                bestCount = count;
                                if (containsEnGb)
                                {
                                    int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
                                    int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
                                    if (gbCount > usCount)
                                    {
                                        languageName = "en_GB";
                                    }
                                }
                            }
                        }
                        break;
                    case "en_gb":
                        count = GetCount(text, AutoDetectWordsEnglish);
                        if (count > bestCount)
                        {
                            int dutchCount = GetCount(text, AutoDetectWordsDutch);
                            if (dutchCount < count)
                            {
                                languageName = shortName;
                                bestCount = count;
                                if (containsEnUs)
                                {
                                    int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
                                    int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
                                    if (gbCount < usCount)
                                    {
                                        languageName = "en_US";
                                    }
                                }
                            }
                        }
                        break;
                    case "es_any":
                    case "es_es":
                        count = GetCount(text, AutoDetectWordsSpanish);
                        if (count > bestCount)
                        {
                            int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                            int portugueseCount = GetCount(text, "[NnCc]ão", "Então", "h?ouve", "pessoal", "rapariga", "tivesse", "fizeste",
                                "jantar", "conheço", "atenção", "foste", "milhões", "devias", "ganhar", "raios"); // not spanish words
                            if (frenchCount < 2 && portugueseCount < 2)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "it_it":
                        count = GetCount(text, AutoDetectWordsItalian);
                        if (count > bestCount)
                        {
                            int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not italian words
                            int spanishCount = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not italian words
                            if (frenchCount < 2 && spanishCount < 2)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "fr_fr":
                        count = GetCount(text, AutoDetectWordsFrench);
                        if (count > bestCount)
                        {
                            int romanianCount = GetCount(text, "[Vv]reau", "[Ss]înt", "[Aa]cum", "pentru", "domnule", "aici");
                            int spanishCount = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                            int italianCount = GetCount(text, AutoDetectWordsItalian);
                            if (romanianCount < 5 && spanishCount < 2 && italianCount < 2)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "de_de":
                        count = GetCount(text, AutoDetectWordsGerman);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "nl_nl":
                        count = GetCount(text, AutoDetectWordsDutch);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "pl_pl":
                        count = GetCount(text, AutoDetectWordsPolish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "el_gr":
                        count = GetCount(text, AutoDetectWordsGreek);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ru_ru":
                        count = GetCount(text, AutoDetectWordsRussian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "uk_ua":
                        count = GetCount(text, AutoDetectWordsUkrainian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ro_ro":
                        count = GetCount(text, AutoDetectWordsRomanian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "hr_hr": // Croatian
                        count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
                        if (count > bestCount)
                        {
                            bestCount = count;
                            languageName = shortName;
                            if (containsSrLatn)
                            {
                                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                                if (serbianCount > croatianCount)
                                {
                                    languageName = "sr-Latn";
                                }
                            }
                        }
                        break;
                    case "sr_latn": // Serbian (Latin)
                        count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                            if (containsHrHr)
                            {
                                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                                if (serbianCount < croatianCount)
                                {
                                    languageName = "hr_HR";
                                }
                            }
                        }
                        break;
                    case "sr": // Serbian (Cyrillic)
                        count = GetCount(text, AutoDetectWordsSerbianCyrillic);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "pt_pt": // Portuguese Portugal
                    case "pt_br": // Portuguese Brazil
                        count = GetCount(text, AutoDetectWordsPortuguese);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "hu_hu": // Hungarian
                        count = GetCount(text, AutoDetectWordsHungarian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "cs_cz": // Czech
                        count = GetCount(text, AutoDetectWordsCzech);
                        if (count > bestCount)
                        {
                            var lithuanianCount = GetCount(text, AutoDetectWordsLithuanian);
                            if (count > lithuanianCount)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "sk_sk": // Slovak
                        count = GetCount(text, AutoDetectWordsSlovak);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "lv_lv": // Latvian
                        count = GetCount(text, AutoDetectWordsLatvian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "lt_lt": // Lithuanian
                    case "lt":    // Lithuanian (Neutral)
                        count = GetCount(text, AutoDetectWordsLithuanian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "hi_in": // Hindi
                    case "hi":
                        count = GetCount(text, AutoDetectWordsHindi);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ur_ur": // Urdu
                    case "ur":
                        count = GetCount(text, AutoDetectWordsUrdu);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "si_si": // Sinhalese
                    case "si_lk": // Sinhala (Sri Lanka)
                    case "si":
                        count = GetCount(text, AutoDetectWordsSinhalese);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "tr_tr": // Turkish
                        count = GetCount(text, AutoDetectWordsTurkish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "he_il": // Hebrew
                        count = GetCount(text, AutoDetectWordsHebrew);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "vi_vn": // Vietnamese
                        count = GetCount(text, AutoDetectWordsVietnamese);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ar": // Arabic
                    case "ar_ar":
                        count = GetCount(text, AutoDetectWordsArabic);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "sq_al": // Albanian
                        count = GetCount(text, AutoDetectWordsAlbanian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "id_id": // Indonesian
                        count = GetCount(text, AutoDetectWordsIndonesian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ko_kr": // Korean
                        count = GetCount(text, AutoDetectWordsKorean);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "mk_mk": // Macedonian
                        count = GetCount(text, AutoDetectWordsMacedonian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "fa": // Farsi (Persian)
                    case "fa_ir":
                        count = GetCount(text, AutoDetectWordsFarsi);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "sl": // Slovenian
                    case "sl_si":
                        count = GetCount(text, AutoDetectWordsSlovenian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "is_is": // Icelandic
                    case "is":    // Icelandic (Neutral)
                        count = GetCount(text, AutoDetectWordsIcelandic);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                }
            }
            return languageName;
        }

        public static Encoding DetectAnsiEncoding(byte[] buffer)
        {
            try
            {
                var greekEncoding = Encoding.GetEncoding(1253); // Greek
                if (GetCount(greekEncoding.GetString(buffer), AutoDetectWordsGreek) > 5)
                {
                    return greekEncoding;
                }

                var russianEncoding = Encoding.GetEncoding(1251); // Cyrillic
                var textEnc1251 = russianEncoding.GetString(buffer);
                if (GetCount(textEnc1251, "что", "быть", "весь", "этот", "один", "такой") > 5) // Russian
                {
                    return russianEncoding;
                }

                var wordMinCount = buffer.Length / 300;

                if (GetCount(textEnc1251, AutoDetectWordsSerbianCyrillic) > wordMinCount)
                {
                    return russianEncoding;
                }

                if (GetCount(textEnc1251, "Какво", "тук", "може", "Как", "Ваше", "какво") > 5) // Bulgarian
                {
                    return russianEncoding;
                }


                if (GetCount(textEnc1251, AutoDetectWordsSerbianCyrillic) > wordMinCount) // Serbian
                {
                    return russianEncoding;
                }

                var encoding1250 = Encoding.GetEncoding(1250); // Central European/Eastern European: Polish, Czech, Slovak, Hungarian, Slovene, Bosnian, Croatian, Serbian (Latin script), Romanian (before 1993 spelling reform) and Albanian
                var textEnc1250 = encoding1250.GetString(buffer);
                if (GetCount(textEnc1250, AutoDetectWordsCroatianAndSerbian) > wordMinCount)
                {
                    return encoding1250;
                }

                if (GetCount(textEnc1250, AutoDetectWordsCzechAndSlovak) > wordMinCount)
                {
                    return encoding1250;
                }

                if (GetCount(textEnc1250, AutoDetectWordsHungarian) > wordMinCount)
                {
                    return encoding1250;
                }

                var encoding1252 = Encoding.GetEncoding(1252); // Latin - English and some other Western languages
                var textEnc1252 = encoding1252.GetString(buffer);
                var pol1252Count = GetCount(textEnc1252, AutoDetectWordsPolish);
                var pol1250Count = GetCount(textEnc1250, AutoDetectWordsPolish);
                var encoding28592 = Encoding.GetEncoding(28592);
                var pol28592Count = GetCount(encoding28592.GetString(buffer), AutoDetectWordsPolish);
                if (pol1252Count > wordMinCount || pol1250Count > wordMinCount)
                {
                    if (pol28592Count > pol1250Count && pol28592Count > pol1252Count)
                    {
                        return encoding28592;
                    }

                    return pol1252Count > pol1250Count ? encoding1252 : encoding1250;
                }

                var dutchCount1252 = GetCount(textEnc1252, AutoDetectWordsDutch);
                if (dutchCount1252 > wordMinCount)
                {
                    return encoding1252;
                }

                var danishCount1252 = GetCount(textEnc1252, AutoDetectWordsDanish);
                if (danishCount1252 > wordMinCount)
                {
                    return encoding1252;
                }

                var swedishCount1252 = GetCount(textEnc1252, AutoDetectWordsSwedish);
                if (swedishCount1252 > wordMinCount)
                {
                    return encoding1252;
                }

                var germanCount1252 = GetCount(textEnc1252, AutoDetectWordsGerman);
                if (germanCount1252 > wordMinCount)
                {
                    return encoding1252;
                }

                var spanishCount1252 = GetCount(textEnc1252, AutoDetectWordsSpanish);
                if (textEnc1252.IndexOf('¡') >= 0)
                {
                    spanishCount1252 += 5;
                }
                if (textEnc1252.IndexOf('¿') >= 0)
                {
                    spanishCount1252 += 5;
                }
                if (spanishCount1252 > wordMinCount && (textEnc1252.Contains('ú') || textEnc1252.Contains('í') || textEnc1252.Contains('ú') || textEnc1252.Contains('ó') || textEnc1252.Contains('é') || textEnc1252.Contains('ñ')))
                {
                    var weirdChars = CountWeirdCharacters(textEnc1252);
                    if (weirdChars < 3)
                    {
                        return encoding1252;
                    }
                }

                var russianEncoding28595 = Encoding.GetEncoding(28595); // Russian
                if (GetCount(russianEncoding28595.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5) // Russian
                {
                    return russianEncoding28595;
                }

                var thaiEncoding = Encoding.GetEncoding(874); // Thai
                var thaiCount = GetCount(thaiEncoding.GetString(buffer), AutoDetectWordsThai);
                var koreanEncoding = Encoding.GetEncoding(949); // Korean
                var koreanCount = GetCount(koreanEncoding.GetString(buffer), AutoDetectWordsKorean);
                if (thaiCount > 10 || koreanCount > 10)
                {
                    return thaiCount > koreanCount ? thaiEncoding : koreanEncoding;
                }

                var arabicEncoding = Encoding.GetEncoding(1256); // Arabic
                var hebrewEncoding = Encoding.GetEncoding(1255); // Hebrew
                if (GetCount(arabicEncoding.GetString(buffer), AutoDetectWordsArabic) > 5)
                {
                    if (GetCount(hebrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 10)
                    {
                        return hebrewEncoding;
                    }

                    return arabicEncoding;
                }
                if (GetCount(hebrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 5)
                {
                    return hebrewEncoding;
                }

                if (GetCount(textEnc1250, "să", "şi", "văzut", "regulă", "găsit", "viaţă") > 99)
                {
                    return encoding1250;
                }

                var encoding28591 = Encoding.GetEncoding(28591);
                var frenchCount1252 = GetCount(textEnc1252, AutoDetectWordsFrench);
                if (frenchCount1252 > wordMinCount)
                {
                    var frenchCount28591 = GetCount(encoding28591.GetString(buffer), AutoDetectWordsFrench);
                    return frenchCount28591 > frenchCount1252 ? encoding28591 : encoding1252;
                }

                var portugueseCount1252 = GetCount(textEnc1252, AutoDetectWordsPortuguese);
                if (portugueseCount1252 > wordMinCount)
                {
                    var portugueseCount28591 = GetCount(encoding28591.GetString(buffer), AutoDetectWordsPortuguese);
                    return portugueseCount28591 > portugueseCount1252 ? encoding28591 : encoding1252;
                }

                var encoding1254 = Encoding.GetEncoding(1254);
                var turkishText = encoding1254.GetString(buffer);
                var turkish1254Count = GetCount(turkishText, AutoDetectWordsTurkish);
                if (turkish1254Count > wordMinCount)
                {
                    return encoding1254;
                }

                var encoding = EncodingTools.DetectInputCodepage(buffer);
                if (encoding.CodePage == 28599)
                {
                    return Encoding.GetEncoding(1254); // prefer 1254 over 28599 
                }

                if (!Configuration.Settings.General.DefaultEncoding.StartsWith("UTF-8", StringComparison.Ordinal))
                {
                    // Use default ANSI encoding
                    foreach (var enc in Configuration.AvailableEncodings)
                    {
                        if (Configuration.Settings.General.DefaultEncoding.StartsWith(enc.CodePage + ":", StringComparison.Ordinal))
                        {
                            return enc;
                        }
                    }
                }

                return encoding;
            }
            catch
            {
                return Encoding.Default;
            }
        }

        private static int CountWeirdCharacters(string textEnc1252)
        {
            var count = 0;
            foreach (var c in textEnc1252)
            {
                if ("¤÷®¬¦¯".Contains(c))
                {
                    count++;
                }
            }

            return count;
        }

        public static Encoding GetEncodingFromFile(string fileName, bool skipAnsiAuto = false)
        {
            var encoding = Encoding.Default;

            try
            {
                foreach (var enc in Configuration.AvailableEncodings)
                {
                    if (enc.WebName == Configuration.Settings.General.DefaultEncoding && enc.WebName != Encoding.Unicode.WebName && enc.WebName != Encoding.UTF8.WebName)
                    {
                        encoding = enc;
                        break;
                    }
                }

                using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bom = new byte[12]; // Get the byte-order mark, if there is one
                    file.Position = 0;
                    var readCount = file.Read(bom, 0, bom.Length);
                    if (readCount < 3)
                    {
                        return encoding;
                    }

                    if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                    {
                        encoding = Encoding.UTF8;
                    }
                    else if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0)
                    {
                        encoding = Encoding.GetEncoding(12000); // UTF-32 (LE)
                    }
                    else if (bom[0] == 0xff && bom[1] == 0xfe)
                    {
                        encoding = Encoding.Unicode;
                    }
                    else if (bom[0] == 0xfe && bom[1] == 0xff) // utf-16 and ucs-2
                    {
                        encoding = Encoding.BigEndianUnicode;
                    }
                    else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) // ucs-4
                    {
                        encoding = Encoding.GetEncoding(12001); // UTF-32 (BE)
                    }
                    else if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76 && (bom[3] == 0x38 || bom[3] == 0x39 || bom[3] == 0x2b || bom[3] == 0x2f)) // utf-7
                    {
                        encoding = Encoding.UTF7;
                    }
                    else if (file.Length > bom.Length)
                    {
                        long length = file.Length;
                        if (length > 500000)
                        {
                            length = 500000;
                        }

                        file.Position = 0;
                        var buffer = new byte[length];
                        file.Read(buffer, 0, buffer.Length);

                        if (IsUtf8(buffer, out var couldBeUtf8))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else if (couldBeUtf8 && Configuration.Settings.General.DefaultEncoding.StartsWith("UTF-8", StringComparison.Ordinal))
                        { // keep utf-8 encoding if it's default
                            encoding = Encoding.UTF8;
                        }
                        else if (couldBeUtf8 && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) && Encoding.Default.GetString(buffer).ToLowerInvariant().Replace('\'', '"').Contains("encoding=\"utf-8\""))
                        { // keep utf-8 encoding for xml files with utf-8 in header (without any utf-8 encoded characters, but with only allowed utf-8 characters)
                            encoding = Encoding.UTF8;
                        }
                        else if (Configuration.Settings.General.AutoGuessAnsiEncoding && !skipAnsiAuto)
                        {
                            var autoDetected = DetectAnsiEncoding(buffer);
                            if (autoDetected != null)
                            {
                                return autoDetected.CodePage < 874 ? Encoding.Default : autoDetected;
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return encoding;
        }

        /// <summary>
        /// Will try to determine if buffer is utf-8 encoded or not.
        /// If any non-utf8 sequences are found then false is returned, if no utf8 multi bytes sequences are found then false is returned.
        /// </summary>
        private static bool IsUtf8(byte[] buffer, out bool couldBeUtf8)
        {
            couldBeUtf8 = false;
            var utf8Count = 0;
            var i = 0;
            while (i < buffer.Length - 3)
            {
                byte b = buffer[i];
                if (b > 127)
                {
                    if (b >= 194 && b <= 223 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191)
                    { // 2-byte sequence
                        utf8Count++;
                        i++;
                    }
                    else if (b >= 224 && b <= 239 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191 &&
                                                     buffer[i + 2] >= 128 && buffer[i + 2] <= 191)
                    { // 3-byte sequence
                        utf8Count++;
                        i += 2;
                    }
                    else if (b >= 240 && b <= 244 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191 &&
                                                     buffer[i + 2] >= 128 && buffer[i + 2] <= 191 &&
                                                     buffer[i + 3] >= 128 && buffer[i + 3] <= 191)
                    { // 4-byte sequence
                        utf8Count++;
                        i += 3;
                    }
                    else
                    {
                        return false;
                    }
                }
                i++;
            }

            couldBeUtf8 = true;
            if (utf8Count == 0)
            {
                return false; // not utf-8 (no characters utf-8 encoded...)
            }

            return true;
        }

        private static readonly char[] RightToLeftLetters = string.Concat(AutoDetectWordsArabic.Concat(AutoDetectWordsHebrew).Concat(AutoDetectWordsFarsi).Concat(AutoDetectWordsUrdu)).Distinct().ToArray();

        public static bool CouldBeRightToLeftLanguage(Subtitle subtitle)
        {
            const int maxNumberOfLinesToCheck = 20;
            var max = Math.Min(maxNumberOfLinesToCheck, subtitle.Paragraphs.Count);
            for (int i = 0; i < max; i++)
            {
                if (ContainsRightToLeftLetter(subtitle.Paragraphs[i].Text))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsRightToLeftLetter(string text)
        {
            foreach (var letter in RightToLeftLetters)
            {
                if (text.Contains(letter))
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetEncodingViaLetter(string text)
        {
            var dictionary = new Dictionary<string, int>(6);

            // Arabic
            var count = 0;
            foreach (var letter in Letters.Arabic)
            {
                if (text.Contains(letter))
                {
                    count++;
                }
            }
            dictionary.Add("ar", count);

            // Korean
            count = 0;
            foreach (var letter in Letters.Korean)
            {
                if (text.Contains(letter))
                {
                    count++;
                }
            }
            dictionary.Add("ko", count);

            // Japanese
            count = 0;
            foreach (var letter in Letters.Japanese)
            {
                if (text.Contains(letter))
                {
                    count++;
                }
            }
            dictionary.Add("ja", count);

            // Thai
            count = 0;
            foreach (var letter in Letters.Thai)
            {
                if (text.Contains(letter))
                {
                    count++;
                }
            }
            dictionary.Add("th", count);

            // Sinhalese
            count = 0;
            foreach (var letter in Letters.Sinhalese)
            {
                if (text.Contains(letter))
                {
                    count++;
                }
            }
            dictionary.Add("si", count);

            // Urdu
            count = 0;
            foreach (var letter in Letters.Urdu)
            {
                if (text.Contains(letter))
                {
                    count++;
                }
            }
            dictionary.Add("ur", count);

            var maxHitsLanguage = dictionary.FirstOrDefault(x => x.Value == dictionary.Values.Max());
            return maxHitsLanguage.Value > 0 ? maxHitsLanguage.Key : null;
        }

        public static bool IsLanguageWithoutPeriods(string language)
        {
            return language == "ko" || language == "zh" || language == "ja" || language == "th";
        }

        public static class Letters
        {
            public const string Arabic = "غظضذخثتشرقصفعسنملكيطحزوهدجبا";
            public const string Korean = "가나다라마바사아자차카타파하아야어여오요우유으이대한민국활화산동물들천국의섬유독춤을춥니다";
            public const string Japanese = "あいうえおかきくけこがぎぐげごさしすせそざじずぜぞたちつてとだぢづでどなにぬねのはひふへほばびぶべぼぱぴぷぺぽまみむめもやゆよらりるれろわをん";
            public const string Thai = "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรลวศษสหฬอฮ";
            public const string Urdu = "ﺍﺎﺁﺂﺏﺑﺒﺐﭖﭘﭙﭗﺕﺗﺘﺖﭦﭨﭩﭧﺙﺛﺜﺚﺝﺟﺠﺞﭺﭼﭽﭻﺡﺣﺤﺢﺥﺧﺨﺦﺩﺪﺫﺬﺭﺮﮌﮍﺯﺰﮊﮋﺱﺳﺴﺲﺵﺷﺸﺶﺹﺻﺼﺺﺽﺿﻀﺾﻁﻃﻄﻂﻅﻇﻈﻆﻉﻋﻌﻊﻍﻏﻐﻎﻑﻓﻔﻒﻕﻗﻘﻖﻙﻛﻜﻚﻻﻼﻝﻟﻠﻞﻡﻣﻤﻢﻥﻧﻨﻦﻭﻮﮮﮯﮦﮨﮩﮧﯼﯾﯿﯽﮪﮬﮭﮫﴽﴼﺀﺋﺌﹱﹷﹹ";
            public const string Sinhalese = "අආඇඈඉඊඋඌඍඎඏඐඑඒඓඔඕඖකඛගඝඞඟචඡජඣඤඥඦටඨඩඪණඬතථදධනඳපඵබභමඹයරලවශෂසහළෆ්ාැෑිීුූෘෙේෛොෝෞෟ෦෧෨෩෪෫෬෭෮෯";
        }
    }
}
