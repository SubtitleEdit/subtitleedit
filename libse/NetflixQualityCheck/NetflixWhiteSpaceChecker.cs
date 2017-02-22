using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixWhiteSpaceChecker : INetflixQualityChecker
    {
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
                if (Regex.Match(p.Text, @"^( |\n|\r\n)[^\s]").Success)
                {
                    AddWhiteSpaceWarning(p, report, 1);
                }

                if (Regex.Match(p.Text, @"[^\s]( |\n|\r\n)$").Success)
                {
                    AddWhiteSpaceWarning(p, report, p.Text.Length);
                }

                // Spaces before punctuation
                foreach (Match m in Regex.Matches(p.Text, @"[^\s]( |\n|\r\n)[!?).,]"))
                {
                    AddWhiteSpaceWarning(p, report, m.Index + 1);
                }

                // 2+ consequent spaces
                foreach (Match m in Regex.Matches(p.Text, "( |\n|\r\n){2,}"))
                {
                    AddWhiteSpaceWarning(p, report, m.Index);
                }
            }
        }
    }
}
