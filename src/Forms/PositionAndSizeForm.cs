using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public /* abstract */ class PositionAndSizeForm : Form
    {
        private static readonly Dictionary<string, Rectangle> _positionsAndSizes = new Dictionary<string, Rectangle>();

        public bool IsPositionAndSizeSaved
        {
            get
            {
                return _positionsAndSizes.ContainsKey(Name);
            }
        }

        public static void SetPositionAndSize(string name, Rectangle bounds)
        {
            if (_positionsAndSizes.ContainsKey(name))
            {
                _positionsAndSizes[name] = bounds;
            }
            else
            {
                _positionsAndSizes.Add(name, bounds);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Rectangle ps;
            if (_positionsAndSizes.TryGetValue(Name, out ps))
            {
                StartPosition = FormStartPosition.Manual;
                Bounds = ps;
            }
            base.OnLoad(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SetPositionAndSize(Name, Bounds);
            base.OnFormClosed(e);
        }
    }
}
