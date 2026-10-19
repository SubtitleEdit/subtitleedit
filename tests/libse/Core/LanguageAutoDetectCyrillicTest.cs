using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// Macedonian, Serbian and Bulgarian share most of their short words (не, да, за, нас, може,
/// така, беше, ...), so natural Macedonian or Serbian text used to be reported as Bulgarian or
/// Russian (user mail, 2026-10-19: "the language shown is Bulgarian instead of Macedonian").
/// The spelling tells them apart for sure: ј/љ/њ/џ are Macedonian and Serbian only, ќ/ѓ/ѕ
/// Macedonian only, ћ/ђ Serbian only, and ъ/ы/э/ё/й/щ/ь/ю/я never occur in either.
/// </summary>
public class LanguageAutoDetectCyrillicTest
{
    // Plot summaries of the same film, in the three languages.
    private const string Macedonian = @"„Пред дождот“ е македонски воен филм од 1994 година, во режија на Милчо Манчевски. Се состои од три поврзани дела: „Зборови“, „Лица“ и „Слики“.
Во првиот дел, во еден манастир, младиот монах Кирил крие Замира, девојка од албанско семејство, која бега од селска потера. Кога селаните ја откриваат, монахот заминува од манастирот заедно со неа, но нивната заедничка иднина завршува трагично.
Вториот дел се случува во Лондон. Фотографот Александар Кирков, Македонец и добитник на Пулицерова награда, има љубовна врска со Ен. Нивната врска е прекината со трагедија во кафеана, каде загинува нејзиниот сопруг.
Третиот дел се одвива во Македонија, каде Александар се враќа во родниот крај, кој е поделен меѓу два народа и полн со недоверба. Тој се обидува да ја спаси Замира од обвинение за убиство, но на крајот загинува од рака на роднина.";

    private const string Bulgarian = @"Преди дъжда е драматичен филм от 1994 г. на режисьора и сценариста Милчо Манчевски, копродукция между Република Македония и Великобритания. В главните роли са Катрин Картлидж, Раде Шербеджия, Грегоар Колин и Лабина Митевска. Музиката е на македонската група „Анастасия“. Филмът е номиниран за „Оскар“ и печели „Златен лъв“ във Венеция през 1994 г.
Историята е разделена на три части, които разказват за невъзможни любови:
Първата част следва млад монах, Кирил, който е положил обет за мълчание, но го нарушава заради албанско момиче, Замира.
Втората част се развива в Лондон и проследява Ан, редактор на снимки, разкъсана между съпруга си Ник и военния фотограф Александър.
Третата част обединява предишните две истории. Александър се връща в Северна Македония, където се среща с Хана, в която е бил влюбен преди заминаването си. Той тръгва да търси Замира, дъщерята на Хана, но на хоризонта се надига буря и филмът започва отначало.";

    private const string Serbian = @"Пре кише је македонски драмски филм из 1994. године, у режији и по сценарију Милча Манчевског. Подељен је у три дела: „Зборови”, „Лица” и „Слике”, а сваки има у средишту трагичну и несрећну љубав.
У делу „Зборови” монах Ћирило, који се заветовао на ћутање у манастиру у македонским планинама, устаје да брани Замиру, младу Албанку која бежи од руље оптужене за убиство. Заједно напуштају манастир и беже кроз планину, али њихова прича се нагло и брутално завршава.
У делу „Лица” радња се одвија у Лондону, где Ана живи растрзана између љубави према супругу Нику и привлачности према разочараном македонском ратном фотографу Александру. Она запада у низ трагичних догађаја након пуцњаве у оближњем ресторану.
У делу „Слике” Александар се враћа у родно село, које је рат поделио по националности и у којем се комшије Албанци гледају као непријатељи. Тамо га стара љубав Хана моли да сачува њену ћерку Замиру. Док Александар тражи Замиру, на хоризонту се ствара олуја, и филм се враћа на почетак.";

    private static readonly string[] MacedonianDialogue =
    {
        "Што правиш тука?", "Не знам, благодарам.", "Ќе видиме што ќе се случи.", "Јас сум тука веќе два часа.",
        "Дали си добро?", "Да, се разбира.", "Тој рече дека ќе дојде утре.", "Не можам да верувам.",
        "Ајде, одиме!", "Каде беше вчера?", "Сакам да ти кажам нешто.", "Многу ми е жал.",
        "Ништо не е во ред.", "Треба да одиме сега.", "Зошто го направи тоа?", "Таа е мојата сестра.",
        "Ние сме пријатели.", "Добро утро, господине.", "Колку чини ова?", "Никогаш не сум бил таму.",
        "Те молам, слушај ме.", "Луѓето зборуваат.", "Ова е многу важно.", "Знаеш ли кој е тој?",
        "Еве, земи го.", "Нема проблем.", "Чекај малку!", "Што се случува овде?", "Тие не се тука.",
        "Можеби имаш право.", "Сега разбирам.", "Ќе ти се јавам подоцна.", "Денес е убав ден.",
        "Имам само едно прашање.", "Секогаш си бил добар човек.", "Не го познавам.", "Види, не е така.",
        "Мора да бидеме внимателни.", "Кога ќе дојдеш?", "Во ред, ќе почекам.",
    };

    private static Subtitle Make(IEnumerable<string> lines)
    {
        var sub = new Subtitle();
        var start = 0;
        foreach (var line in lines)
        {
            sub.Paragraphs.Add(new Paragraph(line, start, start + 2000));
            start += 2500;
        }

        return sub;
    }

    private static IEnumerable<string> Sentences(string text)
    {
        return text.Replace("\r", string.Empty).Split(new[] { ". ", ".\n", ": " }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
    }

    [Fact]
    public void MacedonianProseIsMacedonian()
    {
        Assert.Equal("mk", LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(Make(Sentences(Macedonian))));
    }

    [Fact]
    public void SerbianProseIsSerbian()
    {
        Assert.Equal("sr", LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(Make(Sentences(Serbian))));
    }

    [Fact]
    public void BulgarianProseStaysBulgarian()
    {
        Assert.Equal("bg", LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(Make(Sentences(Bulgarian))));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(40)]
    public void MacedonianDialogueOfAnyLengthIsMacedonian(int lines)
    {
        Assert.Equal("mk", LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(Make(MacedonianDialogue.Take(lines))));
    }

    [Fact]
    public void MacedonianWithoutListWordsFallsBackToLettersNotRussian()
    {
        // No word of any Cyrillic list, so the word pass finds nothing and the letter
        // statistics used to answer "ru" for every Cyrillic text.
        Assert.Equal("mk", LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(Make(new[] { "Ѕвездите светат.", "Меѓутоа, пријателот замина." })));
        Assert.Equal("sr", LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(Make(new[] { "Ђаци певају.", "Међутим, пријатељ оде." })));
    }

    [Fact]
    public void FewMisreadLettersDoNotFlipBulgarian()
    {
        // An OCR'ed Bulgarian file with a stray ј and ќ keeps its ъ/й/я majority.
        var lines = Sentences(Bulgarian).ToList();
        lines[0] = lines[0].Replace("Милчо", "Милчо ј ќ");
        Assert.Equal("bg", LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(Make(lines)));
    }
}
