using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public class ListViewSorter : System.Collections.IComparer
    {
        public int ColumnNumber { get; set; }
        public bool IsNumber { get; set; }
        public bool IsDisplayFileSize { get; set; }
        public bool Descending { get; set; }

        private static readonly Regex Numbers = new Regex(@"\d+", RegexOptions.Compiled);
        private static readonly Regex InvariantNumber = new Regex(@"\d+\.{1,2}", RegexOptions.Compiled);

        public int Compare(object o1, object o2)
        {
            if (!(o1 is ListViewItem lvi1) || !(o2 is ListViewItem lvi2))
            {
                return 0;
            }

            if (Descending)
            {
                (lvi1, lvi2) = (lvi2, lvi1);
            }

            if (IsNumber)
            {
                var s1 = lvi1.SubItems[ColumnNumber].Text;
                var s2 = lvi2.SubItems[ColumnNumber].Text;

                if (InvariantNumber.IsMatch(s1) && InvariantNumber.IsMatch(s2) &&
                    int.TryParse(s1, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var i1) &&
                    int.TryParse(s2, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var i2))
                {
                    return i1 > i2 ? 1 : i1 == i2 ? 0 : -1;
                }

                if (int.TryParse(s1, out var ii1) &&
                    int.TryParse(s2, out var ii2))
                {
                    return ii1 > ii2 ? 1 : ii1 == ii2 ? 0 : -1;
                }
            }

            if (IsDisplayFileSize)
            {
                var i1 = Utilities.DisplayFileSizeToBytes(lvi1.SubItems[ColumnNumber].Text);
                var i2 = Utilities.DisplayFileSizeToBytes(lvi2.SubItems[ColumnNumber].Text);
                return i1 > i2 ? 1 : i1 == i2 ? 0 : -1;
            }

            return NaturalComparer(lvi2.SubItems[ColumnNumber].Text, lvi1.SubItems[ColumnNumber].Text);
        }

        public static void SetSortArrow(ColumnHeader columnHeader, SortOrder sortOrder)
        {
            const string ascArrow = " ▲";
            const string descArrow = " ▼";

            if (columnHeader.Text.EndsWith(ascArrow) || columnHeader.Text.EndsWith(descArrow))
            {
                columnHeader.Text = columnHeader.Text.Substring(0, columnHeader.Text.Length - 2);
            }

            switch (sortOrder)
            {
                case SortOrder.Ascending:
                    columnHeader.Text += ascArrow;
                    break;
                case SortOrder.Descending:
                    columnHeader.Text += descArrow;
                    break;
            }
        }

        public static int NaturalComparer(string x, string y)
        {
            var str2 = Numbers.Replace(x, m => m.Value.PadLeft(10, '0')).RemoveChar(' ');
            var str1 = Numbers.Replace(y, m => m.Value.PadLeft(10, '0')).RemoveChar(' ');
            return string.Compare(str2, str1, StringComparison.OrdinalIgnoreCase);
        }
    }
}
