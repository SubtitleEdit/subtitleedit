using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

public static class NetflixHelper
{
    public static string ConvertNumberToString(string input, bool startWithUppercase, string language)
    {
        var value = input.Trim();

        Dictionary<string, string> dictionary = new Dictionary<string, string>();
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
        else if (language == "de")
        {
            dictionary = new Dictionary<string, string>
            {
                { "0", "null" },
                { "1", "eins" },
                { "2", "zwei" },
                { "3", "drei" },
                { "4", "vier" },
                { "5", "fünf" },
                { "6", "sechs" },
                { "7", "sieben" },
                { "8", "acht" },
                { "9", "neun" },
                { "10", "zehn" },
                { "11", "elf" },
                { "12", "zwölf" },
                { "13", "dreizehn" },
                { "14", "vierzehn" },
                { "15", "fünfzehn" },
                { "16", "sechzehn" },
                { "17", "siebzehn" },
                { "18", "achtzehn" },
                { "19", "neunzehn" },
                { "20", "zwanzig" },
                { "30", "dreißig" },
                { "40", "vierzig" },
                { "50", "fünfzig" },
                { "60", "sechzig" },
                { "70", "siebzig" },
                { "80", "achtzig" },
                { "90", "neunzig" },
                { "100", "einhundert" },
            };
        }
        else if (language == "es")
        {
            dictionary = new Dictionary<string, string>
            {
                { "0", "cero" },
                { "1", "uno" },
                { "2", "dos" },
                { "3", "tres" },
                { "4", "cuatro" },
                { "5", "cinco" },
                { "6", "seis" },
                { "7", "siete" },
                { "8", "ocho" },
                { "9", "nueve" },
                { "10", "diez" },
                { "11", "once" },
                { "12", "doce" },
                { "13", "trece" },
                { "14", "catorce" },
                { "15", "quince" },
                { "16", "dieciséis" },
                { "17", "diecisiete" },
                { "18", "dieciocho" },
                { "19", "diecinueve" },
                { "20", "veinte" },
                { "30", "treinta" },
                { "40", "cuarenta" },
                { "50", "cincuenta" },
                { "60", "sesenta" },
                { "70", "setenta" },
                { "80", "ochenta" },
                { "90", "noventa" },
                { "100", "cien" },
            };
        }
        else if (language == "it")
        {
            dictionary = new Dictionary<string, string>
            {
                { "0", "zero" },
                { "1", "uno" },
                { "2", "due" },
                { "3", "tre" },
                { "4", "quattro" },
                { "5", "cinque" },
                { "6", "sei" },
                { "7", "sette" },
                { "8", "otto" },
                { "9", "nove" },
                { "10", "dieci" },
                { "11", "undici" },
                { "12", "dodici" },
                { "13", "tredici" },
                { "14", "quattordici" },
                { "15", "quindici" },
                { "16", "sedici" },
                { "17", "diciassette" },
                { "18", "diciotto" },
                { "19", "diciannove" },
                { "20", "venti" },
                { "30", "trenta" },
                { "40", "quaranta" },
                { "50", "cinquanta" },
                { "60", "sessanta" },
                { "70", "settanta" },
                { "80", "ottanta" },
                { "90", "novanta" },
                { "100", "cento" },
            };
        }
        else if (language == "fr")
        {
            dictionary = new Dictionary<string, string>
            {
                { "0", "zéro" },
                { "1", "un" },
                { "2", "deux" },
                { "3", "trois" },
                { "4", "quatre" },
                { "5", "cinq" },
                { "6", "six" },
                { "7", "sept" },
                { "8", "huit" },
                { "9", "neuf" },
                { "10", "dix" },
                { "11", "onze" },
                { "12", "douze" },
                { "13", "treize" },
                { "14", "quatorze" },
                { "15", "quinze" },
                { "16", "seize" },
                { "17", "dix-sept" },
                { "18", "dix-huit" },
                { "19", "dix-neuf" },
                { "20", "vingt" },
                { "30", "trente" },
                { "40", "quarante" },
                { "50", "cinquante" },
                { "60", "soixante" },
                { "70", "soixante-dix" },
                { "80", "quatre-vingts" },
                { "90", "quatre-vingt-dix" },
                { "100", "cent" },
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
