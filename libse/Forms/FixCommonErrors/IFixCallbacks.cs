using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public interface IFixCallbacks
    {
        bool AllowFix(Paragraph p, string action);
        void AddFixToListView(Paragraph p, string action, string before, string after);
        void LogStatus(string sender, string message);
        void LogStatus(string sender, string message, bool isImportant);
        void AddToTotalFixes(int count);
        void AddtoTotalErrors(int count);
        void AddtoDeleteIndices(int index);
        SubtitleFormat Format { get; }
        string Language { get; }
    }
}
