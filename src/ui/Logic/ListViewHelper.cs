using Nikse.SubtitleEdit.Controls;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic
{
    public static class ListViewHelper
    {
        public static void SaveListViewState(SubtitleListView subtitleListView, Subtitle subtitle)
        {
            if (!Configuration.Settings.General.ListViewColumnsRememberSize)
            {
                return;
            }

            Configuration.Settings.General.ListViewNumberDisplayIndex = -1;
            Configuration.Settings.General.ListViewStartDisplayIndex = -1;
            Configuration.Settings.General.ListViewEndDisplayIndex = -1;
            Configuration.Settings.General.ListViewDurationDisplayIndex = -1;
            Configuration.Settings.General.ListViewCpsDisplayIndex = -1;
            Configuration.Settings.General.ListViewWpmDisplayIndex = -1;
            Configuration.Settings.General.ListViewGapDisplayIndex = -1;
            Configuration.Settings.General.ListViewActorDisplayIndex = -1;
            Configuration.Settings.General.ListViewRegionDisplayIndex = -1;
            Configuration.Settings.General.ListViewTextDisplayIndex = -1;

            if (subtitleListView.ColumnIndexNumber >= 0)
            {
                var fileHasBookmarkedLines = subtitle != null && subtitle.Paragraphs.Any(p => p.Bookmark != null);
                var columnIndexNumberWidth = subtitleListView.Columns[subtitleListView.ColumnIndexNumber].Width;
                Configuration.Settings.General.ListViewNumberWidth = fileHasBookmarkedLines ? columnIndexNumberWidth - 18 : columnIndexNumberWidth;
                Configuration.Settings.General.ListViewNumberDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexNumber].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexStart >= 0)
            {
                Configuration.Settings.General.ListViewStartWidth = subtitleListView.Columns[subtitleListView.ColumnIndexStart].Width;
                Configuration.Settings.General.ListViewStartDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexStart].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexEnd >= 0)
            {
                Configuration.Settings.General.ListViewEndWidth = subtitleListView.Columns[subtitleListView.ColumnIndexEnd].Width;
                Configuration.Settings.General.ListViewEndDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexEnd].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexDuration >= 0)
            {
                Configuration.Settings.General.ListViewDurationWidth = subtitleListView.Columns[subtitleListView.ColumnIndexDuration].Width;
                Configuration.Settings.General.ListViewDurationDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexDuration].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexCps >= 0)
            {
                Configuration.Settings.General.ListViewCpsWidth = subtitleListView.Columns[subtitleListView.ColumnIndexCps].Width;
                Configuration.Settings.General.ListViewCpsDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexCps].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexWpm >= 0)
            {
                Configuration.Settings.General.ListViewWpmWidth = subtitleListView.Columns[subtitleListView.ColumnIndexWpm].Width;
                Configuration.Settings.General.ListViewWpmDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexWpm].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexGap >= 0)
            {
                Configuration.Settings.General.ListViewGapWidth = subtitleListView.Columns[subtitleListView.ColumnIndexGap].Width;
                Configuration.Settings.General.ListViewGapDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexGap].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexActor >= 0)
            {
                Configuration.Settings.General.ListViewActorWidth = subtitleListView.Columns[subtitleListView.ColumnIndexActor].Width;
                Configuration.Settings.General.ListViewActorDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexActor].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexRegion >= 0)
            {
                Configuration.Settings.General.ListViewRegionWidth = subtitleListView.Columns[subtitleListView.ColumnIndexRegion].Width;
                Configuration.Settings.General.ListViewRegionDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexRegion].DisplayIndex;
            }

            if (subtitleListView.ColumnIndexText >= 0)
            {
                Configuration.Settings.General.ListViewTextWidth = subtitleListView.Columns[subtitleListView.ColumnIndexText].Width;
                Configuration.Settings.General.ListViewTextDisplayIndex = subtitleListView.Columns[subtitleListView.ColumnIndexText].DisplayIndex;
            }
        }

        public static void RestoreListViewDisplayIndices(SubtitleListView subtitleListView)
        {
            if (!Configuration.Settings.General.ListViewColumnsRememberSize)
            {
                return;
            }

            var columnCount = subtitleListView.Columns.Count;

            if (subtitleListView.ColumnIndexNumber >= 0 && 
                Configuration.Settings.General.ListViewNumberDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewNumberDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexNumber].DisplayIndex = Configuration.Settings.General.ListViewNumberDisplayIndex;
            }

            if (subtitleListView.ColumnIndexStart >= 0 && 
                Configuration.Settings.General.ListViewStartDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewStartDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexStart].DisplayIndex = Configuration.Settings.General.ListViewStartDisplayIndex;
            }

            if (subtitleListView.ColumnIndexEnd >= 0 && 
                Configuration.Settings.General.ListViewEndDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewEndDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexEnd].DisplayIndex = Configuration.Settings.General.ListViewEndDisplayIndex;
            }

            if (subtitleListView.ColumnIndexDuration >= 0 && 
                Configuration.Settings.General.ListViewDurationDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewDurationDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexDuration].DisplayIndex = Configuration.Settings.General.ListViewDurationDisplayIndex;
            }

            if (subtitleListView.ColumnIndexCps >= 0 && 
                Configuration.Settings.General.ListViewCpsDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewCpsDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexCps].DisplayIndex = Configuration.Settings.General.ListViewCpsDisplayIndex;
            }

            if (subtitleListView.ColumnIndexWpm >= 0 && 
                Configuration.Settings.General.ListViewWpmDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewWpmDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexWpm].DisplayIndex = Configuration.Settings.General.ListViewWpmDisplayIndex;
            }

            if (subtitleListView.ColumnIndexGap >= 0 && 
                Configuration.Settings.General.ListViewGapDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewGapDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexGap].DisplayIndex = Configuration.Settings.General.ListViewGapDisplayIndex;
            }

            if (subtitleListView.ColumnIndexActor >= 0 && 
                Configuration.Settings.General.ListViewActorDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewActorDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexActor].DisplayIndex = Configuration.Settings.General.ListViewActorDisplayIndex;
            }

            if (subtitleListView.ColumnIndexRegion >= 0 && 
                Configuration.Settings.General.ListViewRegionDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewRegionDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexRegion].DisplayIndex = Configuration.Settings.General.ListViewRegionDisplayIndex;
            }

            if (subtitleListView.ColumnIndexText >= 0 &&
                Configuration.Settings.General.ListViewTextDisplayIndex >= 0 &&
                Configuration.Settings.General.ListViewTextDisplayIndex < columnCount)
            {
                subtitleListView.Columns[subtitleListView.ColumnIndexText].DisplayIndex = Configuration.Settings.General.ListViewTextDisplayIndex;
            }
        }
    }
}
