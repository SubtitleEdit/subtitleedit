using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixWhiteSpaceChecker : INetflixQualityChecker
    {
        private static readonly Regex LineEndingSpaceBefore = new Regex(@"^( |\n|\r\n)[^\s]", RegexOptions.Compiled);
        private static readonly Regex LineEndingSpaceAfter = new Regex(@"[^\s]( |\n|\r\n)$", RegexOptions.Compiled);
        private static readonly Regex SpacesBeforePunctuation = new Regex(@"[^\s]( |\n|\r\n)[!?).,]", RegexOptions.Compiled);
        private static readonly Regex TwoPlusConsequentSpaces = new Regex(@"( |\n|\r\n){2,}", RegexOptions.Compiled);

        private void AddWhiteSpaceWarning(Paragraph p, NetflixQualityReportBuilder report, int pos)
        {
            string timecode = p.StartTime.ToHHMMSSFF();
            string context = NetflixQualityReportBuilder.StringContext(p.Text, pos, 6);
            string comment = string.Format(Configuration.Settings.Language.NetflixQualityCheck.WhiteSpaceCheckReport, pos);

            report.AddRecord(timecode, context, comment);
        }

        public void Check(Subtitle subtitle, NetflixQualityReportBuilder report)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // Line endings
                if (LineEndingSpaceBefore.IsMatch(p.Text))
                {
                    AddWhiteSpaceWarning(p, report, 1);
                }

                if (LineEndingSpaceAfter.IsMatch(p.Text))
                {
                    AddWhiteSpaceWarning(p, report, p.Text.Length);
                }

                // Spaces before punctuation
                foreach (Match m in SpacesBeforePunctuation.Matches(p.Text))
                {
                    AddWhiteSpaceWarning(p, report, m.Index + 1);
                }

                // 2+ consequent spaces
                foreach (Match m in TwoPlusConsequentSpaces.Matches(p.Text))
                {
                    AddWhiteSpaceWarning(p, report, m.Index);
                }
            }
        }
    }
}
