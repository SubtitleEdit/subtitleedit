using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Interfaces
{
    public interface IFixCallbacks : IDoSpell
    {
        bool AllowFix(Paragraph p, string action);
        void AddFixToListView(Paragraph p, string action, string before, string after);
        void LogStatus(string sender, string message);
        void LogStatus(string sender, string message, bool isImportant);
        void UpdateFixStatus(int fixes, string message, string xMessage);
        bool IsName(string candidate);
        HashSet<string> GetAbbreviations();
        void AddToTotalErrors(int count);
        void AddToDeleteIndices(int index);
        SubtitleFormat Format { get; }
        Encoding Encoding { get; }
        string Language { get; }
    }
}
