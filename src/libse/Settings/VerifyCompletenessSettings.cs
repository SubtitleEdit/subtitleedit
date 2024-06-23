namespace Nikse.SubtitleEdit.Core.Settings
{
    public class VerifyCompletenessSettings
    {
        public ListSortEnum ListSort { get; set; }

        public enum ListSortEnum : int
        {
            Coverage = 0,
            Time = 1,
        }

        public VerifyCompletenessSettings()
        {
            ListSort = ListSortEnum.Coverage;
        }
    }
}