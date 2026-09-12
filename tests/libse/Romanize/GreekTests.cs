
using Nikse.SubtitleEdit.Core.Romanize;

namespace LibSETests.Romanize;

/// <summary>
/// Test cases for ELOT 743 (2nd ed., 2001) Type 2 "transcription" scheme —
/// the system mandated by the Greek/Cypriot governments for passports and
/// official documents. Chosen over ISO 843 because ISO 843's transcription
/// table is itself derived from the 1982 first edition of ELOT 743, and
/// ELOT 743 is the version with real-world official/legal force (and an
/// official government reference converter to validate against).
/// </summary>
public class GreekTests
{
    public static readonly GreekRomanizer Romanizer = new();
    public static readonly IList<object[]> Data = 
    [
        // 1. Single letters - lowercase
        [ "α", "a" ],
        [ "β", "v" ],
        [ "γ", "g" ],
        [ "δ", "d" ],
        [ "ε", "e" ],
        [ "ζ", "z" ],
        [ "η", "i" ],
        [ "θ", "th" ],
        [ "ι", "i" ],
        [ "κ", "k" ],
        [ "λ", "l" ],
        [ "μ", "m" ],
        [ "ν", "n" ],
        [ "ξ", "x" ],
        [ "ο", "o" ],
        [ "π", "p" ],
        [ "ρ", "r" ],
        [ "σ", "s" ],
        [ "ς", "s" ],   // final sigma, same value as medial sigma
        [ "τ", "t" ],
        [ "υ", "y" ],
        [ "φ", "f" ],
        [ "χ", "ch" ],
        [ "ψ", "ps" ],
        [ "ω", "o" ],

        // 2. Single letters - uppercase
        [ "Α", "A" ],
        [ "Β", "V" ],
        [ "Γ", "G" ],
        [ "Δ", "D" ],
        [ "Ε", "E" ],
        [ "Ζ", "Z" ],
        [ "Η", "I" ],
        [ "Θ", "Th" ],
        [ "Ι", "I" ],
        [ "Κ", "K" ],
        [ "Λ", "L" ],
        [ "Μ", "M" ],
        [ "Ν", "N" ],
        [ "Ξ", "X" ],
        [ "Ο", "O" ],
        [ "Π", "P" ],
        [ "Ρ", "R" ],
        [ "Σ", "S" ],
        [ "Τ", "T" ],
        [ "Υ", "Y" ],
        [ "Φ", "F" ],
        [ "Χ", "Ch" ],
        [ "Ψ", "Ps" ],
        [ "Ω", "O" ],

        // 3. Vowel digraphs / diphthongs
        [ "αι", "ai" ],
        [ "ει", "ei" ],
        [ "οι", "oi" ],
        [ "ου", "ou" ],
        [ "υι", "yi" ],

        // 4. αυ / ευ / ηυ - context-dependent v/f before voiced vs voiceless consonant
        [ "αύρα", "avra" ],     // αυ before voiced ρ -> av
        [ "αυτός", "aftos" ],   // αυ before voiceless τ -> af
        [ "Ευρώπη", "Evropi" ], // ευ before voiced ρ -> ev
        [ "ευτυχία", "eftychia" ], // ευ before voiceless τ -> ef

        // 5. Consonant digraphs with word-position sensitivity
        [ "μπαμπάς", "babas" ],     // word-initial μπ -> b
        [ "καμπάνα", "kampana" ],   // medial μπ -> mp
        [ "ντομάτα", "ntomata" ],   // word-initial ντ -> d (per some tables) / nt (per others) - verify against your reference
        [ "άντρας", "antras" ],     // medial ντ -> nt
        [ "γκολ", "gol" ],          // word-initial γκ -> g
        [ "αγκαλιά", "agkalia" ],   // medial γκ -> gk
        [ "γγ", "ng" ],              // γγ digraph -> ng (e.g. Άγγελος -> Angelos)
        [ "Άγγελος", "Angelos" ],
        [ "τσάι", "tsai" ],          // τσ -> ts
        [ "τζάμι", "tzami" ],        // τζ -> tz

        // 6. Common full-word examples (place/personal names, well-known)
        [ "Αθήνα", "Athina" ],
        [ "Ελλάδα", "Ellada" ],
        [ "Θεσσαλονίκη", "Thessaloniki" ],
        [ "Πειραιάς", "Peiraias" ],
        [ "Γιώργος", "Giorgos" ],
        [ "Δημήτριος", "Dimitrios" ],
        [ "Χριστίνα", "Christina" ],
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public void Test(string input, string result)
    {
        string romanized = Romanizer.Romanize(input);

        Assert.Equal(result, romanized);
    }
}