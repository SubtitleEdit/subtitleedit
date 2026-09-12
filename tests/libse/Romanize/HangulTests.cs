
using Nikse.SubtitleEdit.Core.Romanize;

namespace LibSETests.Romanize;

/// <summary>
/// Test cases for Revised Romanization of Korean (RR), based on the
/// National Institute of Korean Language standard:
/// https://www.korean.go.kr/front_eng/roman/roman_01.do
/// </summary>
public class HangulTests
{
    public static readonly HangulRomanizer Romanizer = new();
    public static readonly IList<object[]> Data = 
    [
        // 1. Vowels
        [ "아", "a" ],
        [ "어", "eo" ],
        [ "오", "o" ],
        [ "우", "u" ],
        [ "으", "eu" ],
        [ "이", "i" ],
        [ "애", "ae" ],
        [ "에", "e" ],
        [ "외", "oe" ],
        [ "위", "wi" ],
        [ "야", "ya" ],
        [ "여", "yeo" ],
        [ "요", "yo" ],
        [ "유", "yu" ],
        [ "얘", "yae" ],
        [ "예", "ye" ],
        [ "와", "wa" ],
        [ "왜", "wae" ],
        [ "워", "wo" ],
        [ "웨", "we" ],
        [ "의", "ui" ],
        [ "광희문", "Gwanghuimun" ],

        // 2. Consonants - basic values
        [ "가", "ga" ],
        [ "나", "na" ],
        [ "다", "da" ],
        [ "마", "ma" ],
        [ "바", "ba" ],
        [ "사", "sa" ],
        [ "자", "ja" ],
        [ "하", "ha" ],
        [ "까", "kka" ],
        [ "따", "tta" ],
        [ "빠", "ppa" ],
        [ "싸", "ssa" ],
        [ "짜", "jja" ],
        [ "카", "ka" ],
        [ "타", "ta" ],
        [ "파", "pa" ],
        [ "차", "cha" ],
        [ "앙", "ang" ],

        // 3. g/d/b vs k/t/p positional shift
        [ "구미", "Gumi" ],
        [ "영동", "Yeongdong" ],
        [ "백암", "Baegam" ],
        [ "옥천", "Okcheon" ],
        [ "합덕", "Hapdeok" ],
        [ "호법", "Hobeop" ],
        [ "월곶", "Wolgot" ],
        [ "벚꽃", "beotkkot" ],
        [ "한밭", "Hanbat" ],

        // 4. ㄹ -> r / l / ll
        [ "구리", "Guri" ],
        [ "설악", "Seorak" ],
        [ "칠곡", "Chilgok" ],
        [ "임실", "Imsil" ],
        [ "울릉", "Ulleung" ],
        [ "대관령", "Daegwallyeong" ],

        // 5. Assimilation
        [ "백마", "Baengma" ],
        [ "신문로", "Sinmullo" ],
        [ "종로", "Jongno" ],
        [ "왕십리", "Wangsimni" ],
        [ "별내", "Byeollae" ],
        [ "신라", "Silla" ],

        // 6. ㄴ/ㄹ insertion
        [ "학여울", "Hangnyeoul" ],
        [ "알약", "allyak" ],

        // 7. Palatalization
        [ "해돋이", "haedoji" ],
        [ "같이", "gachi" ],
        [ "굳히다", "guchida" ],

        // 8. Aspiration (and noun exception)
        [ "좋고", "joko" ],
        [ "놓다", "nota" ],
        [ "잡혀", "japyeo" ],
        [ "낳지", "nachi" ],
        [ "묵호", "Mukho" ],       // exception: aspiration not reflected
        [ "집현전", "Jipyeonjeon" ], // exception: aspiration not reflected

        // 9. Hyphen for disambiguation
        [ "중앙", "Jung-ang" ],
        [ "반구대", "Ban-gudae" ],
        [ "세운", "Se-un" ],
        [ "해운대", "Hae-undae" ],

        // 10. Proper nouns - capitalization
        [ "서울", "Seoul" ],
        [ "부산", "Busan" ],

        // 11. Personal names (family + given, space-separated)
        [ "민용하", "Min Yongha" ],
        [ "홍길동", "Hong Gildong" ],

        // 12. Administrative units (hyphenated, no sound change before hyphen)
        [ "도봉구", "Dobong-gu" ],
        [ "신창읍", "Sinchang-eup" ],
        [ "삼죽면", "Samjuk-myeon" ],
        [ "인왕리", "Inwang-ri" ],
        [ "당산동", "Dangsan-dong" ],
        [ "종로2가", "Jongno 2(i)-ga" ],
        [ "퇴계로3가", "Toegyero 3(sam)-ga" ],

        // 13. Geographic / cultural / structures (no hyphen)
        [ "남산", "Namsan" ],
        [ "금강", "Geumgang" ],
        [ "독도", "Dokdo" ],
        [ "경복궁", "Gyeongbokgung" ],
        [ "불국사", "Bulguksa" ],
        [ "독립문", "Dongnimmun" ],
        [ "종묘", "Jongmyo" ],
        [ "다보탑", "Dabotap" ],

        // 14. Combined / stress tests
        [ "우주소녀", "ujusonyeo" ],
        [ "러블리즈", "reobeullijeu" ],
        [ "에이핑크", "eipingkeu" ],
        [ "마지막처럼", "majimakcheoreom" ],
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public void Test(string input, string result) 
    {
        string romanized = Romanizer.Romanize(input);

        Assert.Equal(result, romanized);
    }
}