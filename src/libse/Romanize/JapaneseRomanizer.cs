using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Romanize
{
    /// <summary>
    /// Converts Hiragana and Katakana text to a simple Hepburn-style romaji.
    /// This is an approximation intended for subtitle readability.
    /// </summary>
    public class JapaneseRomanizer : IRomanizer
    {
        public const char CharLowerBound1 = '\u3040';
        public const char CharUpperBound1 = '\u30FF';
        public const char CharLowerBound2 = '\u30A0';
        public const char CharUpperBound2 = '\u30FF';

        public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("jp");

        private static readonly Dictionary<string, string> KanaDigraphs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["きゃ"] = "kya",
            ["きゅ"] = "kyu",
            ["きょ"] = "kyo",
            ["しゃ"] = "sha",
            ["しゅ"] = "shu",
            ["しょ"] = "sho",
            ["ちゃ"] = "cha",
            ["ちゅ"] = "chu",
            ["ちょ"] = "cho",
            ["にゃ"] = "nya",
            ["にゅ"] = "nyu",
            ["にょ"] = "nyo",
            ["ひゃ"] = "hya",
            ["ひゅ"] = "hyu",
            ["ひょ"] = "hyo",
            ["みゃ"] = "mya",
            ["みゅ"] = "myu",
            ["みょ"] = "myo",
            ["りゃ"] = "rya",
            ["りゅ"] = "ryu",
            ["りょ"] = "ryo",
            ["ぎゃ"] = "gya",
            ["ぎゅ"] = "gyu",
            ["ぎょ"] = "gyo",
            ["じゃ"] = "ja",
            ["じゅ"] = "ju",
            ["じょ"] = "jo",
            ["びゃ"] = "bya",
            ["びゅ"] = "byu",
            ["びょ"] = "byo",
            ["ぴゃ"] = "pya",
            ["ぴゅ"] = "pyu",
            ["ぴょ"] = "pyo",
            ["キャ"] = "kya",
            ["キュ"] = "kyu",
            ["キョ"] = "kyo",
            ["シャ"] = "sha",
            ["シュ"] = "shu",
            ["ショ"] = "sho",
            ["チャ"] = "cha",
            ["チュ"] = "chu",
            ["チョ"] = "cho",
            ["ニャ"] = "nya",
            ["ニュ"] = "nyu",
            ["ニョ"] = "nyo",
            ["ヒャ"] = "hya",
            ["ヒュ"] = "hyu",
            ["ヒョ"] = "hyo",
            ["ミャ"] = "mya",
            ["ミュ"] = "myu",
            ["ミョ"] = "myo",
            ["リャ"] = "rya",
            ["リュ"] = "ryu",
            ["リョ"] = "ryo",
            ["ギャ"] = "gya",
            ["ギュ"] = "gyu",
            ["ギョ"] = "gyo",
            ["ジャ"] = "ja",
            ["ジュ"] = "ju",
            ["ジョ"] = "jo",
            ["ビャ"] = "bya",
            ["ビュ"] = "byu",
            ["ビョ"] = "byo",
            ["ピャ"] = "pya",
            ["ピュ"] = "pyu",
            ["ピョ"] = "pyo",
        };
        private static readonly Dictionary<char, string> KanaMap = new Dictionary<char, string> 
        {
            ['あ'] = "a",
            ['い'] = "i",
            ['う'] = "u",
            ['え'] = "e",
            ['お'] = "o",
            ['か'] = "ka",
            ['き'] = "ki",
            ['く'] = "ku",
            ['け'] = "ke",
            ['こ'] = "ko",
            ['さ'] = "sa",
            ['し'] = "shi",
            ['す'] = "su",
            ['せ'] = "se",
            ['そ'] = "so",
            ['た'] = "ta",
            ['ち'] = "chi",
            ['つ'] = "tsu",
            ['て'] = "te",
            ['と'] = "to",
            ['な'] = "na",
            ['に'] = "ni",
            ['ぬ'] = "nu",
            ['ね'] = "ne",
            ['の'] = "no",
            ['は'] = "ha",
            ['ひ'] = "hi",
            ['ふ'] = "fu",
            ['へ'] = "he",
            ['ほ'] = "ho",
            ['ま'] = "ma",
            ['み'] = "mi",
            ['む'] = "mu",
            ['め'] = "me",
            ['も'] = "mo",
            ['や'] = "ya",
            ['ゆ'] = "yu",
            ['よ'] = "yo",
            ['ら'] = "ra",
            ['り'] = "ri",
            ['る'] = "ru",
            ['れ'] = "re",
            ['ろ'] = "ro",
            ['わ'] = "wa",
            ['を'] = "o",
            ['ん'] = "n",
            ['が'] = "ga",
            ['ぎ'] = "gi",
            ['ぐ'] = "gu",
            ['げ'] = "ge",
            ['ご'] = "go",
            ['ざ'] = "za",
            ['じ'] = "ji",
            ['ず'] = "zu",
            ['ぜ'] = "ze",
            ['ぞ'] = "zo",
            ['だ'] = "da",
            ['ぢ'] = "ji",
            ['づ'] = "zu",
            ['で'] = "de",
            ['ど'] = "do",
            ['ば'] = "ba",
            ['び'] = "bi",
            ['ぶ'] = "bu",
            ['べ'] = "be",
            ['ぼ'] = "bo",
            ['ぱ'] = "pa",
            ['ぴ'] = "pi",
            ['ぷ'] = "pu",
            ['ぺ'] = "pe",
            ['ぽ'] = "po",
            ['ぁ'] = "a",
            ['ぃ'] = "i",
            ['ぅ'] = "u",
            ['ぇ'] = "e",
            ['ぉ'] = "o",
            ['ゃ'] = "ya",
            ['ゅ'] = "yu",
            ['ょ'] = "yo",
            ['ゎ'] = "wa",
            ['ア'] = "a",
            ['イ'] = "i",
            ['ウ'] = "u",
            ['エ'] = "e",
            ['オ'] = "o",
            ['カ'] = "ka",
            ['キ'] = "ki",
            ['ク'] = "ku",
            ['ケ'] = "ke",
            ['コ'] = "ko",
            ['サ'] = "sa",
            ['シ'] = "shi",
            ['ス'] = "su",
            ['セ'] = "se",
            ['ソ'] = "so",
            ['タ'] = "ta",
            ['チ'] = "chi",
            ['ツ'] = "tsu",
            ['テ'] = "te",
            ['ト'] = "to",
            ['ナ'] = "na",
            ['ニ'] = "ni",
            ['ヌ'] = "nu",
            ['ネ'] = "ne",
            ['ノ'] = "no",
            ['ハ'] = "ha",
            ['ヒ'] = "hi",
            ['フ'] = "fu",
            ['ヘ'] = "he",
            ['ホ'] = "ho",
            ['マ'] = "ma",
            ['ミ'] = "mi",
            ['ム'] = "mu",
            ['メ'] = "me",
            ['モ'] = "mo",
            ['ヤ'] = "ya",
            ['ユ'] = "yu",
            ['ヨ'] = "yo",
            ['ラ'] = "ra",
            ['リ'] = "ri",
            ['ル'] = "ru",
            ['レ'] = "re",
            ['ロ'] = "ro",
            ['ワ'] = "wa",
            ['ヲ'] = "o",
            ['ン'] = "n",
            ['ガ'] = "ga",
            ['ギ'] = "gi",
            ['グ'] = "gu",
            ['ゲ'] = "ge",
            ['ゴ'] = "go",
            ['ザ'] = "za",
            ['ジ'] = "ji",
            ['ズ'] = "zu",
            ['ゼ'] = "ze",
            ['ゾ'] = "zo",
            ['ダ'] = "da",
            ['ヂ'] = "ji",
            ['ヅ'] = "zu",
            ['デ'] = "de",
            ['ド'] = "do",
            ['バ'] = "ba",
            ['ビ'] = "bi",
            ['ブ'] = "bu",
            ['ベ'] = "be",
            ['ボ'] = "bo",
            ['パ'] = "pa",
            ['ピ'] = "pi",
            ['プ'] = "pu",
            ['ペ'] = "pe",
            ['ポ'] = "po",
            ['ァ'] = "a",
            ['ィ'] = "i",
            ['ゥ'] = "u",
            ['ェ'] = "e",
            ['ォ'] = "o",
            ['ャ'] = "ya",
            ['ュ'] = "yu",
            ['ョ'] = "yo",
            ['ヮ'] = "wa",
            ['ヴ'] = "vu",
        };
        private static readonly HashSet<char> Vowels = new HashSet<char> 
        {
            'a', 'e', 'i', 'o', 'u'
        };

        private static void AppendLongVowel(StringBuilder sb)
        {
            for (var i = sb.Length - 1; i >= 0; i--)
            {
                if (Vowels.Contains(sb[i]))
                {
                    sb.Append(sb[i]);
                    return;
                }
            }

            sb.Append('-');
        }
        private static string DuplicateInitialConsonant(string romanized)
        {
            if (string.IsNullOrEmpty(romanized))
            {
                return string.Empty;
            }

            return Vowels.Contains(romanized[0]) ? romanized : romanized[0].ToString();
        }
        private static bool TryMapElement(char ch, out string romanized)
        {
            return KanaMap.TryGetValue(ch, out romanized);
        }

        CultureInfo IRomanizer.Culture { get; } = Culture;

        public bool IsValid(char chr)
        {
            return
                ((chr >= CharLowerBound1) && (chr <= CharUpperBound1)) ||
                ((chr >= CharLowerBound2) && (chr <= CharUpperBound2));
        }
        public bool IsValid(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text.Any(_ => IsValid(_));
        }
        public string Romanize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (ch == 'っ' || ch == 'ッ')
                {
                    if (i + 1 < text.Length && TryMapElement(text[i + 1], out var nextRomanized))
                    {
                        sb.Append(DuplicateInitialConsonant(nextRomanized));
                        continue;
                    }

                    sb.Append("tsu");
                    continue;
                }

                if (ch == 'ー')
                {
                    AppendLongVowel(sb);
                    continue;
                }

                if (i + 1 < text.Length)
                {
                    var digraph = new string(new[] { ch, text[i + 1] });
                    if (KanaDigraphs.TryGetValue(digraph, out var value))
                    {
                        sb.Append(value);
                        i++;
                        continue;
                    }
                }

                if (TryMapElement(ch, out var romanized))
                {
                    sb.Append(romanized);
                    continue;
                }

                sb.Append(ch);
            }

            return sb.ToString();
        }
    }
}
