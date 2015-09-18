using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class EmptyFixCallback : IFixCallbacks
    {

        private string _language = "en";

        public bool AllowFix(Paragraph p, string action)
        {
            return true;
        }

        public void AddFixToListView(Paragraph p, string action, string before, string after)
        {            
        }

        public void LogStatus(string sender, string message)
        {
        }

        public void LogStatus(string sender, string message, bool isImportant)
        {
        }

        public void AddToTotalFixes(int count)
        {
        }

        public void AddtoTotalErrors(int count)
        {
        }

        public void AddtoDeleteIndices(int index)
        {
        }

        public SubtitleFormat Format
        {
            get { return new SubRip(); }
        }

        public string Language
        {
            get { return _language; }
            set { _language = value; }
        }
    }
}
