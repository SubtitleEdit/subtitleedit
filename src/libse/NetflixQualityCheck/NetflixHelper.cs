using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public static class NetflixHelper
    {
        public static string ConvertNumberToString(string input, bool startWithUppercase, string language)
        {
            var value = input.Trim();

            Dictionary<string, string> dictionary = null;
            if (language == "en")
            {
                dictionary = new Dictionary<string, string>
                {
                    { "0", "zero" },
                    { "1", "one" },
                    { "2", "two" },
                    { "3", "three" },
                    { "4", "four" },
                    { "5", "five" },
                    { "6", "six" },
                    { "7", "seven" },
                    { "8", "eight" },
                    { "9", "nine" },
                    { "10", "ten" },
                    { "11", "eleven" },
                    { "12", "twelve" },
                    { "13", "thirteen" },
                    { "14", "fourteen" },
                    { "15", "fifteen" },
                    { "16", "sixteen" },
                    { "17", "seventeen" },
                    { "18", "eighteen" },
                    { "19", "nineteen" },
                    { "20", "twenty" },
                    { "30", "thirty" },
                    { "40", "forty" },
                    { "50", "fifty" },
                    { "60", "sixty" },
                    { "70", "seventy" },
                    { "80", "eighty" },
                    { "90", "ninety" },
                    { "100", "one hundred" },
                };
            }
            else if (language == "da")
            {
                dictionary = new Dictionary<string, string>
                {
                    { "0", "nul" },
                    { "1", "en" },
                    { "2", "to" },
                    { "3", "tre" },
                    { "4", "fire" },
                    { "5", "fem" },
                    { "6", "seks" },
                    { "7", "syv" },
                    { "8", "otte" },
                    { "9", "ni" },
                    { "10", "ti" },
                    { "11", "elleve" },
                    { "12", "tolv" },
                    { "13", "tretten" },
                    { "14", "fjorten" },
                    { "15", "femten" },
                    { "16", "seksten" },
                    { "17", "sytten" },
                    { "18", "atten" },
                    { "19", "nitten" },
                    { "20", "tyve" },
                    { "30", "tredieve" },
                    { "40", "fyrre" },
                    { "50", "halvtreds" },
                    { "60", "treds" },
                    { "70", "halvfjerds" },
                    { "80", "firs" },
                    { "90", "halvfems" },
                    { "100", "ethunderede" },
                };
            }
            else if (language == "pt")
            {
                dictionary = new Dictionary<string, string>
                {
                    { "0", "zero" },
                    { "1", "um" },
                    { "2", "dois" },
                    { "3", "três" },
                    { "4", "quatro" },
                    { "5", "cinco" },
                    { "6", "seis" },
                    { "7", "sete" },
                    { "8", "oito" },
                    { "9", "nove" },
                    { "10", "dez" },
                    { "11", "onze" },
                    { "12", "doze" },
                    { "13", "treze" },
                    { "14", "quatorze" },
                    { "15", "quinze" },
                    { "16", "dezesseis" },
                    { "17", "dezessete" },
                    { "18", "dezoito" },
                    { "19", "dezenove" },
                    { "20", "vinte" },
                    { "30", "trinta" },
                    { "40", "quarenta" },
                    { "50", "cinquenta" },
                    { "60", "sessenta" },
                    { "70", "setenta" },
                    { "80", "oitenta" },
                    { "90", "noventa" },
                    { "100", "cem" },
                };
            }

            var post = string.Empty;
            if (value.EndsWith('.') || value.EndsWith(',') || value.EndsWith('?'))
            {
                post = value.Substring(value.Length - 1);
                value = value.Substring(0, value.Length - 1);
            }

            if (dictionary != null && dictionary.TryGetValue(value, out var result))
            {
                value = result;
            }

            value += post;

            if (startWithUppercase && value.Length > 0)
            {
                return value.CapitalizeFirstLetter();
            }

            return value;
        }
    }
}
