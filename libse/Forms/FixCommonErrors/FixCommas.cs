using Nikse.SubtitleEdit.Core.Interfaces;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixCommas : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var commaDouble = new Regex(@"([\p{L}\d\s])(,,)([\p{L}\d\s])");
            var commaTriple = new Regex(@"([\p{L}\d\s])(, *, *,)([\p{L}\d\s])");
            var commaTripleEndOfLine = new Regex(@"([\p{L}\d\s])(, *, *,)$");
            var commaWhiteSpaceBetween = new Regex(@"([\p{L}\d\s])(,\s+,)([\p{L}\d\s])");
            var commaFollowedByLetter = new Regex(@",(\p{L})");

            string fixAction = Configuration.Settings.Language.FixCommonErrors.FixCommas;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (p.Text.IndexOf(',') >= 0 && callbacks.AllowFix(p, fixAction))
                {
                    var s = p.Text;
                    var oldText = s;

                    s = commaDouble.Replace(s, "$1,$3");
                    s = commaTriple.Replace(s, "$1...$3");
                    s = commaTripleEndOfLine.Replace(s, "$1...");
                    s = commaWhiteSpaceBetween.Replace(s, "$1,$3");
                    s = commaFollowedByLetter.Replace(s, ", $1");

                    while (s.Contains(",."))
                    {
                        s = s.Replace(",.", ".");
                    }

                    while (s.Contains(",!"))
                    {
                        s = s.Replace(",!", "!");
                    }

                    while (s.Contains(",?"))
                    {
                        s = s.Replace(",?", "?");
                    }

                    if (p.Text.IndexOf('،') >= 0 && callbacks.Language == "ar")
                    {
                        var commaDoubleAr = new Regex(@"([\p{L}\d\s])(،،)([\p{L}\d\s])");
                        var commaTripleAr = new Regex(@"([\p{L}\d\s])(، *، *،)([\p{L}\d\s])");
                        var commaTripleEndOfLineAr = new Regex(@"([\p{L}\d\s])(، *، *،)$");
                        var commaWhiteSpaceBetweenAr = new Regex(@"([\p{L}\d\s])(،\s+،)([\p{L}\d\s])");
                        var commaFollowedByLetterAr = new Regex(@"،(\p{L})");
                        s = commaDoubleAr.Replace(s, "$1،$3");
                        s = commaTripleAr.Replace(s, "$1...$3");
                        s = commaTripleEndOfLineAr.Replace(s, "$1...");
                        s = commaWhiteSpaceBetweenAr.Replace(s, "$1،$3");
                        s = commaFollowedByLetterAr.Replace(s, "، $1");

                        while (s.Contains("،."))
                        {
                            s = s.Replace("،.", ".");
                        }

                        while (s.Contains("،!"))
                        {
                            s = s.Replace("،!", "!");
                        }

                        while (s.Contains("،?"))
                        {
                            s = s.Replace("،?", "?");
                        }

                    }

                    if (oldText != s)
                    {
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, s);
                        p.Text = s;
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, Configuration.Settings.Language.FixCommonErrors.FixCommas, fixCount.ToString());
        }
    }
}
