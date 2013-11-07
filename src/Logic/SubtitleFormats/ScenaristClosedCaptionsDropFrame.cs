using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{

    public class ScenaristClosedCaptionsDropFrame : ScenaristClosedCaptions
    {
        //00:01:00:29   9420 9420 94ae 94ae 94d0 94d0 4920 f761 7320 ...    semi colon (instead of colon) before frame number is used to indicate drop frame
        private const string _timeCodeRegEx = @"^\d+:\d\d:\d\d[;,]\d\d\t";
        protected override Regex RegexTimeCodes { get { return new Regex(_timeCodeRegEx); } }

        public ScenaristClosedCaptionsDropFrame()
        {
            DropFrame = true;
        }

        public override string Name
        {
            get { return "Scenarist Closed Captions Drop Frame"; }
        }
    }
}


