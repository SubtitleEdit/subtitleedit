using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class EmptyFixCallback : IFixCallbacks
    {

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

        public void UpdateFixStatus(int fixes, string message, string xMessage)
        {
        }

        public void AddToTotalErrors(int count)
        {
        }

        public void AddToDeleteIndices(int index)
        {
        }

        public SubtitleFormat Format
        {
            get { return new SubRip(); }
        }

        private string _language = "en";

        public string Language
        {
            get { return _language; }
            set { _language = value; }
        }

        public bool IsName(string candidate)
        {
            return false;
        }

        private Encoding _encoding = Encoding.UTF8;

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public HashSet<string> GetAbbreviations()
        {
            return new HashSet<string>();
        }

        public bool DoSpell(string word)
        {
            return false;
        }
    }
}
