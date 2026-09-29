using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Rich Text to plain text
    /// </summary>
    /// <remarks>
    /// Translated from Python located at:
    /// http://stackoverflow.com/a/188877/448
    /// to C# by Chris Benard - http://chrisbenard.net/2014/08/20/Extract-Text-from-RTF-in-.Net
    /// </remarks>
    public static class RichTextToPlainText
    {

        public static IRtfTextConverter NativeRtfTextConverter { get; set; }

        private class StackEntry
        {
            public int NumberOfCharactersToSkip { get; private set; }
            public bool Ignorable { get; private set; }

            public StackEntry(int numberOfCharactersToSkip, bool ignorable)
            {
                NumberOfCharactersToSkip = numberOfCharactersToSkip;
                Ignorable = ignorable;
            }
        }

        // The tokenizer below reads what this regex used to match, one token at a time:
        //   \\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)
        // with Singleline | IgnoreCase. MatchCollection made one Match object per character of
        // the document and kept them all alive until the loop ended (a 5 MB RTF took ~7 s just
        // to be rejected by the RTF formats). Case-insensitive [a-z] also matches a few
        // non-ASCII letters depending on the culture (U+0130 and U+212A in most), so those are
        // asked of a regex built with the same options.
        private static readonly Regex NonAsciiRtfLetter = new Regex("^[a-z]$", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // A set, not a list: ConvertToText probes this once per RTF control word, and an RTF
        // document has thousands of them - a linear scan of ~250 entries per word was the bulk
        // of the import cost.
        private static readonly HashSet<string> Destinations = new HashSet<string>(StringComparer.Ordinal)
        {
            "aftncn","aftnsep","aftnsepc","annotation","atnauthor","atndate","atnicn","atnid",
            "atnparent","atnref","atntime","atrfend","atrfstart","author","background",
            "bkmkend","bkmkstart","blipuid","buptim","category","colorschememapping",
            "colortbl","comment","company","creatim","datafield","datastore","defchp","defpap",
            "do","doccomm","docvar","dptxbxtext","ebcend","ebcstart","factoidname","falt",
            "fchars","ffdeftext","ffentrymcr","ffexitmcr","ffformat","ffhelptext","ffl",
            "ffname","ffstattext","field","file","filetbl","fldinst","fldrslt","fldtype",
            "fname","fontemb","fontfile","fonttbl","footer","footerf","footerl","footerr",
            "footnote","formfield","ftncn","ftnsep","ftnsepc","g","generator","gridtbl",
            "header","headerf","headerl","headerr","hl","hlfr","hlinkbase","hlloc","hlsrc",
            "hsv","htmltag","info","keycode","keywords","latentstyles","lchars","levelnumbers",
            "leveltext","lfolevel","linkval","list","listlevel","listname","listoverride",
            "listoverridetable","listpicture","liststylename","listtable","listtext",
            "lsdlockedexcept","macc","maccPr","mailmerge","maln","malnScr","manager","margPr",
            "mbar","mbarPr","mbaseJc","mbegChr","mborderBox","mborderBoxPr","mbox","mboxPr",
            "mchr","mcount","mctrlPr","md","mdeg","mdegHide","mden","mdiff","mdPr","me",
            "mendChr","meqArr","meqArrPr","mf","mfName","mfPr","mfunc","mfuncPr","mgroupChr",
            "mgroupChrPr","mgrow","mhideBot","mhideLeft","mhideRight","mhideTop","mhtmltag",
            "mlim","mlimloc","mlimlow","mlimlowPr","mlimupp","mlimuppPr","mm","mmaddfieldname",
            "mmath","mmathPict","mmathPr","mmaxdist","mmc","mmcJc","mmconnectstr",
            "mmconnectstrdata","mmcPr","mmcs","mmdatasource","mmheadersource","mmmailsubject",
            "mmodso","mmodsofilter","mmodsofldmpdata","mmodsomappedname","mmodsoname",
            "mmodsorecipdata","mmodsosort","mmodsosrc","mmodsotable","mmodsoudl",
            "mmodsoudldata","mmodsouniquetag","mmPr","mmquery","mmr","mnary","mnaryPr",
            "mnoBreak","mnum","mobjDist","moMath","moMathPara","moMathParaPr","mopEmu",
            "mphant","mphantPr","mplcHide","mpos","mr","mrad","mradPr","mrPr","msepChr",
            "mshow","mshp","msPre","msPrePr","msSub","msSubPr","msSubSup","msSubSupPr","msSup",
            "msSupPr","mstrikeBLTR","mstrikeH","mstrikeTLBR","mstrikeV","msub","msubHide",
            "msup","msupHide","mtransp","mtype","mvertJc","mvfmf","mvfml","mvtof","mvtol",
            "mzeroAsc","mzeroDesc","mzeroWid","nesttableprops","nextfile","nonesttables",
            "objalias","objclass","objdata","object","objname","objsect","objtime","oldcprops",
            "oldpprops","oldsprops","oldtprops","oleclsid","operator","panose","password",
            "passwordhash","pgp","pgptbl","picprop","pict","pn","pnseclvl","pntext","pntxta",
            "pntxtb","printim","private","propname","protend","protstart","protusertbl","pxe",
            "result","revtbl","revtim","rsidtbl","rxe","shp","shpgrp","shpinst",
            "shppict","shprslt","shptxt","sn","sp","staticval","stylesheet","subject","sv",
            "svb","tc","template","themedata","title","txe","ud","upr","userprops",
            "wgrffmtfilter","windowcaption","writereservation","writereservhash","xe","xform",
            "xmlattrname","xmlattrvalue","xmlclose","xmlname","xmlnstbl",
            "xmlopen"
        };

        private static readonly Dictionary<string, string> SpecialCharacters = new Dictionary<string, string>
        {
            { "par", "\n" },
            { "sect", "\n\n" },
            { "page", "\n\n" },
            { "line", "\n" },
            { "tab", "\t" },
            { "emdash", "\u2014" },
            { "endash", "\u2013" },
            { "emspace", "\u2003" },
            { "enspace", "\u2002" },
            { "qmspace", "\u2005" },
            { "bullet", "\u2022" },
            { "lquote", "\u2018" },
            { "rquote", "\u2019" },
            { "ldblquote", "\u201C" },
            { "rdblquote", "\u201D" },
        };

        /// <summary>
        /// Strip RTF Tags from RTF Text
        /// </summary>
        /// <param name="inputRtf">RTF formatted text</param>
        /// <returns>Plain text from RTF</returns>
        public static string ConvertToText(string inputRtf)
        {
            if (inputRtf == null)
            {
                return null;
            }

            // use interface converter if available
            if (NativeRtfTextConverter != null)
            {
                return NativeRtfTextConverter.RtfToText(inputRtf);
            }

            // Opening a file asks every format whether it is theirs, and ~20 RTF based formats
            // each convert the same document (often twice: IsMine + LoadSubtitle) - a 1.3 MB RTF
            // took 13 s to open. The conversion is deterministic, so reuse the last result.
            var cached = _lastConversion;
            if (cached != null && string.Equals(cached.Item1, inputRtf, StringComparison.Ordinal))
            {
                return cached.Item2;
            }

            var result = ConvertToTextUncached(inputRtf);
            _lastConversion = Tuple.Create(inputRtf, result);
            return result;
        }

        private static volatile Tuple<string, string> _lastConversion;

        private static string ConvertToTextUncached(string inputRtf)
        {
            var stack = new Stack<StackEntry>();
            bool ignorable = false;              // Whether this group (and all inside it) are "ignorable".
            int ucskip = 1;                      // Number of ASCII characters to skip after a unicode character.
            int curskip = 0;                     // Number of ASCII characters left to skip
            var outText = new StringBuilder(inputRtf.Length); // Output buffer.
            var length = inputRtf.Length;
            var i = 0;
            while (i < length)
            {
                var ch = inputRtf[i];
                if (ch == '\\' && i + 1 < length)
                {
                    var next = inputRtf[i + 1];
                    if (IsRtfLetter(next)) // \foo, with an optional numeric parameter and one optional space
                    {
                        var wordStart = i + 1;
                        var wordEnd = wordStart + 1;
                        while (wordEnd < length && wordEnd - wordStart < 32 && IsRtfLetter(inputRtf[wordEnd]))
                        {
                            wordEnd++;
                        }

                        var end = wordEnd;
                        var parameterEnd = wordEnd;
                        var digitsStart = end < length && inputRtf[end] == '-' ? end + 1 : end;
                        var digitsEnd = digitsStart;
                        while (digitsEnd < length && digitsEnd - digitsStart < 10 && char.IsDigit(inputRtf[digitsEnd]))
                        {
                            digitsEnd++;
                        }

                        if (digitsEnd > digitsStart)
                        {
                            parameterEnd = digitsEnd;
                            end = digitsEnd;
                        }

                        if (end < length && inputRtf[end] == ' ')
                        {
                            end++;
                        }

                        i = end;
                        curskip = 0;
                        var wordValue = inputRtf.Substring(wordStart, wordEnd - wordStart);
                        if (Destinations.Contains(wordValue))
                        {
                            ignorable = true;
                        }
                        else if (ignorable)
                        {
                        }
                        else if (SpecialCharacters.TryGetValue(wordValue, out var special))
                        {
                            outText.Append(special);
                        }
                        else if (wordValue == "uc")
                        {
                            ucskip = int.Parse(inputRtf.Substring(wordEnd, parameterEnd - wordEnd));
                        }
                        else if (wordValue == "u")
                        {
                            int c = int.Parse(inputRtf.Substring(wordEnd, parameterEnd - wordEnd));
                            if (c < 0)
                            {
                                c += 0x10000;
                            }
                            outText.Append(char.ConvertFromUtf32(c));
                            curskip = ucskip;
                        }

                        continue;
                    }

                    if (next == '\'' && i + 3 < length && IsHexDigit(inputRtf[i + 2]) && IsHexDigit(inputRtf[i + 3])) // \'xx
                    {
                        if (curskip > 0)
                        {
                            curskip -= 1;
                        }
                        else if (!ignorable)
                        {
                            int c = (HexDigit(inputRtf[i + 2]) << 4) | HexDigit(inputRtf[i + 3]);
                            outText.Append(char.ConvertFromUtf32(c));
                        }

                        i += 4;
                        continue;
                    }

                    // \x (not a letter)
                    curskip = 0;
                    if (next == '~')
                    {
                        if (!ignorable)
                        {
                            outText.Append('\xA0');
                        }
                    }
                    else if (next == '{' || next == '}' || next == '\\')
                    {
                        if (!ignorable)
                        {
                            outText.Append(next);
                        }
                    }
                    else if (next == '*')
                    {
                        ignorable = true;
                    }

                    i += 2;
                    continue;
                }

                if (ch == '{' || ch == '}')
                {
                    curskip = 0;
                    if (ch == '{')
                    {
                        // Push state
                        stack.Push(new StackEntry(ucskip, ignorable));
                    }
                    else if (stack.Count > 0)
                    {
                        // Pop state
                        StackEntry entry = stack.Pop();
                        ucskip = entry.NumberOfCharactersToSkip;
                        ignorable = entry.Ignorable;
                    }

                    // else: a '}' with no matching '{'. Popping an empty stack threw
                    // InvalidOperationException, and because ~20 readers run their input through
                    // FromRtf(), a single unbalanced brace anywhere aborted format detection for
                    // the whole file instead of reporting an unknown format. Ignore the brace.
                    i++;
                    continue;
                }

                if (ch == '\r' || ch == '\n')
                {
                    // line breaks in the RTF source are not text
                    while (i < length && (inputRtf[i] == '\r' || inputRtf[i] == '\n'))
                    {
                        i++;
                    }

                    continue;
                }

                // any other character (also a lone backslash at the very end) is text
                if (curskip > 0)
                {
                    curskip -= 1;
                }
                else if (!ignorable)
                {
                    outText.Append(ch);
                }

                i++;
            }

            return outText.ToString();
        }

        private static bool IsRtfLetter(char c)
        {
            if (c < 128)
            {
                var lower = c | 0x20;
                return lower >= 'a' && lower <= 'z';
            }

            return NonAsciiRtfLetter.IsMatch(c.ToString());
        }

        private static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        /// <summary>Value of a single hex digit; the regex only ever matches [0-9a-f], either case.</summary>
        private static int HexDigit(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }

            return (c | 0x20) - 'a' + 10;
        }

        public static string ConvertToRtf(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // use interface converter if available
            if (NativeRtfTextConverter != null)
            {
                NativeRtfTextConverter.TextToRtf(value);
            }

            // special RTF chars
            var backslashed = new StringBuilder(value);
            backslashed.Replace(@"\", @"\\");
            backslashed.Replace(@"{", @"\{");
            backslashed.Replace(@"}", @"\}");
            backslashed.Replace(Environment.NewLine, @"\par" + Environment.NewLine);

            // convert string char by char
            var sb = new StringBuilder();
            foreach (char character in backslashed.ToString())
            {
                if (character <= 0x7f)
                {
                    sb.Append(character);
                }
                else
                {
                    sb.Append("\\u" + Convert.ToUInt32(character) + "?");
                }
            }

            return @"{\rtf1\ansi\ansicpg1252\deff0{\fonttbl\f0\fswiss Helvetica;}\f0\pard " + sb + @"\par" + Environment.NewLine + "}";
        }
    }
}
