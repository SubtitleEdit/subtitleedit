using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Extensions
{
    public static class ListViewExtensions
    {
        public static void CheckAll(this ListView listView) => CheckAll(listView, false);

        public static void CheckAll(this ListView listView, Predicate<ListViewItem> predicate)
        {
            listView.BeginUpdate();
            foreach (ListViewItem item in listView.Items)
            {
                item.Checked = predicate(item);
            }

            listView.EndUpdate();
        }

        public static void CheckAll(this ListView listView, bool select)
        {
            listView.BeginUpdate();
            foreach (ListViewItem item in listView.Items)
            {
                item.Checked = true;
                if (select) item.Selected = true;
            }

            listView.EndUpdate();
        }

        public static void UncheckAll(this ListView listView) => UncheckAll(listView, false);

        public static void UncheckAll(this ListView listView, Predicate<ListViewItem> predicate)
        {
            listView.BeginUpdate();
            foreach (ListViewItem item in listView.Items)
            {
                item.Checked = !predicate(item);
            }

            listView.EndUpdate();
        }

        public static void UncheckAll(this ListView listView, bool unSelect)
        {
            listView.BeginUpdate();
            foreach (ListViewItem item in listView.Items)
            {
                item.Checked = true;
                if (unSelect) item.Selected = false;
            }

            listView.EndUpdate();
        }

        public static void InvertCheck(this ListView listView)
        {
            listView.BeginUpdate();
            foreach (ListViewItem item in listView.Items)
            {
                item.Checked = !item.Checked;
            }

            listView.EndUpdate();
        }
    }
}