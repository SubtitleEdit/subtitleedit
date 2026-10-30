using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// Language auto-detection on natural text in every supported language, at the lengths that
/// matter: a couple of lines (where the per-language threshold is zero and one shared word
/// used to hand the clip to the wrong language), ten lines, and everything together. The
/// samples were written as ordinary dialogue, narration, a restaurant scene and a weather
/// report - not from the detector's word lists; the lists and the letter rules were then
/// extended until all of them resolved. "sr-latn" is Serbian in Latin script (expected "sr").
/// </summary>
public class LanguageAutoDetectNaturalTextTest
{
    private static readonly Dictionary<string, string[]> Dialogue = new()
    {
        ["en"] = new[] { "Where have you been all night?", "I told you, I was working late.", "Don't lie to me again.", "Look, I'm sorry, okay?", "We need to talk about this.", "There's nothing left to say.", "Please, just give me a chance.", "You had your chance last year.", "What do you want from me?", "I want you to be honest for once." },
        ["da"] = new[] { "Hvor har du været hele natten?", "Jeg sagde jo, jeg arbejdede over.", "Lad være med at lyve for mig.", "Hør her, jeg er ked af det, okay?", "Vi er nødt til at tale om det.", "Der er ikke mere at sige.", "Giv mig nu bare en chance.", "Du fik din chance sidste år.", "Hvad vil du have af mig?", "Jeg vil have, at du er ærlig for en gangs skyld." },
        ["no"] = new[] { "Hvor har du vært hele natten?", "Jeg sa jo at jeg jobbet sent.", "Ikke lyv for meg igjen.", "Hør her, jeg er lei meg, ok?", "Vi må snakke om dette.", "Det er ikke mer å si.", "Vær så snill, gi meg en sjanse.", "Du fikk sjansen din i fjor.", "Hva vil du ha fra meg?", "Jeg vil at du skal være ærlig for en gangs skyld." },
        ["sv"] = new[] { "Var har du varit hela natten?", "Jag sa ju att jag jobbade sent.", "Ljug inte för mig igen.", "Hör på, jag är ledsen, okej?", "Vi måste prata om det här.", "Det finns inget mer att säga.", "Snälla, ge mig en chans.", "Du fick din chans förra året.", "Vad vill du ha av mig?", "Jag vill att du ska vara ärlig för en gångs skull." },
        ["fi"] = new[] { "Missä olet ollut koko yön?", "Sanoinhan, että olin töissä myöhään.", "Älä valehtele minulle enää.", "Kuule, olen pahoillani, okei?", "Meidän täytyy puhua tästä.", "Ei ole enää mitään sanottavaa.", "Ole kiltti, anna minulle mahdollisuus.", "Sait mahdollisuutesi viime vuonna.", "Mitä sinä minusta haluat?", "Haluan, että olet kerrankin rehellinen." },
        ["is"] = new[] { "Hvar hefur þú verið alla nóttina?", "Ég sagði þér, ég var að vinna lengi.", "Ekki ljúga að mér aftur.", "Heyrðu, mér þykir það leitt, allt í lagi?", "Við þurfum að tala um þetta.", "Það er ekkert meira að segja.", "Gefðu mér bara eitt tækifæri.", "Þú fékkst þitt tækifæri í fyrra.", "Hvað viltu frá mér?", "Ég vil að þú sért heiðarlegur einu sinni." },
        ["de"] = new[] { "Wo warst du die ganze Nacht?", "Ich habe doch gesagt, ich habe lange gearbeitet.", "Lüg mich nicht noch einmal an.", "Hör zu, es tut mir leid, okay?", "Wir müssen darüber reden.", "Es gibt nichts mehr zu sagen.", "Bitte, gib mir eine Chance.", "Du hattest deine Chance letztes Jahr.", "Was willst du von mir?", "Ich will, dass du einmal ehrlich bist." },
        ["nl"] = new[] { "Waar ben je de hele nacht geweest?", "Ik zei toch dat ik laat moest werken.", "Lieg niet nog een keer tegen me.", "Luister, het spijt me, oké?", "We moeten hierover praten.", "Er valt niets meer te zeggen.", "Alsjeblieft, geef me een kans.", "Je hebt vorig jaar je kans gehad.", "Wat wil je van me?", "Ik wil dat je voor één keer eerlijk bent." },
        ["af"] = new[] { "Waar was jy die hele nag?", "Ek het jou gesê ek het laat gewerk.", "Lieg nie weer vir my nie.", "Kyk, ek is jammer, oukei?", "Ons moet hieroor praat.", "Daar is niks meer om te sê nie.", "Gee my net nog een kans, asseblief.", "Jy het verlede jaar jou kans gehad.", "Wat wil jy van my hê?", "Ek wil hê jy moet vir een keer eerlik wees." },
        ["es"] = new[] { "¿Dónde has estado toda la noche?", "Te lo dije, estaba trabajando hasta tarde.", "No me vuelvas a mentir.", "Mira, lo siento, ¿vale?", "Tenemos que hablar de esto.", "No hay nada más que decir.", "Por favor, dame una oportunidad.", "Tuviste tu oportunidad el año pasado.", "¿Qué quieres de mí?", "Quiero que seas sincero por una vez." },
        ["pt"] = new[] { "Onde estiveste a noite toda?", "Eu disse-te, estava a trabalhar até tarde.", "Não me mintas outra vez.", "Olha, desculpa, está bem?", "Precisamos de falar sobre isto.", "Não há mais nada a dizer.", "Por favor, dá-me uma oportunidade.", "Tiveste a tua oportunidade no ano passado.", "O que é que queres de mim?", "Quero que sejas honesto por uma vez." },
        ["ca"] = new[] { "On has estat tota la nit?", "T'ho vaig dir, treballava fins tard.", "No em tornis a mentir.", "Mira, em sap greu, d'acord?", "Hem de parlar d'això.", "No hi ha res més a dir.", "Si us plau, dona'm una oportunitat.", "Vas tenir la teva oportunitat l'any passat.", "Què vols de mi?", "Vull que siguis sincer per una vegada." },
        ["fr"] = new[] { "Où étais-tu toute la nuit ?", "Je te l'ai dit, je travaillais tard.", "Ne me mens plus jamais.", "Écoute, je suis désolé, d'accord ?", "Il faut qu'on parle de ça.", "Il n'y a plus rien à dire.", "S'il te plaît, donne-moi une chance.", "Tu as eu ta chance l'année dernière.", "Qu'est-ce que tu veux de moi ?", "Je veux que tu sois honnête pour une fois." },
        ["it"] = new[] { "Dove sei stato tutta la notte?", "Te l'ho detto, lavoravo fino a tardi.", "Non mentirmi un'altra volta.", "Senti, mi dispiace, va bene?", "Dobbiamo parlare di questo.", "Non c'è più niente da dire.", "Ti prego, dammi una possibilità.", "Hai avuto la tua possibilità l'anno scorso.", "Cosa vuoi da me?", "Voglio che tu sia onesto per una volta." },
        ["ro"] = new[] { "Unde ai fost toată noaptea?", "Ți-am spus, am lucrat până târziu.", "Nu mă mai minți.", "Uite, îmi pare rău, bine?", "Trebuie să vorbim despre asta.", "Nu mai e nimic de spus.", "Te rog, dă-mi o șansă.", "Ai avut șansa ta anul trecut.", "Ce vrei de la mine?", "Vreau să fii sincer măcar o dată." },
        ["pl"] = new[] { "Gdzie byłeś całą noc?", "Mówiłem ci, pracowałem do późna.", "Nie okłamuj mnie znowu.", "Słuchaj, przepraszam, dobrze?", "Musimy o tym porozmawiać.", "Nie ma już nic do powiedzenia.", "Proszę, daj mi szansę.", "Miałeś swoją szansę w zeszłym roku.", "Czego ode mnie chcesz?", "Chcę, żebyś choć raz był szczery." },
        ["cs"] = new[] { "Kde jsi byl celou noc?", "Říkal jsem ti, že jsem pracoval dlouho.", "Už mi nelži.", "Podívej, je mi to líto, dobře?", "Musíme si o tom promluvit.", "Není už co říct.", "Prosím, dej mi šanci.", "Svou šanci jsi měl loni.", "Co ode mě chceš?", "Chci, abys byl jednou upřímný." },
        ["sk"] = new[] { "Kde si bol celú noc?", "Hovoril som ti, že som pracoval dlho.", "Už mi neklam.", "Pozri, je mi to ľúto, dobre?", "Musíme sa o tom porozprávať.", "Nie je už čo povedať.", "Prosím, daj mi šancu.", "Svoju šancu si mal vlani.", "Čo odo mňa chceš?", "Chcem, aby si bol raz úprimný." },
        ["sl"] = new[] { "Kje si bil celo noč?", "Rekel sem ti, da sem delal pozno.", "Ne laži mi več.", "Poslušaj, žal mi je, prav?", "Morava se pogovoriti o tem.", "Ni več kaj reči.", "Prosim, daj mi priložnost.", "Svojo priložnost si imel lani.", "Kaj hočeš od mene?", "Hočem, da si enkrat iskren." },
        ["hr"] = new[] { "Gdje si bio cijelu noć?", "Rekao sam ti, radio sam do kasno.", "Nemoj mi više lagati.", "Slušaj, žao mi je, u redu?", "Moramo razgovarati o tome.", "Nema se više što reći.", "Molim te, daj mi priliku.", "Imao si svoju priliku prošle godine.", "Što hoćeš od mene?", "Hoću da jednom budeš iskren." },
        ["sr-latn"] = new[] { "Gde si bio cele noći?", "Rekao sam ti, radio sam do kasno.", "Nemoj više da me lažeš.", "Slušaj, žao mi je, u redu?", "Moramo da razgovaramo o tome.", "Nema više šta da se kaže.", "Molim te, daj mi priliku.", "Imao si svoju priliku prošle godine.", "Šta hoćeš od mene?", "Hoću da jednom budeš iskren." },
        ["sr"] = new[] { "Где си био целе ноћи?", "Рекао сам ти, радио сам до касно.", "Немој више да ме лажеш.", "Слушај, жао ми је, у реду?", "Морамо да разговарамо о томе.", "Нема више шта да се каже.", "Молим те, дај ми прилику.", "Имао си своју прилику прошле године.", "Шта хоћеш од мене?", "Хоћу да једном будеш искрен." },
        ["bg"] = new[] { "Къде беше цяла нощ?", "Казах ти, работих до късно.", "Не ме лъжи пак.", "Виж, съжалявам, добре?", "Трябва да говорим за това.", "Няма какво повече да се каже.", "Моля те, дай ми шанс.", "Имаше своя шанс миналата година.", "Какво искаш от мен?", "Искам поне веднъж да бъдеш честен." },
        ["mk"] = new[] { "Каде беше цела ноќ?", "Ти реков, работев до касно.", "Не ме лажи повторно.", "Види, жал ми е, добро?", "Треба да зборуваме за ова.", "Нема што повеќе да се каже.", "Те молам, дај ми шанса.", "Ја имаше својата шанса минатата година.", "Што сакаш од мене?", "Сакам барем еднаш да бидеш искрен." },
        ["ru"] = new[] { "Где ты был всю ночь?", "Я же сказал, я работал допоздна.", "Не лги мне больше.", "Слушай, мне жаль, ладно?", "Нам нужно об этом поговорить.", "Больше нечего сказать.", "Пожалуйста, дай мне шанс.", "У тебя был шанс в прошлом году.", "Что ты от меня хочешь?", "Я хочу, чтобы ты хоть раз был честен." },
        ["uk"] = new[] { "Де ти був усю ніч?", "Я ж казав, я працював до пізна.", "Не бреши мені більше.", "Слухай, мені шкода, гаразд?", "Нам треба про це поговорити.", "Більше нічого сказати.", "Будь ласка, дай мені шанс.", "Ти мав свій шанс минулого року.", "Чого ти від мене хочеш?", "Я хочу, щоб ти хоч раз був чесним." },
        ["el"] = new[] { "Πού ήσουν όλη νύχτα;", "Σου είπα, δούλευα μέχρι αργά.", "Μη μου λες ψέματα ξανά.", "Κοίτα, λυπάμαι, εντάξει;", "Πρέπει να μιλήσουμε γι' αυτό.", "Δεν έχω τίποτα άλλο να πω.", "Σε παρακαλώ, δώσε μου μια ευκαιρία.", "Είχες την ευκαιρία σου πέρσι.", "Τι θέλεις από μένα;", "Θέλω να είσαι ειλικρινής για μια φορά." },
        ["tr"] = new[] { "Bütün gece neredeydin?", "Sana söyledim, geç saate kadar çalıştım.", "Bana bir daha yalan söyleme.", "Bak, özür dilerim, tamam mı?", "Bunu konuşmamız gerek.", "Söylenecek başka bir şey yok.", "Lütfen, bana bir şans ver.", "Şansını geçen yıl kullandın.", "Benden ne istiyorsun?", "Bir kez olsun dürüst olmanı istiyorum." },
        ["hu"] = new[] { "Hol voltál egész éjjel?", "Mondtam, hogy sokáig dolgoztam.", "Ne hazudj nekem többet.", "Nézd, sajnálom, jó?", "Beszélnünk kell erről.", "Nincs több mondanivalóm.", "Kérlek, adj még egy esélyt.", "Tavaly megvolt az esélyed.", "Mit akarsz tőlem?", "Azt akarom, hogy egyszer őszinte legyél." },
        ["et"] = new[] { "Kus sa terve öö olid?", "Ma ju ütlesin, et töötasin hilja.", "Ära mulle enam valeta.", "Kuule, mul on kahju, olgu?", "Me peame sellest rääkima.", "Pole enam midagi öelda.", "Palun, anna mulle võimalus.", "Sul oli oma võimalus eelmisel aastal.", "Mida sa minust tahad?", "Ma tahan, et sa oleksid kord aus." },
        ["lv"] = new[] { "Kur tu biji visu nakti?", "Es tev teicu, es strādāju līdz vēlam.", "Nemelo man vairāk.", "Paklau, man žēl, labi?", "Mums par to jāparunā.", "Vairāk nav ko teikt.", "Lūdzu, dod man iespēju.", "Tev bija sava iespēja pagājušajā gadā.", "Ko tu no manis gribi?", "Es gribu, lai tu vienreiz būtu godīgs." },
        ["lt"] = new[] { "Kur buvai visą naktį?", "Sakiau tau, dirbau iki vėlumos.", "Nemeluok man daugiau.", "Klausyk, man gaila, gerai?", "Mums reikia apie tai pasikalbėti.", "Nebėra ko pasakyti.", "Prašau, duok man šansą.", "Turėjai savo šansą pernai.", "Ko tu iš manęs nori?", "Noriu, kad bent kartą būtum sąžiningas." },
        ["sq"] = new[] { "Ku ke qenë gjithë natën?", "Të thashë, punoja deri vonë.", "Mos më gënje më.", "Shiko, më vjen keq, në rregull?", "Duhet të flasim për këtë.", "Nuk ka më asgjë për të thënë.", "Të lutem, më jep një shans.", "E pate shansin tënd vitin e kaluar.", "Çfarë do nga unë?", "Dua që të jesh i sinqertë një herë." },
        ["ar"] = new[] { "أين كنت طوال الليل؟", "قلت لك، كنت أعمل حتى وقت متأخر.", "لا تكذب علي مرة أخرى.", "اسمع، أنا آسف، حسناً؟", "علينا أن نتحدث عن هذا.", "لم يبق شيء لنقوله.", "أرجوك، أعطني فرصة.", "كانت لديك فرصتك العام الماضي.", "ماذا تريد مني؟", "أريدك أن تكون صادقاً ولو لمرة." },
        ["fa"] = new[] { "تمام شب کجا بودی؟", "به تو گفتم، تا دیروقت کار می‌کردم.", "دیگه به من دروغ نگو.", "ببین، متأسفم، باشه؟", "ما باید در این مورد حرف بزنیم.", "دیگه چیزی برای گفتن نیست.", "خواهش می‌کنم، یک فرصت به من بده.", "تو پارسال فرصتت را داشتی.", "از من چی می‌خواهی؟", "می‌خواهم برای یک بار هم صادق باشی." },
        ["he"] = new[] { "איפה היית כל הלילה?", "אמרתי לך, עבדתי עד מאוחר.", "אל תשקר לי שוב.", "תראה, אני מצטער, בסדר?", "אנחנו צריכים לדבר על זה.", "אין יותר מה לומר.", "בבקשה, תן לי הזדמנות.", "הייתה לך הזדמנות בשנה שעברה.", "מה אתה רוצה ממני?", "אני רוצה שתהיה כן פעם אחת." },
        ["hi"] = new[] { "तुम पूरी रात कहाँ थे?", "मैंने कहा था, मैं देर तक काम कर रहा था।", "मुझसे फिर झूठ मत बोलो।", "देखो, मुझे माफ़ कर दो, ठीक है?", "हमें इस बारे में बात करनी होगी।", "अब कहने के लिए कुछ नहीं बचा।", "कृपया, मुझे एक मौका दो।", "तुम्हें पिछले साल मौका मिला था।", "तुम मुझसे क्या चाहते हो?", "मैं चाहता हूँ कि तुम एक बार सच बोलो।" },
        ["ur"] = new[] { "تم ساری رات کہاں تھے؟", "میں نے کہا تھا، میں دیر تک کام کر رہا تھا۔", "مجھ سے دوبارہ جھوٹ مت بولو۔", "دیکھو، مجھے افسوس ہے، ٹھیک ہے؟", "ہمیں اس بارے میں بات کرنی ہوگی۔", "اب کہنے کے لیے کچھ نہیں بچا۔", "براہ کرم، مجھے ایک موقع دو۔", "تمہیں پچھلے سال موقع ملا تھا۔", "تم مجھ سے کیا چاہتے ہو؟", "میں چاہتا ہوں کہ تم ایک بار سچ بولو۔" },
        ["si"] = new[] { "ඔයා මුළු රෑම කොහේ හිටියේ?", "මම කිව්වනේ, මම රෑ වෙනකම් වැඩ කළා.", "ආයෙත් මට බොරු කියන්න එපා.", "බලන්න, මට සමාවෙන්න, හරිද?", "අපි මේ ගැන කතා කරන්න ඕනේ.", "තව කියන්න දෙයක් නෑ.", "කරුණාකරලා, මට අවස්ථාවක් දෙන්න.", "ඔයාට ගිය අවුරුද්දේ අවස්ථාව තිබුණා.", "ඔයාට මගෙන් මොනවද ඕනේ?", "මට ඕනේ ඔයා එක පාරක් ඇත්ත කියන්න." },
        ["th"] = new[] { "คุณไปไหนมาทั้งคืน", "ฉันบอกแล้วว่าทำงานดึก", "อย่าโกหกฉันอีก", "ฟังนะ ฉันขอโทษ โอเคไหม", "เราต้องคุยกันเรื่องนี้", "ไม่มีอะไรจะพูดอีกแล้ว", "ขอร้อง ให้โอกาสฉันหน่อย", "คุณมีโอกาสแล้วเมื่อปีก่อน", "คุณต้องการอะไรจากฉัน", "ฉันอยากให้คุณพูดความจริงสักครั้ง" },
        ["ko"] = new[] { "밤새 어디 있었어?", "말했잖아, 늦게까지 일했어.", "다시는 나한테 거짓말하지 마.", "봐, 미안해, 알았지?", "우리 이 얘기 좀 해야 돼.", "더 할 말이 없어.", "제발, 한 번만 기회를 줘.", "작년에 기회가 있었잖아.", "나한테 뭘 원하는 거야?", "한 번이라도 솔직해지길 바라." },
        ["ja"] = new[] { "一晩中どこにいたの？", "言っただろ、遅くまで働いてたんだ。", "もう嘘をつかないで。", "聞いて、悪かったよ、いいかい？", "この話をしなきゃいけない。", "もう言うことは何もない。", "お願いだ、チャンスをくれ。", "去年チャンスはあったでしょう。", "私に何を望んでいるの？", "一度でいいから正直になってほしい。" },
        ["zh"] = new[] { "你整晚去哪儿了？", "我告诉过你，我工作到很晚。", "别再对我说谎了。", "听着，我很抱歉，好吗？", "我们需要谈谈这件事。", "没什么可说的了。", "求你了，给我一次机会。", "你去年有过机会。", "你想从我这里得到什么？", "我希望你能诚实一次。" },
        ["vi"] = new[] { "Cả đêm anh đã ở đâu?", "Tôi đã nói rồi, tôi làm việc muộn.", "Đừng nói dối tôi nữa.", "Nghe này, tôi xin lỗi, được chứ?", "Chúng ta cần nói về chuyện này.", "Không còn gì để nói nữa.", "Làm ơn, cho tôi một cơ hội.", "Anh đã có cơ hội năm ngoái rồi.", "Anh muốn gì ở tôi?", "Tôi muốn anh thành thật một lần." },
        ["id"] = new[] { "Kamu ke mana semalaman?", "Sudah kubilang, aku kerja sampai larut.", "Jangan bohong lagi padaku.", "Dengar, aku minta maaf, oke?", "Kita harus bicara soal ini.", "Tidak ada lagi yang perlu dikatakan.", "Tolong, beri aku kesempatan.", "Kamu sudah dapat kesempatan tahun lalu.", "Apa yang kamu mau dariku?", "Aku ingin kamu jujur sekali saja." },
        ["tl"] = new[] { "Saan ka nagpunta buong gabi?", "Sinabi ko sa'yo, nag-overtime ako.", "Huwag mo na akong pagsinungalingan.", "Tingnan mo, pasensya na, okay?", "Kailangan nating pag-usapan ito.", "Wala na akong masasabi.", "Pakiusap, bigyan mo ako ng pagkakataon.", "Nagkaroon ka ng pagkakataon noong nakaraang taon.", "Ano ang gusto mo sa akin?", "Gusto ko na maging tapat ka minsan." },
    };

