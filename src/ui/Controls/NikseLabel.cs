using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Controls
{
    public class NikseLabel : Label
    {
        public NikseLabel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        internal static StringFormat CreateStringFormat(Control control, ContentAlignment contentAlignment, bool showEllipsis)
        {
            var stringFormat = new StringFormat(); //TODO: { Alignment = textAlign};
            if (control.RightToLeft == RightToLeft.Yes)
            {
                stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            }

            if (showEllipsis)
            {
                stringFormat.Trimming = StringTrimming.EllipsisCharacter;
                stringFormat.FormatFlags |= StringFormatFlags.LineLimit;
            }

            stringFormat.HotkeyPrefix = HotkeyPrefix.None;
            if (control.AutoSize)
            {
                stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            }

            return stringFormat;
        }

        internal static TextFormatFlags CreateTextFormatFlags(Control control, ContentAlignment contentAlignment, bool showEllipsis, bool useMnemonic)
        {
            var textFormatFlags = TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak; //TODO: use alignment
            if (showEllipsis)
            {
                textFormatFlags |= TextFormatFlags.EndEllipsis;
            }

            if (control.RightToLeft == RightToLeft.Yes)
            {
                textFormatFlags |= TextFormatFlags.RightToLeft;
            }

            textFormatFlags |= !useMnemonic ? TextFormatFlags.NoPrefix : TextFormatFlags.HidePrefix;

            return textFormatFlags;
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            if (Enabled || !Configuration.Settings.General.UseDarkTheme)
            {
                base.OnPaint(e);
                return;
            }

            var rectangle = new Rectangle(0, 0, Width, Height);

            using (var font = new Font(Font, FontStyle.Italic))
            {
                if (UseCompatibleTextRendering)
                {
                    using (var brush = new SolidBrush(DarkTheme.DarkThemeDisabledColor))
                    using (var stringFormat = CreateStringFormat(this, TextAlign, AutoEllipsis))
                    {
                        e.Graphics.DrawString(Text, font, brush, rectangle, stringFormat);
                    }
                }
                else
                {
                    var textFormatFlags = CreateTextFormatFlags(this, TextAlign, AutoEllipsis, UseMnemonic);
                    TextRenderer.DrawText(e.Graphics, Text, font, rectangle, DarkTheme.DarkThemeDisabledColor, textFormatFlags);
                }
            }
        }
    }
}
