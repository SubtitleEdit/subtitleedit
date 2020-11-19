using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Logic
{
    public class RtfTextConverterRichTextBox : IRtfTextConverter
    {
        public string RtfToText(string rtf)
        {
            var rtBox = new System.Windows.Forms.RichTextBox();
            try
            {
                rtBox.Rtf = rtf;
                return rtBox.Text;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("RtfTextConverterRichTextBox.RtfToText: " + exception.Message);
                return string.Empty;
            }
            finally
            {
                rtBox.Dispose();
            }
        }

        public string TextToRtf(string text)
        {
            var rtBox = new System.Windows.Forms.RichTextBox();
            try
            {
                rtBox.Text = text;
                return rtBox.Rtf;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("RtfTextConverterRichTextBox.TextToRtf: " + exception.Message);
                return string.Empty;
            }
            finally
            {
                rtBox.Dispose();
            }
        }

    }
}