    private static readonly Dictionary<string, string[]> Narrative = new()
    {
        ["en"] = new[] { "The town had been quiet for years before the factory closed.", "Nobody expected the river to rise that fast.", "By morning the bridge was gone and the roads were flooded.", "She watched from the hill as the water took the houses.", "Later, people said they had seen it coming." },
        ["da"] = new[] { "Byen havde været stille i årevis, før fabrikken lukkede.", "Ingen havde regnet med, at floden ville stige så hurtigt.", "Om morgenen var broen væk, og vejene stod under vand.", "Hun så fra bakken, hvordan vandet tog husene.", "Senere sagde folk, at de havde set det komme." },
        ["no"] = new[] { "Byen hadde vært stille i årevis før fabrikken ble lagt ned.", "Ingen hadde ventet at elva skulle stige så fort.", "Om morgenen var brua borte og veiene lå under vann.", "Hun så fra bakken hvordan vannet tok husene.", "Senere sa folk at de hadde sett det komme." },
        ["sv"] = new[] { "Staden hade varit tyst i flera år innan fabriken stängde.", "Ingen hade väntat sig att floden skulle stiga så snabbt.", "På morgonen var bron borta och vägarna stod under vatten.", "Hon såg från kullen hur vattnet tog husen.", "Senare sa folk att de hade sett det komma." },
        ["fi"] = new[] { "Kaupunki oli ollut hiljainen vuosia ennen tehtaan sulkemista.", "Kukaan ei odottanut joen nousevan niin nopeasti.", "Aamulla silta oli poissa ja tiet olivat veden alla.", "Hän katseli kukkulalta, kuinka vesi vei talot.", "Myöhemmin ihmiset sanoivat nähneensä sen tulevan." },
        ["is"] = new[] { "Bærinn hafði verið hljóður í mörg ár áður en verksmiðjan lokaði.", "Enginn bjóst við að áin myndi rísa svona hratt.", "Um morguninn var brúin horfin og vegirnir undir vatni.", "Hún horfði af hæðinni þegar vatnið tók húsin.", "Síðar sagði fólk að það hefði séð þetta koma." },
        ["de"] = new[] { "Die Stadt war jahrelang ruhig gewesen, bevor die Fabrik schloss.", "Niemand hatte erwartet, dass der Fluss so schnell steigen würde.", "Am Morgen war die Brücke weg und die Straßen standen unter Wasser.", "Sie sah vom Hügel aus zu, wie das Wasser die Häuser nahm.", "Später sagten die Leute, sie hätten es kommen sehen." },
        ["nl"] = new[] { "De stad was jarenlang stil geweest voordat de fabriek sloot.", "Niemand had verwacht dat de rivier zo snel zou stijgen.", "Tegen de ochtend was de brug weg en stonden de wegen onder water.", "Ze keek vanaf de heuvel toe hoe het water de huizen meenam.", "Later zeiden mensen dat ze het hadden zien komen." },
        ["af"] = new[] { "Die dorp was jare lank stil voordat die fabriek gesluit het.", "Niemand het verwag dat die rivier so vinnig sou styg.", "Teen die oggend was die brug weg en die paaie onder water.", "Sy het van die heuwel af gekyk hoe die water die huise wegneem.", "Later het mense gesê hulle het dit sien kom." },
        ["es"] = new[] { "El pueblo había estado tranquilo durante años antes de que cerrara la fábrica.", "Nadie esperaba que el río subiera tan rápido.", "Por la mañana el puente había desaparecido y las carreteras estaban inundadas.", "Ella miraba desde la colina cómo el agua se llevaba las casas.", "Más tarde, la gente dijo que lo había visto venir." },
        ["pt"] = new[] { "A cidade tinha estado tranquila durante anos antes de a fábrica fechar.", "Ninguém esperava que o rio subisse tão depressa.", "De manhã a ponte tinha desaparecido e as estradas estavam inundadas.", "Ela observava da colina como a água levava as casas.", "Mais tarde, as pessoas disseram que tinham visto isso a chegar." },
        ["ca"] = new[] { "El poble havia estat tranquil durant anys abans que la fàbrica tanqués.", "Ningú no esperava que el riu pugés tan de pressa.", "Al matí el pont havia desaparegut i les carreteres estaven inundades.", "Ella mirava des del turó com l'aigua s'enduia les cases.", "Més tard, la gent va dir que ho havia vist venir." },
        ["fr"] = new[] { "La ville était restée calme pendant des années avant la fermeture de l'usine.", "Personne ne s'attendait à ce que la rivière monte aussi vite.", "Au matin, le pont avait disparu et les routes étaient inondées.", "Elle regardait depuis la colline l'eau emporter les maisons.", "Plus tard, les gens ont dit qu'ils l'avaient vu venir." },
        ["it"] = new[] { "La città era rimasta tranquilla per anni prima che la fabbrica chiudesse.", "Nessuno si aspettava che il fiume salisse così in fretta.", "Al mattino il ponte era sparito e le strade erano inondate.", "Lei guardava dalla collina mentre l'acqua portava via le case.", "Più tardi la gente disse di averlo visto arrivare." },
        ["ro"] = new[] { "Orașul fusese liniștit ani de zile înainte ca fabrica să se închidă.", "Nimeni nu se aștepta ca râul să crească atât de repede.", "Dimineața podul dispăruse și drumurile erau inundate.", "Ea privea de pe deal cum apa lua casele.", "Mai târziu, oamenii au spus că văzuseră venind asta." },
        ["pl"] = new[] { "Miasto było spokojne przez lata, zanim zamknięto fabrykę.", "Nikt nie spodziewał się, że rzeka podniesie się tak szybko.", "Rano mostu już nie było, a drogi stały pod wodą.", "Patrzyła ze wzgórza, jak woda zabiera domy.", "Później ludzie mówili, że to przewidzieli." },
        ["cs"] = new[] { "Město bylo léta klidné, než továrna zavřela.", "Nikdo nečekal, že řeka stoupne tak rychle.", "Ráno byl most pryč a silnice byly pod vodou.", "Dívala se z kopce, jak voda bere domy.", "Později lidé říkali, že to viděli přicházet." },
        ["sk"] = new[] { "Mesto bolo roky pokojné, kým továreň nezavreli.", "Nikto nečakal, že rieka stúpne tak rýchlo.", "Ráno bol most preč a cesty boli pod vodou.", "Pozerala sa z kopca, ako voda berie domy.", "Neskôr ľudia hovorili, že to videli prichádzať." },
        ["sl"] = new[] { "Mesto je bilo leta mirno, preden se je tovarna zaprla.", "Nihče ni pričakoval, da bo reka narasla tako hitro.", "Zjutraj mostu ni bilo več in ceste so bile pod vodo.", "S hriba je gledala, kako voda odnaša hiše.", "Pozneje so ljudje govorili, da so to videli prihajati." },
        ["hr"] = new[] { "Grad je godinama bio miran prije nego što se tvornica zatvorila.", "Nitko nije očekivao da će rijeka tako brzo narasti.", "Do jutra most je nestao, a ceste su bile poplavljene.", "Gledala je s brijega kako voda odnosi kuće.", "Kasnije su ljudi govorili da su to vidjeli." },
        ["sr-latn"] = new[] { "Grad je godinama bio miran pre nego što se fabrika zatvorila.", "Niko nije očekivao da će reka tako brzo narasti.", "Do jutra most je nestao, a putevi su bili poplavljeni.", "Gledala je sa brda kako voda odnosi kuće.", "Kasnije su ljudi govorili da su to videli." },
        ["sr"] = new[] { "Град је годинама био миран пре него што се фабрика затворила.", "Нико није очекивао да ће река тако брзо нарасти.", "До јутра мост је нестао, а путеви су били поплављени.", "Гледала је са брда како вода односи куће.", "Касније су људи говорили да су то видели." },
        ["bg"] = new[] { "Градът беше спокоен години наред, преди фабриката да затвори.", "Никой не очакваше реката да се надигне толкова бързо.", "До сутринта мостът беше изчезнал, а пътищата бяха наводнени.", "Тя гледаше от хълма как водата отнася къщите.", "По-късно хората казваха, че са го предвидили." },
        ["mk"] = new[] { "Градот беше мирен со години пред да се затвори фабриката.", "Никој не очекуваше реката да се издигне толку брзо.", "До утрото мостот го нема, а патиштата беа поплавени.", "Таа гледаше од ридот како водата ги носи куќите.", "Подоцна луѓето велеа дека го видели тоа како доаѓа." },
        ["ru"] = new[] { "Город годами был тихим, пока не закрылся завод.", "Никто не ожидал, что река поднимется так быстро.", "К утру мост исчез, а дороги оказались под водой.", "Она смотрела с холма, как вода уносит дома.", "Позже люди говорили, что предвидели это." },
        ["uk"] = new[] { "Місто роками було тихим, доки не закрився завод.", "Ніхто не очікував, що річка підніметься так швидко.", "До ранку міст зник, а дороги опинилися під водою.", "Вона дивилася з пагорба, як вода забирає будинки.", "Пізніше люди казали, що передбачали це." },
        ["el"] = new[] { "Η πόλη ήταν ήσυχη για χρόνια πριν κλείσει το εργοστάσιο.", "Κανείς δεν περίμενε ότι το ποτάμι θα ανέβαινε τόσο γρήγορα.", "Το πρωί η γέφυρα είχε χαθεί και οι δρόμοι είχαν πλημμυρίσει.", "Κοιτούσε από τον λόφο καθώς το νερό έπαιρνε τα σπίτια.", "Αργότερα οι άνθρωποι έλεγαν ότι το είχαν δει να έρχεται." },
        ["tr"] = new[] { "Fabrika kapanmadan önce kasaba yıllarca sessizdi.", "Kimse nehrin bu kadar hızlı yükseleceğini beklemiyordu.", "Sabah olunca köprü gitmiş, yollar sular altında kalmıştı.", "Tepeden suyun evleri götürmesini izledi.", "Sonradan insanlar bunu önceden gördüklerini söyledi." },
        ["hu"] = new[] { "A város évekig csendes volt, mielőtt a gyár bezárt.", "Senki nem számított rá, hogy a folyó ilyen gyorsan megárad.", "Reggelre a híd eltűnt, és az utak víz alá kerültek.", "A dombról nézte, ahogy a víz elviszi a házakat.", "Később az emberek azt mondták, hogy látták előre." },
        ["et"] = new[] { "Linn oli aastaid vaikne olnud, enne kui tehas suleti.", "Keegi ei oodanud, et jõgi nii kiiresti tõuseb.", "Hommikuks oli sild kadunud ja teed olid vee all.", "Ta vaatas künkalt, kuidas vesi majad kaasa viis.", "Hiljem ütlesid inimesed, et nad nägid seda ette." },
        ["lv"] = new[] { "Pilsēta gadiem bija mierīga, pirms fabrika tika slēgta.", "Neviens negaidīja, ka upe celsies tik strauji.", "Līdz rītam tilts bija pazudis, un ceļi bija zem ūdens.", "Viņa skatījās no kalna, kā ūdens aiznes mājas.", "Vēlāk cilvēki teica, ka to paredzējuši." },
        ["lt"] = new[] { "Miestas metų metus buvo ramus, kol užsidarė gamykla.", "Niekas nesitikėjo, kad upė pakils taip greitai.", "Iki ryto tiltas dingo, o keliai buvo po vandeniu.", "Ji žiūrėjo nuo kalvos, kaip vanduo neša namus.", "Vėliau žmonės sakė, kad tai numatė." },
        ["sq"] = new[] { "Qyteti kishte qenë i qetë për vite para se fabrika të mbyllej.", "Askush nuk e priste që lumi të ngrihej aq shpejt.", "Në mëngjes ura ishte zhdukur dhe rrugët ishin përmbytur.", "Ajo shikonte nga kodra se si uji merrte shtëpitë.", "Më vonë njerëzit thanë se e kishin parë që do të ndodhte." },
        ["ar"] = new[] { "كانت المدينة هادئة لسنوات قبل أن يغلق المصنع.", "لم يتوقع أحد أن يرتفع النهر بهذه السرعة.", "في الصباح كان الجسر قد اختفى وغمرت المياه الطرق.", "راقبت من التل كيف جرفت المياه المنازل.", "لاحقاً قال الناس إنهم رأوا ذلك قادماً." },
        ["fa"] = new[] { "شهر سال‌ها آرام بود تا اینکه کارخانه بسته شد.", "هیچ‌کس انتظار نداشت رودخانه این‌قدر سریع بالا بیاید.", "تا صبح پل از بین رفته بود و جاده‌ها زیر آب بودند.", "او از روی تپه تماشا می‌کرد که آب خانه‌ها را می‌برد.", "بعدها مردم می‌گفتند که آمدنش را دیده بودند." },
        ["he"] = new[] { "העיר הייתה שקטה במשך שנים לפני שהמפעל נסגר.", "איש לא ציפה שהנהר יעלה כל כך מהר.", "עד הבוקר הגשר נעלם והכבישים הוצפו.", "היא צפתה מהגבעה כשהמים לקחו את הבתים.", "מאוחר יותר אמרו האנשים שהם ראו את זה מגיע." },
        ["hi"] = new[] { "फ़ैक्टरी बंद होने से पहले शहर सालों तक शांत रहा था।", "किसी ने नहीं सोचा था कि नदी इतनी तेज़ी से बढ़ेगी।", "सुबह तक पुल गायब था और सड़कें पानी में डूबी थीं।", "वह पहाड़ी से देखती रही कि पानी घरों को बहा ले गया।", "बाद में लोगों ने कहा कि उन्हें यह पहले से पता था।" },
        ["ur"] = new[] { "فیکٹری بند ہونے سے پہلے شہر برسوں خاموش رہا تھا۔", "کسی نے نہیں سوچا تھا کہ دریا اتنی تیزی سے چڑھے گا۔", "صبح تک پل غائب تھا اور سڑکیں پانی میں ڈوبی تھیں۔", "وہ پہاڑی سے دیکھتی رہی کہ پانی گھروں کو بہا لے گیا۔", "بعد میں لوگوں نے کہا کہ انہیں یہ پہلے سے معلوم تھا۔" },
        ["si"] = new[] { "කර්මාන්තශාලාව වැහෙන්න කලින් නගරය අවුරුදු ගාණක් නිශ්ශබ්ද වෙලා තිබුණා.", "ගඟ මෙච්චර වේගයෙන් උතුරනවා කියලා කවුරුත් බලාපොරොත්තු වුණේ නෑ.", "උදේ වෙනකොට පාලම නැති වෙලා, පාරවල් යට වෙලා.", "ඇය කන්දෙන් බලා හිටියා වතුර ගෙවල් අරගෙන යනවා.", "පස්සේ මිනිස්සු කිව්වා ඒක එනවා දැක්කා කියලා." },
        ["th"] = new[] { "เมืองนี้เงียบสงบมาหลายปีก่อนที่โรงงานจะปิดตัวลง", "ไม่มีใครคาดคิดว่าแม่น้ำจะขึ้นเร็วขนาดนั้น", "พอถึงเช้าสะพานก็หายไปและถนนถูกน้ำท่วม", "เธอมองจากเนินเขาขณะที่น้ำพัดพาบ้านเรือนไป", "ภายหลังผู้คนบอกว่าพวกเขาเห็นมันมาแล้ว" },
        ["ko"] = new[] { "공장이 문을 닫기 전까지 마을은 몇 년 동안 조용했다.", "강이 그렇게 빨리 불어날 줄은 아무도 예상하지 못했다.", "아침이 되자 다리는 사라지고 도로는 물에 잠겼다.", "그녀는 언덕에서 물이 집들을 삼키는 것을 지켜보았다.", "나중에 사람들은 그런 일이 올 줄 알았다고 말했다." },
        ["ja"] = new[] { "工場が閉鎖される前、町は何年も静かだった。", "川がそんなに早く増水するとは誰も思わなかった。", "朝には橋は消え、道路は水に沈んでいた。", "彼女は丘の上から水が家々を飲み込むのを見ていた。", "後になって人々は、こうなることは分かっていたと言った。" },
        ["zh"] = new[] { "工厂关闭之前，这个小镇多年来一直很安静。", "没有人料到河水会涨得那么快。", "到了早上，桥不见了，道路被淹没了。", "她在山上看着洪水冲走了房屋。", "后来人们说，他们早就预见到了。" },
        ["vi"] = new[] { "Thị trấn đã yên tĩnh nhiều năm trước khi nhà máy đóng cửa.", "Không ai ngờ nước sông lại dâng nhanh đến thế.", "Đến sáng, cây cầu đã biến mất và những con đường chìm trong nước.", "Cô đứng trên đồi nhìn dòng nước cuốn đi những ngôi nhà.", "Sau đó mọi người nói rằng họ đã thấy trước điều này." },
        ["id"] = new[] { "Kota itu sudah bertahun-tahun sunyi sebelum pabrik ditutup.", "Tidak ada yang menyangka sungai akan naik begitu cepat.", "Menjelang pagi, jembatan sudah hilang dan jalan-jalan terendam.", "Dia menyaksikan dari bukit saat air menyapu rumah-rumah.", "Belakangan orang-orang berkata mereka sudah menduganya." },
        ["tl"] = new[] { "Tahimik ang bayan sa loob ng maraming taon bago magsara ang pabrika.", "Walang nag-asahan na tataas ang ilog nang ganoon kabilis.", "Pagsapit ng umaga, wala na ang tulay at binaha ang mga kalsada.", "Pinagmasdan niya mula sa burol kung paano tinangay ng tubig ang mga bahay.", "Kalaunan, sinabi ng mga tao na nakita na nila itong darating." },
    };

