using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixWhiteSpaceChecker : INetflixQualityChecker
    {
        public void Check(Subtitle subtitle, NetflixQualityReportBuilder report)
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                foreach (Match m in Regex.Matches(p.Text, "([ ]{2,}|(\n|\r\n){2,})"))
                {
                    string timecode = p.StartTime.ToHHMMSSFF();
                    string context = NetflixQualityReportBuilder.StringContext(p.Text, m.Index, 6);
                    string comment = string.Format(Configuration.Settings.Language.NetflixQualityCheck.WhiteSpaceCheckReport, m.Index);

                    report.AddRecord(timecode, context, comment);
                }
            }
        }
    }
}
