using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public class ListViewSorter : System.Collections.IComparer
    {
        public int Compare(object o1, object o2)
        {
            var lvi1 = o1 as ListViewItem;
            var lvi2 = o2 as ListViewItem;
            if (lvi1 == null || lvi2 == null)
            {
                return 0;
            }

            if (Descending)
            {
                ListViewItem temp = lvi1;
                lvi1 = lvi2;
                lvi2 = temp;
            }

            if (IsNumber)
            {
                var i1 = int.Parse(lvi1.SubItems[ColumnNumber].Text);
                var i2 = int.Parse(lvi2.SubItems[ColumnNumber].Text);
                return (i1 > i2) ? 1 : (i1 == i2 ? 0 : -1);
            }
            return string.Compare(lvi2.SubItems[ColumnNumber].Text, lvi1.SubItems[ColumnNumber].Text, StringComparison.Ordinal);
        }
        public int ColumnNumber { get; set; }
        public bool IsNumber { get; set; }
        public bool Descending { get; set; }
    }
}
