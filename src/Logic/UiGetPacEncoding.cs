using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;

namespace Nikse.SubtitleEdit.Logic
{
    public class UiGetPacEncoding : Pac.IGetPacEncoding
    {

        public int GetPacEncoding(byte[] previewBuffer, string fileName)
        {
            using (var pacEncoding = new PacEncoding(previewBuffer, fileName))
            {
                if (pacEncoding.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Configuration.Settings.General.LastPacCodePage = pacEncoding.CodePageIndex;
                    return pacEncoding.CodePageIndex;
                }
                return -2;
            }
        }

    }
}
