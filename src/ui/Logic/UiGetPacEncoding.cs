using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public class UiGetPacEncoding : Pac.IGetPacEncoding
    {
        private readonly Form _parent;

        public UiGetPacEncoding(Form parent)
        {
            _parent = parent;
        }

        public int GetPacEncoding(byte[] previewBuffer, string fileName)
        {
            using (var pacEncoding = new PacEncoding(previewBuffer, fileName))
            {
                if (pacEncoding.ShowDialog(_parent) == DialogResult.OK)
                {
                    Configuration.Settings.General.LastPacCodePage = pacEncoding.CodePageIndex;
                    return pacEncoding.CodePageIndex;
                }
                return -2;
            }
        }

    }
}
