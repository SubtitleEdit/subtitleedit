using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class ScenaristClosedCaptionsDropFrame : ScenaristClosedCaptions
    {
        //00:01:00:29   9420 9420 94ae 94ae 94d0 94d0 4920 f761 7320 ...    semi colon (instead of colon) before frame number is used to indicate drop frame
        private const string TimeCodeRegEx = @"^\d+:\d\d:\d\d[;,]\d\d\t";
        private static readonly Regex Regex = new Regex(TimeCodeRegEx, RegexOptions.Compiled);
        protected override Regex RegexTimeCodes => Regex;
        public override string Name => "Scenarist Closed Captions Drop Frame";

        public ScenaristClosedCaptionsDropFrame()
        {
            DropFrame = true;
        }
    }
}
