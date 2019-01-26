using System;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    internal static class NetflixHelper
    {
        internal static string ConvertNumberToString(string input, bool startWithUppercase, string language)
        {
            var value = input.Trim();
            if (language == "en")
            {
                if (value.Equals("0", StringComparison.Ordinal))
                {
                    value = "zero";
                }

                if (value.Equals("1", StringComparison.Ordinal))
                {
                    value = "one";
                }

                if (value.Equals("2", StringComparison.Ordinal))
                {
                    value = "two";
                }

                if (value.Equals("3", StringComparison.Ordinal))
                {
                    value = "three";
                }

                if (value.Equals("4", StringComparison.Ordinal))
                {
                    value = "four";
                }

                if (value.Equals("5", StringComparison.Ordinal))
                {
                    value = "five";
                }

                if (value.Equals("6", StringComparison.Ordinal))
                {
                    value = "six";
                }

                if (value.Equals("7", StringComparison.Ordinal))
                {
                    value = "seven";
                }

                if (value.Equals("8", StringComparison.Ordinal))
                {
                    value = "eight";
                }

                if (value.Equals("9", StringComparison.Ordinal))
                {
                    value = "nine";
                }

                if (value.Equals("10", StringComparison.Ordinal))
                {
                    value = "ten";
                }

                if (value.Equals("11", StringComparison.Ordinal))
                {
                    value = "eleven";
                }

                if (value.StartsWith("12", StringComparison.Ordinal))
                {
                    value = "twelve";
                }

                if (value.StartsWith("13", StringComparison.Ordinal))
                {
                    value = "thirteen";
                }

                if (value.StartsWith("14", StringComparison.Ordinal))
                {
                    value = "fourteen";
                }

                if (value.StartsWith("15", StringComparison.Ordinal))
                {
                    value = "fifteen";
                }

                if (value.StartsWith("16", StringComparison.Ordinal))
                {
                    value = "sixteen";
                }

                if (value.StartsWith("17", StringComparison.Ordinal))
                {
                    value = "seventeen";
                }

                if (value.StartsWith("18", StringComparison.Ordinal))
                {
                    value = "eighteen";
                }

                if (value.StartsWith("19", StringComparison.Ordinal))
                {
                    value = "nineteen";
                }

                if (value.StartsWith("20", StringComparison.Ordinal))
                {
                    value = "twenty";
                }

                if (value.StartsWith("30", StringComparison.Ordinal))
                {
                    value = "thirty";
                }

                if (value.StartsWith("40", StringComparison.Ordinal))
                {
                    value = "forty";
                }

                if (value.StartsWith("50", StringComparison.Ordinal))
                {
                    value = "fifty";
                }

                if (value.StartsWith("60", StringComparison.Ordinal))
                {
                    value = "sixty";
                }

                if (value.StartsWith("70", StringComparison.Ordinal))
                {
                    value = "seventy";
                }

                if (value.StartsWith("80", StringComparison.Ordinal))
                {
                    value = "eighty";
                }

                if (value.StartsWith("90", StringComparison.Ordinal))
                {
                    value = "ninety";
                }

                if (value.StartsWith("100", StringComparison.Ordinal))
                {
                    value = "one hundred";
                }
            }
            if (language == "da")
            {
                if (value.Equals("0", StringComparison.Ordinal))
                {
                    value = "nul";
                }

                if (value.Equals("1", StringComparison.Ordinal))
                {
                    value = "en";
                }

                if (value.Equals("2", StringComparison.Ordinal))
                {
                    value = "to";
                }

                if (value.Equals("3", StringComparison.Ordinal))
                {
                    value = "tre";
                }

                if (value.Equals("4", StringComparison.Ordinal))
                {
                    value = "fire";
                }

                if (value.Equals("5", StringComparison.Ordinal))
                {
                    value = "fem";
                }

                if (value.Equals("6", StringComparison.Ordinal))
                {
                    value = "seks";
                }

                if (value.Equals("7", StringComparison.Ordinal))
                {
                    value = "syv";
                }

                if (value.Equals("8", StringComparison.Ordinal))
                {
                    value = "otte";
                }

                if (value.Equals("9", StringComparison.Ordinal))
                {
                    value = "ni";
                }

                if (value.Equals("10", StringComparison.Ordinal))
                {
                    value = "ti";
                }

                if (value.Equals("11", StringComparison.Ordinal))
                {
                    value = "elve";
                }

                if (value.StartsWith("12", StringComparison.Ordinal))
                {
                    value = "tolv";
                }

                if (value.StartsWith("13", StringComparison.Ordinal))
                {
                    value = "tretten";
                }

                if (value.StartsWith("14", StringComparison.Ordinal))
                {
                    value = "fjorten";
                }

                if (value.StartsWith("15", StringComparison.Ordinal))
                {
                    value = "femten";
                }

                if (value.StartsWith("16", StringComparison.Ordinal))
                {
                    value = "seksten";
                }

                if (value.StartsWith("17", StringComparison.Ordinal))
                {
                    value = "sytten";
                }

                if (value.StartsWith("18", StringComparison.Ordinal))
                {
                    value = "atten";
                }

                if (value.StartsWith("19", StringComparison.Ordinal))
                {
                    value = "nitten";
                }

                if (value.StartsWith("20", StringComparison.Ordinal))
                {
                    value = "tyve";
                }

                if (value.StartsWith("30", StringComparison.Ordinal))
                {
                    value = "tredieve";
                }

                if (value.StartsWith("40", StringComparison.Ordinal))
                {
                    value = "fyrre";
                }

                if (value.StartsWith("50", StringComparison.Ordinal))
                {
                    value = "halvtreds";
                }

                if (value.StartsWith("60", StringComparison.Ordinal))
                {
                    value = "treds";
                }

                if (value.StartsWith("70", StringComparison.Ordinal))
                {
                    value = "halvfjerds";
                }

                if (value.StartsWith("80", StringComparison.Ordinal))
                {
                    value = "first";
                }

                if (value.StartsWith("90", StringComparison.Ordinal))
                {
                    value = "halvfems";
                }

                if (value.StartsWith("100", StringComparison.Ordinal))
                {
                    value = "ethunderede";
                }
            }
            if (language == "pt")
            {
                if (value.Equals("0", StringComparison.Ordinal))
                {
                    value = "zero";
                }

                if (value.Equals("1", StringComparison.Ordinal))
                {
                    value = "um";
                }

                if (value.Equals("2", StringComparison.Ordinal))
                {
                    value = "dois";
                }

                if (value.Equals("3", StringComparison.Ordinal))
                {
                    value = "três";
                }

                if (value.Equals("4", StringComparison.Ordinal))
                {
                    value = "quatro";
                }

                if (value.Equals("5", StringComparison.Ordinal))
                {
                    value = "cinco";
                }

                if (value.Equals("6", StringComparison.Ordinal))
                {
                    value = "seis";
                }

                if (value.Equals("7", StringComparison.Ordinal))
                {
                    value = "sete";
                }

                if (value.Equals("8", StringComparison.Ordinal))
                {
                    value = "oito";
                }

                if (value.Equals("9", StringComparison.Ordinal))
                {
                    value = "nove";
                }

                if (value.Equals("10", StringComparison.Ordinal))
                {
                    value = "dez";
                }

                if (value.Equals("11", StringComparison.Ordinal))
                {
                    value = "onze";
                }

                if (value.StartsWith("12", StringComparison.Ordinal))
                {
                    value = "doze";
                }

                if (value.StartsWith("13", StringComparison.Ordinal))
                {
                    value = "treze";
                }

                if (value.StartsWith("14", StringComparison.Ordinal))
                {
                    value = "quatorze";
                }

                if (value.StartsWith("15", StringComparison.Ordinal))
                {
                    value = "quinze";
                }

                if (value.StartsWith("16", StringComparison.Ordinal))
                {
                    value = "dezesseis";
                }

                if (value.StartsWith("17", StringComparison.Ordinal))
                {
                    value = "dezessete";
                }

                if (value.StartsWith("18", StringComparison.Ordinal))
                {
                    value = "dezoito";
                }

                if (value.StartsWith("19", StringComparison.Ordinal))
                {
                    value = "dezenove";
                }

                if (value.StartsWith("20", StringComparison.Ordinal))
                {
                    value = "vinte";
                }

                if (value.StartsWith("30", StringComparison.Ordinal))
                {
                    value = "trinta";
                }

                if (value.StartsWith("40", StringComparison.Ordinal))
                {
                    value = "quarenta";
                }

                if (value.StartsWith("50", StringComparison.Ordinal))
                {
                    value = "cinquenta";
                }

                if (value.StartsWith("60", StringComparison.Ordinal))
                {
                    value = "sessenta";
                }

                if (value.StartsWith("70", StringComparison.Ordinal))
                {
                    value = "setenta";
                }

                if (value.StartsWith("80", StringComparison.Ordinal))
                {
                    value = "oitenta";
                }

                if (value.StartsWith("90", StringComparison.Ordinal))
                {
                    value = "noventa";
                }

                if (value.StartsWith("100", StringComparison.Ordinal))
                {
                    value = "cem";
                }
            }
            if (startWithUppercase && value.Length > 0)
            {
                return value.CapitalizeFirstLetter();
            }
            return value;
        }


    }
}
