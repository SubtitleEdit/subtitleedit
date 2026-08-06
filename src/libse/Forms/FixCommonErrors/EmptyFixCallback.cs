using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
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

        public SubtitleFormat Format => new SubRip();

        public string Language { get; set; } = "en";

        /// <summary>
        /// Names from the <c>Dictionaries/&lt;lang&gt;_names.xml</c> lists, or empty when the
        /// caller does not load them. Backs <see cref="IsName"/> so rules like "Add missing
        /// period" do not treat always-uppercase names (e.g. "Roemenië") as sentence starts
        /// (#12286; case-sensitive, like the GUI names list).
        /// </summary>
        public HashSet<string> Names { get; set; } = new HashSet<string>(StringComparer.Ordinal);

        public bool IsName(string candidate)
        {
            return Names.Contains(candidate);
        }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Abbreviations like "dhr." or "U.S." - empty unless the caller fills it in, e.g. from
        /// <see cref="Dictionaries.AbbreviationList"/> and the names list.
        /// </summary>
        public HashSet<string> Abbreviations { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> GetAbbreviations()
        {
            return Abbreviations;
        }

        public bool DoSpell(string word)
        {
            return false;
        }
    }
}
