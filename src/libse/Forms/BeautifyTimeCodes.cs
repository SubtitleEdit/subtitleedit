using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class BeautifyTimeCodes
    {
        // Utils

        public static double GetInCuesGapMs(double? frameRate = null)
        {
            return (TimeCode.BaseUnit / (frameRate ?? Configuration.Settings.General.CurrentFrameRate)) * Configuration.Settings.BeautifyTimeCodes.Profile.InCuesGap;
        }

        public static double GetOutCuesGapMs(double? frameRate = null)
        {
            return (TimeCode.BaseUnit / (frameRate ?? Configuration.Settings.General.CurrentFrameRate)) * Configuration.Settings.BeautifyTimeCodes.Profile.OutCuesGap;
        }
    }
}
