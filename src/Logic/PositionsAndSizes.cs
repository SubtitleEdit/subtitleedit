using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public class PositionsAndSizes
    {
        List<PositionAndSize> _positionsAndSizes = new List<PositionAndSize>();

        public void AddPositionAndSize(PositionAndSize pAndS)
        {
            _positionsAndSizes.Add(pAndS);
        }

        public bool SetPositionAndSize(Form form)
        {
            return SetPositionAndSize(form, false);
        }

        public bool SetPositionAndSize(Form form, bool ignoreSize)
        {
            if (form == null)
                return false;

            foreach (PositionAndSize ps in _positionsAndSizes)
            {
                if (ps.Name == form.Name)
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Left = ps.Left;
                    form.Top = ps.Top;
                    if (!ignoreSize)
                        form.Size = ps.Size;
                    return true;
                }
            }
            return false;
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
