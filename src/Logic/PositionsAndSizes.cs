using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    class PositionsAndSizes
    {
        List<PositionAndSize> _positionsAndSizes = new List<PositionAndSize>();

        public void SetPositionAndSize(Form form)
        {
            if (form == null)
                return;

            foreach (PositionAndSize ps in _positionsAndSizes)
            {
                if (ps.Name == form.Name)
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Left = ps.Left;
                    form.Top = ps.Top;
                    form.Size = ps.Size;
                    break;
                }
            }
        }

        public void SavePositionAndSize(Form form)
        {
            if (form == null)
                return;

            bool found = false;
            PositionAndSize pAndS = new PositionAndSize();
            foreach (PositionAndSize ps in _positionsAndSizes)
            {
                if (ps.Name == form.Name)
                {
                    found = true;
                    pAndS = ps;
                    break;
                }
            }

            pAndS.Left = form.Left;
            pAndS.Top = form.Top;
            pAndS.Size = form.Size;

            if (!found)
            {
                pAndS.Name = form.Name;
                _positionsAndSizes.Add(pAndS);
            }
        }
    }
}
