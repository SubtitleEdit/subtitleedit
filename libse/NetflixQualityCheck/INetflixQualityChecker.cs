using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public interface INetflixQualityChecker
    {
        void Check(Subtitle subtitle, NetflixQualityReportBuilder report);
    }
}