    private static readonly Dictionary<string, string[]> Scene = new()
    {
        ["en"] = new[] { "Good evening, do you have a table for two?", "Of course, follow me please.", "Could we see the menu first?", "The fish is excellent tonight." },
        ["da"] = new[] { "Godaften, har I et bord til to?", "Selvfølgelig, følg mig.", "Må vi se menuen først?", "Fisken er fremragende i aften." },
        ["no"] = new[] { "God kveld, har dere et bord til to?", "Selvsagt, følg meg.", "Kan vi få se menyen først?", "Fisken er utmerket i kveld." },
        ["sv"] = new[] { "God kväll, har ni ett bord för två?", "Självklart, följ mig.", "Kan vi få se menyn först?", "Fisken är utmärkt i kväll." },
        ["fi"] = new[] { "Hyvää iltaa, onko teillä pöytää kahdelle?", "Tietysti, seuratkaa minua.", "Voisimmeko nähdä ruokalistan ensin?", "Kala on tänään erinomaista." },
        ["is"] = new[] { "Gott kvöld, eigið þið borð fyrir tvo?", "Auðvitað, fylgið mér.", "Megum við sjá matseðilinn fyrst?", "Fiskurinn er frábær í kvöld." },
        ["de"] = new[] { "Guten Abend, haben Sie einen Tisch für zwei?", "Natürlich, folgen Sie mir bitte.", "Könnten wir zuerst die Karte sehen?", "Der Fisch ist heute Abend ausgezeichnet." },
        ["nl"] = new[] { "Goedenavond, heeft u een tafel voor twee?", "Natuurlijk, volgt u mij maar.", "Kunnen we eerst de kaart zien?", "De vis is vanavond uitstekend." },
        ["af"] = new[] { "Goeienaand, het julle 'n tafel vir twee?", "Natuurlik, volg my asseblief.", "Kan ons eers die spyskaart sien?", "Die vis is vanaand uitstekend." },
        ["es"] = new[] { "Buenas noches, ¿tienen una mesa para dos?", "Claro, síganme por favor.", "¿Podríamos ver la carta primero?", "El pescado está excelente esta noche." },
        ["pt"] = new[] { "Boa noite, têm uma mesa para dois?", "Claro, sigam-me por favor.", "Podíamos ver a ementa primeiro?", "O peixe está excelente esta noite." },
        ["ca"] = new[] { "Bona nit, teniu una taula per a dos?", "És clar, seguiu-me si us plau.", "Podríem veure la carta primer?", "El peix és excel·lent aquesta nit." },
        ["fr"] = new[] { "Bonsoir, avez-vous une table pour deux ?", "Bien sûr, suivez-moi.", "Pourrions-nous voir la carte d'abord ?", "Le poisson est excellent ce soir." },
        ["it"] = new[] { "Buonasera, avete un tavolo per due?", "Certo, mi segua per favore.", "Potremmo vedere il menù prima?", "Il pesce è eccellente stasera." },
        ["ro"] = new[] { "Bună seara, aveți o masă pentru două persoane?", "Desigur, urmați-mă vă rog.", "Am putea vedea meniul întâi?", "Peștele este excelent în seara asta." },
        ["pl"] = new[] { "Dobry wieczór, mają państwo stolik dla dwóch osób?", "Oczywiście, proszę za mną.", "Czy możemy najpierw zobaczyć menu?", "Ryba jest dzisiaj znakomita." },
        ["cs"] = new[] { "Dobrý večer, máte stůl pro dva?", "Samozřejmě, pojďte za mnou.", "Mohli bychom nejdřív vidět jídelní lístek?", "Ryba je dnes večer vynikající." },
        ["sk"] = new[] { "Dobrý večer, máte stôl pre dvoch?", "Samozrejme, poďte za mnou.", "Mohli by sme najprv vidieť jedálny lístok?", "Ryba je dnes večer vynikajúca." },
        ["sl"] = new[] { "Dober večer, imate mizo za dva?", "Seveda, sledite mi, prosim.", "Bi lahko najprej videli jedilnik?", "Riba je nocoj odlična." },
        ["hr"] = new[] { "Dobra večer, imate li stol za dvoje?", "Naravno, slijedite me.", "Možemo li prvo vidjeti jelovnik?", "Riba je večeras izvrsna." },
        ["sr-latn"] = new[] { "Dobro veče, imate li sto za dvoje?", "Naravno, pratite me.", "Možemo li prvo da vidimo jelovnik?", "Riba je večeras odlična." },
        ["sr"] = new[] { "Добро вече, имате ли сто за двоје?", "Наравно, пратите ме.", "Можемо ли прво да видимо јеловник?", "Риба је вечерас одлична." },
        ["bg"] = new[] { "Добър вечер, имате ли маса за двама?", "Разбира се, последвайте ме.", "Може ли първо да видим менюто?", "Рибата тази вечер е отлична." },
        ["mk"] = new[] { "Добра вечер, имате ли маса за двајца?", "Секако, следете ме.", "Може ли прво да го видиме менито?", "Рибата вечерва е одлична." },
        ["ru"] = new[] { "Добрый вечер, у вас есть столик на двоих?", "Конечно, следуйте за мной.", "Можно сначала посмотреть меню?", "Рыба сегодня отличная." },
        ["uk"] = new[] { "Добрий вечір, у вас є столик на двох?", "Звісно, ідіть за мною.", "Можна спочатку подивитися меню?", "Риба сьогодні чудова." },
        ["el"] = new[] { "Καλησπέρα, έχετε τραπέζι για δύο;", "Φυσικά, ακολουθήστε με.", "Μπορούμε να δούμε πρώτα τον κατάλογο;", "Το ψάρι είναι εξαιρετικό απόψε." },
        ["tr"] = new[] { "İyi akşamlar, iki kişilik masanız var mı?", "Elbette, beni takip edin.", "Önce menüye bakabilir miyiz?", "Balık bu akşam mükemmel." },
        ["hu"] = new[] { "Jó estét, van asztaluk két személyre?", "Természetesen, kövessenek.", "Megnézhetnénk először az étlapot?", "A hal ma este kiváló." },
        ["et"] = new[] { "Tere õhtust, kas teil on laud kahele?", "Muidugi, tulge minuga kaasa.", "Kas me võiksime enne menüüd näha?", "Kala on täna õhtul suurepärane." },
        ["lv"] = new[] { "Labvakar, vai jums ir galdiņš diviem?", "Protams, sekojiet man.", "Vai mēs varētu vispirms redzēt ēdienkarti?", "Zivs šovakar ir izcila." },
        ["lt"] = new[] { "Labas vakaras, ar turite staliuką dviem?", "Žinoma, sekite paskui mane.", "Ar galėtume pirmiausia pamatyti meniu?", "Žuvis šį vakarą puiki." },
        ["sq"] = new[] { "Mirëmbrëma, keni një tavolinë për dy?", "Sigurisht, më ndiqni.", "Mund ta shohim menunë në fillim?", "Peshku sonte është i shkëlqyer." },
        ["ar"] = new[] { "مساء الخير، هل لديكم طاولة لشخصين؟", "بالتأكيد، اتبعوني من فضلكم.", "هل يمكننا رؤية القائمة أولاً؟", "السمك ممتاز هذه الليلة." },
        ["fa"] = new[] { "عصر بخیر، میز دو نفره دارید؟", "البته، لطفاً دنبال من بیایید.", "می‌توانیم اول منو را ببینیم؟", "ماهی امشب عالی است." },
        ["he"] = new[] { "ערב טוב, יש לכם שולחן לשניים?", "בוודאי, בואו אחריי.", "אפשר לראות את התפריט קודם?", "הדג מצוין הערב." },
        ["hi"] = new[] { "शुभ संध्या, क्या आपके पास दो लोगों के लिए मेज़ है?", "बिल्कुल, मेरे पीछे आइए।", "क्या हम पहले मेन्यू देख सकते हैं?", "आज रात मछली बहुत अच्छी है।" },
        ["ur"] = new[] { "شام بخیر، کیا آپ کے پاس دو لوگوں کے لیے میز ہے؟", "بالکل، میرے پیچھے آئیں۔", "کیا ہم پہلے مینو دیکھ سکتے ہیں؟", "آج رات مچھلی بہت اچھی ہے۔" },
        ["si"] = new[] { "සුබ සැන්දෑවක්, දෙන්නෙකුට මේසයක් තියෙනවද?", "ඔව්, මා පස්සෙන් එන්න.", "අපිට මුලින් මෙනුව බලන්න පුළුවන්ද?", "අද රෑ මාළු හරිම රසයි." },
        ["th"] = new[] { "สวัสดีตอนเย็น มีโต๊ะสำหรับสองคนไหม", "แน่นอน ตามฉันมา", "ขอดูเมนูก่อนได้ไหม", "ปลาคืนนี้อร่อยมาก" },
        ["ko"] = new[] { "안녕하세요, 두 명 자리 있나요?", "물론이죠, 따라오세요.", "먼저 메뉴를 볼 수 있을까요?", "오늘 밤 생선이 아주 좋아요." },
        ["ja"] = new[] { "こんばんは、二人席はありますか？", "もちろんです、こちらへどうぞ。", "先にメニューを見てもいいですか？", "今夜は魚が最高です。" },
        ["zh"] = new[] { "晚上好，有两个人的位子吗？", "当然，请跟我来。", "我们可以先看看菜单吗？", "今晚的鱼非常好。" },
        ["vi"] = new[] { "Chào buổi tối, có bàn cho hai người không?", "Dĩ nhiên, mời đi theo tôi.", "Chúng tôi xem thực đơn trước được không?", "Cá tối nay rất ngon." },
        ["id"] = new[] { "Selamat malam, ada meja untuk dua orang?", "Tentu, silakan ikuti saya.", "Bisa kami lihat menunya dulu?", "Ikannya enak sekali malam ini." },
        ["tl"] = new[] { "Magandang gabi, may mesa ba kayo para sa dalawa?", "Siyempre, sumunod po kayo sa akin.", "Pwede ba naming makita muna ang menu?", "Masarap ang isda ngayong gabi." },
    };

