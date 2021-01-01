using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class Fix3PlusLines : IFixCommonError
    {
        public static class Language
        {
            public static string Fix3PlusLine { get; set; } = "Fix subtitles with more than two lines";
            public static string Fix3PlusLines { get; set; } = "Fix subtitles with more than two lines";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.Fix3PlusLine;
            int iFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                if (Utilities.GetNumberOfLines(p.Text) > 2 && callbacks.AllowFix(p, fixAction))
                {
                    var savedMaxNumberOfLines = Configuration.Settings.General.MaxNumberOfLines;
                    Configuration.Settings.General.MaxNumberOfLines = 2;
                    var oldText = p.Text;
                    try
                    {
                        p.Text = Utilities.AutoBreakLine(p.Text, callbacks.Language);
                    }
                    finally
                    {
                        Configuration.Settings.General.MaxNumberOfLines = savedMaxNumberOfLines;
                    }

                    if (oldText != p.Text)
                    {
                        iFixes++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(iFixes, Language.Fix3PlusLines);
        }
    }
}
