using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic
{
    public class ListViewSorter : System.Collections.IComparer
    {
        public int ColumnNumber { get; set; }
        public bool IsNumber { get; set; }
        public bool IsDisplayFileSize { get; set; }
        public bool Descending { get; set; }

        public int Compare(object o1, object o2)
        {
            if (!(o1 is ListViewItem lvi1) || !(o2 is ListViewItem lvi2))
            {
                return 0;
            }

            if (Descending)
            {
                var temp = lvi1;
                lvi1 = lvi2;
                lvi2 = temp;
            }

            if (IsNumber)
            {
                if (int.TryParse(lvi1.SubItems[ColumnNumber].Text, out var i1) &&
                    int.TryParse(lvi2.SubItems[ColumnNumber].Text, out var i2))
                {
                    return i1 > i2 ? 1 : i1 == i2 ? 0 : -1;
                }
            }

            if (IsDisplayFileSize)
            {
                var i1 = Utilities.DisplayFileSizeToBytes(lvi1.SubItems[ColumnNumber].Text);
                var i2 = Utilities.DisplayFileSizeToBytes(lvi2.SubItems[ColumnNumber].Text);
                return i1 > i2 ? 1 : i1 == i2 ? 0 : -1;
            }

            return string.Compare(lvi2.SubItems[ColumnNumber].Text, lvi1.SubItems[ColumnNumber].Text, StringComparison.Ordinal);
        }
    }
}