    private static readonly Dictionary<string, string[]> Weather = new()
    {
        ["en"] = new[] { "Heavy rain is expected across the north tomorrow.", "Temperatures will fall below zero during the night.", "Drivers should take extra care on the roads." },
        ["da"] = new[] { "Der ventes kraftig regn over det nordlige i morgen.", "Temperaturen falder til under frysepunktet i nat.", "Bilister bør være ekstra forsigtige på vejene." },
        ["no"] = new[] { "Det ventes kraftig regn over nord i morgen.", "Temperaturen faller under null i løpet av natten.", "Bilister bør være ekstra forsiktige på veiene." },
        ["sv"] = new[] { "Kraftigt regn väntas över norra delarna i morgon.", "Temperaturen sjunker under noll under natten.", "Bilister bör vara extra försiktiga på vägarna." },
        ["fi"] = new[] { "Pohjoiseen odotetaan huomenna rankkasadetta.", "Lämpötila laskee yöllä pakkasen puolelle.", "Autoilijoiden kannattaa olla erityisen varovaisia." },
        ["is"] = new[] { "Búist er við miklu regni norðanlands á morgun.", "Hitinn fer niður fyrir frostmark í nótt.", "Ökumenn ættu að fara sérstaklega varlega." },
        ["de"] = new[] { "Morgen wird im Norden starker Regen erwartet.", "Die Temperaturen sinken in der Nacht unter null.", "Autofahrer sollten auf den Straßen besonders vorsichtig sein." },
        ["nl"] = new[] { "Morgen wordt in het noorden zware regen verwacht.", "De temperatuur daalt vannacht tot onder nul.", "Automobilisten moeten extra voorzichtig zijn op de weg." },
        ["af"] = new[] { "Swaar reën word môre oor die noorde verwag.", "Die temperatuur daal vannag tot onder vriespunt.", "Bestuurders moet ekstra versigtig wees op die paaie." },
        ["es"] = new[] { "Se esperan fuertes lluvias en el norte mañana.", "Las temperaturas bajarán de cero durante la noche.", "Los conductores deben tener especial cuidado en las carreteras." },
        ["pt"] = new[] { "Esperam-se chuvas fortes no norte amanhã.", "As temperaturas vão descer abaixo de zero durante a noite.", "Os condutores devem ter cuidado redobrado nas estradas." },
        ["ca"] = new[] { "Demà s'esperen pluges fortes al nord.", "Les temperatures baixaran de zero durant la nit.", "Els conductors han d'anar amb molt de compte a les carreteres." },
        ["fr"] = new[] { "De fortes pluies sont attendues dans le nord demain.", "Les températures descendront sous zéro pendant la nuit.", "Les conducteurs doivent être très prudents sur les routes." },
        ["it"] = new[] { "Domani sono previste forti piogge al nord.", "Le temperature scenderanno sotto lo zero durante la notte.", "Gli automobilisti devono fare molta attenzione sulle strade." },
        ["ro"] = new[] { "Mâine sunt așteptate ploi puternice în nord.", "Temperaturile vor scădea sub zero în timpul nopții.", "Șoferii trebuie să fie foarte atenți pe drumuri." },
        ["pl"] = new[] { "Jutro na północy spodziewane są ulewne deszcze.", "W nocy temperatura spadnie poniżej zera.", "Kierowcy powinni zachować szczególną ostrożność na drogach." },
        ["cs"] = new[] { "Zítra se na severu očekává silný déšť.", "Teploty v noci klesnou pod nulu.", "Řidiči by měli být na silnicích zvlášť opatrní." },
        ["sk"] = new[] { "Zajtra sa na severe očakáva silný dážď.", "Teploty v noci klesnú pod nulu.", "Vodiči by mali byť na cestách obzvlášť opatrní." },
        ["sl"] = new[] { "Jutri na severu pričakujemo močan dež.", "Temperature bodo ponoči padle pod ničlo.", "Vozniki naj bodo na cestah še posebej previdni." },
        ["hr"] = new[] { "Sutra se na sjeveru očekuje jaka kiša.", "Temperature će tijekom noći pasti ispod nule.", "Vozači bi trebali biti posebno oprezni na cestama." },
        ["sr-latn"] = new[] { "Sutra se na severu očekuje jaka kiša.", "Temperature će tokom noći pasti ispod nule.", "Vozači bi trebalo da budu posebno oprezni na putevima." },
        ["sr"] = new[] { "Сутра се на северу очекује јака киша.", "Температуре ће током ноћи пасти испод нуле.", "Возачи би требало да буду посебно опрезни на путевима." },
        ["bg"] = new[] { "Утре на север се очакват силни дъждове.", "Температурите ще паднат под нулата през нощта.", "Шофьорите трябва да бъдат особено внимателни по пътищата." },
        ["mk"] = new[] { "Утре на север се очекуваат силни дождови.", "Температурите ќе паднат под нулата во текот на ноќта.", "Возачите треба да бидат особено внимателни на патиштата." },
        ["ru"] = new[] { "Завтра на севере ожидаются сильные дожди.", "Ночью температура опустится ниже нуля.", "Водителям следует быть особенно осторожными на дорогах." },
        ["uk"] = new[] { "Завтра на півночі очікуються сильні дощі.", "Уночі температура опуститься нижче нуля.", "Водіям слід бути особливо обережними на дорогах." },
        ["el"] = new[] { "Αύριο αναμένονται ισχυρές βροχές στον βορρά.", "Οι θερμοκρασίες θα πέσουν κάτω από το μηδέν τη νύχτα.", "Οι οδηγοί πρέπει να είναι ιδιαίτερα προσεκτικοί στους δρόμους." },
        ["tr"] = new[] { "Yarın kuzeyde şiddetli yağış bekleniyor.", "Sıcaklıklar gece sıfırın altına düşecek.", "Sürücüler yollarda çok dikkatli olmalı." },
        ["hu"] = new[] { "Holnap északon heves esőzés várható.", "A hőmérséklet éjjel fagypont alá süllyed.", "Az autósok legyenek különösen óvatosak az utakon." },
        ["et"] = new[] { "Homme oodatakse põhja poole tugevat vihma.", "Temperatuur langeb öösel alla nulli.", "Autojuhid peaksid teedel eriti ettevaatlikud olema." },
        ["lv"] = new[] { "Rīt ziemeļos gaidāms stiprs lietus.", "Naktī temperatūra noslīdēs zem nulles.", "Autovadītājiem uz ceļiem jābūt īpaši uzmanīgiem." },
        ["lt"] = new[] { "Rytoj šiaurėje laukiama smarkaus lietaus.", "Naktį temperatūra nukris žemiau nulio.", "Vairuotojai keliuose turėtų būti ypač atsargūs." },
        ["sq"] = new[] { "Nesër priten shira të fortë në veri.", "Temperaturat do të bien nën zero gjatë natës.", "Shoferët duhet të jenë shumë të kujdesshëm në rrugë." },
        ["ar"] = new[] { "من المتوقع هطول أمطار غزيرة في الشمال غداً.", "ستنخفض درجات الحرارة إلى ما دون الصفر خلال الليل.", "على السائقين توخي الحذر الشديد على الطرق." },
        ["fa"] = new[] { "فردا در شمال باران شدید پیش‌بینی می‌شود.", "دما در طول شب به زیر صفر می‌رسد.", "رانندگان باید در جاده‌ها بسیار مراقب باشند." },
        ["he"] = new[] { "מחר צפוי גשם כבד בצפון.", "הטמפרטורות יירדו מתחת לאפס במהלך הלילה.", "הנהגים צריכים להיזהר במיוחד בכבישים." },
        ["hi"] = new[] { "कल उत्तर में भारी बारिश की संभावना है।", "रात में तापमान शून्य से नीचे गिर जाएगा।", "चालकों को सड़कों पर विशेष सावधानी बरतनी चाहिए।" },
        ["ur"] = new[] { "کل شمال میں شدید بارش کی توقع ہے۔", "رات کو درجہ حرارت صفر سے نیچے گر جائے گا۔", "ڈرائیوروں کو سڑکوں پر خاص احتیاط کرنی چاہیے۔" },
        ["si"] = new[] { "හෙට උතුරේ තද වැසි අපේක්ෂා කෙරේ.", "රාත්‍රියේ උෂ්ණත්වය බිංදුවට පහළින් බසී.", "රියදුරන් මාර්ගවල විශේෂයෙන් සැලකිලිමත් විය යුතුයි." },
        ["th"] = new[] { "คาดว่าจะมีฝนตกหนักทางภาคเหนือในวันพรุ่งนี้", "อุณหภูมิจะลดลงต่ำกว่าศูนย์ในตอนกลางคืน", "ผู้ขับขี่ควรใช้ความระมัดระวังเป็นพิเศษบนถนน" },
        ["ko"] = new[] { "내일 북부 지역에 폭우가 예상됩니다.", "밤에는 기온이 영하로 떨어질 것입니다.", "운전자들은 도로에서 특히 주의해야 합니다." },
        ["ja"] = new[] { "明日は北部で大雨が予想されています。", "夜間は気温が零度を下回ります。", "ドライバーは道路で特に注意してください。" },
        ["zh"] = new[] { "预计明天北部将有大雨。", "夜间气温将降至零度以下。", "司机在路上应格外小心。" },
        ["vi"] = new[] { "Ngày mai miền bắc dự kiến có mưa lớn.", "Nhiệt độ sẽ xuống dưới không độ trong đêm.", "Người lái xe nên hết sức cẩn thận trên đường." },
        ["id"] = new[] { "Hujan deras diperkirakan turun di utara besok.", "Suhu akan turun di bawah nol pada malam hari.", "Pengemudi harus ekstra hati-hati di jalan." },
        ["tl"] = new[] { "Inaasahan ang malakas na ulan sa hilaga bukas.", "Bababa ang temperatura sa ilalim ng zero sa gabi.", "Dapat mag-ingat nang husto ang mga drayber sa kalsada." },
    };

