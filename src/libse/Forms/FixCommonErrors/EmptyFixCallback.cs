using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.Text;

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
            // Empty callback
        }

        public void AddFixToListView(Paragraph p, string action, string before, string after, bool isChecked)
        {
            // Empty callback
        }

        public void LogStatus(string sender, string message)
        {
            // Empty callback
        }

        public void LogStatus(string sender, string message, bool isImportant)
        {
            // Empty callback
        }

        public void UpdateFixStatus(int fixes, string message)
        {
            // Empty callback
        }

        public void AddToTotalErrors(int count)
        {
            // Empty callback
        }

        public void AddToDeleteIndices(int index)
        {
            // Empty callback
        }

        public SubtitleFormat Format
        {
            get { return new SubRip(); }
        }

        public string Language { get; set; } = "en";

        public bool IsName(string candidate)
        {
            return false;
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

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
