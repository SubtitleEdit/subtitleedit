using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Cea608
{
    public static class Constants
    {
        public const string ColorWhite = "white";
        public const string ColorGreen = "green";
        public const string ColorBlue = "blue";
        public const string ColorCyan = "cyan";
        public const string ColorRed = "red";
        public const string ColorYellow = "yellow";
        public const string ColorMagenta = "magenta";
        public const string ColorBlack = "black";
        public const string ColorTransparent = "transparent";

        public const string EmptyChar = " ";

        public static string[] PacDataColors = new string[]
        {
            ColorWhite,
            ColorGreen,
            ColorBlue,
            ColorCyan,
            ColorRed,
            ColorYellow,
            ColorMagenta,
            ColorBlack,
            ColorTransparent
        };

        public const int ScreenRowCount = 15;
        public const int ScreenColCount = 32;


        public static Dictionary<int, int> ExtendedCharCodes = new Dictionary<int, int>
        {
            { 0x2a, 0xe1 },  // lowercase a, acute accent
            { 0x5c, 0xe9 },  // lowercase e, acute accent
            { 0x5e, 0xed },  // lowercase i, acute accent
            { 0x5f, 0xf3 },  // lowercase o, acute accent
            { 0x60, 0xfa },  // lowercase u, acute accent
            { 0x7b, 0xe7 },  // lowercase c with cedilla
            { 0x7c, 0xf7 },  // division symbol
            { 0x7d, 0xd1 },  // uppercase N tilde
            { 0x7e, 0xf1 },  // lowercase n tilde
            { 0x7f, 0x2588 }, // Full block
            // THIS BLOCK INCLUDES THE 16 EXTENDED (TWO-BYTE) LINE 21 CHARACTERS
            // THAT COME FROM HI BYTE=0x11 AND LOW BETWEEN 0x30 AND 0x3F
            // THIS MEANS THAT \x50 MUST BE ADDED TO THE VALUES
            { 0x80, 0xae },  // Registered symbol (R)
            { 0x81, 0xb0 },  // degree sign
            { 0x82, 0xbd },  // 1/2 symbol
            { 0x83, 0xbf },  // Inverted (open) question mark
            { 0x84, 0x2122 }, // Trademark symbol (TM)
            { 0x85, 0xa2 },  // Cents symbol
            { 0x86, 0xa3 },  // Pounds sterling
            { 0x87, 0x266a }, // Music 8'th note
            { 0x88, 0xe0 },  // lowercase a, grave accent
            { 0x89, 0x20 },  // transparent space (regular)
            { 0x8a, 0xe8 },  // lowercase e, grave accent
            { 0x8b, 0xe2 },  // lowercase a, circumflex accent
            { 0x8c, 0xea },  // lowercase e, circumflex accent
            { 0x8d, 0xee },  // lowercase i, circumflex accent
            { 0x8e, 0xf4 },  // lowercase o, circumflex accent
            { 0x8f, 0xfb },  // lowercase u, circumflex accent
            // THIS BLOCK INCLUDES THE 32 EXTENDED (TWO-BYTE) LINE 21 CHARACTERS
            // THAT COME FROM HI BYTE=0x12 AND LOW BETWEEN 0x20 AND 0x3F
            { 0x90, 0xc1 },  // capital letter A with acute
            { 0x91, 0xc9 },  // capital letter E with acute
            { 0x92, 0xd3 },  // capital letter O with acute
            { 0x93, 0xda },  // capital letter U with acute
            { 0x94, 0xdc },  // capital letter U with diaresis
            { 0x95, 0xfc },  // lowercase letter U with diaeresis
            { 0x96, 0x2018 }, // opening single quote
            { 0x97, 0xa1 },  // inverted exclamation mark
            { 0x98, 0x2a },  // asterisk
            { 0x99, 0x2019 }, // closing single quote
            { 0x9a, 0x2501 }, // box drawings heavy horizontal
            { 0x9b, 0xa9 },  // copyright sign
            { 0x9c, 0x2120 }, // Service mark
            { 0x9d, 0x2022 }, // (round) bullet
            { 0x9e, 0x201c }, // Left double quotation mark
            { 0x9f, 0x201d }, // Right double quotation mark
            { 0xa0, 0xc0 },  // uppercase A, grave accent
            { 0xa1, 0xc2 },  // uppercase A, circumflex
            { 0xa2, 0xc7 },  // uppercase C with cedilla
            { 0xa3, 0xc8 },  // uppercase E, grave accent
            { 0xa4, 0xca },  // uppercase E, circumflex
            { 0xa5, 0xcb },  // capital letter E with diaresis
            { 0xa6, 0xeb },  // lowercase letter e with diaresis
            { 0xa7, 0xce },  // uppercase I, circumflex
            { 0xa8, 0xcf },  // uppercase I, with diaresis
            { 0xa9, 0xef },  // lowercase i, with diaresis
            { 0xaa, 0xd4 },  // uppercase O, circumflex
            { 0xab, 0xd9 },  // uppercase U, grave accent
            { 0xac, 0xf9 },  // lowercase u, grave accent
            { 0xad, 0xdb },  // uppercase U, circumflex
            { 0xae, 0xab },  // left-pointing double angle quotation mark
            { 0xaf, 0xbb },  // right-pointing double angle quotation mark
            // THIS BLOCK INCLUDES THE 32 EXTENDED (TWO-BYTE) LINE 21 CHARACTERS
            // THAT COME FROM HI BYTE=0x13 AND LOW BETWEEN 0x20 AND 0x3F
            { 0xb0, 0xc3 },  // Uppercase A, tilde
            { 0xb1, 0xe3 },  // Lowercase a, tilde
            { 0xb2, 0xcd },  // Uppercase I, acute accent
            { 0xb3, 0xcc },  // Uppercase I, grave accent
            { 0xb4, 0xec },  // Lowercase i, grave accent
            { 0xb5, 0xd2 },  // Uppercase O, grave accent
            { 0xb6, 0xf2 },  // Lowercase o, grave accent
            { 0xb7, 0xd5 },  // Uppercase O, tilde
            { 0xb8, 0xf5 },  // Lowercase o, tilde
            { 0xb9, 0x7b },  // Open curly brace
            { 0xba, 0x7d },  // Closing curly brace
            { 0xbb, 0x5c },  // Backslash
            { 0xbc, 0x5e },  // Caret
            { 0xbd, 0x5f },  // Underscore
            { 0xbe, 0x7c },  // Pipe (vertical line)
            { 0xbf, 0x223c }, // Tilde operator
            { 0xc0, 0xc4 },  // Uppercase A, umlaut
            { 0xc1, 0xe4 },  // Lowercase A, umlaut
            { 0xc2, 0xd6 },  // Uppercase O, umlaut
            { 0xc3, 0xf6 },  // Lowercase o, umlaut
            { 0xc4, 0xdf },  // Esszett (sharp S)
            { 0xc5, 0xa5 },  // Yen symbol
            { 0xc6, 0xa4 },  // Generic currency sign
            { 0xc7, 0x2503 }, // Box drawings heavy vertical
            { 0xc8, 0xc5 },  // Uppercase A, ring
            { 0xc9, 0xe5 },  // Lowercase A, ring
            { 0xca, 0xd8 },  // Uppercase O, stroke
            { 0xcb, 0xf8 },  // Lowercase o, strok
            { 0xcc, 0x250f }, // Box drawings heavy down and right
            { 0xcd, 0x2513 }, // Box drawings heavy down and left
            { 0xce, 0x2517 }, // Box drawings heavy up and right
            { 0xcf, 0x251b }  // Box drawings heavy up and left
        };

        public static Dictionary<int, int> Channel1RowsMap = new Dictionary<int, int>
        {
            { 0x11, 1 },
            { 0x12, 3 },
            { 0x15, 5 },
            { 0x16, 7 },
            { 0x17, 9 },
            { 0x10, 11 },
            { 0x13, 12 },
            { 0x14, 14 }
        };

        public static Dictionary<int, int> Channel2RowsMap = new Dictionary<int, int>
        {
            { 0x19, 1 },
            { 0x1A, 3 },
            { 0x1D, 5 },
            { 0x1E, 7 },
            { 0x1F, 9 },
            { 0x18, 11 },
            { 0x1B, 12 },
            { 0x1C, 14 }
        };
    }
}
