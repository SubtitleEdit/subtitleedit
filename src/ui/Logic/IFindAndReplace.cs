namespace Nikse.SubtitleEdit.Logic
{
    public interface IFindAndReplace
    {
        void FindDialogFind(string findText);
        void FindDialogFindPrevious();
        void FindDialogClose();

        void ReplaceDialogFind();
        void ReplaceDialogReplace();
        void ReplaceDialogReplaceAll();
        void ReplaceDialogClose();
    }
}