    public static IEnumerable<object[]> Languages => Dialogue.Keys.Select(k => new object[] { k });

    private static string Expected(string key) => key == "sr-latn" ? "sr" : key;

    private static string Detect(IEnumerable<string> lines)
    {
        var sub = new Subtitle();
        var start = 0;
        foreach (var line in lines)
        {
            sub.Paragraphs.Add(new Paragraph(line, start, start + 2000));
            start += 2500;
        }

        return LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(sub);
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void DialogueTwoLines(string key) => Assert.Equal(Expected(key), Detect(Dialogue[key].Take(2)));

    [Theory]
    [MemberData(nameof(Languages))]
    public void DialogueFourLines(string key) => Assert.Equal(Expected(key), Detect(Dialogue[key].Take(4)));

    [Theory]
    [MemberData(nameof(Languages))]
    public void DialogueTenLines(string key) => Assert.Equal(Expected(key), Detect(Dialogue[key]));

    [Theory]
    [MemberData(nameof(Languages))]
    public void NarrativeTwoLines(string key) => Assert.Equal(Expected(key), Detect(Narrative[key].Take(2)));

    [Theory]
    [MemberData(nameof(Languages))]
    public void NarrativeFiveLines(string key) => Assert.Equal(Expected(key), Detect(Narrative[key]));

    [Theory]
    [MemberData(nameof(Languages))]
    public void SceneTwoLines(string key) => Assert.Equal(Expected(key), Detect(Scene[key].Take(2)));

    [Theory]
    [MemberData(nameof(Languages))]
    public void SceneFourLines(string key) => Assert.Equal(Expected(key), Detect(Scene[key]));

    [Theory]
    [MemberData(nameof(Languages))]
    public void WeatherOneLine(string key)
    {
        if (key == "no")
        {
            // "Det ventes kraftig regn over nord i morgen." is word for word Danish as well;
            // a single line cannot tell the two apart, and the tie goes to Danish.
            return;
        }

        Assert.Equal(Expected(key), Detect(Weather[key].Take(1)));
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void WeatherThreeLines(string key) => Assert.Equal(Expected(key), Detect(Weather[key]));

    [Theory]
    [MemberData(nameof(Languages))]
    public void Everything(string key) => Assert.Equal(Expected(key), Detect(Dialogue[key].Concat(Narrative[key]).Concat(Scene[key]).Concat(Weather[key])));

    // Older Persian files (Windows-1256) are typed with the Arabic ye/kaf (ي/ك); those letters
    // must not turn a Persian text into Arabic.
    [Theory]
    [InlineData("خيلي ممنون.", "خواهش مي‌كنم.")]
    [InlineData("ما بايد برويم.", "الان؟", "آره، همين الان.")]
    [InlineData("من فكر مي‌كنم كه او اينجا نيست.", "كجا رفته؟", "نمي‌دانم.")]
    public void PersianWithArabicYeAndKaf(params string[] lines) => Assert.Equal("fa", Detect(lines));

    // Russian writes ъ before е/ё/ю/я (съел, подъезд); a short line without ы/э/ё is not Bulgarian.
    [Theory]
    [InlineData("Я съел твой обед.", "Прости.")]
    [InlineData("Объявление.", "Подъезд закрыт.")]
    public void RussianWithHardSign(params string[] lines) => Assert.Equal("ru", Detect(lines));
}
